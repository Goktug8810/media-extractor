using System.Net;
using MediaExtractor.Domain.Device;
using MediaExtractor.Application.Http;
using MediaExtractor.Application.Device;
using MediaExtractor.Application.UseCases;
using MediaExtractor.Domain.Media;
using MediaExtractor.Domain.YouTube;
using MediaExtractor.Application.YouTube;
using MediaExtractor.Infrastructure.YouTube;
using MediaExtractor.Infrastructure.YouTube.MediaExtractor.Infrastructure.YouTube;
using Microsoft.AspNetCore.Mvc;
using Jint;
using MediaExtractor.Infrastructure.Utils;


var builder = WebApplication.CreateBuilder(args);

// Memory cache (BaseJs plan cache için gerekli)
builder.Services.AddMemoryCache();

// N optimizer
builder.Services.AddSingleton<INParameterOptimizer, DynamicNParameterOptimizer>();
builder.Services.AddScoped<OptimizeNInUrlUseCase>();

// ----------------------------------------------------
//  HTTP + HTML Fetch + Device + UA Spoof
// ----------------------------------------------------
builder.Services.AddSingleton<DeviceDetector>();
builder.Services.AddSingleton<ISpoofUserAgentProvider, SpoofUserAgentProvider>();
builder.Services.AddSingleton<GetSpoofedUserAgent>();

builder.Services.AddScoped<IMediaHtmlFetcher, YouTubeHttpClient>();
builder.Services.AddScoped<YouTubeHttpClient>(); 

// ----------------------------------------------------
//  STREAM EXTRACTION
// ----------------------------------------------------
builder.Services.AddScoped<GetYoutubeHtmlUseCase>();
builder.Services.AddScoped<IPlayerResponseExtractor, PlayerResponseExtractor>();
builder.Services.AddScoped<ExtractPlayerResponseUseCase>();

builder.Services.AddScoped<IStreamSelector, StreamSelector>();
builder.Services.AddScoped<SelectStreamByItagUseCase>();

builder.Services.AddScoped<ISignatureCipherParser, SignatureCipherParser>();

// ----------------------------------------------------
//  BASE.JS + SIGNATURE DECODE PIPELINE
// ----------------------------------------------------
builder.Services.AddScoped<IPlayerScriptUrlExtractor, PlayerScriptUrlExtractor>();
builder.Services.AddScoped<FetchPlayerScriptUseCase>();

builder.Services.AddScoped<ISignaturePlanExtractor, JsSignaturePlanExtractor>();
builder.Services.AddScoped<ISignatureDecoder, SignatureDecoder>();

// Base.js content cache (Singleton)
builder.Services.AddSingleton<IBaseJsCache, BaseJsCache>();

// Signature decode plan cache (Singleton)
builder.Services.AddSingleton<ISignaturePlanCache, InMemorySignaturePlanCache>();

builder.Services.AddScoped<DecodeSignatureUseCase>();

// ----------------------------------------------------
//  DOWNLOAD PIPELINE
// ----------------------------------------------------
// HttpClient + Typed Client
builder.Services.AddHttpClient<IMediaDownloader, YouTubeMediaDownloader>(client =>
{
    client.DefaultRequestVersion = HttpVersion.Version11;
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
});
builder.Services.AddScoped<DownloadMediaUseCase>();

var app = builder.Build();

app.MapGet("/html", async (
    [FromQuery] string url,
    HttpContext ctx,
    [FromServices] YouTubeHttpClient yt) =>
{
    var ua = ctx.Request.Headers["User-Agent"].ToString();
    return await yt.GetHtmlAsync(url, ua);
});

app.MapGet("/youtube/test", async (
    [FromQuery] string url,
    HttpRequest request,
    [FromServices] GetYoutubeHtmlUseCase useCase) =>
{
    var ua = request.Headers.UserAgent.ToString();

    var (html, protocol) = await useCase.ExecuteWithProtocolAsync(url, ua);

    return Results.Json(new {
        protocol = protocol.ToString(),
        isHttp2 = protocol.Major == 2,
        isHttp3 = protocol.Major == 3,
        htmlLength = html.Length
    });
});

app.MapGet("/youtube/streams", async (
    [FromQuery] string url,
    HttpContext ctx,
    [FromServices] YouTubeHttpClient yt,
    [FromServices] ExtractPlayerResponseUseCase extractor
) =>
{
    var ua = ctx.Request.Headers["User-Agent"].ToString();

    var html = await yt.GetHtmlAsync(url, ua);

    var result = extractor.Execute(html);

    return Results.Json(new {
        title = result.Title,
        videoId = result.VideoId,
        streamCount = result.Streams.Count,
        streams = result.Streams.Select(s => new {
            s.Itag,
            s.Format,
            s.Type,
            s.Quality,
            hasCipher = s.Url == "[signatureCipher]",
            url = s.Url
        })
    });
});

app.MapGet("/youtube/stream", async (
    [FromQuery] string url,
    [FromQuery] int itag,
    HttpContext ctx,
    [FromServices] YouTubeHttpClient yt,
    [FromServices] ExtractPlayerResponseUseCase extractor,
    [FromServices] SelectStreamByItagUseCase selectorUseCase,
    [FromServices] FetchPlayerScriptUseCase fetchScriptUseCase,
    [FromServices] DecodeSignatureUseCase decodeSignatureUseCase
) =>
{
    var ua = ctx.Request.Headers["User-Agent"].ToString();

    var html = await yt.GetHtmlAsync(url, ua);
    var response = extractor.Execute(html);

    var selected = selectorUseCase.Execute(response, itag);

    string finalUrl;

    if (selected.Cipher is not null)
    {
        var scriptResult = await fetchScriptUseCase.ExecuteAsync(html, ua);
        finalUrl = decodeSignatureUseCase.Execute(selected.Cipher, scriptResult.BaseJs, scriptResult.PlayerId);
    }
    else
    {
        finalUrl = selected.Url!;
    }

    return Results.Json(new {
        selected.Itag,
        selected.Format,
        selected.Type,
        selected.Quality,
        url = finalUrl,
        hasCipher = selected.Cipher is not null
    });
});

app.MapGet("/youtube/download", async (
    [FromQuery] string url,
    [FromQuery] int itag,
    HttpContext ctx,
    [FromServices] YouTubeHttpClient yt,
    [FromServices] ExtractPlayerResponseUseCase extractor,
    [FromServices] SelectStreamByItagUseCase selectorUseCase,
    [FromServices] FetchPlayerScriptUseCase fetchScriptUseCase,
    [FromServices] DecodeSignatureUseCase decodeSignatureUseCase,
    [FromServices] DownloadMediaUseCase downloadUseCase
) =>
{
    var incomingUa = ctx.Request.Headers.UserAgent.ToString();

    // 1) HTML al
    var html = await yt.GetHtmlAsync(url, incomingUa);
    var response = extractor.Execute(html);

    // 2) Stream seç
    var selected = selectorUseCase.Execute(response, itag);

    // 3) base.js gerekli mi?
    var needsBaseJs = selected.Cipher is not null
                      || (selected.Url?.Contains("n=", StringComparison.Ordinal) ?? false);

    PlayerScriptResult? baseJsResult = null;

    if (needsBaseJs)
    {
        baseJsResult = await fetchScriptUseCase.ExecuteAsync(html, incomingUa);
    }

    // 4) Sig decode (cipher varsa)
    string finalUrl;
    if (selected.Cipher is not null && baseJsResult is not null)
    {
        finalUrl = decodeSignatureUseCase.Execute(
            selected.Cipher,
            baseJsResult.BaseJs,     // DOĞRU
            baseJsResult.PlayerId
        );
    }
    else
    {
        finalUrl = selected.Url!;
    }

    // 5) Response header'ları ayarla
    ctx.Response.StatusCode = 200;
    ctx.Response.Headers.ContentType = selected.Format;
    ctx.Response.Headers.ContentDisposition =
        $"attachment; filename=\"video_{response.VideoId}_{itag}.bin\"";

    // 6) Download (n optimize burada baseJs alıyor)
    await downloadUseCase.ExecuteAsync(
        finalUrl,
        baseJsResult?.BaseJs,       // DOĞRU
        incomingUa,
        ctx.Response.Body,
        ctx.RequestAborted
    );

    return Results.Empty;
});

// Local helper’lar
static string GetExtensionFromFormat(string format)
{
    format = format.ToLowerInvariant();

    if (format.Contains("mp4"))  return "mp4";
    if (format.Contains("webm")) return "webm";
    if (format.Contains("3gpp")) return "3gp";
    if (format.Contains("mpeg")) return "mpg";

    return "bin";
}

static string SanitizeFileName(string name)
{
    var invalid = Path.GetInvalidFileNameChars();
    return string.Concat(name.Select(ch => invalid.Contains(ch) ? '_' : ch));
}



app.Run();
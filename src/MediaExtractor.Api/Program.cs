using MediaExtractor.Domain.Device;
using MediaExtractor.Application.Http;
using MediaExtractor.Application.Device;
using MediaExtractor.Application.UseCases;
using MediaExtractor.Domain.Media;
using MediaExtractor.Infrastructure.YouTube;
using Microsoft.AspNetCore.Mvc;
using MediaExtractor.Domain.YouTube; 


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DeviceDetector>();
builder.Services.AddSingleton<ISpoofUserAgentProvider, SpoofUserAgentProvider>();
builder.Services.AddSingleton<GetSpoofedUserAgent>();
builder.Services.AddScoped<IMediaHtmlFetcher, YouTubeHttpClient>();
builder.Services.AddScoped<YouTubeHttpClient>();  

builder.Services.AddScoped<GetYoutubeHtmlUseCase>();
builder.Services.AddScoped<IPlayerResponseExtractor, PlayerResponseExtractor>();
builder.Services.AddScoped<ExtractPlayerResponseUseCase>();
builder.Services.AddScoped<IStreamSelector, StreamSelector>();
builder.Services.AddScoped<SelectStreamByItagUseCase>();

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
    [FromServices] SelectStreamByItagUseCase selectorUseCase
) =>
{
    var ua = ctx.Request.Headers["User-Agent"].ToString();

    var html = await yt.GetHtmlAsync(url, ua);
    var response = extractor.Execute(html);

    var selected = selectorUseCase.Execute(response, itag);

    return Results.Json(new {
        selected.Itag,
        selected.Format,
        selected.Type,
        selected.Quality,
        selected.Url
    });
});



app.Run();
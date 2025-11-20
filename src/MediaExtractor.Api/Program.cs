using MediaExtractor.Domain.Device;
using MediaExtractor.Application.Http;
using MediaExtractor.Application.Device;
using MediaExtractor.Application.UseCases;
using MediaExtractor.Domain.Media;
using MediaExtractor.Infrastructure.YouTube;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DeviceDetector>();
builder.Services.AddSingleton<ISpoofUserAgentProvider, SpoofUserAgentProvider>();
builder.Services.AddSingleton<GetSpoofedUserAgent>();
builder.Services.AddScoped<IMediaHtmlFetcher, YouTubeHttpClient>();
builder.Services.AddScoped<GetYoutubeHtmlUseCase>();


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

app.Run();
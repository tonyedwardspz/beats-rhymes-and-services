using Microsoft.AspNetCore.Antiforgery;
using Xabe.FFmpeg;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery();

FFmpeg.SetExecutablesPath("/opt/homebrew/bin/");

builder.Services.AddSingleton<ITranscodeService, FfMpegTranscodeService>();
builder.Services.AddSingleton<AudioFileHelper>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddSingleton<IWhisperService, WhisperService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapWhisperEndpoints();

app.Run();
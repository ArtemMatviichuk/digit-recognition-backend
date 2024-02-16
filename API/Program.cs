using API.Common.DTO;
using API.Services.Implementations;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<IDigitRecognitionService, DigitRecognitionService>();
builder.Services.AddTransient<IImageProcessor, ImageProcessor>();
builder.Services.AddSingleton<INeuralNetworkHolder, NeuralNetworkHolder>();

var app = builder.Build();

app.MapControllers();
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.Run();

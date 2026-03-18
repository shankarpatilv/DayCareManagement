using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure.System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IClock, SystemClock>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

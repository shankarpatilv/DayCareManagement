using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.Infrastructure.System;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IClock, SystemClock>();
var connectionString = ConnectionStringResolver.ResolveOrThrow(
    builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddDbContext<DayCareManagementDbContext>(options =>
	options.UseNpgsql(connectionString));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

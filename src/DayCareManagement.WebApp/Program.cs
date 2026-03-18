var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

using DayCareManagement.WebApi.Configuration;
using DayCareManagement.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiServices(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapWebApiEndpoints();

app.Run();

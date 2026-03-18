using DayCareManagement.WebApp.Components;
using DayCareManagement.WebApp.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<AuthenticatedApiHttpMessageHandler>();

builder.Services.AddHttpClient<DayCareApiClient>((serviceProvider, client) =>
{
	var apiOptions = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
	if (string.IsNullOrWhiteSpace(apiOptions.BaseUrl))
	{
		throw new InvalidOperationException("Api:BaseUrl configuration is required.");
	}

	client.BaseAddress = new Uri(apiOptions.BaseUrl, UriKind.Absolute);
})
	.AddHttpMessageHandler<AuthenticatedApiHttpMessageHandler>();

var app = builder.Build();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

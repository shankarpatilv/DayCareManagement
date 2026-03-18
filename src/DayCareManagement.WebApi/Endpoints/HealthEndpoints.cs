namespace DayCareManagement.WebApi.Endpoints;

public static class HealthEndpoints
{
	public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
		return endpoints;
	}
}
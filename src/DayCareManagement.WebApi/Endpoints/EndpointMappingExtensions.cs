namespace DayCareManagement.WebApi.Endpoints;

public static class EndpointMappingExtensions
{
	public static IEndpointRouteBuilder MapWebApiEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapHealthEndpoints();
		endpoints.MapAuthEndpoints();

		var featureGroup = endpoints.MapGroup(string.Empty)
			.RequireAuthorization();

		featureGroup.MapStudentAndImmunizationEndpoints();
		featureGroup.MapTeacherEndpoints();
		featureGroup.MapRenewalEndpoints();
		featureGroup.MapStateRuleEndpoints();

		return endpoints;
	}
}
using System.Net.Http.Headers;

namespace DayCareManagement.WebApp.Services;

public sealed class AuthenticatedApiHttpMessageHandler(AuthSession authSession) : DelegatingHandler
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(authSession.Token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token);
		}

		return base.SendAsync(request, cancellationToken);
	}
}
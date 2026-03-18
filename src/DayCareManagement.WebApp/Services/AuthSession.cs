namespace DayCareManagement.WebApp.Services;

public sealed class AuthSession
{
	public string? Token { get; private set; }
	public string? Role { get; private set; }
	public int? SubjectId { get; private set; }

	public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

	public void Set(string token, string role, int subjectId)
	{
		Token = token;
		Role = role;
		SubjectId = subjectId;
	}

	public void Clear()
	{
		Token = null;
		Role = null;
		SubjectId = null;
	}
}
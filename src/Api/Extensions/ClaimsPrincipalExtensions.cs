using System.Security.Claims;

namespace Api
{
	public static class ClaimsPrincipalExtensions
    {
		public static int GetProjectId(this ClaimsPrincipal user) => int.Parse(user.FindFirstValue("ProjectId"));
	}
}

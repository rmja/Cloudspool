using System.Security.Claims;

namespace Api
{
	public static class ClaimsPrincipalExtensions
    {
		public static int GetProjectId(this ClaimsPrincipal user) => int.Parse(user.FindFirstValue("ProjectId"));
		public static bool TryGetZoneId(this ClaimsPrincipal user, out int zoneId) => int.TryParse(user.FindFirstValue("ZoneId"), out zoneId);
	}
}

using System.Security.Claims;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Common;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public int? TeamId => ParseInt(Principal?.FindFirstValue("teamId"));

    public int? StaffId => ParseInt(Principal?.FindFirstValue("staffId"));

    public int? JudgeId => ParseInt(Principal?.FindFirstValue("judgeId"));

    public string? Username => Principal?.FindFirstValue(ClaimTypes.Name);

    public bool IsStaff => Role is not null && AppRoles.IsStaff(Role);

    public bool IsInRole(string role) =>
        string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}

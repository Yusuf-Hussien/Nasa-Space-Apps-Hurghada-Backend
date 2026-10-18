using System.Security.Claims;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Common;

/// <summary>Role checks shared by controllers and SignalR hubs, so the staff
/// definition lives in exactly one place instead of string literals everywhere.</summary>
public static class ClaimsPrincipalExtensions
{
    public static bool IsStaff(this ClaimsPrincipal? principal) =>
        principal?.Identity?.IsAuthenticated == true &&
        (principal.IsInRole(AppRoles.Admin) ||
         principal.IsInRole(AppRoles.CoreTeam) ||
         principal.IsInRole(AppRoles.Judge));
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/core-team")]
[AllowAnonymous]
public class CoreTeamController : ControllerBase
{
    private readonly StaffService _staffService;

    public CoreTeamController(StaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// Public list of active CoreTeam members for the homepage section.
    /// Returns only <c>staffId</c>, <c>fullName</c>, <c>memberType</c>, and <c>photo</c>.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CoreTeamMemberResponse>>> GetCoreTeam()
    {
        var result = await _staffService.GetPublicCoreTeamAsync();
        return result.ToHttpResult(this, members => members.Select(CoreTeamMemberResponse.From).ToList());
    }
}

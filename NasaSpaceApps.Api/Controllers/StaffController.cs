using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class StaffController : ControllerBase
{
    private readonly StaffService _staffService;

    public StaffController(StaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>List staff. Filter by role and/or memberType.</summary>
    [HttpGet]
    public async Task<ActionResult<List<StaffResponse>>> GetStaffs(
        [FromQuery] string? role = null,
        [FromQuery] string? memberType = null,
        [FromQuery] string? search = null)
    {
        var result = await _staffService.GetStaffsAsync(role, memberType, search);
        return result.ToHttpResult(this, staff => staff.Select(StaffResponse.From).ToList());
    }

    /// <summary>Get a single staff member by id.</summary>
    [HttpGet("{staffId:int}")]
    public async Task<ActionResult<StaffResponse>> GetStaff(int staffId)
    {
        var result = await _staffService.GetStaffByIdAsync(staffId);
        return result.ToHttpResult(this, StaffResponse.From);
    }

    /// <summary>
    /// Create a staff member (CoreTeam or Judge or Admin).
    /// CoreTeam requires memberType: Organizer | Mentor | Developer | Marketing | PR | Media | Highboard.
    /// Judges auto-get a linked Judge profile for score attribution.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StaffResponse>> AddStaff([FromBody] StaffInput input)
    {
        var result = await _staffService.AddStaffAsync(input);
        return result.ToHttpResult(this, StaffResponse.From);
    }

    /// <summary>Update a staff member. Password is optional (only set to change it).</summary>
    [HttpPut("{staffId:int}")]
    public async Task<ActionResult<StaffResponse>> UpdateStaff(int staffId, [FromBody] StaffUpdateInput input)
    {
        var result = await _staffService.UpdateStaffAsync(staffId, input);
        return result.ToHttpResult(this, StaffResponse.From);
    }

    /// <summary>Deactivate (soft-delete) a staff member. They can no longer log in.</summary>
    [HttpPatch("{staffId:int}/deactivate")]
    public async Task<ActionResult<StaffResponse>> DeactivateStaff(int staffId)
    {
        var result = await _staffService.SetStatusAsync(staffId, "Inactive");
        return result.ToHttpResult(this, StaffResponse.From);
    }

    /// <summary>Reactivate a staff member.</summary>
    [HttpPatch("{staffId:int}/activate")]
    public async Task<ActionResult<StaffResponse>> ActivateStaff(int staffId)
    {
        var result = await _staffService.SetStatusAsync(staffId, "Active");
        return result.ToHttpResult(this, StaffResponse.From);
    }

    /// <summary>Permanently delete a staff member.</summary>
    [HttpDelete("{staffId:int}")]
    public async Task<ActionResult<DeletedResponse>> DeleteStaff(int staffId)
    {
        var result = await _staffService.DeleteStaffAsync(staffId);
        return result.ToHttpResult(this, _ => DeletedResponse.Of(staffId));
    }

    /// <summary>List available core-team member types (for the frontend dropdown).</summary>
    [HttpGet("member-types")]
    public ActionResult<IReadOnlyList<string>> GetMemberTypes()
    {
        return Ok(CoreTeamTypes.All);
    }
}

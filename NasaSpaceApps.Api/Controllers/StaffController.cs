using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
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
    public async Task<ActionResult> GetStaffs(
        [FromQuery] string? role = null,
        [FromQuery] string? memberType = null,
        [FromQuery] string? search = null)
    {
        var result = await _staffService.GetStaffsAsync(role, memberType, search);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(ToDto));
    }

    /// <summary>Get a single staff member by id.</summary>
    [HttpGet("{staffId:int}")]
    public async Task<ActionResult> GetStaff(int staffId)
    {
        var result = await _staffService.GetStaffByIdAsync(staffId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!));
    }

    /// <summary>
    /// Create a staff member (CoreTeam or Judge or Admin).
    /// CoreTeam requires memberType: Organizer | Mentor | Developer | Marketing | PR | Media | Highboard.
    /// Judges auto-get a linked Judge profile for score attribution.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> AddStaff([FromBody] StaffInputDto input)
    {
        var result = await _staffService.AddStaffAsync(new StaffInput(
            input.FullName, input.Username, input.Email, input.Password, input.Role, input.MemberType));

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!));
    }

    /// <summary>Update a staff member. Password is optional (only set to change it).</summary>
    [HttpPut("{staffId:int}")]
    public async Task<ActionResult> UpdateStaff(int staffId, [FromBody] StaffUpdateDto input)
    {
        var result = await _staffService.UpdateStaffAsync(staffId, new StaffUpdateInput(
            input.FullName, input.Username, input.Email, input.Password, input.Role, input.MemberType, input.Status));

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!));
    }

    /// <summary>Deactivate (soft-delete) a staff member. They can no longer log in.</summary>
    [HttpPatch("{staffId:int}/deactivate")]
    public async Task<ActionResult> DeactivateStaff(int staffId)
    {
        var result = await _staffService.DeactivateStaffAsync(staffId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { staffId, status = "Inactive" });
    }

    /// <summary>Reactivate a staff member.</summary>
    [HttpPatch("{staffId:int}/activate")]
    public async Task<ActionResult> ActivateStaff(int staffId)
    {
        var update = await _staffService.GetStaffByIdAsync(staffId);
        if (!update.IsSuccess)
        {
            return StatusCode(update.Error!.HttpStatus, new { code = update.Error.Code, message = update.Error.Message });
        }

        var s = update.Value!;
        var result = await _staffService.UpdateStaffAsync(staffId, new StaffUpdateInput(
            s.FullName, s.Username, s.Email, null, s.Role, s.MemberType, "Active"));

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!));
    }

    /// <summary>Permanently delete a staff member.</summary>
    [HttpDelete("{staffId:int}")]
    public async Task<ActionResult> DeleteStaff(int staffId)
    {
        var result = await _staffService.DeleteStaffAsync(staffId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { staffId, deleted = true });
    }

    /// <summary>List available core-team member types (for the frontend dropdown).</summary>
    [HttpGet("member-types")]
    public ActionResult GetMemberTypes()
    {
        return Ok(CoreTeamTypes.All);
    }

    private static object ToDto(Staff s) => new
    {
        staffId = s.StaffId,
        fullName = s.FullName,
        username = s.Username,
        email = s.Email,
        role = s.Role,
        memberType = s.MemberType,
        status = s.Status,
        judgeId = s.JudgeId,
        createdAt = s.CreatedAt
    };
}

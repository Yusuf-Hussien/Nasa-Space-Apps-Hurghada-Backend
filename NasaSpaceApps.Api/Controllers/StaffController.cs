using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Services;

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

    [HttpGet]
    public async Task<ActionResult> GetStaffs([FromQuery] string? role = null, [FromQuery] string? search = null)
    {
        var result = await _staffService.GetStaffsAsync(role, search);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(s => new
        {
            staffId = s.StaffId,
            fullName = s.FullName,
            username = s.Username,
            email = s.Email,
            role = s.Role,
            status = s.Status,
            judgeId = s.JudgeId,
            createdAt = s.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult> AddStaff([FromBody] StaffInputDto input)
    {
        var result = await _staffService.AddStaffAsync(new StaffInput(
            input.FullName, input.Username, input.Email, input.Password, input.Role));

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var s = result.Value!;
        return Ok(new
        {
            staffId = s.StaffId,
            fullName = s.FullName,
            username = s.Username,
            email = s.Email,
            role = s.Role,
            status = s.Status,
            judgeId = s.JudgeId,
            createdAt = s.CreatedAt
        });
    }
}

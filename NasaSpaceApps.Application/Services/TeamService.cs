using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class TeamService
{
    private readonly INasaSpaceAppsRepository _repository;

    public TeamService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Team>>> SearchTeamsAsync(string? search)
    {
        var teams = await _repository.GetTeamsAsync(search);
        return Result<List<Team>>.Success(teams);
    }

    public async Task<Result<Team>> GetTeamAsync(int teamId)
    {
        var team = await _repository.GetTeamByIdAsync(teamId);
        if (team is null)
        {
            return Result<Team>.Failure("team.not_found", 404, "Team was not found.");
        }

        return Result<Team>.Success(team);
    }

    public async Task<Result<List<Student>>> GetStudentsAsync(int teamId)
    {
        var students = await _repository.GetStudentsByTeamIdAsync(teamId);
        return Result<List<Student>>.Success(students);
    }

    /// <summary>Set or clear a team's logo (base64 image or URL, max 20 MB decoded).</summary>
    public async Task<Result<Team>> SetLogoAsync(int teamId, string? logo)
    {
        var team = await _repository.GetTeamByIdAsync(teamId);
        if (team is null)
        {
            return Result<Team>.Failure("team.not_found", 404, "Team was not found.");
        }

        var logoError = ImageValidator.Validate(logo, "logo");
        if (logoError is not null)
        {
            return Result<Team>.Failure(logoError.Code, 400, logoError.Message);
        }

        team.Logo = string.IsNullOrWhiteSpace(logo) ? null : logo.Trim();
        await _repository.UpdateTeamAsync(team);
        return Result<Team>.Success(team);
    }
}

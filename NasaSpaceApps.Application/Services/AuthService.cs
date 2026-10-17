using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class AuthService
{
    private readonly INasaSpaceAppsRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(INasaSpaceAppsRepository repository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Unified login. Staff provide { identifier } (email or username) + password.
    /// Participants provide { teamId } + password.
    /// </summary>
    public async Task<Result<LoginResponse>> LoginUnifiedAsync(string? identifier, int? teamId, string password)
    {
        var hasIdentifier = !string.IsNullOrWhiteSpace(identifier);
        var hasTeamId = teamId.HasValue && teamId.Value > 0;

        if (hasTeamId && hasIdentifier)
        {
            return Result<LoginResponse>.Failure(
                "auth.ambiguous", 400,
                "Provide either teamId (participant) or identifier (staff), not both.");
        }

        if (!hasTeamId && !hasIdentifier)
        {
            return Result<LoginResponse>.Failure(
                "auth.missing_identifier", 400,
                "Provide teamId (participant) or identifier (staff) to sign in.");
        }

        return hasTeamId
            ? await LoginParticipantAsync(teamId!.Value, password)
            : await LoginStaffAsync(identifier!.Trim(), password);
    }

    private async Task<Result<LoginResponse>> LoginParticipantAsync(int teamId, string password)
    {
        var team = await _repository.GetTeamByIdAsync(teamId);

        if (team is null || !team.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            return Result<LoginResponse>.Failure("auth.invalid_credentials", 401, "Invalid teamId or password.");
        }

        if (!_passwordHasher.Verify(password, team.PasswordHash))
        {
            return Result<LoginResponse>.Failure("auth.invalid_credentials", 401, "Invalid teamId or password.");
        }

        var token = _tokenService.CreateParticipantToken(team);
        var response = new LoginResponse(
            AccessToken: token,
            Role: AppRoles.Participant,
            Name: team.Name,
            TeamId: team.TeamId,
            Username: team.Username);

        return Result<LoginResponse>.Success(response);
    }

    private async Task<Result<LoginResponse>> LoginStaffAsync(string identifier, string password)
    {
        var staff = await _repository.GetStaffByIdentifierAsync(identifier);

        if (staff is null || !staff.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            return Result<LoginResponse>.Failure("auth.invalid_credentials", 401, "Invalid email/username or password.");
        }

        if (!_passwordHasher.Verify(password, staff.PasswordHash))
        {
            return Result<LoginResponse>.Failure("auth.invalid_credentials", 401, "Invalid email/username or password.");
        }

        var token = _tokenService.CreateStaffToken(staff);
        var response = new LoginResponse(
            AccessToken: token,
            Role: staff.Role,
            Name: staff.FullName,
            Username: staff.Username,
            Email: staff.Email,
            StaffId: staff.StaffId,
            JudgeId: staff.JudgeId);

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<LoginResponse>> GetCurrentUserAsync(ICurrentUser user)
    {
        if (user.TeamId.HasValue)
        {
            var team = await _repository.GetTeamByIdAsync(user.TeamId.Value);
            if (team is null)
            {
                return Result<LoginResponse>.Failure("user.not_found", 404, "Team not found.");
            }

            return Result<LoginResponse>.Success(new LoginResponse(
                AccessToken: string.Empty,
                Role: AppRoles.Participant,
                Name: team.Name,
                TeamId: team.TeamId,
                Username: team.Username));
        }

        if (user.StaffId.HasValue)
        {
            var staff = await _repository.GetStaffByIdAsync(user.StaffId.Value);
            if (staff is null)
            {
                return Result<LoginResponse>.Failure("user.not_found", 404, "Staff account not found.");
            }

            return Result<LoginResponse>.Success(new LoginResponse(
                AccessToken: string.Empty,
                Role: staff.Role,
                Name: staff.FullName,
                Username: staff.Username,
                Email: staff.Email,
                StaffId: staff.StaffId,
                JudgeId: staff.JudgeId));
        }

        return Result<LoginResponse>.Failure("user.unauthenticated", 401, "Not authenticated.");
    }
}

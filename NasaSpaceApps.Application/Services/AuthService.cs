using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;

namespace NasaSpaceApps.Application.Services;

public sealed record LoginResponse(int TeamId, string Name, string Username, string AccessToken);

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

    public async Task<Result<LoginResponse>> LoginAsync(string username, string password)
    {
        var normalized = username.Trim();
        var team = await _repository.GetTeamByUsernameAsync(normalized);

        if (team is null || !_passwordHasher.Verify(password, team.PasswordHash))
        {
            return Result<LoginResponse>.Failure("auth.invalid_credentials", 401, "Invalid username or password.");
        }

        var token = _tokenService.CreateAccessToken(team);
        var response = new LoginResponse(team.TeamId, team.Name, team.Username, token);

        return Result<LoginResponse>.Success(response);
    }
}

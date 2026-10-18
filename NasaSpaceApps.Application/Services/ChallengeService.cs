using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class ChallengeService
{
    private readonly INasaSpaceAppsRepository _repository;

    public ChallengeService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Challenge>>> GetChallengesAsync()
    {
        var challenges = await _repository.GetChallengesAsync();
        return Result<List<Challenge>>.Success(challenges);
    }

    public async Task<Result<Challenge>> GetChallengeAsync(int challengeId)
    {
        var challenge = await _repository.GetChallengeByIdAsync(challengeId);
        if (challenge is null)
        {
            return Result<Challenge>.Failure("challenge.not_found", 404, "Challenge was not found.");
        }

        return Result<Challenge>.Success(challenge);
    }
}

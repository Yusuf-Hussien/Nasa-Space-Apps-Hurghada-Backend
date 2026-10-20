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

    /// <summary>Set or clear a challenge's image (base64 image or URL, max 20 MB decoded).</summary>
    public async Task<Result<Challenge>> SetImageAsync(int challengeId, string? image)
    {
        var challenge = await _repository.GetChallengeByIdAsync(challengeId);
        if (challenge is null)
        {
            return Result<Challenge>.Failure("challenge.not_found", 404, "Challenge was not found.");
        }

        var imageError = ImageValidator.Validate(image, "image");
        if (imageError is not null)
        {
            return Result<Challenge>.Failure(imageError.Code, 400, imageError.Message);
        }

        challenge.Image = string.IsNullOrWhiteSpace(image) ? null : image.Trim();
        await _repository.UpdateChallengeAsync(challenge);
        return Result<Challenge>.Success(challenge);
    }
}

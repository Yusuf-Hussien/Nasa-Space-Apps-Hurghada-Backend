using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class SeasonMediaService
{
    private readonly INasaSpaceAppsRepository _repository;

    public SeasonMediaService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<SeasonMedia>>> GetMediaAsync(bool includeInactive = false)
    {
        var media = await _repository.GetSeasonMediaAsync(!includeInactive);
        return Result<List<SeasonMedia>>.Success(media);
    }

    public async Task<Result<SeasonMedia>> GetMediaByIdAsync(int mediaId)
    {
        var media = await _repository.GetSeasonMediaByIdAsync(mediaId);
        if (media is null)
        {
            return Result<SeasonMedia>.Failure("season_media.not_found", 404, "Season media not found.");
        }

        return Result<SeasonMedia>.Success(media);
    }

    public async Task<Result<SeasonMedia>> AddAsync(SeasonMediaInput input)
    {
        var validation = Validate(input);
        if (validation is not null)
        {
            return validation;
        }

        var media = new SeasonMedia
        {
            Season = input.Season.Trim(),
            Title = input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Image = string.IsNullOrWhiteSpace(input.Image) ? null : input.Image.Trim(),
            Link = string.IsNullOrWhiteSpace(input.Link) ? null : input.Link.Trim(),
            MediaType = string.IsNullOrWhiteSpace(input.MediaType) ? null : input.MediaType.Trim(),
            DisplayOrder = input.DisplayOrder,
            Active = input.Active
        };

        var created = await _repository.AddSeasonMediaAsync(media);
        return Result<SeasonMedia>.Success(created);
    }

    public async Task<Result<SeasonMedia>> UpdateAsync(int mediaId, SeasonMediaInput input)
    {
        var media = await _repository.GetSeasonMediaByIdAsync(mediaId);
        if (media is null)
        {
            return Result<SeasonMedia>.Failure("season_media.not_found", 404, "Season media not found.");
        }

        var validation = Validate(input);
        if (validation is not null)
        {
            return validation;
        }

        media.Season = input.Season.Trim();
        media.Title = input.Title.Trim();
        media.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        media.Image = string.IsNullOrWhiteSpace(input.Image) ? null : input.Image.Trim();
        media.Link = string.IsNullOrWhiteSpace(input.Link) ? null : input.Link.Trim();
        media.MediaType = string.IsNullOrWhiteSpace(input.MediaType) ? null : input.MediaType.Trim();
        media.DisplayOrder = input.DisplayOrder;
        media.Active = input.Active;

        await _repository.UpdateSeasonMediaAsync(media);
        return Result<SeasonMedia>.Success(media);
    }

    public async Task<Result<bool>> DeleteAsync(int mediaId)
    {
        var media = await _repository.GetSeasonMediaByIdAsync(mediaId);
        if (media is null)
        {
            return Result<bool>.Failure("season_media.not_found", 404, "Season media not found.");
        }

        await _repository.DeleteSeasonMediaAsync(media);
        return Result<bool>.Success(true);
    }

    private static Result<SeasonMedia>? Validate(SeasonMediaInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Season))
        {
            return Result<SeasonMedia>.Failure("season_media.invalid", 400, "Season is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return Result<SeasonMedia>.Failure("season_media.invalid", 400, "Title is required.");
        }

        var imageError = ImageValidator.Validate(input.Image, "image");
        if (imageError is not null)
        {
            return Result<SeasonMedia>.Failure(imageError.Code, 400, imageError.Message);
        }

        return null;
    }
}

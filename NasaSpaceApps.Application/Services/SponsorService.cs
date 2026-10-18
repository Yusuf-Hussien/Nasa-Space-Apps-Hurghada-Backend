using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class SponsorService
{
    private readonly INasaSpaceAppsRepository _repository;

    public SponsorService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Sponsor>>> GetSponsorsAsync(bool includeInactive = false)
    {
        var sponsors = await _repository.GetSponsorsAsync(!includeInactive);
        return Result<List<Sponsor>>.Success(sponsors);
    }

    public async Task<Result<Sponsor>> GetSponsorByIdAsync(int sponsorId)
    {
        var sponsor = await _repository.GetSponsorByIdAsync(sponsorId);
        if (sponsor is null)
        {
            return Result<Sponsor>.Failure("sponsor.not_found", 404, "Sponsor not found.");
        }

        return Result<Sponsor>.Success(sponsor);
    }

    public async Task<Result<Sponsor>> AddSponsorAsync(SponsorInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<Sponsor>.Failure("sponsor.invalid", 400, "Name is required.");
        }

        var tier = await _repository.GetSponsorTierByIdAsync(input.TierId);
        if (tier is null)
        {
            return Result<Sponsor>.Failure("sponsor.tier_not_found", 400, "Sponsor tier not found.");
        }

        var sponsor = new Sponsor
        {
            Name = input.Name.Trim(),
            Logo = string.IsNullOrWhiteSpace(input.Logo) ? null : input.Logo.Trim(),
            Link = string.IsNullOrWhiteSpace(input.Link) ? null : input.Link.Trim(),
            TierId = input.TierId,
            Active = input.Active,
            DisplayOrder = input.DisplayOrder
        };

        var created = await _repository.AddSponsorAsync(sponsor);
        return Result<Sponsor>.Success(created);
    }

    public async Task<Result<Sponsor>> UpdateSponsorAsync(int sponsorId, SponsorUpdateInput input)
    {
        var sponsor = await _repository.GetSponsorByIdAsync(sponsorId);
        if (sponsor is null)
        {
            return Result<Sponsor>.Failure("sponsor.not_found", 404, "Sponsor not found.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<Sponsor>.Failure("sponsor.invalid", 400, "Name is required.");
        }

        var tier = await _repository.GetSponsorTierByIdAsync(input.TierId);
        if (tier is null)
        {
            return Result<Sponsor>.Failure("sponsor.tier_not_found", 400, "Sponsor tier not found.");
        }

        sponsor.Name = input.Name.Trim();
        sponsor.Logo = string.IsNullOrWhiteSpace(input.Logo) ? null : input.Logo.Trim();
        sponsor.Link = string.IsNullOrWhiteSpace(input.Link) ? null : input.Link.Trim();
        sponsor.TierId = input.TierId;
        sponsor.Active = input.Active;
        sponsor.DisplayOrder = input.DisplayOrder;

        await _repository.UpdateSponsorAsync(sponsor);
        return Result<Sponsor>.Success(sponsor);
    }

    public async Task<Result<bool>> DeleteSponsorAsync(int sponsorId)
    {
        var sponsor = await _repository.GetSponsorByIdAsync(sponsorId);
        if (sponsor is null)
        {
            return Result<bool>.Failure("sponsor.not_found", 404, "Sponsor not found.");
        }

        await _repository.DeleteSponsorAsync(sponsor);
        return Result<bool>.Success(true);
    }

    public async Task<Result<List<SponsorTier>>> GetSponsorTiersAsync()
    {
        var tiers = await _repository.GetSponsorTiersAsync();
        return Result<List<SponsorTier>>.Success(tiers);
    }

    public async Task<Result<SponsorTier>> GetSponsorTierByIdAsync(int tierId)
    {
        var tier = await _repository.GetSponsorTierByIdAsync(tierId);
        if (tier is null)
        {
            return Result<SponsorTier>.Failure("sponsor_tier.not_found", 404, "Sponsor tier not found.");
        }

        return Result<SponsorTier>.Success(tier);
    }

    public async Task<Result<SponsorTier>> AddSponsorTierAsync(SponsorTierInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<SponsorTier>.Failure("sponsor_tier.invalid", 400, "Name is required.");
        }

        var tier = new SponsorTier
        {
            Name = input.Name.Trim(),
            Priority = input.Priority,
            BannerEnabled = input.BannerEnabled
        };

        var created = await _repository.AddSponsorTierAsync(tier);
        return Result<SponsorTier>.Success(created);
    }

    public async Task<Result<SponsorTier>> UpdateSponsorTierAsync(int tierId, SponsorTierUpdateInput input)
    {
        var tier = await _repository.GetSponsorTierByIdAsync(tierId);
        if (tier is null)
        {
            return Result<SponsorTier>.Failure("sponsor_tier.not_found", 404, "Sponsor tier not found.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<SponsorTier>.Failure("sponsor_tier.invalid", 400, "Name is required.");
        }

        tier.Name = input.Name.Trim();
        tier.Priority = input.Priority;
        tier.BannerEnabled = input.BannerEnabled;

        await _repository.UpdateSponsorTierAsync(tier);
        return Result<SponsorTier>.Success(tier);
    }

    public async Task<Result<bool>> DeleteSponsorTierAsync(int tierId)
    {
        var tier = await _repository.GetSponsorTierByIdAsync(tierId);
        if (tier is null)
        {
            return Result<bool>.Failure("sponsor_tier.not_found", 404, "Sponsor tier not found.");
        }

        await _repository.DeleteSponsorTierAsync(tier);
        return Result<bool>.Success(true);
    }
}
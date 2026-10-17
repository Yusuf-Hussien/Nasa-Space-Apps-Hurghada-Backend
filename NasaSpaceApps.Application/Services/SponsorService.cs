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

    public async Task<Result<List<Sponsor>>> GetSponsorsAsync()
    {
        var sponsors = await _repository.GetSponsorsAsync(true);
        return Result<List<Sponsor>>.Success(sponsors);
    }
}

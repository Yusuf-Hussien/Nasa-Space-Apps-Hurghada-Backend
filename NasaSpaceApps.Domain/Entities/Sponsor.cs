namespace NasaSpaceApps.Domain.Entities;

public class Sponsor
{
    public int SponsorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Link { get; set; }
    public int TierId { get; set; }
    public SponsorTier? Tier { get; set; }
    public bool Active { get; set; }
    public int? DisplayOrder { get; set; }
}

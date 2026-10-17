namespace NasaSpaceApps.Domain.Entities;

public class SponsorTier
{
    public int TierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Priority { get; set; }
    public bool BannerEnabled { get; set; }
    public ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
}

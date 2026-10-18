namespace NasaSpaceApps.Api.Controllers;

/// <summary>Standard response body for a successful delete.</summary>
public sealed record DeletedResponse(int Id, bool Deleted)
{
    public static DeletedResponse Of(int id) => new(id, true);
}

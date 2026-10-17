using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ITokenService
{
    string CreateAccessToken(Team team);
}

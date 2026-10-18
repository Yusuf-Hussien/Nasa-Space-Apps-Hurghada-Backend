using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Interfaces;

public interface ITokenService
{
    string CreateParticipantToken(Team team, bool rememberMe = false);
    string CreateStaffToken(Staff staff, bool rememberMe = false);
}

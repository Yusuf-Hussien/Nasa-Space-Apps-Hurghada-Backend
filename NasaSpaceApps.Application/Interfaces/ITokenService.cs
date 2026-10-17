using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Interfaces;

public interface ITokenService
{
    string CreateParticipantToken(Team team);
    string CreateStaffToken(Staff staff);
}

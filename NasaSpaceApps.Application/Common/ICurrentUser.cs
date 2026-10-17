namespace NasaSpaceApps.Application.Common;

public interface ICurrentUser
{
    int? TeamId { get; }
    int? StaffId { get; }
    int? JudgeId { get; }
    string? Username { get; }
    string? Role { get; }

    bool IsAuthenticated { get; }
    bool IsStaff { get; }
    bool IsInRole(string role);
}

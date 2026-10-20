using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Models;

public sealed record LoginResponse(
    string AccessToken,
    string Role,
    string Name,
    int? TeamId = null,
    string? Username = null,
    string? Email = null,
    int? StaffId = null,
    int? JudgeId = null,
    Team? Team = null,
    IReadOnlyCollection<Student>? TeamStudents = null);

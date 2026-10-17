using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class AttendanceService
{
    private readonly INasaSpaceAppsRepository _repository;

    public AttendanceService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<TeamAttendanceRow>>> GetTeamsWithAttendanceAsync(string? search, DateTime? eventDate)
    {
        var teams = await _repository.GetTeamsAsync(search);
        var students = await _repository.GetAllStudentsWithTeamAsync();

        List<Attendance>? attendances = null;
        if (eventDate.HasValue)
        {
            attendances = await _repository.GetAttendancesAsync(null, eventDate.Value, null);
        }

        var attendanceLookup = attendances is null
            ? new Dictionary<int, Attendance>()
            : attendances.ToDictionary(a => a.StudentId, a => a);

        var rows = teams.Select(team => new TeamAttendanceRow(
            team.TeamId,
            team.Name,
            team.Username,
            team.Status,
            students
                .Where(s => s.TeamId == team.TeamId)
                .OrderBy(s => s.Name)
                .Select(s =>
                {
                    attendanceLookup.TryGetValue(s.StudentId, out var att);
                    return new TeamAttendanceStudent(
                        s.StudentId,
                        s.Name,
                        s.Email,
                        s.Contact,
                        att?.Status,
                        att?.MarkedBy);
                })
                .ToList()
        )).ToList();

        return Result<List<TeamAttendanceRow>>.Success(rows);
    }

    public async Task<Result<BulkAttendanceResult>> MarkBulkAsync(DateTime eventDate, List<AttendanceEntry> entries, string markedByUsername)
    {
        if (entries is null || entries.Count == 0)
        {
            return Result<BulkAttendanceResult>.Failure("attendance.empty", 400, "At least one attendance entry is required.");
        }

        foreach (var entry in entries)
        {
            if (!AttendanceStatuses.IsValid(entry.Status))
            {
                return Result<BulkAttendanceResult>.Failure(
                    "attendance.invalid_status", 400,
                    $"Invalid status '{entry.Status}'. Allowed: {string.Join(", ", AttendanceStatuses.All)}.");
            }
        }

        var studentIds = entries.Select(e => e.StudentId).Distinct().ToList();
        var students = await _repository.GetStudentsByIdsAsync(studentIds);
        var foundIds = students.Select(s => s.StudentId).ToHashSet();
        var missing = studentIds.Where(id => !foundIds.Contains(id)).ToList();

        if (missing.Count > 0)
        {
            return Result<BulkAttendanceResult>.Failure(
                "attendance.students_not_found", 404,
                $"Students not found: {string.Join(", ", missing)}.");
        }

        var date = eventDate.Date;
        int created = 0, updated = 0;

        foreach (var entry in entries)
        {
            var existing = await _repository.GetAttendanceForStudentDateAsync(entry.StudentId, date);

            if (existing is null)
            {
                await _repository.AddAttendanceAsync(new Attendance
                {
                    StudentId = entry.StudentId,
                    EventDate = date,
                    Status = NormalizeStatus(entry.Status),
                    MarkedBy = markedByUsername,
                    Timestamp = DateTime.UtcNow
                });
                created++;
            }
            else
            {
                existing.Status = NormalizeStatus(entry.Status);
                existing.MarkedBy = markedByUsername;
                existing.Timestamp = DateTime.UtcNow;
                await _repository.UpdateAttendanceAsync(existing);
                updated++;
            }
        }

        return Result<BulkAttendanceResult>.Success(new BulkAttendanceResult(updated, created));
    }

    public async Task<Result<List<Attendance>>> GetAttendancesAsync(int? teamId, DateTime? eventDate, string? status)
    {
        var attendances = await _repository.GetAttendancesAsync(teamId, eventDate, status);
        return Result<List<Attendance>>.Success(attendances);
    }

    private static string NormalizeStatus(string status)
    {
        var trimmed = status.Trim();
        foreach (var known in AttendanceStatuses.All)
        {
            if (string.Equals(trimmed, known, StringComparison.OrdinalIgnoreCase))
            {
                return known;
            }
        }

        return trimmed;
    }
}

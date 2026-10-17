using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class RequestService
{
    private readonly INasaSpaceAppsRepository _repository;

    public RequestService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ServiceRequest>>> GetRequestsAsync(int? teamId = null, string? type = null, string? status = null)
    {
        var requests = await _repository.GetRequestsAsync(teamId, type, status);
        return Result<List<ServiceRequest>>.Success(requests);
    }

    public async Task<Result<ServiceRequest>> CreateRequestAsync(int teamId, string type, string message)
    {
        var team = await _repository.GetTeamByIdAsync(teamId);
        if (team is null)
        {
            return Result<ServiceRequest>.Failure("team.not_found", 404, "The team does not exist.");
        }

        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(message))
        {
            return Result<ServiceRequest>.Failure("request.invalid", 400, "Type and message are required.");
        }

        var request = new ServiceRequest
        {
            TeamId = teamId,
            Type = type,
            Message = message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        };

        var created = await _repository.AddRequestAsync(request);
        return Result<ServiceRequest>.Success(created);
    }
}

public sealed class AttendanceService
{
    private readonly INasaSpaceAppsRepository _repository;

    public AttendanceService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Attendance>> MarkAttendanceAsync(int studentId, DateTime eventDate, string status, string? markedBy)
    {
        var student = await _repository.GetStudentByIdAsync(studentId);
        if (student is null)
        {
            return Result<Attendance>.Failure("student.not_found", 404, "Student was not found.");
        }

        var attendance = new Attendance
        {
            StudentId = studentId,
            EventDate = eventDate,
            Status = status,
            MarkedBy = markedBy,
            Timestamp = DateTime.UtcNow
        };

        var saved = await _repository.AddAttendanceAsync(attendance);
        return Result<Attendance>.Success(saved);
    }
}

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

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

    public async Task<Result<ServiceRequest>> ResolveRequestAsync(int requestId, string status)
    {
        var request = await _repository.GetRequestByIdAsync(requestId);
        if (request is null)
        {
            return Result<ServiceRequest>.Failure("request.not_found", 404, "Request not found.");
        }

        if (!string.Equals(status, "In Progress", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, "Resolved", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ServiceRequest>.Failure("request.invalid_status", 400, "Status must be In Progress, Resolved, or Rejected.");
        }

        request.Status = char.ToUpper(status[0]) + status[1..].ToLower();
        if (string.Equals(request.Status, "Resolved", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            request.ResolvedAt = DateTime.UtcNow;
        }

        await _repository.UpdateRequestAsync(request);
        return Result<ServiceRequest>.Success(request);
    }
}

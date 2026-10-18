using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a support request.</summary>
public sealed record ServiceRequestResponse(
    int RequestId,
    int TeamId,
    string Type,
    string Status,
    string Message,
    DateTime CreatedAt,
    DateTime? ResolvedAt)
{
    public static ServiceRequestResponse From(ServiceRequest request) =>
        new(
            RequestId: request.RequestId,
            TeamId: request.TeamId,
            Type: request.Type,
            Status: request.Status,
            Message: request.Message,
            CreatedAt: request.CreatedAt,
            ResolvedAt: request.ResolvedAt);
}

using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a student on a team.</summary>
public sealed record StudentResponse(
    int StudentId,
    string Name,
    string? Email,
    string? Contact,
    string? Status)
{
    public static StudentResponse From(Student student) =>
        new(student.StudentId, student.Name, student.Email, student.Contact, student.Status);
}

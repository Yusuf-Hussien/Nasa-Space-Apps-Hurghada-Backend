using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class StaffService
{
    private readonly INasaSpaceAppsRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public StaffService(INasaSpaceAppsRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<List<Staff>>> GetStaffsAsync(string? role, string? search)
    {
        var staffs = await _repository.GetStaffsAsync(role, search);
        return Result<List<Staff>>.Success(staffs);
    }

    public async Task<Result<Staff>> AddStaffAsync(StaffInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FullName) ||
            string.IsNullOrWhiteSpace(input.Username) ||
            string.IsNullOrWhiteSpace(input.Email) ||
            string.IsNullOrWhiteSpace(input.Password))
        {
            return Result<Staff>.Failure("staff.invalid", 400, "FullName, Username, Email and Password are required.");
        }

        if (!AppRoles.StaffRoles.Contains(input.Role, StringComparer.OrdinalIgnoreCase))
        {
            return Result<Staff>.Failure(
                "staff.invalid_role", 400,
                $"Role must be one of: {string.Join(", ", AppRoles.StaffRoles)}.");
        }

        if (input.Password.Length < 6)
        {
            return Result<Staff>.Failure("staff.weak_password", 400, "Password must be at least 6 characters.");
        }

        var normalizedRole = AppRoles.StaffRoles.First(r => r.Equals(input.Role, StringComparison.OrdinalIgnoreCase));
        var username = input.Username.Trim();
        var email = input.Email.Trim();

        var existingByUsername = await _repository.GetStaffByIdentifierAsync(username);
        if (existingByUsername is not null && existingByUsername.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
        {
            return Result<Staff>.Failure("staff.username_taken", 409, "Username is already taken.");
        }

        var existingByEmail = await _repository.GetStaffByIdentifierAsync(email);
        if (existingByEmail is not null && existingByEmail.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        {
            return Result<Staff>.Failure("staff.email_taken", 409, "Email is already registered.");
        }

        var staff = new Staff
        {
            FullName = input.FullName.Trim(),
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(input.Password),
            Role = normalizedRole,
            Status = "Active"
        };

        // If creating a Judge, auto-create a matching Judge record for score attribution
        if (normalizedRole == AppRoles.Judge)
        {
            staff.Judge = new Judge
            {
                Name = input.FullName.Trim(),
                Account = email,
                Role = AppRoles.Judge
            };
        }

        var created = await _repository.AddStaffAsync(staff);
        return Result<Staff>.Success(created);
    }
}

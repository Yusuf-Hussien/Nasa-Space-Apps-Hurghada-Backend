using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
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

    public async Task<Result<List<Staff>>> GetStaffsAsync(string? role, string? memberType, string? search)
    {
        var staffs = await _repository.GetStaffsAsync(role, memberType, search);
        return Result<List<Staff>>.Success(staffs);
    }

    public async Task<Result<Staff>> GetStaffByIdAsync(int staffId)
    {
        var staff = await _repository.GetStaffByIdAsync(staffId);
        if (staff is null)
        {
            return Result<Staff>.Failure("staff.not_found", 404, "Staff member not found.");
        }

        return Result<Staff>.Success(staff);
    }

    public async Task<Result<Staff>> AddStaffAsync(StaffInput input)
    {
        var validation = ValidateInput(input.FullName, input.Username, input.Email, input.Role, input.MemberType);
        if (validation is not null)
        {
            return validation;
        }

        if (input.Password!.Length < 6)
        {
            return Result<Staff>.Failure("staff.weak_password", 400, "Password must be at least 6 characters.");
        }

        var photoError = ImageValidator.Validate(input.Photo, "photo");
        if (photoError is not null)
        {
            return Result<Staff>.Failure(photoError.Code, 400, photoError.Message);
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
            MemberType = ResolveMemberType(normalizedRole, input.MemberType),
            Photo = string.IsNullOrWhiteSpace(input.Photo) ? null : input.Photo.Trim(),
            Status = "Active"
        };

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

    public async Task<Result<Staff>> UpdateStaffAsync(int staffId, StaffUpdateInput input)
    {
        var staff = await _repository.GetStaffByIdAsync(staffId);
        if (staff is null)
        {
            return Result<Staff>.Failure("staff.not_found", 404, "Staff member not found.");
        }

        var validation = ValidateInput(input.FullName, input.Username, input.Email, input.Role, input.MemberType);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedRole = AppRoles.StaffRoles.First(r => r.Equals(input.Role, StringComparison.OrdinalIgnoreCase));
        var username = input.Username.Trim();
        var email = input.Email.Trim();

        // Username uniqueness (exclude self)
        var existingByUsername = await _repository.GetStaffByIdentifierAsync(username);
        if (existingByUsername is not null && existingByUsername.StaffId != staffId)
        {
            return Result<Staff>.Failure("staff.username_taken", 409, "Username is already taken.");
        }

        // Email uniqueness (exclude self)
        var existingByEmail = await _repository.GetStaffByIdentifierAsync(email);
        if (existingByEmail is not null && existingByEmail.StaffId != staffId)
        {
            return Result<Staff>.Failure("staff.email_taken", 409, "Email is already registered.");
        }

        var photoError = ImageValidator.Validate(input.Photo, "photo");
        if (photoError is not null)
        {
            return Result<Staff>.Failure(photoError.Code, 400, photoError.Message);
        }

        // Password change (optional, only if provided)
        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            if (input.Password.Length < 6)
            {
                return Result<Staff>.Failure("staff.weak_password", 400, "Password must be at least 6 characters.");
            }

            staff.PasswordHash = _passwordHasher.Hash(input.Password);
        }

        var roleChanged = !staff.Role.Equals(normalizedRole, StringComparison.OrdinalIgnoreCase);


        staff.FullName = input.FullName.Trim();
        staff.Username = username;
        staff.Email = email;
        staff.Role = normalizedRole;
        staff.MemberType = ResolveMemberType(normalizedRole, input.MemberType);
        staff.Photo = string.IsNullOrWhiteSpace(input.Photo) ? null : input.Photo.Trim();

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var normalizedStatus = input.Status.Trim();
            if (normalizedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                normalizedStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                staff.Status = normalizedStatus;
            }
        }

        // If role changed to Judge and no linked Judge record, create one
        if (normalizedRole == AppRoles.Judge && !staff.JudgeId.HasValue)
        {
            staff.Judge = new Judge
            {
                Name = staff.FullName,
                Account = staff.Email,
                Role = AppRoles.Judge
            };
        }

        // If role changed away from Judge, clear the link (keep Judge record for score history)
        if (roleChanged && normalizedRole != AppRoles.Judge)
        {
            staff.JudgeId = null;
            staff.Judge = null;
        }

        await _repository.UpdateStaffAsync(staff);
        return Result<Staff>.Success(staff);
    }

    /// <summary>Soft-delete / restore. Inactive accounts can no longer log in.</summary>
    public async Task<Result<Staff>> SetStatusAsync(int staffId, string status)
    {
        var staff = await _repository.GetStaffByIdAsync(staffId);
        if (staff is null)
        {
            return Result<Staff>.Failure("staff.not_found", 404, "Staff member not found.");
        }

        staff.Status = status;
        await _repository.UpdateStaffAsync(staff);
        return Result<Staff>.Success(staff);
    }

    public async Task<Result<Staff>> DeleteStaffAsync(int staffId)
    {
        var staff = await _repository.GetStaffByIdAsync(staffId);
        if (staff is null)
        {
            return Result<Staff>.Failure("staff.not_found", 404, "Staff member not found.");
        }

        await _repository.DeleteStaffAsync(staff);
        return Result<Staff>.Success(staff);
    }

    private static Result<Staff>? ValidateInput(
        string? fullName, string? username, string? email, string? role, string? memberType)
    {
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email))
        {
            return Result<Staff>.Failure("staff.invalid", 400, "FullName, Username and Email are required.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return Result<Staff>.Failure("staff.invalid_role", 400, "Role is required.");
        }

        if (!AppRoles.StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            return Result<Staff>.Failure(
                "staff.invalid_role", 400,
                $"Role must be one of: {string.Join(", ", AppRoles.StaffRoles)}.");
        }

        var normalizedRole = AppRoles.StaffRoles.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));

        // MemberType is required for CoreTeam
        if (normalizedRole == AppRoles.CoreTeam)
        {
            if (string.IsNullOrWhiteSpace(memberType))
            {
                return Result<Staff>.Failure(
                    "staff.member_type_required", 400,
                    $"MemberType is required for CoreTeam. One of: {string.Join(", ", CoreTeamTypes.All)}.");
            }

            if (!CoreTeamTypes.IsValid(memberType))
            {
                return Result<Staff>.Failure(
                    "staff.invalid_member_type", 400,
                    $"MemberType must be one of: {string.Join(", ", CoreTeamTypes.All)}.");
            }
        }

        return null;
    }

    private static string? ResolveMemberType(string normalizedRole, string? memberType)
    {
        if (normalizedRole == AppRoles.CoreTeam && !string.IsNullOrWhiteSpace(memberType))
        {
            return CoreTeamTypes.Normalize(memberType);
        }

        return null;
    }
}

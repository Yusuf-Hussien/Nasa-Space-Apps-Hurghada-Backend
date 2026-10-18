using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Infrastructure.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
    }

    public string CreateParticipantToken(Team team, bool rememberMe = false)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, team.TeamId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, team.TeamId.ToString()),
            new Claim(ClaimTypes.Name, team.Username),
            new Claim(ClaimTypes.Role, AppRoles.Participant),
            new Claim("teamId", team.TeamId.ToString()),
            new Claim("name", team.Name)
        };

        return BuildToken(claims, rememberMe);
    }

    public string CreateStaffToken(Staff staff, bool rememberMe = false)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, staff.StaffId.ToString()),
            new(ClaimTypes.NameIdentifier, staff.StaffId.ToString()),
            new(ClaimTypes.Name, staff.Username),
            new(ClaimTypes.Role, staff.Role),
            new("staffId", staff.StaffId.ToString()),
            new("name", staff.FullName),
            new("email", staff.Email)
        };

        if (staff.JudgeId.HasValue)
        {
            claims.Add(new Claim("judgeId", staff.JudgeId.Value.ToString()));
        }

        return BuildToken(claims, rememberMe);
    }

    private string BuildToken(IEnumerable<Claim> claims, bool rememberMe)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var lifetimeMinutes = rememberMe ? _options.RememberMeExpiryMinutes : _options.ExpiryMinutes;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(lifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

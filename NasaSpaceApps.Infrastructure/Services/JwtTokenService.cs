using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Infrastructure.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateParticipantToken(Team team)
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

        return BuildToken(claims);
    }

    public string CreateStaffToken(Staff staff)
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

        return BuildToken(claims);
    }

    private string BuildToken(IEnumerable<Claim> claims)
    {
        var keyValue = _configuration["Jwt:Key"] ?? "ThisIsADevelopmentJwtSigningKey1234567890";
        var key = Encoding.UTF8.GetBytes(keyValue);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var expiryMinutes = 720; // default 12 hours — event-long hackathon session
        if (int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var configured) && configured > 0)
        {
            expiryMinutes = configured;
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "NasaSpaceApps",
            audience: _configuration["Jwt:Audience"] ?? "NasaSpaceApps.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

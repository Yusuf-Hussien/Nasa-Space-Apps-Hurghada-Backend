namespace NASAHackathon.API.DTOs.Auth
{
    public class TeamLoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } = 3600;
        public TeamAuthDto Team { get; set; } = new();
    }

    public class TeamAuthDto
    {
        public int TeamId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int ChallengeId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

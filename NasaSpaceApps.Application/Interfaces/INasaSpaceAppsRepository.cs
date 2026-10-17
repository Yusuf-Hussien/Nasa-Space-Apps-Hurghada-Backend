using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Interfaces;

public interface INasaSpaceAppsRepository
{
    Task<Team?> GetTeamByUsernameAsync(string username);
    Task<Team?> GetTeamByIdAsync(int teamId);
    Task<List<Team>> GetTeamsAsync(string? search = null);
    Task<List<Challenge>> GetChallengesAsync();
    Task<Challenge?> GetChallengeByIdAsync(int challengeId);
    Task<Student?> GetStudentByIdAsync(int studentId);
    Task<List<Student>> GetStudentsByTeamIdAsync(int teamId);
    Task<List<ServiceRequest>> GetRequestsAsync(int? teamId = null, string? type = null, string? status = null);
    Task<ServiceRequest> AddRequestAsync(ServiceRequest request);
    Task<Attendance> AddAttendanceAsync(Attendance attendance);
    Task<List<JudgeScore>> GetScoresForTeamAsync(int teamId);
    Task<List<JudgingCriterion>> GetJudgingCriteriaAsync();
    Task<JudgingCriterion?> GetJudgingCriterionByIdAsync(int criterionId);
    Task AddJudgeScoreAsync(JudgeScore score);
    Task<List<Sponsor>> GetSponsorsAsync(bool activeOnly = true);
    Task<List<SponsorTier>> GetSponsorTiersAsync();
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<Question>> GetQuestionsByChallengeAsync(int challengeId);
    Task<List<GameParticipation>> GetParticipationsAsync(int questionId);
}

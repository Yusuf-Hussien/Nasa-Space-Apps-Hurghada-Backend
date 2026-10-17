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
    Task<List<Student>> GetStudentsByIdsAsync(IEnumerable<int> studentIds);
    Task<List<Student>> GetAllStudentsWithTeamAsync();
    Task<List<ServiceRequest>> GetRequestsAsync(int? teamId = null, string? type = null, string? status = null);
    Task<ServiceRequest?> GetRequestByIdAsync(int requestId);
    Task<ServiceRequest> AddRequestAsync(ServiceRequest request);
    Task UpdateRequestAsync(ServiceRequest request);
    Task<Attendance> AddAttendanceAsync(Attendance attendance);
    Task<Attendance?> GetAttendanceForStudentDateAsync(int studentId, DateTime eventDate);
    Task UpdateAttendanceAsync(Attendance attendance);
    Task<List<Attendance>> GetAttendancesAsync(int? teamId, DateTime? eventDate, string? status);
    Task<List<JudgeScore>> GetScoresForTeamAsync(int teamId);
    Task<List<JudgingCriterion>> GetJudgingCriteriaAsync();
    Task<JudgingCriterion?> GetJudgingCriterionByIdAsync(int criterionId);
    Task AddJudgeScoreAsync(JudgeScore score);
    Task<List<Sponsor>> GetSponsorsAsync(bool activeOnly = true);
    Task<List<SponsorTier>> GetSponsorTiersAsync();

    // Staff (Admin / CoreTeam / Judge login pool)
    Task<Staff?> GetStaffByIdentifierAsync(string identifier);
    Task<Staff?> GetStaffByIdAsync(int staffId);
    Task<List<Staff>> GetStaffsAsync(string? role = null, string? search = null);
    Task<Staff> AddStaffAsync(Staff staff);

    // Questions
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<Question>> GetQuestionsByChallengeAsync(int challengeId);
    Task<List<Question>> GetPublishedQuestionsAsync(int? challengeId = null);
    Task<List<Question>> GetAllQuestionsAsync(int? challengeId = null);
    Task<Question> AddQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);

    // Game (side trophy)
    Task<List<GameParticipation>> GetParticipationsAsync(int questionId);
    Task<GameParticipation?> GetParticipationAsync(int questionId, int teamId);
    Task<GameParticipation> AddParticipationAsync(GameParticipation participation);
    Task<Dictionary<int, (int CorrectCount, int TotalPoints)>> GetTeamGameTotalsAsync();
}

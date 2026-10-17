using Microsoft.EntityFrameworkCore;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;
using NasaSpaceApps.Infrastructure.Data;

namespace NasaSpaceApps.Infrastructure.Repositories;

public sealed class NasaSpaceAppsRepository : INasaSpaceAppsRepository
{
    private readonly NasaSpaceAppsDbContext _context;

    public NasaSpaceAppsRepository(NasaSpaceAppsDbContext context)
    {
        _context = context;
    }

    public async Task<Team?> GetTeamByUsernameAsync(string username)
    {
        var normalized = username.Trim().ToUpper();
        return await _context.Teams
            .Include(team => team.Challenge)
            .FirstOrDefaultAsync(team => team.Username.ToUpper() == normalized);
    }

    public async Task<Team?> GetTeamByIdAsync(int teamId)
    {
        return await _context.Teams
            .Include(team => team.Challenge)
            .FirstOrDefaultAsync(team => team.TeamId == teamId);
    }

    public async Task<List<Team>> GetTeamsAsync(string? search = null)
    {
        var query = _context.Teams.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToUpper();
            var hasNumeric = int.TryParse(search.Trim(), out var searchId);
            query = query.Where(team =>
                team.Name.ToUpper().Contains(normalized) ||
                team.Username.ToUpper().Contains(normalized) ||
                (hasNumeric && team.TeamId == searchId));
        }

        return await query
            .OrderBy(team => team.Name)
            .ToListAsync();
    }

    public async Task<List<Challenge>> GetChallengesAsync()
    {
        return await _context.Challenges
            .OrderBy(challenge => challenge.Title)
            .ToListAsync();
    }

    public async Task<Challenge?> GetChallengeByIdAsync(int challengeId)
    {
        return await _context.Challenges
            .Include(challenge => challenge.Teams)
            .FirstOrDefaultAsync(challenge => challenge.ChallengeId == challengeId);
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        return await _context.Students
            .Include(student => student.Team)
            .FirstOrDefaultAsync(student => student.StudentId == studentId);
    }

    public async Task<List<Student>> GetStudentsByTeamIdAsync(int teamId)
    {
        return await _context.Students
            .Where(student => student.TeamId == teamId)
            .OrderBy(student => student.Name)
            .ToListAsync();
    }

    public async Task<List<Student>> GetStudentsByIdsAsync(IEnumerable<int> studentIds)
    {
        var ids = studentIds.ToList();
        return await _context.Students
            .Where(student => ids.Contains(student.StudentId))
            .ToListAsync();
    }

    public async Task<List<Student>> GetAllStudentsWithTeamAsync()
    {
        return await _context.Students
            .Include(student => student.Team)
            .OrderBy(student => student.Name)
            .ToListAsync();
    }

    public async Task<List<ServiceRequest>> GetRequestsAsync(int? teamId = null, string? type = null, string? status = null)
    {
        var query = _context.ServiceRequests.AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(request => request.TeamId == teamId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(request => request.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(request => request.Status == status);
        }

        return await query
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
    }

    public async Task<ServiceRequest?> GetRequestByIdAsync(int requestId)
    {
        return await _context.ServiceRequests
            .FirstOrDefaultAsync(request => request.RequestId == requestId);
    }

    public async Task<ServiceRequest> AddRequestAsync(ServiceRequest request)
    {
        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task UpdateRequestAsync(ServiceRequest request)
    {
        _context.ServiceRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<Attendance> AddAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
    }

    public async Task<Attendance?> GetAttendanceForStudentDateAsync(int studentId, DateTime eventDate)
    {
        var date = eventDate.Date;
        return await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.EventDate.Date == date);
    }

    public async Task UpdateAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Attendance>> GetAttendancesAsync(int? teamId, DateTime? eventDate, string? status)
    {
        var query = _context.Attendances
            .Include(a => a.Student)
            .AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(a => a.Student!.TeamId == teamId.Value);
        }

        if (eventDate.HasValue)
        {
            var date = eventDate.Value.Date;
            query = query.Where(a => a.EventDate.Date == date);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        return await query
            .OrderBy(a => a.EventDate)
            .ThenBy(a => a.Student!.Name)
            .ToListAsync();
    }

    public async Task<List<JudgeScore>> GetScoresForTeamAsync(int teamId)
    {
        return await _context.JudgeScores
            .Where(score => score.TeamId == teamId)
            .Include(score => score.Criterion)
            .OrderByDescending(score => score.Timestamp)
            .ToListAsync();
    }

    public async Task<List<JudgingCriterion>> GetJudgingCriteriaAsync()
    {
        return await _context.JudgingCriteria
            .OrderBy(criterion => criterion.Name)
            .ToListAsync();
    }

    public async Task<JudgingCriterion?> GetJudgingCriterionByIdAsync(int criterionId)
    {
        return await _context.JudgingCriteria
            .FirstOrDefaultAsync(criterion => criterion.CriterionId == criterionId);
    }

    public async Task AddJudgeScoreAsync(JudgeScore score)
    {
        _context.JudgeScores.Add(score);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Sponsor>> GetSponsorsAsync(bool activeOnly = true)
    {
        var query = _context.Sponsors
            .Include(sponsor => sponsor.Tier)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(sponsor => sponsor.Active);
        }

        return await query
            .OrderBy(sponsor => sponsor.DisplayOrder ?? int.MaxValue)
            .ToListAsync();
    }

    public async Task<List<SponsorTier>> GetSponsorTiersAsync()
    {
        return await _context.SponsorTiers
            .Include(tier => tier.Sponsors)
            .OrderBy(tier => tier.Priority ?? int.MaxValue)
            .ToListAsync();
    }

    // ---- Staff ----

    public async Task<Staff?> GetStaffByIdentifierAsync(string identifier)
    {
        var normalized = identifier.Trim().ToUpper();
        return await _context.Staff
            .Include(s => s.Judge)
            .FirstOrDefaultAsync(s =>
                s.Username.ToUpper() == normalized ||
                s.Email.ToUpper() == normalized);
    }

    public async Task<Staff?> GetStaffByIdAsync(int staffId)
    {
        return await _context.Staff
            .Include(s => s.Judge)
            .FirstOrDefaultAsync(s => s.StaffId == staffId);
    }

    public async Task<List<Staff>> GetStaffsAsync(string? role = null, string? search = null)
    {
        var query = _context.Staff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(s => s.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToUpper();
            query = query.Where(s =>
                s.FullName.ToUpper().Contains(normalized) ||
                s.Username.ToUpper().Contains(normalized) ||
                s.Email.ToUpper().Contains(normalized));
        }

        return await query.OrderBy(s => s.FullName).ToListAsync();
    }

    public async Task<Staff> AddStaffAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    // ---- Questions ----

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await _context.Questions
            .Include(q => q.Challenge)
            .FirstOrDefaultAsync(question => question.QuestionId == id);
    }

    public async Task<List<Question>> GetQuestionsByChallengeAsync(int challengeId)
    {
        return await _context.Questions
            .Where(question => question.ChallengeId == challengeId)
            .OrderBy(question => question.QuestionId)
            .ToListAsync();
    }

    public async Task<List<Question>> GetPublishedQuestionsAsync(int? challengeId = null)
    {
        var query = _context.Questions.Where(q => q.IsPublished && q.IsActive);

        if (challengeId.HasValue)
        {
            query = query.Where(q => q.ChallengeId == challengeId.Value);
        }

        return await query.OrderBy(q => q.QuestionId).ToListAsync();
    }

    public async Task<List<Question>> GetAllQuestionsAsync(int? challengeId = null)
    {
        var query = _context.Questions.Where(q => q.IsActive);

        if (challengeId.HasValue)
        {
            query = query.Where(q => q.ChallengeId == challengeId.Value);
        }

        return await query.OrderBy(q => q.QuestionId).ToListAsync();
    }

    public async Task<Question> AddQuestionAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    // ---- Game (side trophy) ----

    public async Task<List<GameParticipation>> GetParticipationsAsync(int questionId)
    {
        return await _context.GameParticipations
            .Where(participation => participation.QuestionId == questionId)
            .OrderByDescending(participation => participation.SubmittedAt)
            .ToListAsync();
    }

    public async Task<GameParticipation?> GetParticipationAsync(int questionId, int teamId)
    {
        return await _context.GameParticipations
            .FirstOrDefaultAsync(p => p.QuestionId == questionId && p.TeamId == teamId);
    }

    public async Task<GameParticipation> AddParticipationAsync(GameParticipation participation)
    {
        _context.GameParticipations.Add(participation);
        await _context.SaveChangesAsync();
        return participation;
    }

    public async Task<Dictionary<int, (int CorrectCount, int TotalPoints)>> GetTeamGameTotalsAsync()
    {
        var participations = await _context.GameParticipations
            .Include(p => p.Question)
            .ToListAsync();

        return participations
            .Where(p => p.Question != null)
            .GroupBy(p => p.TeamId)
            .ToDictionary(
                g => g.Key,
                g => (
                    CorrectCount: g.Count(p => p.Correct),
                    TotalPoints: g.Where(p => p.Correct).Sum(p => p.Question!.Points)
                ));
    }
}

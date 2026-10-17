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
        return await _context.Teams
            .Include(team => team.Challenge)
            .FirstOrDefaultAsync(team => team.Username == username);
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
            query = query.Where(team => team.Name.Contains(search) || team.Username.Contains(search));
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

    public async Task<ServiceRequest> AddRequestAsync(ServiceRequest request)
    {
        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<Attendance> AddAttendanceAsync(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance;
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

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await _context.Questions
            .FirstOrDefaultAsync(question => question.QuestionId == id);
    }

    public async Task<List<Question>> GetQuestionsByChallengeAsync(int challengeId)
    {
        return await _context.Questions
            .Where(question => question.ChallengeId == challengeId)
            .OrderBy(question => question.QuestionId)
            .ToListAsync();
    }

    public async Task<List<GameParticipation>> GetParticipationsAsync(int questionId)
    {
        return await _context.GameParticipations
            .Where(participation => participation.QuestionId == questionId)
            .OrderByDescending(participation => participation.SubmittedAt)
            .ToListAsync();
    }
}

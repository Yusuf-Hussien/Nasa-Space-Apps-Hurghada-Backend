using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Application.Tests;

public class QuestionScoreMatchTests
{
    private static NasaSpaceApps.Domain.Entities.QuestionScore Existing(int id, int teamId, int criterionId, int? questionId, int judgeId, string? name) =>
        new()
        {
            QuestionScoreId = id,
            TeamId = teamId,
            CriterionId = criterionId,
            CriterionQuestionId = questionId,
            JudgeId = judgeId,
            QuestionName = name
        };

    [Fact]
    public void Matches_ShouldMatchCatalogueQuestion_ByJudgeAndQuestionId()
    {
        var existing = Existing(1, 10, 5, 100, 7, "Question text");
        Assert.True(QuestionScoreMatch.Matches(existing, 10, 5, 100, 7, "Question text"));
        Assert.False(QuestionScoreMatch.Matches(existing, 10, 5, 100, 8, "Question text"));
        Assert.False(QuestionScoreMatch.Matches(existing, 10, 5, 101, 7, "Question text"));
    }

    [Fact]
    public void Matches_ShouldMatchAdHocQuestion_ByNameWhenQuestionIdIsNull()
    {
        var existing = Existing(1, 10, 5, null, 7, "Bonus: teamwork");
        Assert.True(QuestionScoreMatch.Matches(existing, 10, 5, null, 7, "Bonus: teamwork"));
        Assert.True(QuestionScoreMatch.Matches(existing, 10, 5, null, 7, "  Bonus: teamwork  "));
        Assert.False(QuestionScoreMatch.Matches(existing, 10, 5, null, 7, "Bonus: effort"));
    }

    [Fact]
    public void Matches_ShouldTreatNullAdHocNamesAsTheSameSlot()
    {
        var existing = Existing(1, 10, 5, null, 7, null);
        Assert.True(QuestionScoreMatch.Matches(existing, 10, 5, null, 7, null));
        Assert.True(QuestionScoreMatch.Matches(existing, 10, 5, null, 7, "   "));
    }

    [Fact]
    public void FindExisting_ShouldReturnMatchingRow()
    {
        var scores = new List<NasaSpaceApps.Domain.Entities.QuestionScore>
        {
            Existing(1, 10, 5, 100, 7, "Q1"),
            Existing(2, 10, 5, null, 7, "Bonus")
        };

        var found = QuestionScoreMatch.FindExisting(scores, 10, 5, 100, 7, "Q1");
        Assert.NotNull(found);
        Assert.Equal(1, found!.QuestionScoreId);

        var bonus = QuestionScoreMatch.FindExisting(scores, 10, 5, null, 7, "Bonus");
        Assert.NotNull(bonus);
        Assert.Equal(2, bonus!.QuestionScoreId);
    }
}

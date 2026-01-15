using JobHunter.Models;
using JobHunter.Persistence;

namespace JobHunter.Services;

public class ScoreService
{
    private readonly LearningRepository _learning;

    public ScoreService(LearningRepository learning)
    {
        _learning = learning;
    }

    public int Calculate(Job job)
    {
        double score = 0;
        var title = job.Title.ToLower();

        if (title.Contains(".net")) score += _learning.GetWeight("skill:.net");
        if (title.Contains("unity")) score += _learning.GetWeight("skill:unity");
        if (title.Contains("ai")) score += _learning.GetWeight("skill:ai");
        if (title.Contains("maui")) score += _learning.GetWeight("skill:maui");

        return (int)Math.Round(score * 10);
    }
}
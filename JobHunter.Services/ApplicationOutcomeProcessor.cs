using JobHunter.Enums;
using JobHunter.Models;
using JobHunter.Persistence;

namespace JobHunter.Services;

public class ApplicationOutcomeProcessor
{
    private readonly LearningRepository _learning;

    public ApplicationOutcomeProcessor(LearningRepository learning)
    {
        _learning = learning;
    }

    public void Process(Job job, ApplicationOutcome outcome)
    {
        var delta = outcome switch
        {
            ApplicationOutcome.Interview => +1.0,
            ApplicationOutcome.Rejected => -1.0,
            _ => 0.0
        };

        if (delta == 0) return;

        foreach (var feature in ExtractFeatures(job))
        {
            _learning.UpdateWeight(feature, delta);
        }
    }

    private IEnumerable<string> ExtractFeatures(Job job)
    {
        var title = job.Title.ToLower();

        if (title.Contains(".net")) yield return "skill:.net";
        if (title.Contains("unity")) yield return "skill:unity";
        if (title.Contains("ai") || title.Contains("ml")) yield return "skill:ai";
        if (title.Contains("maui")) yield return "skill:maui";
        if (title.Contains("security")) yield return "skill:security";

        yield return "company:generic"; // placeholder p/ parser futuro
    }
}
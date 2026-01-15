using JobHunter.Models;

namespace JobHunter.Services;

public class DynamicCvScorer
{
    private readonly CvProfile cv;

    public DynamicCvScorer(CvProfile cv) => this.cv = cv;

    public int Score(Job job)
    {
        int score = 0;
        foreach (var s in cv.Skills)
            if (job.FullText.Contains(s.Key, StringComparison.OrdinalIgnoreCase))
                score += s.Value;
        return score;
    }
}

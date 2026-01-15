using JobHunter.Enums;
using JobHunter.Models;

namespace JobHunter.Services;

public class CvWeightTrainer
{
    private readonly CvProfile cv;
    public CvWeightTrainer(CvProfile cv) => this.cv = cv;

    public void Train(Job job, ApplicationOutcome outcome)
    {
        int delta = outcome switch
        {
            ApplicationOutcome.Interview => 5,
            ApplicationOutcome.Rejected => -3,
            _ => 0
        };

        foreach (var k in cv.Skills.Keys.ToList())
            if (job.FullText.Contains(k, StringComparison.OrdinalIgnoreCase))
                cv.Skills[k] = Math.Clamp(cv.Skills[k] + delta, 1, 100);
    }
}

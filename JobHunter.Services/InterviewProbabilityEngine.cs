using JobHunter.Models;

namespace JobHunter.Services;

public class InterviewProbabilityEngine
{
    public double Calculate(Job job)
    {
        double p = job.Score / 200.0;
        return Math.Min(p, 0.95);
    }
}

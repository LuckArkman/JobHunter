using JobHunter.Enums;
using JobHunter.Models;
using JobHunter.Persistence;
using JobHunter.Services;

namespace JobHunter.UI.Services;

public class DashboardService
{
    private readonly JobRepository _jobs;
    private readonly FeedbackRepository _feedback;
    private readonly ApplicationOutcomeProcessor _processor;

    public DashboardService(
        JobRepository jobs,
        FeedbackRepository feedback,
        ApplicationOutcomeProcessor processor)
    {
        _jobs = jobs;
        _feedback = feedback;
        _processor = processor;
    }

    public List<Job> GetJobs()
        => _jobs.GetAll()
            .OrderByDescending(j => j.Score)
            .ToList();

    public void RegisterFeedback(Job job, ApplicationOutcome outcome)
    {
        _feedback.Save(job.Url, outcome);
        _processor.Process(job, outcome);
    }
}

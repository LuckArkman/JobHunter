using JobHunter.Enums;
using JobHunter.Models;
using JobHunter.Persistence;
using JobHunter.Services;

namespace JobHunter.UI.Services;

public class DashboardService
{
    private readonly JobRepository _jobs;
    private readonly LinkedInCollector _collector;
    private readonly DynamicCvScorer _scorer;
    private readonly FeedbackRepository _feedback;
    private readonly ApplicationOutcomeProcessor _processor;

    public DashboardService(
        JobRepository jobs,
        LinkedInCollector collector,
        DynamicCvScorer scorer,
        FeedbackRepository feedback,
        ApplicationOutcomeProcessor processor)
    {
        _jobs = jobs;
        _collector = collector;
        _scorer = scorer;
        _feedback = feedback;
        _processor = processor;
    }

    public List<Job> GetJobs()
        => _jobs.GetAll()
            .OrderByDescending(j => j.Score)
            .ToList();

    public async Task<int> RunCollectionAndScoring()
    {
        // 1. Busca dados da web
        var rawJobs = await _collector.CollectAsync();
        int newJobsCount = 0;

        foreach (var job in rawJobs)
        {
            // 2. Evita salvar vagas repetidas no banco de VAGAS
            if (_jobs.GetByUrl(job.Url) != null) 
                continue;

            // 3. Calcula Score
            job.Score = _scorer.Score(job);

            // 4. Salva apenas vagas relevantes
            if (job.Score > 0)
            {
                _jobs.Save(job);
                newJobsCount++;
            }
        }

        return newJobsCount;
    }

    public void RegisterFeedback(Job job, ApplicationOutcome outcome)
    {
        _feedback.Save(job.Url, outcome);
        _processor.Process(job, outcome);
    }
}
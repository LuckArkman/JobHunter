using JobHunter.Enums;
using JobHunter.Models;

namespace JobHunter.Services;

public class CompanyRankService
{
    private readonly Dictionary<string, CompanyProfile> companies = new();

    public void Register(Job job, ApplicationOutcome outcome)
    {
        if (!companies.ContainsKey(job.Company))
            companies[job.Company] = new CompanyProfile { Name = job.Company };

        if (outcome == ApplicationOutcome.Interview)
            companies[job.Company].Interviews++;

        if (outcome == ApplicationOutcome.Rejected)
            companies[job.Company].Rejections++;
    }

    public IEnumerable<CompanyProfile> Rank()
        => companies.Values.OrderByDescending(c => c.Score);
}

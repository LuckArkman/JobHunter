using JobHunter.Models;

namespace JobHunter.Services;

public class JobFilter
{
    private readonly string[] required = { ".NET", "C#" };

    public bool IsMatch(Job job)
        => required.Any(k => job.FullText.Contains(k, StringComparison.OrdinalIgnoreCase));
}

using JobHunter.Models;

namespace JobHunter.Services;

using HtmlAgilityPack;

public class LinkedInCollector
{
    private readonly string[] queries =
    {
        ".NET Unity",
        ".NET Artificial Intelligence",
        "Neural Networks C#",
        "Genetic Algorithms C#",
        ".NET MAUI",
        ".NET Security"
    };

    public async Task<List<Job>> CollectAsync()
    {
        var jobs = new List<Job>();
        using var http = new HttpClient();

        http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        foreach (var q in queries)
        {
            var url = $"https://www.linkedin.com/jobs/search/?keywords={Uri.EscapeDataString(q)}";
            var html = await http.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//a[contains(@class,'base-card__full-link')]");
            if (nodes == null) continue;

            foreach (var n in nodes)
            {
                jobs.Add(new Job
                {
                    Title = n.InnerText.Trim(),
                    Url = n.GetAttributeValue("href", ""),
                    Source = "LinkedIn"
                });
            }
        }

        return jobs.GroupBy(j => j.Url).Select(g => g.First()).ToList();
    }
}

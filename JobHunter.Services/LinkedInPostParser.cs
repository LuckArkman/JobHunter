using JobHunter.Models;

namespace JobHunter.Services;

using HtmlAgilityPack;

public class LinkedInPostParser
{
    public async Task<List<Job>> CollectAsync(string keyword)
    {
        var jobs = new List<Job>();
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        var url = $"https://www.linkedin.com/search/results/content/?keywords={Uri.EscapeDataString(keyword)}";
        var html = await http.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'/jobs/view')]");
        if (nodes == null) return jobs;

        foreach (var n in nodes)
        {
            jobs.Add(new Job
            {
                Title = "Vaga via Post",
                Url = n.GetAttributeValue("href", ""),
                Source = "LinkedIn Post"
            });
        }

        return jobs;
    }
}

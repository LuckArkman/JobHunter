namespace JobHunter.Models;

public class Job
{
    public string Title { get; set; }
    public string Company { get; set; } = "Unknown";
    public string Url { get; set; }
    public string Source { get; set; }
    public int Score { get; set; }
    public double Probability { get; set; }
    public string FullText => $"{Title} {Company}";
}
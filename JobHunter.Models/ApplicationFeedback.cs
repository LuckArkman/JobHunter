using JobHunter.Enums;

namespace JobHunter.Models;

public class ApplicationFeedback
{
    public string JobUrl { get; set; } = string.Empty;
    public ApplicationOutcome Outcome { get; set; }
    public DateTime Date { get; set; }
}
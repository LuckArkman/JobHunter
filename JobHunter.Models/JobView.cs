namespace JobHunter.Models;

public class JobView
{
    public Job Job { get; set; }
    public int Score => Job.Score;
    public double Probability => Job.Probability;
}

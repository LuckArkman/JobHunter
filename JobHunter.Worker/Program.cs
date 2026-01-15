using JobHunter.Models;
using JobHunter.Persistence;
using JobHunter.Services;

var cv = new CvProfile();
var scorer = new DynamicCvScorer(cv);
var probability = new InterviewProbabilityEngine();
var collector = new LinkedInCollector();
var repo = new JobRepository();

var jobs = await collector.CollectAsync();

foreach (var job in jobs)
{
    job.Score = scorer.Score(job);
    job.Probability = probability.Calculate(job);
    repo.Save(job);
    Console.WriteLine($"{job.Score} | {job.Title}");
}
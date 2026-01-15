using JobHunter.Models;
using JobHunter.Persistence;
using JobHunter.Services;
using JobHunter.UI.Components;
using JobHunter.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<CvProfile>();
builder.Services.AddSingleton<DynamicCvScorer>();
builder.Services.AddSingleton<InterviewProbabilityEngine>();
builder.Services.AddSingleton<CvWeightTrainer>();
builder.Services.AddSingleton<CompanyRankService>();
builder.Services.AddSingleton<ApplicationOutcomeProcessor>();
builder.Services.AddSingleton<ScoreService>();
builder.Services.AddSingleton<JobRepository>();
builder.Services.AddSingleton<FeedbackRepository>();

builder.Services.AddSingleton<DashboardService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
using JobHunter.Models;
using JobHunter.Persistence;
using JobHunter.Services;
using JobHunter.UI.Components;
using JobHunter.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Modelos e Lógica de Negócio
builder.Services.AddSingleton<CvProfile>();
builder.Services.AddSingleton<DynamicCvScorer>();
builder.Services.AddSingleton<InterviewProbabilityEngine>();
builder.Services.AddSingleton<CvWeightTrainer>();
builder.Services.AddSingleton<CompanyRankService>();

// Persistência
builder.Services.AddSingleton<LearningRepository>();
builder.Services.AddSingleton<JobRepository>();
builder.Services.AddSingleton<FeedbackRepository>();

// Serviços de Processamento
builder.Services.AddSingleton<ApplicationOutcomeProcessor>();
builder.Services.AddSingleton<ScoreService>();
builder.Services.AddTransient<LinkedInCollector>(); // Coletor leve (HttpClient)

// O Bot de Automação (Scoped pois mantém estado do Browser por sessão de uso)
builder.Services.AddScoped<LinkedInBotService>();

// Serviço de UI
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
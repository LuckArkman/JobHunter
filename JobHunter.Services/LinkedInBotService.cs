using JobHunter.Models;
using JobHunter.Persistence;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace JobHunter.Services;

public class LinkedInBotService : IDisposable
{
    private IWebDriver? _driver;
    private readonly IConfiguration _config;
    private readonly FeedbackRepository _feedbackRepo;
    public event Action<string>? OnLog;

    public LinkedInBotService(IConfiguration config, FeedbackRepository feedbackRepo)
    {
        _config = config;
        _feedbackRepo = feedbackRepo;
    }

    private void Log(string message) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");

    public void InitializeDriver()
    {
        if (_driver != null) return;

        var options = new ChromeOptions();
        // options.AddArgument("--headless"); // Descomente para rodar sem interface gráfica
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");

        // Importante para evitar detecção básica de bots
        options.AddArgument("--disable-blink-features=AutomationControlled");

        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Login()
    {
        InitializeDriver();
        var email = _config["LinkedIn:Email"];
        var pwd = _config["LinkedIn:Password"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            Log("ERRO: Credenciais não configuradas no appsettings.json");
            return;
        }

        try
        {
            Log("Acessando página de login...");
            _driver!.Navigate().GoToUrl("https://www.linkedin.com/login");

            _driver.FindElement(By.Id("username")).SendKeys(email);
            _driver.FindElement(By.Id("password")).SendKeys(pwd);

            Log("Enviando credenciais...");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Espera manual para caso haja CAPTCHA ou 2FA
            Log("Aguardando carregamento do feed (10s)...");
            Thread.Sleep(10000);
        }
        catch (Exception ex)
        {
            Log($"Erro no Login: {ex.Message}");
        }
    }

    public async Task RunAutoApply(List<Job> jobs)
    {
        int max = int.Parse(_config["BotSettings:MaxApplicationsPerRun"] ?? "5");
        int count = 0;

        foreach (var job in jobs)
        {
            if (count >= max) break;

            // Só aplica se tiver score alto e for Easy Apply (URL interna)
            if (job.Score < int.Parse(_config["BotSettings:MinScoreToApply"] ?? "0"))
            {
                Log($"Pulando {job.Title} (Score {job.Score} muito baixo).");
                continue;
            }

            // Verifica se é uma vaga interna do LinkedIn (Easy Apply costuma ter IDs numéricos na URL ou /jobs/view)
            if (!job.Url.Contains("linkedin.com"))
            {
                Log($"Pulando {job.Title} (Link externo).");
                continue;
            }

            Log($"Processando: {job.Title}...");
            bool success = ApplyToJob(job);

            if (success)
            {
                _feedbackRepo.Save(job.Url, Enums.ApplicationOutcome.Sent);
                count++;
            }
            else
            {
                _feedbackRepo.Save(job.Url, Enums.ApplicationOutcome.Ignored);
            }

            // Pausa humana para evitar banimento
            int delay = new Random().Next(5000, 10000);
            Log($"Aguardando {delay}ms...");
            await Task.Delay(delay);
        }

        Log("Ciclo de aplicação finalizado.");
    }

    private bool ApplyToJob(Job job)
    {
        try
        {
            _driver!.Navigate().GoToUrl(job.Url);
            Thread.Sleep(3000);

            // Tentar localizar o botão "Candidatura Simplificada" (Easy Apply)
            // O seletor pode variar, usando XPath para buscar pelo texto
            var buttons = _driver.FindElements(By.XPath("//button[contains(., 'Candidatura simplificada')]"));

            if (buttons.Count == 0)
            {
                buttons = _driver.FindElements(By.XPath("//button[contains(., 'Easy Apply')]"));
            }

            if (buttons.Count == 0)
            {
                Log("Botão Easy Apply não encontrado ou já aplicado.");
                return false;
            }

            buttons[0].Click();
            Thread.Sleep(2000);

            // Fluxo do Modal de Aplicação
            return HandleApplicationModal();
        }
        catch (Exception ex)
        {
            Log($"Erro ao aplicar para {job.Title}: {ex.Message}");
            return false;
        }
    }

    private bool HandleApplicationModal()
    {
        int attempts = 0;
        while (attempts < 10) // Evita loop infinito
        {
            try
            {
                // Prioridade 1: Botão Enviar (Sucesso)
                var submitBtns =
                    _driver!.FindElements(
                        By.XPath("//button[contains(., 'Enviar candidatura') or contains(., 'Submit application')]"));
                if (submitBtns.Count > 0)
                {
                    submitBtns[0].Click();
                    Log("Candidatura enviada com sucesso!");
                    Thread.Sleep(2000);
                    // Fechar modal de confirmação se houver
                    try
                    {
                        _driver.FindElement(By.CssSelector("button[aria-label='Dismiss']")).Click();
                    }
                    catch
                    {
                    }

                    return true;
                }

                // Prioridade 2: Botão Avançar/Revisar
                var nextBtns =
                    _driver.FindElements(
                        By.XPath("//button[contains(., 'Avançar') or contains(., 'Next') or contains(., 'Review')]"));
                if (nextBtns.Count > 0)
                {
                    nextBtns[0].Click();
                    Thread.Sleep(1000);
                    attempts++;
                    continue;
                }

                // Se não achou botão de avançar nem de enviar, pode ter travado em validação de form
                // Tenta preencher inputs genéricos (muito difícil generalizar, mas aqui vai um best-effort)
                FillGenericInputs();

                // Se mesmo após tentar preencher não achar botão, aborta
                Log("Não foi possível avançar no formulário.");
                return false;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private void FillGenericInputs()
    {
        // Exemplo: Tenta achar radios e marcar "Sim" para perguntas legais
        try
        {
            // Seleciona o primeiro radio button de cada grupo (geralmente é "Sim")
            // Cuidado: isso é arriscado em produção real
            var radios = _driver!.FindElements(By.CssSelector("input[type='radio']"));
            foreach (var r in radios)
            {
                if (!r.Selected)
                {
                    try
                    {
                        _driver.FindElement(By.CssSelector($"label[for='{r.GetAttribute("id")}']")).Click();
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
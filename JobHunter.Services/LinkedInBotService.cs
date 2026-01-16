using JobHunter.Models;
using JobHunter.Persistence;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace JobHunter.Services;

public class LinkedInBotService : IDisposable
{
    private IWebDriver? _driver;
    private readonly IConfiguration _config;
    private readonly FeedbackRepository _feedbackRepo;

    public event Action<string>? OnLog;

// Respostas padrão para o Bot preencher formulários
    private readonly Dictionary<string, string> _smartAnswers = new()
    {
        { "years", "5" },
        { "experiência", "5" },
        { "mobile", "+5562982424441" },
        { "phone", "5562982424441" },
        { "celular", "5562982424441" },
        { "salary", "10000" },
        { "pretensão", "10000" },
        { "english", "madium" },
        { "inglês", "medium" }
    };

    public LinkedInBotService(IConfiguration config, FeedbackRepository feedbackRepo)
    {
        _config = config;
        _feedbackRepo = feedbackRepo;
    }

    private void Log(string msg) => OnLog?.Invoke(msg);

    public void Login()
    {
        try 
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--remote-allow-origins=*");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            var email = _config["LinkedIn:Email"];
            var pwd = _config["LinkedIn:Password"];

            Log("🔑 Acessando LinkedIn...");
            _driver.Navigate().GoToUrl("https://www.linkedin.com/login");
            
            _driver.FindElement(By.Id("username")).SendKeys(email);
            _driver.FindElement(By.Id("password")).SendKeys(pwd);
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            Log("⏳ Aguardando 10s (Verifique se pediu Captcha)...");
            Thread.Sleep(10000); 
        }
        catch (Exception ex)
        {
            Log($"❌ Erro no Login: {ex.Message}");
            Dispose();
        }
    }

    public async Task RunAutoApply(List<Job> jobs)
    {
        if (_driver == null) { Log("❌ Navegador fechado."); return; }

        Log($"🚀 Iniciando automação em {jobs.Count} vagas...");

        foreach (var job in jobs)
        {
            // Verificação de segurança do browser
            if (_driver.WindowHandles.Count == 0) break;

            // 1. Verifica Persistência
            if (_feedbackRepo.HasApplied(job.Url))
            {
                Log($"⏭️ Já aplicado: {job.Title}");
                continue;
            }

            if (!job.Url.Contains("linkedin.com")) continue;

            Log($"💼 Processando: {job.Title}...");

            bool aplicou = ApplyToJob(job);

            if (aplicou)
            {
                Log($"✅ SUCESSO: {job.Title}");
                _feedbackRepo.Save(job.Url, Enums.ApplicationOutcome.Sent);
            }
            // Se falhou, o erro já foi logado dentro do ApplyToJob

            await Task.Delay(new Random().Next(3000, 6000)); 
        }
        
        Log("🏁 Finalizado.");
    }

    private bool ApplyToJob(Job job)
    {
        try
        {
            if (_driver == null) return false;

            _driver.Navigate().GoToUrl(job.Url);
            
            // Espera curta inicial para carregamento da página
            Thread.Sleep(2000);

            // --- 1. VERIFICAÇÃO DE SEGURANÇA: JÁ APLICADO? ---
            // Procura pelos textos que aparecem na sua print: "Candidatura enviada" ou "Status da candidatura"
            var alreadyApplied = _driver.FindElements(By.XPath(
                "//*[contains(text(), 'Candidatura enviada') or contains(text(), 'Application sent') or contains(@class, 'jobs-apply-button--disabled')]"
            ));

            if (alreadyApplied.Count > 0 && alreadyApplied.Any(e => e.Displayed))
            {
                Log($"⚠️ Vaga já aplicada anteriormente: {job.Title}");
                // IMPORTANTE: Salva no banco agora para o bot nunca mais abrir esse link
                _feedbackRepo.Save(job.Url, Enums.ApplicationOutcome.Sent);
                return false; 
            }

            // --- 2. BUSCA DO BOTÃO DE APLICAÇÃO ---
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            IWebElement? element = null;
            try 
            {
                element = wait.Until(d => 
                {
                    // Tenta achar o botão de aplicação
                    var candidates = d.FindElements(By.CssSelector(".jobs-apply-button"));
                    
                    if (!candidates.Any())
                    {
                        // Fallback por texto se a classe falhar
                        candidates = d.FindElements(By.XPath("//*[contains(text(), 'Candidatura simplificada') or contains(text(), 'Easy Apply')]"));
                    }

                    return candidates.FirstOrDefault(e => e.Displayed && e.Enabled);
                });
            }
            catch (WebDriverTimeoutException) 
            {
                Log($"⚠️ Botão não encontrado (Timeout) para: {job.Title}");
                // Se não achou botão e nem o aviso de "Já aplicado", pode ser erro de carga ou vaga expirada
                return false;
            }

            if (element == null) return false;
            
            // Scroll para garantir visibilidade
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Thread.Sleep(500);

            // Clique
            try {
                element.Click();
            }
            catch {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
            }
            
            Thread.Sleep(3000); 

            // Checa redirecionamento externo
            if (_driver.WindowHandles.Count > 1)
            {
                Log($"⏩ Site externo detectado. Pulando...");
                _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                _driver.Close();
                _driver.SwitchTo().Window(_driver.WindowHandles.First());
                return false;
            }

            return HandleFormLoop();
        }
        catch (Exception ex)
        {
            Log($"❌ Erro técnico ({job.Title}): {ex.Message}");
            return false;
        }
    }

    private bool HandleFormLoop()
    {
        for (int i = 0; i < 10; i++) 
        {
            try
            {
                FillSmartAnswers();

                // CORREÇÃO: Botão Enviar em PT/EN
                var submit = _driver!.FindElements(By.XPath(
                    "//button[contains(., 'Submit') or contains(., 'Enviar candidatura')]"
                ));
                
                if (submit.Count > 0)
                {
                    submit[0].Click();
                    Thread.Sleep(3000); // Espera enviar
                    
                    // Tenta fechar o modal de sucesso
                    try { 
                        _driver.FindElement(By.CssSelector("button[aria-label='Dismiss']")).Click(); 
                    } catch {}
                    
                    return true;
                }

                // CORREÇÃO: Botão Avançar em PT/EN
                var next = _driver.FindElements(By.XPath(
                    "//button[contains(., 'Next') or contains(., 'Avançar')]"
                ));
                
                if (next.Count > 0)
                {
                    next[0].Click();
                    Thread.Sleep(1000);
                    continue;
                }
                
                // CORREÇÃO: Botão Revisar em PT/EN
                var review = _driver.FindElements(By.XPath(
                    "//button[contains(., 'Review') or contains(., 'Revisar')]"
                ));
                
                if (review.Count > 0)
                {
                    review[0].Click();
                    Thread.Sleep(1000);
                    continue;
                }

                // Verifica erros
                var errors = _driver.FindElements(By.CssSelector(".artdeco-inline-feedback--error"));
                if(errors.Count > 0) return false;
            }
            catch { return false; }
            
            Thread.Sleep(500);
        }
        return false;
    }

    private void FillSmartAnswers()
    {
        try
        {
            var inputs = _driver!.FindElements(By.CssSelector("input[type='text'], input[type='number']"));
            foreach(var input in inputs)
            {
                if(!string.IsNullOrEmpty(input.GetAttribute("value"))) continue;
                
                string label = "";
                try { label = input.FindElement(By.XPath("./..")).Text.ToLower(); } catch {}

                foreach(var key in _smartAnswers.Keys)
                {
                    if(label.Contains(key))
                    {
                        input.SendKeys(_smartAnswers[key]);
                        break;
                    }
                }
            }
        }
        catch {}
    }

    public void Dispose()
    {
        try { _driver?.Quit(); _driver?.Dispose(); } catch {}
    }
}
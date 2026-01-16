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
    
    // Header importante para simular um browser real e receber o HTML completo
    http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,pt-BR;q=0.8");

    foreach (var q in queries)
    {
        // Adicionando filtro &f_AL=true para trazer apenas vagas "Easy Apply" (Candidatura Simplificada)
        var url = $"https://www.linkedin.com/jobs/search/?keywords={Uri.EscapeDataString(q)}&f_AL=true";
        
        try 
        {
            var html = await http.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Seleciona os cards de vaga
            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'base-card')]");
            if (nodes == null) continue;

            foreach (var n in nodes)
            {
                // Tenta extrair o título
                var titleNode = n.SelectSingleNode(".//span[contains(@class, 'base-search-card__title')]") 
                                ?? n.SelectSingleNode(".//h3");
                var title = titleNode?.InnerText.Trim() ?? "Unknown Title";

                // Tenta extrair a empresa
                var companyNode = n.SelectSingleNode(".//a[contains(@class, 'hidden-nested-link')]") 
                                  ?? n.SelectSingleNode(".//h4[contains(@class, 'base-search-card__subtitle')]");
                var company = companyNode?.InnerText.Trim() ?? "Unknown Company";

                // Pega o Link
                var linkNode = n.SelectSingleNode(".//a[contains(@class, 'base-card__full-link')]");
                var jobUrl = linkNode?.GetAttributeValue("href", "") ?? "";

                // Limpeza básica
                company = System.Net.WebUtility.HtmlDecode(company);
                title = System.Net.WebUtility.HtmlDecode(title);

                if (!string.IsNullOrEmpty(jobUrl))
                {
                    jobs.Add(new Job
                    {
                        Title = title,
                        Company = company, // Agora deve vir preenchido corretamente
                        Url = jobUrl,
                        Source = "LinkedIn",
                        // Se veio dessa busca com f_AL=true, é quase certeza que é Easy Apply
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao coletar query {q}: {ex.Message}");
        }
    }

    return jobs.GroupBy(j => j.Url).Select(g => g.First()).ToList();
}
}

using JobHunter.Models;

namespace JobHunter.Services;

public class CoverLetterGenerator
{
    public string[] Generate(Job job) => new[]
    {
        $"Olá, tenho forte experiência em .NET, IA e Unity. Interesse na vaga {job.Title}.",
        $"Sou desenvolvedor sênior .NET com foco em IA, MAUI e segurança. Vaga: {job.Title}.",
        $"Atuo com .NET, redes neurais e algoritmos genéticos. Gostaria de conversar sobre {job.Title}."
    };
}

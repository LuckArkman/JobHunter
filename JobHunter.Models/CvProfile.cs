namespace JobHunter.Models;

public class CvProfile
{
    public Dictionary<string, int> Skills { get; } = new()
    {
        { ".NET", 30 }, { "C#", 25 }, { "ASP.NET", 20 }, { "Blazor", 15 },
        { "Unity", 25 },
        { "Artificial Intelligence", 30 },
        { "Machine Learning", 25 },
        { "Neural", 35 },
        { "Genetic", 35 },
        { "MAUI", 20 },
        { "Security", 20 },
        { "LGPD", 15 }
    };
}

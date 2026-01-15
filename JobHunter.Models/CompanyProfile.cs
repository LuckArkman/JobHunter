namespace JobHunter.Models;

public class CompanyProfile
{
    public string Name { get; set; }
    public int Interviews { get; set; }
    public int Rejections { get; set; }
    public double Score => Interviews - (Rejections * 0.5);
}

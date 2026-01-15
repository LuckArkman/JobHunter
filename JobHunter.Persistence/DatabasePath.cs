using System.IO;

namespace JobHunter.Persistence;

public static class DatabasePath
{
    public static string Get(string fileName)
    {
        var root = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        var appFolder = Path.Combine(root, "JobHunter");

        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        return Path.Combine(appFolder, fileName);
    }
}
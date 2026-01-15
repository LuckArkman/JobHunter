using JobHunter.Models;

namespace JobHunter.Services;

public class TelegramNotifier
{
    private readonly string token;
    private readonly string chat;

    public TelegramNotifier(string token, string chat)
    {
        this.token = token;
        this.chat = chat;
    }

    public async Task Notify(Job job)
    {
        using var http = new HttpClient();
        var url = $"https://api.telegram.org/bot{token}/sendMessage";

        await http.PostAsync(url,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["chat_id"] = chat,
                ["text"] = $"{job.Title}\nScore: {job.Score}\n{job.Url}"
            }));
    }
}

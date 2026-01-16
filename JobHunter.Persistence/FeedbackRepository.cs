using JobHunter.Enums;
using JobHunter.Models;
using Microsoft.Data.Sqlite;

namespace JobHunter.Persistence;

public class FeedbackRepository
{
    private string Conn = "";

    public FeedbackRepository()
    {
        var dbPath = DatabasePath.Get("jobs.db");
        Conn = $"Data Source={dbPath}";
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        // Tabela para armazenar histórico
        cmd.CommandText =
            "CREATE TABLE IF NOT EXISTS Feedback (" +
            "JobUrl TEXT PRIMARY KEY, " +
            "Outcome INT, " +
            "Date TEXT)";
        cmd.ExecuteNonQuery();
    }

// Grava o resultado
    public void Save(string jobUrl, ApplicationOutcome outcome)
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "INSERT OR REPLACE INTO Feedback (JobUrl, Outcome, Date) " +
            "VALUES ($u, $o, $d)";

        cmd.Parameters.AddWithValue("$u", jobUrl);
        cmd.Parameters.AddWithValue("$o", (int)outcome);
        cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));

        cmd.ExecuteNonQuery();
    }

// --- NOVO MÉTODO: Verifica se já aplicou ---
    public bool HasApplied(string jobUrl)
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        // 0 = Sent (Enviado)
        cmd.CommandText = "SELECT COUNT(*) FROM Feedback WHERE JobUrl = $u AND Outcome = 0";
        cmd.Parameters.AddWithValue("$u", jobUrl);

        var count = Convert.ToInt64(cmd.ExecuteScalar());
        return count > 0;
    }

    public List<ApplicationFeedback> GetAll()
    {
        var list = new List<ApplicationFeedback>();

        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT JobUrl, Outcome, Date FROM Feedback ORDER BY Date DESC";

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new ApplicationFeedback
            {
                JobUrl = r.GetString(0),
                Outcome = (ApplicationOutcome)r.GetInt32(1),
                Date = DateTime.Parse(r.GetString(2))
            });
        }

        return list;
    }
}
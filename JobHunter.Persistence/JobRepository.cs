using JobHunter.Models;
using Microsoft.Data.Sqlite;

namespace JobHunter.Persistence;

public class JobRepository
{
    private const string Conn = "Data Source=data/jobs.db";

    public JobRepository()
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "CREATE TABLE IF NOT EXISTS Jobs (" +
            "Url TEXT PRIMARY KEY, " +
            "Title TEXT, " +
            "Score INT)";
        cmd.ExecuteNonQuery();
    }

    // CREATE
    public void Save(Job job)
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "INSERT OR IGNORE INTO Jobs (Url, Title, Score) " +
            "VALUES ($u, $t, $s)";

        cmd.Parameters.AddWithValue("$u", job.Url);
        cmd.Parameters.AddWithValue("$t", job.Title);
        cmd.Parameters.AddWithValue("$s", job.Score);

        cmd.ExecuteNonQuery();
    }

    // READ (Dashboard usa isso)
    public List<Job> GetAll()
    {
        var jobs = new List<Job>();

        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Url, Title, Score FROM Jobs";

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            jobs.Add(new Job
            {
                Url = r.GetString(0),
                Title = r.GetString(1),
                Score = r.GetInt32(2)
            });
        }

        return jobs;
    }

    // UPDATE (feedback / aprendizado)
    public void Update(Job job)
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "UPDATE Jobs SET " +
            "Title = $t, " +
            "Score = $s " +
            "WHERE Url = $u";

        cmd.Parameters.AddWithValue("$u", job.Url);
        cmd.Parameters.AddWithValue("$t", job.Title);
        cmd.Parameters.AddWithValue("$s", job.Score);

        cmd.ExecuteNonQuery();
    }

    // READ por URL (opcional, mas útil)
    public Job? GetByUrl(string url)
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "SELECT Url, Title, Score FROM Jobs WHERE Url = $u";
        cmd.Parameters.AddWithValue("$u", url);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return new Job
        {
            Url = r.GetString(0),
            Title = r.GetString(1),
            Score = r.GetInt32(2)
        };
    }
}

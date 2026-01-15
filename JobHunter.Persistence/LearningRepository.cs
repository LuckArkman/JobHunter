using JobHunter.Models;
using Microsoft.Data.Sqlite;

namespace JobHunter.Persistence;

public class LearningRepository
{
    private const string Conn = "Data Source=data/learning.db";

    public LearningRepository()
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "CREATE TABLE IF NOT EXISTS LearningWeights (" +
            "Key TEXT PRIMARY KEY, " +
            "Weight REAL)";
        cmd.ExecuteNonQuery();
    }

    public double GetWeight(string key)
    {
        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "SELECT Weight FROM LearningWeights WHERE Key = $k";
        cmd.Parameters.AddWithValue("$k", key);

        var result = cmd.ExecuteScalar();
        return result == null ? 0.0 : Convert.ToDouble(result);
    }

    public void UpdateWeight(string key, double delta)
    {
        var current = GetWeight(key);
        var updated = current + delta;

        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText =
            "INSERT INTO LearningWeights (Key, Weight) VALUES ($k, $w) " +
            "ON CONFLICT(Key) DO UPDATE SET Weight = $w";

        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$w", updated);

        cmd.ExecuteNonQuery();
    }

    public List<LearningWeight> GetAll()
    {
        var list = new List<LearningWeight>();

        using var c = new SqliteConnection(Conn);
        c.Open();

        var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Key, Weight FROM LearningWeights";

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new LearningWeight
            {
                Key = r.GetString(0),
                Weight = r.GetDouble(1)
            });
        }

        return list;
    }
}

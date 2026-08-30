using MySqlConnector;

namespace MediStock.Tests;

// ============================================================
// TestDatabase
//  Provisions a throwaway 'medistock_test' database by replaying
//  every migration in /database in order, then seeding a
//  super-admin + admin user for the test suite.
// ============================================================
public static class TestDatabase
{
    public const string PharmacyId = "9001";
    public const string AdminEmail = "admin@test.co";
    public const string SuperEmail = "super@test.co";
    public const string Password = "Test@1234";

    public static string ServerConn
        => $"Server={Env("MEDISTOCK_TEST_SERVER", "164.92.97.131")};" +
           $"User ID={Env("MEDISTOCK_TEST_USER", "RizikiDev")};" +
           $"Password={Env("MEDISTOCK_TEST_PASSWORD", "Master@047")};" +
           "SslMode=None;AllowPublicKeyRetrieval=True";

    private static string Env(string name, string fallback)
        => Environment.GetEnvironmentVariable(name) ?? fallback;
    public static string TestConnectionString
        => ServerConn + ";Database=medistock_test";
    private static string ScriptConnectionString
        => TestConnectionString + ";Allow User Variables=True";

    private static readonly object _lock = new();

    public static void Provision()
    {
        lock (_lock)
        {
            using var client = new MySqlConnection(ServerConn);
            client.Open();

            using (var drop = new MySqlCommand("DROP DATABASE IF EXISTS medistock_test", client))
                drop.ExecuteNonQuery();
            using (var create = new MySqlCommand("CREATE DATABASE medistock_test CHARACTER SET utf8mb4", client))
                create.ExecuteNonQuery();
            client.Close();

            var migrations = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "database"));
            foreach (var file in Directory.GetFiles(migrations, "*.sql")
                         .OrderBy(f => int.Parse(Path.GetFileName(f).Split('_')[0]), Comparer<int>.Default))
            {
                RunScript(ScriptConnectionString, File.ReadAllText(file));
            }

            Seed();
        }
    }

    private static void RunScript(string conn, string script)
    {
        using var connection = new MySqlConnection(conn);
        connection.Open();
        foreach (var statement in SplitScript(script))
        {
            using var cmd = new MySqlCommand(statement, connection);
            cmd.CommandTimeout = 120;
            cmd.ExecuteNonQuery();
        }
    }

    private static IEnumerable<string> SplitScript(string script)
    {
        string delim = ";";
        string current = "";

        foreach (var raw in script.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            var trimmed = line.Trim();

            // A USE statement would redirect the connection to the live DB — drop it.
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^USE\s+\w+;?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                continue;

            if (trimmed.StartsWith("DELIMITER ", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(current)) yield return current;
                current = "";
                delim = trimmed["DELIMITER ".Length..].Trim();
                continue;
            }

            if (current.Length > 0) current += "\n";
            current += line;

            string trimmedCurrent = current.TrimEnd();
            if (trimmedCurrent.EndsWith(delim, StringComparison.Ordinal))
            {
                yield return trimmedCurrent[..^delim.Length].Trim();
                current = "";
            }
        }

        if (!string.IsNullOrWhiteSpace(current)) yield return current;
    }

    private static void Seed()
    {
        string adminHash = BCrypt.Net.BCrypt.HashPassword(Password);
        string superHash = adminHash;

        using var conn = new MySqlConnection(TestConnectionString);
        conn.Open();

        using (var ph = new MySqlCommand(
            "INSERT INTO pharmacies (id, name, slug, phone, email, address, license_number, currency, is_active, is_deleted, created_on) " +
            "VALUES (9001, 'Test Pharmacy', 'test-pharmacy', '0700000000', 'pharmacy@test.co', 'Test Address', 'TST-0001', 'KES', 1, 0, NOW())", conn))
            ph.ExecuteNonQuery();

        using (var u1 = new MySqlCommand(
            "INSERT INTO portal_users (id, pharmacy_id, role_id, first_name, last_name, email, mobile, password, is_deleted, created_on) " +
            "VALUES (9001, 9001, 2, 'Admin', 'User', @e1, '0700000001', @p1, 0, NOW())", conn))
        {
            u1.Parameters.AddWithValue("@e1", AdminEmail);
            u1.Parameters.AddWithValue("@p1", adminHash);
            u1.ExecuteNonQuery();
        }

        using (var u2 = new MySqlCommand(
            "INSERT INTO portal_users (id, pharmacy_id, role_id, first_name, last_name, email, mobile, password, is_deleted, created_on) " +
            "VALUES (9002, 9001, 1, 'Super', 'Admin', @e2, '0700000002', @p2, 0, NOW())", conn))
        {
            u2.Parameters.AddWithValue("@e2", SuperEmail);
            u2.Parameters.AddWithValue("@p2", superHash);
            u2.ExecuteNonQuery();
        }
    }

    public static void Cleanup(params string[] sql)
    {
        using var conn = new MySqlConnection(TestConnectionString);
        conn.Open();
        foreach (var s in sql)
        {
            if (string.IsNullOrWhiteSpace(s)) continue;
            using var cmd = new MySqlCommand(s, conn);
            cmd.ExecuteNonQuery();
        }
    }

    public static object? Scalar(string sql)
    {
        using var conn = new MySqlConnection(TestConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(sql, conn);
        return cmd.ExecuteScalar();
    }
}
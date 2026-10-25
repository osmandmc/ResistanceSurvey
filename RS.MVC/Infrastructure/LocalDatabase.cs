using System;
using Microsoft.Data.SqlClient;

namespace RS.MVC.Infrastructure
{
    /// <summary>Whether a connection string points at a database on this machine (F11).</summary>
    public static class LocalDatabase
    {
        public static bool IsLocal(string connectionString)
        {
            string server;
            try
            {
                server = new SqlConnectionStringBuilder(connectionString).DataSource ?? "";
            }
            catch (ArgumentException)
            {
                return false;
            }
            var host = server.Replace("tcp:", "", StringComparison.OrdinalIgnoreCase).Split(',', '\\')[0].Trim().ToLowerInvariant();
            return host is "localhost" or "127.0.0.1" or "::1" or "." or "(local)" or "(localdb)";
        }
    }
}

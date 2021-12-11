using System.Data;

namespace backend.Utilitis
{
    public static class ConnectionsData
    {
        public static string GetConectionString(string database)
        {
            string host = "194.150.103.18";
            string username = "postgres";
            string password = "!Malinka@pass#database";

            return $"Host={host};Username={username};Password={password};Database={database}";
        }
    }
}
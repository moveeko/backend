using backend.Utilities;
using backend.Utilitis;
using Npgsql;

namespace backend.Companies
{
    public class Company
    {
        public string? CompanyId;
        public string CompanyName;
        public string? CompanyEmail;
        public User[]? CompanyUsers;

        public List<int> workers;

        public Company(string? id, string? companyEmail, string? name){
            CompanyId = id;
            CompanyEmail = companyEmail;
            CompanyName = name;
        }

        public Company(string? id, string? companyEmail, string companyName, List<int> ids)
        {
            CompanyId = id;
            CompanyEmail = companyEmail;
            CompanyName = companyName;
            workers = ids;
        }

        public async Task<bool> AddWorkers(int userid)
        {
            string sql = $"INSERT INTO company_{CompanyId}.workers VALUES({userid});";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            Company company;

            await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
            {
                await con.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await con.CloseAsync();
            }
            
            await con.CloseAsync();
            return true;
        }
    }
    
    private static async Task<bool> IsUserInCompany(int id)
    {
        string sql = $"SELECT * FROM base.company WHERE idtoken = {id};";
        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
        bool hasRows;
        using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
        {
            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            hasRows = reader.HasRows;

            await con.CloseAsync();
        }
        return !hasRows;

    }
}


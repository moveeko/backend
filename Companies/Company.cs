using backend.Utilitis;
using Npgsql;

namespace backend.Companies;

public class Company
{
    public string? CompanyId;
    public string CompanyName;
    public string? CompanyEmail;

    public List<int> workers;

    public Company(string? id, string? companyEmail)
    {
        CompanyId = id;
        CompanyEmail = companyEmail;
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
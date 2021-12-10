using System.Text;
using System.Xml.Serialization;
using backend.UserManager;
using backend.Utilities;
using backend.Utilitis;
using Npgsql;

namespace backend.Companies;

public static class CompaniesMethod
{
    public static async Task<object> IsCompanyExist(string? id)
    {
        string sql = $"SELECT * FROM base.base WHERE id = {id};";

        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
        NpgsqlCommand command = new NpgsqlCommand(sql, con);

        await con.OpenAsync();
        NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            
        var result = reader.HasRows;

        await con.CloseAsync();

        if (result)
        {
            return GetCompany(id,  false).Result;
        }
        else
        {
            throw new CustomError("UserNotExist");
        }
    }
    public static async Task<object> CreateCompany(string? name, string? email, string? password)
    {
        string? id = CreateCompanyMethod.GenerateId().Result;
        Company company = new Company(id, email);
            
        if (!ValidateLogin(name,email, password))
        {
            throw new CustomError("InvalidLogin");
        }
        if (CreateCompanyMethod.IsLoginOrEmailExist(company).Result)
        {
            throw new CustomError("UserIsExist");
        }

        await CreateCompanyMethod.CreateDataBase(company, password);

        return company;
    }
    private static async Task<string> GetUserPassword(int id)
    {
        string? password = string.Empty;
        string sql = $"SELECT password, login FROM user_{id}.user;";

        NpgsqlConnection con = new(ConnectionsData.GetConectionString("moveeko"));
        await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
        {
            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();
                password = reader["password"].ToString();
            }

            await con.CloseAsync();
        }
            
        if (password != null) return Encoding.UTF8.GetString(Convert.FromBase64String(password));
        else
        {
            return "";
        }
    }
    public static async Task<Company> GetCompany(string? id, bool reqCheckId)
    {
        if (reqCheckId)
        {
            var name = "";
            try
            {
                await IsCompanyExist(id);
            }
            catch (CustomError cf)
            {
                name = cf.Name;
            }

            throw new CustomError(name);
        }

        string sql = $"SELECT * FROM company_{id}.data;";

        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
        Company company;

        await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
        {
            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();
            string companyid = reader.GetString(0);
            string name = reader.GetString(1);
            string email = reader.GetString(3);
            string avatar = reader.GetString(4);

            company = new Company(id, email, name, ReturnWorkers(companyid).Result);
        }
            
        await con.CloseAsync();

        return company;
    }
    public async static Task<List<int>> ReturnWorkers(string id)
    {
        string sql = $"SELECT * FROM company_{id}.workers;";

        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));

        List<int> workers = new List<int>();
        
        await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
        {
            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                workers.Add((int)reader["id"]);
            }

            await con.CloseAsync();
        }

        return workers;
    }
    public static bool ValidateLogin(string? name, string? email, string? password)
    {
        if (password.Length <= 5 || password.Length >= 30)
            return false;
        if (!password.Any(char.IsUpper))
            return false;
        if (!email.Contains('@'))
            return false;
        if (!email.Contains('.'))
            return false;
        if (!password.Any(char.IsLower))
            return false;

        return true;
    }

    public async static Task<List<Company>> GetAllCompany()
    {
        List<Company> companies = new List<Company>();
        string sql = $"SELECT * FROM base.company;";

        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
        NpgsqlCommand command = new NpgsqlCommand(sql, con);

        await con.OpenAsync();
        NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        var result = reader.HasRows;

        await con.CloseAsync();

        while (reader.Read())
        {
            companies.Add(GetCompany(reader.GetString(0), false).Result);
        }

        return companies;
    }

    private static class CreateCompanyMethod
        {
            private static async Task<string?> CreateIdToken()
            {
                Random rnd = new Random();

                List<char> chars = new List<char>();
                const int lengthToken = 6;

                for (int i = 65; i != 90; i++)
                {
                    chars.Add((char)i);
                }
                
                Random random = new Random();
                string? IdToken= "";

                for (int i = 0; i != lengthToken; i++)
                {
                    IdToken += chars[random.Next(0, chars.Count - 1)];
                }

                return IdToken;
            }
            public static async Task<string?> GenerateId()
            {
                NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));

                string? IdToken = CreateIdToken().Result;
                
                string sql = $"SELECT * FROM base.base WHERE idtoken = '{IdToken}';";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    await con.OpenAsync();

                    NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        IdToken = CreateIdToken().Result;
                        int idFromData = int.Parse(reader["id"].ToString() ?? string.Empty);
                        if (IdToken != idFromData.ToString())
                        {
                            break;
                        }
                    }

                    await con.CloseAsync();
                    return IdToken;
                }
            }
            public static async Task CreateDataBase(Company company, string? password)
            {
                string schemaName = "company_" + company.CompanyId;

                if (password != null)
                {
                    NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
                    await con.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand();
                    command.Connection = con;

                    string sql = $"create schema {schemaName}; alter schema {schemaName} owner to postgres;";
                    command.CommandText = sql;

                    await command.ExecuteNonQueryAsync();
                    
                    sql = $"create table company_{company.CompanyId}.data(" +
                          $"idtoken string," +
                          $"name varchar(255)," +
                          $"email varchar(255)," +
                          $"password varchar(255)," +
                          $"avatar varchar(1048576));";
                    command.CommandText = sql;
                    await command.ExecuteNonQueryAsync();

                    sql = $"create table company_{company.CompanyId}.workers(int id);";
                    command.CommandText = sql;
                    await command.ExecuteNonQueryAsync();
                    
                    
                    command.CommandText =
                        $"Insert into base.company (id, email)  VALUES({company.CompanyId},'{company.CompanyEmail}');";
                    await command.ExecuteNonQueryAsync();
                    
                    command.CommandText =
                        $"Insert into company_{company.CompanyId}.data (id, name, email, email, password, avatar)" +
                        $" VALUES({company.CompanyId},'{company.CompanyName}','{company.CompanyEmail}', '{Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}', '{"defult"}');";
                    await command.ExecuteNonQueryAsync();
                    
                    await con.CloseAsync();
                }
            }
            public static async Task<bool> IsLoginOrEmailExist(Company company)
            {
                string sql = $"SELECT * FROM base.company WHERE email = '{company.CompanyEmail}';";
                NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    await con.OpenAsync();
                    NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                    bool hasRows = reader.HasRows;

                    await con.CloseAsync();
                    return hasRows;
                }
            }
        }
}
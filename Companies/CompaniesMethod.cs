using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using backend.Companies;
using backend.UserManager;
using backend.Utilities;
using backend.Utilitis;
using Npgsql;

namespace backend.Companies
{
    public static class CompaniesMethod
    {
        public static async Task<object> IsCompanyExist(string? id)
        {
            string sql = $"SELECT * FROM base.company WHERE idtoken = '{id}';";

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
                throw new CustomError("CompanyNotExist");
            }
        }
        public static async Task<object> CreateCompany(string? name, string? email, string? password)
        {
            string? id = CreateCompanyMethod.GenerateId().Result;
            Company company = new Company(id, email, name);
                
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
        private static async Task<string> GetCompanyPassword(string id)
        {
            string? password = string.Empty;
            string sql = $"SELECT password FROM company_{id}.data;";

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
        public static async Task<Company> GetCompany(string? id, bool reqCheckId = true)
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
                    throw new CustomError(name);
                }
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
                string email = reader.GetString(2);
                string avatar = reader.GetString(4);

                company = new Company(id, email, name, avatar, ReturnWorkers(companyid).Result);

                company.maxusers = reader.GetInt32(5);
            }

            
            company.companyPointsAvg = await company.CalculatePoints(true);
            company.companyPointsSum = await company.CalculatePoints(false);
                
            await con.CloseAsync();

            return company;
        }
        public static async Task<List<int>> ReturnWorkers(string id)
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
        public static async Task<List<Company>> GetAllCompany()
        {
            List<Company> companies = new List<Company>();
            string sql = $"SELECT * FROM base.company;";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand(sql, con);

            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                companies.Add(GetCompany(reader.GetString(0), false).Result);
            }
            
            await con.CloseAsync();

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
                    
                    string sql = $"SELECT * FROM base.company WHERE idtoken = '{IdToken}';";

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
                              $"idtoken varchar(255)," +
                              $"name varchar(255)," +
                              $"email varchar(255)," +
                              $"password varchar(255)," +
                              $"avatar varchar(1048576)," +
                              $"maxusers int);";
                        command.CommandText = sql;
                        await command.ExecuteNonQueryAsync();

                        sql = $"create table company_{company.CompanyId}.workers(id int);";
                        command.CommandText = sql;
                        await command.ExecuteNonQueryAsync();
                        
                        
                        command.CommandText =
                            $"Insert into base.company (idtoken, email)  VALUES('{company.CompanyId}','{company.CompanyEmail}');";
                        await command.ExecuteNonQueryAsync();

                        string avatar =
                            "data:image/png;base64,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";
                        

                        command.CommandText =
                            $"Insert into company_{company.CompanyId}.data (idtoken, name, email, password, avatar, maxusers)" +
                            $" VALUES('{company.CompanyId}','{company.CompanyName}','{company.CompanyEmail}', '{Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}', '{avatar}', 0);";
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
        public static async Task<object> Login(string? email, string? password)
        {
            string sql = $"SELECT idtoken FROM base.company WHERE email = '{email}';";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand(sql, con);

            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            string id;
            if (reader.HasRows)
            {
                await reader.ReadAsync();
                id = reader.GetString(0);
            }
            else
            {
                throw new CustomError("InvalidLogin");
            }

            await con.CloseAsync();

            if (password != GetCompanyPassword(id).Result)
            {
                throw new CustomError("InvalidLogin");
            }

            Company company = GetCompany(id, false).Result;

            return company;
        }
    }
}
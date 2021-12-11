using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using backend.UserManager;
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
        public string? CompanyAvatar;
        public User[]? CompanyUsers;

        public int companyPointsSum;
        public int companyPointsAvg;

        public List<int> workers;

        public int maxusers;

        public Company(string? id, string? companyEmail, string? name)
        {
            CompanyId = id;
            CompanyEmail = companyEmail;
            CompanyName = name;
        }

        public Company(string? id, string? companyEmail, string companyName,string companyAvatar, List<int> ids)
        {
            CompanyId = id;
            CompanyEmail = companyEmail;
            CompanyName = companyName;
            CompanyAvatar = companyAvatar;
            workers = ids;
        }

        public async Task<bool> DeleteWorker(int userid)
        {
            string sql = $"DELETE FROM company_{CompanyId}.workers WHERE id = {userid});";

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
        
        public async Task<bool> AddWorkers(int userid)
        {
            string sql = $"INSERT INTO company_{CompanyId}.workers VALUES({userid});";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            Company company;

            await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
            {
                await con.OpenAsync();
                await command.ExecuteNonQueryAsync();
                
                command.CommandText = $"UPDATE user_{userid}.user SET companytoken = '{CompanyId}';";
                await command.ExecuteNonQueryAsync();

                await con.CloseAsync();
            }

            return true;
        }
        
        public async Task<bool> RemoveWorkers(int userid)
        {
            string sql = $"DELETE FROM company_{CompanyId}.workers WHERE id = {userid};";

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
        
        public async Task<List<User>> ReturnWorkers()
        {
            string sql = $"SELECT * FROM company_{CompanyId}.workers;";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));

            List<User> workers = new List<User>();

            await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
            {
                await con.OpenAsync();
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    workers.Add(UserMethod.GetUserData(reader.GetInt32(0), false).Result);
                }
                await con.CloseAsync();
            }

            await con.CloseAsync();
            
            return workers;
        }  

        private static async Task<bool> IsUserInCompany(int id)
        {
            string sql = $"SELECT * FROM base.company WHERE idtoken = {id};";
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            bool hasRows;
            await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
            {
                await con.OpenAsync();
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                hasRows = reader.HasRows;

                await con.CloseAsync();
            }

            return !hasRows;

        }
        
        public async Task<bool> SetNewEmail(string? newEmail) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE company_{this.CompanyId}.data SET email = '{newEmail}';";
            await command.ExecuteNonQueryAsync();
            
            command.CommandText =
                $"UPDATE base.company SET email = '{newEmail}' WHERE id = {this.CompanyId};";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        public async Task<bool> SetNewAvatar(string? newAvatar) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE company_{this.CompanyId}.data SET avatar = '{newAvatar}';";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        public async Task<bool> SetNewPassword(string? newPassword) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            if (newPassword != null)
                command.CommandText =
                    $"UPDATE company_{this.CompanyId}.data SET password = '{Convert.ToBase64String(Encoding.UTF8.GetBytes(newPassword))}';";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        public async Task<int> CalculatePoints(bool AVG)
        {
            var list = await this.ReturnWorkers();

            int sum = 0;
            int i = 0;

            foreach (var item in list)
            {
                sum += item.points;
                i++;
            }

            if (AVG)
            {
                if (sum == 0) return 0;
                return sum / i;
            }
            else
            {
                return sum;
            }
        }

        public async Task<bool> ChoosePlan(int maxusers)
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE company_{this.CompanyId}.data SET maxusers = {maxusers};";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        
    }
}


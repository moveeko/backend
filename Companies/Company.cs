using System.Text;
using backend.User;
using backend.Utilities;
using Npgsql;

namespace backend.Companies
{
    public class Company
    {
        private static readonly string DataBaseName = "moveeko"; 

        public string? CompanyId;
        public string? CompanyName;
        public string? CompanyEmail;
        public string? CompanyAvatar;
        public User.User[]? CompanyUsers;

        public int CompanyPointsSum;
        public int CompanyPointsAvg;

        public List<int>? Workers;
        public List<int>? WorkersIds;

        public int Maxusers;
        
        public Company(string? id, string? companyEmail, string? companyName,string companyAvatar, List<int>? ids, List<int>? workers)
        {
            CompanyId = id;
            CompanyEmail = companyEmail; 
            CompanyName = companyName;
            CompanyAvatar = companyAvatar;
            Workers = ids;
            WorkersIds = workers ?? throw new ArgumentNullException(nameof(workers));
        }

        public Company(string? id, string? companyEmail, string? name, string companyAvatar, List<int>? workers)
        {
            CompanyId = id;
            CompanyEmail = companyEmail;
            CompanyName = name;
            CompanyAvatar = companyAvatar;
            Workers = workers;
        }

        public Company(string? id, string? companyEmail, string? companyName)
        {
            CompanyId = id;
            CompanyEmail = companyEmail;
            CompanyName = companyName;
        }

        public async Task<bool> DeleteWorker(int userid)
        {
            string sql = $"DELETE FROM company_{CompanyId}.workers WHERE id = {userid});";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));

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
            if (await IsUserInCompany(userid)) throw new CustomError("User is Exist in Company");
            
            string sql = $"INSERT INTO company_{CompanyId}.workers VALUES({userid});";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));

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

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));

            await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
            {
                await con.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await con.CloseAsync();
            }

            await con.CloseAsync();
            return true;
        }
        
        public async Task<List<User.User>> ReturnWorkers()
        {
            string sql = $"SELECT * FROM company_{CompanyId}.workers;";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));

            List<User.User> workers = new List<User.User>();

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

        //Todo: Dodac do joinUser
        private static async Task<bool> IsUserInCompany(int id)
        {
            string sql = $"SELECT * FROM base.company WHERE idtoken = {id};";
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
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
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
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
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
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
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
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
        public async Task<int> CalculatePoints(bool avg)
        {
            var list = await this.ReturnWorkers();

            int sum = 0;
            int i = 0;

            foreach (var item in list)
            {
                sum += item.Points;
                i++;
            }

            if (avg)
            {
                if (i == 0) return 0;
                return sum / i;
            }
            else
            {
                return sum;
            }
        }

        public async Task<bool> ChoosePlan(int usercount)
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE company_{this.CompanyId}.data SET usercount = {usercount};";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using backend.Utilities;
using Npgsql;
using backend.Utilitis;

namespace backend.UserManager
{
    public static class UserMethod
    {
        public static async Task<object> IsUserExist(int id)
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
                return GetUserData(id,  false).Result;
            }
            else
            {
                throw new CustomError("UserNotExist");
            }
        }
        public static async Task<object> Login(string? email, string? password)
        {
            string sql = $"SELECT id FROM base.base WHERE email = '{email}';";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand(sql, con);

            await con.OpenAsync();
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            int id;
            if (reader.HasRows)
            {
                await reader.ReadAsync();
                id = reader.GetInt32(0);
            }
            else
            {
                await con.CloseAsync();

                throw new CustomError("InvalidLogin");
            }

            await con.CloseAsync();

            if (password != GetUserPassword(id).Result)
            {
                var result = GetUserPassword(id).Result;
                throw new CustomError("InvalidLogin");
            }

            User user = GetUserData(id, false).Result;

            return user;
        }
        public static async Task<User> GetUserData(int id, bool reqCheckId = true)
        {
            if (reqCheckId)
            {
                var name = "";
                try
                {
                    await IsUserExist(id);
                }
                catch (CustomError cf)
                {
                    name = cf.Name;
                    throw new CustomError(name);
                }
            }

            string sql = $"SELECT * FROM user_{id}.user;";

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            User user;

            await using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
            {
                await con.OpenAsync();
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                await reader.ReadAsync();
                string firstName = reader.GetString(1);
                string lastName = reader.GetString(2);
                string email = reader.GetString(3);
                string avatar = reader.GetString(4);

                user = new User(
                    id,
                    firstName,
                    lastName,
                    email, avatar);
            }
            
            await con.CloseAsync();

            return user;
        }
        private static async Task<string> GetUserPassword(int id)
        {
            string? password = string.Empty;
            string sql = $"SELECT password, email FROM user_{id}.user;";

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
        public static async Task<User> CreateUser(string? firstName, string? lastName, string? email, string? password)
        {
            int id = UserCreateUserMethods.GenerateId().Result;
            User user = new (id, firstName, lastName, email);
            
            if (!UserCreateUserMethods.ValidateLogin(firstName,lastName,email, password))
            {
                throw new CustomError("InvalidLogin");
            }
            if (UserCreateUserMethods.IsLoginOrEmailExist(user).Result)
            {
                throw new CustomError("UserIsExist");
            }

            await UserCreateUserMethods.CreateDataBase(user, password);

            return user;
        }
        
        private static class UserCreateUserMethods
        {
            public static async Task<int> GenerateId()
            {
                NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
                Random rnd = new Random();

                int id = rnd.Next(100000, 999999);
                string sql = $"SELECT * FROM base.base WHERE id = {id};";

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con))
                {
                    await con.OpenAsync();

                    NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        id = rnd.Next(100000, 999999);
                        int idFromData = int.Parse(reader["id"].ToString() ?? string.Empty);
                        if (id != idFromData)
                        {
                            break;
                        }
                    }

                    await con.CloseAsync();
                    return id;
                }
            }
            public static async Task CreateDataBase(User user, string? password)
            {
                string schemaName = "user_" + user.Id.ToString();

                if (password != null)
                {
                    NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
                    await con.OpenAsync();
                    NpgsqlCommand command = new NpgsqlCommand();
                    command.Connection = con;

                    string sql = $"create schema {schemaName}; alter schema {schemaName} owner to postgres;";
                    command.CommandText = sql;

                    await command.ExecuteNonQueryAsync();


                    sql = $"create table user_{user.Id}.user(" +
                          $"id int," +
                          $"firstName varchar(255)," +
                          $"lastName varchar(255)," +
                          $"email varchar(255)," +
                          $"password varchar(255)," +
                          $"avatar varchar(1048576)," +
                          $"companyToken varchar(255));";
                    command.CommandText = sql;
                    await command.ExecuteNonQueryAsync();

                    sql = $"create table user_{user.Id}.activity(" +
                          $"data varchar(255)," +
                          $"type int," +
                          $"index serial);";
                    command.CommandText = sql;
                    await command.ExecuteNonQueryAsync();
                    
                    
                    command.CommandText =
                        $"Insert into base.base (id, email)  VALUES({user.Id},'{user.Email}');";
                    await command.ExecuteNonQueryAsync();
                    
                    string baseImage =
                        "data:image/png;base64,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";

                    command.CommandText =
                        $"Insert into user_{user.Id}.user (id, firstName, lastName, email, password, avatar, companyToken)" +
                        $" VALUES({user.Id},'{user.FirstName}','{user.LastName}','{user.Email}','{Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}', '{baseImage}', '0');";
                    await command.ExecuteNonQueryAsync();
                    
                    await con.CloseAsync();
                }
            }
            public static async Task<bool> IsLoginOrEmailExist(User user)
            {
                string sql = $"SELECT * FROM base.base WHERE email = '{user.Email}';";
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
            public static bool ValidateLogin(string firstName, string lastName, string email, string password)
            {
                if ((password.Length <= 5 || password.Length >= 30))
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
        }
        private static class LoginMethod
        {
            public static string GenerateToken(User user)
            {
                List<char> chars = new List<char>();
                const int lengthToken = 6;

                for (int i = 65; i != 90; i++)
                {
                    chars.Add((char)i);
                }
                
                Random random = new Random();
                string token = "";

                for (int i = 0; i != lengthToken; i++)
                {
                    token += chars[random.Next(0, chars.Count - 1)];
                }
                
                NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
                NpgsqlCommand command = new NpgsqlCommand();

                command.CommandText =
                    $"Insert into user_{user.Id}.tokens VALUES('{token}', true);";
                command.Connection = con;

                con.Open();
                command.ExecuteNonQuery();
                con.Close();
                
                return token;
                
                
            }
        }
    }
}
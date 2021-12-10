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
                          $"avatar varchar(1048576));";
                    command.CommandText = sql;
                    await command.ExecuteNonQueryAsync();

                    sql = $"create table user_{user.Id}.tokens(" +
                          $"token varchar(6)," +
                          $"isActivated bool);";
                    command.CommandText = sql;
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = $"create table user_{user.Id}.friends(" +
                                          "recipient int," +
                                          "type int);";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = $"create table user_{user.Id}.activity(" +
                                          "day varchar(255)," +
                                          "steps int);";
                    await command.ExecuteNonQueryAsync();
                    
                    command.CommandText =
                        $"Insert into base.base (id, email)  VALUES({user.Id},'{user.Email}');";
                    await command.ExecuteNonQueryAsync();
                    
                    command.CommandText =
                        $"Insert into user_{user.Id}.user (id, firstName, lastName, email, password, avatar)" +
                        $" VALUES({user.Id},'{user.FirstName}','{user.LastName}','{user.Email}','{Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}', '{"defult"}');";
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
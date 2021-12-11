using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using backend.Utilities;
using Npgsql;
using walkwards_api.Structure;
using walkwards_api.Utilitis;

namespace backend.UserManager
{
    public class User
    {        
        public User(int id, string? firstName, string? lastName, string? email, string token, string? avatar = "defult", bool? isActivated = false, int accoutPrivacy = 0)
        {
            Id = id;
            FirstName = firstName;
            LastName = firstName;
            Email = email;
            Token = token;
            Avatar = avatar == null ? "None" : avatar;
            IsActivated = isActivated == null ? true : (bool)isActivated;
        }

        public User(int id, string? firstName, string lastName, string? email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;            
            Email = email;
        }
        //propertis
        public int Id { get ; set; }
        public string? FirstName { get ; set; }
        public string? LastName { get ; set; }
        public string? Email { get ; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int PlaceInRanging { get ; set; }
        public bool IsActivated { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? Avatar { get; set; }
        
        public string? Token;
        
        //other
        public async Task<bool> ChcekToken(string token)
        {
            //token for postman
            if(token == "2ykrohna")
            {
                return true;
            }

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();

            try
            {
                command.CommandText = $"SELECT * FROM user_{this.Id}.tokens WHERE token = '{token}';";
                NpgsqlDataReader reader = command.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    await con.CloseAsync();
                    return true;
                }
                else
                {
                    await con.CloseAsync();
                    return false;
                }
            }
            catch (Exception)
            {
                throw new CustomError("InvalidToken");
            }
        }
        //friends
        public async Task<object> AddFriend(int recipient) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();

            //
            command.CommandText = $"SELECT * FROM user_{this.Id}.friends WHERE recipient = {recipient};";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                throw new CustomError("RequestHasAlreadyBeenSent");
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            try
            {
                await UserMethod.IsUserExist(recipient);
            }
            catch (CustomError ce)
            {
                throw new CustomError(ce.Name);
            }
            await con.CloseAsync();
            await con.OpenAsync();

            //Add to sender data
            command.CommandText =
                $"INSERT INTO user_{this.Id}.friends VALUES({recipient}, {(int) FriendType.Sent});";
            await command.ExecuteNonQueryAsync();

            //Add to recipient data
            command.CommandText =
                $"INSERT INTO user_{recipient}.friends VALUES({this.Id}, {(int) FriendType.Request});";
            await command.ExecuteNonQueryAsync();

            await con.CloseAsync();
            return true;
        }

        public async Task<List<User>> GetAllFriend() //Async 
        {
            List<User> friends = new List<User>();

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            
            //
            command.CommandText = $"SELECT * FROM user_{this.Id}.friends;";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                friends.Add(UserMethod.GetUserData(reader.GetInt32(0)).Result);
            }
            await con.CloseAsync();

            return friends;
        }
        public async Task<FriendType> GetFriendShip(int recipient) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;

            await con.OpenAsync();

            try
            {
                await UserMethod.IsUserExist(recipient);
                command.CommandText = $"SELECT * FROM user_{this.Id}.friends WHERE recipient = {recipient};";
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                reader.Read();

                bool hasRows = reader.HasRows;


                if (hasRows)
                {
                    int friendType = reader.GetInt32(1);
                    await con.CloseAsync();
                    return (FriendType) friendType;
                }
                else
                {
                    await con.CloseAsync();
                    return FriendType.None;
                }
            }
            catch (CustomError cf)
            {
                throw new CustomError(cf.Name);
            }
        }
        public async Task<object> AcceptOrCancelFriendRequest(int recipient, bool accept) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            try
            {
                FriendType type = await GetFriendShip(recipient);

                await UserMethod.IsUserExist(recipient);
                if (type == FriendType.Request)
                {
                    await con.OpenAsync();
                    if (accept)
                    {
                        command.CommandText =
                            $"UPDATE user_{Id}.friends SET type = {(int) FriendType.Friend} WHERE recipient = {recipient};";
                        await command.ExecuteNonQueryAsync();
                        command.CommandText =
                            $"UPDATE user_{recipient}.friends SET type = {(int) FriendType.Friend} WHERE recipient = {Id};";
                        await command.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        command.CommandText = $"DELETE FROM user_{Id}.friends WHERE recipient = {recipient};";
                    }

                    await con.CloseAsync();
                }
                else
                {
                    throw new CustomError("UserIsNotFriend");
                }
            }
            catch (CustomError cf)
            {
                throw new CustomError(cf.Name);
            }

            return true;
        }

        public async Task<object> RemoveFriend(int recipient) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (UserMethod.IsUserExist(recipient) == null) throw new CustomError("UserIsNotFriend");
            if (GetFriendShip(recipient).Result != FriendType.None)
            {
                await con.OpenAsync();

                command.CommandText = $"DELETE FROM user_{Id}.friends WHERE recipient = {recipient};";
                await command.ExecuteNonQueryAsync();
                command.CommandText = $"DELETE FROM user_{recipient}.friends WHERE recipient = {Id};";
                await command.ExecuteNonQueryAsync();

                await con.CloseAsync();
            }
            else
            {
                throw new CustomError("UserIsNotFriend");
            }


            return true;
        }

        //activity
        public async Task<object> AddActivity(int steps) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            await con.OpenAsync();
            
            command.Connection = con;
            command.CommandText = $"SELECT * FROM user_{this.Id}.activity WHERE day = '{DateTime.Now:MM/dd/yyyy}';";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            bool hasRows = reader.HasRows;

            await con.CloseAsync();

            if (hasRows)
            {
                
                await con.OpenAsync();
                command.CommandText = $"UPDATE user_{this.Id}.activity SET steps = {steps} WHERE day = '{DateTime.Now:MM/dd/yyyy}';";
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                await con.OpenAsync();
                command.CommandText = $"INSERT INTO user_{this.Id}.activity VALUES('{DateTime.Now:MM/dd/yyyy}', {steps});";
                await command.ExecuteNonQueryAsync();          
            }

            await con.CloseAsync();

            return true;
        }
        public async Task<List<Activity>> GetAllActivity() //Async
        {
            List<Activity> activities = new List<Activity>();
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            
            //
            command.CommandText = $"SELECT * FROM user_{this.Id}.activity;";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                activities.Add(new Activity(reader.GetString(0), reader.GetInt32(1)));
            }
            
            await con.CloseAsync();
            return activities;
        }
        public async Task<List<Activity>> GetLastWeekActivity() //Async
        {
            List<Activity> activities = new List<Activity>();
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            
            command.CommandText = $"SELECT * FROM user_{this.Id}.activity ORDER BY day ASC;";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            int i = 0;
            while (reader.Read())
            {
                Activity acti = new Activity(reader.GetString(0), reader.GetInt32(1));
                activities.Add(acti);
            }
            await con.CloseAsync();
            return activities;
        }
        public async Task<Activity> GetActivityCurrentDay() //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            
            command.CommandText = $"SELECT * FROM user_{this.Id}.activity WHERE day = '{DateTime.Now:MM/dd/yyyy}';";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            reader.Read();
            Activity currentDay= new Activity();

            if (reader.HasRows)
            {
                currentDay = new Activity(reader.GetString(0), reader.GetInt32(1));
            }

            await con.CloseAsync();

            return currentDay;
        }
        
        //settings
        public async Task Logout(string token)
        {
            if (!ChcekToken(token).Result) throw new CustomError("InvalidToken");
            
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();

            command.CommandText =
                $"DELETE FROM user_{Id}.tokens WHERE token ='{token}';";
            command.Connection = con;

            await con.OpenAsync();
            await command.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }
        public async Task<object> SetNewLogin(string? newLogin) //Async
        {                // ReSharper disable once ConditionIsAlwaysTrueOrFalse

            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE user_{this.Id}.user SET login = '{newLogin}';";
            await command.ExecuteNonQueryAsync();            
            command.CommandText =
                $"UPDATE base.base SET login = '{newLogin}' WHERE id = {this.Id};";
            await command.ExecuteNonQueryAsync();
            
            
            await con.CloseAsync();

            return true;
        }
        public async Task<object> SetNewEmail(string? newEmail) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE user_{this.Id}.user SET email = '{newEmail}';";
            await command.ExecuteNonQueryAsync();
            
            command.CommandText =
                $"UPDATE base.base SET email = '{newEmail}' WHERE id = {this.Id};";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        public async Task<object> SetNewAvatar(string? newAvatar) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            command.CommandText =
                $"UPDATE user_{this.Id}.user SET avatar = '{newAvatar}';";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
        public async Task ResetPasswordSendEmail()
        {
            MailMessage msg = new MailMessage();
                
            msg.From = new MailAddress("walkwardsdev@gmail.com");
            if (Email != null) msg.To.Add(Email);
            msg.Subject = "Aktywuj konto";
            msg.Body = $"<a href='https://backend.walkwards.pl/User/ActivateAccount?id={this.Id}'>Aktywuj konto</a>";
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = true;
                
            using (SmtpClient client = new SmtpClient())
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("walkwardsdev@gmail.com", "uK8ujw@vH@qh2e6g!V_phWu*P!");
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                
                await client.SendMailAsync(msg);
            }
        }
        
        //ranking
        public async Task<int> GetPlaceInRanking()
        {
            List<int> ids = new List<int>();
            
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("Main"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            
            //
            command.CommandText = $"SELECT * FROM base.base;";
            NpgsqlDataReader reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
            await con.CloseAsync();

            List<UserActivity> activities = new List<UserActivity>();
            foreach (int id in ids)
            {
                activities.Add(new UserActivity(UserMethod.GetUserData(id, false).Result.GetActivityCurrentDay().Result, UserMethod.GetUserData(id, false).Result));
            }

            activities = activities.OrderByDescending(userActivity => userActivity.Steps).ToList();

            int i = 1;
            foreach (var activity in activities)
            {
                if (activity.Id == Id)
                {
                    return i;
                }

                i++;
            }

            return 0;
        }

    }
}
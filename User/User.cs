using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using backend.Utilitis;

namespace backend.UserManager
{
    public class User
    {        
        public User(int id, string? firstName, string? lastName, string? email, string token, string? avatar = "defult", int accoutPrivacy = 0)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Avatar = avatar == null ? "None" : avatar;
            CompanyToken = token;
        }

        public User(int id, string? firstName, string? lastName, string? email)
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
        public string? Avatar { get; set; }
        public string CompanyToken;
        
        public async Task<bool> SetNewEmail(string? newEmail) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
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
        public async Task<bool> SetNewAvatar(string? newAvatar) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
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
        
        public async Task<bool> SetNewPassword(string? newPassword) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
            await con.OpenAsync();
            //
            if (newPassword != null)
                command.CommandText =
                    $"UPDATE user_{this.Id}.user SET password = '{Convert.ToBase64String(Encoding.UTF8.GetBytes(newPassword))}';";
            await command.ExecuteNonQueryAsync();
            
            await con.CloseAsync();
            return true;
        }
    }
}
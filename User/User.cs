using System.Text;
using backend.Utilities;
using Npgsql;

namespace backend.User
{
    public class User
    {        
        private static readonly string DataBaseName = "moveeko"; 

        public User(int id, string? firstName, string? lastName, string? email, string token, List<ActivityHandler.ReturnDay> activity)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            CompanyToken = token;
            this.Activity = activity;
        }

        public User(int id, string? firstName, string? lastName, string? email, string token, string avatar)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            CompanyToken = token;
            Avatar = avatar;
        }
        
        public User(int id, string? firstName, string? lastName, string? email, string avatar)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Avatar = avatar;
        }
        
        public User(int id, string? firstName, string? lastName, string? email, List<ActivityHandler.ReturnDay> activity, string companyToken)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;            
            Email = email;
            this.Activity = activity;
            CompanyToken = companyToken;
        }
        //propertis
        public int Id { get ; set; }
        public string? FirstName { get ; set; }
        public string? LastName { get ; set; }
        public string? Email { get ; set; }
        public string CompanyToken;
        public List<ActivityHandler.ReturnDay> Activity;

        private string Avatar;
        public int Points;
        
        
        public async Task<bool> SetNewAvatar(string? newAvatar) //Async
        {
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
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
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString(DataBaseName));
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
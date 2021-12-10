using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using backend.Utilities;
using Npgsql;
using backend.structure;
using backend.Utilitis;

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
            Avatar = avatar == null ? "None" : avatar;
            IsActivated = isActivated == null ? true : (bool)isActivated;
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int PlaceInRanging { get ; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? Avatar { get; set; }
        
        
        
        //settings
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
    }
}
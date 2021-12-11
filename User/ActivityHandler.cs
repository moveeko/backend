using System;
using System.Threading.Tasks;
using backend.UserManager;
using backend.Utilitis;
using Npgsql;

namespace backend.UserManager
{
    public class ActivityHandler
    {
        public enum TransportType
        {
            Car,
            Walk,
            Bike,
        }
    
        public class  OneDay
        {
            public DateTime Data;
            public TransportType Type;

            public OneDay(DateTime data, TransportType type)
            {
                Data = data;
                Type = type;
            }
        }

        public static async Task<object> AddActivity(User user, TransportType type)
        {
            OneDay today = new OneDay(DateTime.Today, type);
        
        
            NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
            await con.OpenAsync();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = con;
        
            command.CommandText =
                $"Insert into user_{user.Id}.activity (data, type)  VALUES('{today.Data.ToString()}', {(int) today.Type});";
            await command.ExecuteNonQueryAsync();
        
            return today;
        } 
    }
}
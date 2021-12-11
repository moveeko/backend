using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public class ReturnDay
        {
            public string Data;
            public int Type;
            public ReturnDay(string data, int type)
            {
                Data = data;
                Type = type;
            }        
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


            await con.CloseAsync();
            
            return today; 
        }
    
         public static async Task<List<ReturnDay>> ReturnActivity(User user, int limit)
         {
             NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
             await con.OpenAsync();
             NpgsqlCommand command = new NpgsqlCommand();
             command.Connection = con;
             
             command.CommandText =
                 $"SELECT * FROM user_{user.Id}.activity ORDER BY index DESC LIMIT {limit};";
             NpgsqlDataReader reader = await command.ExecuteReaderAsync();

             List<ReturnDay> list = new();

             while (await reader.ReadAsync())
             {
                 list.Add(new ReturnDay(reader.GetString(0), reader.GetInt32(1)));
                 var stop = "";
             }

             await con.CloseAsync();

             
             return list;
         }
    }   
}
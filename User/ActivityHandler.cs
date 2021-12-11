using backend.Utilitis;
using Npgsql;

namespace backend.UserManager;

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

    public static async Task<object> AddActivityBeforeWork(User user, TransportType type)
    {
        OneDay today = new OneDay(DateTime.Today, type);
        
        
        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
        await con.OpenAsync();
        NpgsqlCommand command = new NpgsqlCommand();
        command.Connection = con;
        
        command.CommandText =
            $"Insert into user_{user.Id}.activity (data, type)  VALUES('{today.Data.ToString()}', {(int) today.Type}, null);";
        await command.ExecuteNonQueryAsync();
        
        return today;
    }

    public static async Task<object> ReturnActivity(User user, int limit)
    {
        NpgsqlConnection con = new NpgsqlConnection(ConnectionsData.GetConectionString("moveeko"));
        await con.OpenAsync();
        NpgsqlCommand command = new NpgsqlCommand();
        command.Connection = con;
        
        command.CommandText =
            $"SELECT * FROM user_{user.Id}.activity ORDER BY index ASC LIMIT {limit});";
        await command.ExecuteNonQueryAsync();

        return null;
    }
}
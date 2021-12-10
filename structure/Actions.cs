using System;
using System.Threading.Tasks;

namespace backend.structure
{
    public enum Actions
    {
        
    }

    public static class ActionHandler
    {
        public static async Task<object> GetAction(Actions action)
        {
            return action switch
            {
                _ => new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }
    }
}
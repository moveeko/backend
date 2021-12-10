using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.structure
{
    public enum Actions
    {
        GetCompanyRanking,
        GetAllCompanies,
        RegisterCompany,
        CreateCompanyToken,
        GetCompanyToken,
        ChangeCompanyPrivacy,
        GetTodayActivity,
        SetStartActivity,
        SetEndActivity,
        JoinCompany,
        Empty
    }

    public static class ActionHandler
    {
        public static async Task<object> GetAction(Actions action, Dictionary<string, object> args)
        {
            return action switch
            {
                _ => new ArgumentOutOfRangeException(nameof(action), action, null)
            };
        }
    }
}
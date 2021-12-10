using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.UserManager;
using backend.Utilities;

namespace backend.structure
{
    public enum Actions
    {
        Login,
        IsUserExist,
        GetUserData,
        CreateUser,
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
                Actions.Login => UserMethod.Login(args["email"].ToString(), args["password"].ToString()).Result,
                Actions.CreateUser => UserMethod.CreateUser(args["firstName"].ToString(), args["lastName"].ToString(), args["email"].ToString(), args["password"].ToString()).Result,
                Actions.IsUserExist => UserMethod.IsUserExist((int)args["id"]).Result,
                Actions.GetUserData => UserMethod.GetUserData((int)args["id"]).Result,
                
                _ => throw new CustomError("UnknowAction", 500)
            };
        }
    }
}
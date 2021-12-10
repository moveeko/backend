using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Companies;
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
                Actions.IsUserExist => await UserMethod.IsUserExist((int)args["id"]),
                Actions.GetUserData => await UserMethod.GetUserData((int)args["id"]),
                
                Actions.RegisterCompany => await CompaniesMethod.CreateCompany(args["name"].ToString(), args["email"].ToString(), args["password"].ToString()),
                Actions.JoinCompany => await CompaniesMethod.GetCompany(args["token"].ToString(), false).Result.AddWorkers((int)args["id"]),
                
                _ => throw new CustomError("UnknownAction", 500)
            };
        }
    }
}
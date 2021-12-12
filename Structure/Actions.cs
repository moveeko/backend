using backend.Companies;
using backend.User;
using backend.Utilities;

namespace backend.structure
{
    public enum Actions
    {
        LoginUser,
        IsUserExist,
        GetUserData,
        CreateUser,
        GetCompanyRanking,
        GetAllCompanies,
        RegisterCompany,
        DeleteWorkerFromCompany,
        SetNewAvatar,
        SetNewEmail,
        SetNewPassword,
        ChoosePlan,
        AddActivity,
        GetCompanyWorkers,
        JoinCompany,
        LoginCompany,
        ReturnActivity,
        SetNewEmailCompany,
        SetNewAvatarCompany,
        SetNewPasswordCompany
    }

    public static class ActionHandler
    {
        public static async Task<object> GetAction(Actions action, Dictionary<string, object> args)
        {
            return action switch
            {
                Actions.LoginUser => UserMethod.Login(args["email"].ToString(), args["password"].ToString()).Result,
                Actions.IsUserExist => await UserMethod.IsUserExist((int)args["id"]),
                Actions.GetUserData => await UserMethod.GetUserData((int)args["id"]),
                Actions.CreateUser => UserMethod.CreateUser(args["firstName"].ToString(), args["lastName"].ToString(), args["email"].ToString(), args["password"].ToString()).Result,

                Actions.GetAllCompanies => await CompaniesMethod.GetAllCompany(),
                
                Actions.SetNewAvatar  => await UserMethod.GetUserData((int)args["id"]).Result.SetNewAvatar(args["newAvatar"].ToString()),
                Actions.SetNewPassword  => await UserMethod.GetUserData((int)args["id"]).Result.SetNewPassword(args["newPassword"].ToString()),
                
                Actions.RegisterCompany => await CompaniesMethod.CreateCompany(args["companyName"].ToString(), args["companyEmail"].ToString(), args["companyPassword"].ToString()),
                
                Actions.JoinCompany => await CompaniesMethod.GetCompany(args["companyToken"].ToString(), false).Result.AddWorkers((int)args["id"]),
                Actions.GetCompanyWorkers => await CompaniesMethod.GetCompany(args["token"].ToString(), false).Result.ReturnWorkers(),
                Actions.LoginCompany => await CompaniesMethod.Login(args["companyEmail"].ToString(), args["companyPassword"].ToString()),
                Actions.DeleteWorkerFromCompany => CompaniesMethod.GetCompany(args["token"].ToString(),  false).Result.DeleteWorker((int)args["id"]),
                
                Actions.AddActivity => await ActivityHandler.AddActivity(await UserMethod.GetUserData((int)args["id"], false), (ActivityHandler.TransportType)args["type"]),
                Actions.ReturnActivity => await ActivityHandler.ReturnActivity(await UserMethod.GetUserData((int)args["id"], false), (int)args["limit"]),
                
                Actions.SetNewEmailCompany  => await CompaniesMethod.GetCompany(args["token"].ToString()).Result.SetNewEmail(args["newEmail"].ToString()),
                Actions.SetNewAvatarCompany  => await CompaniesMethod.GetCompany(args["token"].ToString()).Result.SetNewAvatar(args["avatar"].ToString()),
                Actions.SetNewPasswordCompany  => await CompaniesMethod.GetCompany(args["token"].ToString()).Result.SetNewPassword(args["newPassword"].ToString()),
                Actions.ChoosePlan  => await CompaniesMethod.GetCompany(args["token"].ToString()).Result.ChoosePlan((int)args["maxusers"]),
                
                _ => throw new CustomError("UnknownAction", 500)
            };
        }
    }
}
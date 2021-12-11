using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.structure;
using backend.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace backend.Controllers
{
    [ApiController]
    public class DeveloperController : Controller
    {
        private readonly IWebHostEnvironment _config;

        public DeveloperController(IWebHostEnvironment config)
        {
            _config = config;
        }
        
        //Actions
        [HttpPost("/api/v0/action/login")]
        public async Task<ActionResult<object>> Login(string email, string password)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new []
                {
                    new HandleAction.Arg("email", email),
                    new HandleAction.Arg("password", password)
                }, Actions.LoginUser);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/action/isUserExist")]
        public async Task<ActionResult<object>> IsUserExist(int id)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new []
                {
                    new HandleAction.Arg("id", id)
                }, Actions.IsUserExist);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/action/getUserData")]
        public async Task<ActionResult<object>> GetUserData(int id)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new []
                {
                    new HandleAction.Arg("id", id)
                }, Actions.GetUserData);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/action/createUser")]
        public async Task<ActionResult<object>> CreateUser(string firstName, string lastName, string email, string password)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("firstName", firstName),
                    new HandleAction.Arg("lastName", lastName),
                    new HandleAction.Arg("email", email),
                    new HandleAction.Arg("password", password)
                }, Actions.CreateUser);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
    

        //Company Functions
        [HttpGet("/api/v0/company/getCompanyRanking")]
        public async Task<ActionResult<object>> GetCompanyRanking()
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(null, Actions.GetCompanyRanking);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpGet("/api/v0/company/getAllCompanies")]
        public async Task<ActionResult<object>> GetAllCompanies()
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(null, Actions.GetAllCompanies);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/company/registerCompany")]
        public async Task<ActionResult<object>> RegisterCompany(string companyName, string companyEmail, string companyPassword)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("name", companyName),
                    new HandleAction.Arg("email", companyEmail),
                    new HandleAction.Arg("password", companyPassword)
                }, Actions.RegisterCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/company/loginCompany")]
        public async Task<ActionResult<object>> LoginCompany(string companyEmail, string companyPassword)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("email", companyEmail),
                    new HandleAction.Arg("password", companyPassword)
                }, Actions.LoginCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/company/joinCompany")]
        public async Task<ActionResult<object>> JoinCompany(string companyToken, int id)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("token", companyToken),
                    new HandleAction.Arg("id", id)
                }, Actions.JoinCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/company/returnWorkers")]
        public async Task<ActionResult<object>> ReturnWorkers(string token)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("token", token)
                }, Actions.GetCompanyWorkers);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/company/removeWorker")]
        public async Task<ActionResult<object>> RemoveWorker(string token, int id)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("token", token),
                    new HandleAction.Arg("id", id)
                }, Actions.DeleteWorkerFromCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        //User actions

        [HttpPost("/api/v0/user/setNewEmail")]
        public async Task<ActionResult<object>> NewEmail(int id, string newEmail)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("id", id),
                    new HandleAction.Arg("newEmail", newEmail)
                }, Actions.SetNewEmail);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/user/setNewPassword")]
        public async Task<ActionResult<object>> NewPassword(int id, string newPassword)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("id", id),
                    new HandleAction.Arg("newPassword", newPassword)
                }, Actions.SetNewPassword);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v0/user/setNewAvatar")]
        public async Task<ActionResult<object>> NewAvatar(int id, string newAvatar)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new[]
                {
                    new HandleAction.Arg("id", id),
                    new HandleAction.Arg("newAvatar", newAvatar)
                }, Actions.SetNewAvatar);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }

    }
}

using System;
using System.Threading.Tasks;
using backend.structure;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Cors;

namespace backend.Controllers
{
    [EnableCors]
    [ApiController]
    public class MainController : Controller
    {
        private readonly IWebHostEnvironment _config;

        public MainController(IWebHostEnvironment config)
        {
            _config = config;
        }

       [HttpPost("/api/v1/action/login")]
        public async Task<ActionResult<object>> Login([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "email, string", Actions.LoginUser);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/action/isUserExist")]
        public async Task<ActionResult<object>> IsUserExist([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id", Actions.IsUserExist);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/action/getUserData")]
        public async Task<ActionResult<object>> GetUserData([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id", Actions.GetUserData);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }

        [HttpPost("/api/v1/action/createUser")]
        public async Task<ActionResult<object>> CreateUser([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "firstName, lastName, email, password", Actions.CreateUser);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
    

        //Company Functions
        [HttpGet("/api/v1/company/getCompanyRanking")]
        public async Task<ActionResult<object>> GetCompanyRanking()
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new JObject(), "", Actions.GetCompanyRanking);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpGet("/api/v1/company/getAllCompanies")]
        public async Task<ActionResult<object>> GetAllCompanies()
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(new JObject(), "", Actions.GetAllCompanies);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/company/registerCompany")]
        public async Task<ActionResult<object>> RegisterCompany([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "companyName, companyEmail, companyPassword", Actions.RegisterCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/company/loginCompany")]
        public async Task<ActionResult<object>> LoginCompany([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "companyEmail, companyPassword", Actions.LoginCompany);


                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/company/joinCompany")]
        public async Task<ActionResult<object>> JoinCompany([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "companyToken, id", Actions.JoinCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/company/returnWorkers")]
        public async Task<ActionResult<object>> ReturnWorkers([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "token", Actions.GetCompanyWorkers);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/company/removeWorker")]
        public async Task<ActionResult<object>> RemoveWorker([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "token, id", Actions.DeleteWorkerFromCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/company/setNewAvatar")]
        public async Task<ActionResult<object>> SetNewAvatar([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "token, avatar", Actions.SetNewAvatarCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        //User actions

        [HttpPost("/api/v1/user/setNewEmail")]
        public async Task<ActionResult<object>> NewEmail([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id, newEmail", Actions.SetNewEmailCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/user/setNewPassword")]
        public async Task<ActionResult<object>> NewPassword([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id, newPassword", Actions.SetNewPasswordCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/user/setNewAvatar")]
        public async Task<ActionResult<object>> NewAvatar([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id, newAvatar", Actions.SetNewAvatarCompany);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        [HttpPost("/api/v1/user/AddActivity")]
        public async Task<ActionResult<object>> AddActivity([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id, activityType", Actions.AddActivity);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
    
        [HttpPost("/api/v1/user/ReturnActivity")]
        public async Task<ActionResult<object>> ReturnActivity([FromBody] JObject json)
        {
            Task<dynamic> task = Task.Run(async () =>
            {
                HandleAction action = new(_config);

                await action.SetResponse(json, "id, limit", Actions.AddActivity);

                return action.Response;
            })!;

            await task.WaitAsync(TimeSpan.FromSeconds(999));

            dynamic data = task.Result;

            return data;
        }
        
        
    }
}

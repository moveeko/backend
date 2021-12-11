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
    }
}

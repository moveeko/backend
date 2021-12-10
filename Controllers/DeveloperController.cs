using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
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

        [HttpPost("path")]
        public async Task<ActionResult<object>> Teample(string data)
        {
            return "";
        }
    }
}

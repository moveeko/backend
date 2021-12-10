using System;
using System.Threading.Tasks;
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

        [HttpPost("/")]
        public async Task<ActionResult<object>> Teample()
        {
            return "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.structure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace backend.Utilities
{
    public class HandleAction : ControllerBase
    {
        public class Arg
        {
            public Arg(string name, object value)
            {
                this.Name = name;
                this.Value = value is string ? value.ToString()!.Replace(" ", "") : value;
            }

            public string Name { get; set; }
            public object Value { get; set; }
        }

        private readonly IWebHostEnvironment _config;

        public HandleAction(IWebHostEnvironment config)
        {
            _config = config;
            Response = new CustomError("UnKnownError", 500);
        }
        
        public new dynamic? Response;

        public ObjectResult ExceptionStringHandler(Exception ex)
        {
            return StatusCode(500, _config.IsDevelopment() ? ex.Message : "please contact support");
        }
        
         public async Task SetResponse(Arg[]? args, Actions action)
        {
            try
            {
                Dictionary<string, object> data = args is null ? 
                    new Dictionary<string, object>() : 
                    args.ToDictionary(item => item.Name, item => item.Value);
                
                var task = Task.Run(async () => await ActionHandler.GetAction(action, data));

                //TimeoutException if too long (10 sec)
                await task.WaitAsync(TimeSpan.FromSeconds(10));
                Response = task.Result;
            }
            catch (TimeoutException)
            {
                Response = StatusCode(410, "TimeoutException");
            }
            catch (CustomError ce)
            {
                Response = StatusCode(ce.StatusCode, ce.Name);
            }
            catch (Exception ex)
            {
                if(ex.GetType() == typeof(AggregateException))
                {
                    if(ex.InnerException is CustomError cr)
                    {
                        Response = StatusCode(cr.StatusCode, cr.Name);
                        return;
                    }         
                }

                Response = ExceptionStringHandler(ex);
            }
        }
        
        public async Task SetResponse(JObject json, string reqArgs, Actions action)
        {
            Dictionary<string, object> formData = 
                json.ToObject<Dictionary<string, object>>() ?? 
                throw new InvalidOperationException("data form json was null");
            
            List <Arg> args = new();
            bool error = false;

            foreach (string arg in reqArgs.Replace(" ", "").Split(","))
            {
                if (!formData.ContainsKey(arg))
                {
                    Response = BadRequest();
                    error = true;
                    break;
                }


                args.Add(arg is "id" or "steps" or "friendId"? 
                    new Arg(arg, (int)(long)formData[arg]) : new Arg(arg, formData[arg]));
            }

            if (!error)
            {
                await SetResponse(args.ToArray(), action);
            }
        }
    }
}
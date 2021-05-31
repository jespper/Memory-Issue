using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 999, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("test")]
        public IActionResult Test(int amount = 3000000)
        {
            try
            {
                using var export = new Export();

                var bytes = export.Test(amount);

                var file = File($"\\reports\\{export.Id}.csv", "text/csv", "test.csv");
                //Response.
                return file;
            }
            catch (Exception)
            {
                Console.WriteLine(this);
                throw;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (context.Result is VirtualFileResult file)
            {
                context.HttpContext.Response.OnCompleted(() =>
                {
                    var task = new Task(() => RemoveOldFile(file.FileName));
                    task.Start();
                    return task;
                });
            }
        }

        private static void RemoveOldFile(string fileName)
        {
            System.IO.File.Delete(Environment.CurrentDirectory + "\\wwwroot" + fileName);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

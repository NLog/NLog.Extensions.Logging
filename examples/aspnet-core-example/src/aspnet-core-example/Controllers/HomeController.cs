using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace aspnet_core_example.Controllers
{
    public class HomeController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public IActionResult Index()
        {
            Logger.Info("Index page says hello");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            Logger.Error("Oops");
            return View();
        }
    }
}

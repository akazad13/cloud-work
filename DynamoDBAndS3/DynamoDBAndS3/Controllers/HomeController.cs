using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shafiya_Gagandeep_Lab3.Models;
using Microsoft.AspNetCore.Http;

namespace Shafiya_Gagandeep_Lab3.Controllers
{
    public class HomeController : Controller
    {
        // S3Bucket = navyafarhanalab3bucket
        // DynamoDB table = COMP306Lab3Table

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View("Error", TempData["error"].ToString());
        }

    }
}

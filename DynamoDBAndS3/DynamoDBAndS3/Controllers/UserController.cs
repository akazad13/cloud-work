using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shafiya_Gagandeep_Lab3.Data;
using Shafiya_Gagandeep_Lab3.Models;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Shafiya_Gagandeep_Lab3.Controllers
{
    public class UserController : Controller
    {
        private IAmazonDynamoDB dynamoDBClient;
        public UserController(IAmazonDynamoDB dynamoDBClient)
        {
            //_context = context;
            this.dynamoDBClient = dynamoDBClient;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser()
        {
            User user = new User();
            user.Username = Request.Form["username"];
            user.Password = Request.Form["password"];
            user.Firstname = Request.Form["firstname"];
            user.Lastname = Request.Form["lastname"];
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            if(!await service.UserExists(user.Username))
            {
                User newUser = await service.InsertUser(user);
                return RedirectToAction("List", "Movie");
            }
            else
            {
                TempData["error"] = "Username already taken";
                return RedirectToAction("Error", "Home");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Login()
        {
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            if (await service.LoginSuccess(username, password))
            {
                HttpContext.Session.SetString("LoggedIn", username);
                return Redirect("/Movie/List");
            }
            else
            {
                TempData["error"] = "Error with logging in";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}

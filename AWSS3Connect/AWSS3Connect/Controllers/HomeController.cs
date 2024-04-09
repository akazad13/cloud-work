using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AWSS3Connect.Models;
using AWSS3Connect.Utilities;
using Newtonsoft.Json;

namespace AWSS3Connect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHelper _helper;

        public HomeController(ILogger<HomeController> logger, IHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Error = "";
            var books = await _helper.ReadAllObjectDataAsync();
            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewBag.Error = "";
            var ret = await _helper.ReadObjectDataAsync(id);
            if (ret[1] != "Success")
            {
                ViewBag.Error = "There are some error connection to AWS. Please refresh the page.";
                return RedirectToAction(nameof(Index));
            }
            var bookDetail = JsonConvert.DeserializeObject<Book>(ret[0]);
            return View("Details", bookDetail);
        }

        public IActionResult Create()
        {
            ViewBag.Error = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,BookName,Description")] Book book)
        {
            ViewBag.Error = "";
            if (ModelState.IsValid)
            {
                if(await _helper.IsObjectExist(book.BookId))
                {
                    ViewBag.Error = "This Book Id is already exist. Please provide another one.";
                    return View(book);
                }
                var data = JsonConvert.SerializeObject(book);
                var success = await _helper.FileUploadonAmazonS3(book.BookId, data);
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Error = "";
            var ret = await _helper.ReadObjectDataAsync(id);
            if (ret[1] != "Success")
            {
                ViewBag.Error = "Failed to get the book details.";
                return RedirectToAction(nameof(Index));
            }
            var bookDetail = JsonConvert.DeserializeObject<Book>(ret[0]);
            return View("Edit", bookDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookName,Description")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.BookId = id;
                var data = JsonConvert.SerializeObject(book);
                var success = await _helper.FileUploadonAmazonS3(book.BookId, data);
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var ret = await _helper.DeleteAWSS3Object(id);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

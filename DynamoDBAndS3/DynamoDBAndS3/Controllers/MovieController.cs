using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shafiya_Gagandeep_Lab3.Data;
using Shafiya_Gagandeep_Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Shafiya_Gagandeep_Lab3.Controllers
{
    public class MovieController : Controller
    {
        private IAmazonDynamoDB dynamoDBClient;
        public MovieController(IAmazonDynamoDB dynamoDBClient)
        {
            this.dynamoDBClient = dynamoDBClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult UploadMovie()
        {
            if(String.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedIn")))
            {
                TempData["error"] = "Please log in to upload a movie";
                return RedirectToAction("Error", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMovieToAWS(string movieName, IFormFile movieFile)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            Movie movie = new Movie();
            movie.Name = movieName;
            movie.File = S3Link.Create(context, "navyafarhanalab3bucket", movieFile.FileName, Amazon.RegionEndpoint.USEast1);
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            Movie newMovie = await service.InsertMovie(movie);

            var fileName = ContentDispositionHeaderValue
                .Parse(movieFile.ContentDisposition)
                .FileName
                .TrimStart().ToString();

            bool status;
            using (var fileStream = movieFile.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                status = await UploadFileAsync(ms, fileName, null);
            }

            return Redirect("/Movie/List");
        }

        public async Task<bool> UploadFileAsync(Stream fileStream, string fileName, string directory = null)
        {
            try
            {
                IAmazonS3 S3Client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);
                var fileTransferUtility = new TransferUtility(S3Client);


                var bucketPath = "navyafarhanalab3bucket";

                var fileUploadRequest = new TransferUtilityUploadRequest()
                {
                    CannedACL = S3CannedACL.PublicRead,
                    BucketName = bucketPath,
                    Key = fileName,
                    InputStream = fileStream
                };

                await fileTransferUtility.UploadAsync(fileUploadRequest);

                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    // add error logging
                }
                else
                {
                    //add error handling
                }
                return false;
            }
        }

        public async Task<IActionResult> List()
        {
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            List<Movie> movies = await service.GetListOfMovies();

            return View(movies);
        }

        public async Task<IActionResult> Ratings()
        {
            string movieName = Request.Form["movieName"];
            string movieId = Request.Form["movieId"];
            ViewBag.MovieId = movieId;
            ViewBag.MovieName = movieName;

            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            List<Rating> ratings = await service.GetRatings(movieId);

            return View(ratings);
        }

        public async Task<IActionResult> PostRatingAsync()
        {
            if(string.IsNullOrWhiteSpace(HttpContext.Session.GetString("LoggedIn")))
            {
                TempData["error"] = "Please log in to post a rating/comment";
                return RedirectToAction("Error", "Home");
            }
            string movieId = Request.Form["movieId"];
            string movieName = Request.Form["movieName"];
            string movieRating = Request.Form["movieRating"];
            string movieComment = Request.Form["movieComment"];

            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            string user = HttpContext.Session.GetString("LoggedIn");
            Rating rating = new Rating()
            { 
                Username = user,
                Rate = movieRating,
                Comment = movieComment,
                Date = DateTime.Now.ToString(),
                Id = movieId,
                SortId = "COMMENT_"+ Guid.NewGuid().ToString()
            };
            DynamoDBServices service = new DynamoDBServices(dynamoDBClient);
            Rating newRating = await service.InsertRating(rating);

            List<Rating> ratings = await service.GetRatings(movieId);
            ViewBag.MovieId = movieId;
            ViewBag.MovieName = movieName;
            return View("Ratings", ratings);
        }
    }
}

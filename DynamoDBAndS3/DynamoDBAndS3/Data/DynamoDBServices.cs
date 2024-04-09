using Shafiya_Gagandeep_Lab3.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shafiya_Gagandeep_Lab3.Data
{
    public class DynamoDBServices
    {
        IAmazonDynamoDB dynamoDBClient { get; set; }

        public DynamoDBServices(IAmazonDynamoDB dynamoDBClient)
        {
            this.dynamoDBClient = dynamoDBClient;
        }

        public async Task<User> InsertUser(User user)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);

            user.Id = "USER_" + user.Username;
            user.SortId = user.Id;
            await context.SaveAsync(user, default(System.Threading.CancellationToken));
            //User newUser = await context.LoadAsync<User>(user.Id, default(System.Threading.CancellationToken));
            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            Table userTable = Table.LoadTable(dynamoDBClient, "COMP306Lab3Table");
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.Equal, "USER_" + username);
            Search search = userTable.Scan(scanFilter);
            return search.Count == 1;
        }

        public async Task<Movie> InsertMovie(Movie movie)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);

            Table movieTable = Table.LoadTable(dynamoDBClient, "COMP306Lab3Table");
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.BeginsWith, "MOVIE_");
            scanFilter.AddCondition("SortId", ScanOperator.BeginsWith, "MOVIE_");
            Search search = movieTable.Scan(scanFilter);
            int numOfMovies = search.Count;

            movie.Id = "MOVIE_" + (numOfMovies + 1).ToString();
            movie.SortId = movie.Id;
            await context.SaveAsync(movie, default(System.Threading.CancellationToken));
            //User newUser = await context.LoadAsync<User>(user.Id, default(System.Threading.CancellationToken));
            return movie;
        }

        public async Task<bool> LoginSuccess(string username, string password)
        {
            Table userTable = Table.LoadTable(dynamoDBClient, "COMP306Lab3Table");
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.Equal, "USER_" + username);
            Search search = userTable.Scan(scanFilter);

            if(search.Count == 1)
            {
                List<Document> documentList = new List<Document>();
                documentList = await search.GetNextSetAsync();
                foreach(var document in documentList)
                {
                    if (document["Password"].ToString().Equals(password))
                        return true;
                }
            }
            return false;
        }

        public async Task<List<Movie>> GetListOfMovies()
        {
            Table movieTable = Table.LoadTable(dynamoDBClient, "COMP306Lab3Table");
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.BeginsWith, "MOVIE_");
            scanFilter.AddCondition("SortId", ScanOperator.NotContains, "COMMENT_");
            Search search = movieTable.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            List<Movie> moviesList = new List<Movie>();
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            do
            {
                documentList = await search.GetNextSetAsync();
                
                foreach (var document in documentList)
                {
                    Movie movie = await context.LoadAsync<Movie>(document["Id"].ToString(), document["Id"].ToString(), default(System.Threading.CancellationToken));

                    moviesList.Add(movie);
                }
            } while (!search.IsDone);

            return moviesList;
        }

        public async Task<List<Rating>> GetRatings(string movieId)
        {
            Table ratings = Table.LoadTable(dynamoDBClient, "COMP306Lab3Table");
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("Id", ScanOperator.Equal, movieId);
            scanFilter.AddCondition("SortId", ScanOperator.BeginsWith, "COMMENT_");
            Search search = ratings.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            List<Rating> ratingsList = new List<Rating>();
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);
            do
            {
                documentList = await search.GetNextSetAsync();

                foreach (var document in documentList)
                {
                    Rating rating = await context.LoadAsync<Rating>(document["Id"].ToString(), document["SortId"].ToString(), default(System.Threading.CancellationToken));

                    ratingsList.Add(rating);
                }
            } while (!search.IsDone);

            ratingsList = ratingsList.OrderByDescending(r => DateTime.Parse(r.Date)).ToList();
            return ratingsList;
        }
        public async Task<Rating> InsertRating(Rating rating)
        {
            DynamoDBContext context = new DynamoDBContext(dynamoDBClient);

            await context.SaveAsync(rating, default(System.Threading.CancellationToken));
            //User newUser = await context.LoadAsync<User>(user.Id, default(System.Threading.CancellationToken));
            return rating;
        }
    }
}

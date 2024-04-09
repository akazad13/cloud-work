using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shafiya_Gagandeep_Lab3.Models
{
    [DynamoDBTable("COMP306Lab3Table")]
    public class Rating
    {
        [DynamoDBHashKey]
        public string Id { get; set; } // MovieID
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string SortId { get; set; } // RatingID

        public string Rate { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string Username { get; set; }
    }
}

using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shafiya_Gagandeep_Lab3.Models
{
    [DynamoDBTable("COMP306Lab3Table")]
    public class Movie
    {
        [DynamoDBHashKey]
        public string Id { get; set; } // MovieID
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string SortId { get; set; }

        public string Name { get; set; }
        public S3Link File { get; set; }
    }
}

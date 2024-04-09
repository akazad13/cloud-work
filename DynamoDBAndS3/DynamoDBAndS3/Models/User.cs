using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shafiya_Gagandeep_Lab3.Models
{
    [DynamoDBTable("COMP306Lab3Table")]
    public class User
    {
        [DynamoDBHashKey]
        public string Id { get; set; } // UserID
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string SortId { get; set; } // UserID

        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}

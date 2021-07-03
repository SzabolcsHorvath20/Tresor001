using System;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tresor001
{
    public class Review : TableEntity
    {
        public Review() { }

        public Review(string partition, string row)
        {
            PartitionKey = partition;
            RowKey = row;
        }
        public int review_rating { get; set; }
        public string review_name { get; set; }
        public string review_text { get; set; }
        public string review_category { get; set; }

    }
}

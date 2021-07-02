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

        public string text { get; set; }

        public override string ToString()
        {
            return text;
        }
    }
}

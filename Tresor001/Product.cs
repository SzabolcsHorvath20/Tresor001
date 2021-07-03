﻿using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tresor001
{
    public class Product : TableEntity
    {
        public Product() { }

        public Product(string partition, string row)
        {
            PartitionKey = partition;
            RowKey = row;
        }

        public string product_description { get; set; }
        public int product_rating { get; set; }
        public string product_name { get; set; }
    }
}

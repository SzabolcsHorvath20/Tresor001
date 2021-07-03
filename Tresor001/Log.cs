using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tresor001
{
    public class Log : TableEntity
    {
        public Log() { }

        public Log(string partition, string row)
        {
            PartitionKey = partition;
            RowKey = row;
        }

    }
}

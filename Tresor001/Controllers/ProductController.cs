using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tresor001.Controllers
{
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=tresor01;AccountKey=upMX4oCjopXt8t+l2xPoHRqS1zoIITBebAtUXfeGlGTqSV47KKVNZ/XQ7pLTjqWddz1PSScDQcw5ICyO9JMdHA==;EndpointSuffix=core.windows.net";
        private string tableName = "Product";

        [HttpGet]
        public IActionResult Get(string id)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            var products = table.ExecuteQuery(new TableQuery<Product>()).ToList();
            List<Product> selected_products = new List<Product>();
            foreach (var item in products)
            {
                if (item.product_name == id)
                {
                    selected_products.Add(item);
                }
            }
            var sorted_products = selected_products.OrderBy(x => x.Timestamp);
            
            JObject json = new JObject();
            json["products"] = JToken.FromObject(sorted_products);
            return Ok(json);
        }

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Tresor001.Controllers
{
    [Route("api/Review")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=tresor01;AccountKey=upMX4oCjopXt8t+l2xPoHRqS1zoIITBebAtUXfeGlGTqSV47KKVNZ/XQ7pLTjqWddz1PSScDQcw5ICyO9JMdHA==;EndpointSuffix=core.windows.net";
        private string tableName = "tresor001";
        public bool viewedLast = true;

        [HttpGet]
        public IActionResult Get()
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            var reviews = table.ExecuteQuery(new TableQuery<Review>()).ToList();
            var sortedreviews = reviews.OrderBy(x => x.Timestamp);
            JObject json = new JObject();
            json["reviews"] = JToken.FromObject(sortedreviews);
            viewedLast = true;
            return Ok(json);
        }

        [HttpPost]
        public IActionResult Post()
        {
            if (viewedLast == false)
            {
                return BadRequest("Can't write a review without viewing the previous ones.");
            }
            else
            {
                return Ok();
            }
        }
    }
}

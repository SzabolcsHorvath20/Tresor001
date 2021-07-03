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
        private string tableNameReview = "Review";
        private string tableNameProduct = "Product";

        [HttpGet]
        public IActionResult Get()
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableNameReview);
            var reviews = table.ExecuteQuery(new TableQuery<Review>()).ToList();
            var sortedreviews = reviews.OrderBy(x => x.Timestamp);
            JObject json = new JObject();
            json["reviews"] = JToken.FromObject(sortedreviews);
            return Ok(json);
        }

        [HttpPost]
        public IActionResult Post(JObject jreview)
        {
            Review review = jreview.ToObject<Review>();
            if (review.review_text.Length > 500)
            {
                return BadRequest("Cannot save review. Review size is more then 500 characters.");
            }
            else
            {
                InsertReview(review);
                UpdateRating(review.review_category, review.PartitionKey);
                return Ok();
            }
        }

        public async void InsertReview(Review review)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableNameReview);
            TableOperation insertReview = TableOperation.InsertOrMerge(review);
            TableResult result = await table.ExecuteAsync(insertReview);
        }

        public async void UpdateRating(string category, string review_id)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClientProduct = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableProduct = tableClientProduct.GetTableReference(tableNameProduct);
            TableOperation retrieveOperation = TableOperation.Retrieve<Product>(category, review_id);
            TableResult result = await tableProduct.ExecuteAsync(retrieveOperation);
            var retrievedProduct = result.Result as Product;

            CloudTableClient tableClientReview = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableReview = tableClientReview.GetTableReference(tableNameReview);
            var query = new TableQuery<Review>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, review_id));
            var resultReview = tableReview.ExecuteQuery<Review>(query).ToList();
            int sumRating = 0;
            foreach (var item in resultReview)
            {
                sumRating += item.review_rating;
            }
            int totalRating = sumRating / resultReview.Count;
            retrievedProduct.product_rating = totalRating;

            TableOperation updateOperation = TableOperation.Merge(retrievedProduct);
            tableProduct.Execute(updateOperation);



        }
    }
}

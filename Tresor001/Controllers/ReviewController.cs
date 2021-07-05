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
        private static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=tresor01;AccountKey=upMX4oCjopXt8t+l2xPoHRqS1zoIITBebAtUXfeGlGTqSV47KKVNZ/XQ7pLTjqWddz1PSScDQcw5ICyO9JMdHA==;EndpointSuffix=core.windows.net";
        private static string tableNameReview = "Review";
        private static string tableNameProduct = "Product";
        private static string tableNameLog = "Log";
        private CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

        [HttpGet]
        public IActionResult Get(string id, string category, int rows)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableNameReview);
            var reviews = table.ExecuteQuery(new TableQuery<Review>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id))).ToList();
            var sortedreviews = reviews.OrderBy(x => x.Timestamp).Reverse();
            if (GetProduct(category, id).Result == null)
            {
                return BadRequest("No product by that name.");
            }
            else
            {
                JObject json = new JObject();
                json["reviews"] = JToken.FromObject(sortedreviews.Take(rows));
                InsertLog(id);
                return Ok(json);
            }
        }

        

        [HttpPost]
        public IActionResult Post(JObject jreview)
        {
            Review review = jreview.ToObject<Review>();
            review.RowKey = Convert.ToString(GetLatest(review.PartitionKey));
            Product retrievedProduct = GetProduct(review.review_category, review.PartitionKey).Result;
            if (CheckLog(review.PartitionKey).Result == null)
            {               
                return BadRequest("Can't post review. Didn't read the previous ones.");
            }
            else
            {
                if (review.review_text.Length > 500)
                {
                    return BadRequest("Cannot save review. Review size is more then 500 characters.");
                }
                else
                {
                    if (review.review_rating < 1 || review.review_rating > 5)
                    {
                        return BadRequest("Product rating must be between 1 and 5.");
                    }
                    else
                    {
                        InsertReview(review);
                        UpdateRating(retrievedProduct, review.PartitionKey);
                        DeleteLog(CheckLog(review.PartitionKey).Result);
                        return Ok("Review posted.");
                    }
                }
            }
        }

        public async void InsertReview(Review review)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableNameReview);
            TableOperation insertReview = TableOperation.InsertOrMerge(review);
            TableResult result = await table.ExecuteAsync(insertReview);
        }

        public int GetLatest(string id)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableNameReview);
            var reviews = table.ExecuteQuery(new TableQuery<Review>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id))).ToList();
            List<int> rowKeys = new List<int>();
            foreach (var item in reviews)
            {
                rowKeys.Add(Convert.ToInt32(item.RowKey));
            }
            if (rowKeys.Count == 0)
            {
                return 1;
            }
            else
            {
                return rowKeys.OrderByDescending(x => x).First() + 1;
            }

        }

        public async Task<Product> GetProduct(string category, string review_id)
        {
            CloudTableClient tableClientProduct = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableProduct = tableClientProduct.GetTableReference(tableNameProduct);
            TableOperation retrieveOperation = TableOperation.Retrieve<Product>(category, review_id);
            TableResult result = await tableProduct.ExecuteAsync(retrieveOperation);
            return result.Result as Product;
        }
        public void UpdateRating(Product retrievedProduct, string reviewId)
        {
            CloudTableClient tableClientProduct = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableProduct = tableClientProduct.GetTableReference(tableNameProduct);

            CloudTableClient tableClientReview = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableReview = tableClientReview.GetTableReference(tableNameReview);
            var query = new TableQuery<Review>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, reviewId));
            var resultReview = tableReview.ExecuteQuery<Review>(query).ToList();
            double sumRating = 0;
            foreach (var item in resultReview)
            {
                sumRating += item.review_rating;
            }
            double totalRating = Math.Round((sumRating / resultReview.Count), 1);
            retrievedProduct.product_rating = totalRating;

            TableOperation updateOperation = TableOperation.Merge(retrievedProduct);
            tableProduct.Execute(updateOperation);

        }

        public async void InsertLog(string product_id)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableLog = tableClient.GetTableReference(tableNameLog);
            Log log = new Log(product_id, HttpContext.Connection.Id);
            TableOperation insertLog = TableOperation.InsertOrMerge(log);
            TableResult result = await tableLog.ExecuteAsync(insertLog);
        }

        public async Task<Log> CheckLog(string product_id)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableLog = tableClient.GetTableReference(tableNameLog);
            TableOperation retrieveOperation = TableOperation.Retrieve<Log>(product_id, HttpContext.Connection.Id.ToString());
            TableResult result = await tableLog.ExecuteAsync(retrieveOperation);         
            var retrievedLog = result.Result as Log;
            return retrievedLog;
        }

        public async void DeleteLog(Log retrievedLog)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable tableLog = tableClient.GetTableReference(tableNameLog);
            TableOperation deleteLog = TableOperation.Delete(retrievedLog);
            TableResult resultDel = await tableLog.ExecuteAsync(deleteLog);
        }
    }
}

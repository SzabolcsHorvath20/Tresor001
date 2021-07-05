using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tresor001.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            string information =
                "http://localhost:24018/api/Product?id=Doll \n" +
                "Gets the details of the product. \n \n" +
                "http://localhost:24018/api/Review?id=Doll&rows=5&category=Toys \n" +
                "Gets the reviews of the specified product. \n" +
                "Sending over the category is also necessary for faster searching. \n" +
                "The number of rows can be specified and the reviews are always returned ordered by the date, so the most recent is first. \n\n" +
                "http://localhost:24018/api/Review \n" +
                "";
            return Ok(information);
            
        }
    }
}

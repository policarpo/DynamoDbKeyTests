using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("purchase-prices")]
    public class PurchasePricesController : ControllerBase
    {
        private readonly ILogger<PurchasePricesController> _logger;

        public PurchasePricesController(ILogger<PurchasePricesController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload")]
        public IActionResult Upload([FromForm]PurchasePriceUploadData uploadData)
        {
            if (uploadData == null)
                return BadRequest("uploaded data is null");

            if (uploadData.PurchasePriceFile == null)
                return BadRequest("file is null");

            return Ok();
        }
    }

    public class PurchasePriceUploadData
    {
        public IFormFile PurchasePriceFile { get; set; }
        public string PriceType { get; set; }
        public int SupplierId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

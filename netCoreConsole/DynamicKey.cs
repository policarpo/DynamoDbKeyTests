using System;
using System.Text;
using System.Threading.Tasks;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace netCoreConsole
{
    class DynamicKey
    {
        private readonly DynamoDBContext _context;
        private DateTime _date;

        public DynamicKey(DynamoDBContext dynamoDbContext)
        {
            _context = dynamoDbContext;
        }

        public async Task<PurchasePriceDynamicTable> SetupData()
        {
            //active perm
            int productId = 123;
            int supplierId = 1;
            var priceType = "permanent";
            var entity = new PurchasePriceDynamicTable($"P#{productId}", $"S#{supplierId}");
            entity.ProductId = productId;
            entity.SupplierId = supplierId;
            entity.PriceType = priceType;
            entity.StartDate = _date.ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5).ToString("yyyy-MM-dd");
            await _context.SaveAsync(entity);

            //active perm
            supplierId = 2;
            entity = new PurchasePriceDynamicTable($"S#{supplierId}", $"P#{productId}");
            entity.ProductId = productId;
            entity.SupplierId = supplierId;
            entity.PriceType = priceType;
            entity.StartDate = _date.ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5).ToString("yyyy-MM-dd");
            await _context.SaveAsync(entity);

            productId = 456;
            //active perm
            entity = new PurchasePriceDynamicTable($"P#{productId}_S#{supplierId}_{priceType}", _date.ToString("yyyy-MM-dd"));
            entity.ProductId = productId;
            entity.SupplierId = supplierId;
            entity.PriceType = priceType;
            entity.StartDate = _date.ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5).ToString("yyyy-MM-dd");
            await _context.SaveAsync(entity);

            var sortKey = _date.AddDays(-2).ToString("yyyy-MM-dd");
            entity = new PurchasePriceDynamicTable($"P#{productId}_S#{supplierId}_{priceType}", sortKey);
            entity.ProductId = productId;
            entity.SupplierId = supplierId;
            entity.PriceType = priceType;
            entity.StartDate = sortKey;
            entity.EndDate = _date.ToString("yyyy-MM-dd");
            await _context.SaveAsync(entity);

            ////upcoming temp
            priceType = "temp";
            entity = new PurchasePriceDynamicTable($"{priceType}", $"P#{productId}_S#{supplierId}");
            entity.ProductId = productId;
            entity.SupplierId = supplierId;
            entity.PriceType = priceType;
            entity.StartDate = _date.AddDays(3).ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5).ToString("yyyy-MM-dd");
            await _context.SaveAsync(entity);

            //past temp
            entity = new PurchasePriceDynamicTable($"{priceType}", $"P#{productId}_S#{supplierId}");
            entity.ProductId = productId;
            entity.SupplierId = supplierId;
            entity.PriceType = priceType;
            entity.StartDate = _date.AddDays(-15).ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(-5).ToString("yyyy-MM-dd");
            await _context.SaveAsync(entity);

            Console.WriteLine("Inserted items");
            return entity;
        }

        internal async Task Run(DateTime date)
        {
            _date = date;
            await SetupData();

            await QueryByTypeAndStartDate("P#456_S#2_permanent", new DateTime(2020, 12, 1));
            await QueryByTypeStartAndEndDate("P#456_S#2_permanent", 
                new DateTime(2020, 12, 1), 
                new DateTime(2020, 12, 4));
            await QueryPricesAsOfDate("P#123", "S#1", new DateTime(2020, 12, 2));
            await QueryPricesAsOfDate("P#123", "S#1", new DateTime(2020, 12, 6));

            await QueryByProductAndSupplier("P#456", "S#2");
            //await QueryByProductAndSupplier("P#456", "S#2");
        }

        private async Task QueryByTypeAndStartDate(string id, DateTime startDate)
        {
            var result = await _context.QueryAsync<PurchasePriceDynamicTable>(
                    id,
                    QueryOperator.GreaterThanOrEqual,
                    new[] { startDate.ToString("yyyy-MM-dd") })
                .GetRemainingAsync();

            Console.WriteLine($"by type and start date found:{result.Count}");
        }

        private async Task QueryByTypeStartAndEndDate(string id, DateTime startDate, DateTime endDate)
        {
            var result = await _context.QueryAsync<PurchasePriceDynamicTable>(
                    id,
                    QueryOperator.GreaterThanOrEqual,
                    new[] { startDate.ToString("yyyy-MM-dd") },
                    new DynamoDBOperationConfig()
                    {
                        QueryFilter = {
                            new ScanCondition(
                                "EndDate",
                                ScanOperator.LessThanOrEqual,
                                endDate.ToString("yyyy-MM-dd"))
                        }
                    })
                .GetRemainingAsync();

            Console.WriteLine($"by type start and end date found:{result.Count}");
        }

        private async Task QueryPricesAsOfDate(string partitionKey, string sortKey, DateTime lastOrderedDate)
        {
            var result = await _context.QueryAsync<PurchasePriceDynamicTable>(partitionKey,
                QueryOperator.Equal,
                new[] { sortKey },
                new DynamoDBOperationConfig()
                {
                    QueryFilter = {
                    new ScanCondition(
                        "StartDate",
                        ScanOperator.LessThanOrEqual,
                        lastOrderedDate.ToString("yyyy-MM-dd")
                    ),
                    new ScanCondition(
                        "EndDate",
                        ScanOperator.GreaterThanOrEqual,
                        lastOrderedDate.ToString("yyyy-MM-dd"))
                    }
                }).GetRemainingAsync();

            Console.WriteLine($"last ordered found:{result.Count}");
        }

        private async Task QueryByProductAndSupplier(string partitionKey, string sortKey)
        {
            var result = await _context.QueryAsync<PurchasePriceDynamicTable>(partitionKey,
                QueryOperator.BeginsWith,
                new[] { sortKey })
                .GetRemainingAsync();

            Console.WriteLine($"by product and supplier found:{result.Count}");
        }

        private async Task QueryByProduct(string partitionKey)
        {
            var result = await _context.QueryAsync<PurchasePriceDynamicTable>(partitionKey)
                .GetRemainingAsync();

            Console.WriteLine($"by product found:{result.Count}");
        }
    }
}

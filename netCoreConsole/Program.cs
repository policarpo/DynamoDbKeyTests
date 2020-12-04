using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using Microsoft.Extensions.Configuration;

namespace netCoreConsole
{
    class Program
    {
        private static DynamoDBContext _context;
        static DateTime _date = new DateTime(2020, 12, 4);

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string, string>("AWS:Region", "eu-west-1"),
                    new KeyValuePair<string, string>("AWS:Profile", "development")})
                .Build();

            var options = configuration.GetAWSOptions();

            var dynamoClient = options.CreateServiceClient<IAmazonDynamoDB>();
            _context = new DynamoDBContext(dynamoClient);
            var entity = await SetupData();

            await QueryByTypeAndStartDate(entity.Id, new DateTime(2020, 12, 1));
            await QueryByTypeStartAndEndDate(entity.Id, new DateTime(2020, 12, 1), new DateTime(2020, 12, 10));

            var lastOrderedDate = _date.AddDays(-15);

            await QueryPricesAsOfDate(entity.ProductId, entity.SupplierId, lastOrderedDate);
            await QueryByProduct(entity.ProductId);
            await QueryByProductAndSupplier(entity.ProductId, entity.SupplierId);

            var existingTemporaryPrices = await GetExistingTemporaryPrices(456, 2);
            var upcomingTemporaryPrices = existingTemporaryPrices
               .Where(p => p.IsUpcoming)
               .ToArray();

            if (upcomingTemporaryPrices.Any())
            {
                await Save(upcomingTemporaryPrices.First());
            }
        }

        private static async Task<PurchasePriceFixedTable> SetupData()
        {
            //active perm
            var entity = new PurchasePriceFixedTable(123, 1, "permanent");
            
            entity.StartDate = _date.ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5);
            await _context.SaveAsync(entity);

            //active perm
            entity = new PurchasePriceFixedTable(456, 1, "permanent");
            entity.StartDate = _date.ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5);
            await _context.SaveAsync(entity);

            //active perm
            entity = new PurchasePriceFixedTable(456, 2, "permanent");
            entity.StartDate = _date.ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5);
            await _context.SaveAsync(entity);

            //upcoming temp
            entity = new PurchasePriceFixedTable(456, 2, "temp");
            entity.StartDate = _date.AddDays(3).ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(5);
            await _context.SaveAsync(entity);

            //past temp
            entity = new PurchasePriceFixedTable(456, 2, "temp");
            entity.StartDate = _date.AddDays(-15).ToString("yyyy-MM-dd");
            entity.EndDate = _date.AddDays(-5);
            await _context.SaveAsync(entity);

            Console.WriteLine("Inserted items");
            return entity;
        }

        private static async Task Save(PurchasePriceFixedTable existingActivePrice)
        {
            //update/save price
            existingActivePrice.EndDate = _date.AddDays(1);
            await _context.SaveAsync(existingActivePrice);
        }

        private static async Task Delete(PurchasePriceFixedTable[] upcomingTemporaryPrices)
        {
            foreach (var upcoming in upcomingTemporaryPrices)
            {
                //delete
                await _context.DeleteAsync<PurchasePriceFixedTable>(upcoming.Id, upcoming.StartDate);
            }
        }

        private async static Task<List<PurchasePriceFixedTable>> GetExistingTemporaryPrices(int productId , int supplierId)
        {
            string id = $"P#{productId}_S#{supplierId}_temp";
            var result = await _context.QueryAsync<PurchasePriceFixedTable>(id)
                .GetRemainingAsync();
            return result;
        }

        private static async Task QueryByTypeStartAndEndDate(string id, DateTime startDate, DateTime endDate)
        {
            var result = await _context.QueryAsync<PurchasePriceFixedTable>(
                    id, 
                    QueryOperator.GreaterThanOrEqual,
                    new[] { startDate.ToString("yyyy-MM-dd") },
                    new DynamoDBOperationConfig()
                    {
                         QueryFilter = {
                            new ScanCondition(
                                "EndDate",
                                ScanOperator.LessThanOrEqual,
                                endDate)
                        }
                     })
                .GetRemainingAsync();

            Console.WriteLine($"by type start and end date found:{result.Count}");
        }

        private static async Task QueryByTypeAndStartDate(string id, DateTime startDate)
        {
            var result = await _context.QueryAsync<PurchasePriceFixedTable>(
                    id, 
                    QueryOperator.GreaterThanOrEqual,
                    new[] { startDate.ToString("yyyy-MM-dd") })
                .GetRemainingAsync();

            Console.WriteLine($"by type and start date found:{result.Count}");
        }

        private static async Task QueryPricesAsOfDate(int productId, int supplierId, DateTime lastOrderedDate)
        {
            var result =  await _context.QueryAsync<PurchasePriceFixedTable>(productId,
                QueryOperator.Equal,
                new[] { (object)supplierId },
                new DynamoDBOperationConfig()
                {
                    IndexName = "ProductId-SupplierId-index",
                    QueryFilter = {
                    new ScanCondition(
                        "StartDate",
                        ScanOperator.LessThanOrEqual,
                        lastOrderedDate.ToString("yyyy-MM-dd")
                    ),
                    new ScanCondition(
                        "EndDate",
                        ScanOperator.GreaterThanOrEqual,
                        lastOrderedDate)
                    }
                }).GetRemainingAsync();

            Console.WriteLine($"last ordered found:{result.Count}");
        }

        private static async Task QueryByProductAndSupplier(int productId, int supplierId)
        {
            var result = await _context.QueryAsync<PurchasePriceFixedTable>(productId,
                QueryOperator.Equal,
                new[] { (object)supplierId },
                new DynamoDBOperationConfig()
                {
                    IndexName = "ProductId-SupplierId-index"
                }).GetRemainingAsync();

            Console.WriteLine($"by product and supplier found:{result.Count}");
        }

        private static async Task QueryByProduct(int productId)
        {
            var result = await _context.QueryAsync<PurchasePriceFixedTable>(productId,
                new DynamoDBOperationConfig()
                {
                    IndexName = "ProductId-SupplierId-index",
                }).GetRemainingAsync();

            Console.WriteLine($"by product found:{result.Count}");
        }
    }
}

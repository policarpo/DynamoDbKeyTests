using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

namespace netCoreConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string, string>("AWS:Region", "eu-west-1"),
                    new KeyValuePair<string, string>("AWS:Profile", "development")})
                .Build();

            var options = configuration.GetAWSOptions();

            var dynamoClient = options.CreateServiceClient<IAmazonDynamoDB>();
            var context = new DynamoDBContext(dynamoClient);

            //active perm
            var table = new PurchasePriceFixedTable(123, 1, "permanent");
            table.StartDate = DateTime.Today.ToString("yyyy-MM-dd");
            table.EndDate = DateTime.Today.AddDays(5);
            await context.SaveAsync(table);

            //active perm
            table = new PurchasePriceFixedTable(456, 1, "permanent");
            table.StartDate = DateTime.Today.ToString("yyyy-MM-dd");
            table.EndDate = DateTime.Today.AddDays(5);
            await context.SaveAsync(table);

            //active perm
            table = new PurchasePriceFixedTable(456, 2, "permanent");
            table.StartDate = DateTime.Today.ToString("yyyy-MM-dd");
            table.EndDate = DateTime.Today.AddDays(5);
            await context.SaveAsync(table);

            //upcoming temp
            table = new PurchasePriceFixedTable(456, 2, "temp");
            table.StartDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");
            table.EndDate = DateTime.Today.AddDays(5);
            await context.SaveAsync(table);

            //past temp
            table = new PurchasePriceFixedTable(456, 2, "temp");
            table.StartDate = DateTime.Today.AddDays(-15).ToString("yyyy-MM-dd");
            table.EndDate = DateTime.Today.AddDays(-5);
            await context.SaveAsync(table);

            Console.WriteLine("Inserted items");

            //get upcoming price
            //context.QueryAsync<PurchasePriceFixedTable>( table.Id, Amazon.DynamoDBv2.DocumentModel.QueryOperator.GreaterThanOrEqual,
            //    new[] { DateTime.Today.AddDays(3).ToString("yyyy-MM-dd") });

            //get active price
            //context.QueryAsync<PurchasePriceFixedTable>(table.Id, Amazon.DynamoDBv2.DocumentModel.QueryOperator.GreaterThanOrEqual,
            //    new[] { DateTime.Today.ToString("yyyy-MM-dd") });

            var lastOrderedDate = DateTime.Today.AddDays(-15);
            //get last ordered
            var result = await context.QueryAsync<PurchasePriceFixedTable>(table.ProductId,
                QueryOperator.Equal,
                new[] { (object)table.SupplierId },
                new DynamoDBOperationConfig()
                {
                    IndexName= "ProductId-SupplierId-index",
                    QueryFilter = {
                        new ScanCondition(
                            "StartDate",
                            ScanOperator.LessThanOrEqual,
                            lastOrderedDate.ToString("yyyy-MM-dd")
                        ),
                        new ScanCondition(
                            "EndDate",
                            ScanOperator.GreaterThanOrEqual,
                            lastOrderedDate
                        )
                    }
                }).GetRemainingAsync();

            Console.WriteLine(result.Count);
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}

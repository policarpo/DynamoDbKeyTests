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
            
            //await new FixedKey(_context).Run(_date);
            await new DynamicKey(_context).Run(_date);
        }
    }
}

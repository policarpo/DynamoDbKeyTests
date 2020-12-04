
using Amazon.DynamoDBv2.DataModel;

namespace netCoreConsole
{
    [DynamoDBTable("purchase_prices_dynamic_key")]
    public class PurchasePriceDynamicTable
    {
        public PurchasePriceDynamicTable() { }

        public PurchasePriceDynamicTable(string partitionKey, string sortKey)
        {
            PartitionKey = partitionKey;
            SortKey = sortKey;
        }

        [DynamoDBHashKey]
        public string PartitionKey { get; set; }
        [DynamoDBRangeKey]
        public string SortKey { get; set; }
        [DynamoDBProperty]
        public string EndDate { get; set; }
        [DynamoDBProperty]
        public string StartDate { get; set; }
        [DynamoDBProperty]
        public int ProductId { get; set; }
        [DynamoDBProperty]
        public int SupplierId { get; set; }
        [DynamoDBProperty]
        public string PriceType { get; set; }
    }
}
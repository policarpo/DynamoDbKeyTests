using System;

using Amazon.DynamoDBv2.DataModel;

namespace netCoreConsole
{
    [DynamoDBTable("purchase_prices_fixed_composite_key")]
    public class PurchasePriceFixedTable
    {
        [DynamoDBHashKey]
        public string Id
        {
            get; set;
        }

        [DynamoDBRangeKey]
        public string StartDate { get; set; }
        [DynamoDBProperty]
        public DateTime? EndDate { get; set; }
        [DynamoDBProperty]
        public int ProductId { get; set; }
        [DynamoDBProperty]
        public int SupplierId { get; set; }
        [DynamoDBProperty]
        public string PriceType { get; set; }
        public PurchasePriceFixedTable()
        {

        }

        public bool IsActive
        {
            get {
                var startDate = DateTime.ParseExact(StartDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                return startDate<= DateTime.Today && (EndDate == null || DateTime.Today <= EndDate); 
            }
        }

        public bool IsUpcoming
        {
            get
            {
                var startDate = DateTime.ParseExact(StartDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                return startDate > DateTime.Today;
            }
        }

        public PurchasePriceFixedTable(int productId, int supplierId, string priceType)
        {
            ProductId = productId;
            SupplierId = supplierId;
            PriceType = priceType;
            Id = $"P#{ProductId}_S#{SupplierId}_{PriceType}";
        }
    }

    [DynamoDBTable("purchase_prices_fixed_composite_key")]
    public class PurchasePriceDynamicTable
    {
        [DynamoDBHashKey]
        public string PartitionKey;
        [DynamoDBRangeKey]
        public string SortKey;
        [DynamoDBProperty]
        public DateTime? EndDate;
        [DynamoDBProperty]
        public DateTime StartDate;
        [DynamoDBProperty]
        public string ProductId;
        [DynamoDBProperty]
        public string SupplierId;
        [DynamoDBProperty]
        public string PriceType;

    }
}
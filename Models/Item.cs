using Azure;
using Azure.Data.Tables;

namespace AppService.Models
{
    public class Item : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Custom properties
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }

        public Item()
        {
        }

        public Item(string partitionKey, string name)
        {
            PartitionKey = partitionKey;
            RowKey = Guid.NewGuid().ToString();
            Name = name;
            IsActive = true;
        }
    }

    public class CreateItemRequest
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class UpdateItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}

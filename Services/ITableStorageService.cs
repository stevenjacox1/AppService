using AppService.Models;

namespace AppService.Services
{
    public interface ITableStorageService
    {
        Task<Item> CreateItemAsync(CreateItemRequest request);
        Task<Item?> GetItemAsync(string partitionKey, string rowKey);
        Task<List<Item>> GetItemsByPartitionKeyAsync(string partitionKey);
        Task<List<Item>> GetAllItemsAsync();
        Task<Item> UpdateItemAsync(string partitionKey, string rowKey, UpdateItemRequest request);
        Task DeleteItemAsync(string partitionKey, string rowKey);
    }
}

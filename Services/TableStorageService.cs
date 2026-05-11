using AppService.Models;
using Azure.Data.Tables;

namespace AppService.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<TableStorageService> _logger;

        public TableStorageService(TableClient tableClient, ILogger<TableStorageService> logger)
        {
            _tableClient = tableClient;
            _logger = logger;
        }

        public async Task<Item> CreateItemAsync(CreateItemRequest request)
        {
            var item = new Item(request.PartitionKey, request.Name)
            {
                Description = request.Description,
                Price = request.Price
            };

            try
            {
                await _tableClient.AddEntityAsync(item);
                _logger.LogInformation($"Item created: {item.RowKey}");
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating item: {ex.Message}");
                throw;
            }
        }

        public async Task<Item?> GetItemAsync(string partitionKey, string rowKey)
        {
            try
            {
                var result = await _tableClient.GetEntityAsync<Item>(partitionKey, rowKey);
                return result.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Item not found: {partitionKey} - {rowKey}");
                return null;
            }
        }

        public async Task<List<Item>> GetItemsByPartitionKeyAsync(string partitionKey)
        {
            try
            {
                var items = new List<Item>();
                await foreach (var entity in _tableClient.QueryAsync<Item>(
                    x => x.PartitionKey == partitionKey))
                {
                    items.Add(entity);
                }
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving items by partition: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            try
            {
                var items = new List<Item>();
                await foreach (var entity in _tableClient.QueryAsync<Item>())
                {
                    items.Add(entity);
                }
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all items: {ex.Message}");
                throw;
            }
        }

        public async Task<Item> UpdateItemAsync(string partitionKey, string rowKey, UpdateItemRequest request)
        {
            try
            {
                var item = await GetItemAsync(partitionKey, rowKey);
                if (item == null)
                    throw new KeyNotFoundException($"Item {rowKey} not found");

                item.Name = request.Name;
                item.Description = request.Description;
                item.Price = request.Price;
                item.IsActive = request.IsActive;

                await _tableClient.UpdateEntityAsync(item, item.ETag);
                _logger.LogInformation($"Item updated: {rowKey}");
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating item: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteItemAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
                _logger.LogInformation($"Item deleted: {rowKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting item: {ex.Message}");
                throw;
            }
        }
    }
}

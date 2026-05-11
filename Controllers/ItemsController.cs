using AppService.Models;
using AppService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(ITableStorageService tableStorageService, ILogger<ItemsController> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new item
        /// </summary>
        /// <response code="201">Item created successfully</response>
        /// <response code="400">Invalid request</response>
        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem([FromBody] CreateItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _tableStorageService.CreateItemAsync(request);
            return CreatedAtAction(nameof(GetItem), new { partitionKey = item.PartitionKey, rowKey = item.RowKey }, item);
        }

        /// <summary>
        /// Get all items
        /// </summary>
        /// <response code="200">Returns all items</response>
        [HttpGet]
        public async Task<ActionResult<List<Item>>> GetAllItems()
        {
            var items = await _tableStorageService.GetAllItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Get items by partition key
        /// </summary>
        /// <param name="partitionKey">The partition key to filter by</param>
        /// <response code="200">Returns items for the partition</response>
        [HttpGet("partition/{partitionKey}")]
        public async Task<ActionResult<List<Item>>> GetItemsByPartition(string partitionKey)
        {
            var items = await _tableStorageService.GetItemsByPartitionKeyAsync(partitionKey);
            return Ok(items);
        }

        /// <summary>
        /// Get a specific item by partition key and row key
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <response code="200">Item found</response>
        /// <response code="404">Item not found</response>
        [HttpGet("{partitionKey}/{rowKey}")]
        public async Task<ActionResult<Item>> GetItem(string partitionKey, string rowKey)
        {
            var item = await _tableStorageService.GetItemAsync(partitionKey, rowKey);
            if (item == null)
            {
                return NotFound(new { message = $"Item with key {rowKey} not found" });
            }

            return Ok(item);
        }

        /// <summary>
        /// Update an existing item
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <param name="request">The updated item data</param>
        /// <response code="200">Item updated successfully</response>
        /// <response code="404">Item not found</response>
        /// <response code="400">Invalid request</response>
        [HttpPut("{partitionKey}/{rowKey}")]
        public async Task<ActionResult<Item>> UpdateItem(string partitionKey, string rowKey, [FromBody] UpdateItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var item = await _tableStorageService.UpdateItemAsync(partitionKey, rowKey, request);
                return Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Item with key {rowKey} not found" });
            }
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <response code="204">Item deleted successfully</response>
        /// <response code="404">Item not found</response>
        [HttpDelete("{partitionKey}/{rowKey}")]
        public async Task<IActionResult> DeleteItem(string partitionKey, string rowKey)
        {
            try
            {
                await _tableStorageService.DeleteItemAsync(partitionKey, rowKey);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting item: {ex.Message}");
                return NotFound(new { message = $"Item with key {rowKey} not found" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <response code="200">Service is healthy</response>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}

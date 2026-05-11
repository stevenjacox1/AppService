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
        [HttpPost]
        [ProduceResponseType(StatusCodes.Status201Created)]
        [ProduceResponseType(StatusCodes.Status400BadRequest)]
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
        [HttpGet]
        [ProduceResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Item>>> GetAllItems()
        {
            var items = await _tableStorageService.GetAllItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Get items by partition key
        /// </summary>
        [HttpGet("partition/{partitionKey}")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Item>>> GetItemsByPartition(string partitionKey)
        {
            var items = await _tableStorageService.GetItemsByPartitionKeyAsync(partitionKey);
            return Ok(items);
        }

        /// <summary>
        /// Get a specific item by partition key and row key
        /// </summary>
        [HttpGet("{partitionKey}/{rowKey}")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status404NotFound)]
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
        [HttpPut("{partitionKey}/{rowKey}")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        [ProduceResponseType(StatusCodes.Status404NotFound)]
        [ProduceResponseType(StatusCodes.Status400BadRequest)]
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
        [HttpDelete("{partitionKey}/{rowKey}")]
        [ProduceResponseType(StatusCodes.Status204NoContent)]
        [ProduceResponseType(StatusCodes.Status404NotFound)]
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
        [HttpGet("health")]
        [ProduceResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}

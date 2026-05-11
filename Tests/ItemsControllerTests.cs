using AppService.Controllers;
using AppService.Models;
using AppService.Services;
using Moq;

namespace AppService.Tests
{
    [TestClass]
    public class ItemsControllerTests
    {
        private Mock<ITableStorageService> _mockTableStorageService;
        private Mock<ILogger<ItemsController>> _mockLogger;
        private ItemsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockTableStorageService = new Mock<ITableStorageService>();
            _mockLogger = new Mock<ILogger<ItemsController>>();
            _controller = new ItemsController(_mockTableStorageService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task CreateItem_WithValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new CreateItemRequest
            {
                PartitionKey = "category1",
                Name = "Test Item",
                Description = "Test Description",
                Price = 9.99m
            };

            var expectedItem = new Item("category1", "Test Item")
            {
                Description = "Test Description",
                Price = 9.99m
            };

            _mockTableStorageService
                .Setup(x => x.CreateItemAsync(request))
                .ReturnsAsync(expectedItem);

            // Act
            var result = await _controller.CreateItem(request);

            // Assert
            Assert.IsNotNull(result);
            _mockTableStorageService.Verify(x => x.CreateItemAsync(request), Times.Once);
        }

        [TestMethod]
        public async Task GetAllItems_ReturnsOkResult()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item("category1", "Item 1"),
                new Item("category2", "Item 2")
            };

            _mockTableStorageService
                .Setup(x => x.GetAllItemsAsync())
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetAllItems();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Value.Count);
        }

        [TestMethod]
        public async Task GetItem_WithValidKeys_ReturnsOkResult()
        {
            // Arrange
            var partitionKey = "category1";
            var rowKey = "item-1";
            var item = new Item(partitionKey, "Test Item") { RowKey = rowKey };

            _mockTableStorageService
                .Setup(x => x.GetItemAsync(partitionKey, rowKey))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.GetItem(partitionKey, rowKey);

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Test Item", result.Value.Name);
        }

        [TestMethod]
        public async Task GetItem_WithInvalidKeys_ReturnsNotFound()
        {
            // Arrange
            var partitionKey = "category1";
            var rowKey = "invalid-key";

            _mockTableStorageService
                .Setup(x => x.GetItemAsync(partitionKey, rowKey))
                .ReturnsAsync((Item)null);

            // Act
            var result = await _controller.GetItem(partitionKey, rowKey);

            // Assert
            Assert.IsNull(result.Value);
        }
    }
}

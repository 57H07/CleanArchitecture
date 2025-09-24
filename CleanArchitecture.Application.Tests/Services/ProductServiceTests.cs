using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Tests.Helpers;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockUserRepository = new Mock<IUserRepository>();

        // Setup UnitOfWork to return mocked repositories
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        _productService = new ProductService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = TestDataBuilder.CreateProductList(3);
        _mockProductRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<ProductDto>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var productId = 1;
        var product = TestDataBuilder.CreateValidProduct(productId);
        
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product 1");
        result.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var productId = 999;
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenUserExists_ShouldCreateProduct()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateValidProductDto("New Product", 1);
        var createdProduct = TestDataBuilder.CreateValidProduct(1, 1);
        createdProduct.Name = createDto.Name;
        createdProduct.Price = createDto.Price;

        _mockUserRepository.Setup(r => r.ExistsAsync(createDto.UserId))
            .ReturnsAsync(true);
        _mockProductRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Price.Should().Be(createDto.Price);
        _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateValidProductDto("Test Product", 999);

        _mockUserRepository.Setup(r => r.ExistsAsync(createDto.UserId))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _productService.CreateAsync(createDto);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User with ID 999 not found.");
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_ShouldUpdateProduct()
    {
        // Arrange
        var productId = 1;
        var updateDto = TestDataBuilder.CreateValidProductDto("Updated Product", 1);
        var existingProduct = TestDataBuilder.CreateValidProduct(productId, 1);
        existingProduct.Name = "Old Product";

        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockUserRepository.Setup(r => r.ExistsAsync(updateDto.UserId))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateAsync(productId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Product");
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        var productId = 1;
        
        _mockProductRepository.Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _productService.DeleteAsync(productId);

        // Assert
        _mockProductRepository.Verify(r => r.DeleteAsync(productId), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenProductExists_ShouldReturnTrue()
    {
        // Arrange
        var productId = 1;
        _mockProductRepository.Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(true);

        // Act
        var result = await _productService.ExistsAsync(productId);

        // Assert
        result.Should().BeTrue();
    }
}

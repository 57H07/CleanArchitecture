using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using AutoFixture;
using AutoFixture.Kernel;

namespace CleanArchitecture.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly ProductService _productService;
    private readonly Fixture _fixture;

    public ProductServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _fixture = new Fixture();

        // Configure AutoFixture to handle circular references
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Setup UnitOfWork to return mocked repositories
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);

        _productService = new ProductService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = _fixture.CreateMany<Product>(3).ToList();
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
        var product = _fixture.Build<Product>()
            .With(p => p.Id, productId)
            .With(p => p.Name, "Test Product")
            .Create();
        
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
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
        var createDto = _fixture.Build<CreateProductDto>()
            .With(p => p.UserId, 1)
            .With(p => p.Name, "New Product")
            .With(p => p.Price, 99.99m)
            .Create();

        var user = _fixture.Build<User>().With(u => u.Id, 1).Create();
        var createdProduct = _fixture.Build<Product>()
            .With(p => p.Id, 1)
            .With(p => p.Name, createDto.Name)
            .Create();

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
        _mockProductRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var createDto = _fixture.Build<CreateProductDto>()
            .With(p => p.UserId, 999)
            .With(p => p.Price, 99.99m)
            .Create();

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
        var updateDto = _fixture.Build<CreateProductDto>()
            .With(p => p.Name, "Updated Product")
            .With(p => p.UserId, 1)
            .Create();

        var existingProduct = _fixture.Build<Product>()
            .With(p => p.Id, productId)
            .With(p => p.Name, "Old Product")
            .Create();

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

using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product?.Adapt<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetByUserIdAsync(int userId)
    {
        var products = await _unitOfWork.Products.GetByUserIdAsync(userId);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAvailableProductsAsync();
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        // Validate that the user exists
        if (!await _unitOfWork.Users.ExistsAsync(createProductDto.UserId))
        {
            throw new KeyNotFoundException($"User with ID {createProductDto.UserId} not found.");
        }

        var product = createProductDto.Adapt<Product>();
        product.CreatedAt = DateTime.UtcNow;

        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return createdProduct.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, CreateProductDto updateProductDto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        // Validate that the user exists if changing user
        if (product.UserId != updateProductDto.UserId && 
            !await _unitOfWork.Users.ExistsAsync(updateProductDto.UserId))
        {
            throw new KeyNotFoundException($"User with ID {updateProductDto.UserId} not found.");
        }

        updateProductDto.Adapt(product);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return product.Adapt<ProductDto>();
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _unitOfWork.Products.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateStockAsync(int id, int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        product.UpdateStock(quantity);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Products.ExistsAsync(id);
    }
}

using CleanArchitecture.Application.Collections;
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Interfaces.Repositories;
using CleanArchitecture.Application.Interfaces.Services;
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

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        return product?.Adapt<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var pagedProducts = await _unitOfWork.Products.GetPagedAsync(filter, cancellationToken);

        return pagedProducts.ToPagedResult(dto => dto.Adapt<ProductDto>());
    }

    public async Task<IEnumerable<ProductDto>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByCustomerIdAsync(customerId, cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(category, cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetAvailableProductsAsync(cancellationToken);
        return products.Adapt<IEnumerable<ProductDto>>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Customers.ExistsAsync(createProductDto.CustomerId, cancellationToken))
        {
            throw new EntityNotFoundException("Customer", createProductDto.CustomerId);
        }

        var product = createProductDto.Adapt<Product>();
        product.ValidateBusinessRules();
        product.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, CreateProductDto updateProductDto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException("Product", id);
        }

        if (product.CustomerId != updateProductDto.CustomerId &&
            !await _unitOfWork.Customers.ExistsAsync(updateProductDto.CustomerId, cancellationToken))
        {
            throw new EntityNotFoundException("Customer", updateProductDto.CustomerId);
        }

        updateProductDto.Adapt(product);
        product.ValidateBusinessRules();
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Adapt<ProductDto>();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.Products.ExistsAsync(id, cancellationToken))
        {
            throw new EntityNotFoundException("Product", id);
        }

        await _unitOfWork.Products.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(int id, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException("Product", id);
        }

        product.UpdateStock(quantity);
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.ExistsAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetDistinctCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Products.GetDistinctCategoriesAsync(cancellationToken);
    }
}

using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class ProductMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.IsInStock, src => src.IsInStock())
            .Map(dest => dest.IsPurchasable, src => src.IsPurchasable())
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.Name : string.Empty);

        // Avoid tracked entity to keep their old value.
        config.NewConfig<CreateProductDto, Product>()
            .IgnoreNullValues(false);

        config.NewConfig<ProductDto, CreateProductDto>();
    }
}

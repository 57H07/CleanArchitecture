using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class ProductMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.IsInStock, src => src.IsInStock())
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.Name : string.Empty);

        TypeAdapterConfig<CreateProductDto, Product>.NewConfig()
            .IgnoreNullValues(true);
    }
}

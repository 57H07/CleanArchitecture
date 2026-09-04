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

        // Also the update mapping (ProductService.UpdateAsync adapts onto the tracked
        // entity), so null must overwrite or cleared fields would keep their old value.
        TypeAdapterConfig<CreateProductDto, Product>.NewConfig()
            .IgnoreNullValues(false);
    }
}

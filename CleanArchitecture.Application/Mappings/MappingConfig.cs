using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class MappingConfig
{
    public static void Configure()
    {
        // User mappings
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.FullName, src => src.GetFullName())
            .Map(dest => dest.Age, src => src.GetAge());

        TypeAdapterConfig<CreateUserDto, User>.NewConfig()
            .IgnoreNullValues(true);

        // Product mappings
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.IsInStock, src => src.IsInStock())
            .Map(dest => dest.UserName, src => src.User != null ? src.User.GetFullName() : string.Empty);

        TypeAdapterConfig<CreateProductDto, Product>.NewConfig()
            .IgnoreNullValues(true);
    }
}

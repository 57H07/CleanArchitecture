using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using CleanArchitecture.Application.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Interfaces.Services;

namespace CleanArchitecture.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Configure Mapster
        MappingConfig.Configure();
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<IMapper, ServiceMapper>();

        // Add application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class CustomerMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<Customer, CustomerDto>.NewConfig();

        TypeAdapterConfig<CreateCustomerDto, Customer>.NewConfig()
            .IgnoreNullValues(true);
    }
}

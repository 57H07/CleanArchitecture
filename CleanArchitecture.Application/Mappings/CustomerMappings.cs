using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class CustomerMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<Customer, CustomerDto>.NewConfig();

        // Also the update mapping (CustomerService.UpdateAsync adapts onto the tracked
        // entity), so null must overwrite or cleared fields would keep their old value.
        TypeAdapterConfig<CreateCustomerDto, Customer>.NewConfig()
            .IgnoreNullValues(false);
    }
}

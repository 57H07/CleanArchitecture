using CleanArchitecture.Application.DTOs;
using CleanArchitecture.Domain.Entities;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class UserMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.FullName, src => src.GetFullName())
            .Map(dest => dest.Age, src => src.GetAge());

        TypeAdapterConfig<CreateUserDto, User>.NewConfig()
            .IgnoreNullValues(true);
    }
}
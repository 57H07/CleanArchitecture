using System.Reflection;
using Mapster;

namespace CleanArchitecture.Application.Mappings;

public class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        config.Compile();
    }
}

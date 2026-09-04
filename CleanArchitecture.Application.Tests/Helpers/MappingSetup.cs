using System.Runtime.CompilerServices;
using CleanArchitecture.Application.Mappings;

namespace CleanArchitecture.Application.Tests.Helpers;

/// <summary>
/// Applies the real Mapster registrations before any test runs, as Program.cs does at
/// startup. Without this the suite exercises Mapster's convention fallback instead.
/// </summary>
public static class MappingSetup
{
    [ModuleInitializer]
    internal static void Initialize() => MappingConfig.Configure();
}

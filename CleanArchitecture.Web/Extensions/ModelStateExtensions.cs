using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CleanArchitecture.Web.Extensions;

public static class ModelStateExtensions
{
    public static Dictionary<string, string> ToErrorDictionary(this ModelStateDictionary modelState) =>
        modelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors[0].ErrorMessage);
}

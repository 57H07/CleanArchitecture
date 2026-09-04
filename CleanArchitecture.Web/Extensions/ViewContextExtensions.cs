using Microsoft.AspNetCore.Mvc.Rendering;

namespace CleanArchitecture.Web.Extensions;

public static class ViewContextExtensions
{
    /// <summary>
    /// "page" when the request matches, otherwise null so the aria-current attribute is omitted.
    /// </summary>
    public static string? AriaCurrent(this ViewContext viewContext, string controller, string? action = null)
    {
        var currentController = viewContext.RouteData.Values["controller"] as string;
        var currentAction = viewContext.RouteData.Values["action"] as string;

        var matches = string.Equals(currentController, controller, StringComparison.OrdinalIgnoreCase)
            && (action is null || string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase));

        return matches ? "page" : null;
    }
}

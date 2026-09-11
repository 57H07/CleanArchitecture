using CleanArchitecture.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Web.Extensions;

public static class ControllerExtensions
{
    public static void NotifySuccess(this Controller controller, string message) =>
        controller.TempData[ToastMessage.SuccessKey] =
            new ToastMessage { Header = "Success", Message = message }.Serialize();

    public static void NotifyError(this Controller controller, string message) =>
        controller.TempData[ToastMessage.ErrorKey] =
            new ToastMessage { Header = "Error", Message = message }.Serialize();
}

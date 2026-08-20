using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.ViewModels.Shared;

namespace CleanArchitecture.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationViewModel model)
        {
            return View(model);
        }
    }
}

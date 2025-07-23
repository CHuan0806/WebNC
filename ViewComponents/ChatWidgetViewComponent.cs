using Microsoft.AspNetCore.Mvc;

namespace QLNhaSach1.ViewComponents
{
    public class ChatWidgetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
} 
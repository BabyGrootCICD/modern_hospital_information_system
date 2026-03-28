using Microsoft.AspNetCore.Mvc;

namespace SHIS.HisOrder.MVC.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

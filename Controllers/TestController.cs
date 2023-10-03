using Microsoft.AspNetCore.Mvc;

namespace WIL_Project.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

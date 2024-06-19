using Microsoft.AspNetCore.Mvc;

namespace giggreen.Controllers
{
    public class DashBoardController : Controller
    {
        public IActionResult DashBoard()
        {
            //if (TempData["UserName"] == null || TempData["RoleName"] == null)
            //{
   
            //    return RedirectToAction("Index", "Home");
            //}
        
            return View();
        }
    }
}

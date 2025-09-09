using Microsoft.AspNetCore.Mvc;

namespace POE_MVC_part1.Controllers
{
    public class Users : Controller
    {
        public IActionResult Lecture()
        {
            return View();
        }

        public IActionResult Academic_Manager()
        {
            return View();
        }

        public IActionResult Program_Coordinator()
        {
            return View();
        }

        public IActionResult ViewHistory()
        {
            return View();
        }
        public IActionResult ViewandPreApprove()
        {
            return View();
        }

        public IActionResult finalApproval()
        {
            return View();
        }
    }
}

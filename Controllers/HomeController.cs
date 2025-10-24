using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POE_MVC_part1.Models;

namespace POE_MVC_part1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ConnectDatabase connectdata = new ConnectDatabase();
            connectdata.GenerateTable();

            //exceptionHandling

            return RedirectToAction("Dashboard");
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult contact()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LogIn(Login user_login)
        {
            if (ModelState.IsValid)
            {
                ConnectDatabase log_in = new ConnectDatabase();

                // Use your existing method to get the employee name and number
                string employeeName = "";
                int empNum = 0;
                String lastname = "";
                bool isfound = log_in.GetUserInfo(user_login.email, user_login.password, user_login.Role, out employeeName, out empNum, out lastname);

                if (isfound)
                {
                    // Storing into session
                    HttpContext.Session.SetString("EmployeeName", employeeName);
                    HttpContext.Session.SetString("EmployeeSurname", lastname);
                    HttpContext.Session.SetInt32("EmpoyeeNum", empNum);

                    // Redirecting the login based on the roles based on role
                    switch (user_login.Role)
                    {
                        case "Lecturer":
                            return RedirectToAction("Lecture", "Users");
                        case "PC":
                            return RedirectToAction("Program_Coordinator", "Users");
                        case "AM":
                            return RedirectToAction("Academic_Manager", "Users");
                        default:
                            return RedirectToAction("Dashboard", "Home");
                    }
                }
                else
                {
                    Console.WriteLine("not found");
                }
            }

            return View(user_login);
        }



        [HttpGet]
        public IActionResult Register()
        {
       
            return View();
        }


        [HttpPost]
        public IActionResult Register(Register reg)
        {

            ConnectDatabase connect_data = new ConnectDatabase();
         
            if (!ModelState.IsValid)
            {
             
                return View(reg);
            }
            else
            {

                connect_data.Store_into_Table(reg.name, reg.LastName, reg.cellNumber, reg.email, reg.password, reg.Role);
                return RedirectToAction("LogIn");
            }


        }


        public IActionResult Dashboard()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

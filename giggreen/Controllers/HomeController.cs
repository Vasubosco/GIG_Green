using giggreen.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
namespace giggreen.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

        public HomeController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        {
            this.Environment = _environment;
        }



        //public HomeController(Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
        //{
        //    this.Environment = _environment;
        //}


        public IActionResult Index()
        {
            TempData["UserName"] = null;
            TempData["RoleName"] = null;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
          
            if (ModelState.IsValid)
            {

                List<UserDetails> users = new List<UserDetails>();

                XmlDocument doc = new XmlDocument();

                doc.Load(string.Concat(this.Environment.WebRootPath, "/Users.xml"));

                foreach (XmlNode node in doc.SelectNodes("/Users/User"))
                {
                    //Fetch the Node values and assign it to Model.
                    users.Add(new UserDetails
                    {
                        Username = node["Username"].InnerText,
                        Password = node["Password"].InnerText,
                        RoleName = node["RoleName"].InnerText
                    });
                }


                if (users.Count > 0)
                {

                    foreach (var user in users)
                    {
                        if (user.Username.ToLower().Trim() == model.Username.ToLower().Trim() && user.Password == model.Password)
                        {

             

                            TempData["Username"] =  user.Username;
                            TempData["RoleName"] = user.RoleName;
                            return RedirectToAction("DashBoard", "DashBoard");
                            
                        }
                    }
                }
        
            }
            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        public IActionResult Privacy()
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
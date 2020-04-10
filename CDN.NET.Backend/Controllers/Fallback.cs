using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace CDN.NET.Backend.Controllers
{
    public class Fallback : Controller
    {
        public IActionResult Index()
        {
            // This will be used as fallback. When we access a route that isnt the api it will fallback to the index.html file
            // and pass the routing and responsibility to Angular
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}
using System.Threading.Tasks;
using ArgonautCore.Network.Health.Models;
using Microsoft.AspNetCore.Mvc;

namespace CDN.NET.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public ActionResult<HealthStatus> GetHealthStatus()
        {
            return Ok(new HealthStatus("CDN.NET", Status.Healthy)
            {
                Description = "Content distribution network used in many Argonaut services"
            });
        }
    }
}
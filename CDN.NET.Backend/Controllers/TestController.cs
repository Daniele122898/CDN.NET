using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CDN.NET.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("authTest")]
        [Authorize]
        public IActionResult AuthTest()
        {
            return Ok("Well done");
        }
    }
}
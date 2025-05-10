using Microsoft.AspNetCore.Mvc;
using System;

namespace connect_us_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new 
            { 
                status = "healthy", 
                timestamp = DateTime.Now,
                message = "Server is operating normally."
            });
        }
    }
} 
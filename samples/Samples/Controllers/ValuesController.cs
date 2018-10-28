using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Samples.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IOptions<Settings> _settings;

        public ValuesController(IOptions<Settings> settings)
        {
            _settings = settings;
        }

        // GET api/values
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(new
            {
                Settings = _settings.Value
            });
        }
    }
}

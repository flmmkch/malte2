using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RemissionController : ControllerBase
    {
        private readonly RemissionService _remissionService;

        private readonly ILogger<RemissionController> _logger;

        public RemissionController(RemissionService remissionService, ILogger<RemissionController> logger)
        {
            _remissionService = remissionService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<Remission> Get([FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;
            return _remissionService.GetItems(dateStart, dateEnd);
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] Remission[] remissions)
        {
            await _remissionService.CreateUpdate(remissions);
        }

        [HttpDelete]
        public async Task Delete([FromBody] Remission[] remissions)
        {
            await _remissionService.Delete(remissions);
        }
    }

}
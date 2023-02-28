using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;
using Malte2.Model.Edition;
using Malte2.Extensions;
using Malte2.Model;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RemissionController : ControllerBase
    {
        private readonly RemissionService _remissionService;
        private readonly OperatorService _operatorService;

        private readonly ILogger<RemissionController> _logger;

        public RemissionController(RemissionService remissionService, OperatorService operatorService, ILogger<RemissionController> logger)
        {
            _remissionService = remissionService;
            _operatorService = operatorService;
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

        [HttpGet]
        public IAsyncEnumerable<RemissionOperationCheck> GetRemissionChecks([FromQuery(Name = "upToDate")] string? upToDateString = null, [FromQuery] long? remissionId = null)
        {
            DateTime? upToDate = upToDateString != null ? DateTime.Parse(upToDateString) : null;
            return _remissionService.GetRemissionChecks(upToDate, remissionId);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateEdition([FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;

            string title = "";
            if (dateStart.HasValue && dateEnd.HasValue && dateStart.Value.Month == dateEnd.Value.Month) {
                title = $"{dateStart.Value.ToString("MMMM yyyy")}";
            }

            var remissions = await _remissionService.GetItems(dateStart, dateEnd).ToListAsync();
            var operators = await _operatorService.GetItems().BuildDictionaryById();

            XlsxEdition? xlsxEdition = new Malte2.Model.Accounting.Edition.RemissionEdition(title, remissions, operators);

            var editionStream = xlsxEdition.ProduceXlsx();
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"Dépôts bancaires {title}.xlsx";
            return File(editionStream, contentType, fileName);
        }
    }

}
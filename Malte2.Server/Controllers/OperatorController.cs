using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OperatorController : ControllerBase
    {
        private readonly OperatorService _operatorService;

        private readonly ILogger<OperatorController> _logger;

        public OperatorController(OperatorService operatorService, ILogger<OperatorController> logger)
        {
            _operatorService = operatorService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<Operator> Get()
        {
            return _operatorService.GetOperators();
        }

        [HttpGet]
        public IActionResult GenerateEdition([FromQuery] long id)
        {
            var editionStream = Malte2.Model.Accounting.Edition.OperatorEdition.CreateEdition();
            string contentType = "application/pdf";
            string fileName = "Opérateurs.pdf";
            return File(editionStream, contentType, fileName);
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] Operator[] operators)
        {
            await _operatorService.CreateUpdateOperators(operators);
        }

        [HttpDelete]
        public async Task Delete([FromBody] Operator[] operators)
        {
            await _operatorService.DeleteOperators(operators);
        }
    }

}
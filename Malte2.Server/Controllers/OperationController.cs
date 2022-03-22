using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OperationController : ControllerBase
    {
        private readonly OperationService _operationService;

        private readonly ILogger<OperationController> _logger;

        public OperationController(OperationService operationService, ILogger<OperationController> logger)
        {
            _operationService = operationService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<Operation> Get()
        {
            return _operationService.GetItems();
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] Operation[] operations)
        {
            await _operationService.CreateUpdate(operations);
        }

        [HttpDelete]
        public async Task Delete([FromBody] Operation[] operations)
        {
            await _operationService.Delete(operations);
        }
    }

}
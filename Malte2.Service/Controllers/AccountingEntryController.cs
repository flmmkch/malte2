using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountingEntryController : ControllerBase
    {
        private readonly AccountingEntryService _accountingEntryService;

        private readonly ILogger<OperatorController> _logger;

        public AccountingEntryController(AccountingEntryService accountingEntryService, ILogger<OperatorController> logger)
        {
            _accountingEntryService = accountingEntryService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<AccountingEntry> Get()
        {
            return _accountingEntryService.GetItems();
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] AccountingEntry[] accountingEntries)
        {
            await _accountingEntryService.CreateUpdate(accountingEntries);
        }

        [HttpDelete]
        public async Task Delete([FromBody] AccountingEntry[] accountingEntries)
        {
            await _accountingEntryService.Delete(accountingEntries);
        }
    }

}
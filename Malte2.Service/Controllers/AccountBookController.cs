using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountBookController : ControllerBase
    {
        private readonly AccountBookService _accountBookService;

        private readonly ILogger<AccountBookController> _logger;

        public AccountBookController(AccountBookService accountBookService, ILogger<AccountBookController> logger)
        {
            _accountBookService = accountBookService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<AccountBook> Get()
        {
            return _accountBookService.GetItems();
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] AccountBook[] accountBooks)
        {
            await _accountBookService.CreateUpdate(accountBooks);
        }

        [HttpDelete]
        public async Task Delete([FromBody] AccountBook[] accountBooks)
        {
            await _accountBookService.Delete(accountBooks);
        }
    }

}
using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountingCategoryController : ControllerBase
    {
        private readonly AccountingCategoryService _accountingCategoryService;

        private readonly ILogger<OperatorController> _logger;

        public AccountingCategoryController(AccountingCategoryService accountingCategoryService, ILogger<OperatorController> logger)
        {
            _accountingCategoryService = accountingCategoryService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<AccountingCategory> Get()
        {
            return _accountingCategoryService.GetItems();
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] AccountingCategory[] accountingCategories)
        {
            await _accountingCategoryService.CreateUpdate(accountingCategories);
        }

        [HttpDelete]
        public async Task Delete([FromBody] AccountingCategory[] accountingCategories)
        {
            await _accountingCategoryService.Delete(accountingCategories);
        }
    }

}
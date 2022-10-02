using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;
using Malte2.Extensions;
using CsvHelper;
using Malte2.Model;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OperationController : ControllerBase
    {
        private readonly OperationService _operationService;
        private readonly AccountBookService _accountBookService;
        private readonly AccountingEntryService _accountingEntryService;
        private readonly AccountingCategoryService _accountingCategoryService;

        private readonly ILogger<OperationController> _logger;

        public OperationController(
            OperationService operationService,
            AccountBookService accountBookService,
            AccountingEntryService accountingEntryService,
            AccountingCategoryService accountingCategoryService,
            ILogger<OperationController> logger
            )
        {
            _operationService = operationService;
            _accountBookService = accountBookService;
            _accountingEntryService = accountingEntryService;
            _accountingCategoryService = accountingCategoryService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<Operation> Get([FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;
            return _operationService.GetItems(dateStart, dateEnd);
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

        [HttpGet]
        public async Task<IActionResult> GeneratePdf([FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null, [FromQuery(Name = "paymentMethod")] PaymentMethod? paymentMethod = null, [FromQuery(Name = "accountBook")] long? accountBookId = null, [FromQuery(Name = "accountingEntry")] long? accountingEntryId = null, [FromQuery(Name = "category")] long? categoryId = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;
            List<Operation> operations = await _operationService.GetItems(dateStart, dateEnd, paymentMethod, accountBookId, accountingEntryId, categoryId).ToListAsync();
            Dictionary<long, AccountBook> accountBooks = await _accountBookService.GetItems().BuildDictionaryById();
            Dictionary<long, AccountingEntry> accountingEntries = await _accountingEntryService.GetItems().BuildDictionaryById();
            Dictionary<long, AccountingCategory> categories = await _accountingCategoryService.GetItems().BuildDictionaryById();
            string title = "Opérations";
            if (dateStart.HasValue && dateEnd.HasValue && dateStart.Value.Month == dateEnd.Value.Month) {
                title = $"Opérations : {dateStart.Value.ToString("MMMM yyyy")}";
            }
            var editionDocument = Malte2.Model.Accounting.Edition.OperationEdition.CreateOperationsDocument(title, operations, accountBooks, accountingEntries, categories);
            var editionStream = Malte2.Model.Accounting.Edition.OperationEdition.RenderDocumentPdf(editionDocument);
            string contentType = "application/pdf";
            string fileName = "Édition.pdf";
            return File(editionStream, contentType, fileName);
        }

        [HttpGet]
        public IAsyncEnumerable<string> GetLabels()
        {
            return _operationService.GetLabels();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCsv([FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null, [FromQuery(Name = "paymentMethod")] PaymentMethod? paymentMethod = null, [FromQuery(Name = "accountBook")] long? accountBookId = null, [FromQuery(Name = "accountingEntry")] long? accountingEntryId = null, [FromQuery(Name = "category")] long? categoryId = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;
            List<OperationEditionCsvLine> operationsCsvLines = await _operationService.GetEditionItems(dateStart, dateEnd, paymentMethod, accountBookId, accountingEntryId, categoryId).ToListAsync();
            MemoryStream csvStream = new MemoryStream();
            using (var writer = new StreamWriter(csvStream, System.Text.Encoding.UTF8, 2048, true))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
            {
                csv.WriteRecords(operationsCsvLines);
            }
            csvStream.Seek(0, SeekOrigin.Begin);
            string contentType = "text/csv";
            string fileName = "Opérations.csv";
            return File(csvStream, contentType, fileName);
        }
    }

}
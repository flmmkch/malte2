using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;
using Malte2.Extensions;
using CsvHelper;
using Malte2.Model;
using Malte2.Model.Edition;

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
        private readonly BoarderService _boarderService;

        private readonly ILogger<OperationController> _logger;

        public OperationController(
            OperationService operationService,
            AccountBookService accountBookService,
            AccountingEntryService accountingEntryService,
            AccountingCategoryService accountingCategoryService,
            BoarderService boarderService,
            ILogger<OperationController> logger
            )
        {
            _operationService = operationService;
            _accountBookService = accountBookService;
            _accountingEntryService = accountingEntryService;
            _accountingCategoryService = accountingCategoryService;
            _boarderService = boarderService;
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
        public async Task<IActionResult> GenerateEdition([FromQuery(Name = "editionType")] OperationEditionType editionType = OperationEditionType.ByAccountBook, [FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null, [FromQuery(Name = "paymentMethod")] PaymentMethod? paymentMethod = null, [FromQuery(Name = "accountBook")] long? accountBookId = null, [FromQuery(Name = "accountingEntry")] long? accountingEntryId = null, [FromQuery(Name = "category")] long? categoryId = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;

            string title = "";
            if (dateStart.HasValue && dateEnd.HasValue && dateStart.Value.Month == dateEnd.Value.Month) {
                title = $"{dateStart.Value.ToString("MMMM yyyy")}";
            }

            var operations = await _operationService.GetItems(dateStart, dateEnd, paymentMethod, accountBookId, accountingEntryId, categoryId).ToListAsync();
            var accountBooks = await _accountBookService.GetItems().BuildDictionaryById();
            var accountingEntries = await _accountingEntryService.GetItems().BuildDictionaryById();
            var accountingCategories = await _accountingCategoryService.GetItems().BuildDictionaryById();
            var boarderListItems = await _boarderService.GetItemList().BuildDictionaryById();
            Dictionary<long, string> boarderNames = new Dictionary<long, string>(boarderListItems.Select((idBoarderListItemPair) => new KeyValuePair<long, string>(idBoarderListItemPair.Key, idBoarderListItemPair.Value.Name)));

            XlsxEdition? xlsxEdition = null;

            switch (editionType)
            {
                case OperationEditionType.ByAccountBook:
                    xlsxEdition = new Malte2.Model.Accounting.Edition.OperationByAccountBookXlsxEdition(title, operations, accountBooks, accountingEntries, accountingCategories, boarderNames);
                    break;
                case OperationEditionType.ByAccountBookAndPaymentMethod:
                    xlsxEdition = new Malte2.Model.Accounting.Edition.OperationByAccountBookAndPaymentMethodXlsxEdition(title, operations, accountBooks, accountingEntries, accountingCategories, boarderNames);
                    break;
                case OperationEditionType.ByAccountBookAndCategory:
                    xlsxEdition = new Malte2.Model.Accounting.Edition.OperationByAccountBookAndCategoryXlsxEdition(title, operations, accountBooks, accountingEntries, accountingCategories, boarderNames);
                    break;
            }
            
            if (xlsxEdition != null) {
                var editionStream = xlsxEdition.ProduceXlsx();
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = $"Opérations {title}.xlsx";
                return File(editionStream, contentType, fileName);
            }

            return NotFound();
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
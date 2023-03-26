using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model;
using Malte2.Model.Accounting;
using Malte2.Model.Boarding;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BoarderController : ControllerBase
    {
        private readonly BoarderService _boarderService;
        private readonly OperationService _operationService;
        private readonly AccountingEntryService _entryService;

        private readonly ILogger<BoarderController> _logger;

        public BoarderController(BoarderService boarderService, OperationService operationService, AccountingEntryService entryService, ILogger<BoarderController> logger)
        {
            _boarderService = boarderService;
            _operationService = operationService;
            _entryService = entryService;
            _logger = logger;
        }

        [HttpGet]
        public async IAsyncEnumerable<BoarderListItemResponse> List([FromQuery(Name = "occupancyDate")] string? roomOccupancyDateTimeString = null, bool balances = false)
        {
            DateTime roomOccupancyDateTime = roomOccupancyDateTimeString != null ? DateTime.Parse(roomOccupancyDateTimeString) : DateTime.Now;
            var boarderListItemsAsync = _boarderService.GetItemList(roomOccupancyDateTime);
            if (balances) {
                Dictionary<long, AccountingEntry> entryDictionary = await _entryService.GetItems().BuildDictionaryById();
                await foreach (BoarderListItemResponse boarderListItem in boarderListItemsAsync) {
                    Amount balanceAmount = new Amount();
                    await foreach (Operation operation in _operationService.GetItems(null, roomOccupancyDateTime, null, null, null, null, boarderListItem.BoarderId)) {
                        if (entryDictionary.TryGetValue(operation.AccountingEntryId, out AccountingEntry? entry)) {
                            switch (entry.EntryType) {
                                case AccountingEntryType.Expense:
                                    balanceAmount -= operation.Amount;
                                    break;
                                case AccountingEntryType.Revenue:
                                    balanceAmount += operation.Amount;
                                    break;
                            }
                        }
                    }
                    boarderListItem.Balance = balanceAmount;
                    yield return boarderListItem;
                }
            }
            else {
                await foreach (BoarderListItemResponse boarderListItem in boarderListItemsAsync) {
                    yield return boarderListItem;
                }
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<Boarder> Details([FromRoute] long id)
        {
            Boarder? boarder = await _boarderService.GetDetails(id);
            if (boarder != null) {
                return boarder!;
            }
            throw new BadHttpRequestException("Not Found", 404);
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] Boarder[] boarders)
        {
            await _boarderService.CreateUpdate(boarders);
        }

        [HttpDelete]
        public async Task Delete([FromBody] Boarder[] boarders)
        {
            await _boarderService.Delete(boarders);
        }
    }

}
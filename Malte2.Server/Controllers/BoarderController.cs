using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Boarding;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BoarderController : ControllerBase
    {
        private readonly BoarderService _boarderService;

        private readonly ILogger<BoarderController> _logger;

        public BoarderController(BoarderService boarderService, ILogger<BoarderController> logger)
        {
            _boarderService = boarderService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<BoarderListItemResponse> List([FromQuery(Name = "occupancyDate")] string? roomOccupancyDateTimeString = null)
        {
            DateTime roomOccupancyDateTime = DateTime.Now;
            if (roomOccupancyDateTimeString != null)
            {
                roomOccupancyDateTime = DateTime.Parse(roomOccupancyDateTimeString);
            }
            return _boarderService.GetItemList(roomOccupancyDateTime);
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
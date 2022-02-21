using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Boarding;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BoardingRoomController : ControllerBase
    {
        private readonly BoardingRoomService _boardingRoomService;

        private readonly ILogger<BoardingRoomController> _logger;

        public BoardingRoomController(BoardingRoomService boardingRoomService, ILogger<BoardingRoomController> logger)
        {
            _boardingRoomService = boardingRoomService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<BoardingRoom> Get()
        {
            return _boardingRoomService.GetItems();
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] BoardingRoom[] boardingRooms)
        {
            await _boardingRoomService.CreateUpdate(boardingRooms);
        }

        [HttpDelete]
        public async Task Delete([FromBody] BoardingRoom[] boardingRooms)
        {
            await _boardingRoomService.Delete(boardingRooms);
        }
    }

}
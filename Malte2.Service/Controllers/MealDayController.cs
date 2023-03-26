using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.MealDay;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MealDayController : ControllerBase
    {
        private readonly MealDayService _mealDayService;

        private readonly ILogger<MealDayController> _logger;

        public MealDayController(MealDayService mealDayService, ILogger<MealDayController> logger)
        {
            _mealDayService = mealDayService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<MealDay> Get([FromQuery(Name = "dateStart")] string? dateStartString = null, [FromQuery(Name = "dateEnd")] string? dateEndString = null)
        {
            DateTime? dateStart = dateStartString != null ? DateTime.Parse(dateStartString) : null;
            DateTime? dateEnd = dateEndString != null ? DateTime.Parse(dateEndString) : null;
            return _mealDayService.Get(dateStart, dateEnd);
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] MealDay[] mealDays)
        {
            await _mealDayService.CreateUpdate(mealDays);
        }

        [HttpDelete]
        public async Task Delete([FromBody] MealDay[] mealDays)
        {
            await _mealDayService.Delete(mealDays);
        }
    }

}
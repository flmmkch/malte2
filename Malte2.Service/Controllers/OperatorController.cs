﻿using Microsoft.AspNetCore.Mvc;
using Malte2.Services;
using Malte2.Model.Accounting;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OperatorController : ControllerBase
    {
        private readonly OperatorService _operatorService;

        private readonly ILogger<OperatorController> _logger;

        public OperatorController(OperatorService operatorService, ILogger<OperatorController> logger)
        {
            _operatorService = operatorService;
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<Operator> Get([FromQuery] bool onlyEnabled = false)
        {
            return _operatorService.GetItems(onlyEnabled);
        }

        [HttpPost]
        public async Task CreateUpdate([FromBody] Operator[] operators)
        {
            await _operatorService.CreateUpdate(operators);
        }

        [HttpDelete]
        public async Task Delete([FromBody] Operator[] operators)
        {
            await _operatorService.Delete(operators);
        }
    }

}
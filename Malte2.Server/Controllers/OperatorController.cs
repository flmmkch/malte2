using Microsoft.AspNetCore.Mvc;
using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Controllers
{

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OperatorController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<OperatorController> _logger;

        public OperatorController(DatabaseContext databaseContext, ILogger<OperatorController> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        [HttpGet]
        public async IAsyncEnumerable<Operator> Get()
        {
            using (var command = new SQLiteCommand("SELECT operator_id, name FROM operator", _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string? operatorName = reader["name"]! as string;
                        Operator oper = new Operator(operatorName!);
                        oper.Id = reader["operator_id"]! as long?;
                        yield return oper;
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult GenerateEdition([FromQuery] long id)
        {
            var editionStream = Malte2.Model.Accounting.Edition.OperatorEdition.CreateEdition();
            string contentType = "application/pdf";
            string fileName = "Opérateurs.pdf";
            return File(editionStream, contentType, fileName);
        }

        [HttpPost]
        public bool CreateUpdate([FromBody] Operator[] operators)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Operator oper in operators)
                {
                    if (oper.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("", _databaseContext.Connection, transaction))
                        {

                        }
                    }
                    else
                    {
                        using (var command = new SQLiteCommand("INSERT INTO operator(name) VALUES (:name)", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("name", oper.Name);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
            }
            return true;
        }

        [HttpDelete]
        public bool Delete(Operator[] operators)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Operator oper in operators)
                {
                    if (oper.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM operator WHERE operator_id = :operatorId", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("operatorId", oper.Id.Value);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                transaction.Commit();
            }
            return true;
        }
    }


}
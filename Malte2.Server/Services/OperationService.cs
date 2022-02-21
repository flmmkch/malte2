using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class OperationService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<OperationService> _logger;

        public OperationService(DatabaseContext databaseContext, ILogger<OperationService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<Operation> GetItems()
        {
            string commandText = $"SELECT operation_id, label FROM operation ORDER BY operation_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Operation operation = new Operation
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("operation_id")),
                            OperatorId = reader.GetInt64(reader.GetOrdinal("operator_id")),
                            AccountingEntryId = reader.GetInt64(reader.GetOrdinal("accounting_entry_id")),
                            // TODO parse using global culture info
                            OperationDateTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))!),
                            Label = reader.GetString(reader.GetOrdinal("label")),
                            BoarderId = reader.GetInt64(reader.GetOrdinal("boarder_id")),
                            PaymentMethod = (PaymentMethod) Enum.ToObject(typeof(PaymentMethod), reader.GetInt64(reader.GetOrdinal("payment_method"))),
                            PaymentMethodInfo = reader.GetString(reader.GetOrdinal("payment_method_info")),
                            AccountBookId = reader.GetInt64(reader.GetOrdinal("account_book_id")),
                        };
                        yield return operation;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<Operation> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Operation operation in accountingEntries)
                {
                    if (operation.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand(@"UPDATE operation
                        SET label = :label,
                        WHERE operation_id = :operation_id", _databaseContext.Connection, transaction))
                        {
                            MapOperationParameters(operation, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        using (var command = new SQLiteCommand("INSERT INTO operation(label) VALUES (:label)", _databaseContext.Connection, transaction))
                        {
                            MapOperationParameters(operation, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }

        private void MapOperationParameters(Operation operation, SQLiteParameterCollection parameters)
        {
            if (operation.Id.HasValue)
            {
                parameters.AddWithValue("operation_id", operation.Id!);
            }
            parameters.AddWithValue("label", operation.Label);
        }

        public async Task Delete(IEnumerable<Operation> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Operation operation in accountingEntries)
                {
                    if (operation.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM operation WHERE operation_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", operation.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
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

        public async IAsyncEnumerable<Operation> GetItems(DateTime? dateStart, DateTime? dateEnd)
        {
            string commandText = @"SELECT
            operation_id,
            operator_id,
            accounting_entry_id,
            category_id,
            date,
            label,
            boarder_id,
            payment_method,
            check_number,
            transfer_number,
            card_ticket_number,
            account_book_id,
            amount
            FROM operation
            WHERE (:date_start IS NULL OR :date_start <= date) AND (:date_end IS NULL OR :date_end >= date)
            ORDER BY operation_id ASC;";
            commandText = commandText + @" ORDER BY operation_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                command.Parameters.AddWithValue("date_start", DateTimeDatabaseUtils.GetStringFromNullableDate(dateStart));
                command.Parameters.AddWithValue("date_end", DateTimeDatabaseUtils.GetStringFromNullableDate(dateEnd));
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Operation operation = new Operation
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("operation_id")),
                            OperatorId = reader.GetInt64(reader.GetOrdinal("operator_id")),
                            AccountingEntryId = reader.GetInt64(reader.GetOrdinal("accounting_entry_id")),
                            AccountingCategoryId = DatabaseValueUtils.GetNullableInt64FromReader(reader, reader.GetOrdinal("category_id")),
                            OperationDateTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))!),
                            Label = reader.GetString(reader.GetOrdinal("label")),
                            BoarderId = DatabaseValueUtils.GetNullableInt64FromReader(reader, reader.GetOrdinal("boarder_id")),
                            PaymentMethod = (PaymentMethod) Enum.ToObject(typeof(PaymentMethod), reader.GetInt64(reader.GetOrdinal("payment_method"))),
                            CheckNumber = DatabaseValueUtils.GetNullableInt64FromReader(reader, reader.GetOrdinal("check_number")),
                            TransferNumber = DatabaseValueUtils.GetNullableInt64FromReader(reader, reader.GetOrdinal("transfer_number")),
                            CardTicketNumber = DatabaseValueUtils.GetNullableInt64FromReader(reader, reader.GetOrdinal("card_ticket_number")),
                            AccountBookId = reader.GetInt64(reader.GetOrdinal("account_book_id")),
                            Amount = new Amount(reader.GetInt64(reader.GetOrdinal("amount"))),
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
                    string commandSql;
                    if (operation.Id.HasValue)
                    {
                        commandSql = @"UPDATE operation
                        SET operator_id = :operator_id,
                        accounting_entry_id = :accounting_entry_id,
                        category_id = :category_id,
                        date = :date,
                        label = :label,
                        boarder_id = :boarder_id,
                        payment_method = :payment_method,
                        check_number = :check_number,
                        card_ticket_number = :card_ticket_number,
                        transfer_number = :transfer_number,
                        account_book_id = :account_book_id,
                        amount = :amount
                        WHERE operation_id = :operation_id";
                    }
                    else
                    {
                        commandSql = @"INSERT INTO operation(
                            operator_id,
                            accounting_entry_id,
                            category_id,
                            date,
                            label,
                            boarder_id,
                            payment_method,
                            check_number,
                            card_ticket_number,
                            transfer_number,
                            account_book_id,
                            amount
                            ) VALUES (
                            :operator_id,
                            :accounting_entry_id,
                            :category_id,
                            :date,
                            :label,
                            :boarder_id,
                            :payment_method,
                            :check_number,
                            :card_ticket_number,
                            :transfer_number,
                            :account_book_id,
                            :amount
                            )";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (operation.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("operation_id", operation.Id!);
                        }
                        command.Parameters.AddWithValue("operator_id", operation.OperatorId);
                        command.Parameters.AddWithValue("accounting_entry_id", operation.AccountingEntryId);
                        command.Parameters.AddWithValue("category_id", operation.AccountingCategoryId);
                        command.Parameters.AddWithValue("date", DateTimeDatabaseUtils.GetStringFromDate(operation.OperationDateTime));
                        command.Parameters.AddWithValue("label", operation.Label);
                        command.Parameters.AddWithValue("boarder_id", operation.BoarderId);
                        command.Parameters.AddWithValue("payment_method", operation.PaymentMethod);
                        command.Parameters.AddWithValue("check_number", operation.CheckNumber);
                        command.Parameters.AddWithValue("card_ticket_number", operation.CardTicketNumber);
                        command.Parameters.AddWithValue("transfer_number", operation.TransferNumber);
                        command.Parameters.AddWithValue("account_book_id", operation.AccountBookId);
                        command.Parameters.AddWithValue("amount", operation.Amount.GetLong());
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
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

        public async IAsyncEnumerable<string> GetLabels()
        {
            string commandText = @"SELECT DISTINCT label FROM operation;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        yield return reader.GetString(reader.GetOrdinal("label"));
                    }
                }
            }
        }
    }
}
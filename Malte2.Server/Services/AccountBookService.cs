using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class AccountBookService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<AccountBookService> _logger;

        public AccountBookService(DatabaseContext databaseContext, ILogger<AccountBookService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<AccountBook> GetItems()
        {
            string commandText = $"SELECT account_book_id, label FROM account_book ORDER BY account_book_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AccountBook accountBook = new AccountBook
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("account_book_id")),
                            Label = reader.GetString(reader.GetOrdinal("label")),
                        };
                        yield return accountBook;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<AccountBook> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (AccountBook accountBook in accountingEntries)
                {
                    string commandSql;
                    if (accountBook.Id.HasValue)
                    {
                        commandSql = @"UPDATE account_book
                        SET label = :label
                        WHERE account_book_id = :account_book_id";
                    }
                    else
                    {
                        commandSql = "INSERT INTO account_book(label) VALUES (:label)";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (accountBook.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("account_book_id", accountBook.Id!);
                        }
                        command.Parameters.AddWithValue("label", accountBook.Label);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
        }

        public async Task Delete(IEnumerable<AccountBook> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (AccountBook accountBook in accountingEntries)
                {
                    if (accountBook.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM account_book WHERE account_book_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", accountBook.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
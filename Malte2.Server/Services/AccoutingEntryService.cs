using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class AccountingEntryService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<AccountingEntryService> _logger;

        public AccountingEntryService(DatabaseContext databaseContext, ILogger<AccountingEntryService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<AccountingEntry> GetItems()
        {
            string commandText = $"SELECT accounting_entry_id, label, has_boarder, accounting_entry_type FROM accounting_entry ORDER BY accounting_entry_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AccountingEntry accountingEntry = new AccountingEntry
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("accounting_entry_id")),
                            Label = reader.GetString(reader.GetOrdinal("label")),
                            HasBoarder = reader.GetBoolean(reader.GetOrdinal("has_boarder")),
                            EntryType = (AccountingEntryType) Enum.ToObject(typeof(AccountingEntryType), reader.GetByte(reader.GetOrdinal("accounting_entry_type"))),
                        };
                        yield return accountingEntry;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<AccountingEntry> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (AccountingEntry accountingEntry in accountingEntries)
                {
                    string commandSql;
                    if (accountingEntry.Id.HasValue)
                    {
                        commandSql = @"UPDATE accounting_entry
                        SET label = :label,
                        has_boarder = :has_boarder,
                        accounting_entry_type = :accounting_entry_type
                        WHERE accounting_entry_id = :accounting_entry_id";
                    }
                    else
                    {
                        commandSql = "INSERT INTO accounting_entry(label, has_boarder, accounting_entry_type) VALUES (:label, :has_boarder, :accounting_entry_type)";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (accountingEntry.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("accounting_entry_id", accountingEntry.Id!);
                        }
                        command.Parameters.AddWithValue("label", accountingEntry.Label);
                        command.Parameters.AddWithValue("has_boarder", accountingEntry.HasBoarder);
                        command.Parameters.AddWithValue("accounting_entry_type", accountingEntry.EntryType);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
        }

        public async Task Delete(IEnumerable<AccountingEntry> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (AccountingEntry accountingEntry in accountingEntries)
                {
                    if (accountingEntry.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM accounting_entry WHERE accounting_entry_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", accountingEntry.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
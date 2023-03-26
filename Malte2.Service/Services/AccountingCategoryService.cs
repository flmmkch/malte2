using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class AccountingCategoryService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<AccountingCategoryService> _logger;

        public AccountingCategoryService(DatabaseContext databaseContext, ILogger<AccountingCategoryService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<AccountingCategory> GetItems()
        {
            string commandText = $"SELECT accounting_category_id, label, accounting_entry_id FROM accounting_category ORDER BY accounting_category_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AccountingCategory accountingCategory = new AccountingCategory
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("accounting_category_id")),
                            Label = reader.GetString(reader.GetOrdinal("label")),
                            AccountingEntryId = DatabaseValueUtils.GetNullableInt64FromReader(reader, reader.GetOrdinal("accounting_entry_id")),
                        };
                        yield return accountingCategory;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<AccountingCategory> accountingCategories)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (AccountingCategory accountingCategory in accountingCategories)
                {
                    string commandSql;
                    if (accountingCategory.Id.HasValue)
                    {
                        commandSql = @"UPDATE accounting_category
                        SET label = :label,
                        accounting_entry_id = :accounting_entry_id
                        WHERE accounting_category_id = :accounting_category_id";
                    }
                    else
                    {
                        commandSql = "INSERT INTO accounting_category(label, accounting_entry_id) VALUES (:label, :accounting_entry_id)";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (accountingCategory.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("accounting_category_id", accountingCategory.Id!);
                        }
                        command.Parameters.AddWithValue("label", accountingCategory.Label);
                        command.Parameters.AddWithValue("accounting_entry_id", accountingCategory.AccountingEntryId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
        }

        public async Task Delete(IEnumerable<AccountingCategory> accountingCategories)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (AccountingCategory accountingCategory in accountingCategories)
                {
                    if (accountingCategory.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM accounting_category WHERE accounting_category_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", accountingCategory.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
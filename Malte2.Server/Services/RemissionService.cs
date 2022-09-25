using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class RemissionService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<RemissionService> _logger;

        public RemissionService(DatabaseContext databaseContext, ILogger<RemissionService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<Remission> GetItems(DateTime? dateStart, DateTime? dateEnd)
        {
            string commandText = @"SELECT
            remission_id,
            date,
            operator_id,
            cash_deposit_01c,
            cash_deposit_02c,
            cash_deposit_05c,
            cash_deposit_10c,
            cash_deposit_20c,
            cash_deposit_50c,
            cash_deposit_001e,
            cash_deposit_002e,
            cash_deposit_005e,
            cash_deposit_010e,
            cash_deposit_020e,
            cash_deposit_050e,
            cash_deposit_100e,
            cash_deposit_200e,
            cash_deposit_500e,
            notes
            FROM remission
            WHERE (:date_start IS NULL OR :date_start <= date) AND (:date_end IS NULL OR :date_end >= date)
            ORDER BY date, remission_id DESC;";
            string selectCheckCommandText = @"SELECT check_number, amount FROM remission_check WHERE remission_id = :remission_id";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            using (var selectCheckCommand = new SQLiteCommand(selectCheckCommandText, _databaseContext.Connection))
            {
                command.Parameters.AddWithValue("date_start", DateTimeDatabaseUtils.GetStringFromNullableDate(dateStart));
                command.Parameters.AddWithValue("date_end", DateTimeDatabaseUtils.GetStringFromNullableDate(dateEnd));
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Remission remission = new Remission
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("remission_id")),
                            OperatorId = reader.GetInt64(reader.GetOrdinal("operator_id")),
                            Notes = reader.GetString(reader.GetOrdinal("notes")),
                            DateTime = DateTimeDatabaseUtils.GetDateFromReader(reader, reader.GetOrdinal("date")),
                        };
                        foreach (Remission.CashValue cashValue in Enum.GetValues<Remission.CashValue>()) {
                            string? columnName = Remission.GetCashValueSqlColumnName(cashValue);
                            if (columnName != null) {
                                int count = reader.GetInt32(reader.GetOrdinal(columnName));
                                if (count > 0) {
                                    remission.CashDeposits.Add(new Remission.CashDeposit { Value = cashValue, Count = count });
                                }
                            }
                            else {
                                throw new Exception($"Column name not found for {cashValue.ToString()}");
                            }
                        }
                        selectCheckCommand.Parameters.Clear();
                        selectCheckCommand.Parameters.AddWithValue("remission_id", remission.Id);
                        using (var selectCheckReader = await selectCheckCommand.ExecuteReaderAsync()) {
                            while (await selectCheckReader.ReadAsync()) {
                                Remission.CheckRemission check = new Remission.CheckRemission {
                                    Amount = new Amount(selectCheckReader.GetInt64(selectCheckReader.GetOrdinal("amount"))),
                                    CheckNumber = DatabaseValueUtils.GetNullableUint64FromReader(selectCheckReader, selectCheckReader.GetOrdinal("check_number")),
                                };
                                remission.CheckRemissions.Add(check);
                            }
                        }
                        yield return remission;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<Remission> remissions)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Remission remission in remissions)
                {
                    // remission details and cash
                    string commandSql;
                    if (remission.Id.HasValue)
                    {
                        commandSql = @"UPDATE remission
                        SET date = :date,
                            operator_id = :operator_id,
                            notes = :notes,
                            cash_deposit_01c = :cash_deposit_01c,
                            cash_deposit_02c = :cash_deposit_02c,
                            cash_deposit_05c = :cash_deposit_05c,
                            cash_deposit_10c = :cash_deposit_10c,
                            cash_deposit_20c = :cash_deposit_20c,
                            cash_deposit_50c = :cash_deposit_50c,
                            cash_deposit_001e = :cash_deposit_001e,
                            cash_deposit_002e = :cash_deposit_002e,
                            cash_deposit_005e = :cash_deposit_005e,
                            cash_deposit_010e = :cash_deposit_010e,
                            cash_deposit_020e = :cash_deposit_020e,
                            cash_deposit_050e = :cash_deposit_050e,
                            cash_deposit_100e = :cash_deposit_100e,
                            cash_deposit_200e = :cash_deposit_200e,
                            cash_deposit_500e = :cash_deposit_500e
                        WHERE remission_id = :remission_id";
                    }
                    else
                    {
                        commandSql = "INSERT INTO remission(date, operator_id, notes, cash_deposit_01c, cash_deposit_02c, cash_deposit_05c, cash_deposit_10c, cash_deposit_20c, cash_deposit_50c, cash_deposit_001e, cash_deposit_002e, cash_deposit_005e, cash_deposit_010e, cash_deposit_020e, cash_deposit_050e, cash_deposit_100e, cash_deposit_200e, cash_deposit_500e) VALUES (:date, :operator_id, :notes, :cash_deposit_01c, :cash_deposit_02c, :cash_deposit_05c, :cash_deposit_10c, :cash_deposit_20c, :cash_deposit_50c, :cash_deposit_001e, :cash_deposit_002e, :cash_deposit_005e, :cash_deposit_010e, :cash_deposit_020e, :cash_deposit_050e, :cash_deposit_100e, :cash_deposit_200e, :cash_deposit_500e)";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (remission.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("remission_id", remission.Id!);
                        }
                        command.Parameters.AddWithValue("date", DateTimeDatabaseUtils.GetStringFromDate(remission.DateTime));
                        command.Parameters.AddWithValue("operator_id", remission.OperatorId);
                        command.Parameters.AddWithValue("notes", remission.Notes);
                        foreach (Remission.CashValue cashValue in (Remission.CashValue[])Enum.GetValues(typeof(Remission.CashValue)))
                        {
                            string? cashValueColumnString = Remission.GetCashValueSqlColumnName(cashValue);
                            if (cashValueColumnString == null)
                            {
                                throw new Exception($"No sql column defined for {cashValue}");
                            }
                            int count = remission.CashDeposits.Where(cd => cd.Value == cashValue).Select(cd => cd.Count).FirstOrDefault();
                            command.Parameters.AddWithValue(cashValueColumnString!, count);
                        }
                        await command.ExecuteNonQueryAsync();
                    }
                    // remission checks
                    long remissionId;
                    // find the actual remission id
                    if (remission.Id.HasValue) {
                        remissionId = remission.Id.Value;
                    }
                    else {
                        // use the last row insert
                        commandSql = "SELECT remission_id FROM remission WHERE remission.rowid = :remission_row_id";
                        using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("remission_row_id", _databaseContext.Connection.LastInsertRowId);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    remissionId = reader.GetInt64(reader.GetOrdinal("remission_id"));
                                }
                                else
                                {
                                    throw new Exception("Failed to find newly inserted remission id");
                                }
                            }
                        }
                    }
                    // clear the existing checks
                    if (remission.Id.HasValue)
                    {
                        commandSql = "DELETE FROM remission_check WHERE remission_check.remission_id = :remission_id";
                        using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("remission_id", remissionId);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    // add the checks
                    commandSql = "INSERT INTO remission_check(remission_id, check_number, amount) VALUES (:remission_id, :check_number, :amount)";
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        foreach (Remission.CheckRemission check in remission.CheckRemissions) {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("remission_id", remissionId);
                            command.Parameters.AddWithValue("check_number", check.CheckNumber);
                            command.Parameters.AddWithValue("amount", check.Amount.GetLong());
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }

        public async Task Delete(IEnumerable<Remission> remissions)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Remission oper in remissions)
                {
                    if (oper.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM remission_check WHERE remission_id = :remissionId", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("remissionId", oper.Id.Value);
                            await command.ExecuteNonQueryAsync();
                        }
                        using (var command = new SQLiteCommand("DELETE FROM remission WHERE remission_id = :remissionId", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("remissionId", oper.Id.Value);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
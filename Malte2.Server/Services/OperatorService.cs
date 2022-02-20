using Malte2.Database;
using Malte2.Model.Accounting;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class OperatorService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<OperatorService> _logger;

        public OperatorService(DatabaseContext databaseContext, ILogger<OperatorService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<Operator> GetItems(bool onlyEnabled = false)
        {
            string whereFilter = "";
            if (onlyEnabled) {
                whereFilter = "WHERE enabled = 1";
            }
            string commandText = $"SELECT operator_id, name, phone_number, enabled FROM operator {whereFilter} ORDER BY operator_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string operatorName = reader.GetString(reader.GetOrdinal("name"));
                        Operator oper = new Operator(operatorName!);
                        oper.PhoneNumber = reader.GetString(reader.GetOrdinal("phone_number"));
                        oper.Id = reader.GetInt64(reader.GetOrdinal("operator_id"));
                        oper.Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"));
                        yield return oper;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<Operator> operators)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Operator oper in operators)
                {
                    string commandSql;
                    if (oper.Id.HasValue)
                    {
                        commandSql = @"UPDATE operator
                        SET name = :name,
                        enabled = :enabled,
                        phone_number = :phone_number
                        WHERE operator_id = :operator_id";
                    }
                    else
                    {
                        commandSql = "INSERT INTO operator(name, enabled, phone_number) VALUES (:name, :enabled, :phone_number)";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        MapOperatorParameters(oper, command.Parameters);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
        }

        private void MapOperatorParameters(Operator oper, SQLiteParameterCollection parameters)
        {
            if (oper.Id.HasValue)
            {
                parameters.AddWithValue("operator_id", oper.Id!);
            }
            parameters.AddWithValue("name", oper.Name);
            parameters.AddWithValue("enabled", oper.Enabled);
            parameters.AddWithValue("phone_number", oper.PhoneNumber);
        }

        public async Task Delete(IEnumerable<Operator> operators)
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
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
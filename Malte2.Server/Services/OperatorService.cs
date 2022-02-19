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

        public async IAsyncEnumerable<Operator> GetOperators(bool onlyEnabled = false)
        {
            string whereFilter = "";
            if (onlyEnabled) {
                whereFilter = "WHERE enabled = 1";
            }
            string commandText = $"SELECT operator_id, name, phone, enabled FROM operator {whereFilter} ORDER BY operator_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string operatorName = reader.GetString(reader.GetOrdinal("name"));
                        Operator oper = new Operator(operatorName!);
                        oper.PhoneNumber = reader.GetString(reader.GetOrdinal("phone"));
                        oper.Id = reader.GetInt64(reader.GetOrdinal("operator_id"));
                        oper.Enabled = reader.GetBoolean(reader.GetOrdinal("enabled"));
                        yield return oper;
                    }
                }
            }
        }

        public async Task CreateUpdateOperators(IEnumerable<Operator> operators)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Operator oper in operators)
                {
                    if (oper.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand(@"UPDATE operator
                        SET name = :name,
                        enabled = :enabled,
                        phone = :phone
                        WHERE operator_id = :operator_id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("operator_id", oper.Id!);
                            MapOperatorParameters(oper, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        using (var command = new SQLiteCommand("INSERT INTO operator(name, enabled, phone) VALUES (:name, :enabled, :phone)", _databaseContext.Connection, transaction))
                        {
                            MapOperatorParameters(oper, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }

        private void MapOperatorParameters(Operator oper, SQLiteParameterCollection parameters)
        {
            parameters.AddWithValue("name", oper.Name);
            parameters.AddWithValue("enabled", oper.Enabled);
            parameters.AddWithValue("phone", oper.PhoneNumber);
        }

        public async Task DeleteOperators(IEnumerable<Operator> operators)
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
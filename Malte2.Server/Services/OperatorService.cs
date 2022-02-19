using Microsoft.AspNetCore.Mvc;
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
            string commandText = $"SELECT operator_id, name, phone FROM operator {whereFilter} ORDER BY operator_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string? operatorName = reader["name"]! as string;
                        Operator oper = new Operator(operatorName!);
                        if (reader["phone"] != null) {
                            oper.PhoneNumber = (reader["phone"] as string)!;
                        }
                        oper.Id = reader["operator_id"]! as long?;
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
                        phone = :phone,
                        WHERE operator_id = :operator_id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("operator_id", oper.Id.Value);
                            command.Parameters.AddWithValue("name", oper.Name);
                            command.Parameters.AddWithValue("enabled", oper.Enabled);
                            command.Parameters.AddWithValue("phone", oper.PhoneNumber);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        using (var command = new SQLiteCommand("INSERT INTO operator(name, enabled, phone) VALUES (:name, :enabled, :phone)", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("name", oper.Name);
                            command.Parameters.AddWithValue("enabled", oper.Enabled);
                            command.Parameters.AddWithValue("phone", oper.PhoneNumber);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
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
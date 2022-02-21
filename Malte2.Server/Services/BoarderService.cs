using Malte2.Database;
using Malte2.Model.Boarding;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class BoarderService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<BoarderService> _logger;

        public BoarderService(DatabaseContext databaseContext, ILogger<BoarderService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<Boarder> GetItems()
        {
            string commandText = $"SELECT boarder_id, label FROM boarder ORDER BY boarder_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Boarder boarder = new Boarder
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("boarder_id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                        };
                        yield return boarder;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<Boarder> boarders)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Boarder boarder in boarders)
                {
                    if (boarder.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand(@"UPDATE boarder
                        SET label = :label,
                        WHERE boarder_id = :boarder_id", _databaseContext.Connection, transaction))
                        {
                            MapBoarderParameters(boarder, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        using (var command = new SQLiteCommand("INSERT INTO boarder(label) VALUES (:label)", _databaseContext.Connection, transaction))
                        {
                            MapBoarderParameters(boarder, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }

        private void MapBoarderParameters(Boarder boarder, SQLiteParameterCollection parameters)
        {
            if (boarder.Id.HasValue)
            {
                parameters.AddWithValue("boarder_id", boarder.Id!);
            }
            parameters.AddWithValue("name", boarder.Name);
        }

        public async Task Delete(IEnumerable<Boarder> boarders)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Boarder boarder in boarders)
                {
                    if (boarder.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM boarder WHERE boarder_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", boarder.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
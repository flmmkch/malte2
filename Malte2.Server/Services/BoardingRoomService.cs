using Malte2.Database;
using Malte2.Model.Boarding;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class BoardingRoomService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<BoardingRoomService> _logger;

        public BoardingRoomService(DatabaseContext databaseContext, ILogger<BoardingRoomService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<BoardingRoom> GetItems()
        {
            string commandText = $"SELECT boarding_room_id, room_name FROM boarding_room ORDER BY boarding_room_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        BoardingRoom boardingRoom = new BoardingRoom
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("boarding_room_id")),
                            Label = reader.GetString(reader.GetOrdinal("room_name")),
                        };
                        yield return boardingRoom;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<BoardingRoom> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (BoardingRoom boardingRoom in accountingEntries)
                {
                    if (boardingRoom.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand(@"UPDATE boarding_room
                        SET room_name = :room_name
                        WHERE boarding_room_id = :boarding_room_id", _databaseContext.Connection, transaction))
                        {
                            MapBoardingRoomParameters(boardingRoom, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        using (var command = new SQLiteCommand("INSERT INTO boarding_room(room_name) VALUES (:room_name)", _databaseContext.Connection, transaction))
                        {
                            MapBoardingRoomParameters(boardingRoom, command.Parameters);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }

        private void MapBoardingRoomParameters(BoardingRoom boardingRoom, SQLiteParameterCollection parameters)
        {
            if (boardingRoom.Id.HasValue)
            {
                parameters.AddWithValue("boarding_room_id", boardingRoom.Id!);
            }
            parameters.AddWithValue("room_name", boardingRoom.Label);
        }

        public async Task Delete(IEnumerable<BoardingRoom> accountingEntries)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (BoardingRoom boardingRoom in accountingEntries)
                {
                    if (boardingRoom.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM boarding_room WHERE boarding_room_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", boardingRoom.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
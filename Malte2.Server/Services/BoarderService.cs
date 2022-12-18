using Malte2.Database;
using Malte2.Model.Accounting;
using Malte2.Model.Boarding;
using System.Data.Common;
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

        public async IAsyncEnumerable<BoarderListItemResponse> GetItemList(DateTime? roomOccupancyDateTime = null)
        {
            string commandText = @"SELECT b.boarder_id, b.name, r.room_name
                FROM boarder b
                LEFT JOIN occupancy o ON ((o.boarder_id = b.boarder_id) AND (o.date_start IS NULL OR o.date_start <= :occupancy_date) AND (o.date_end IS NULL OR o.date_end >= :occupancy_date))
                LEFT JOIN boarding_room r ON r.boarding_room_id = o.boarding_room_id
                ORDER BY b.boarder_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                if (roomOccupancyDateTime.HasValue) {
                    command.Parameters.AddWithValue("occupancy_date", roomOccupancyDateTime.Value.ToString("s"));
                } else {
                    command.Parameters.AddWithValue("occupancy_date", null);
                }
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        BoarderListItemResponse itemResponse = new BoarderListItemResponse
                        {
                            BoarderId = reader.GetInt64(reader.GetOrdinal("boarder_id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            RoomName = DatabaseValueUtils.GetNullableStringFromReader(reader, reader.GetOrdinal("room_name")),
                        };
                        yield return itemResponse;
                    }
                }
            }
        }

        public async Task<Boarder?> GetDetails(long boarderId)
        {
            string commandText = @"SELECT boarder_id,
                name,
                nationality,
                birth_date,
                birth_place,
                phone_number,
                notes,
                total_amount_deposited
                FROM boarder WHERE boarder_id = :boarder_id
                ORDER BY boarder_id ASC
                LIMIT 1;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                command.Parameters.AddWithValue("boarder_id", boarderId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Boarder boarder = new Boarder
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("boarder_id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Nationality = reader.GetString(reader.GetOrdinal("nationality")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("phone_number")),
                            BirthDate = DateTimeDatabaseUtils.GetNullableDateFromReader(reader, reader.GetOrdinal("birth_date")),
                            BirthPlace = DatabaseValueUtils.GetNullableStringFromReader(reader, reader.GetOrdinal("birth_place")),
                            Notes = reader.GetString(reader.GetOrdinal("notes")),
                            TotalAmountDeposited = Amount.FromString(reader.GetString(reader.GetOrdinal("total_amount_deposited")))
                        };
                        return boarder;
                    }
                }
            }
            return null;
        }

        public async Task CreateUpdate(IEnumerable<Boarder> boarders)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (Boarder boarder in boarders)
                {
                    string commandSql;
                    if (boarder.Id.HasValue)
                    {
                        commandSql = @"UPDATE boarder
                        SET name = :name,
                        nationality = :nationality,
                        phone_number = :phone_number,
                        birth_place = :birth_place,
                        birth_date = :birth_date,
                        notes = :notes,
                        total_amount_deposited = :total_amount_deposited
                        WHERE boarder_id = :boarder_id";
                    }
                    else
                    {
                        commandSql = @"INSERT INTO boarder(name,
                            nationality,
                            phone_number,
                            birth_place,
                            birth_date,
                            notes,
                            total_amount_deposited
                            ) VALUES (:name,
                            :nationality,
                            :phone_number,
                            :birth_place,
                            :birth_date,
                            :notes,
                            :total_amount_deposited
                            )";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (boarder.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("boarder_id", boarder.Id!);
                        }
                        command.Parameters.AddWithValue("name", boarder.Name);
                        command.Parameters.AddWithValue("nationality", boarder.Nationality);
                        command.Parameters.AddWithValue("phone_number", boarder.PhoneNumber);
                        command.Parameters.AddWithValue("birth_place", boarder.BirthPlace);
                        command.Parameters.AddWithValue("birth_date", DateTimeDatabaseUtils.GetStringFromNullableDate(boarder.BirthDate));
                        command.Parameters.AddWithValue("notes", boarder.Notes);
                        command.Parameters.AddWithValue("total_amount_deposited", boarder.TotalAmountDeposited.ToString());
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
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
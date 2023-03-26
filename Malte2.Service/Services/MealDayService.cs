using Malte2.Database;
using Malte2.Model.Accounting;
using Malte2.Model.MealDay;
using System.Data.SQLite;

namespace Malte2.Services
{

    public class MealDayService
    {
        private readonly DatabaseContext _databaseContext;

        private readonly ILogger<MealDayService> _logger;

        public MealDayService(DatabaseContext databaseContext, ILogger<MealDayService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async IAsyncEnumerable<MealDay> Get(DateTime? dateStart, DateTime? dateEnd)
        {
            string commandText = @"SELECT meal_id, date, nb_boarders, nb_patrons, nb_others, nb_caterers
                FROM meal m
                WHERE (:date_start IS NULL OR :date_start <= date) AND (:date_end IS NULL OR :date_end >= date)
                ORDER BY date, meal_id ASC;";
            using (var command = new SQLiteCommand(commandText, _databaseContext.Connection))
            {
                command.Parameters.AddWithValue("date_start", DateTimeDatabaseUtils.GetStringFromNullableDate(dateStart));
                command.Parameters.AddWithValue("date_end", DateTimeDatabaseUtils.GetStringFromNullableDate(dateEnd));
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        MealDay mealDay = new MealDay
                        {
                            Id = reader.GetInt64(reader.GetOrdinal("meal_id")),
                            MealDateTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))!),
                            BoarderCount = reader.GetInt32(reader.GetOrdinal("nb_boarders")),
                            PatronCount = reader.GetInt32(reader.GetOrdinal("nb_patrons")),
                            OtherCount = reader.GetInt32(reader.GetOrdinal("nb_others")),
                            CatererCount = reader.GetInt32(reader.GetOrdinal("nb_caterers")),
                        };
                        yield return mealDay;
                    }
                }
            }
        }

        public async Task CreateUpdate(IEnumerable<MealDay> mealDays)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (MealDay mealDay in mealDays)
                {
                    string commandSql;
                    if (mealDay.Id.HasValue)
                    {
                        commandSql = @"UPDATE meal
                        SET date = :date,
                            nb_boarders = :nb_boarders,
                            nb_patrons = :nb_patrons,
                            nb_others = :nb_others,
                            nb_caterers = :nb_caterers
                        WHERE meal_id = :meal_id";
                    }
                    else
                    {
                        commandSql = @"INSERT INTO meal(
                                date,
                                nb_boarders,
                                nb_patrons,
                                nb_others,
                                nb_caterers
                            ) VALUES (
                                :date,
                                :nb_boarders,
                                :nb_patrons,
                                :nb_others,
                                :nb_caterers
                            )";
                    }
                    using (var command = new SQLiteCommand(commandSql, _databaseContext.Connection, transaction))
                    {
                        if (mealDay.Id.HasValue)
                        {
                            command.Parameters.AddWithValue("meal_id", mealDay.Id!);
                        }
                        command.Parameters.AddWithValue("date", DateTimeDatabaseUtils.GetStringFromDate(mealDay.MealDateTime));
                        command.Parameters.AddWithValue("nb_boarders", mealDay.BoarderCount);
                        command.Parameters.AddWithValue("nb_patrons", mealDay.PatronCount);
                        command.Parameters.AddWithValue("nb_others", mealDay.OtherCount);
                        command.Parameters.AddWithValue("nb_caterers", mealDay.CatererCount);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await transaction.CommitAsync();
            }
        }

        public async Task Delete(IEnumerable<MealDay> mealDays)
        {
            using (var transaction = _databaseContext.Connection.BeginTransaction())
            {
                foreach (MealDay mealDay in mealDays)
                {
                    if (mealDay.Id.HasValue)
                    {
                        using (var command = new SQLiteCommand("DELETE FROM meal WHERE meal_id = :id", _databaseContext.Connection, transaction))
                        {
                            command.Parameters.AddWithValue("id", mealDay.Id!);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                await transaction.CommitAsync();
            }
        }
    }


}
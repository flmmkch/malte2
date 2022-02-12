using System.Data.SQLite;
using System.Data;

namespace Malte2.Database
{

    public class DatabaseContext : IDisposable
    {
        public SQLiteConnection Connection { get; private set; }

        public DatabaseContext(string connectionString)
        {
            Connection = InitializeDatabaseConnection(connectionString);
        }

        ~DatabaseContext()
        {
            Dispose();
        }

        public void Dispose()
        {
            Connection.Close();
        }

        private static SQLiteConnection InitializeDatabaseConnection(string connectionString)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            new SQLiteCommand("PRAGMA foreign_keys = ON;", connection).ExecuteNonQuery();
            return connection;
        }

        public bool? HasForeignKeySupport()
        {
            using (SQLiteCommand command = new SQLiteCommand("PRAGMA foreign_keys;", Connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0) == 1;
                    }
                    return null;
                }
            }
        }

        #region Global options
        private static readonly string OPTION_KEY_DB_VERSION = "DB_VERSION";

        private T? GetOptionValue<T>(string optionKey, Func<SQLiteDataReader, T> getValueObject)
        {
            using (SQLiteCommand command = new SQLiteCommand("SELECT value FROM global_option WHERE key = @Key;", Connection, null))
            {
                command.Parameters.AddWithValue("@Key", optionKey);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return getValueObject(reader);
                    }
                    return default(T);
                }
            }
        }

        private void SetOptionValue<T>(string optionKey, T optionValue, SQLiteTransaction? transaction = null)
        {
            using (SQLiteCommand command = new SQLiteCommand(@"
INSERT OR IGNORE INTO global_option (key, value) VALUES (@Key, @Value);
UPDATE global_option SET value = @Value WHERE key = @Key;
", Connection, transaction))
            {
                command.Parameters.AddWithValue("@Key", optionKey);
                command.Parameters.AddWithValue("@Value", optionValue);
                command.ExecuteNonQuery();
            }
        }

        public int? GetDatabaseVersion()
        {
            using (SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='global_option';", Connection, null))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return GetOptionValue<int>(OPTION_KEY_DB_VERSION, reader => Int32.Parse(reader.GetString(0)));
                    }
                    return null;
                }
            }
        }

        public void UpdateDatabaseVersion(int newVersion, SQLiteTransaction transaction)
        {
            SetOptionValue(OPTION_KEY_DB_VERSION, newVersion, transaction);
        }

        #endregion
    }

}
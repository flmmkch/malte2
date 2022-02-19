using System.Data.SQLite;

namespace Malte2.Database
{

    public static class DatabaseUpdater
    {
        public static void UpdateDatabase(DatabaseContext databaseContext)
        {
            var currentVersion = databaseContext.GetDatabaseVersion();
            using (SQLiteTransaction transaction = databaseContext.Connection.BeginTransaction())
            {
                bool commitTransaction = false;
                if (!currentVersion.HasValue || currentVersion.Value < 0)
                {
                    DatabaseUpdater.CreateDatabase(databaseContext, transaction);
                    commitTransaction = true;
                }
                if (commitTransaction)
                {
                    transaction.Commit();
                }
            }
        }

        private static void CreateDatabase(DatabaseContext databaseContext, SQLiteTransaction transaction)
        {
            // initialize tables
            string commandSql = @"
CREATE TABLE global_option(
    key TEXT,
    value TEXT
);
CREATE UNIQUE INDEX idx_global_option_key ON global_option(key);
CREATE TABLE operator(
    operator_id INTEGER PRIMARY KEY,
    enabled INTEGER NOT NULL DEFAULT 1,
    name TEXT,
    phone TEXT
);
CREATE UNIQUE INDEX idx_operator_name ON operator(name);
CREATE TABLE accounting_entry(
    accounting_entry_id INTEGER PRIMARY KEY,
    label TEXT NOT NULL,
    has_boarder INTEGER NOT NULL
);
CREATE TABLE operation(
    operation_id INTEGER PRIMARY KEY,
    operator_id INTEGER NOT NULL REFERENCES operator(operator_id),
    accounting_entry_id INTEGER NOT NULL REFERENCES accounting_entry(accounting_entry_id),
    date_time TEXT NOT NULL,
    label TEXT NOT NULL
);
";
            new SQLiteCommand(commandSql, databaseContext.Connection, transaction).ExecuteNonQuery();
            databaseContext.UpdateDatabaseVersion(1, transaction);
        }
    }

}
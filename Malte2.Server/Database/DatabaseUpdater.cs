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
            applyCommand(databaseContext, transaction, @"
CREATE TABLE global_option(
    key TEXT,
    value TEXT
);
CREATE UNIQUE INDEX idx_global_option_key ON global_option(key);
CREATE TABLE operator(
    operator_id INTEGER PRIMARY KEY,
    enabled INTEGER NOT NULL DEFAULT 1,
    name TEXT,
    phone_number TEXT
);
CREATE UNIQUE INDEX idx_operator_name ON operator(name);
CREATE TABLE accounting_entry(
    accounting_entry_id INTEGER PRIMARY KEY,
    label TEXT NOT NULL,
    has_boarder INTEGER NOT NULL DEFAULT 0,
    accounting_entry_type INTEGER NOT NULL
);
CREATE TABLE account_book(
    account_book_id INTEGER PRIMARY KEY,
    label TEXT NOT NULL,
    notes TEXT NOT NULL DEFAULT ''
);
CREATE TABLE boarding_room(
    boarding_room_id INTEGER PRIMARY KEY,
    room_name TEXT NOT NULL
);
CREATE TABLE deposit_type(
    deposit_type_id INTEGER PRIMARY KEY,
    label TEXT NOT NULL,
    amount TEXT NOT NULL
);
CREATE TABLE boarder(
    boarder_id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    phone_number TEXT NOT NULL,
    total_amount_deposited TEXT NOT NULL
);
CREATE TABLE boarder_deposit(
    boarder_deposit_id INTEGER PRIMARY KEY,
    boarder_id NTEGER NOT NULL REFERENCES boarder(boarder_id),
    deposit_type_id INTEGER NOT NULL REFERENCES deposit_type(deposit_type_id),
    amount TEXT NOT NULL
);
CREATE TABLE boarding(
    boarding_id INTEGER PRIMARY KEY,
    boarder_id INTEGER NOT NULL REFERENCES boarder(boarder_id),
    boarding_room_id INTEGER NOT NULL REFERENCES boarding_room(boarding_room_id),
    date_start TEXT NOT NULL,
    date_end TEXT NOT NULL,
    start_notes TEXT NOT NULL DEFAULT '',
    end_notes TEXT NOT NULL DEFAULT ''
);
CREATE TABLE operation(
    operation_id INTEGER PRIMARY KEY,
    operator_id INTEGER NOT NULL REFERENCES operator(operator_id),
    accounting_entry_id INTEGER NOT NULL REFERENCES accounting_entry(accounting_entry_id),
    date TEXT NOT NULL,
    label TEXT NOT NULL,
    payment_means INTEGER NOT NULL,
    payment_means_info TEXT NOT NULL DEFAULT '',
    boarder_id INTEGER REFERENCES boarder(boarder_id),
    amount INTEGER NOT NULL
);
CREATE TABLE meal(
    meal_id INTEGER PRIMARY KEY,
    date TEXT NOT NULL,
    nb_boarders INTEGER NOT NULL DEFAULT 0,
    nb_patrons INTEGER NOT NULL DEFAULT 0,
    nb_others INTEGER NOT NULL DEFAULT 0,
    nb_caterers INTEGER NOT NULL DEFAULT 0
);
CREATE UNIQUE INDEX idx_meal_date ON meal(date);
CREATE TABLE remission(
    remission_id INTEGER PRIMARY KEY,
    date TEXT NOT NULL,
    operator_id INTEGER NOT NULL REFERENCES operator(operator_id),
    payment_means INTEGER NOT NULL
);
CREATE INDEX idx_remission_date ON remission(date);
CREATE TABLE remission_operation(
    remission_operation_id INTEGER PRIMARY KEY,
    remission_id INTEGER NOT NULL REFERENCES remission(remission_id),
    operation_id INTEGER NOT NULL REFERENCES operation(operation_id)
);
CREATE TABLE remission_cash(
    remission_cash_id INTEGER PRIMARY KEY,
    remission_id INTEGER NOT NULL REFERENCES remission(remission_id),
    operation_id INTEGER NOT NULL REFERENCES operation(operation_id)
);
");

            // default values: accounting entries
            applyCommand(databaseContext, transaction, @"INSERT INTO accounting_entry(label, has_boarder, accounting_entry_type) VALUES ('Pension (recettes)', 1, 1);");
            applyCommand(databaseContext, transaction, @"INSERT INTO accounting_entry(label, has_boarder, accounting_entry_type) VALUES ('Emprunt pensionnaire', 1, 0);");
            applyCommand(databaseContext, transaction, @"INSERT INTO accounting_entry(label, has_boarder, accounting_entry_type) VALUES ('Remboursement pensionnaire', 1, 0);");
            applyCommand(databaseContext, transaction, @"INSERT INTO accounting_entry(label, accounting_entry_type) VALUES ('Restaurant (recettes)', 1);");
            applyCommand(databaseContext, transaction, @"INSERT INTO accounting_entry(label, accounting_entry_type) VALUES ('Divers (dépenses)', 0);");
            applyCommand(databaseContext, transaction, @"INSERT INTO accounting_entry(label, accounting_entry_type) VALUES ('Autre (recettes)', 0);");

            // default values
            databaseContext.UpdateDatabaseVersion(1, transaction);
        }
        private static void applyCommand(DatabaseContext databaseContext, SQLiteTransaction transaction, string commandSql)
        {
            using (SQLiteCommand command = new SQLiteCommand(commandSql, databaseContext.Connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

    }
}
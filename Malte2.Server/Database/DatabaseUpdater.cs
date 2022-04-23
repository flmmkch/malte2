using System.Data.SQLite;

namespace Malte2.Database
{

    public static class DatabaseUpdater
    {
        public static readonly int DATABASE_VERSION = 2;

        public static void UpdateDatabase(DatabaseContext databaseContext)
        {
            var currentVersion = databaseContext.GetDatabaseVersion();
            using (SQLiteTransaction transaction = databaseContext.Connection.BeginTransaction())
            {
                bool commitTransaction = false;
                if (!currentVersion.HasValue || currentVersion.Value < 0)
                {
                    DatabaseUpdater.CreateDatabase(databaseContext, transaction);
                    currentVersion = databaseContext.GetDatabaseVersion();
                    commitTransaction = true;
                }
                if (currentVersion == 1) {
                    DatabaseUpdater.UpdateV1ToV2(databaseContext, transaction);
                    currentVersion = databaseContext.GetDatabaseVersion();
                    commitTransaction = true;
                }
                if (commitTransaction && currentVersion == DATABASE_VERSION)
                {
                    transaction.Commit();
                }
                else if (commitTransaction) {
                    throw new Exception($"Failed to update database version from {currentVersion}");
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
    birth_date TEXT,
    birth_place TEXT,
    phone_number TEXT NOT NULL,
    nationality TEXT NOT NULL,
    notes TEXT NOT NULL,
    total_amount_deposited TEXT NOT NULL
);
CREATE TABLE boarder_deposit(
    boarder_deposit_id INTEGER PRIMARY KEY,
    boarder_id INTEGER NOT NULL REFERENCES boarder(boarder_id),
    deposit_type_id INTEGER NOT NULL REFERENCES deposit_type(deposit_type_id),
    amount TEXT NOT NULL
);
CREATE TABLE occupancy(
    occupancy_id INTEGER PRIMARY KEY,
    boarder_id INTEGER NOT NULL REFERENCES boarder(boarder_id),
    boarding_room_id INTEGER NOT NULL REFERENCES boarding_room(boarding_room_id),
    date_start TEXT,
    date_end TEXT,
    start_notes TEXT NOT NULL DEFAULT '',
    end_notes TEXT NOT NULL DEFAULT ''
);
CREATE TABLE operation(
    operation_id INTEGER PRIMARY KEY,
    operator_id INTEGER NOT NULL REFERENCES operator(operator_id),
    accounting_entry_id INTEGER NOT NULL REFERENCES accounting_entry(accounting_entry_id),
    account_book_id INTEGER NOT NULL REFERENCES account_book(account_book_id),
    date TEXT NOT NULL,
    label TEXT NOT NULL,
    payment_method INTEGER NOT NULL,
    boarder_id INTEGER REFERENCES boarder(boarder_id),
    amount INTEGER NOT NULL,
    check_number INTEGER NULL UNIQUE,
    transfer_number INTEGER NULL UNIQUE,
    card_number TEXT NULL,
    CHECK (payment_method <> 1 OR check_number IS NOT NULL)
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

        private static void UpdateV1ToV2(DatabaseContext databaseContext, SQLiteTransaction transaction)
        {
            // upgrade tables
            applyCommand(databaseContext, transaction, @"
CREATE TABLE accounting_category(
    accounting_category_id INTEGER PRIMARY KEY,
    label TEXT NOT NULL
);

DROP TABLE boarder_deposit;

CREATE TABLE boarder_deposit(
    boarder_deposit_id INTEGER PRIMARY KEY,
    boarder_id INTEGER NOT NULL REFERENCES boarder(boarder_id),
    deposit_type_id INTEGER NOT NULL REFERENCES deposit_type(deposit_type_id),
    amount TEXT NOT NULL
);
CREATE UNIQUE INDEX idx_boarder_deposit ON boarder_deposit(boarder_id, deposit_type_id);

ALTER TABLE operation ADD COLUMN category_id INTEGER NULL REFERENCES accounting_category(accounting_category_id);

ALTER TABLE operation ADD COLUMN card_ticket_number INTEGER NULL;

UPDATE operation SET card_ticket_number = CAST(card_number as INTEGER) WHERE card_number IS NOT NULL; 

ALTER TABLE operation DROP COLUMN card_number;

CREATE INDEX idx_operation_by_boarder ON operation(boarder_id, date) WHERE boarder_id NOT NULL;

CREATE INDEX idx_operation_by_payment_method ON operation(payment_method, date);

CREATE INDEX idx_operation_by_date ON operation(date, operation_id);

ALTER TABLE operation ADD COLUMN details TEXT NOT NULL DEFAULT '';

ALTER TABLE operation ADD COLUMN invoice TEXT;

ALTER TABLE operation DROP COLUMN payment_method_info;

");
            // update database version
            databaseContext.UpdateDatabaseVersion(2, transaction);
        }

    }
}
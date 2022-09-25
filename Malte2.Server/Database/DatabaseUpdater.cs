using System.Data.SQLite;

namespace Malte2.Database
{

    public static class DatabaseUpdater
    {
        public static readonly int DATABASE_VERSION = 3;

        public static readonly Dictionary<int, Action<DatabaseContext, SQLiteTransaction>> UPGRADE_ACTION_TABLE = new Dictionary<int, Action<DatabaseContext, SQLiteTransaction>>
        {
            [1] = DatabaseUpdater.UpdateV1ToV2,
            [2] = DatabaseUpdater.UpdateV2ToV3,
        };

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
                while (currentVersion.HasValue && UPGRADE_ACTION_TABLE.TryGetValue(currentVersion.Value, out var upgradeAction)) {
                    upgradeAction(databaseContext, transaction);
                    currentVersion = databaseContext.GetDatabaseVersion();
                    commitTransaction = true;
                }
                if (commitTransaction)
                {
                    if (currentVersion == DATABASE_VERSION)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception($"Failed to update database version from {currentVersion}");
                    }
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
    accounting_entry_id INTEGER NULL REFERENCES accounting_entry(accounting_entry_id),
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

CREATE TRIGGER ck_operation_category_entry
BEFORE UPDATE OF category_id, accounting_entry_id ON operation
FOR EACH ROW WHEN NEW.category_id IS NOT NULL AND NEW.accounting_entry_id NOT IN (SELECT accounting_entry_id FROM accounting_category WHERE accounting_category_id = NEW.category_id)
BEGIN
    SELECT RAISE(FAIL, 'Operations must have the same accounting entry as their category');
END;

CREATE TRIGGER ck_category_entry_operations
BEFORE UPDATE OF accounting_entry_id ON accounting_category
FOR EACH ROW WHEN ((SELECT COUNT(*) FROM operation WHERE operation.category_id = NEW.accounting_category_id AND operation.accounting_entry_id <> NEW.accounting_entry_id) > 0)
BEGIN
    SELECT RAISE(FAIL, 'Operations must have the same accounting entry as their category');
END;

ALTER TABLE operation ADD COLUMN card_ticket_number INTEGER NULL;

UPDATE operation SET card_ticket_number = CAST(card_number as INTEGER) WHERE card_number IS NOT NULL; 

ALTER TABLE operation DROP COLUMN card_number;

CREATE INDEX idx_operation_by_boarder ON operation(boarder_id, date) WHERE boarder_id NOT NULL;

CREATE INDEX idx_operation_by_payment_method ON operation(payment_method, date);

CREATE INDEX idx_operation_by_date ON operation(date, operation_id);

ALTER TABLE operation ADD COLUMN details TEXT NOT NULL DEFAULT '';

ALTER TABLE operation ADD COLUMN invoice TEXT;

");
            // update database version
            databaseContext.UpdateDatabaseVersion(2, transaction);
        }


        private static void UpdateV2ToV3(DatabaseContext databaseContext, SQLiteTransaction transaction)
        {
            // upgrade tables
            applyCommand(databaseContext, transaction, @"
DROP TABLE remission_cash;

ALTER TABLE remission_operation RENAME TO remission_check;

ALTER TABLE remission DROP COLUMN payment_means;

ALTER TABLE remission_check DROP COLUMN operation_id;

ALTER TABLE remission_check ADD COLUMN check_number INTEGER NULL;

CREATE INDEX idx_remission_check_remission ON remission_check(remission_id);

CREATE UNIQUE INDEX idx_remission_check_check_number ON remission_check(check_number);

ALTER TABLE remission_check ADD COLUMN amount INTEGER NOT NULL;

ALTER TABLE remission ADD COLUMN cash_deposit_01c INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_02c INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_05c INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_10c INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_20c INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_50c INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_001e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_002e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_005e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_010e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_020e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_050e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_100e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_200e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN cash_deposit_500e INTEGER NOT NULL DEFAULT 0;

ALTER TABLE remission ADD COLUMN notes TEXT NOT NULL DEFAULT '';

");
            // update database version
            databaseContext.UpdateDatabaseVersion(3, transaction);
        }

    }
}

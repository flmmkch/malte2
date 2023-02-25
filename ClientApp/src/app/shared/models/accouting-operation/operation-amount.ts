import { DictionaryById } from "../../utils/dictionary-by-id";
import { AccountingEntry, EntryType } from "../accounting-entry.model";
import { Amount } from "../amount.model";
import { Operation } from "../operation.model";

export function getOperationBalance(op: Operation, entries: DictionaryById<AccountingEntry>): Amount {
    let entry = entries[op.accountingEntryId];
    return getEntryTypeAmountBalance(op.amount, entry.entryType);
}

export function getEntryTypeAmountBalance(amount: Amount, entryType: EntryType): Amount {
    if (entryType === EntryType.Expense) {
        return Amount.from(0)!.substract(amount);
    }
    else if (entryType === EntryType.Revenue) {
        return amount;
    }
    return Amount.from(0)!;
}

export function calculateTotal(ops: Operation[], entries: DictionaryById<AccountingEntry>): Amount {
    interface OpEntry {
        op: Operation,
        entry: AccountingEntry,
    }
    return ops
        .map(op => <OpEntry> { op: op, entry: entries[op.accountingEntryId] })
        .reduce((prev, { op, entry }) => prev.add(getEntryTypeAmountBalance(op.amount, entry.entryType)), Amount.from(0)!)
        ;
}

export function calculateEntryTypeTotal(ops: Operation[], entryType: EntryType, entries: DictionaryById<AccountingEntry>): Amount {
    interface OpEntry {
        op: Operation,
        entry: AccountingEntry,
    }
    return ops
        .map(op => <OpEntry> { op, entry: entries[op.accountingEntryId] })
        .filter(({ entry: accountingEntry }) => accountingEntry.entryType == entryType)
        .map(({ op }) => op.amount)
        .reduce((prev, cur) => prev.add(cur), Amount.from(0)!)
        ;
}

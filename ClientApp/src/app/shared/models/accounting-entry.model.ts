export enum EntryType {
    /** Dépense */
    Expense,
    /** Recette */
    Revenue,
}

export class AccountingEntry {
    constructor(public readonly id?: number) { }
    public label: string = '';
    public dependsOnBoarder: boolean = false;
    public entryType: EntryType = EntryType.Expense;

    public static entryTypeToString(entryType: EntryType): string {
        switch (entryType) {
            case EntryType.Expense:
                return 'Dépense';
            case EntryType.Revenue:
                return 'Recette';
            default:
                return '';
        }
    }

    public get entryTypeString(): string {
        return AccountingEntry.entryTypeToString(this.entryType);
    }
}


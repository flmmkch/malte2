import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ListTable, SetCurrentWorkingItemEventArgs } from 'src/app/modules/list-table/list-table.component';
import { AccountingEntry, EntryType } from 'src/app/shared/models/accounting-entry.model';
import { AccountingEntryService } from 'src/app/shared/services/accounting-entry.service';

@Component({
  selector: 'app-accounting-entries',
  templateUrl: './accounting-entries.component.html',
})
export class AccountingEntriesComponent implements OnInit, AfterViewInit {
  public items?: AccountingEntry[];
  
  readonly accountingEntryFormGroup = new FormGroup({
    labelControl: new FormControl(),
    entryTypeControl: new FormControl(),
    hasBoarderControl: new FormControl(),
  });

  @ViewChild('accountingEntryListTable') accountingEntryListTable!: ListTable;

  constructor(private readonly _service: AccountingEntryService) { }
  
  private _currentLoadingPromise?: Promise<AccountingEntry[]>;

  public get currentLoadingPromise(): Promise<AccountingEntry[]> | undefined {
    return this._currentLoadingPromise;
  }

  load(): Promise<AccountingEntry[]> {
    let observable = this._service.get();
    observable.subscribe(i => {
      this.items = i;
    });
    this._currentLoadingPromise = observable.toPromise();
    return this._currentLoadingPromise;
  }


  delete(accountingEntry: AccountingEntry) {
    if (accountingEntry.id) {
      this._service.delete([accountingEntry]).subscribe(() => this.load(), console.error);
    }
  }

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.accountingEntryListTable.onCreate.subscribe(() => this.accountingEntryListTable.currentWorkingItem = new AccountingEntry());
    this.accountingEntryListTable.onDelete.subscribe((accountingEntry: AccountingEntry) => this.delete(accountingEntry));
    this.accountingEntryListTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<AccountingEntry>) => {
      const accountingEntry = e.value;
      if (accountingEntry) {
        this.accountingEntryFormGroup.controls.labelControl.setValue(accountingEntry.label);
        this.accountingEntryFormGroup.controls.hasBoarderControl.setValue(accountingEntry.dependsOnBoarder);
        this.accountingEntryFormGroup.controls.entryTypeControl.setValue(accountingEntry.entryType);
      }
    });
    this.accountingEntryListTable.confirmDeleteMessage = (accountingEntry: AccountingEntry) => `Supprimer l'imputation comptable ${accountingEntry.label} ?`;
  }

  onSubmit(): void {
    const accountingEntry = this.accountingEntryListTable.currentWorkingItem as AccountingEntry;
    accountingEntry.label = this.accountingEntryFormGroup.controls.labelControl.value;
    accountingEntry.dependsOnBoarder = this.accountingEntryFormGroup.controls.hasBoarderControl.value || false;
    accountingEntry.entryType = Number.parseInt(this.accountingEntryFormGroup.controls.entryTypeControl.value);
    this._service.createUpdate([accountingEntry]).subscribe(() => {
      this.accountingEntryFormGroup.reset();
      this.accountingEntryListTable.cancelEdit();
      this.load();
    }, console.error);
  }
  

  getEntryTypes(): EntryType[] {
    return [EntryType.Expense, EntryType.Revenue];
  }

  getEntryTypeString(value: EntryType): string {
    return AccountingEntry.entryTypeToString(value);
  }
}

import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { combineLatestWith, map, Observable } from 'rxjs';
import { AccountingCategory } from 'src/app/shared/models/accounting-category.model';
import { AccountingEntry } from 'src/app/shared/models/accounting-entry.model';
import { AccountingCategoryService } from 'src/app/shared/services/accounting-category.service';
import { AccountingEntryService } from 'src/app/shared/services/accounting-entry.service';
import { DictionaryById, listToDictionary } from 'src/app/shared/utils/dictionary-by-id';
import { ListTable, SetCurrentWorkingItemEventArgs } from '../../list-table/list-table.component';


interface CategoryDisplay {
  id?: number,
  label: string,
  accountingEntryName: string,
  category: AccountingCategory,
}

@Component({
  selector: 'app-accounting-categories',
  templateUrl: './accounting-categories.component.html',
  styleUrls: []
})
export class AccountingCategoriesComponent implements OnInit, AfterViewInit {

  public items: AccountingCategory[] = [];

  public displayItems: CategoryDisplay[] = [];
  
  public accountingEntries: DictionaryById<AccountingEntry> = [];

  public get accountingEntryList(): AccountingEntry[] {
    return Object.values(this.accountingEntries);
  }

  private _entriesObservable: Observable<DictionaryById<AccountingEntry>>;

  readonly categoryFormGroup = new FormGroup({
    labelControl: new FormControl(),
    entryControl: new FormControl(),
  });

  @ViewChild('categoryListTable') categoryListTable!: ListTable;

  constructor(private readonly _service: AccountingCategoryService, private readonly _entryService: AccountingEntryService) {
    this._entriesObservable = this.loadContext();
  }

  load(): Observable<AccountingCategory[]> {
    let observable = this._service.get();
    observable.subscribe(i => {
      this.items = i;
      if (this.items.length == 0) {
        this.categoryListTable.addItem();
      }
    });
    observable.pipe(combineLatestWith(this._entriesObservable))
      .subscribe(([categories, entries]) => {
        this.displayItems = categories.map(category => AccountingCategoriesComponent.createDisplayItem(category, entries));
      });
    return observable;
  }

  static createDisplayItem(category: AccountingCategory, entries: DictionaryById<AccountingEntry>): CategoryDisplay {
    let accountingEntryName = '';
    if (category.accountingEntryId && category.accountingEntryId in entries) {
      accountingEntryName = entries[category.accountingEntryId].label;
    }
    else if (category.accountingEntryId === undefined) {
      accountingEntryName = '';
    }
    return {
      id: category.id,
      label: category.label,
      accountingEntryName,
      category,
    };
  }

  loadContext(): Observable<DictionaryById<AccountingEntry>> {
    const observable = this._entryService.get().pipe(map(listToDictionary));
    observable.subscribe({ next: dict => this.accountingEntries = dict, error: console.error });
    return observable;
  }

  delete(accountingCategory: AccountingCategory) {
    if (accountingCategory.id) {
      this._service.delete([accountingCategory]).subscribe({ next: () => this.load(), error: console.error });
    }
  }

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.categoryListTable.onCreate.subscribe(() => this.categoryListTable.currentWorkingItem = AccountingCategoriesComponent.createDisplayItem(new AccountingCategory(), this.accountingEntries));
    this.categoryListTable.onDelete.subscribe((accountingCategory: AccountingCategory) => this.delete(accountingCategory));
    this.categoryListTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<CategoryDisplay>) => {
      const accountingCategory = e.value;
      if (accountingCategory) {
        this.categoryFormGroup.controls.labelControl.setValue(accountingCategory.label);
        this.categoryFormGroup.controls.entryControl.setValue(accountingCategory.category.accountingEntryId);
      }
    });
    this.categoryListTable.confirmDeleteMessage = (accountingCategory: AccountingCategory) => `Supprimer la catégorie ${accountingCategory.label} ?`;
  }

  onSubmit(): void {
    const accountingCategory = this.categoryListTable.currentWorkingItem as AccountingCategory;
    accountingCategory.label = this.categoryFormGroup.controls.labelControl.value;
    accountingCategory.accountingEntryId = this.categoryFormGroup.controls.entryControl.value;
    this._service.createUpdate([accountingCategory]).subscribe({
      next: () => {
        this.categoryFormGroup.reset();
        this.categoryListTable.cancelEdit();
        this.load();
      }, error: console.error
    });
  }

}

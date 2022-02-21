import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ListTable, SetCurrentWorkingItemEventArgs } from 'src/app/shared/list-table/list-table.component';
import { AccountBook } from 'src/app/shared/models/account-book.model';
import { AccountBookService } from 'src/app/shared/services/account-book.service';

@Component({
  selector: 'app-account-books',
  templateUrl: './account-books.component.html'
})
export class AccountBooksComponent implements OnInit, AfterViewInit {
  public items?: AccountBook[];
  
  readonly formGroup = new FormGroup({
    labelControl: new FormControl(),
  });

  constructor(private readonly _service: AccountBookService) { }

  @ViewChild('listTable') listTable!: ListTable;

  
  private _currentLoadingPromise?: Promise<AccountBook[]>;

  public get currentLoadingPromise(): Promise<AccountBook[]> | undefined {
    return this._currentLoadingPromise;
  }

  load(): Promise<AccountBook[]> {
    let observable = this._service.get();
    observable.subscribe(items => {
      this.items = items;
      if (this.items.length == 0) {
        this.listTable.addItem();
      }
    });
    this._currentLoadingPromise = observable.toPromise();
    return this._currentLoadingPromise;
  }


  delete(accountBook: AccountBook) {
    if (accountBook.id) {
      this._service.delete([accountBook]).subscribe(() => this.load(), console.error);
    }
  }

  ngOnInit(): void {
      this.load();
  }

  ngAfterViewInit(): void {
    this.listTable.onCreate.subscribe(() => this.listTable.currentWorkingItem = new AccountBook());
    this.listTable.onDelete.subscribe((accountBook: AccountBook) => this.delete(accountBook));
    this.listTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<AccountBook>) => {
      const accountBook = e.value;
      if (accountBook) {
        this.formGroup.controls.labelControl.setValue(accountBook.label);
      }
    });
    this.listTable.confirmDeleteMessage = (accountBook: AccountBook) => `Supprimer l'imputation comptable ${accountBook.label} ?`;
  }

  onSubmit(): void {
    const accountBook = this.listTable.currentWorkingItem as AccountBook;
    accountBook.label = this.formGroup.controls.labelControl.value;
    this._service.createUpdate([accountBook]).subscribe(() => {
      this.formGroup.reset();
      this.listTable.cancelEdit();
      this.load();
    }, console.error);
  }

}

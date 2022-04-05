import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { ListTable, SetCurrentWorkingItemEventArgs } from 'src/app/modules/list-table/list-table.component';
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

  load(): Observable<AccountBook[]> {
    let observable = this._service.get();
    observable.subscribe(items => {
      this.items = items;
      if (this.items.length == 0) {
        this.listTable.addItem();
      }
    });
    return observable;
  }


  delete(accountBook: AccountBook) {
    if (accountBook.id) {
      this._service.delete([accountBook]).subscribe({ next: () => this.load(), error: console.error });
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
    this.listTable.confirmDeleteMessage = (accountBook: AccountBook) => `Supprimer le livre comptable ${accountBook.label} ?`;
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

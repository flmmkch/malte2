import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { AccountingCategory } from 'src/app/shared/models/accounting-category.model';
import { AccountingCategoryService } from 'src/app/shared/services/accounting-category.service';
import { ListTable, SetCurrentWorkingItemEventArgs } from '../../list-table/list-table.component';

@Component({
  selector: 'app-accounting-categories',
  templateUrl: './accounting-categories.component.html',
  styleUrls: []
})
export class AccountingCategoriesComponent implements OnInit, AfterViewInit {

  public items?: AccountingCategory[];

  readonly categoryFormGroup = new FormGroup({
    labelControl: new FormControl(),
  });

  @ViewChild('categoryListTable') categoryListTable!: ListTable;

  constructor(private readonly _service: AccountingCategoryService) { }

  load(): Observable<AccountingCategory[]> {
    let observable = this._service.get();
    observable.subscribe(i => {
      this.items = i;
      if (this.items.length == 0) {
        this.categoryListTable.addItem();
      }
    });
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
    this.categoryListTable.onCreate.subscribe(() => this.categoryListTable.currentWorkingItem = new AccountingCategory());
    this.categoryListTable.onDelete.subscribe((accountingCategory: AccountingCategory) => this.delete(accountingCategory));
    this.categoryListTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<AccountingCategory>) => {
      const accountingCategory = e.value;
      if (accountingCategory) {
        this.categoryFormGroup.controls.labelControl.setValue(accountingCategory.label);
      }
    });
    this.categoryListTable.confirmDeleteMessage = (accountingCategory: AccountingCategory) => `Supprimer la catégorie ${accountingCategory.label} ?`;
  }

  onSubmit(): void {
    const accountingCategory = this.categoryListTable.currentWorkingItem as AccountingCategory;
    accountingCategory.label = this.categoryFormGroup.controls.labelControl.value;
    this._service.createUpdate([accountingCategory]).subscribe({
      next: () => {
        this.categoryFormGroup.reset();
        this.categoryListTable.cancelEdit();
        this.load();
      }, error: console.error
    });
  }

}

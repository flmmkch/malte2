import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { zip } from 'rxjs';
import { AccountBook } from 'src/app/shared/models/account-book.model';
import { AccountingCategory } from 'src/app/shared/models/accounting-category.model';
import { AccountingEntry } from 'src/app/shared/models/accounting-entry.model';
import { calculateTotal } from 'src/app/shared/models/accouting-operation/operation-amount';
import { createOperationDisplay, OperationDisplay } from 'src/app/shared/models/accouting-operation/operation-display';
import { Amount } from 'src/app/shared/models/amount.model';
import { Boarder } from 'src/app/shared/models/boarder.model';
import { Operation } from 'src/app/shared/models/operation.model';
import { Operator } from 'src/app/shared/models/operator.model';
import { AccountBookService } from 'src/app/shared/services/account-book.service';
import { AccountingCategoryService } from 'src/app/shared/services/accounting-category.service';
import { AccountingEntryService } from 'src/app/shared/services/accounting-entry.service';
import { BoarderService } from 'src/app/shared/services/boarder.service';
import { OperationService } from 'src/app/shared/services/operation.service';
import { OperatorService } from 'src/app/shared/services/operator.service';
import { dateToFormValue, formValueToDate } from 'src/app/shared/utils/date-time-form-conversion';
import { DictionaryById, listToDictionary } from 'src/app/shared/utils/dictionary-by-id';

@Component({
  selector: 'app-boarder-details',
  templateUrl: './boarder-details.component.html',
  styleUrls: ['./boarder-details.component.css', '../../shared/components/style/amount.css'],
})
export class BoarderDetailsComponent implements OnInit {

  constructor(
    private readonly _service: BoarderService,
    private readonly _operationsService: OperationService,
    private readonly _accountBooksService: AccountBookService,
    private readonly _accountingCategoryService: AccountingCategoryService,
    private readonly _accountingEntryService: AccountingEntryService,
    private readonly _operatorService: OperatorService,
    private _route: ActivatedRoute,
    private readonly _router: Router,
    ) { }

  @Input() edition: boolean = false;

  public boarder?: Boarder;

  public accounting?: {
    operations: Operation[],
    books: DictionaryById<AccountBook>,
    categories: DictionaryById<AccountingCategory>,
    operators: DictionaryById<Operator>,
    entries: DictionaryById<AccountingEntry>,
  };

  private _formValidationErrorMessage?: string;

  public get formValidationErrorMessage(): string | undefined {
    return this._formValidationErrorMessage;
  }

  boarderFormGroup: FormGroup = new FormGroup({
    nameControl: new FormControl(),
    phoneNumberControl: new FormControl(),
    birthDateControl: new FormControl(),
    birthPlaceControl: new FormControl(),
    nationalityControl: new FormControl(),
    notesControl: new FormControl(),
  });

  ngOnInit(): void {
    this.resetValidationErrorMessage();
    this._route.data.subscribe({
      next: data => {
        const boarder: Boarder | undefined = data['boarder'];
        this.initializeBoarder(boarder);
      },
      error: e => this.resetValidationErrorMessage(e),
    });
  }

  initializeBoarder(boarder?: Boarder): void {
    if (boarder) {
      this.boarderFormGroup.controls.nameControl.setValue(boarder.name);
      this.boarderFormGroup.controls.nationalityControl.setValue(boarder.nationality);
      this.boarderFormGroup.controls.phoneNumberControl.setValue(boarder.phoneNumber);
      this.boarderFormGroup.controls.birthDateControl.setValue(boarder.birthDate ? dateToFormValue(boarder.birthDate) : undefined);
      this.boarderFormGroup.controls.birthPlaceControl.setValue(boarder.birthPlace);
      this.boarderFormGroup.controls.notesControl.setValue(boarder.notes);
      this.boarder = boarder;
      if (boarder.id) {
        this.initializeOperations(boarder.id);
      }
    }
    else {
      // création : toujours en mode édition
      this.edition = true;
    }
  }

  initializeOperations(boarderId: number) {
    zip([
      this._operationsService.get({ boarderId }),
      this._accountBooksService.get(),
      this._accountingCategoryService.get(),
      this._accountingEntryService.get(),
      this._operatorService.get(),
    ])
    .subscribe({
      next: ([operations, books, categories, entries, operators]) => {
        this.accounting = {
          operations,
          books: listToDictionary(books),
          categories: listToDictionary(categories),
          entries: listToDictionary(entries),
          operators: listToDictionary(operators),
        };
      }
    });
  }

  onSubmit() {
    let boarder = this.boarder || new Boarder();
    boarder.name = this.boarderFormGroup.controls.nameControl.value;
    boarder.nationality = this.boarderFormGroup.controls.nationalityControl.value || '';
    boarder.phoneNumber = this.boarderFormGroup.controls.phoneNumberControl.value || '';
    boarder.birthDate = this.boarderFormGroup.controls.birthDateControl.value ? formValueToDate(this.boarderFormGroup.controls.birthDateControl.value) : undefined;
    boarder.birthPlace = this.boarderFormGroup.controls.birthPlaceControl.value;
    boarder.notes = this.boarderFormGroup.controls.notesControl.value || '';
    this._service.createUpdate([boarder]).subscribe(() => {
      if (this.boarder !== undefined) {
        this.setEditMode(false);
        this.resetValidationErrorMessage();
      }
      else {
        this.navigateToBoarderList();
      }
    }, e => this.resetValidationErrorMessage(e));
  }

  setEditMode(edition: boolean) {
    this.edition = edition;
  }

  delete() {
    if (this.boarder) {
      this._service.delete([this.boarder]).subscribe(() => {
        this.navigateToBoarderList();
      }, e => this.resetValidationErrorMessage(e));
    }
  }

  navigateToBoarderList() {
    this._router.navigate(['/boarders']);
  }

  resetValidationErrorMessage(e?: any) {
    if (e) {
      if (typeof e === 'string') {
        this._formValidationErrorMessage = e;
      }
      else if (e instanceof Error) {
        this._formValidationErrorMessage = e.message;
      }
      else if (e instanceof HttpErrorResponse) {
        this._formValidationErrorMessage = e.statusText;
      }
      else {
        this._formValidationErrorMessage = JSON.stringify(e); 
      }
    }
    else {
      this._formValidationErrorMessage = undefined;
    }
  }

  get accountingOperationsDisplay(): OperationDisplay[] {
    if (this.accounting && this.boarder) {
      let contextDicts = {
        boarders: [{ boarderId: this.boarder.id || -1, name: this.boarder.name }],
        books: this.accounting.books,
        categories: this.accounting.categories,
        entries: this.accounting.entries,
        operators: this.accounting.operators,
      };
      return this.accounting.operations.map(op => createOperationDisplay(op, contextDicts));
    }
    else {
      return [];
    }
  }

  get totalBalance(): Amount {
    if (this.accounting) {
      return calculateTotal(this.accounting.operations, this.accounting.entries);
    }
    else {
      return Amount.from(0)!;
    }
  }
}

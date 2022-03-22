import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { map } from 'rxjs/internal/operators/map';
import { AccountBook } from 'src/app/shared/models/account-book.model';
import { AccountingEntry } from 'src/app/shared/models/accounting-entry.model';
import { Amount } from 'src/app/shared/models/amount.model';
import { BoarderListItem } from 'src/app/shared/models/boarder.model';
import { Operation } from 'src/app/shared/models/operation.model';
import { Operator } from 'src/app/shared/models/operator.model';
import { allPaymentMethods, PaymentMethod, paymentMethodString } from 'src/app/shared/models/payment-method.model';
import { AccountBookService } from 'src/app/shared/services/account-book.service';
import { AccountingEntryService } from 'src/app/shared/services/accounting-entry.service';
import { BoarderService } from 'src/app/shared/services/boarder.service';
import { OperationService } from 'src/app/shared/services/operation.service';
import { OperatorService } from 'src/app/shared/services/operator.service';
import { dateToFormValue, formValueToDate } from 'src/app/shared/utils/date-time-form-conversion';
import { DictionaryById, listToDictionary, listToDictionaryWithFunc } from 'src/app/shared/utils/dictionary-by-id';
import { ListTable, SetCurrentWorkingItemEventArgs } from '../list-table/list-table.component';

export interface OperationDisplay {
    operation?: Operation;
    amount: string;
    accountingEntryName: string;
    accountBookName: string;
    dateTime: string;
    operatorName: string;
    label: string;
    boarderName: string;
    paymentMethod: string;
}

@Component({
    selector: 'app-operations',
    templateUrl: './operations.component.html',
})
export class OperationsComponent implements OnInit, AfterViewInit {
    public items: Operation[] = [];

    public accountBooks: DictionaryById<AccountBook> = [];
    public accountingEntries: DictionaryById<AccountingEntry> = [];
    public operators: DictionaryById<Operator> = [];
    public boarders: DictionaryById<BoarderListItem> = [];

    public get accountBookList(): AccountBook[] {
        return Object.values(this.accountBooks);
    }

    public get accountingEntryList(): AccountingEntry[] {
        return Object.values(this.accountingEntries);
    }

    public get operatorList(): Operator[] {
        return Object.values(this.operators);
    }

    public get boarderList(): BoarderListItem[] {
        return Object.values(this.boarders);
    }


    public paymentMethods: PaymentMethod[] = allPaymentMethods();

    paymentMethodString = paymentMethodString;

    public itemsDisplayed: OperationDisplay[] = [];

    @ViewChild('listTable') listTable!: ListTable;

    public readonly opsFormGroup = new FormGroup({
        amountCtrl: new FormControl(),
        entryCtrl: new FormControl(),
        bookCtrl: new FormControl(),
        dateTimeCtrl: new FormControl(dateToFormValue(new Date())),
        operCtrl: new FormControl(),
        labelCtrl: new FormControl(),
        boarderCtrl: new FormControl(),
        paymentMethodCtrl: new FormControl(PaymentMethod.Cash),
        paymentMethodDetailsCtrl: new FormControl(),
    });

    constructor(private readonly _opService: OperationService,
        private readonly _accountingEntryService: AccountingEntryService,
        private readonly _accountBookService: AccountBookService,
        private readonly _operatorService: OperatorService,
        private readonly _boarderService: BoarderService,

    ) {
    }

    ngOnInit(): void {
        this.loadContext()
            .then(ctxt => this.load(ctxt));
    }

    public loadContext(): Promise<[DictionaryById<Operator>, DictionaryById<AccountBook>, DictionaryById<AccountingEntry>, DictionaryById<BoarderListItem>]> {
        const operatorsLoaded = this._operatorService.get().pipe(map(listToDictionary));
        operatorsLoaded.subscribe(opers => this.operators = opers, console.error);
        const accountBooksLoaded = this._accountBookService.get().pipe(map(listToDictionary));
        accountBooksLoaded.subscribe(books => this.accountBooks = books, console.error);
        const accountingEntriesLoaded = this._accountingEntryService.get().pipe(map(listToDictionary));
        accountingEntriesLoaded.subscribe(entries => this.accountingEntries = entries, console.error);
        const boardersLoaded = this._boarderService.list().pipe(map(l => listToDictionaryWithFunc(l, boarderItem => boarderItem.boarderId)));
        boardersLoaded.subscribe(boarders => this.boarders = boarders, console.error);
        return Promise.all([operatorsLoaded.toPromise(), accountBooksLoaded.toPromise(), accountingEntriesLoaded.toPromise(), boardersLoaded.toPromise()]);
    }

    public load([opers, books, entries, boarders]: [DictionaryById<Operator>, DictionaryById<AccountBook>, DictionaryById<AccountingEntry>, DictionaryById<BoarderListItem>] = [this.operators, this.accountBooks, this.accountingEntries, this.boarders]): Promise<Operation[]> {
        const operationsLoaded: Observable<Operation[]> = this._opService.get();
        operationsLoaded.subscribe(ops => {
            this.items = ops;
            if (ops.length === 0) {
                this.listTable.addItem();
            }
            const orderedOps = ops.slice();
            orderedOps.sort((op1, op2) => op2.dateTime.getTime() - op1.dateTime.getTime());
            this.itemsDisplayed = orderedOps.map(op => this.createOperationDisplay(op, opers, books, entries, boarders));
        }, console.error);
        return operationsLoaded.toPromise();
    }

    createOperationDisplay(op: Operation, opers: DictionaryById<Operator> = this.operators, books: DictionaryById<AccountBook> = this.accountBooks, entries: DictionaryById<AccountingEntry> = this.accountingEntries, boarders: DictionaryById<BoarderListItem> = this.boarders): OperationDisplay {
        const accountBookName = books[op.accountBookId].label;
        const accountingEntryName = entries[op.accountingEntryId].label;
        const operatorName = op.operatorId ? opers[op.operatorId].name : '';
        const boarderName = op.boarderId ? boarders[op.boarderId].name : '';
        const opDisplay: OperationDisplay = {
            operation: op,
            amount: op.amount.toStringLocale(),
            accountBookName: accountBookName,
            accountingEntryName: accountingEntryName,
            dateTime: op.dateTime.toLocaleDateString(),
            operatorName: operatorName,
            boarderName: boarderName,
            label: op.label,
            paymentMethod: paymentMethodString(op.paymentMethod),
        };
        return opDisplay;
    }

    ngAfterViewInit(): void {
        this.listTable.onCreate.subscribe((): OperationDisplay => {
            return {
                amount: '',
                accountBookName: '',
                accountingEntryName: '',
                boarderName: '',
                dateTime: new Date().toLocaleDateString(),
                label: '',
                operation: undefined,
                operatorName: '',
                paymentMethod: PaymentMethod[this.opsFormGroup.controls.paymentMethodCtrl.value],
            };
        });
        this.listTable.onDelete.subscribe((op: OperationDisplay) => this.delete(op));
        this.listTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<OperationDisplay>) => {
            this.resetValidationErrorMessage();
            if (e.value && e.value.operation) {
                this.opsFormGroup.controls.dateTimeCtrl.setValue(dateToFormValue(e.value.operation.dateTime));
                this.opsFormGroup.controls.amountCtrl.setValue(e.value.amount);
                this.opsFormGroup.controls.bookCtrl.setValue(e.value.operation.accountBookId);
                this.opsFormGroup.controls.entryCtrl.setValue(e.value.operation.accountingEntryId);
                this.opsFormGroup.controls.operCtrl.setValue(e.value.operation.operatorId);
                this.opsFormGroup.controls.boarderCtrl.setValue(e.value.operation.boarderId !== undefined ? e.value.operation.boarderId : undefined);
                this.opsFormGroup.controls.paymentMethodCtrl.setValue(e.value.operation.paymentMethod);
                this.opsFormGroup.controls.labelCtrl.setValue(e.value.operation.label);
            }
            else {
                this.opsFormGroup.controls.dateTimeCtrl.setValue(dateToFormValue(new Date()));
                this.opsFormGroup.controls.amountCtrl.setValue(undefined);
            }
        });
        this.listTable.confirmDeleteMessage = (operator: Operator) => `Supprimer l'opération ?`;
    }

    public onSubmit() {
        const isNewItem = this.listTable.editingNewItem();
        const opDisplay = this.listTable.currentWorkingItem as OperationDisplay | null;
        const amount = this.opsFormGroup.controls.amountCtrl.value ? Amount.fromStringLocale(this.opsFormGroup.controls.amountCtrl.value) : undefined;
        if (!amount) {
            this.resetValidationErrorMessage(`Le montant est invalide.`);
            return;
        }
        const accountBookId = Number.parseInt(this.opsFormGroup.controls.bookCtrl.value);
        if (!(accountBookId in this.accountBooks)) {
            this.resetValidationErrorMessage(`Le livre comptable est invalide.`);
            return;
        }
        const accountingEntryId = Number.parseInt(this.opsFormGroup.controls.entryCtrl.value);
        if (!(accountingEntryId in this.accountingEntries)) {
            this.resetValidationErrorMessage(`L'imputation comptable est invalide.`);
            return;
        }
        const paymentMethod: PaymentMethod = Number.parseInt(this.opsFormGroup.controls.paymentMethodCtrl.value);
        if (!(paymentMethod in PaymentMethod)) {
            this.resetValidationErrorMessage(`Le moyen de paiement est invalide.`);
            return;
        }
        const operatorId = Number.parseInt(this.opsFormGroup.controls.operCtrl.value);
        if (!(operatorId in this.operators)) {
            this.resetValidationErrorMessage(`L'opérateur est invalide.`);
            return;
        }
        let op = opDisplay?.operation;
        if (!op) {
            op = new Operation(undefined, amount, accountingEntryId, paymentMethod, accountBookId, operatorId);
            if (opDisplay) {
                opDisplay.operation = op;
            }
        }
        else {
            op.amount = amount;
            op.accountBookId = accountBookId;
            op.accountingEntryId = accountingEntryId;
            op.paymentMethod = paymentMethod;
            op.operatorId = operatorId;
        }
        if (!this.opsFormGroup.controls.dateTimeCtrl.value) {
            this.resetValidationErrorMessage(`La date est invalide.`);
            return;
        }
        op.dateTime = formValueToDate(this.opsFormGroup.controls.dateTimeCtrl.value);
        const accountingEntry = this.accountingEntries[op.accountingEntryId];
        if (accountingEntry.dependsOnBoarder && this.opsFormGroup.controls.boarderCtrl.value && this.opsFormGroup.controls.boarderCtrl.value != "") {
            op.boarderId = Number.parseInt(this.opsFormGroup.controls.boarderCtrl.value);
        }
        else {
            op.boarderId = undefined;
        }
        op.label = this.opsFormGroup.controls.labelCtrl.value || '';

        this._opService.createUpdate([op]).subscribe(() => {
            this.resetValidationErrorMessage();
            if (isNewItem) {
                this.listTable.addItem();
                this.opsFormGroup.controls.amountCtrl.setValue(undefined);
            }
            else {
                this.opsFormGroup.reset();
                this.listTable.cancelEdit();
            }
            this.load();
        }, err => this.resetValidationErrorMessage(err));
    }

    public delete(opDisplay: OperationDisplay) {
        if (opDisplay.operation?.id !== undefined) {
            this._opService.delete([opDisplay.operation]).subscribe(() => this.load(), console.error);
        }
    }

    public get workingItemEntryDependsOnBoarder(): boolean {
        if (this.opsFormGroup.controls.entryCtrl.value !== undefined) {
            const accountingEntryId = Number.parseInt(this.opsFormGroup.controls.entryCtrl.value);
            if (accountingEntryId in this.accountingEntries) {
                const accountingEntry = this.accountingEntries[accountingEntryId];
                return accountingEntry.dependsOnBoarder;
            }
        }
        return false;
    }

    private _formValidationErrorMessage?: string;

    public get formValidationErrorMessage(): string | undefined {
        return this._formValidationErrorMessage;
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
}

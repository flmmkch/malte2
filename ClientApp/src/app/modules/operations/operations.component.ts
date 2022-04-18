import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, EventEmitter, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { NgbDate, NgbDatepicker, NgbDatepickerI18n, NgbDatepickerI18nDefault, NgbDatepickerNavigateEvent } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, forkJoin, Observable } from 'rxjs';
import { combineLatestWith, map } from 'rxjs/operators';
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
import { datePickerValueToDate, dateRangeFromDatepickerDate, dateRangeFromDatepickerMonthYear, dateToDatePickerValue } from 'src/app/shared/utils/date-time-form-conversion';
import { DictionaryById, listToDictionary, listToDictionaryWithFunc } from 'src/app/shared/utils/dictionary-by-id';
import { ListTable, SetCurrentWorkingItemEventArgs } from '../list-table/list-table.component';

export interface OperationDisplay {
    operation: Operation;
    amount: string;
    accountingEntryName: string;
    accountBookName: string;
    dateTime: string;
    operatorName: string;
    label: string;
    boarderName: string;
    paymentMethod: string;
}

export interface ContextDicts {
    operators: DictionaryById<Operator>;
    books: DictionaryById<AccountBook>;
    entries: DictionaryById<AccountingEntry>;
    boarders: DictionaryById<BoarderListItem>;
}

@Component({
    selector: 'app-operations',
    templateUrl: './operations.component.html',
    styleUrls: ['./operations.component.css'],
    providers: [{ provide: NgbDatepickerI18n, useClass: NgbDatepickerI18nDefault }]
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
        dateTimeCtrl: new FormControl(),
        operCtrl: new FormControl(),
        labelCtrl: new FormControl(),
        boarderCtrl: new FormControl(),
        paymentMethodCtrl: new FormControl(PaymentMethod.Cash),
        paymentCheckNbCtrl: new FormControl(),
        paymentCardNbCtrl: new FormControl(),
        paymentTransferNbCtrl: new FormControl(),
    });

    constructor(private readonly _opService: OperationService,
        private readonly _accountingEntryService: AccountingEntryService,
        private readonly _accountBookService: AccountBookService,
        private readonly _operatorService: OperatorService,
        private readonly _boarderService: BoarderService,

    ) {
        this._contexts = this.loadContext();
    }

    private _contexts: Observable<ContextDicts>;

    private _reloader = new BehaviorSubject(null);

    private _dateNavigation: EventEmitter<[Date, Date]> = new EventEmitter();

    private _currentDateRange!: [Date, Date];

    public get currentDateRange(): [Date, Date] {
        return this._currentDateRange;
    }

    ngOnInit(): void {
        const operationsLoaded: Observable<Operation[]> = this._opService.getOnDateRange(this._dateNavigation);
        operationsLoaded
            .pipe(combineLatestWith(this._contexts, this._reloader))
            .subscribe({
                next: ([ops, context]) => this.rebuildOperations(ops, context),
                error: console.error,
            });
            
        this._dateNavigation.subscribe(([dateBegin, dateEnd]) => this._currentDateRange = [dateBegin, dateEnd]);
    }

    public loadContext(): Observable<ContextDicts> {
        const operatorsLoaded = this._operatorService.get().pipe(map(listToDictionary));
        operatorsLoaded.subscribe({ next: opers => this.operators = opers, error: console.error });
        const accountBooksLoaded = this._accountBookService.get().pipe(map(listToDictionary));
        accountBooksLoaded.subscribe({ next: books => this.accountBooks = books, error: console.error });
        const accountingEntriesLoaded = this._accountingEntryService.get().pipe(map(listToDictionary));
        accountingEntriesLoaded.subscribe({ next: entries => this.accountingEntries = entries, error: console.error });
        const boardersLoaded = this._boarderService.list().pipe(map(l => listToDictionaryWithFunc(l, boarderItem => boarderItem.boarderId)));
        boardersLoaded.subscribe({ next: boarders => this.boarders = boarders, error: console.error });
        return forkJoin({
            operators: operatorsLoaded,
            books: accountBooksLoaded,
            entries: accountingEntriesLoaded,
            boarders: boardersLoaded,
        });
    }

    private rebuildOperations(ops: Operation[], { operators, books, entries, boarders }: ContextDicts) {
        this.items = ops;
        if (ops.length === 0) {
            this.listTable.addItem();
        }
        else {
            this.listTable.cancelEdit();
        }
        const orderedOps = ops.slice();
        orderedOps.sort((op1, op2) => {
            const timeDiff = op2.dateTime.getTime() - op1.dateTime.getTime();
            if (timeDiff !== 0) {
                return timeDiff;
            }
            return (op2.id || 0) - (op1.id || 0);
        });
        this.itemsDisplayed = orderedOps.map(op => this.createOperationDisplay(op, operators, books, entries, boarders));
    }

    public dateNavigation(event: NgbDatepickerNavigateEvent) {
        const [dateBegin, dateEnd] = dateRangeFromDatepickerMonthYear(event.next);
        const date = this.opsFormGroup.controls.dateTimeCtrl.value ? datePickerValueToDate(this.opsFormGroup.controls.dateTimeCtrl.value) : undefined;
        if (date && dateBegin.getMonth() != date.getMonth()) {
            this.opsFormGroup.controls.dateTimeCtrl.setValue(dateToDatePickerValue(dateBegin));
        }
        this._lastOperationEntered = undefined;
        this._dateNavigation.emit([dateBegin, dateEnd]);
    }

    @ViewChild('dateNavigator') dateNavigator!: NgbDatepicker;

    dateNavigatorFormCtrl = new FormControl();

    public dateSelection(eventDate: NgbDate) {
        this.opsFormGroup.controls.dateTimeCtrl.setValue(eventDate);
        const [dateBegin, dateEnd] = dateRangeFromDatepickerDate(eventDate);
        if (this._currentDateRange[0].getTime() === dateBegin.getTime() && this._currentDateRange[1].getTime() === dateEnd.getTime()) {
            dateBegin.setDate(1);
            dateEnd.setMonth(dateBegin.getMonth() + 1);
            dateEnd.setSeconds(dateEnd.getSeconds() - 1);
            this._dateNavigation.emit([dateBegin, dateEnd]);
            this.dateNavigator.writeValue(new NgbDate(1, 1, 1));
            this.dateNavigatorFormCtrl.reset();
        }
        else {
            this._dateNavigation.emit([dateBegin, dateEnd]);
        }
    }

    createOperationDisplay(op: Operation, opers: DictionaryById<Operator> = this.operators, books: DictionaryById<AccountBook> = this.accountBooks, entries: DictionaryById<AccountingEntry> = this.accountingEntries, boarders: DictionaryById<BoarderListItem> = this.boarders): OperationDisplay {
        const accountBookName = op.accountBookId in books ? books[op.accountBookId].label : '';
        const accountingEntryName = op.accountingEntryId in entries ? entries[op.accountingEntryId].label : '';
        const operatorName = op.operatorId in opers ? opers[op.operatorId].name : '';
        const boarderName = op.boarderId && op.boarderId in boarders ? boarders[op.boarderId].name : '';
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

    private createNewOpDisplay(copyOldOpDisplay?: OperationDisplay): OperationDisplay {
        let operation = new Operation(undefined, Amount.from(0)!, -1, PaymentMethod.Card, -1, -1);
        if (copyOldOpDisplay && copyOldOpDisplay.operation) {
            operation.accountBookId = copyOldOpDisplay.operation.accountBookId;
            operation.accountingEntryId = copyOldOpDisplay.operation.accountingEntryId;
            operation.boarderId = copyOldOpDisplay.operation.boarderId;
            operation.dateTime = copyOldOpDisplay.operation.dateTime;
            operation.label = copyOldOpDisplay.operation.label;
            operation.operatorId = copyOldOpDisplay.operation.operatorId;
            operation.paymentMethod = copyOldOpDisplay.operation.paymentMethod;
            operation.checkNumber = copyOldOpDisplay.operation.checkNumber ? copyOldOpDisplay.operation.checkNumber + BigInt(1) : undefined;
            operation.cardNumber = copyOldOpDisplay.operation.cardNumber;
            operation.transferNumber = copyOldOpDisplay.operation.transferNumber ? copyOldOpDisplay.operation.transferNumber + BigInt(1) : undefined;
        }
        else {
            operation.dateTime = this.opsFormGroup.controls.dateTimeCtrl.value ? datePickerValueToDate(this.opsFormGroup.controls.dateTimeCtrl.value) : new Date();
        }
        const opDisplay = this.createOperationDisplay(operation);
        opDisplay.amount = '';
        return opDisplay;
    }

    private _lastOperationEntered?: OperationDisplay;

    ngAfterViewInit(): void {
        this.listTable.onCreate.subscribe(() => {
            let opDisplay: OperationDisplay = this.createNewOpDisplay(this._lastOperationEntered);
            this.listTable.currentWorkingItem = opDisplay;
        });
        this.listTable.onDelete.subscribe((op: OperationDisplay) => this.delete(op));
        this.listTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<OperationDisplay>) => {
            this.resetValidationErrorMessage();
            if (e.value && e.value.operation) {
                this.opsFormGroup.controls.dateTimeCtrl.setValue(dateToDatePickerValue(e.value.operation.dateTime));
                this.opsFormGroup.controls.amountCtrl.setValue(e.value.amount);
                this.opsFormGroup.controls.bookCtrl.setValue(e.value.operation.accountBookId);
                this.opsFormGroup.controls.entryCtrl.setValue(e.value.operation.accountingEntryId);
                this.opsFormGroup.controls.operCtrl.setValue(e.value.operation.operatorId);
                this.opsFormGroup.controls.boarderCtrl.setValue(e.value.operation.boarderId !== undefined ? e.value.operation.boarderId : undefined);
                this.opsFormGroup.controls.paymentMethodCtrl.setValue(e.value.operation.paymentMethod);
                this.opsFormGroup.controls.labelCtrl.setValue(e.value.operation.label);
                this.opsFormGroup.controls.paymentCheckNbCtrl.setValue(e.value.operation.checkNumber?.toString());
                this.opsFormGroup.controls.paymentCardNbCtrl.setValue(e.value.operation.cardNumber);
                this.opsFormGroup.controls.paymentTransferNbCtrl.setValue(e.value.operation.transferNumber?.toString());
            }
            else {
                this.opsFormGroup.controls.amountCtrl.setValue(undefined);
            }
        });
        this.listTable.confirmDeleteMessage = (operator: Operator) => `Supprimer l'opération ?`;
    }

    public onSubmit() {
        const isNewItem = this.listTable.editingNewItem();
        const opDisplay = this.listTable.currentWorkingItem as OperationDisplay | null;
        if (opDisplay) {
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
            let op = opDisplay.operation;
            op.amount = amount;
            op.accountBookId = accountBookId;
            op.accountingEntryId = accountingEntryId;
            op.paymentMethod = paymentMethod;
            op.operatorId = operatorId;
            if (!this.opsFormGroup.controls.dateTimeCtrl.value) {
                this.resetValidationErrorMessage(`La date est invalide.`);
                return;
            }
            op.dateTime = datePickerValueToDate(this.opsFormGroup.controls.dateTimeCtrl.value);
            const accountingEntry = this.accountingEntries[op.accountingEntryId];
            if (accountingEntry.dependsOnBoarder && this.opsFormGroup.controls.boarderCtrl.value && this.opsFormGroup.controls.boarderCtrl.value != "") {
                op.boarderId = Number.parseInt(this.opsFormGroup.controls.boarderCtrl.value);
            }
            else {
                op.boarderId = undefined;
            }
            op.label = this.opsFormGroup.controls.labelCtrl.value || '';
            // détails du paiement
            if (op.paymentMethod === PaymentMethod.Check) {
                op.checkNumber = undefined;
                if (this.opsFormGroup.controls.paymentCheckNbCtrl.value) {
                    try {
                        op.checkNumber = BigInt(this.opsFormGroup.controls.paymentCheckNbCtrl.value)
                    }
                    catch {
                        // nothing
                    }
                }
                if (op.checkNumber === undefined) {
                    this.resetValidationErrorMessage(`Le numéro de chèque est invalide.`);
                    return;
                }
            }
            else {
                op.checkNumber = undefined;
            }

            if (op.paymentMethod === PaymentMethod.Card) {
                op.cardNumber = this.opsFormGroup.controls.paymentCardNbCtrl.value;
            }
            else {
                op.cardNumber = undefined;
            }

            if (op.paymentMethod === PaymentMethod.Transfer) {
                op.transferNumber = undefined;
                if (this.opsFormGroup.controls.paymentTransferNbCtrl.value) {
                    try {
                        op.transferNumber = BigInt(this.opsFormGroup.controls.paymentTransferNbCtrl.value)
                    }
                    catch {
                        // nothing
                    }
                }
            }
            else {
                op.transferNumber = undefined;
            }
            this._lastOperationEntered = opDisplay;

            this._opService.createUpdate([op]).subscribe({
                next: () => {
                    this.resetValidationErrorMessage();
                    if (isNewItem) {
                        this.listTable.addItem();
                        const oldOpDisplay = this.createOperationDisplay(op!);
                        let opDisplay: OperationDisplay = this.createNewOpDisplay(oldOpDisplay);
                        this.listTable.currentWorkingItem = opDisplay;
                    }
                    else {
                        this.listTable.cancelEdit();
                    }
                    this.reload();
                },
                error: err => this.resetValidationErrorMessage(err)
            });
        }
    }

    public reload() {
        this._reloader.next(null);
    }

    public delete(opDisplay: OperationDisplay) {
        if (opDisplay.operation?.id !== undefined) {
            this._opService.delete([opDisplay.operation]).subscribe({ next: () => this.reload(), error: console.error });
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

    public get workingItemIsCheckPayment(): boolean {
        return this.workingItemPaymentMethod == PaymentMethod.Check;
    }

    public get workingItemIsCardPayment(): boolean {
        return this.workingItemPaymentMethod == PaymentMethod.Card;
    }

    public get workingItemIsTransferPayment(): boolean {
        return this.workingItemPaymentMethod == PaymentMethod.Transfer;
    }

    public get workingItemPaymentMethod(): PaymentMethod | undefined {
        if (this.opsFormGroup.controls.paymentMethodCtrl.value !== undefined) {
            const paymentMethod: PaymentMethod = Number.parseInt(this.opsFormGroup.controls.paymentMethodCtrl.value);
            if (paymentMethod in PaymentMethod) {
                return paymentMethod;
            }
        }
        return undefined;
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

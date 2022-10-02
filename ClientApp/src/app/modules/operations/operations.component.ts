import { getLocaleDateFormat } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, EventEmitter, Injectable, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { NgbDate, NgbDateParserFormatter, NgbDatepicker, NgbDatepickerI18n, NgbDatepickerI18nDefault, NgbDatepickerNavigateEvent, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, forkJoin, Observable } from 'rxjs';
import { combineLatestWith, map } from 'rxjs/operators';
import { AccountBook } from 'src/app/shared/models/account-book.model';
import { AccountingCategory } from 'src/app/shared/models/accounting-category.model';
import { AccountingEntry, EntryType } from 'src/app/shared/models/accounting-entry.model';
import { Amount } from 'src/app/shared/models/amount.model';
import { BoarderListItem } from 'src/app/shared/models/boarder.model';
import { Operation } from 'src/app/shared/models/operation.model';
import { Operator } from 'src/app/shared/models/operator.model';
import { allPaymentMethods, PaymentMethod, paymentMethodString } from 'src/app/shared/models/payment-method.model';
import { AccountBookService } from 'src/app/shared/services/account-book.service';
import { AccountingCategoryService } from 'src/app/shared/services/accounting-category.service';
import { AccountingEntryService } from 'src/app/shared/services/accounting-entry.service';
import { BoarderService } from 'src/app/shared/services/boarder.service';
import { OperationFilters, OperationService } from 'src/app/shared/services/operation.service';
import { OperatorService } from 'src/app/shared/services/operator.service';
import { datePickerValueToDate, dateRangeFromDatepickerDate, dateRangeFromDatepickerMonthYear, dateToDatePickerValue } from 'src/app/shared/utils/date-time-form-conversion';
import { DictionaryById, listToDictionary, listToDictionaryWithFunc } from 'src/app/shared/utils/dictionary-by-id';
import { FrenchDateParserFormatter } from 'src/app/shared/utils/french-date-parser-formatter';
import { ListTable, SetCurrentWorkingItemEventArgs } from '../list-table/list-table.component';

export interface OperationDisplay {
    operation: Operation;
    amount: string;
    accountingEntryName: string;
    categoryName: string;
    accountBookName: string;
    dateTime: string;
    operatorName: string;
    label: string;
    boarderName: string;
    paymentMethod: string;
}

interface ContextDicts {
    operators: DictionaryById<Operator>;
    books: DictionaryById<AccountBook>;
    entries: DictionaryById<AccountingEntry>;
    categories: DictionaryById<AccountingCategory>;
    boarders: DictionaryById<BoarderListItem>;
}

@Component({
    selector: 'app-operations',
    templateUrl: './operations.component.html',
    styleUrls: ['./operations.component.css'],
    providers: [
        { provide: NgbDatepickerI18n, useClass: NgbDatepickerI18nDefault },
        {provide: NgbDateParserFormatter, useClass: FrenchDateParserFormatter }
    ]
})
export class OperationsComponent implements OnInit, AfterViewInit {
    public items: Operation[] = [];

    public accountBooks: DictionaryById<AccountBook> = [];
    public accountingEntries: DictionaryById<AccountingEntry> = [];
    public categories: DictionaryById<AccountingCategory> = [];
    public operators: DictionaryById<Operator> = [];
    public boarders: DictionaryById<BoarderListItem> = [];

    public get accountBookList(): AccountBook[] {
        return Object.values(this.accountBooks);
    }

    public get accountingEntryList(): AccountingEntry[] {
        return Object.values(this.accountingEntries);
    }

    public categoryListForEntry(accountingEntryId?: number | string): AccountingCategory[] {
        let entryCategories = Object.values(this.categories)
        if (accountingEntryId != null) {
            if (typeof accountingEntryId === 'string') {
                accountingEntryId = Number.parseInt(accountingEntryId);
            }
            entryCategories = entryCategories.filter(category => category.accountingEntryId == null || category.accountingEntryId === accountingEntryId);
        }
        return [... entryCategories];
    }

    public get operatorList(): Operator[] {
        return Object.values(this.operators);
    }

    public get boarderList(): BoarderListItem[] {
        return Object.values(this.boarders);
    }


    public paymentMethods: PaymentMethod[] = allPaymentMethods();

    public readonly paymentMethodString = paymentMethodString;

    public filters: OperationFilters = {};

    public get filteringPaymentMethod(): PaymentMethod | null {
        return this.filters.paymentMethod || null;
    }
    
    public set filteringPaymentMethod(value: PaymentMethod | null) {
        this.filters.paymentMethod = value || undefined;
    }

    public get filteringAccountBook(): AccountBook | null {
        return this.filters.accountBook || null;
    }
    
    public set filteringAccountBook(value: AccountBook | null) {
        this.filters.accountBook = value || undefined;
    }

    public get filteringAccountingEntry(): AccountingEntry | null {
        return this.filters.accountingEntry || null;
    }
    
    public set filteringAccountingEntry(value: AccountingEntry | null) {
        this.filters.accountingEntry = value || undefined;
    }

    public get filteringAccountingCategory(): AccountingCategory | null {
        return this.filters.category || null;
    }
    
    public set filteringAccountingCategory(value: AccountingCategory | null) {
        this.filters.category = value || undefined;
    }

    public updateFilters() {
        this.rebuildOperations();
    }

    public itemsDisplayed: OperationDisplay[] = [];

    @ViewChild('listTable') listTable!: ListTable;

    public readonly opsFormGroup = new FormGroup({
        amountCtrl: new FormControl(),
        entryCtrl: new FormControl(),
        categoryCtrl: new FormControl(),
        bookCtrl: new FormControl(),
        dateTimeCtrl: new FormControl(),
        operCtrl: new FormControl(),
        labelCtrl: new FormControl(),
        detailsCtrl: new FormControl(),
        invoiceCtrl: new FormControl(),
        boarderCtrl: new FormControl(),
        paymentMethodCtrl: new FormControl(PaymentMethod.Cash),
        paymentCheckNbCtrl: new FormControl(),
        paymentCardTicketNbCtrl: new FormControl(),
        paymentTransferNbCtrl: new FormControl(),
    });

    constructor(readonly opService: OperationService,
        private readonly _accountingEntryService: AccountingEntryService,
        private readonly _categoryService: AccountingCategoryService,
        private readonly _accountBookService: AccountBookService,
        private readonly _operatorService: OperatorService,
        private readonly _boarderService: BoarderService,

    ) {
        this._contexts = this.loadContext();
    }

    private _contexts: Observable<ContextDicts>;

    private _dateNavigation: EventEmitter<[Date, Date]> = new EventEmitter();

    private _currentDateRange!: [Date, Date];

    public get currentDateRange(): [Date, Date] {
        return this._currentDateRange;
    }

    ngOnInit(): void {
        const operationsLoaded: Observable<Operation[]> = this.opService.getOnDateRange(this._dateNavigation);
        operationsLoaded
            .pipe(combineLatestWith(this._contexts))
            .subscribe({
                next: ([ops, context]) => {
                    this.items = ops;
                    this.rebuildOperations(ops, context);
                },
                error: console.error,
            });

        operationsLoaded.subscribe(() => this.rebuildLabels());
            
        this._dateNavigation.subscribe(([dateBegin, dateEnd]) => this._currentDateRange = [dateBegin, dateEnd]);
    }

    public loadContext(): Observable<ContextDicts> {
        const operatorsLoaded = this._operatorService.get().pipe(map(listToDictionary));
        operatorsLoaded.subscribe({ next: opers => this.operators = opers, error: console.error });
        const accountBooksLoaded = this._accountBookService.get().pipe(map(listToDictionary));
        accountBooksLoaded.subscribe({ next: books => this.accountBooks = books, error: console.error });
        const accountingEntriesLoaded = this._accountingEntryService.get().pipe(map(listToDictionary));
        accountingEntriesLoaded.subscribe({ next: entries => this.accountingEntries = entries, error: console.error });
        const categoriesLoaded = this._categoryService.get().pipe(map(listToDictionary));
        categoriesLoaded.subscribe({ next: categories => this.categories = categories, error: console.error });
        const boardersLoaded = this._boarderService.list().pipe(map(l => listToDictionaryWithFunc(l, boarderItem => boarderItem.boarderId)));
        boardersLoaded.subscribe({ next: boarders => this.boarders = boarders, error: console.error });
        return forkJoin({
            operators: operatorsLoaded,
            books: accountBooksLoaded,
            entries: accountingEntriesLoaded,
            categories: categoriesLoaded,
            boarders: boardersLoaded,
        });
    }

    private rebuildOperations(ops: Operation[] = this.items, { operators, books, entries, categories, boarders }: ContextDicts =  { books: this.accountBooks, entries: this.accountingEntries, categories: this.categories, operators: this.operators, boarders: this.boarders }) {
        if (ops.length === 0) {
            this.listTable.addItem();
        }
        else {
            this.listTable.cancelEdit();
        }
        let orderedOps = ops.slice();
        // apply filters
        if (this.filteringPaymentMethod) {
            orderedOps = orderedOps.filter(op => this.filteringPaymentMethod === op.paymentMethod);
        }
        if (this.filteringAccountBook) {
            orderedOps = orderedOps.filter(op => this.filteringAccountBook?.id === op.accountBookId);
        }
        if (this.filteringAccountingEntry) {
            orderedOps = orderedOps.filter(op => this.filteringAccountingEntry?.id === op.accountingEntryId);
        }
        if (this.filteringAccountingCategory) {
            orderedOps = orderedOps.filter(op => this.filteringAccountingCategory?.id === op.categoryId);
        }
        // sort by date
        orderedOps.sort((op1, op2) => {
            const timeDiff = op2.dateTime.getTime() - op1.dateTime.getTime();
            if (timeDiff !== 0) {
                return timeDiff;
            }
            return (op2.id || 0) - (op1.id || 0);
        });
        // build items displayed
        this.itemsDisplayed = orderedOps
            .map(op => this.createOperationDisplay(op, { operators, books, entries, categories, boarders }));
        this.recalculateTotals(this.itemsDisplayed.map(itemDisplayed => itemDisplayed.operation));
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

    createOperationDisplay(op: Operation, { books, entries, categories, operators, boarders }: ContextDicts = { books: this.accountBooks, entries: this.accountingEntries, categories: this.categories, operators: this.operators, boarders: this.boarders }): OperationDisplay {
        const accountBookName = op.accountBookId in books ? books[op.accountBookId].label : '';
        const accountingEntryName = op.accountingEntryId in entries ? entries[op.accountingEntryId].label : '';
        const categoryName = (op.categoryId !== undefined && op.categoryId in categories) ? categories[op.categoryId].label : '';
        const operatorName = op.operatorId in operators ? operators[op.operatorId].name : '';
        const boarderName = op.boarderId && op.boarderId in boarders ? boarders[op.boarderId].name : '';
        const opDisplay: OperationDisplay = {
            operation: op,
            amount: op.amount.toLocaleString(),
            accountBookName,
            categoryName,
            accountingEntryName,
            dateTime: op.dateTime.toLocaleDateString(),
            operatorName,
            boarderName,
            label: op.label,
            paymentMethod: paymentMethodString(op.paymentMethod),
        };
        return opDisplay;
    }

    private createNewOpDisplay(copyOldOpDisplay?: OperationDisplay): OperationDisplay {
        // create operation
        let operation = new Operation(undefined, Amount.from(0)!, -1, PaymentMethod.Cash, -1, -1);
        // account for filters
        if (this.filteringPaymentMethod) {
            operation.paymentMethod = this.filteringPaymentMethod;
        }
        if (this.filteringAccountBook && typeof this.filteringAccountBook.id === 'number') {
            operation.accountBookId = this.filteringAccountBook.id;
        }
        if (this.filteringAccountingEntry && typeof this.filteringAccountingEntry.id === 'number') {
            operation.accountingEntryId = this.filteringAccountingEntry.id;
        }
        if (this.filteringAccountingCategory && typeof this.filteringAccountingCategory.id === 'number') {
            operation.categoryId = this.filteringAccountingCategory.id;
            if (typeof this.filteringAccountingCategory.accountingEntryId === 'number') {
                operation.accountingEntryId = this.filteringAccountingCategory.accountingEntryId;
            }
        }

        // date
        const today = new Date();
        if (today >= this._currentDateRange[0] && today <= this._currentDateRange[1]) {
            operation.dateTime = today;
        }
        else {
            operation.dateTime = this._currentDateRange[0];
        }

        // copy previous operation
        if (copyOldOpDisplay && copyOldOpDisplay.operation) {
            operation.accountBookId = copyOldOpDisplay.operation.accountBookId;
            operation.accountingEntryId = copyOldOpDisplay.operation.accountingEntryId;
            operation.categoryId = copyOldOpDisplay.operation.categoryId;
            operation.boarderId = copyOldOpDisplay.operation.boarderId;
            operation.label = copyOldOpDisplay.operation.label;
            operation.operatorId = copyOldOpDisplay.operation.operatorId;
            operation.paymentMethod = copyOldOpDisplay.operation.paymentMethod;
            operation.checkNumber = copyOldOpDisplay.operation.checkNumber ? copyOldOpDisplay.operation.checkNumber + BigInt(1) : undefined;
            operation.cardTicketNumber = copyOldOpDisplay.operation.cardTicketNumber ? copyOldOpDisplay.operation.cardTicketNumber + BigInt(1) : undefined;
            operation.transferNumber = copyOldOpDisplay.operation.transferNumber ? copyOldOpDisplay.operation.transferNumber + BigInt(1) : undefined;
        }

        // create the operation display
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
                this.opsFormGroup.controls.categoryCtrl.setValue(e.value.operation.categoryId);
                this.opsFormGroup.controls.operCtrl.setValue(e.value.operation.operatorId);
                this.opsFormGroup.controls.boarderCtrl.setValue(e.value.operation.boarderId !== undefined ? e.value.operation.boarderId : undefined);
                this.opsFormGroup.controls.paymentMethodCtrl.setValue(e.value.operation.paymentMethod);
                this.opsFormGroup.controls.labelCtrl.setValue(e.value.operation.label);
                this.opsFormGroup.controls.detailsCtrl.setValue(e.value.operation.details);
                this.opsFormGroup.controls.invoiceCtrl.setValue(e.value.operation.invoice);
                this.opsFormGroup.controls.paymentCheckNbCtrl.setValue(e.value.operation.checkNumber?.toString());
                this.opsFormGroup.controls.paymentCardTicketNbCtrl.setValue(e.value.operation.cardTicketNumber?.toString());
                this.opsFormGroup.controls.paymentTransferNbCtrl.setValue(e.value.operation.transferNumber?.toString());
            }
            else {
                this.opsFormGroup.controls.amountCtrl.setValue(undefined);
                this.opsFormGroup.controls.labelCtrl.setValue(undefined);
                this.opsFormGroup.controls.detailsCtrl.setValue(undefined);
                this.opsFormGroup.controls.invoiceCtrl.setValue(undefined);
                this.opsFormGroup.controls.paymentMethodCtrl.setValue(undefined);
                this.opsFormGroup.controls.entryCtrl.setValue(undefined);
                this.opsFormGroup.controls.categoryCtrl.setValue(undefined);
            }
        });
        this.listTable.confirmDeleteMessage = () => `Supprimer l'opération ?`;
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
            const categoryId = this.opsFormGroup.controls.categoryCtrl.value ? Number.parseInt(this.opsFormGroup.controls.categoryCtrl.value) : undefined;
            if (typeof categoryId === 'number' && !(categoryId in this.categories)) {
                this.resetValidationErrorMessage(`La catégorie est invalide.`);
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
            op.categoryId = categoryId;
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
            op.details = this.opsFormGroup.controls.detailsCtrl.value || '';
            op.invoice = this.opsFormGroup.controls.invoiceCtrl.value;
            
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
                if (this.opsFormGroup.controls.paymentCardTicketNbCtrl.value) {
                    try {
                        op.cardTicketNumber = BigInt(this.opsFormGroup.controls.paymentCardTicketNbCtrl.value);
                    }
                    catch {
                        // nothing
                    }
                    if (op.cardTicketNumber === undefined) {
                        this.resetValidationErrorMessage(`Le numéro de ticket de carte est invalide.`);
                        return;
                    }
                }
            }
            else {
                op.cardTicketNumber = undefined;
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

            this.opService.createUpdate([op]).subscribe({
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
        this._dateNavigation.emit(this._currentDateRange);
    }

    public delete(opDisplay: OperationDisplay) {
        if (opDisplay.operation?.id !== undefined) {
            this.opService.delete([opDisplay.operation]).subscribe({ next: () => this.reload(), error: console.error });
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

    private calculateTotal(ops: Operation[], entryType: EntryType): Amount {
        interface OpEntry {
            operation: Operation,
            accountingEntry: AccountingEntry,
        }
        return ops
            .map(op => <OpEntry> { operation: op, accountingEntry: this.accountingEntries[op.accountingEntryId] })
            .filter(({ accountingEntry }) => accountingEntry.entryType == entryType)
            .map(({ operation }) => operation.amount)
            .reduce((prev, cur) => prev.add(cur), Amount.from(0)!)
            ;
    }

    private recalculateTotals(ops: Operation[]) {
        this.totalDisplayedRevenue = this.calculateTotal(ops, EntryType.Revenue);
        this.totalDisplayedExpense = this.calculateTotal(ops, EntryType.Expense);
        this.totalDisplayedBalance = this.totalDisplayedRevenue.substract(this.totalDisplayedExpense);
    }

    totalDisplayedRevenue: Amount = Amount.from(0)!;

    totalDisplayedExpense: Amount = Amount.from(0)!;

    totalDisplayedBalance: Amount = Amount.from(0)!;

    public labels: string[] = [];

    private rebuildLabels() {
        this.opService.getLabels()
            .subscribe({
                next: labels => {
                    this.labels = labels;
                },
                error: console.error,
            });
    }

    public get invoices(): string[] {
        const invoices: string[] = [];
        for (const op of this.itemsDisplayed.map(displayOp => displayOp.operation)) {
            if (op.invoice && !invoices.includes(op.invoice)) {
                invoices.push(op.invoice);
            }
        }
        return invoices;
    }
}

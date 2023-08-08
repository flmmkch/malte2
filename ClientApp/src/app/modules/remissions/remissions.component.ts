import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, ElementRef, EventEmitter, Injectable, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { NgbDate, NgbDateParserFormatter, NgbDatepicker, NgbDatepickerI18n, NgbDatepickerI18nDefault, NgbDatepickerNavigateEvent, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { forkJoin, Observable, Subscription } from 'rxjs';
import { combineLatestWith, map } from 'rxjs/operators';
import { Amount } from 'src/app/shared/models/amount.model';
import { allCashValueItems as cashValues, CashDeposit, CashValue, CheckRemission, getCashValueAmount, Remission, RemissionOperationCheck } from 'src/app/shared/models/remission.model';
import { Operator } from 'src/app/shared/models/operator.model';
import { RemissionService } from 'src/app/shared/services/remission.service';
import { OperatorService } from 'src/app/shared/services/operator.service';
import { datePickerValueToDate, dateRangeFromDatepickerDate, dateRangeFromDatepickerMonthYear, dateToDatePickerValue } from 'src/app/shared/utils/date-time-form-conversion';
import { ListTable, SetCurrentWorkingItemEventArgs } from '../list-table/list-table.component';
import { FrenchDateParserFormatter } from 'src/app/shared/utils/french-date-parser-formatter';
import { DictionaryById, listToDictionary } from 'src/app/shared/utils/dictionary-by-id';

export interface RemissionDisplay {
    remission: Remission;
    cashAmount: string;
    checkAmount: string;
    totalAmount: string;
    dateTime: string;
    operatorName: string;
    notes: string;
}

interface ContextDicts {
    operators: DictionaryById<Operator>;
}

interface CashValueFormControl {
    formControl: FormControl,
    cashValue: CashValue,
    formControlName: string,
}

@Component({
    selector: 'app-remissions',
    templateUrl: './remissions.component.html',
    styleUrls: ['./remissions.component.css'],
    providers: [
        { provide: NgbDatepickerI18n, useClass: NgbDatepickerI18nDefault },
        { provide: NgbDateParserFormatter, useClass: FrenchDateParserFormatter }
    ]
})
export class RemissionsComponent implements OnInit, AfterViewInit {
    public items: Remission[] = [];

    @ViewChild('listTable') listTable!: ListTable;

    public readonly remissionFormGroup: FormGroup = new FormGroup({
        dateTimeCtrl: new FormControl(),
        operCtrl: new FormControl(),
        notesCtrl: new FormControl(),
        ...{}
    });
    
    public operators: DictionaryById<Operator> = [];

    constructor(readonly remissionService: RemissionService,
        private readonly _operatorService: OperatorService,
    ) {
        // cash value form controls
        this.cashValueFormControls = cashValues().map(cashValue => <CashValueFormControl> { formControl: new FormControl(0), cashValue, formControlName: `c${cashValue}input` });
        for (let cashValueFormControl of this.cashValueFormControls) {
            this.remissionFormGroup.addControl(cashValueFormControl.formControlName, cashValueFormControl.formControl);
        }
        // load context
        this._contexts = this.loadContext();
    }

    private _contexts: Observable<ContextDicts>;
    private _dateNavigation: EventEmitter<[Date, Date]> = new EventEmitter();

    private _currentDateRange!: [Date, Date];

    public get currentDateRange(): [Date, Date] {
        return this._currentDateRange;
    }

    ngOnInit(): void {
        // load remissions
        const remissionsLoaded: Observable<Remission[]> = this.remissionService.getOnDateRange(this._dateNavigation);
        remissionsLoaded
           .pipe(combineLatestWith(this._contexts))
            .subscribe({
                next: ([remissions, context]) => {
                    this.items = remissions;
                    this.rebuildRemissions(remissions, context);
                },
                error: console.error,
            });
            
        this._dateNavigation.subscribe(([dateBegin, dateEnd]) => this._currentDateRange = [dateBegin, dateEnd]);
    }

    public loadContext(): Observable<ContextDicts> {
        const operatorsLoaded = this._operatorService.get().pipe(map(listToDictionary));
        operatorsLoaded.subscribe({ next: opers => this.operators = opers, error: console.error });
        return forkJoin({
            operators: operatorsLoaded,
        });
    }

    public get operatorList(): Operator[] {
        return Object.values(this.operators);
    }

    private rebuildRemissions(remissions: Remission[] = this.items, { operators }: ContextDicts =  { operators: this.operators }) {
        if (remissions.length === 0) {
            this.listTable.addItem();
        }
        else {
            this.listTable.cancelEdit();
        }
        const orderedOps = remissions.slice();
        orderedOps.sort((left, right) => {
            const timeDiff = right.dateTime.getTime() - left.dateTime.getTime();
            if (timeDiff !== 0) {
                return timeDiff;
            }
            return (right.id || 0) - (left.id || 0);
        });
        this.recalculateTotals(remissions);
        this.itemsDisplayed = remissions.map(remission => this.createRemissionDisplay(remission));
    }

    public dateNavigation(event: NgbDatepickerNavigateEvent) {
        const [dateBegin, dateEnd] = dateRangeFromDatepickerMonthYear(event.next);
        const date = this.remissionFormGroup.controls.dateTimeCtrl.value ? datePickerValueToDate(this.remissionFormGroup.controls.dateTimeCtrl.value) : undefined;
        if (date && dateBegin.getMonth() != date.getMonth()) {
            this.remissionFormGroup.controls.dateTimeCtrl.setValue(dateToDatePickerValue(dateBegin));
        }
        this._lastRemissionEntered = undefined;
        this._dateNavigation.emit([dateBegin, dateEnd]);
    }

    @ViewChild('dateNavigator') dateNavigator!: NgbDatepicker;

    dateNavigatorFormCtrl = new FormControl();

    public dateSelection(eventDate: NgbDate) {
        this.remissionFormGroup.controls.dateTimeCtrl.setValue(eventDate);
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

    public itemsDisplayed: RemissionDisplay[] = [];

    createRemissionDisplay(remission: Remission, operators: DictionaryById<Operator> = this.operators): RemissionDisplay {
        const operatorName = remission.operatorId in operators ? operators[remission.operatorId].name : '';
        const opDisplay: RemissionDisplay = {
            remission: remission,
            dateTime: remission.dateTime.toLocaleDateString(),
            operatorName,
            notes: remission.notes,
            cashAmount: `${remission.totalCashAmount.toLocaleString()} €`,
            checkAmount: `${remission.totalCheckAmount.toLocaleString()} €`,
            totalAmount: `${remission.totalAmount.toLocaleString()} €`,
        };
        return opDisplay;
    }

    private createNewRemissionDisplay(copyOldRemissionDisplay?: RemissionDisplay): RemissionDisplay {
        let remission = new Remission(undefined, copyOldRemissionDisplay?.remission?.operatorId || this.operatorList[0].id || 0);
        const today = new Date();
        if (today >= this._currentDateRange[0] && today <= this._currentDateRange[1]) {
            remission.dateTime = today;
        }
        else {
            remission.dateTime = this._currentDateRange[0];
        }
        if (copyOldRemissionDisplay && copyOldRemissionDisplay.remission) {
            remission.operatorId = copyOldRemissionDisplay.remission.operatorId;
        }
        return this.createRemissionDisplay(remission);
    }

    private _lastRemissionEntered?: RemissionDisplay;

    ngAfterViewInit(): void {
        this.listTable.onCreate.subscribe(() => {
            let opDisplay: RemissionDisplay = this.createNewRemissionDisplay(this._lastRemissionEntered);
            this.listTable.currentWorkingItem = opDisplay;
        });
        this.listTable.onDelete.subscribe((op: RemissionDisplay) => this.delete(op));
        this.listTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<RemissionDisplay>) => this.setWorkingItem(e));
        this.listTable.confirmDeleteMessage = (itemDisplay: RemissionDisplay) => `Supprimer le dépôt bancaire du ${itemDisplay.remission.dateTime.toLocaleDateString()} ?`;
    }

    private setWorkingItem(e: SetCurrentWorkingItemEventArgs<RemissionDisplay>) {
        this.resetValidationErrorMessage();
        if (e.value && e.value.remission) {
            this.remissionFormGroup.controls.dateTimeCtrl.setValue(dateToDatePickerValue(e.value.remission.dateTime));
            this.remissionFormGroup.controls.operCtrl.setValue(e.value.remission.operatorId);
            this.remissionFormGroup.controls.notesCtrl.setValue(e.value.remission.notes);
            // cash
            for (let cashValueFormControl of this.cashValueFormControls) {
                let cashDeposit = e.value.remission.cashDeposits.find(deposit => deposit.value == cashValueFormControl.cashValue);
                if (cashDeposit !== undefined) {
                    cashValueFormControl.formControl.setValue(cashDeposit.count.toString());
                }
                else {
                    cashValueFormControl.formControl.setValue(0);
                }
            }
            this.workingItemChecks = [... e.value.remission.checkRemissions];
        }
        else {
            this.remissionFormGroup.controls.dateTimeCtrl.setValue(undefined);
            this.remissionFormGroup.controls.operCtrl.setValue(undefined);
            this.remissionFormGroup.controls.notesCtrl.setValue(undefined);
            for (let cashValueFormControl of this.cashValueFormControls) {
                cashValueFormControl.formControl.setValue(0);
            }
            this.workingItemChecks = [];
        }
        this.checkFormGroup.controls.checkNumberCtrl.setValue(undefined);
        this.checkFormGroup.controls.checkAmountCtrl.setValue(undefined);
        this.currentAvailableChecksSubscription?.unsubscribe();
        this.workingItemOperationChecks = [];
        if (e.value) {
            this.reloadAvailableChecks(e.value?.remission.dateTime);
        }
    }

    @ViewChild('cashFormGroup') cashFormGroup!: ElementRef<HTMLElement>;

    private cashValueFormControls: CashValueFormControl[] = [];

    public get workingItemCashDeposits(): CashDeposit[] {
        return this.cashValueFormControls.map(cashValueFormInput => new CashDeposit(cashValueFormInput.cashValue, BigInt(Number.parseInt(cashValueFormInput.formControl.value || 0)))).filter(deposit => deposit.count > 0);
    }

    public get workingItemTotalCashAmount(): Amount {
        return this.calculateTotal(this.workingItemCashDeposits, (cashDeposit) => cashDeposit.totalAmount);
    }

    workingItemChecks: CheckRemission[] = [];

    public get workingItemTotalCheckAmount(): Amount {
        return this.calculateTotal(this.workingItemChecks, (checkRemission) => checkRemission.amount);
    }

    public readonly checkFormGroup = new FormGroup({
        checkNumberCtrl: new FormControl(),
        checkAmountCtrl: new FormControl(),
    });

    public addCheck() {
        let checkAmount = Amount.from(this.checkFormGroup.controls.checkAmountCtrl.value);
        let checkNumber = this.checkFormGroup.controls.checkNumberCtrl.value;
        if ((typeof checkNumber === 'string' || checkNumber === undefined) && checkAmount !== undefined) {
            this.workingItemChecks.push(new CheckRemission(checkAmount, checkNumber !== undefined ? BigInt(checkNumber) : undefined));
            this.checkFormGroup.controls.checkAmountCtrl.setValue(undefined);
            this.checkFormGroup.controls.checkNumberCtrl.setValue(undefined);
        }
    }

    public deleteCheck(deletedCheck: CheckRemission) {
        this.workingItemChecks = this.workingItemChecks.filter(check => check !== deletedCheck);
    }

    public get workingItemTotalAmount(): Amount {
        return this.workingItemTotalCashAmount.add(this.workingItemTotalCheckAmount);
    }

    private currentAvailableChecksSubscription?: Subscription;
    workingItemOperationChecks: RemissionOperationCheck[] = [];

    public reloadAvailableChecks(date?: Date): Subscription {
        return this.remissionService
            .getOperationChecks(date)
            .subscribe({
                next: (remissionOperationChecks) => {
                    this.workingItemOperationChecks = remissionOperationChecks;
                },
                error: console.error,
            });
    }

    public addOperationCheck(operationCheck: RemissionOperationCheck) {
        this.workingItemOperationChecks = this.workingItemOperationChecks.filter(oc => oc !== operationCheck);
        this.workingItemChecks.push(new CheckRemission(operationCheck.amount, BigInt(operationCheck.checkNumber)));
    }

    public onSubmit() {
        const isNewItem = this.listTable.editingNewItem();
        const remissionDisplay = this.listTable.currentWorkingItem as RemissionDisplay | null;
        if (remissionDisplay) {
            // data validation
            let remission = remissionDisplay.remission;
            // operator
            const operatorId = Number.parseInt(this.remissionFormGroup.controls.operCtrl.value);
            if (!(operatorId in this.operators)) {
                this.resetValidationErrorMessage(`L'opérateur·rice est invalide.`);
                return;
            }
            remission.operatorId = operatorId;
            // date
            if (!this.remissionFormGroup.controls.dateTimeCtrl.value) {
                this.resetValidationErrorMessage(`La date est invalide.`);
                return;
            }
            remission.dateTime = datePickerValueToDate(this.remissionFormGroup.controls.dateTimeCtrl.value);
            // notes
            remission.notes = this.remissionFormGroup.controls.notesCtrl.value || '';
            // cash
            remission.cashDeposits = this.workingItemCashDeposits;

            // checks
            remission.checkRemissions = this.workingItemChecks;

            // update
            this._lastRemissionEntered = remissionDisplay;

            this.remissionService.createUpdate([remission]).subscribe({
                next: () => {
                    this.resetValidationErrorMessage();
                    if (isNewItem) {
                        this.listTable.addItem();
                        const oldOpDisplay = this.createRemissionDisplay(remission!);
                        let opDisplay: RemissionDisplay = this.createNewRemissionDisplay(oldOpDisplay);
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

    cashValues = cashValues;
    getCashValueAmount = getCashValueAmount;

    public reload() {
        this._dateNavigation.emit(this._currentDateRange);
    }

    public delete(remissionDisplay: RemissionDisplay) {
        if (remissionDisplay.remission?.id !== undefined) {
            this.remissionService.delete([remissionDisplay.remission]).subscribe({ next: () => this.reload(), error: console.error });
        }
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

    private calculateTotal<T>(remissions: T[], getAmountFn: (obj: T) => Amount): Amount {
        return remissions
            .map(getAmountFn)
            .reduce((prev, cur) => prev.add(cur), Amount.from(0)!)
            ;
    }

    private recalculateTotals(remissions: Remission[]) {
        this.totalCashAmount = this.calculateTotal(remissions, (remission) => remission.totalCashAmount);
        this.totalCheckAmount = this.calculateTotal(remissions, (remission) => remission.totalCheckAmount);
        this.totalAmount = this.calculateTotal(remissions, (remission) => remission.totalAmount);
    }

    totalCheckAmount: Amount = Amount.from(0)!;

    totalCashAmount: Amount = Amount.from(0)!;

    totalAmount: Amount = Amount.from(0)!;

    public labels: string[] = [];
}

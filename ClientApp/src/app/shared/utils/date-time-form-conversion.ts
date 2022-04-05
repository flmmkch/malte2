import { NgbDate, NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";

export function dateToFormValue(date: Date): string {
    let _date = new Date(date);
    _date.setMinutes(_date.getMinutes() - _date.getTimezoneOffset());
    return _date.toJSON().slice(0, 10);
}

export function formValueToDate(formValue: string): Date {
    return new Date(formValue);
}

export function dateToDatePickerValue(date: Date): NgbDate {
    return new NgbDate(date.getFullYear(), date.getMonth() + 1, date.getDate());
}

export function datePickerValueToDate(datePickerValue: NgbDateStruct): Date {
    return new Date(datePickerValue.year, datePickerValue.month - 1, datePickerValue.day);
}

export function dateRangeFromDatepickerDate(date: NgbDate): [Date, Date] {
    const dateBegin = new Date(date.year, date.month - 1, date.day);
    const dateEnd = new Date(dateBegin);
    dateEnd.setDate(dateEnd.getDate() + 1);
    dateEnd.setSeconds(dateEnd.getSeconds() - 1);
    return [dateBegin, dateEnd];
}

export function dateRangeFromDatepickerMonthYear(datepickerMonthYear: { month: number, year: number }): [Date, Date] {
    const dateBegin = new Date(datepickerMonthYear.year, datepickerMonthYear.month - 1);
    const dateEnd = new Date(dateBegin);
    dateEnd.setMonth(dateEnd.getMonth() + 1);
    dateEnd.setSeconds(dateEnd.getSeconds() - 1);
    return [dateBegin, dateEnd];
}
export function dateToFormValue(date: Date): string {
    let _date = new Date(date);
    _date.setMinutes(_date.getMinutes() - _date.getTimezoneOffset());
    return _date.toJSON().slice(0, 10);
}

export function formValueToDate(formValue: string): Date {
    return new Date(formValue);
}
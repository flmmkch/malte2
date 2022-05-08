import { Component, EventEmitter, Inject, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { NgbDatepickerNavigateEvent, NgbDatepicker, NgbDate } from '@ng-bootstrap/ng-bootstrap';
import { MealDay } from 'src/app/shared/models/meal-day.model';
import { MealDayService } from 'src/app/shared/services/meal-day.service';
import { datePickerValueToDate, dateRangeFromDatepickerDate, dateRangeFromDatepickerMonthYear, dateToDatePickerValue, dateToFormValue } from 'src/app/shared/utils/date-time-form-conversion';

@Component({
  selector: 'app-meals',
  templateUrl: './meals.component.html',
})
export class MealsComponent {
  constructor(private readonly _mealDayService: MealDayService) {
    this._mealDayService.getOnDate(this._dateNavigation)
      .subscribe((mealDays) => {
        const mealDay = mealDays.length > 0 ? mealDays[0] : undefined;
        this.currentMealDay = mealDay;
      });
    this.currentDate = new Date();
  }

  private _currentDate!: Date;

  public get currentDate() {
    return this._currentDate;
  }

  public set currentDate(date: Date) {
    this._currentDate = date;
    this.dateNavigatorFormCtrl.setValue(dateToDatePickerValue(date));
    this._dateNavigation.emit(date);
  }

  private _currentMealDay: MealDay | undefined;

  public get currentMealDay(): MealDay | undefined {
    return this._currentMealDay;
  }

  public set currentMealDay(value: MealDay | undefined) {
    this._currentMealDay = value;
    this.mealFormGroup.controls.nbBoardersControl.setValue(value ? value.boarderCount : 0);
    this.mealFormGroup.controls.nbPatronsControl.setValue(value ? value.patronCount : 0);
    this.mealFormGroup.controls.nbOthersControl.setValue(value ? value.otherCount : 0);
    this.mealFormGroup.controls.nbCaterersControl.setValue(value ? value.catererCount : 0);
  }

  readonly mealFormGroup = new FormGroup({
    nbBoardersControl: new FormControl(),
    nbPatronsControl: new FormControl(),
    nbOthersControl: new FormControl(),
    nbCaterersControl: new FormControl(),
  });

  onSubmit() {
    const mealDay = new MealDay(this.currentMealDay?.id);
    mealDay.dateTime = this.currentDate;
    mealDay.boarderCount = this.mealFormGroup.controls.nbBoardersControl.value ? Number.parseInt(this.mealFormGroup.controls.nbBoardersControl.value) : 0;
    mealDay.patronCount = this.mealFormGroup.controls.nbPatronsControl.value ? Number.parseInt(this.mealFormGroup.controls.nbPatronsControl.value) : 0;
    mealDay.otherCount = this.mealFormGroup.controls.nbOthersControl.value ? Number.parseInt(this.mealFormGroup.controls.nbOthersControl.value) : 0;
    mealDay.catererCount = this.mealFormGroup.controls.nbCaterersControl.value ? Number.parseInt(this.mealFormGroup.controls.nbCaterersControl.value) : 0;
    this._mealDayService.createUpdate([mealDay])
      .subscribe({ next: () => this.currentMealDay = mealDay, error: console.error });
  }

  @ViewChild('dateNavigator') dateNavigator!: NgbDatepicker;

  dateNavigatorFormCtrl = new FormControl();

  private _dateNavigation: EventEmitter<Date> = new EventEmitter();

  public dateSelection(eventDate: NgbDate) {
    this.dateNavigatorFormCtrl.setValue(eventDate);
    const [date] = dateRangeFromDatepickerDate(eventDate);
    this._dateNavigation.emit(date);
    this._currentDate = date;
  }
}

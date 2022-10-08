import { AfterViewInit, Component, EventEmitter, Inject, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { NgbDatepickerNavigateEvent, NgbDatepicker, NgbDate } from '@ng-bootstrap/ng-bootstrap';
import { combineLatestWith, map, mapTo, mergeMap, pipe } from 'rxjs';
import { MealDay } from 'src/app/shared/models/meal-day.model';
import { MealDayService } from 'src/app/shared/services/meal-day.service';
import { datePickerValueToDate, dateRangeFromDatepickerDate, dateRangeFromDatepickerMonthYear, dateToDatePickerValue, dateToFormValue } from 'src/app/shared/utils/date-time-form-conversion';


interface NavigationMonthMealsResponse
{
  currentDay: Date,
  monthMeals: MealDay[],
}

@Component({
  selector: 'app-meals',
  styleUrls: ['./meals.component.css'],
  templateUrl: './meals.component.html',
})
export class MealsComponent implements AfterViewInit {
  constructor(private readonly _mealDayService: MealDayService) {
    this._dateNavigation.pipe(mergeMap(dateNavigated => this._mealDayService.get(dateNavigated.month).pipe(map(monthMeals => <NavigationMonthMealsResponse> { currentDay: dateNavigated.current, monthMeals }))))
      .subscribe(({ currentDay, monthMeals }) => {
        this.monthMeals = monthMeals;
        this.currentMealDay = this.dayMonthMeal(currentDay)!;
      });
    this.currentDate = new Date();
  }

  ngAfterViewInit(): void {
    this.currentDate = this.currentDate;
  }

  public dayMonthMeal(day: Date): MealDay | undefined {
    const monthDayMeal = this.monthMeals.filter(meal => meal.dateTime.getDate() === day.getDate());
    if (monthDayMeal.length > 0) {
      return monthDayMeal[0];
    }
    return undefined;
  }

  public monthMeals: MealDay[] = [];

  private _currentDate!: Date;

  public get currentDate() {
    return this._currentDate;
  }

  public set currentDate(current: Date) {
    this._currentDate = current;
    if (this.dateNavigator) {
      const ngbDate = dateToDatePickerValue(current);
      this.dateNavigator.navigateTo(ngbDate);
      this.dateNavigator.focusDate(ngbDate);
      this.dateNavigator.focusSelect();
    }
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

  currentValues(): MealDay {
    const mealDay = new MealDay(this.currentMealDay?.id);
    mealDay.dateTime = this.currentDate;
    mealDay.boarderCount = this.mealFormGroup.controls.nbBoardersControl.value ? Number.parseInt(this.mealFormGroup.controls.nbBoardersControl.value) : 0;
    mealDay.patronCount = this.mealFormGroup.controls.nbPatronsControl.value ? Number.parseInt(this.mealFormGroup.controls.nbPatronsControl.value) : 0;
    mealDay.otherCount = this.mealFormGroup.controls.nbOthersControl.value ? Number.parseInt(this.mealFormGroup.controls.nbOthersControl.value) : 0;
    mealDay.catererCount = this.mealFormGroup.controls.nbCaterersControl.value ? Number.parseInt(this.mealFormGroup.controls.nbCaterersControl.value) : 0;
    return mealDay;
  }

  onSubmit() {
    const mealDay = this.currentValues();
    this._mealDayService.createUpdate([mealDay])
      .subscribe({ next: () => { this.currentMealDay = mealDay; if (this.dayIsEmpty(dateToDatePickerValue(this.currentDate))) {
        this.monthMeals.push(mealDay);
      } }, error: console.error });
  }

  @ViewChild('dateNavigator') dateNavigator!: NgbDatepicker;

  dateNavigatorFormCtrl = new FormControl();

  private _dateNavigation: EventEmitter<{ current: Date, month: [Date, Date] }> = new EventEmitter();

  public dateSelection(eventDate: NgbDate) {
    this.dateNavigatorFormCtrl.setValue(eventDate, { emitEvent: false });
    const [current] = dateRangeFromDatepickerDate(eventDate);
    const month = dateRangeFromDatepickerMonthYear(eventDate);
    this._dateNavigation.emit({ current, month });
    this._currentDate = current;
  }

  public dateNavigation(event: NgbDatepickerNavigateEvent) {
      const month = dateRangeFromDatepickerMonthYear(event.next);
      this._dateNavigation.emit({ current: this._currentDate, month });
  }

  public dayInCurrentMonth(date: NgbDate): boolean {
    if (this.dateNavigator) {
      return (date.month === this.dateNavigator.model.firstDate?.month && date.year === this.dateNavigator.model.firstDate?.year);
    }
    return false;
  }

  public dayIsEmpty(date: NgbDate): boolean {
    const [jsDate] = dateRangeFromDatepickerDate(date);
    return this.dayMonthMeal(jsDate) === undefined;
  }

  public getMealDayDescription(date: NgbDate): string | null {
    const [jsDate] = dateRangeFromDatepickerDate(date);
    const mealDay = this.dayMonthMeal(jsDate);
    if (mealDay) {
      return `Clients : ${mealDay.patronCount}\nPensionnaires : ${mealDay.boarderCount}\nTraiteurs : ${mealDay.catererCount}\nAutres : ${mealDay.otherCount}`;
    }
    return null;
  }

  public canSave() {
    if (this.currentMealDay === undefined) {
      return true;
    }
    let mealCounts: ((mealDay: MealDay) => number)[] = [
      mealDay => mealDay.patronCount,
      mealDay => mealDay.boarderCount,
      mealDay => mealDay.catererCount,
      mealDay => mealDay.otherCount,
    ];
    const mealDay = this.currentValues();
    const sameMealCounts = mealCounts.map(mealCountFn => mealCountFn(mealDay) === mealCountFn(this.currentMealDay!)).reduceRight((prev, current) => prev && current);
    return !sameMealCounts;
  }
}

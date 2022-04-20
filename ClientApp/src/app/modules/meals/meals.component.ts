import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { dateToFormValue } from 'src/app/shared/utils/date-time-form-conversion';

@Component({
  selector: 'app-meals',
  templateUrl: './meals.component.html',
})
export class MealsComponent {
  constructor(private readonly _http: HttpClient, @Inject('API_BASE_URL') private readonly baseUrl: string) {
    this.currentDate = new Date();
  }

  public currentDate: Date;

  public get currentDateInputValue(): string {
    return dateToFormValue(this.currentDate);
  }

  public set currentDateInputValue(dateInputValue: string) {
    let local = new Date(dateInputValue);
    this.currentDate = local;
  }

  readonly mealFormGroup = new FormGroup({
    nbBoardersControl: new FormControl(),
    nbPatronsControl: new FormControl(),
    nbOthersControl: new FormControl(),
    nbCaterersControl: new FormControl(),
  });

  onSubmit() {
    // TODO
  }
}

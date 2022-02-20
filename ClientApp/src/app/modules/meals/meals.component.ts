import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-meals',
  templateUrl: './meals.component.html',
})
export class MealsComponent {
  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') private readonly baseUrl: string) {
    this.currentDate = new Date();
  }

  public currentDate: Date;

  public get currentDateInputValue(): string {
    let local = new Date(this.currentDate);
    local.setMinutes(local.getMinutes() - local.getTimezoneOffset());
    return local.toJSON().slice(0,10);
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

import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { exhaustAll, map } from 'rxjs/operators';
import { MealDay } from '../models/meal-day.model';
import { dateToSerializationString } from '../utils/date-time-form-conversion';

@Injectable({
  providedIn: 'root'
})
export class MealDayService {

  constructor(private readonly _http: HttpClient, @Inject('API_BASE_URL') readonly baseUrl: string) {
  }

  get(dateRange?: [Date?, Date?]): Observable<MealDay[]> {
    let args: string = '?';
    if (dateRange && dateRange[0]) {
      args = args + `&dateStart=${dateToSerializationString(dateRange[0])}`;
    }
    if (dateRange && dateRange[1]) {
      args = args + `&dateEnd=${dateToSerializationString(dateRange[1])}`;
    }
    return this._http
      .get<MealDayJson[]>(this.baseUrl + `api/mealDay/get${args}`)
      .pipe(map(mealDaysJson => mealDaysJson.map(fromJson)));
  }

  getOnDate(dateRangeObservable: Observable<Date>): Observable<MealDay[]> {
    return dateRangeObservable.pipe(map((date) => this.get([date, date])), exhaustAll());
  }

  createUpdate(mealDays: [MealDay]): Observable<Object> {
    let mealDaysJson = mealDays.map(toJson);
    return this._http.post(this.baseUrl + 'api/mealDay/createUpdate', mealDaysJson);
  }

  delete(mealDays: [MealDay]): Observable<Object> {
    let mealDaysJson = mealDays.map(toJson);
    return this._http.delete(this.baseUrl + 'api/mealDay/delete', { body: mealDaysJson });
  }
}

export interface MealDayJson {
  id?: number,
  dt: string,
  nb: number,
  np: number,
  no: number,
  nc: number,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: MealDayJson): MealDay {
    const mealDay = new MealDay(json.id);
    mealDay.dateTime = new Date(json.dt);
    mealDay.boarderCount = json.nb;
    mealDay.patronCount = json.np;
    mealDay.otherCount = json.no;
    mealDay.catererCount = json.nc;
    return mealDay;
}

/** Conversion depuis l'objet JSON */
export function toJson(mealDay: MealDay): MealDayJson {
  return {
    id: mealDay.id,
    dt: dateToSerializationString(mealDay.dateTime),
    nb: mealDay.boarderCount,
    np: mealDay.patronCount,
    no: mealDay.otherCount,
    nc: mealDay.catererCount,
  };
}

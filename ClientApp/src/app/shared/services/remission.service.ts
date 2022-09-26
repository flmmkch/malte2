import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { exhaustAll, map } from 'rxjs/operators';
import { Amount } from '../models/amount.model';
import { CashDeposit, CashValue, CheckRemission, Remission, RemissionOperationCheck } from '../models/remission.model';
import { dateToSerializationString } from '../utils/date-time-form-conversion';

@Injectable({
  providedIn: 'root'
})
export class RemissionService {

  constructor(private readonly _http: HttpClient, @Inject('API_BASE_URL') readonly baseUrl: string) {
  }

  get(dateRange?: [Date?, Date?]): Observable<Remission[]> {
    let args: string = '?';
    if (dateRange && dateRange[0]) {
      args = args + `&dateStart=${dateToSerializationString(dateRange[0])}`;
    }
    if (dateRange && dateRange[1]) {
      args = args + `&dateEnd=${dateToSerializationString(dateRange[1])}`;
    }
    return this._http
      .get<RemissionJson[]>(this.baseUrl + `api/remission/get${args}`)
      .pipe(map(remissionsJson => remissionsJson.map(fromJson)));
  }

  getOnDateRange(dateRangeObservable: Observable<[Date, Date]>): Observable<Remission[]> {
    return dateRangeObservable.pipe(map((dateRange) => this.get(dateRange)), exhaustAll());
  }

  createUpdate(remissions: [Remission]): Observable<Object> {
    let remissionsJson = remissions.map(toJson);
    return this._http.post(this.baseUrl + 'api/remission/createUpdate', remissionsJson);
  }

  delete(remissions: [Remission]): Observable<Object> {
    let remissionsJson = remissions.map(toJson);
    return this._http.delete(this.baseUrl + 'api/remission/delete', { body: remissionsJson });
  }

  getOperationChecks(upToDate?: Date, remissionId?: number): Observable<RemissionOperationCheck[]> {
    let args: string = '?';
    if (upToDate !== undefined) {
      args = args + `&upToDate=${dateToSerializationString(upToDate)}`;
    }
    if (remissionId !== undefined) {
      args = args + `&remissionId=${remissionId}`;
    }
    return this._http
      .get<RemissionOperationCheckJson[]>(this.baseUrl + `api/remission/getRemissionChecks${args}`)
      .pipe(map(remissionsJson => remissionsJson.map(operationCheckFromJson)));
  }
}

export interface CashDepositJson {
  v: number,
  n: number,
}

export interface CheckRemissionJson {
  n?: number,
  a: string,
}

export interface RemissionJson {
  id?: number,
  o: number,
  dt: string,
  n: string,
  h: CashDepositJson[],
  k: CheckRemissionJson[],
}

/** Conversion depuis l'objet JSON */
function fromJson(json: RemissionJson): Remission {
    const remission = new Remission(json.id, json.o);
    remission.dateTime = new Date(json.dt);
    remission.notes = json.n;
    remission.cashDeposits = json.h.map(h => new CashDeposit(<CashValue> h.v, BigInt(h.n)));
    remission.checkRemissions = json.k.map(k => new CheckRemission(Amount.from(k.a)!, k.n != null ? BigInt(k.n) : undefined));
    return remission;
}

/** Conversion depuis l'objet JSON */
function toJson(remission: Remission): RemissionJson {
  return {
    id: remission.id,
    o: remission.operatorId,
    dt: dateToSerializationString(remission.dateTime),
    n: remission.notes,
    h: remission.cashDeposits.map(cashDeposit => <CashDepositJson> { n: Number(cashDeposit.count), v: cashDeposit.value }),
    k: remission.checkRemissions.map(checkRemission => <CheckRemissionJson> { a: checkRemission.amount.toString(), n: checkRemission.checkNumber ? Number(checkRemission.checkNumber) : undefined }),
  };
}

interface RemissionOperationCheckJson {
  n: number,
  dt: string,
  l: string,
  d: string,
  a: string,
  r?: number,
}

function operationCheckFromJson(json: RemissionOperationCheckJson): RemissionOperationCheck {
  return new RemissionOperationCheck(
    json.n,
    new Date(json.dt),
    json.l,
    json.d,
    Amount.from(json.a)!,
    json.r,
  );
}

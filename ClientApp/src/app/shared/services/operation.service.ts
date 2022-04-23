import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { exhaustAll, map } from 'rxjs/operators';
import { Amount } from '../models/amount.model';
import { Operation } from '../models/operation.model';
import { PaymentMethod } from '../models/payment-method.model';
import { dateToSerializationString } from '../utils/date-time-form-conversion';

@Injectable({
  providedIn: 'root'
})
export class OperationService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  get(dateRange?: [Date?, Date?]): Observable<Operation[]> {
    let args: string = '?';
    if (dateRange && dateRange[0]) {
      args = args + `&dateStart=${dateToSerializationString(dateRange[0])}`;
    }
    if (dateRange && dateRange[1]) {
      args = args + `&dateEnd=${dateToSerializationString(dateRange[1])}`;
    }
    return this._http
      .get<OperationJson[]>(this.baseUrl + `api/operation/get${args}`)
      .pipe(map(operationsJson => operationsJson.map(fromJson)));
  }

  getOnDateRange(dateRangeObservable: Observable<[Date, Date]>): Observable<Operation[]> {
    return dateRangeObservable.pipe(map((dateRange) => this.get(dateRange)), exhaustAll());
  }

  createUpdate(operations: [Operation]): Observable<Object> {
    let operationsJson = operations.map(toJson);
    return this._http.post(this.baseUrl + 'api/operation/createUpdate', operationsJson);
  }

  delete(operations: [Operation]): Observable<Object> {
    let operationsJson = operations.map(toJson);
    return this._http.delete(this.baseUrl + 'api/operation/delete', { body: operationsJson });
  }

  editionPdfDownloadUrl(dateRange: [Date, Date]): string {
    return this.baseUrl + `api/operation/generateEditionPdf?dateStart=${dateToSerializationString(dateRange[0])}&dateEnd=${dateToSerializationString(dateRange[1])}`;
  }

  getLabels(): Observable<string[]> {
    return this._http
      .get<string[]>(this.baseUrl + `api/operation/getLabels`);
  }
}

export interface OperationJson {
  id?: number,
  a: string,
  op: number,
  dt: string,
  ae: number,
  ac?: number,
  b: number,
  pm: number,
  pkn?: string,
  ctn?: string,
  ptn?: string,
  l: string,
  bd?: number,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: OperationJson): Operation {
    const amount = Amount.from(json.a);
    if (!amount)
        throw new Error(`Unable to parse amount from ${json.a}`);
    const operation = new Operation(json.id, amount, json.ae, <PaymentMethod> json.pm, json.b, json.op);
    operation.dateTime = new Date(json.dt);
    operation.categoryId = json.ac;
    operation.label = json.l;
    operation.boarderId = json.bd;
    operation.checkNumber = json.pkn ? BigInt(json.pkn) : undefined;
    operation.cardTicketNumber = json.ctn ? BigInt(json.ctn) : undefined;
    operation.transferNumber = json.ptn ? BigInt(json.ptn) : undefined;
    return operation;
}

/** Conversion depuis l'objet JSON */
export function toJson(operation: Operation): OperationJson {
  return {
    id: operation.id,
    a: operation.amount.toString(),
    op: operation.operatorId,
    dt: dateToSerializationString(operation.dateTime),
    ae: operation.accountingEntryId,
    ac: operation.categoryId,
    b: operation.accountBookId,
    pm: operation.paymentMethod,
    pkn: operation.checkNumber?.toString(),
    ctn: operation.cardTicketNumber?.toString(),
    ptn: operation.transferNumber?.toString(),
    bd: operation.boarderId,
    l: operation.label,
  };
}

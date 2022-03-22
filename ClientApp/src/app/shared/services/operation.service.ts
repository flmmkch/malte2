import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Amount } from '../models/amount.model';
import { Operation } from '../models/operation.model';
import { PaymentMethod } from '../models/payment-method.model';

@Injectable({
  providedIn: 'root'
})
export class OperationService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  get(): Observable<Operation[]> {
    let args: string = '?';
    return this._http
      .get<OperationJson[]>(this.baseUrl + `api/operation/get${args}`)
      .pipe(map(operationsJson => operationsJson.map(fromJson)));
  }

  createUpdate(operations: [Operation]): Observable<Object> {
    let operationsJson = operations.map(toJson);
    return this._http.post(this.baseUrl + 'api/operation/createUpdate', operationsJson);
  }

  delete(operations: [Operation]): Observable<Object> {
    let operationsJson = operations.map(toJson);
    return this._http.delete(this.baseUrl + 'api/operation/delete', { body: operationsJson });
  }
}

export interface OperationJson {
  id?: number,
  a: string,
  op: number,
  dt: string,
  ae: number,
  b: number,
  pm: number,
  pi: string,
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
    operation.label = json.l;
    operation.boarderId = json.bd;
    operation.paymentMethodInfo = json.pi;
    return operation;
}

/** Conversion depuis l'objet JSON */
export function toJson(operation: Operation): OperationJson {
  return {
    id: operation.id,
    a: operation.amount.toString(),
    op: operation.operatorId,
    dt: operation.dateTime.toISOString(),
    ae: operation.accountingEntryId,
    b: operation.accountBookId,
    pm: operation.paymentMethod,
    pi: operation.paymentMethodInfo,
    bd: operation.boarderId,
    l: operation.label,
  };
}

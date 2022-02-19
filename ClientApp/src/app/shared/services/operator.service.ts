import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Operator } from '../models/operator.model';

@Injectable({
  providedIn: 'root'
})
export class OperatorService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  getOperators(onlyEnabled: boolean = false): Observable<Operator[]> {
    return this._http
      .get<OperatorJson[]>(this.baseUrl + 'api/operator/get')
      .pipe(map(operatorsJson => operatorsJson.map(operatorFromJson)));
  }

  createUpdateOperators(operators: [Operator]): Observable<Object> {
    let operatorsJson = operators.map(operatorToJson);
    return this._http.post(this.baseUrl + 'api/operator/createUpdate', operatorsJson);
  }

  deleteOperators(operators: [Operator]): Observable<Object> {
    let operatorsJson = operators.map(operatorToJson);
    return this._http.delete(this.baseUrl + 'api/operator/delete', { body: operatorsJson });
  }
}

export interface OperatorJson {
  id?: number,
  n: string,
  p?: string,
  e?: boolean,
}

/** Conversion depuis l'objet JSON */
export function operatorFromJson(json: OperatorJson): Operator {
  const operator = new Operator(json.n, json.id);
  operator.phone = json.p || '';
  if (json.e !== undefined) {
    operator.enabled = json.e;
  }
  return operator;
}

/** Conversion depuis l'objet JSON */
export function operatorToJson(operator: Operator): OperatorJson {
  return {
    id: operator.id,
    n: operator.name,
    p: operator.phone,
    e: operator.enabled,
  };
}

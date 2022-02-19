import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Operator, OperatorJson } from '../models/operator.model';

@Injectable({
  providedIn: 'root'
})
export class OperatorService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  getOperators(onlyEnabled: boolean = false): Observable<Operator[]> {
    return this._http
      .get<OperatorJson[]>(this.baseUrl + 'api/operator/get')
      .pipe(map(operatorsJson => operatorsJson.map(Operator.fromJson)));
  }

  createUpdateOperators(operators: [Operator]): Observable<Object> {
    let operatorsJson = operators.map(o => o.toJson());
    return this._http.post(this.baseUrl + 'api/operator/createUpdate', operatorsJson);
  }

  deleteOperators(operators: [Operator]): Observable<Object> {
    let operatorsJson = operators.map(o => o.toJson());
    return this._http.delete(this.baseUrl + 'api/operator/delete', { body: operatorsJson });
  }
}

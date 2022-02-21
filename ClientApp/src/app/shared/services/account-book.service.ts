import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountBook } from '../models/account-book.model';

@Injectable({
  providedIn: 'root'
})
export class AccountBookService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  get(): Observable<AccountBook[]> {
    return this._http
      .get<AccountBookJson[]>(this.baseUrl + 'api/accountBook/get')
      .pipe(map(accountingEntriesJson => accountingEntriesJson.map(fromJson)));
  }

  createUpdate(accountingEntries: [AccountBook]): Observable<Object> {
    let accountingEntriesJson = accountingEntries.map(toJson);
    return this._http.post(this.baseUrl + 'api/accountBook/createUpdate', accountingEntriesJson);
  }

  delete(accountingEntries: [AccountBook]): Observable<Object> {
    let accountingEntriesJson = accountingEntries.map(toJson);
    return this._http.delete(this.baseUrl + 'api/accountBook/delete', { body: accountingEntriesJson });
  }
}

export interface AccountBookJson {
  id?: number,
  l: string,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: AccountBookJson): AccountBook {
  const accountBook = new AccountBook(json.id);
  accountBook.label = json.l;
  return accountBook;
}

/** Conversion depuis l'objet JSON */
export function toJson(accountBook: AccountBook): AccountBookJson {
  return {
    id: accountBook.id,
    l: accountBook.label,
  };
}

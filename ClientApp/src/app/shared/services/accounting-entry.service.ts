import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountingEntry, EntryType } from '../models/accounting-entry.model';

@Injectable({
  providedIn: 'root'
})
export class AccountingEntryService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  get(onlyEnabled: boolean = false): Observable<AccountingEntry[]> {
    return this._http
      .get<AccountingEntryJson[]>(this.baseUrl + 'api/accountingEntry/get')
      .pipe(map(accountingEntriesJson => accountingEntriesJson.map(fromJson)));
  }

  createUpdate(accountingEntries: [AccountingEntry]): Observable<Object> {
    let accountingEntriesJson = accountingEntries.map(toJson);
    return this._http.post(this.baseUrl + 'api/accountingEntry/createUpdate', accountingEntriesJson);
  }

  delete(accountingEntries: [AccountingEntry]): Observable<Object> {
    let accountingEntriesJson = accountingEntries.map(toJson);
    return this._http.delete(this.baseUrl + 'api/accountingEntry/delete', { body: accountingEntriesJson });
  }
}

export interface AccountingEntryJson {
  id?: number,
  l: string,
  b?: boolean,
  t: number,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: AccountingEntryJson): AccountingEntry {
  const accountingEntry = new AccountingEntry(json.id);
  accountingEntry.label = json.l;
  accountingEntry.dependsOnBoarder = json.b || false;
  accountingEntry.entryType = <EntryType> json.t;
  return accountingEntry;
}

/** Conversion depuis l'objet JSON */
export function toJson(accountingEntry: AccountingEntry): AccountingEntryJson {
  return {
    id: accountingEntry.id,
    l: accountingEntry.label,
    b: accountingEntry.dependsOnBoarder,
    t: accountingEntry.entryType,
  };
}

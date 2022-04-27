import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountingCategory } from '../models/accounting-category.model';

@Injectable({
  providedIn: 'root'
})
export class AccountingCategoryService {

  constructor(private readonly _http: HttpClient, @Inject('API_BASE_URL') readonly baseUrl: string) {
  }

  get(): Observable<AccountingCategory[]> {
    return this._http
      .get<AccountingCategoryJson[]>(this.baseUrl + 'api/accountingCategory/get')
      .pipe(map(accountingCategoriesJson => accountingCategoriesJson.map(fromJson)));
  }

  createUpdate(accountingCategories: [AccountingCategory]): Observable<Object> {
    let accountingCategoriesJson = accountingCategories.map(toJson);
    return this._http.post(this.baseUrl + 'api/accountingCategory/createUpdate', accountingCategoriesJson);
  }

  delete(accountingCategories: [AccountingCategory]): Observable<Object> {
    let accountingCategoriesJson = accountingCategories.map(toJson);
    return this._http.delete(this.baseUrl + 'api/accountingCategory/delete', { body: accountingCategoriesJson });
  }
}

export interface AccountingCategoryJson {
  id?: number,
  l: string,
  ae?: number,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: AccountingCategoryJson): AccountingCategory {
  const accountingCategory = new AccountingCategory(json.id);
  accountingCategory.label = json.l;
  accountingCategory.accountingEntryId = json.ae;
  return accountingCategory;
}

/** Conversion depuis l'objet JSON */
export function toJson(accountingCategory: AccountingCategory): AccountingCategoryJson {
  return {
    id: accountingCategory.id,
    l: accountingCategory.label,
    ae: accountingCategory.accountingEntryId,
  };
}

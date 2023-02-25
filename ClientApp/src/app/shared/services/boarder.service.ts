import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Amount } from '../models/amount.model';
import { Boarder, BoarderListItem } from '../models/boarder.model';
import { dateToSerializationString } from '../utils/date-time-form-conversion';

@Injectable({
  providedIn: 'root'
})
export class BoarderService {

  constructor(private readonly _http: HttpClient, @Inject('API_BASE_URL') readonly baseUrl: string) {
  }

  details(id: number): Observable<Boarder> {
    return this._http
      .get<BoarderJson>(this.baseUrl + `api/boarder/details/${id}`)
      .pipe(map(fromJson));
  }

  list(parameters?: { occupancyDate?: Date, balances?: boolean }): Observable<BoarderListItem[]> {
    const urlSearchParams = new URLSearchParams();
    if (parameters?.occupancyDate) {
      urlSearchParams.append('occupancyDate', dateToSerializationString(parameters.occupancyDate));
    }
    if (parameters?.balances === true) {
      urlSearchParams.append('balances', true.toString());
    }
    return this._http
      .get<BoarderListItemJson[]>(`${this.baseUrl}api/boarder/list?${urlSearchParams.toString()}`)
      .pipe(map(boardersJson => boardersJson.map(listItemFromJson)));
  }

  createUpdate(boarders: [Boarder]): Observable<Object> {
    let boardersJson = boarders.map(toJson);
    return this._http.post(this.baseUrl + 'api/boarder/createUpdate', boardersJson);
  }

  delete(boarders: [Boarder]): Observable<Object> {
    let boardersJson = boarders.map(toJson);
    return this._http.delete(this.baseUrl + 'api/boarder/delete', { body: boardersJson });
  }
}

export interface BoarderJson {
  id?: number,
  n: string,
  na: string,
  bd?: string,
  bp?: string,
  p: string,
  m: string,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: BoarderJson): Boarder {
  const boarder = new Boarder(json.id);
  boarder.name = json.n;
  boarder.nationality = json.na;
  boarder.birthDate = json.bd ? new Date(json.bd) : undefined;
  boarder.birthPlace = json.bp;
  boarder.phoneNumber = json.p;
  boarder.notes = json.m;
  return boarder;
}

/** Conversion depuis l'objet JSON */
export function toJson(boarder: Boarder): BoarderJson {
  return {
    id: boarder.id,
    n: boarder.name,
    na: boarder.nationality,
    bd: boarder.birthDate? dateToSerializationString(boarder.birthDate) : undefined,
    bp: boarder.birthPlace,
    p: boarder.phoneNumber,
    m: boarder.notes,
  };
}

export interface BoarderListItemJson {
  b: number,
  n: string,
  r: string,
  a?: string,
}

export function listItemFromJson(json: BoarderListItemJson): BoarderListItem {
  const listItem = new BoarderListItem(json.b, json.n, json.r);
  if (json.a) {
    listItem.balance = Amount.from(json.a);
  }
  return listItem;
}

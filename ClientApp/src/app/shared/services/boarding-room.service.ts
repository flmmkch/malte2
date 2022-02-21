import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BoardingRoom } from '../models/boarding-room.model';

@Injectable({
  providedIn: 'root'
})
export class BoardingRoomService {

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
  }

  get(): Observable<BoardingRoom[]> {
    return this._http
      .get<BoardingRoomJson[]>(this.baseUrl + 'api/boardingRoom/get')
      .pipe(map(accountingEntriesJson => accountingEntriesJson.map(fromJson)));
  }

  createUpdate(accountingEntries: [BoardingRoom]): Observable<Object> {
    let accountingEntriesJson = accountingEntries.map(toJson);
    return this._http.post(this.baseUrl + 'api/boardingRoom/createUpdate', accountingEntriesJson);
  }

  delete(accountingEntries: [BoardingRoom]): Observable<Object> {
    let accountingEntriesJson = accountingEntries.map(toJson);
    return this._http.delete(this.baseUrl + 'api/boardingRoom/delete', { body: accountingEntriesJson });
  }
}

export interface BoardingRoomJson {
  id?: number,
  l: string,
}

/** Conversion depuis l'objet JSON */
export function fromJson(json: BoardingRoomJson): BoardingRoom {
  const boardingRoom = new BoardingRoom(json.id);
  boardingRoom.label = json.l;
  return boardingRoom;
}

/** Conversion depuis l'objet JSON */
export function toJson(boardingRoom: BoardingRoom): BoardingRoomJson {
  return {
    id: boardingRoom.id,
    l: boardingRoom.label,
  };
}

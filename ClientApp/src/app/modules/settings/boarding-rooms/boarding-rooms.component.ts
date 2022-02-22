import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ListTable, SetCurrentWorkingItemEventArgs } from 'src/app/modules/list-table/list-table.component';
import { BoardingRoom } from 'src/app/shared/models/boarding-room.model';
import { BoardingRoomService } from 'src/app/shared/services/boarding-room.service';

@Component({
  selector: 'app-boarding-rooms',
  templateUrl: './boarding-rooms.component.html'
})
export class BoardingRoomsComponent implements OnInit, AfterViewInit {
  public items?: BoardingRoom[];
  
  readonly formGroup = new FormGroup({
    labelControl: new FormControl(),
  });

  constructor(private readonly _service: BoardingRoomService) { }

  @ViewChild('listTable') listTable!: ListTable;

  
  private _currentLoadingPromise?: Promise<BoardingRoom[]>;

  public get currentLoadingPromise(): Promise<BoardingRoom[]> | undefined {
    return this._currentLoadingPromise;
  }

  load(): Promise<BoardingRoom[]> {
    let observable = this._service.get();
    observable.subscribe(items => {
      this.items = items;
      if (this.items.length == 0) {
        this.listTable.addItem();
      }
    });
    this._currentLoadingPromise = observable.toPromise();
    return this._currentLoadingPromise;
  }


  delete(boardingRoom: BoardingRoom) {
    if (boardingRoom.id) {
      this._service.delete([boardingRoom]).subscribe(() => this.load(), console.error);
    }
  }

  ngOnInit(): void {
      this.load();
  }

  ngAfterViewInit(): void {
    this.listTable.onCreate.subscribe(() => this.listTable.currentWorkingItem = new BoardingRoom());
    this.listTable.onDelete.subscribe((boardingRoom: BoardingRoom) => this.delete(boardingRoom));
    this.listTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<BoardingRoom>) => {
      const boardingRoom = e.value;
      if (boardingRoom) {
        this.formGroup.controls.labelControl.setValue(boardingRoom.label);
      }
    });
    this.listTable.confirmDeleteMessage = (boardingRoom: BoardingRoom) => `Supprimer la chambre ${boardingRoom.label} ?`;
  }

  onSubmit(): void {
    const boardingRoom = this.listTable.currentWorkingItem as BoardingRoom;
    boardingRoom.label = this.formGroup.controls.labelControl.value;
    this._service.createUpdate([boardingRoom]).subscribe(() => {
      this.formGroup.reset();
      this.listTable.cancelEdit();
      this.load();
    }, console.error);
  }

}

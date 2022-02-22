
import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { ListTable } from 'src/app/modules/list-table/list-table.component';
import { BoarderListItem } from 'src/app/shared/models/boarder.model';
import { BoarderService } from 'src/app/shared/services/boarder.service';

@Component({
  selector: 'app-boarders',
  templateUrl: './boarders.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./boarders.component.css']
})
export class BoardersComponent implements OnInit {
  public items?: BoarderListItem[];
  
  constructor(private readonly _service: BoarderService) { }

  @ViewChild('listTable') listTable!: ListTable;

  
  private _currentLoadingPromise?: Promise<BoarderListItem[]>;

  public get currentLoadingPromise(): Promise<BoarderListItem[]> | undefined {
    return this._currentLoadingPromise;
  }

  load(): Promise<BoarderListItem[]> {
    const today = new Date();
    const observable = this._service.list(today);
    observable.subscribe(items => {
      this.items = items;
    });
    this._currentLoadingPromise = observable.toPromise();
    return this._currentLoadingPromise;
  }

  ngOnInit(): void {
      this.load();
  }

}

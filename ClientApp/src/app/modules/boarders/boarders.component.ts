
import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs';
import { ListTable } from 'src/app/modules/list-table/list-table.component';
import { BoarderListItem } from 'src/app/shared/models/boarder.model';
import { BoarderService } from 'src/app/shared/services/boarder.service';

@Component({
  selector: 'app-boarders',
  templateUrl: './boarders.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./boarders.component.css', '../../shared/components/style/amount.css'],
})
export class BoardersComponent implements OnInit {
  public items?: BoarderListItem[];
  
  constructor(private readonly _service: BoarderService) { }

  @ViewChild('listTable') listTable!: ListTable;

  load(): Observable<BoarderListItem[]> {
    const today = new Date();
    const observable = this._service.list({ occupancyDate: today, balances: true });
    observable.subscribe(items => {
      this.items = items;
    });
    return observable;
  }

  ngOnInit(): void {
      this.load();
  }

}

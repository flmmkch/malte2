import { Component } from '@angular/core';

@Component({
  selector: 'app-operations',
  templateUrl: './operations.component.html',
})
export class OperationsComponent {
  public items: OperationJson[] = []

  modifyItem(item: OperationJson) {}

  deleteItem(item: OperationJson) {}
}

export interface OperationJson {
  id?: number;
  label: string;
  accountingEntryId: number;
  date: Date;
}

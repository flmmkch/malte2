import { Component } from '@angular/core';

@Component({
  selector: 'app-accounting-entries',
  templateUrl: './accounting-entries.component.html',
})
export class AccountingEntriesComponent {
  public readonly items: AccountingEntryJson[] = [
    { id: 1, label: 'Pension (recette)', dependsOnBoarder: true },
    { id: 2, label: 'Pensionnaire (dépense)', dependsOnBoarder: true },
    { id: 3, label: 'Restaurant (recette)' },
    { id: 4, label: 'Boulangerie' },
    { id: 5, label: 'Epicerie' },
    { id: 6, label: 'Boucherie' },
    { id: 7, label: 'Divers' },
  ]

  modifyItem(item: AccountingEntryJson) {}

  deleteItem(item: AccountingEntryJson) {}
}

export interface AccountingEntryJson {
  id?: number;
  label: string;
  dependsOnBoarder?: boolean;
}

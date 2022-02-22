import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Operator } from 'src/app/shared/models/operator.model';
import { ListTable, SetCurrentWorkingItemEventArgs } from 'src/app/modules/list-table/list-table.component';
import { OperatorService } from 'src/app/shared/services/operator.service';

@Component({
  selector: 'app-operators',
  templateUrl: './operators.component.html',
  providers: [OperatorService]
})
export class OperatorsComponent implements OnInit, AfterViewInit {
  public operators?: Operator[];

  readonly operatorFormGroup = new FormGroup({
    nameControl: new FormControl(),
    phoneControl: new FormControl(),
    enabledControl: new FormControl(),
  });

  constructor(private _operatorService: OperatorService) {
    this.load();
  }

  @ViewChild('operatorListTable') operatorListTable!: ListTable;

  ngOnInit(): void {
    this.load();
  }

  onSubmit() {
    const operator = this.operatorListTable.currentWorkingItem as Operator;
    operator.name = this.operatorFormGroup.controls.nameControl.value;
    operator.phone = this.operatorFormGroup.controls.phoneControl.value || '';
    operator.enabled = this.operatorFormGroup.controls.enabledControl.value || false;
    this._operatorService.createUpdate([operator]).subscribe(() => {
      this.operatorFormGroup.reset();
      this.operatorListTable.cancelEdit();
      this.load();
    }, console.error);
  }

  load() {
    this._operatorService.get().subscribe(operators => {
      this.operators = operators;
      if (this.operators.length == 0) {
        this.operatorListTable.addItem();
      }
    }, console.error);
  }

  delete(operator: Operator) {
    if (operator.id) {
      this._operatorService.delete([operator]).subscribe(() => this.load(), console.error);
    }
  }

  ngAfterViewInit(): void {
    this.operatorListTable.onCreate.subscribe(() => this.operatorListTable.currentWorkingItem = new Operator(''));
    this.operatorListTable.onDelete.subscribe((operator: Operator) => this.delete(operator));
    this.operatorListTable.onSetWorkingItem.subscribe((e: SetCurrentWorkingItemEventArgs<Operator>) => {
      const operator = e.value;
      if (operator) {
        this.operatorFormGroup.controls.nameControl.setValue(operator.name);
        this.operatorFormGroup.controls.phoneControl.setValue(operator.phone);
        this.operatorFormGroup.controls.enabledControl.setValue(operator.enabled);
      }
    });
    this.operatorListTable.confirmDeleteMessage = (operator: Operator) => `Supprimer l'opérateur·rice ${operator.name} ?`;
  }

  displayInactiveOperators: boolean = false;

  get filteredOperators(): Operator[] | undefined {
    let list = this.operators;
    if (list && !this.displayInactiveOperators) {
      list = list.filter(o => o.enabled);
    }
    return list;
  }
}

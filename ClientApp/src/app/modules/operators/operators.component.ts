import { AfterViewInit, Component, Inject, ViewChild, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Operator } from 'src/app/shared/models/operator.model';
import { ListTable } from 'src/app/shared/list-table/list-table.component';
import { OperatorService } from 'src/app/shared/services/operator.service';

@Component({
  selector: 'app-operators',
  templateUrl: './operators.component.html',
  providers: [OperatorService]
})
export class OperatorsComponent implements AfterViewInit {
  public operators: Operator[] = [];

  readonly operatorFormGroup = new FormGroup({
    nameControl: new FormControl(),
    phoneControl: new FormControl(),
    enabledControl: new FormControl(),
  });

  constructor(private _operatorService: OperatorService) {
    this.load();
  }

  @ViewChild('operatorListTable') operatorListTable!: ListTable;

  onSubmit() {
    const operator = this.operatorListTable.currentWorkingItem as Operator;
    operator.name = this.operatorFormGroup.controls.nameControl.value;
    operator.phone = this.operatorFormGroup.controls.phoneControl.value || '';
    operator.enabled = this.operatorFormGroup.controls.enabledControl.value || false;
    this._operatorService.createUpdateOperators([operator]).subscribe(() => {
      this.operatorFormGroup.reset();
      this.operatorListTable.cancelEdit();
      this.load();
    }, console.error);
  }

  load() {
    this._operatorService.getOperators().subscribe(operators => {
      this.operators = operators;
      if (this.operators.length == 0) {
        this.operatorListTable.addItem();
      }
    }, console.error);
  }


  deleteOperator(operator: Operator) {
    if (operator.id) {
      this._operatorService.deleteOperators([operator]).subscribe(() => this.load(), console.error);
    }
  }

  ngAfterViewInit(): void {
    this.operatorListTable.onCreate.subscribe(() => this.operatorListTable.currentWorkingItem = new Operator(''));
    this.operatorListTable.onDelete.subscribe((operator) => this.deleteOperator(operator as Operator));
    this.operatorListTable.onSetWorkingItem.subscribe(e => {
      const operator = e.value as Operator | null;
      if (operator) {
        this.operatorFormGroup.controls.nameControl.setValue(operator.name);
        this.operatorFormGroup.controls.phoneControl.setValue(operator.phone);
        this.operatorFormGroup.controls.enabledControl.setValue(operator.enabled);
      }
    });
    this.operatorListTable.confirmDeleteMessage = (operator: Operator) => `Supprimer l'opérateur·rice ${operator.name} ?`;
  }

  displayInactiveOperators: boolean = false;

  get filteredOperators(): Operator[] {
    let list = this.operators;
    if (!this.displayInactiveOperators) {
      list = list.filter(o => o.enabled);
    }
    return list;
  }
}

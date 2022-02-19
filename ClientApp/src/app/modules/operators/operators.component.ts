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
  });

  constructor(private _operatorService: OperatorService) {
    this.load();
  }

  @ViewChild('operatorListTable') operatorListTable!: ListTable;

  onSubmitCreate() {
    const newOperator = new Operator(this.operatorFormGroup.controls.nameControl.value);
    newOperator.phone = this.operatorFormGroup.controls.phoneControl.value || '';
    this._operatorService.createUpdateOperators([newOperator]).subscribe(() => {
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
    this.operatorListTable.onDelete.subscribe((operator) => this.deleteOperator(operator as Operator));
    this.operatorListTable.confirmDeleteMessage = (operator: Operator) => `Supprimer l'opérateur·rice ${operator.name} ?`;
  }
}

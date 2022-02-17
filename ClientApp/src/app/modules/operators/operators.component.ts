import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';
import { Operator, OperatorJson } from 'src/app/shared/models/operator.model';

@Component({
  selector: 'app-operators',
  templateUrl: './operators.component.html',
  styleUrls: ['./operators.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class OperatorsComponent {
  public operators: Operator[] = [];

  readonly operatorFormGroup = new FormGroup({
    nameControl: new FormControl(),
  });

  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') readonly baseUrl: string) {
    this.load();
  }

  onSubmit() {
    let newOperator = <OperatorJson>{
      n: this.operatorFormGroup.controls.nameControl.value,
    };
    this._http.post(this.baseUrl + 'api/operator/createUpdate', [newOperator])
      .subscribe(() => {
        this.operatorFormGroup.reset();
        this.load();
      }, error => console.error(error));
    this.cancelEdit();
  }

  load() {
    this._http.get<OperatorJson[]>(this.baseUrl + 'api/operator/get').subscribe(result => {
      this.operators = result.map(o => Operator.fromJson(o));
    }, error => console.error(error));
  }

  addItem() {
    this.currentEditMode = EditMode.NewItem;
  }

  modifyItem(operator: Operator) {
    // TODO
  }

  deleteItem(operator: Operator) {
    if (operator.id) {
      const operatorJson = operator.toJson();
      this._http.delete(this.baseUrl + 'api/operator/delete', { body: [operatorJson] })
      .subscribe(() => {
        this.load();
      }, error => console.error(error));
    }
  }

  public currentEditMode: EditMode | null = null;

  editingNewItem(): boolean {
    return this.currentEditMode == EditMode.NewItem;
  }

  editing(): boolean {
    return this.currentEditMode != null;
  }

  cancelEdit() {
    this.currentEditMode = null;
  }
}

export enum EditMode {
  NewItem,
  ModifyItem,
}

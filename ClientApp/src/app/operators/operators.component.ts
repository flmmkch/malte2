import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-operators',
  templateUrl: './operators.component.html'
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
    this._http.post<boolean>(this.baseUrl + 'api/operator/createUpdate', [newOperator])
      .subscribe(() => {
        this.operatorFormGroup.reset();
        this.load();
      }, error => console.error(error));
  }

  load() {
    this._http.get<OperatorJson[]>(this.baseUrl + 'api/operator/get').subscribe(result => {
      this.operators = result.map(o => <Operator>{ id: o.id, name: o.n });
    }, error => console.error(error));
  }

  generateEditionDownload() {
    this._http.get(this.baseUrl + 'api/operator/generateEdition', { responseType: "blob", headers: { 'Accept': 'application/pdf' } })
      .subscribe(blob => {
        saveAs(blob, 'édition.pdf');
      });
  }
}

interface Operator {
  id?: number;
  name: string;
}

interface OperatorJson {
  id?: number;
  n: string;
}

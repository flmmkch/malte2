import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-boarders',
  templateUrl: './boarders.component.html',
})
export class BoardersComponent {
  constructor(private readonly _http: HttpClient, @Inject('BASE_URL') private readonly baseUrl: string) {
  }
  

  readonly boarderFormGroup = new FormGroup({
    nameControl: new FormControl(),
  });

  onSubmit() {
    // TODO
  }
}

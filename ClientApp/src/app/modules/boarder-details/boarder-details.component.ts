import { HttpErrorResponse } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Boarder } from 'src/app/shared/models/boarder.model';
import { BoarderService } from 'src/app/shared/services/boarder.service';
import { dateToFormValue, formValueToDate } from 'src/app/shared/utils/date-time-form-conversion';

@Component({
  selector: 'app-boarder-details',
  templateUrl: './boarder-details.component.html',
  styleUrls: ['./boarder-details.component.css']
})
export class BoarderDetailsComponent implements OnInit {

  constructor(private readonly _service: BoarderService, private _route: ActivatedRoute, private readonly _router: Router) { }

  @Input() edition: boolean = false;

  public boarder?: Boarder;

  private _formValidationErrorMessage?: string;

  public get formValidationErrorMessage(): string | undefined {
    return this._formValidationErrorMessage;
  }

  boarderFormGroup: FormGroup = new FormGroup({
    nameControl: new FormControl(),
    phoneNumberControl: new FormControl(),
    birthDateControl: new FormControl(),
    birthPlaceControl: new FormControl(),
    nationalityControl: new FormControl(),
    notesControl: new FormControl(),
  });

  ngOnInit(): void {
    this.resetValidationErrorMessage();
    this._route.data.subscribe(data => {
      const boarder: Boarder | undefined = data['boarder'];
      if (boarder) {
        this.boarderFormGroup.controls.nameControl.setValue(boarder.name);
        this.boarderFormGroup.controls.nationalityControl.setValue(boarder.nationality);
        this.boarderFormGroup.controls.phoneNumberControl.setValue(boarder.phoneNumber);
        this.boarderFormGroup.controls.birthDateControl.setValue(boarder.birthDate ? dateToFormValue(boarder.birthDate) : undefined);
        this.boarderFormGroup.controls.birthPlaceControl.setValue(boarder.birthPlace);
        this.boarderFormGroup.controls.notesControl.setValue(boarder.notes);
        this.boarder = boarder;
      }
      else {
        // création : toujours en mode édition
        this.edition = true;
      }
    }, e => this.resetValidationErrorMessage(e));
  }

  onSubmit() {
    let boarder = this.boarder || new Boarder();
    boarder.name = this.boarderFormGroup.controls.nameControl.value;
    boarder.nationality = this.boarderFormGroup.controls.nationalityControl.value || '';
    boarder.phoneNumber = this.boarderFormGroup.controls.phoneNumberControl.value || '';
    boarder.birthDate = this.boarderFormGroup.controls.birthDateControl.value ? formValueToDate(this.boarderFormGroup.controls.birthDateControl.value) : undefined;
    boarder.birthPlace = this.boarderFormGroup.controls.birthPlaceControl.value;
    boarder.notes = this.boarderFormGroup.controls.notesControl.value || '';
    this._service.createUpdate([boarder]).subscribe(() => {
      if (this.boarder !== undefined) {
        this.setEditMode(false);
        this.resetValidationErrorMessage();
      }
      else {
        this.navigateToBoarderList();
      }
    }, e => this.resetValidationErrorMessage(e));
  }

  setEditMode(edition: boolean) {
    this.edition = edition;
  }

  delete() {
    if (this.boarder) {
      this._service.delete([this.boarder]).subscribe(() => {
        this.navigateToBoarderList();
      }, e => this.resetValidationErrorMessage(e));
    }
  }

  navigateToBoarderList() {
    this._router.navigate(['/boarders']);
  }

  resetValidationErrorMessage(e?: any) {
    if (e) {
      if (typeof e === 'string') {
        this._formValidationErrorMessage = e;
      }
      else if (e instanceof Error) {
        this._formValidationErrorMessage = e.message;
      }
      else if (e instanceof HttpErrorResponse) {
        this._formValidationErrorMessage = e.statusText;
      }
      else {
        this._formValidationErrorMessage = JSON.stringify(e); 
      }
    }
    else {
      this._formValidationErrorMessage = undefined;
    }
  }
}

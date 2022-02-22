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

  @Input() boarderId?: number;

  boarder?: Boarder;

  boarderFormGroup: FormGroup = new FormGroup({
    nameControl: new FormControl(),
    phoneNumberControl: new FormControl(),
    birthDateControl: new FormControl(),
    birthPlaceControl: new FormControl(),
    nationalityControl: new FormControl(),
    notesControl: new FormControl(),
  });

  private getIdFromRoute(): number | undefined
  {
    const routeId = this._route.snapshot.paramMap.get('id');
    if (routeId) {
      return Number.parseInt(routeId);
    }
    return undefined;
  }

  ngOnInit(): void {
    this.boarderId = this.getIdFromRoute();
    if (this.boarderId) {
      const observable = this._service.details(this.boarderId);
      observable.subscribe(b => {
        this.boarderFormGroup.controls.nameControl.setValue(b.name);
        this.boarderFormGroup.controls.nationalityControl.setValue(b.nationality);
        this.boarderFormGroup.controls.phoneNumberControl.setValue(b.phoneNumber);
        this.boarderFormGroup.controls.birthDateControl.setValue(b.birthDate ? dateToFormValue(b.birthDate) : undefined);
        this.boarderFormGroup.controls.birthPlaceControl.setValue(b.birthPlace);
        this.boarderFormGroup.controls.notesControl.setValue(b.notes);
        this.boarder = b;
      });
    }
    else {
      // création : toujours en mode édition
      this.edition = true;
    }
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
      if (this.boarderId !== undefined) {
        this.setEditMode(false);
      }
      else {
        this.navigateToBoarderList();
      }
    }, console.error);
  }

  setEditMode(edition: boolean) {
    this.edition = edition;
  }

  delete() {
    if (this.boarder) {
      this._service.delete([this.boarder]).subscribe(() => {
        this.navigateToBoarderList();
      }, console.error);
    }
  }

  navigateToBoarderList() {
    this._router.navigate(['/boarders']);
  }
}

import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  constructor(private readonly _http: HttpClient, @Inject('API_BASE_URL') private readonly baseUrl: string) {
  }

  generateEditionDownload() {
    this._http.get(this.baseUrl + 'api/operator/generateEdition', { responseType: "blob", headers: { 'Accept': 'application/pdf' } })
      .subscribe(blob => {
        saveAs(blob, 'édition.pdf');
      });
  }
}

import { Component, Inject, OnInit } from '@angular/core';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent implements OnInit {

  constructor(@Inject('APP_VERSION') readonly version: string, @Inject('ASSETS_URL') readonly assets_url: string) { }

  ngOnInit(): void {
  }

}

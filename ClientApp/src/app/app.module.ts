import { BrowserModule } from '@angular/platform-browser';
import { LOCALE_ID, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';  

import { AppComponent } from './app.component';
import { NavMenuComponent } from './modules/nav-menu/nav-menu.component';
import { HomeComponent } from './modules/home/home.component';
import { OperatorsComponent } from './modules/settings/operators/operators.component';
import { AccountingEntriesComponent } from './modules/settings/accounting-entries/accounting-entries.component';
import { MealsComponent } from './modules/meals/meals.component';
import { OperationsComponent } from './modules/operations/operations.component';
import { BoardersComponent } from './modules/boarders/boarders.component';
import { CommonModule } from '@angular/common';
import { NgbDatepickerModule, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AccountBooksComponent } from './modules/settings/account-books/account-books.component';
import { BoardingRoomsComponent } from './modules/settings/boarding-rooms/boarding-rooms.component';
import { BoarderDetailsComponent } from './modules/boarder-details/boarder-details.component';
import { AppRoutingModule } from './app-routing.module';
import { ListTableModule } from './modules/list-table/list-table.module';
import { AccountingCategoriesComponent } from './modules/settings/accounting-categories/accounting-categories.component';
import { FooterComponent } from './modules/footer/footer.component';
import { RemissionsComponent } from './modules/remissions/remissions.component';

export const APP_VERSION = '0.9.0';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    MealsComponent,
    OperationsComponent,
    BoardersComponent,
    RemissionsComponent,
    OperatorsComponent,
    AccountingEntriesComponent,
    AccountBooksComponent,
    BoardingRoomsComponent,
    BoarderDetailsComponent,
    AccountingCategoriesComponent,
    FooterComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    NgbModule,
    AppRoutingModule,
    ListTableModule,
  ],
  providers: [{provide: LOCALE_ID, useValue: 'fr'}],
  bootstrap: [AppComponent]
})
export class AppModule { }

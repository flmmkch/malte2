import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';  

import { AppComponent } from './app.component';
import { NavMenuComponent } from './modules/nav-menu/nav-menu.component';
import { HomeComponent } from './modules/home/home.component';
import { OperatorsComponent } from './modules/operators/operators.component';
import { AccountingEntriesComponent } from './modules/accounting-entries/accounting-entries.component';
import { MealsComponent } from './modules/meals/meals.component';
import { OperationsComponent } from './modules/operations/operations.component';
import { BoardersComponent } from './modules/boarders/boarders.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    MealsComponent,
    OperationsComponent,
    BoardersComponent,
    OperatorsComponent,
    AccountingEntriesComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'operations', component: OperationsComponent },
      { path: 'boarders', component: BoardersComponent },
      { path: 'meals', component: MealsComponent },
      { path: 'settings/operators', component: OperatorsComponent },
      { path: 'settings/accounting-entries', component: AccountingEntriesComponent },
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

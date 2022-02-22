import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './modules/home/home.component';
import { OperationsComponent } from './modules/operations/operations.component';
import { BoardersComponent } from './modules/boarders/boarders.component';
import { MealsComponent } from './modules/meals/meals.component';
import { OperatorsComponent } from './modules/settings/operators/operators.component';
import { AccountBooksComponent } from './modules/settings/account-books/account-books.component';
import { AccountingEntriesComponent } from './modules/settings/accounting-entries/accounting-entries.component';
import { BoardingRoomsComponent } from './modules/settings/boarding-rooms/boarding-rooms.component';
import { BoarderDetailsComponent } from './modules/boarder-details/boarder-details.component';


const appRoutes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'operations', component: OperationsComponent },
  { path: 'boarders/add', component: BoarderDetailsComponent },
  { path: 'boarders/details/:id', component: BoarderDetailsComponent },
  { path: 'boarders/list', component: BoardersComponent },
  { path: 'boarders',   redirectTo: 'boarders/list', pathMatch: 'full' },
  { path: 'meals', component: MealsComponent },
  { path: 'settings/operators', component: OperatorsComponent },
  { path: 'settings/accounting-entries', component: AccountingEntriesComponent },
  { path: 'settings/account-books', component: AccountBooksComponent },
  { path: 'settings/boarding-rooms', component: BoardingRoomsComponent },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forRoot(
      appRoutes,
      // { enableTracing: true } // <-- debugging purposes only
    )
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule { }

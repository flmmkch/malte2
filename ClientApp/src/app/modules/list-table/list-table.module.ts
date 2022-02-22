import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ColumnHeaderDirective, ColumnValueTemplateDirective, EditorContentDirective, EmptyItemsContentDirective, ListTable, ListTableColumn } from './list-table.component';



@NgModule({
  declarations: [
    ListTable,
    ListTableColumn,
    EditorContentDirective,
    EmptyItemsContentDirective,
    ColumnValueTemplateDirective,
    ColumnHeaderDirective,
  ],
  imports: [
    CommonModule,
  ],
  exports: [
    ListTable,
    ListTableColumn,
    ColumnValueTemplateDirective,
    ColumnHeaderDirective,
    EditorContentDirective,
    EmptyItemsContentDirective,
  ],
})
export class ListTableModule { }

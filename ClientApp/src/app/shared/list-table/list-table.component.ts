import { Component, ContentChild, ContentChildren, ElementRef, EventEmitter, Input, Output, QueryList, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs';

export enum EditMode {
  NewItem,
  ModifyItem,
}

export type ObservableLike<T> = Observable<T> | { (): T } | { (): PromiseLike<T> };

@Component({
  selector: 'list-table-column',
  template: ``
})
export class ListTableColumn {
  @Input() width: string = 'auto';

  @Input() title: string = '';

  @Input() readonly: boolean = false;

  @Input() rowheader: boolean = false;

  @Input() placeholder: string = '';

  @Input() property: string | undefined;

  displayItemProperty(item: any) {
    if (this.property && this.property in item) {
      return item[this.property];
    }
    return '';
  }
}

@Component({
  selector: 'list-table',
  templateUrl: './list-table.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./list-table.component.css']
})
export class ListTable {

  constructor() { }

  @Input() editable: boolean = true;

  @Input() enableAddRemove: boolean = true;

  private _currentEditMode: EditMode | null = null;

  public get currentEditMode(): EditMode | null {
    return this._currentEditMode;
  }

  public set currentEditMode(value: EditMode | null) {
    if (this.editable || this.enableAddRemove) {
      this._currentEditMode = value;
    }
    else {
      console.error('Unable to set edit mode in this list table.');
    }
  }

  @Input() items: any[] = [];

  @ContentChildren(ListTableColumn) columns!: QueryList<ListTableColumn>;

  @ContentChild('editor') editor: ElementRef<HTMLElement> | undefined;

  @Output() onSubmitNew: EventEmitter<any> = new EventEmitter();

  @Output() onSubmitEdit: EventEmitter<any> = new EventEmitter();

  @Output() onDelete: EventEmitter<any> = new EventEmitter();

  @Input() confirmDelete: boolean = true;

  @Input() confirmDeleteMessage: string | { (item: any): string } = 'Supprimer cet élément?';

  editingNewItem(): boolean {
    return this.currentEditMode == EditMode.NewItem;
  }

  editing(): boolean {
    return this.currentEditMode != null;
  }

  cancelEdit() {
    this.currentEditMode = null;
  }

  submitNewItem() {
    this.onSubmitNew?.emit();
  }

  currentWorkingItem: any = null;

  addItem() {
    // TODO
    this.currentEditMode = EditMode.NewItem;
    this.editor?.nativeElement.focus();
  }

  modifyItem(item: any) {
    // TODO
  }

  deleteItem(item: any) {
    this.onDelete?.emit(item);
    this.currentWorkingItem = null;
  }

  totalColumnCount(): number {
    return this.columns.length + (this.editable || this.enableAddRemove ? 2 : 0);
  }

  confirmDeleteMessageDisplay(): string {
    if (typeof(this.confirmDeleteMessage) == 'string') {
      return this.confirmDeleteMessage;
    }
    return this.confirmDeleteMessage(this.currentWorkingItem);
  }
}

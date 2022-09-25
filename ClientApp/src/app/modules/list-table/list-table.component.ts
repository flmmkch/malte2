import { AfterViewChecked, Component, ContentChild, ContentChildren, Directive, ElementRef, EventEmitter, Input, Output, QueryList, TemplateRef, ViewEncapsulation } from '@angular/core';
import { Observable } from 'rxjs';
import { Modal } from 'bootstrap';

export enum EditMode {
  NewItem,
  ModifyItem,
}

export type ObservableLike<T> = Observable<T> | { (): T } | { (): PromiseLike<T> };

@Directive({
  selector: '[list-table-empty]'
})
export class EmptyItemsContentDirective {
  
}

@Directive({
  selector: '[list-table-editor]'
})
export class EditorContentDirective {
  constructor(public elementRef: ElementRef<HTMLElement>) {}
}

@Directive({
  selector: '[value-template]',
  inputs: ['visibleOnlyOnHover: visible-on-hover']
})
export class ColumnValueTemplateDirective {
  constructor(public templateRef: TemplateRef<unknown>) {}
  
  visibleOnlyOnHover: boolean = false;
}

@Directive({
  selector: '[column-header]',
})
export class ColumnHeaderDirective {
  constructor(public templateRef: TemplateRef<unknown>) {}
}

export type ListTableColumnType = 'text' | 'checkbox';

@Component({
  selector: 'list-table-column',
  template: ''
})
export class ListTableColumn {
  @Input() width: string = 'auto';

  @Input() title: string = '';

  @Input() readonly: boolean = false;

  @Input() rowheader: boolean = false;

  @Input() placeholder: string = '';

  @Input() property?: string;

  @Input() type: ListTableColumnType = 'text';

  @ContentChild(ColumnValueTemplateDirective) valueTemplate?: ColumnValueTemplateDirective;

  @ContentChild(ColumnHeaderDirective) headerContent?: ColumnHeaderDirective;

  getItemProperty<T>(item: T): string {
    if (this.property && this.property in item) {
      const prop = this.property as keyof (typeof item);
      const value = item[prop];
      if (typeof value === 'string') {
        return value;
      }
      if (typeof value === 'boolean' || typeof value === 'number' || typeof value === 'bigint') {
        return value.toString();
      }
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
export class ListTable implements AfterViewChecked {

  constructor() { }

  private readonly _viewCheckedEvent = new EventEmitter();

  get viewChecked(): Observable<Object> {
    return this._viewCheckedEvent;
  }

  ngAfterViewChecked(): void {
    this._viewCheckedEvent.emit();
  }

  @Input() editable: boolean = true;

  @Input() enableAddRemove: boolean = true;

  @Input() addLabel: string = 'Ajouter';

  @Input() modifyLabel: string = 'Modifier';

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

  @Input() items: Object[] | undefined;

  @ContentChildren(ListTableColumn) columns!: QueryList<ListTableColumn>;

  @ContentChild(EditorContentDirective) editor?: EditorContentDirective;

  @ContentChild(EmptyItemsContentDirective) emptyContent?: EmptyItemsContentDirective;

  getEditorForm(): HTMLFormElement | null {
    const editorElement = this.editor?.elementRef.nativeElement;
    if (editorElement instanceof HTMLFormElement) {
      return editorElement;
    }
    else {
      return editorElement?.querySelector('form') || null;
    }
  }
  
  @Output() onCreate: EventEmitter<any> = new EventEmitter();

  @Output() onDelete: EventEmitter<Object> = new EventEmitter();

  @Output() onSetWorkingItem: EventEmitter<SetCurrentWorkingItemEventArgs<Object>> = new EventEmitter();

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
    this.currentWorkingItem = null;
  }

  private _currentWorkingItem: Object | null = null;

  /** Objet en cours d'édition */
  public get currentWorkingItem(): Object | null {
    return this._currentWorkingItem;
  }

  /** Objet en cours d'édition */
  public set currentWorkingItem(value: Object | null) {
    const eventArgs: SetCurrentWorkingItemEventArgs<Object> = { value: value, oldValue: this._currentWorkingItem, preventDefault: false };
    this.onSetWorkingItem.emit(eventArgs);
    if (!eventArgs.preventDefault) {
      this._currentWorkingItem = value;
    }
  }

  private focusFormOnNextUpdate() {
    const subscription = this.viewChecked.subscribe(() => {
      this.getEditorForm()?.querySelector<HTMLInputElement>('[autofocus]')?.focus();
      subscription.unsubscribe();
    });
  }

  /** Entrer dans le mode d'ajout d'un nouvel élément */
  addItem() {
    this.onCreate.emit();
    this.currentEditMode = EditMode.NewItem;
    this.focusFormOnNextUpdate();
  }

  /** Entrer dans le mode de modification d'un élément de la liste */
  modifyItem(item: Object) {
    this.currentWorkingItem = item;
    this.currentEditMode = EditMode.ModifyItem;
    this.focusFormOnNextUpdate();
  }

  /** Supprimer un élément de la liste avec demande de confirmation si nécessaire */
  askDeleteItem(item: Object) {
    this.currentWorkingItem = item;
    if (this.confirmDelete) {
      const subscription = this.viewChecked.subscribe(() => {
        const modalElement = document.querySelector('#confirmDeleteModal')!;
        const modal = new Modal(modalElement, { });
        modal.show();
        subscription.unsubscribe();
      });
    }
    else {
      this.deleteItem(item);
    }
  }

  /** Supprimer un élément de la liste */
  deleteItem(item: Object) {
    this.onDelete?.emit(item);
    this.currentWorkingItem = null;
  }

  /** Nombre total de colonnes dans le tableau */
  get totalColumnCount(): number {
    return this.columns.length + (this.editable || this.enableAddRemove ? 2 : 0);
  }

  confirmDeleteMessageDisplay(): string {
    if (typeof(this.confirmDeleteMessage) == 'string') {
      return this.confirmDeleteMessage;
    }
    return this.confirmDeleteMessage(this.currentWorkingItem);
  }

  get validateButtonLabel(): string {
    if (this.editingNewItem()) {
      return 'Ajouter';
    }
    return 'Valider';
  }
}

export interface SetCurrentWorkingItemEventArgs<T extends Object> {
  value: T | null;
  oldValue: T | null;
  preventDefault: boolean;
}

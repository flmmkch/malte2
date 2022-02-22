import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListTable } from './list-table.component';

describe('ListTableComponent', () => {
  let component: ListTable;
  let fixture: ComponentFixture<ListTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ListTable ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ListTable);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

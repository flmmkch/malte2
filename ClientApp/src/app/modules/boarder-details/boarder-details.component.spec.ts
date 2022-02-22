import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BoarderDetailsComponent } from './boarder-details.component';

describe('BoarderDetailsComponent', () => {
  let component: BoarderDetailsComponent;
  let fixture: ComponentFixture<BoarderDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BoarderDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BoarderDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

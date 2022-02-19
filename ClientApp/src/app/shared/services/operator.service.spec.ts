import { HttpClientModule } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';

import { OperatorService } from './operator.service';

describe('OperatorService', () => {
  let service: OperatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule],
      providers: [{ provide: 'BASE_URL', useValue: 'http://localhost/', deps: [] }]
    });
    service = TestBed.inject(OperatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

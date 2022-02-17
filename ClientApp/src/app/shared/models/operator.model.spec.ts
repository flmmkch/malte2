import { Operator } from './operator.model';

describe('Operator', () => {
  it('should create an instance', () => {
    expect(new Operator('Pierre')).toBeTruthy();
  });
});

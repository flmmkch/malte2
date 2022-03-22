import { Amount } from './amount.model';

describe('Amount', () => {
    it('should be able to be written to a string', () => {
        expect(Amount.from(0)?.toString()).toEqual('0.00');
        expect(Amount.from(10)?.toString()).toEqual('10.00');
        expect(Amount.from(12)?.toString()).toEqual('12.00');
        expect(Amount.from(12.5)?.toString()).toEqual('12.50');
        expect(Amount.from(12.51)?.toString()).toEqual('12.51');
    });
    it('should be able to be parsed from a string', () => {
        expect(Amount.from('0')?.toString()).toEqual('0.00');
        expect(Amount.from('10')?.toString()).toEqual('10.00');
        expect(Amount.from('12')?.toString()).toEqual('12.00');
        expect(Amount.from('12.5')?.toString()).toEqual('12.50');
        expect(Amount.from('12.51')?.toString()).toEqual('12.51');
    });
    it('should be able to be written to a string (negative)', () => {
        expect(Amount.from(-0)?.toString()).toEqual('0.00');
        expect(Amount.from(-10)?.toString()).toEqual('-10.00');
        expect(Amount.from(-12)?.toString()).toEqual('-12.00');
        expect(Amount.from(-12.5)?.toString()).toEqual('-12.50');
        expect(Amount.from(-12.51)?.toString()).toEqual('-12.51');
    });
    it('should be able to be parsed from a string (negative)', () => {
        expect(Amount.from('-0')?.toString()).toEqual('0.00');
        expect(Amount.from('-10')?.toString()).toEqual('-10.00');
        expect(Amount.from('-12')?.toString()).toEqual('-12.00');
        expect(Amount.from('-12.5')?.toString()).toEqual('-12.50');
        expect(Amount.from('-12.51')?.toString()).toEqual('-12.51');
    });
});

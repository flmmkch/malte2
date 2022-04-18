export class Amount {
    private constructor(private _value: bigint) {}

    private static amountParsingRegex = /(?<minusSign>-)?(?<integralPart>[0-9]+)(?:\.(?<decimalPart>[0-9]{1,2}).*)?/;
    
    public static from(value: string | number): Amount | undefined {
        if (typeof value === 'string') {
            const match = value.match(this.amountParsingRegex);
            if (match && match.groups?.integralPart) {
                const integralPartStr = match.groups.integralPart;
                const integralPart = BigInt(integralPartStr) * BigInt(100);
                let decimalPart;
                if (match.groups?.decimalPart && match.groups?.decimalPart.length === 2) {
                    decimalPart = BigInt(match.groups?.decimalPart);
                }
                else if (match.groups?.decimalPart && match.groups?.decimalPart.length === 1) {
                    decimalPart = BigInt(match.groups?.decimalPart) * BigInt(10);
                }
                else {
                    decimalPart = BigInt(0);
                }
                let bigIntValue = integralPart + decimalPart;
                if (match.groups.minusSign) {
                    bigIntValue = - bigIntValue;
                }
                return new Amount(bigIntValue);
            }
        }
        else if (typeof value === 'number') {
            return new Amount(BigInt(value * 100));
        }
        return undefined;
    }

    public static LOCAL_DECIMAL_SEPARATOR = Amount.getDecimalSeparator();
    public static GLOBAL_DECIMAL_SEPARATOR = '.';

    private static getDecimalSeparator(locale?: string): string {
        return Intl.NumberFormat(locale).formatToParts(1.1).find(part => part.type == 'decimal')!.value;
    }

    public static fromStringLocale(value: string): Amount | undefined {
        const translatedValue = value.replace(Amount.LOCAL_DECIMAL_SEPARATOR, Amount.GLOBAL_DECIMAL_SEPARATOR);
        return Amount.from(translatedValue);
    }

    public toString(): string {
        let valueString = this._value.toString(10);
        if (valueString.length === 2) {
            valueString = `0${valueString}`;
        }
        else if (valueString.length === 1) {
            valueString = `00${valueString}`;
        }
        valueString = `${valueString.slice(0, valueString.length - 2)}${Amount.GLOBAL_DECIMAL_SEPARATOR}${valueString.slice(valueString.length - 2)}`;
        return valueString;
    }

    public toLocaleString(): string {
        return this.toString().replace(Amount.GLOBAL_DECIMAL_SEPARATOR, Amount.LOCAL_DECIMAL_SEPARATOR);
    }

    public add(amount: Amount): Amount {
        return new Amount(this._value + amount._value);
    }

    public substract(amount: Amount): Amount {
        return new Amount(this._value - amount._value);
    }

    public isStrictPositive(): boolean {
        return this._value > 0;
    }

    public isStrictNegative(): boolean {
        return this._value < 0;
    }
}
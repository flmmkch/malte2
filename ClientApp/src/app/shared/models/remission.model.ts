import { Amount } from "./amount.model";

export class Remission {
    constructor(public readonly id: number | undefined,
        /** Opérateur */
        public operatorId: number,
    ) {
        this.dateTime = new Date();
    }
    /** Date de l'opération */
    public dateTime: Date;
    /** Notes */
    public notes: string = '';
    /** Chèques */
    public checkRemissions: CheckRemission[] = [];
    /** Espèces */
    public cashDeposits: CashDeposit[] = [];

    public get totalCashAmount(): Amount {
        return this.cashDeposits.map(cashDeposit => cashDeposit.totalAmount).reduce((sum, amount) => sum.add(amount), Amount.from(0)!);
    }

    public get totalCheckAmount(): Amount {
        return this.checkRemissions.map(checkRemission => checkRemission.amount).reduce((sum, amount) => sum.add(amount), Amount.from(0)!);
    }

    public get totalAmount(): Amount {
        return this.totalCashAmount.add(this.totalCheckAmount);
    }
}

export class CheckRemission {
    constructor(
        public amount: Amount,
        public checkNumber?: bigint,
    ) {}
}

export enum CashValue {
    c01,
    c02,
    c05,
    c10,
    c20,
    c50,
    e001,
    e002,
    e005,
    e010,
    e020,
    e050,
    e100,
    e200,
    e500,
}

export function allCashValueItems(): CashValue[] {
    return [
        CashValue.c01,
        CashValue.c02,
        CashValue.c05,
        CashValue.c10,
        CashValue.c20,
        CashValue.c50,
        CashValue.e001,
        CashValue.e002,
        CashValue.e005,
        CashValue.e010,
        CashValue.e020,
        CashValue.e050,
        CashValue.e100,
        CashValue.e200,
        CashValue.e500,
    ];
}

export function getCashValueAmount(cashValue: CashValue): Amount {
    switch (cashValue) {
        case CashValue.c01:
            return Amount.from(0.01)!;
        case CashValue.c02:
            return Amount.from(0.02)!;
        case CashValue.c05:
            return Amount.from(0.05)!;
        case CashValue.c10:
            return Amount.from(0.10)!;
        case CashValue.c20:
            return Amount.from(0.20)!;
        case CashValue.c50:
            return Amount.from(0.50)!;
        case CashValue.e001:
            return Amount.from(1)!;
        case CashValue.e002:
            return Amount.from(2)!;
        case CashValue.e005:
            return Amount.from(5)!;
        case CashValue.e010:
            return Amount.from(10)!;
        case CashValue.e020:
            return Amount.from(20)!;
        case CashValue.e050:
            return Amount.from(50)!;
        case CashValue.e100:
            return Amount.from(100)!;
        case CashValue.e200:
            return Amount.from(200)!;
        case CashValue.e500:
            return Amount.from(500)!;
        default:
            throw new Error(`Invalid cash value item: ${cashValue}`);
    }
}

export class CashDeposit {
    constructor(
        public readonly value: CashValue,
        public count: bigint = BigInt(0),
    ) {
    }
    public get totalAmount(): Amount {
        return getCashValueAmount(this.value).multiply(this.count);
    }
}

export class RemissionOperationCheck {
    constructor(
        /** Check number */
        public checkNumber: number,
        /** Operation date */
        public dateTime: Date,
        /** Operation label */
        public label: string,
        /** Operation details */
        public details: string,
        /** Check amount */
        public amount: Amount,
        /** Check remission id */
        public remissionId?: number,
        
    )
    {
    }
}

export class Operator {
    public constructor(
        public name: string,
        public readonly id?: number,
        public phone: string = '',
        public enabled: boolean = true,
    ) { }

    public static fromJson(json: OperatorJson): Operator {
        const operator = new Operator(json.n, json.id);
        operator.phone = json.p || '';
        if (json.e !== undefined) {
            operator.enabled = json.e;
        }
        return operator;
    }

    public toJson(): OperatorJson {
        return {
            id: this.id,
            n: this.name,
            p: this.phone,
            e: this.enabled,
        }
    }
}

export interface OperatorJson {
    id?: number,
    n: string,
    p?: string,
    e?: boolean,
}
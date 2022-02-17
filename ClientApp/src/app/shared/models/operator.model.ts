export class Operator {
    public constructor(
        public name: string,
        public readonly id?: number,
        public enabled: boolean = true,
    ) { }

    public static fromJson(json: OperatorJson): Operator {
        let operator = new Operator(json.n, json.id);
        if (json.e) {
            operator.enabled = json.e;
        }
        return operator;
    }

    public toJson(): OperatorJson {
        return {
            id: this.id,
            n: this.name,
            e: this.enabled,
        }
    }
}

export interface OperatorJson {
    id?: number,
    n: string,
    e?: boolean,
}
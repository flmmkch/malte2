export class MealDay {

    constructor(public readonly id: number | undefined,
        ) {
        this.dateTime = new Date();
    }

    public dateTime: Date;
    
    public boarderCount: number = 0;
    public patronCount: number = 0;
    public otherCount: number = 0;
    public catererCount: number = 0;
}

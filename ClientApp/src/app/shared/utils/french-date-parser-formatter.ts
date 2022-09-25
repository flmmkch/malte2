import { Injectable } from "@angular/core";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";

@Injectable()
export class FrenchDateParserFormatter {
    parse(value: string): NgbDateStruct {
        const dateReMatch = value.match(/([0-9]{1,2})\/([0-9]{1,2})\/([0-9]+)/);
        if (dateReMatch && dateReMatch.length === 3) {
            const [dayStr, monthStr, yearStr] = dateReMatch;
            return {
                day: Number.parseInt(dayStr),
                month: Number.parseInt(monthStr),
                year: Number.parseInt(yearStr),
            };
        }
        throw new Error(`Failed to parse date from ${value}`);
    }
    format(date: NgbDateStruct): string {
        if (date) {
            return `${date.day.toString().padStart(2, '0')}/${date.month.toString().padStart(2, '0')}/${date.year.toString().padStart(4, '0')}`;
        }
        return '';
    }
}

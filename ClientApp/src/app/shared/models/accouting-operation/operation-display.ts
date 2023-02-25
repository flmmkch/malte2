import { Operation } from "../operation.model";
import { paymentMethodString } from "../payment-method.model";
import { ContextDicts } from "./context-dicts";

export interface OperationDisplay {
    operation: Operation;
    amount: string;
    accountingEntryName: string;
    categoryName: string;
    accountBookName: string;
    dateTime: string;
    operatorName: string;
    label: string;
    boarderName: string;
    paymentMethod: string;
}

export function createOperationDisplay(op: Operation, { books, entries, categories, operators, boarders }: ContextDicts): OperationDisplay {
    const accountBookName = op.accountBookId in books ? books[op.accountBookId].label : '';
    const accountingEntryName = op.accountingEntryId in entries ? entries[op.accountingEntryId].label : '';
    const categoryName = (op.categoryId !== undefined && op.categoryId in categories) ? categories[op.categoryId].label : '';
    const operatorName = op.operatorId in operators ? operators[op.operatorId].name : '';
    const boarderName = op.boarderId && op.boarderId in boarders ? boarders[op.boarderId].name : '';
    const opDisplay: OperationDisplay = {
        operation: op,
        amount: op.amount.toLocaleString(),
        accountBookName,
        categoryName,
        accountingEntryName,
        dateTime: op.dateTime.toLocaleDateString(),
        operatorName,
        boarderName,
        label: op.label,
        paymentMethod: paymentMethodString(op.paymentMethod),
    };
    return opDisplay;
}

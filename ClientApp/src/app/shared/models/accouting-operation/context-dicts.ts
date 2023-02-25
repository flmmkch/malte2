import { DictionaryById } from "../../utils/dictionary-by-id";
import { Operator } from "../operator.model";
import { AccountBook } from "../account-book.model";
import { AccountingCategory } from "../accounting-category.model";
import { AccountingEntry } from "../accounting-entry.model";
import { BoarderListItem } from "../boarder.model";

export interface ContextDicts {
    operators: DictionaryById<Operator>;
    books: DictionaryById<AccountBook>;
    entries: DictionaryById<AccountingEntry>;
    categories: DictionaryById<AccountingCategory>;
    boarders: DictionaryById<BoarderListItem>;
}

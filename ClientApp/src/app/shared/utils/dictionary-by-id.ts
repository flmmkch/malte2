export interface IHasId {
    id?: number;
};

export type DictionaryById<T> = { [id: number] : T };

export function listToDictionary<T extends IHasId>(array: T[]): DictionaryById<T> {
    const dictionary: DictionaryById<T> = { };
    for (const item of array) {
        if (typeof item.id === 'number') {
            dictionary[item.id] = item;
        }
    }
    return dictionary;
}

export function listToDictionaryWithFunc<T>(array: T[], idFunc: (item: T) => number | undefined): DictionaryById<T> {
    const dictionary: DictionaryById<T> = { };
    for (const item of array) {
        const id = idFunc(item);
        if (typeof id === 'number') {
            dictionary[id] = item;
        }
    }
    return dictionary;
}

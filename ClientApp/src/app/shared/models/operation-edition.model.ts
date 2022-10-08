
export enum OperationEditionType {
    ByAccountBook = 0,
    ByAccountBookAndPaymentMethod = 1,
    ByAccountBookAndCategory = 2,
}

export function allOperationEditionTypes(): OperationEditionType[] {
    return [
        OperationEditionType.ByAccountBook,
        OperationEditionType.ByAccountBookAndPaymentMethod,
        OperationEditionType.ByAccountBookAndCategory,
    ];
}

export function operationEditionTypeString(operationEditionType: OperationEditionType): string {
    switch (operationEditionType) {
        case OperationEditionType.ByAccountBook:
            return 'Par livre de compte';
        case OperationEditionType.ByAccountBookAndPaymentMethod:
            return 'Par livre et par moyen de paiement';
        case OperationEditionType.ByAccountBookAndCategory:
            return 'Par livre et par catégorie';
    }
}


/** Moyen de paiement */
export enum PaymentMethod {
    /** Espèces */
    Cash = 0,
    /** Chèque */
    Check = 1,
    /** Carte */
    Card = 2,
    /** Virement */
    Transfer = 3,
}

export function allPaymentMethods(): PaymentMethod[] {
    return [PaymentMethod.Cash, PaymentMethod.Check, PaymentMethod.Card, PaymentMethod.Transfer];
}

export function paymentMethodString(paymentMethod: PaymentMethod): string {
    switch (paymentMethod) {
        case PaymentMethod.Card:
            return 'Carte';
        case PaymentMethod.Cash:
            return 'Espèces';
        case PaymentMethod.Check:
            return 'Chèque';
        case PaymentMethod.Transfer:
            return 'Virement';
    }
}

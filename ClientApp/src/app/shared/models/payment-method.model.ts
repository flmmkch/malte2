/** Moyen de paiement */
export enum PaymentMethod {
    /** Espèces */
    Cash,
    /** Chèque */
    Check,
    /** Carte */
    Card,
    /** Virement */
    Transfer,
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
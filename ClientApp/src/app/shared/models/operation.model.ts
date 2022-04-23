import { Amount } from "./amount.model";
import { PaymentMethod } from "./payment-method.model";

export class Operation {
    constructor(public readonly id: number | undefined,
        /** Montant */
        public amount: Amount,
        /** Imputation comptable */
        public accountingEntryId: number,
        /** Moyen de paiement */
        public paymentMethod: PaymentMethod,
        /** Livre comptable */
        public accountBookId: number,
        /** Opérateur */
        public operatorId: number,
        ) {
        this.dateTime = new Date();
    }
    /** Date de l'opération */
    public dateTime: Date;
    /** Libellé */
    public label: string = '';
    public details: string = '';
    /** Facture */
    public invoice?: string;
    /** Pensionnaire */
    public boarderId?: number;
    /** Catégorie */
    public categoryId?: number;
    /** Détails de la méthode de paiement */
    public checkNumber?: bigint;
    public cardTicketNumber?: bigint;
    public transferNumber?: bigint;
}

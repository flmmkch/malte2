/** Opérateur */
export class Operator {
    public constructor(
        /** Nom de l'opérateur */
        public name: string,
        /** ID de l'opérateur */
        public readonly id?: number,
        /** Numéro de téléphone de l'opérateur */
        public phone: string = '',
        /** Actif ou non */
        public enabled: boolean = true,
    ) { }
}

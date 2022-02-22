/** Pensionnaire */
export class Boarder {
    constructor(public readonly id?: number) { }
    /** Nom */
    public name: string = '';

    /** Date de naissance */
    public birthDate?: Date;
    /** Lieu de naissance */
    public birthPlace?: string;
    /** Nationalité */
    public nationality: string = '';

    /** Numéro de téléphone portable */
    public phoneNumber: string = '';

    /** Divers */
    public notes: string = '';

    /** TODO cautions, référent opérateur, autres champs */
}

export class BoarderListItem {
    constructor(public readonly boarderId: number,
        public name: string,
        public currentRoomName?: string
    ) { }
}

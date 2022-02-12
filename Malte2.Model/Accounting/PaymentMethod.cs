namespace Malte2.Model.Accounting
{

    /// <summary>Moyen de paiement</summary>
    public enum PaymentMethod
    {
        /// <summary>Espèces</summary>
        Cash,
        /// <summary>Chèque</summary>
        Check,
        /// <summary>Carte bancaire</summary>
        Card,
        /// <summary>Virement</summary>
        Transfer,
    }

}
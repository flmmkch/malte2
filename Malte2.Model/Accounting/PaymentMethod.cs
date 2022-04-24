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

    public static class PaymentMethodExtensions {

        public static string GetDisplayString(this PaymentMethod paymentMethod) {
            switch (paymentMethod) {
                case PaymentMethod.Cash:
                    return "Espèces";
                case PaymentMethod.Card:
                    return "Carte";
                case PaymentMethod.Check:
                    return "Chèque";
                case PaymentMethod.Transfer:
                    return "Virement";
            }
            return "";
        }
    }

}
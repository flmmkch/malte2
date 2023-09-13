namespace Malte2.Model.Accounting
{

    /// <summary>Moyen de paiement</summary>
    public enum PaymentMethod
    {
        /// <summary>Espèces</summary>
        Cash = 0,
        /// <summary>Chèque</summary>
        Check = 1,
        /// <summary>Carte bancaire</summary>
        Card = 2,
        /// <summary>Virement</summary>
        Transfer = 3,
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
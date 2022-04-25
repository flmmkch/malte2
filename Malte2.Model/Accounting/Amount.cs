using System.Globalization;
using System.Linq;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    /// <summary>Montant comptable</summary>
    [JsonConverter(typeof(Amount.JsonConverter))]
    public struct Amount
    {
        public static readonly long PRECISION = 100;

        public static readonly int DECIMALS = (int)Math.Log10(PRECISION);

        /// <summary>Amount * PRECISION</summary>
        private long _amount;

        public Amount()
        {
            this._amount = 0;
        }

        public Amount(long amount)
        {
            _amount = amount;
        }

        public long GetLong()
        {
            return _amount;
        }

        public override string ToString()
        {
            return ToCultureString(CultureInfo.InvariantCulture);
        }

        public string ToCultureString(CultureInfo cultureInfo)
        {
            long beforeDecimal = _amount / PRECISION;
            long afterDecimal = (_amount > 0 ? _amount : -_amount) % PRECISION;
            return $"{beforeDecimal}{cultureInfo.NumberFormat.NumberDecimalSeparator}{afterDecimal:00}";
        }

        public static Amount operator +(Amount amount1, Amount amount2)
        {
            return new Amount(amount1._amount + amount2._amount);
        }

        public static Amount operator -(Amount amount1, Amount amount2)
        {
            return new Amount(amount1._amount - amount2._amount);
        }

        public static Amount operator -(Amount amount)
        {
            return new Amount(-amount._amount);
        }

        public static Amount? TryFromString(string amountString, CultureInfo? culture = null)
        {
            if (culture == null) {
                culture = CultureInfo.InvariantCulture;
            }
            int decimalStrIndex = amountString.IndexOf(culture.NumberFormat.NumberDecimalSeparator);
            if (decimalStrIndex > 0)
            {
                string beforeDecimalStr = amountString.Substring(0, decimalStrIndex);
                if (long.TryParse(beforeDecimalStr, out long beforeDecimal))
                {
                    string afterDecimalStr = "";
                    if (decimalStrIndex + 1 < amountString.Length)
                    {
                        afterDecimalStr = amountString.Substring(decimalStrIndex + 1, Math.Min(2, amountString.Length - decimalStrIndex - 1));
                    }
                    var afterDecimalSb = new System.Text.StringBuilder(afterDecimalStr);
                    afterDecimalSb.Append(Enumerable.Repeat('0', DECIMALS - afterDecimalStr.Length).ToArray());
                    string fullAfterDecimalStr = afterDecimalSb.ToString();
                    if (long.TryParse(fullAfterDecimalStr, out long afterDecimal))
                    {
                        if (beforeDecimal < 0)
                        {
                            afterDecimal = -afterDecimal;
                        }
                        return new Amount(beforeDecimal * PRECISION + afterDecimal);
                    }
                }
            }
            else
            {
                if (long.TryParse(amountString, out long parsedAmount))
                {
                    return new Amount(parsedAmount * PRECISION);
                }
            }
            return null;
        }

        public static Amount FromString(string amountString)
        {
            Amount? amount = TryFromString(amountString);
            if (amount.HasValue)
            {
                return amount.Value;
            }
            throw new System.ArgumentException($"Failed to parse amount: \"{amountString}\".");
        }

        public class JsonConverter : System.Text.Json.Serialization.JsonConverter<Amount>
        {
            public override Amount Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                return Amount.FromString(reader.GetString()!);
            }

            public override void Write(
                Utf8JsonWriter writer,
                Amount amount,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(amount.ToString());
            }
        }
    }

}
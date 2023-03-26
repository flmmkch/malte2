using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace Malte2.Model.Accounting
{

    public class OperationEditionCsvLine
    {
        [Name("Date")]
        public DateOnly Date { get; set; }

        [Name("Recettes")]
        [TypeConverter(typeof(NullableAmountToLocaleStringConverter))]
        public Amount? Revenues { get; set; }

        [Name("Dépenses")]
        [TypeConverter(typeof(NullableAmountToLocaleStringConverter))]
        public Amount? Expenses { get; set; }

        [Name("Livre comptable")]
        public string AccountBookName { get; set; } = "";


        [Name("Imputation comptable")]
        public string AccountingEntryName { get; set; } = "";

        [Name("Catégorie")]
        public string? CategoryName { get; set; }

        [Name("Moyen de paiement")]
        [TypeConverter(typeof(PaymentMethodToStringConverter))]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Name("Libellé")]
        public string Label { get; set; } = "";

        private class PaymentMethodToStringConverter: ITypeConverter {

            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) {
                throw new NotImplementedException();
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) {
                return ((PaymentMethod) value).GetDisplayString();
            }
        }

        private class NullableAmountToLocaleStringConverter: ITypeConverter {

            public object? ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) {
                if (!string.IsNullOrWhiteSpace(text)) {
                    return Amount.TryFromString(text, row.Configuration.CultureInfo);
                }
                else {
                    return null;
                }
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) {
                Amount? amount = (Amount?) value;
                if (amount != null) {
                    return amount.Value.ToCultureString(row.Configuration.CultureInfo);
                }
                else {
                    return string.Empty;
                }
            }
            
        }
    }
}
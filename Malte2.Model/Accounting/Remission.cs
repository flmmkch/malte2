using System.Text.Json.Serialization;
using Malte2.Model.Attributes;

namespace Malte2.Model.Accounting
{

    public class Remission : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("dt")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("o")]
        public long OperatorId { get; set; }
        
        [JsonPropertyName("n")]
        public string Notes { get; set; } = "";

        public enum CashValue {
            [SqlColumn("cash_deposit_01c")]
            c01,
            [SqlColumn("cash_deposit_02c")]
            c02,
            [SqlColumn("cash_deposit_05c")]
            c05,
            [SqlColumn("cash_deposit_10c")]
            c10,
            [SqlColumn("cash_deposit_20c")]
            c20,
            [SqlColumn("cash_deposit_50c")]
            c50,
            [SqlColumn("cash_deposit_001e")]
            e001,
            [SqlColumn("cash_deposit_002e")]
            e002,
            [SqlColumn("cash_deposit_005e")]
            e005,
            [SqlColumn("cash_deposit_010e")]
            e010,
            [SqlColumn("cash_deposit_020e")]
            e020,
            [SqlColumn("cash_deposit_050e")]
            e050,
            [SqlColumn("cash_deposit_100e")]
            e100,
            [SqlColumn("cash_deposit_200e")]
            e200,
            [SqlColumn("cash_deposit_500e")]
            e500,
        }

        public struct CashDeposit {
            [JsonPropertyName("v")]
            public CashValue Value { get; set; }
            [JsonPropertyName("n")]
            public int Count { get; set; }
        }

        [JsonPropertyName("h")]
        public List<CashDeposit> CashDeposits { get; set; } = new List<CashDeposit>();

        public struct CheckRemission {
            [JsonPropertyName("n")]
            public ulong? CheckNumber { get; set; }
            [JsonPropertyName("a")]
            public Amount Amount { get; set; } = new Amount();

            public CheckRemission(Amount amount, ulong? checkNumber = null)
            {
                Amount = amount;
                CheckNumber = checkNumber;
            }
        }

        [JsonPropertyName("k")]
        public List<CheckRemission> CheckRemissions { get; set; } = new List<CheckRemission>();

        public static string? GetCashValueSqlColumnName(CashValue cashValue) {
            System.Reflection.MemberInfo? memberInfo = typeof(CashValue).GetMember(cashValue.ToString()).FirstOrDefault();

            if (memberInfo != null)
            {
                return SqlColumnAttribute.GetColumnName(memberInfo!);
            }
            return null;
        }
    }

}
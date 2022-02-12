using Xunit;
using Malte2.Model.Accounting;
using System;

namespace Malte2.Tests
{

    public class AmountTest
    {
        [Fact]
        public void CheckDecimalsIsAPositiveNaturalNumber()
        {
            Assert.True(Amount.DECIMALS > 0, $"{Amount.DECIMALS} is not a positive natural number.");
        }

        [Fact]
        public void CheckPrecisionDecimals()
        {
            double decimalsPow = Math.Pow(10.0, Amount.DECIMALS);
            Assert.Equal(decimalsPow, Amount.PRECISION);
        }

        [Theory]
        [InlineData("3.14", "3.14")]
        [InlineData("3.00", "3")]
        [InlineData("30.00", "30")]
        [InlineData("1500.00", "1500")]
        [InlineData("1500.00", "1500.0")]
        [InlineData("1500.00", "1500.00")]
        [InlineData("1500.50", "1500.50")]
        [InlineData("1500.57", "1500.57")]
        [InlineData("-1500.57", "-1500.57")]
        [InlineData("-1500.00", "-1500")]
        [InlineData("0.00", "0")]
        [InlineData("0.00", "0.00")]
        public void CheckAmountParsing(string expectedString, string inputString)
        {
            Amount amount = Amount.FromString(inputString);
            string newAmountString = amount.ToString();

            Assert.Equal(expectedString, newAmountString);
        }
    }
}
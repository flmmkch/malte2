using System.Data.SQLite;
using System.Data.Common;

namespace Malte2.Database
{
    public static class DateTimeDatabaseUtils
    {
        public static DateTime GetDateFromReader(DbDataReader reader, int columnNumber)
        {
            string dateString = reader.GetString(columnNumber);
            return DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static DateTime? GetNullableDateFromReader(DbDataReader reader, int columnNumber)
        {
            if (reader.IsDBNull(columnNumber)) {
                return null;
            }
            return GetDateFromReader(reader, columnNumber);
        }

        public static string GetStringFromDate(DateTime date)
        {
            return date.ToString("s");
        }

        public static string? GetStringFromNullableDate(DateTime? date)
        {
            if (date.HasValue) {
                return GetStringFromDate(date.Value);
            }
            return null;
        }
    }
}
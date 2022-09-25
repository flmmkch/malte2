using System.Data.SQLite;
using System.Data.Common;

namespace Malte2.Database
{
    public static class DatabaseValueUtils
    {
        public static string? GetNullableStringFromReader(DbDataReader reader, int columnNumber)
        {
            if (reader.IsDBNull(columnNumber)) {
                return null;
            }
            return reader.GetString(columnNumber);
        }

        public static long? GetNullableInt64FromReader(DbDataReader reader, int columnNumber)
        {
            if (reader.IsDBNull(columnNumber)) {
                return null;
            }
            return reader.GetInt64(columnNumber);
        }

        public static ulong? GetNullableUint64FromReader(DbDataReader reader, int columnNumber)
        {
            if (reader.IsDBNull(columnNumber)) {
                return null;
            }
            return reader.GetFieldValue<ulong>(columnNumber);
        }
    }
}
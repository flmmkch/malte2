using System.Reflection;

namespace Malte2.Model.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class SqlColumnAttribute : System.Attribute
    {  
        public SqlColumnAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }

        public string ColumnName { get; private set; }

        public static string? GetColumnName(Assembly assembly) {
            SqlColumnAttribute? attr = (SqlColumnAttribute?) Attribute.GetCustomAttribute(assembly, typeof (SqlColumnAttribute));
            if (attr != null) {
                return attr.ColumnName;
            }
            else {
                return null;
            }
        }

        public static string? GetColumnName(MemberInfo memberInfo) {
            SqlColumnAttribute? attr = (SqlColumnAttribute?) memberInfo.GetCustomAttribute(typeof (SqlColumnAttribute));
            if (attr != null) {
                return attr.ColumnName;
            }
            else {
                return null;
            }
        }
    }
}
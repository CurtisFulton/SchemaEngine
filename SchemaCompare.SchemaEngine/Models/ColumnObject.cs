using SchemaCompare.SchemaEngine.Extensions;
using SchemaCompare.SchemaEngine.Schema;
using System.Text;

namespace SchemaCompare.SchemaEngine
{
    public class ColumnObject : IDatabaseObject
    {
        public string TableName { get; set; }
        public string TableSchema { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string ColumnDefault { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public int ColumnPosition { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public int MaxCharacters { get; set; }
        public int DatetimePrecision { get; set; }
        public string IndexName { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }

        public ObjectType Type => ObjectType.Column;
        public string FullyQualifiedName => $"[{this.TableSchema}].[{this.ColumnName}]";
        
        public bool Equals(IDatabaseObject obj, Options options)
        {
            if (obj is ColumnObject otherCol) {
                if (this.TableSchema != otherCol.TableSchema)
                    return false;

                if (this.ColumnName != otherCol.ColumnName)
                    return false;
                
                if (this.IsNullable != otherCol.IsNullable)
                    return false;

                if (this.DataType != otherCol.DataType)
                    return false;

                if (this.MaxCharacters != otherCol.MaxCharacters)
                    return false;

                if (this.DatetimePrecision != otherCol.DatetimePrecision)
                    return false;

                if (this.NumericPrecision != otherCol.NumericPrecision)
                    return false;

                if (this.NumericScale != otherCol.NumericScale)
                    return false;

                return true;
            }

            return false;
        }

        public string GetColumnDefinition()
        {
            var sb = new StringBuilder();

            sb.Append("[" + this.ColumnName + "]");
            sb.Append(" [" + this.DataType + "]");

            // Add precision for different datatypes
            if (this.DataType.Contains("char")) {
                // If max characters is -1, it was set as CHAR(MAX)
                if (this.MaxCharacters == -1) {
                    sb.Append("(MAX)");
                } else {
                    sb.Append("(");
                    sb.Append(this.MaxCharacters);
                    sb.Append(")");
                }
            } else if (this.DataType == "datetimeoffset") {
                sb.Append("(");
                sb.Append(this.DatetimePrecision);
                sb.Append(")");
            } else if (this.DataType == "numeric" || this.DataType == "decimal") {
                sb.Append("(");
                sb.Append(this.NumericPrecision);
                sb.Append(", ");
                sb.Append(this.NumericScale);
                sb.Append(")");
            }

            // Check for column Identity
            if (this.IsIdentity) {
                sb.Append(" IDENTITY(1,1)");
            }

            // Check for Nullable/Not null
            if (this.IsNullable) {
                sb.Append(" NULL");
            } else {
                sb.Append(" NOT NULL");
            }

            // Check for a default value
            if (this.ColumnDefault.HasValue()) {
                sb.Append(" DEFAULT");
                sb.Append(this.DatetimePrecision);
            }

            return sb.ToString();
        }

        public override string ToString() => this.ColumnName;
    }
}
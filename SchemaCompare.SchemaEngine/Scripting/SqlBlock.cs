using System;
using System.Text;
using SchemaCompare.SchemaEngine.Schema;

namespace SchemaCompare.SchemaEngine.Scripting
{
    public class SqlBlock : IScriptBlock
    {
        public BlockType Type { get; private set; }

        public string ObjectName { get; private set; }
        public ObjectType ObjectType { get; private set; }

        private StringBuilder StringBuilder { get; set; } = new StringBuilder();

        public SqlBlock(BlockType type, string objectName, ObjectType objectType)
        {
            this.Type = type;
            this.ObjectName = objectName;
            this.ObjectType = objectType;
        }

        public void Append(string value) => this.StringBuilder.Append(value);
        public void AppendLine(string value) => this.StringBuilder.AppendLine(value);

        public override string ToString() => this.StringBuilder.ToString();
    }
}
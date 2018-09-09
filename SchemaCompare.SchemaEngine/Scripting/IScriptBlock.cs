using SchemaCompare.SchemaEngine.Schema;

namespace SchemaCompare.SchemaEngine.Scripting
{
    public interface IScriptBlock
    {
        BlockType Type { get; }

        string ObjectName { get; }
        ObjectType ObjectType { get; }


        void Append(string value);
        void AppendLine(string value);
    }
}
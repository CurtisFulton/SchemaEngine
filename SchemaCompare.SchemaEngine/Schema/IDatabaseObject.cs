using System.Collections.Generic;
using System.Data;

namespace SchemaCompare.SchemaEngine.Schema
{
    public interface IDatabaseObject
    {
        ObjectType Type { get; }
        string FullyQualifiedName { get; }

        bool Equals(IDatabaseObject obj, Options options);
    }
}
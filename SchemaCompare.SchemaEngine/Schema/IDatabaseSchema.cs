using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Schema
{
    public interface IDatabaseSchema 
    {
        List<IDatabaseObject> this[ObjectType type] { get; set; }
    }
}
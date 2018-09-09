using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Schema
{
    public interface IDatabase
    {
        void Register(string connectionString);

        List<IDatabaseObject> this[ObjectType type] { get; set; }
    }
}
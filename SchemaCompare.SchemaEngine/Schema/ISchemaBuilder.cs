namespace SchemaCompare.SchemaEngine.Schema
{
    public interface ISchemaBuilder
    {
        IDatabaseSchema GetSchema(string connectionString);
    }
}

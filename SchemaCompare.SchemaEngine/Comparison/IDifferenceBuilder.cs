using SchemaCompare.SchemaEngine.Schema;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public interface IDifferenceBuilder
    {
        IDifferences BuildDifferences(IDatabase databaseA, IDatabase databaseB, Options options);
    }
}

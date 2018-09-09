using SchemaCompare.SchemaEngine.Schema;
using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public interface IDatabaseObjectComparer
    {
        List<IDifference> GetDifferences(List<IDatabaseObject> itemsA, List<IDatabaseObject> itemsB, Options options);
    }
}
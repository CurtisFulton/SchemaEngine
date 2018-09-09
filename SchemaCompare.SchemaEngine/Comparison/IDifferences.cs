using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public interface IDifferences : IEnumerable<IDifference>
    {
        int Count { get; }
    }
}

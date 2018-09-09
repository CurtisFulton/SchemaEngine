using System.Collections.Generic;
using System.Linq;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public class Differences : List<IDifference>, IDifferences
    {
        new public void AddRange(IEnumerable<IDifference> differences)
        {
            if (differences == null || differences.FirstOrDefault() == null)
                return;

            base.AddRange(differences);
        }
    }
}
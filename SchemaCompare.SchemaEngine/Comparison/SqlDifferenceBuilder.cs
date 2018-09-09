using SchemaCompare.SchemaEngine.Comparison;
using SchemaCompare.SchemaEngine.Schema;
using System;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public class SqlDifferenceBuilder : IDifferenceBuilder
    {
        private IDatabaseObjectComparer SimpleComparer { get; set; } = new SimpleComparer();

        public IDifferences BuildDifferences(IDatabase databaseA, IDatabase databaseB, Options options)
        {
            var differences = new Differences();

            differences.AddRange(this.SimpleComparer.GetDifferences(databaseA[ObjectType.Table], databaseB[ObjectType.Table], options));
            differences.AddRange(this.SimpleComparer.GetDifferences(databaseA[ObjectType.View], databaseB[ObjectType.View], options));
            differences.AddRange(this.SimpleComparer.GetDifferences(databaseA[ObjectType.Procedure], databaseB[ObjectType.Procedure], options));

            return differences;
        }
    }
}
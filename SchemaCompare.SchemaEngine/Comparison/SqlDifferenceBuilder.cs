using SchemaCompare.SchemaEngine.Comparison;
using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public class SqlDifferenceBuilder : IDifferenceBuilder
    {
        public IDifferences BuildDifferences(IDatabase databaseA, IDatabase databaseB, Options options)
        {
            var differences = new Differences();

            differences.AddRange(this.GetDifferences(databaseA[ObjectType.Table],           databaseB[ObjectType.Table],            options));
            differences.AddRange(this.GetDifferences(databaseA[ObjectType.View],            databaseB[ObjectType.View],             options));
            differences.AddRange(this.GetDifferences(databaseA[ObjectType.Procedure],       databaseB[ObjectType.Procedure],        options));
            differences.AddRange(this.GetDifferences(databaseA[ObjectType.TableFunction],   databaseB[ObjectType.TableFunction],    options));
            differences.AddRange(this.GetDifferences(databaseA[ObjectType.ScalarFunction],  databaseB[ObjectType.ScalarFunction],   options));

            return differences;
        }

        private List<IDifference> GetDifferences(List<IDatabaseObject> itemsA, List<IDatabaseObject> itemsB, Options options)
        {
            if (itemsA == null)
                return null;

            if (itemsB == null)
                itemsB = new List<IDatabaseObject>();

            List<IDifference> differences = new List<IDifference>();
            // Copy of Items B's objects so we can remove the objects as they match
            List<IScriptableObject> itemsBCompare = itemsB.Cast<IScriptableObject>().ToList();

            // Loop over every object in A and compare it to every object in B
            foreach (IScriptableObject itemA in itemsA) {
                var differenceType = DifferenceType.Equal;
                IScriptableObject equivalentObject = null;

                int loopLength = itemsBCompare.Count;
                for (int i = 0; i < loopLength; i++) {
                    IScriptableObject itemB = itemsBCompare[i];
                    
                    // Check the names. If they don't have the same name they are not the same object
                    if (itemA.FullyQualifiedName == itemB.FullyQualifiedName) {
                        equivalentObject = itemB;
                        itemsBCompare[i] = itemsBCompare.Last();
                        itemsBCompare.RemoveAt(loopLength - 1);
                        break;
                    }
                }

                if (equivalentObject == null) {
                    // If no equivalent object was found, this ony exists in A
                    differenceType = DifferenceType.OnlyInA;
                } else {
                    // Check if objects are actually equal with a deeper check
                    if (!itemA.Equals(equivalentObject, options)) {
                        differenceType = DifferenceType.Different;
                    }
                }

                // Add the difference to the differences list
                var difference = new Difference(differenceType, itemA.Type, itemA.FullyQualifiedName);
                difference.DatabaseObjectA = itemA;
                difference.DatabaseObjectB = equivalentObject;

                differences.Add(difference);
            }
            
            // Any leftover items in itemsBCompare are object OnlyInB
            foreach (var item in itemsBCompare) {
                var difference = new Difference(DifferenceType.OnlyInB, item.Type, item.FullyQualifiedName);
                difference.DatabaseObjectB = item as IScriptableObject;

                differences.Add(difference);
            }

            return differences;
        }
    }
}
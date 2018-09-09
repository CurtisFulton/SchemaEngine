using SchemaCompare.SchemaEngine.Comparison;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaCompare.SchemaEngine.Scripting
{
    public class SqlScriptBuilder : IScriptBuilder
    {
        public List<IScriptBlock> GenerateScript(IDifferences differences, ScriptDirection direction, Options options)
        {
            var blocks = new List<IScriptBlock>(differences.Count);

            foreach (var difference in differences) {
                if (!difference.UseInGeneration) continue;

                IScriptableObject sourceObj = difference.DatabaseObjectA;
                IScriptableObject targetObj = difference.DatabaseObjectB;
                
                DifferenceType differenceType = difference.Type;

                // If the direction FromBToA, flip it around
                if (direction == ScriptDirection.FromBToA) {
                    sourceObj = difference.DatabaseObjectB;
                    targetObj = difference.DatabaseObjectA;

                    // Swap the 'OnlyIn' type if it is one of them
                    if (differenceType == DifferenceType.OnlyInA)
                        differenceType = DifferenceType.OnlyInB;
                    else if (differenceType == DifferenceType.OnlyInB)
                        differenceType = DifferenceType.OnlyInA;
                }

                // Add the block/s for this difference
                switch (differenceType) {
                    case DifferenceType.Different:
                        blocks.AddRange(sourceObj.AlterTo(targetObj, options));
                        break;
                    case DifferenceType.OnlyInA:
                        blocks.Add(sourceObj.DropBlock(options));
                        break;
                    case DifferenceType.OnlyInB:
                        blocks.Add(targetObj.CreateBlock(options));
                        break;
                }
            }

            // Filter out any null values before returning
            blocks = blocks.Where(block => block != null).ToList();

            return blocks;
        }
    }
}
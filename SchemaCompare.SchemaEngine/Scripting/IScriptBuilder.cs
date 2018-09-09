using SchemaCompare.SchemaEngine.Comparison;
using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Scripting
{
    public interface IScriptBuilder
    {
        List<IScriptBlock> GenerateScript(IDifferences differences, ScriptDirection direction, Options options);
    }
}

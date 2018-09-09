using SchemaCompare.SchemaEngine.Schema;
using System;
using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Scripting
{
    public interface IScriptableObject : IDatabaseObject
    {
        IEnumerable<IScriptBlock> AlterTo(IDatabaseObject obj, Options options);
        IScriptBlock CreateBlock(Options options);
        IScriptBlock DropBlock(Options options);
    }
}
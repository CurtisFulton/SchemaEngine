using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public interface IDifference
    {
        bool UseInGeneration { get; set; }

        DifferenceType Type { get;  }

        ObjectType ObjectType { get;  }
        string ObjectName { get; }

        IScriptableObject DatabaseObjectA { get; }
        IScriptableObject DatabaseObjectB { get; }
    }
}

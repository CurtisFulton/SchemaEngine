using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;

namespace SchemaCompare.SchemaEngine.Comparison
{
    public class Difference : IDifference
    {
        public bool UseInGeneration { get; set; }

        public DifferenceType Type { get; private set; }

        public ObjectType ObjectType { get; private set; }
        public string ObjectName { get; private set; }

        public IScriptableObject DatabaseObjectA { get; internal set; }
        public IScriptableObject DatabaseObjectB { get; internal set; }

        public Difference(DifferenceType type, ObjectType objectType, string objectName)
        {
            this.Type = type;
            this.ObjectType = objectType;
            this.ObjectName = objectName;
        }
    }
}
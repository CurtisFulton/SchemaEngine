using SchemaCompare.SchemaEngine.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Schema
{
    public class SqlSchema : IDatabaseSchema, IEnumerable<KeyValuePair<ObjectType, List<IDatabaseObject>>>
    {
        private Dictionary<ObjectType, List<IDatabaseObject>> Schema { get; set; } = new Dictionary<ObjectType, List<IDatabaseObject>>();

        public List<IDatabaseObject> this[ObjectType type] 
        {
            get {
                if (this.Schema.TryGetValue(type, out List<IDatabaseObject> val))
                    return val;
                return null;
            }
            set {
                if (value.IsEmpty()) {
                    this.Schema.Remove(type);
                } else {
                    this.Schema[type] = value;
                }
            }
        }

        public IEnumerator<KeyValuePair<ObjectType, List<IDatabaseObject>>> GetEnumerator() => this.Schema.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.Schema.GetEnumerator();
    }
}
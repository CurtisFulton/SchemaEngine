using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SchemaCompare.SchemaEngine
{
    public class FunctionObject : IDatabaseObject, IScriptableObject
    {
        public string Catalog { get; set; }
        public string SchemaName { get; set; }
        public string FunctionName { get; set; }

        private string functionDefinition;
        public string FunctionDefinition {
            get { return this.functionDefinition; }
            set { this.functionDefinition = value.Trim(); }
        }

        public ObjectType Type { get; set; } = ObjectType.TableFunction;
        public string FullyQualifiedName => $"[{this.SchemaName}].[{this.FunctionName}]";

        private string TypeString => this.Type == ObjectType.TableFunction ? "TF" : "FN";

        public IEnumerable<IScriptBlock> AlterTo(IDatabaseObject obj, Options options)
        {
            return new List<IScriptBlock>() { this.CreateFunction() };
        }

        public IScriptBlock CreateBlock(Options options)
        {
            return this.CreateFunction();
        }

        public IScriptBlock DropBlock(Options options)
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.FunctionName, this.Type);
            
            block.AppendLine("IF OBJECT_ID(N'" + this.FunctionName + "', N'" + this.TypeString + " ') IS NOT NULL");
            block.Append("DROP FUNCTION " + this.FullyQualifiedName);

            return block;
        }

        private IScriptBlock CreateFunction()
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.FunctionName, this.Type);

            // If it exists, drop it
            block.AppendLine("IF OBJECT_ID(N'" + this.FunctionName + "', N'" + this.TypeString + "') IS NOT NULL");
            block.AppendLine("    DROP FUNCTION " + this.FullyQualifiedName);
            block.AppendLine("GO");

            block.Append(this.FunctionDefinition);

            return block;
        }

        public bool Equals(IDatabaseObject obj, Options options)
        {
            if (obj is FunctionObject == false)
                return false;

            var otherFunction = obj as FunctionObject;

            if (this.FullyQualifiedName != otherFunction.FullyQualifiedName)
                return false;

            if (!this.FunctionDefinition.Equals(otherFunction.FunctionDefinition))
                return false;

            return true;
        }
    }
}
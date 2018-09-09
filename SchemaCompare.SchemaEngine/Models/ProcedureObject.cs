using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;
using System;
using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine
{
    public class ProcedureObject : IDatabaseObject, IScriptableObject
    {
        public string ProcedureCatalog { get; set; }
        public string SchemaName { get; set; }
        public string ProcedureName { get; set; }

        private string procedureDefinition;
        public string ProcedureDefinition {
            get { return this.procedureDefinition; }
            set { this.procedureDefinition = value.Trim(); }
        }

        public ObjectType Type => ObjectType.Procedure;
        public string FullyQualifiedName => $"[{this.SchemaName}].[{this.ProcedureName}]";

        public IEnumerable<IScriptBlock> AlterTo(IDatabaseObject obj, Options options)
        {
            return new List<IScriptBlock>() { this.CreateProcedure() };
        }

        public IScriptBlock CreateBlock(Options options)
        {
            return this.CreateProcedure();
        }

        public IScriptBlock DropBlock(Options options)
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.ProcedureName, this.Type);

            block.AppendLine("IF OBJECT_ID(N'" + this.ProcedureName + "', N'P') IS NOT NULL");
            block.Append("DROP PROCEDURE " + this.FullyQualifiedName);

            return block;
        }

        private IScriptBlock CreateProcedure()
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.ProcedureName, this.Type);

            // If it exists, drop it
            block.AppendLine("IF OBJECT_ID(N'" + this.ProcedureName + "', N'P') IS NOT NULL");
            block.AppendLine("    DROP PROCEDURE " + this.FullyQualifiedName);
            block.AppendLine("GO");

            block.Append(this.ProcedureDefinition);

            return block;
        }

        public bool Equals(IDatabaseObject obj, Options options)
        {
            if (obj is ProcedureObject == false)
                return false;

            var otherProcedure = obj as ProcedureObject;

            if (this.FullyQualifiedName != otherProcedure.FullyQualifiedName)
                return false;

            if (!this.ProcedureDefinition.Equals(otherProcedure.ProcedureDefinition))
                return false;

            return true;
        }
    }
}
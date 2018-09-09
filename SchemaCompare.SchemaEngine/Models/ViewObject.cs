using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SchemaCompare.SchemaEngine
{
    public class ViewObject : IDatabaseObject, IScriptableObject
    {
        public string ViewName { get; set; }
        public string SchemaName { get; set; }

        private string viewDeifintion;
        public string ViewDefinition {
            get { return this.viewDeifintion; } 
            set { this.viewDeifintion = value.Trim(); }//QueryHelper.ReduceObjectDefintion(value); }
        }
        
        public ObjectType Type => ObjectType.View;
        public string FullyQualifiedName => $"[{this.SchemaName}].[{this.ViewName}]";

        public IEnumerable<IScriptBlock> AlterTo(IDatabaseObject obj, Options options)
        {
            return new List<IScriptBlock>() { this.CreateViewBlock() };
        }

        public IScriptBlock CreateBlock(Options options)
        {
            return this.CreateViewBlock();
        }

        public IScriptBlock DropBlock(Options options)
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.ViewName, this.Type);

            block.AppendLine("IF OBJECT_ID(N'" + this.ViewName + "', N'V') IS NOT NULL");
            block.Append("DROP VIEW " + this.FullyQualifiedName);

            return block;
        }

        private IScriptBlock CreateViewBlock()
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.ViewName, this.Type);

            // If it exists, drop it
            block.AppendLine("IF OBJECT_ID(N'" + this.ViewName + "', N'V') IS NOT NULL");
            block.AppendLine("    DROP VIEW " + this.FullyQualifiedName);
            block.AppendLine("GO");
            
            block.Append(this.ViewDefinition);

            return block;
        }

        public bool Equals(IDatabaseObject obj, Options options)
        {
            if (obj is ViewObject == false)
                return false;

            var otherView = obj as ViewObject;

            if (this.FullyQualifiedName != otherView.FullyQualifiedName)
                return false;
            
            if (!this.ViewDefinition.Equals(otherView.ViewDefinition))
                return false;

            return true;
        }
    }
}
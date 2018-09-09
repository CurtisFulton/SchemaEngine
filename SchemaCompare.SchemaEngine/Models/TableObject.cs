using SchemaCompare.SchemaEngine.Comparison;
using SchemaCompare.SchemaEngine.Extensions;
using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchemaCompare.SchemaEngine
{
    public class TableObject : IDatabaseObject, IScriptableObject
    {
        public string TableCatalog { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }

        public ObjectType Type => ObjectType.Table;
        public string FullyQualifiedName => $"[{this.TableSchema}].[{this.TableName}]";

        internal List<ColumnObject> Columns { get; set; }
        // TODO Indexes, Constraints, etc
        
        public IEnumerable<IScriptBlock> AlterTo(IDatabaseObject obj, Options options)
        {
            if (obj is TableObject == false) throw new InvalidOperationException("Cannot alter table to an object that is not a table");

            var blocks = new List<IScriptBlock>();
            var otherTable = obj as TableObject;

            List<ColumnObject> modifiedColumns = new List<ColumnObject>();
            List<ColumnObject> additionalColumns = new List<ColumnObject>();

            // Start off assuming all columns will be removed, and we take items out of the list as we compare
            List<ColumnObject> removedColumns = new List<ColumnObject>(this.Columns);

            // Find all the modified and additional columns
            foreach (ColumnObject columnA in otherTable.Columns) {
                ColumnObject equivalentColumn = null;

                int loopLength = removedColumns.Count;
                for (int i = 0; i < loopLength; i++) {
                    ColumnObject columnB = removedColumns[i];

                    // If a column with the same name exists, then the column is not being removed anymore 
                    // and we need to do a deeper check on the columns to see if they are equal
                    if (columnA.FullyQualifiedName == columnB.FullyQualifiedName) {
                        equivalentColumn = columnB;
                        removedColumns[i] = removedColumns.Last();
                        removedColumns.RemoveAt(loopLength - 1);
                        break;
                    }
                }

                // If there was no equivalent object, it is a new column so end here
                if (equivalentColumn == null) {
                    additionalColumns.Add(columnA);
                    continue;
                }

                // If the columns have the same name but are not complete equal, it has been modified
                if (!columnA.Equals(equivalentColumn, options)) {
                    modifiedColumns.Add(columnA);
                } 
            }

            // Drop columns that should no longer be on the table
            if (removedColumns.HasItems()) {
                var block = new SqlBlock(BlockType.Alter, this.TableName, ObjectType.Column);
                
                foreach (var column in removedColumns) {
                    block.Append("ALTER TABLE ");
                    block.Append(this.FullyQualifiedName);
                    block.Append(" DROP COLUMN ");
                    block.AppendLine(column.ColumnName);
                    block.AppendLine("GO");
                }

                blocks.Add(block);
            }

            // Add new columns
            if (additionalColumns.HasItems()) {
                var block = new SqlBlock(BlockType.Alter, this.TableName, ObjectType.Column);

                foreach (var column in additionalColumns) {
                    block.Append("ALTER TABLE ");
                    block.Append(this.FullyQualifiedName);
                    block.Append(" ADD ");
                    block.AppendLine(column.GetColumnDefinition());
                    block.AppendLine("GO");
                }

                blocks.Add(block);
            }

            // Modify existing columns
            if (modifiedColumns.HasItems()) {
                var block = new SqlBlock(BlockType.Alter, this.TableName, ObjectType.Column);

                foreach (var column in modifiedColumns) {
                    block.Append("ALTER TABLE ");
                    block.Append(this.FullyQualifiedName);
                    block.Append(" ALTER COLUMN ");
                    block.AppendLine(column.GetColumnDefinition());
                    block.AppendLine("GO");
                }

                blocks.Add(block);
            }

            return blocks;
        }

        public IScriptBlock CreateBlock(Options options)
        {
            SqlBlock block = new SqlBlock(BlockType.Create, this.TableName, this.Type);

            // Header
            block.AppendLine($"IF OBJECT_ID(N'{this.TableName}', N'U') IS NULL");
            block.AppendLine($"  CREATE TABLE [{this.TableName}] (");

            // Add each column in the table
            foreach (ColumnObject column in this.Columns) {
                block.Append("    ");
                block.Append(column.GetColumnDefinition());
                block.AppendLine(",");
            }

            // Add the PK constraint
            ColumnObject[] pkColumns = this.Columns.Where(col => col.IsPrimaryKey).ToArray();
            if (pkColumns.HasValues()) {
                block.Append("    Constraint [" + pkColumns.First().IndexName + "] PRIMARY KEY NONCLUSTERED (");

                block.AppendLine(String.Join(", ", pkColumns.Select(col => col.ColumnName)) + ")");
            }

            // TODO: Add constraints

            block.Append("  );");

            return block;
        }

        public IScriptBlock DropBlock(Options options)
        {
            SqlBlock block = new SqlBlock(BlockType.Drop, this.TableName, this.Type);

            block.AppendLine("IF OBJECT_ID(N'" + this.TableName + "', N'U') IS NOT NULL");
            block.Append("DROP TABLE " + this.FullyQualifiedName);

            return block;
        }

        public bool Equals(IDatabaseObject obj, Options options)
        {
            // If the object isn't a table object, it won't be equal
            if (obj is TableObject == false) return false;

            var tblObj = obj as TableObject;
            
            // Check all the columns are the same
            if (!ColumnsEqual(this.Columns, tblObj.Columns, options))
                return false;

            return true; ;
        }

        #region Helper Methods
        
        private bool ColumnsEqual(List<ColumnObject> columnsA, List<ColumnObject> columnsB, Options options)
        {
            if (columnsA.Count != columnsB.Count)
                return false;

            var columnsBCopy = new List<ColumnObject>(columnsB);
            
            foreach (ColumnObject columnA in columnsA) {
                ColumnObject equivalentColumn = null;

                int loopLength = columnsBCopy.Count;
                for (int i = 0; i < loopLength; i++) {
                    ColumnObject columnB = columnsBCopy[i];

                    // If a column with the same name exists, then the column is not being removed anymore 
                    // and we need to do a deeper check on the columns to see if they are equal
                    if (columnA.FullyQualifiedName == columnB.FullyQualifiedName) {
                        equivalentColumn = columnB;
                        columnsBCopy[i] = columnsBCopy.Last();
                        columnsBCopy.RemoveAt(loopLength - 1);
                        break;
                    }
                }

                // If there was no equivalent object, it is a new column so end here
                if (equivalentColumn == null) {
                    return false;
                }

                // If the columns have the same name but are not complete equal, it has been modified
                if (!columnA.Equals(equivalentColumn, options)) {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
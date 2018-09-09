using Dapper;
using SchemaCompare.SchemaEngine.Extensions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SchemaCompare.SchemaEngine.Schema
{
    public class SqlSchemaBuilder : ISchemaBuilder
    {
        private IDatabase Database { get; set; }

        public SqlSchemaBuilder(IDatabase database)
        {
            this.Database = database;
        }

        public IDatabaseSchema GetSchema(string connectionString) => this.GetSchema(connectionString, Options.Default);
        public IDatabaseSchema GetSchema(string connectionString, Options options)
        {
            var schema = new SqlSchema {
                [ObjectType.Table] = GetUserTables(connectionString),
                [ObjectType.View] = GetViews(connectionString),
                [ObjectType.Procedure] = GetProcedures(connectionString),
                [ObjectType.TableFunction] = GetTableFunctions(connectionString),
                [ObjectType.ScalarFunction] = GetScalarFunctions(connectionString),
            };

            return schema;
        }

        public async Task<IDatabaseSchema> GetSchemaAsync(string connectionString) => await this.GetSchemaAsync(connectionString, Options.Default);
        public async Task<IDatabaseSchema> GetSchemaAsync(string connectionString, Options options)
        {
            // Start loading all the different data sets without awaiting any of them
            var tableTask = Task.Run(() => GetUserTables(connectionString));
            var viewTask = Task.Run(() => GetViews(connectionString));
            var procTask = Task.Run(() => GetProcedures(connectionString));
            var tableFuncTask = Task.Run(() => GetTableFunctions(connectionString));
            var scalarFuncTask = Task.Run(() => GetScalarFunctions(connectionString));
            
            var schema = new SqlSchema {
                [ObjectType.Table] = await tableTask,
                [ObjectType.View] = await viewTask,
                [ObjectType.Procedure] = await procTask,
                [ObjectType.TableFunction] = await tableFuncTask,
                [ObjectType.ScalarFunction] = await scalarFuncTask,
            };
            
            return schema;
        }

        private List<IDatabaseObject> GetUserTables(string connectionString)
        {
            var baseTables = new List<IDatabaseObject>();

            IEnumerable<TableObject> allTables = null;
            IEnumerable<ColumnObject> allColumns = null;

            // Get all Tables, Columns, TODO: and Indexes
            using (var con = new SqlConnection(connectionString)) {
                allTables = con.Query<TableObject>(QueryHelper.TableQuery, new { Catalog = con.Database });
                allColumns = con.Query<ColumnObject>(QueryHelper.ColumnQuery);
            }

            // Add the data for each table
            foreach (TableObject table in allTables) {
                string objectName = $"[{table.TableSchema}].[{table.TableName}]";

                // Find any columns that have the same table name as the current table
                List<ColumnObject> tableColumns = allColumns.Where(col => col.TableName == table.TableName).ToList();
                
                if (tableColumns.Count == 0) continue;
                
                // Add the columns and indexes to the table object
                table.Columns = tableColumns;

                // Add this table
                baseTables.Add(table);
            }

            return baseTables;
        }

        private List<IDatabaseObject> GetViews(string connectionString)
        {
            List<ViewObject> allViews = null;

            // Get all the views
            using (var con = new SqlConnection(connectionString)) {
                allViews = con.Query<ViewObject>(QueryHelper.ViewQuery, new { Catalog = con.Database }).ToList();
            }
            
            // Cast the views list to be IDatabaseObjects and return
            return allViews.Cast<IDatabaseObject>().ToList();
        }

        private List<IDatabaseObject> GetProcedures(string connectionString)
        {
            List<ProcedureObject> allProcedures = null;

            // Get all the procedures
            using (var con = new SqlConnection(connectionString)) {
                allProcedures = con.Query<ProcedureObject>(QueryHelper.ProcedureQuery, new { Catalog = con.Database }).ToList();
            }

            // Cast the procedures list to be IDatabaseObjects and return
            return allProcedures.Cast<IDatabaseObject>().ToList();
        }

        private List<IDatabaseObject> GetTableFunctions(string connectionString)
        {
            List<FunctionObject> allTableFunctions = null;

            // Get all the procedures
            using (var con = new SqlConnection(connectionString)) {
                allTableFunctions = con.Query<FunctionObject>(QueryHelper.TableFunctionQuery, new { Catalog = con.Database }).ToList();
            }

            // Cast the procedures list to be IDatabaseObjects and return
            return allTableFunctions.Cast<IDatabaseObject>().ToList();
        }

        private List<IDatabaseObject> GetScalarFunctions(string connectionString)
        {
            List<FunctionObject> allScalarFunctions = null;

            // Get all the procedures
            using (var con = new SqlConnection(connectionString)) {
                allScalarFunctions = con.Query<FunctionObject>(QueryHelper.ScalarFunctionQuery, new { Catalog = con.Database }).ToList();
            }

            // Update all the func types to be Scalar
            allScalarFunctions.ForEach(func => func.Type = ObjectType.ScalarFunction);

            // Cast the procedures list to be IDatabaseObjects and return
            return allScalarFunctions.Cast<IDatabaseObject>().ToList();
        }
    }
}
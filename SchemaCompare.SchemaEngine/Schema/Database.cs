using SchemaCompare.SchemaEngine.Comparison;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SchemaCompare.SchemaEngine.Schema
{
    public class Database : IDatabase
    {
        private IDatabaseSchema Schema { get; set; }
        
        private string ServerName { get; set; }
        private string DatabaseName { get; set; }

        public IDifferences CompareWith(Database database) => this.CompareWith(database, Options.Default);
        public IDifferences CompareWith(Database database, Options options)
        {
            var differenceBuilder = new SqlDifferenceBuilder();

            return differenceBuilder.BuildDifferences(this, database, options);
        }

        public void Register(string connectionString) => this.Register(connectionString, Options.Default);
        public void Register(string connectionString, Options options)
        {
            this.SetupDatabaseFromConnectionString(connectionString);

            var schemaBuilder = new SqlSchemaBuilder(this);
            this.Schema = schemaBuilder.GetSchema(connectionString);
        }

        public async Task RegisterAsync(string connectionString, Options options)
        {
            this.SetupDatabaseFromConnectionString(connectionString);

            var schemaBuilder = new SqlSchemaBuilder(this);
            this.Schema = await schemaBuilder.GetSchemaAsync(connectionString, options);
        }

        private void SetupDatabaseFromConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            this.ServerName = builder.DataSource;
            this.DatabaseName = builder.InitialCatalog;
        }

        public List<IDatabaseObject> this[ObjectType type] {
            get => this.Schema[type];
            set => this.Schema[type] = value;
        }
    }
}
using SchemaCompare.SchemaEngine;
using SchemaCompare.SchemaEngine.Schema;
using SchemaCompare.SchemaEngine.Scripting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SchemaCompare.App
{
    public class Program
    {
        private const string ConnectionStringA = @"Data Source=localhost\SQLEXPRESS2008;Initial Catalog=MEXDB;Integrated Security=False;User ID=sa;Password=Admin123;MultipleActiveResultSets=True";
        private const string ConnectionStringB = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=OboeteDB;Integrated Security=False;User ID=sa;Password=Admin123;MultipleActiveResultSets=True";
        private const string OutputDir = @"D:\Programming\C#\SchemaCompare\Output.txt";

        static void Main(string[] args)
        {
            TestSchemaCompare().Wait();
        }

        private static async Task TestSchemaCompare()
        {
            Stopwatch sw = new Stopwatch();

            var dbA = new Database();
            var dbB = new Database();

            sw.Start();

            int numLoops = 1;
            for (int i = 0; i < numLoops; i++) {
                await RegisterDatabasesAsync(dbA, dbB);
                //RegisterDatabases(dbA, dbB);
            }
            
            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds / numLoops}ms to register DB's");
            sw.Restart();

            var differences = dbA.CompareWith(dbB);

            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to compare DB's");
            
            var scriptBuilder = new SqlScriptBuilder();

            sw.Restart();
            var scriptBlocks = scriptBuilder.GenerateScript(differences, ScriptDirection.FromBToA, Options.Default);

            StringBuilder script = new StringBuilder();

            foreach (var block in scriptBlocks) {
                var sqlString = block.ToString().TrimEnd();
                script.AppendLine(sqlString);

                if (!sqlString.EndsWith("GO"))
                    script.AppendLine("GO");

                script.AppendLine();
            }

            sw.Stop();
            Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to build update script");

            File.WriteAllText(OutputDir, script.ToString());

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static async Task RegisterDatabasesAsync(Database dbA, Database dbB)
        {
            var dbATask = dbA.RegisterAsync(ConnectionStringA, Options.Default);
            var dbBTask = dbB.RegisterAsync(ConnectionStringB, Options.Default);

            await Task.WhenAll(dbATask, dbBTask);
        }

        private static void RegisterDatabases(Database dbA, Database dbB)
        {
            dbA.Register(ConnectionStringA, Options.Default);
            dbB.Register(ConnectionStringB, Options.Default);
        }
    }
}

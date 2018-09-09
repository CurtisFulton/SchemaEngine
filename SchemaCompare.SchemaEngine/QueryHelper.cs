using SchemaCompare.SchemaEngine.Extensions;
using System;
using System.Text.RegularExpressions;

namespace SchemaCompare.SchemaEngine
{
    public static class QueryHelper
    {
        public const string TableQuery = @"
            SELECT 
	            TABLE_CATALOG TableCatalog,
	            TABLE_SCHEMA TableSchema,
	            TABLE_NAME TableName 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG=@Catalog
        ";

        public const string ColumnQuery = @"
            with ColumnData AS 
            (
                SELECT 
	                t.NAME TableName,
		            isc.TABLE_SCHEMA TableSchema,
	                col.NAME ColumnName,
	                isc.DATA_TYPE DataType,
	                isc.COLUMN_DEFAULT DefaultValue,
	                CASE
			            WHEN isc.IS_NULLABLE = 'NO' THEN 0
			            ELSE 1
		            END IsNullable,
	                col.is_identity IsIdentity,
	                isc.ORDINAL_POSITION ColumnPosition,
	                ind.is_primary_key IsPrimaryKey,
	                ind.is_unique IsUnique,
	                isc.CHARACTER_MAXIMUM_LENGTH MaxCharacters,
	                isc.DATETIME_PRECISION DateTimePrecision,
                    ind.name IndexName,
		            isc.NUMERIC_PRECISION NumericPrecision,
		            isc.NUMERIC_SCALE NumericScale,
	                ROW_NUMBER() OVER (PARTITION BY t.name, col.NAME ORDER BY t.NAME, isc.ORDINAL_POSITION, ind.is_primary_key desc, col.is_identity desc) as rn
                FROM SYS.COLUMNS col
                INNER JOIN SYS.TABLES t ON t.OBJECT_ID = col.OBJECT_ID
                INNER JOIN INFORMATION_SCHEMA.COLUMNS isc ON  isc.COLUMN_NAME = col.NAME AND isc.TABLE_NAME = t.name
                LEFT OUTER JOIN SYS.INDEX_COLUMNS ic ON ic.OBJECT_ID = col.OBJECT_ID AND ic.column_id = col.column_id
                LEFT OUTER JOIN SYS.INDEXES ind ON ind.OBJECT_ID = ic.OBJECT_ID AND ind.index_id = ic.index_id 
                WHERE t.TYPE = N'U'
            )
            Select *
            From ColumnData 
            where rn = 1 
            ORDER BY ColumnData.TableName, ColumnData.ColumnPosition, ColumnData.IsPrimaryKey desc, ColumnData.IsIdentity desc
        ";

        public const string ViewQuery = @"
            SELECT 
	            vw.TABLE_CATALOG [ViewCatalog],
	            vw.TABLE_NAME [ViewName], 
	            vw.TABLE_SCHEMA [SchemaName], 
	            OBJECT_DEFINITION(obj.OBJECT_ID) [ViewDefinition] 
            FROM sys.objects obj
            INNER JOIN INFORMATION_SCHEMA.VIEWS vw on vw.TABLE_NAME = obj.name
            where type='V' and vw.TABLE_CATALOG = @Catalog
        ";

        // Matches with the 'CREATE/ALTER <NAME> AS' part of an object definition
        private static readonly Regex ViewRegex = new Regex(@"^.*?(?:CREATE|ALTER).*?AS\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Removes the 'CREATE/ALTER <NAME> AS' at the start of an object definition.
        /// </summary>
        /// <param name="objectDefinition">Object definition as returned by the Database</param>
        /// <returns>The true definition for an object</returns>
        public static string ReduceObjectDefintion(string objectDefinition)
        {
            if (objectDefinition.IsEmpty())
                return null;

            var match = ViewRegex.Match(objectDefinition);
            
            if (objectDefinition.Contains("Extension_PurchaseOrder")) {
                var suc = match.Success;
            }

            if (!match.Success)
                return objectDefinition;

            var reducedDefinition = objectDefinition.Replace(match.Value, "");
            return reducedDefinition;
        }
    }
}
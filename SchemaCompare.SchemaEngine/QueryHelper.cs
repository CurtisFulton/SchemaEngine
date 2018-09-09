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

        public const string ProcedureQuery = @"
            SELECT 
	            DB_NAME() as [ProcedureCatalog],
	            schm.name as [SchemaName],
	            OBJECT_NAME(obj.OBJECT_ID) [ProcedureName],
	            OBJECT_DEFINITION(obj.OBJECT_ID) [ProcedureDefinition]
            FROM sys.objects obj
            INNER JOIN sys.schemas schm on schm.schema_id = obj.schema_id
            where obj.type = 'P' and DB_NAME() = @Catalog
        ";
    }
}
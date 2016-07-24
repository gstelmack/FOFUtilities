using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Maddenator
{
	static class TDBAccess
	{
		public enum FieldType { tdbString = 0, tdbBinary = 1, tdbSInt = 2, tdbUInt = 3, tdbFloat = 4, tdbInt = 0x2CE };

		public struct FieldProperties
		{
			public string Name;
			public int Size;
			public FieldType FieldType;
		}

		public struct TableProperties
		{
			public string Name;
			public int FieldCount;
			public int Capacity;
			public int RecordCount;
			public int DeletedCount;
			public int NextDeletedRecord;
			public bool Flag0;
			public bool Flag1;
			public bool Flag2;
			public bool Flag3;
			public bool NonAllocated;
		}

		[DllImport("tdbaccess.dll")]
		public static extern int TDBOpen(string FileName);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBClose(int DBIndex);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBSave(int DBIndex);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBDatabaseCompact(int DBIndex);

		[DllImport("tdbaccess.dll")]
		public static extern int TDBDatabaseGetTableCount(int DBIndex);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBFieldGetProperties(int DBIndex, string TableName, int FieldIndex, ref FieldProperties FieldProperties);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBTableGetProperties(int DBIndex, int TableIndex, ref TableProperties TableProperties);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBFieldGetValueAsBinary(int DBIndex, string TableName, string FieldName, int RecNo, ref string OutBuffer);

		[DllImport("tdbaccess.dll")]
		public static extern float TDBFieldGetValueAsFloat(int DBIndex, string TableName, string FieldName, int RecNo);

		[DllImport("tdbaccess.dll")]
		public static extern int TDBFieldGetValueAsInteger(int DBIndex, string TableName, string FieldName, int RecNo);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBFieldGetValueAsString(int DBIndex, string TableName, string FieldName, int RecNo, ref string OutBuffer);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBFieldSetValueAsFloat(int DBIndex, string TableName, string FieldName, int RecNo, float NewValue);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBFieldSetValueAsInteger(int DBIndex, string TableName, string FieldName, int RecNo, int NewValue);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBFieldSetValueAsString(int DBIndex, string TableName, string FieldName, int RecNo, string NewValue);

		[DllImport("tdbaccess.dll")]
		public static extern int TDBQueryFindUnsignedInt(int DBIndex, string TableName, string FieldName, int Value);

		[DllImport("tdbaccess.dll")]
		public static extern int TDBQueryGetResult(int Index);

		[DllImport("tdbaccess.dll")]
		public static extern int TDBQueryGetResultSize();

		[DllImport("tdbaccess.dll")]
		public static extern int TDBTableRecordAdd(int DBIndex, string TableName, bool AllowExpand);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBTableRecordChangeDeleted(int DBIndex, string TableName, int RecNo, bool Deleted);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBTableRecordDeleted(int DBIndex, string TableName, int RecNo);

		[DllImport("tdbaccess.dll")]
		public static extern bool TDBTableRecordRemove(int DBIndex, string TableName, int RecNo);
	}
}

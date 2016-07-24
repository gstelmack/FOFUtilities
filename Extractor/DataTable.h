#pragma once

#include <vector>

class DataTable
{
public:
	DataTable(void);
	~DataTable(void);

	bool LoadHTML(const char* filePath);
	bool LoadCSV(const char* filePath);

	inline size_t			GetRowCount()const { return mTable.size(); };
	inline size_t			GetColumnCount(size_t row)const;
	inline const CString&	GetCell(size_t row, size_t column)const;

private:

	void ProcessHTMLRow(const CString& rowString);
	void ProcessCSVRow(const CString& rowString);

	typedef std::vector<CString> DataRow;
	typedef std::vector<DataRow> FullTable;

	FullTable	mTable;
};

size_t DataTable::GetColumnCount(size_t row) const
{
	ASSERT( row < GetRowCount() ); 
	return mTable[row].size();
};

const CString& DataTable::GetCell(size_t row, size_t column) const
{
	ASSERT( row < GetRowCount() );
	ASSERT( column < GetColumnCount(row) );
	return mTable[row][column];
}

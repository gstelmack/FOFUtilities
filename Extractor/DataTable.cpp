#include "StdAfx.h"
#include "datatable.h"

#include <fstream>

DataTable::DataTable(void)
{
}

DataTable::~DataTable(void)
{
}

bool DataTable::LoadHTML(const char *filePath)
{
	ASSERT(filePath);

	mTable.clear();
	mTable.reserve(1200);

	std::ifstream htmlFile(filePath);
	if (!htmlFile.good())
	{
		return false;
	}

	static const size_t kLineBufSize = 16*1024;
	char lineBuf[kLineBufSize];
	htmlFile.getline(lineBuf,kLineBufSize);
	CString lineString;
	while (htmlFile.good() && !htmlFile.eof())
	{
		lineString = lineBuf;

		if (lineString.Left(4) == "<tr>")
		{
			ProcessHTMLRow(lineString);
		}

		htmlFile.getline(lineBuf,kLineBufSize);
	}

	return true;
}

void DataTable::ProcessHTMLRow(const CString &rowString)
{
	int curPos = 0;
	int endPos = 0;
	int startPos;
	size_t rowIndex = mTable.size();
	mTable.resize(rowIndex+1);
	CString columnString;
	while (   (curPos = rowString.Find("<td >",curPos)) >= 0
		   && (endPos = rowString.Find("</td>",curPos+5)) >= 0
		   )
	{
		startPos = curPos + 5;
		columnString = rowString.Mid(startPos,endPos-startPos);
		mTable[rowIndex].push_back(columnString);
		curPos = endPos+5;
	}
}

bool DataTable::LoadCSV(const char *filePath)
{
	ASSERT(filePath);

	mTable.clear();
	mTable.reserve(1200);

	std::ifstream csvFile(filePath);
	if (!csvFile.good())
	{
		return false;
	}

	static const size_t kLineBufSize = 16*1024;
	char lineBuf[kLineBufSize];
	csvFile.getline(lineBuf,kLineBufSize);
	CString lineString;
	while (csvFile.good() && !csvFile.eof())
	{
		lineString = lineBuf;
		ProcessCSVRow(lineString);

		csvFile.getline(lineBuf,kLineBufSize);
	}

	return true;
}

void DataTable::ProcessCSVRow(const CString &rowString)
{
	int curPos = 0;
	int endPos;
	int columnStart;
	int columnEnd;
	size_t rowIndex = mTable.size();
	mTable.resize(rowIndex+1);
	CString columnString;
	while ( curPos < rowString.GetLength() )
	{
		// Find the end of the current column. If the column starts with a quote,
		// it's the next quote, otherwise the next comma, otherwise the end of the
		// line.
		if (rowString.GetAt(curPos) == '\"')
		{
			endPos = rowString.Find('\"',curPos+1);
			columnStart = curPos + 1;
			if (endPos < 0)
			{
				// This would be an error in the file.
				endPos = rowString.GetLength();
			}
			curPos = endPos + 2;
		}
		else
		{
			endPos = rowString.Find(',',curPos);
			columnStart = curPos;
			if (endPos < 0)
			{
				endPos = rowString.GetLength();
			}
			curPos = endPos + 1;
		}
		columnEnd = endPos;

		columnString = rowString.Mid(columnStart,columnEnd - columnStart);
		mTable[rowIndex].push_back(columnString);
	}
}

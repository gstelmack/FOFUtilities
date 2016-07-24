using System;
using System.Collections.Generic;
using System.Text;

namespace DataReader
{
	public class CSVHelper
	{
		static public string[] ParseLine(string curLine)
		{
			System.Collections.ArrayList fields = new System.Collections.ArrayList();

			string columnString = "";
			int curPos = 0;
			int endPos;
			int columnStart;
			int columnEnd;
			while (curPos < curLine.Length)
			{
				// Find the end of the current column. If the column starts with a quote,
				// it's the next quote, otherwise the next comma, otherwise the end of the
				// line.
				if (curLine[curPos] == '\"')
				{
					endPos = curLine.IndexOf('\"', curPos + 1);
					columnStart = curPos + 1;
					if (endPos < 0)
					{
						// This would be an error in the file.
						endPos = curLine.Length;
					}
					curPos = endPos + 2;
				}
				else
				{
					endPos = curLine.IndexOf(',', curPos);
					columnStart = curPos;
					if (endPos < 0)
					{
						endPos = curLine.Length;
					}
					curPos = endPos + 1;
				}
				columnEnd = endPos;

				columnString = curLine.Substring(columnStart, columnEnd - columnStart);
				fields.Add(columnString);
			}

			string[] result = new string[fields.Count];
			for (int i = 0; i < fields.Count; i++)
			{
				result[i] = (string)fields[i];
			}
			return result;
		}
	}
}

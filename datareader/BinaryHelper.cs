using System;
using System.Collections.Generic;
using System.Text;

namespace DataReader
{
	class BinaryHelper
	{
#if DEBUG
		static private System.IO.StreamWriter TraceWriter;
		static private string Indent = "";
#endif

		static public void SetupTracer(string filePath)
		{
#if DEBUG
			// Make sure we don't have an active tracer.
			System.Diagnostics.Debug.Assert(TraceWriter == null);

			// Creates the text file that the tracer will write to.
			string filename = System.IO.Path.GetFileName(filePath);
			filename += ".log";
			TraceWriter = new System.IO.StreamWriter(filename, false);
#endif
		}

		static public void ClearTracer()
		{
#if DEBUG
			// Clear out any remaining output.
			if (TraceWriter != null)
			{
				TraceWriter.Flush();
				TraceWriter.Close();
			}

			// Flag us as having properly cleaned out the prior listener.
			TraceWriter = null;
#endif
		}

		static public void TracerIndent()
		{
#if DEBUG
			Indent += "    ";
#endif
		}

		static public void TracerOutdent()
		{
#if DEBUG
			Indent = Indent.Substring(0, Indent.Length - 4);
#endif
		}

		static public void TracerWriteLine(string line)
		{
#if DEBUG
			if (TraceWriter != null)
			{
				TraceWriter.WriteLine(Indent + line);
				//TraceWriter.Flush();
			}
#endif
		}

		static public short Clamp(short inVal, int min, int max)
		{
			if (inVal < min)
			{
				return (short)min;
			}
			else if (inVal > max)
			{
				return (short)max;
			}
			else
			{
				return inVal;
			}
		}

		static public string ExtractString(System.IO.BinaryReader inFile, short length, string header)
		{
			string myString = "";
			for (short i = 0; i < length; i++)
			{
				Char nextChar = inFile.ReadChar();
				if (System.Char.IsLetterOrDigit(nextChar)
					|| System.Char.IsPunctuation(nextChar)
					|| System.Char.IsWhiteSpace(nextChar)
					)
				{
					myString += nextChar;
				}
			}

			TracerWriteLine(header + " = '" + myString + "'");
			return myString;
		}

		static public void WriteString(System.IO.BinaryWriter outFile, string chars)
		{
			for (int i = 0; i < chars.Length; i++)
			{
				outFile.Write(chars[i]);
			}
		}

		static public short ReadInt16(System.IO.BinaryReader inFile, string name)
		{
			short value = inFile.ReadInt16();
			TracerWriteLine(name + " = " + value);
			return value;
		}

		static public int ReadInt32(System.IO.BinaryReader inFile, string name)
		{
			int value = inFile.ReadInt32();
			TracerWriteLine(name + " = " + value);
			return value;
		}

		static public int ReadCodedInt32(System.IO.BinaryReader inFile, string name)
		{
			int val1 = (int)inFile.ReadInt16();
			int val2 = (int)inFile.ReadInt16();
			int value = (val2 * 32768) + val1;
			TracerWriteLine(name + " = " + value);
			return value;
		}

		static private string ProbeChars(byte[] readBytes, int count)
		{
			string result = "";
			System.Text.Encoding windows1252Encoding = System.Text.Encoding.GetEncoding(1252);
			char[] charBuffer = new char[count];
			for (int i = 0; i < count; i++)
			{
				if (readBytes[i] >= 32 && readBytes[i]<128)
				{
					windows1252Encoding.GetChars(readBytes,i,1,charBuffer,i);
				}
				else
				{
					charBuffer[i] = '.';
				}
				result += charBuffer[i];
			}
			return result;
		}

		static private string ProbeShorts(byte[] readBytes, int count)
		{
			string result = "";
			for (int i = 0; i < count; i++)
			{
				int x = readBytes[i * 2] + (readBytes[i * 2 + 1] << 8);
				result += String.Format(" {0,6}",x.ToString());
			}
			return result;
		}

		static private string ProbeLongs(byte[] readBytes, int count)
		{
			string result = "";
			for (int i = 0; i < count; i++)
			{
				int x = readBytes[i * 4] +
					(readBytes[i * 4 + 1] << 8) +
					(readBytes[i * 4 + 2] << 16) +
					(readBytes[i * 4 + 3] << 24);
				result += String.Format(" {0,11}", x.ToString());
			}
			return result;
		}

		static public void ProbeBytes(System.IO.BinaryReader inFile, long numBytes)
		{
			long maxBytes = inFile.BaseStream.Length - inFile.BaseStream.Position;

			TracerWriteLine("Bytes at " + inFile.BaseStream.Position);
			TracerIndent();
			long bytesLeft = System.Math.Min(numBytes,maxBytes);
			byte[] readBytes;
			string writeString;
			while (bytesLeft >= 8)
			{
				readBytes = inFile.ReadBytes(8);
				writeString = "8 bytes: CCCCCCCC ";
				writeString += ProbeChars(readBytes, 8);
				writeString += "   | SSSS ";
				writeString += ProbeShorts(readBytes, 4);
				writeString += "   | LL ";
				writeString += ProbeLongs(readBytes, 2);
				writeString += " Offset = " + (numBytes - bytesLeft).ToString();
				TracerWriteLine(writeString);
				bytesLeft -= 8;
			}
			if (bytesLeft >= 4)
			{
				readBytes = inFile.ReadBytes(4);
				writeString = "4 bytes: CCCC     ";
				writeString += ProbeChars(readBytes, 4);
				writeString += "       | SS   ";
				writeString += ProbeShorts(readBytes, 2);
				writeString += "                 | L  ";
				writeString += ProbeLongs(readBytes, 1);
				writeString += "             Offset = " + (numBytes - bytesLeft).ToString();
				TracerWriteLine(writeString);
				bytesLeft -= 4;
			}
			if (bytesLeft >= 2)
			{
				readBytes = inFile.ReadBytes(2);
				writeString = "2 bytes: CC       ";
				writeString += ProbeChars(readBytes, 2);
				writeString += "         | S    ";
				writeString += ProbeShorts(readBytes, 1);
				writeString += "                                                      ";
				writeString += "Offset = " + (numBytes - bytesLeft).ToString();
				TracerWriteLine(writeString);
				bytesLeft -= 2;
			}
			if (bytesLeft >= 1)
			{
				readBytes = inFile.ReadBytes(1);
				writeString = "1 byte : C        ";
				writeString += ProbeChars(readBytes, 1);
				writeString += "                                                                            ";
				writeString += " Offset = " + (numBytes - bytesLeft).ToString();
				TracerWriteLine(writeString);
				bytesLeft -= 1;
			}
			TracerOutdent();
		}

		static public void ProbeTo(System.IO.BinaryReader inFile, long filePosition)
		{
			long curPosition = inFile.BaseStream.Position;
			if (curPosition < filePosition)
			{
				ProbeBytes(inFile, filePosition - curPosition);
			}
		}
	}
}

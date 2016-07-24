using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MLTools
{
	public class GrowthData
	{
		private string mFilePath;
		private const int kMaxAttributeCounts = 15;

		private class PlayerData
		{
			public string mName;
			public string mPosition;
			public string mPositionGroup;
			public string mCollege;
			public string mBirthDate;
			public string mHomeTown;
			public int mHeight;
			public int mWeight;
			public int mExperience;
			public int mVolatility;
			public int mSolecismic;
			public double m40Time;
			public int mBench;
			public double mAgility;
			public int mBroadJump;
			public int mPositionDrill;
			public int mPercentDeveloped;
			public int mFormations;
			public int[] mAttributes = new int[kMaxAttributeCounts * 2];
			public int[] mPeakAttributes = new int[kMaxAttributeCounts];
			public int mPeakCurrent;
			public int mPeakFuture;
			public int mAttribCount;

			public string GetKey()
			{
				return mName + mPositionGroup + mCollege + mBirthDate + mHomeTown;
			}
		}
		private Dictionary<string, PlayerData> mPlayerData = new Dictionary<string, PlayerData>();

		public GrowthData(string filePath)
		{
			mFilePath = filePath;
		}

		public void Process()
		{
			var draftFiles = Directory.EnumerateFiles(mFilePath, "Draft*.csv");
			foreach (string curDraftFile in draftFiles)
			{
				LoadDraftFile(curDraftFile);
			}

			var rosterFiles = Directory.EnumerateFiles(mFilePath, "Roster*.csv");
			foreach (string curRosterFile in rosterFiles)
			{
				LoadRosterFile(curRosterFile);
			}

			System.Collections.Generic.Dictionary<string, StreamWriter> posGroupOutFiles = new System.Collections.Generic.Dictionary<string, StreamWriter>();
			posGroupOutFiles.Add("QB", new StreamWriter(Path.Combine(mFilePath, "GrowthQB.csv"), true));
			posGroupOutFiles.Add("QB_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthQB_NC.csv"), true));
			posGroupOutFiles.Add("RB", new StreamWriter(Path.Combine(mFilePath, "GrowthRB.csv"), true));
			posGroupOutFiles.Add("RB_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthRB_NC.csv"), true));
			posGroupOutFiles.Add("FB", new StreamWriter(Path.Combine(mFilePath, "GrowthFB.csv"), true));
			posGroupOutFiles.Add("FB_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthFB_NC.csv"), true));
			posGroupOutFiles.Add("WR", new StreamWriter(Path.Combine(mFilePath, "GrowthWR.csv"), true));
			posGroupOutFiles.Add("WR_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthWR_NC.csv"), true));
			posGroupOutFiles.Add("TE", new StreamWriter(Path.Combine(mFilePath, "GrowthTE.csv"), true));
			posGroupOutFiles.Add("TE_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthTE_NC.csv"), true));
			posGroupOutFiles.Add("T", new StreamWriter(Path.Combine(mFilePath, "GrowthT.csv"), true));
			posGroupOutFiles.Add("T_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthT_NC.csv"), true));
			posGroupOutFiles.Add("G", new StreamWriter(Path.Combine(mFilePath, "GrowthG.csv"), true));
			posGroupOutFiles.Add("G_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthG_NC.csv"), true));
			posGroupOutFiles.Add("C", new StreamWriter(Path.Combine(mFilePath, "GrowthC.csv"), true));
			posGroupOutFiles.Add("C_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthC_NC.csv"), true));
			posGroupOutFiles.Add("DE", new StreamWriter(Path.Combine(mFilePath, "GrowthDE.csv"), true));
			posGroupOutFiles.Add("DE_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthDE_NC.csv"), true));
			posGroupOutFiles.Add("DT", new StreamWriter(Path.Combine(mFilePath, "GrowthDT.csv"), true));
			posGroupOutFiles.Add("DT_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthDT_NC.csv"), true));
			posGroupOutFiles.Add("ILB", new StreamWriter(Path.Combine(mFilePath, "GrowthILB.csv"), true));
			posGroupOutFiles.Add("ILB_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthILB_NC.csv"), true));
			posGroupOutFiles.Add("OLB", new StreamWriter(Path.Combine(mFilePath, "GrowthOLB.csv"), true));
			posGroupOutFiles.Add("OLB_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthOLB_NC.csv"), true));
			posGroupOutFiles.Add("CB", new StreamWriter(Path.Combine(mFilePath, "GrowthCB.csv"), true));
			posGroupOutFiles.Add("CB_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthCB_NC.csv"), true));
			posGroupOutFiles.Add("S", new StreamWriter(Path.Combine(mFilePath, "GrowthS.csv"), true));
			posGroupOutFiles.Add("S_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthS_NC.csv"), true));
			posGroupOutFiles.Add("P", new StreamWriter(Path.Combine(mFilePath, "GrowthP.csv"), true));
			posGroupOutFiles.Add("P_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthP_NC.csv"), true));
			posGroupOutFiles.Add("K", new StreamWriter(Path.Combine(mFilePath, "GrowthK.csv"), true));
			posGroupOutFiles.Add("K_NC", new StreamWriter(Path.Combine(mFilePath, "GrowthK_NC.csv"), true));

			string filename = Path.Combine(mFilePath, "PeakData.csv");
			using (System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename))
			{
				outFile.WriteLine("Name,Pos,PG,Coll,Birth,HomeTown,Ht,Wt,Exp,Vol,Sole,40,Ben,Agi,BJ,PD,PctD,Form,Cur,Fut");
				foreach (KeyValuePair<string, PlayerData> kvp in mPlayerData)
				{
					if (kvp.Value.mExperience > 0)
					{
						outFile.Write("\"" + kvp.Value.mName + "\",");
						outFile.Write(kvp.Value.mPosition + ",");
						outFile.Write(kvp.Value.mPositionGroup + ",");
						outFile.Write("\"" + kvp.Value.mCollege + "\",");
						outFile.Write(kvp.Value.mBirthDate + ",");
						outFile.Write("\"" + kvp.Value.mHomeTown + "\",");
						outFile.Write(kvp.Value.mHeight + ",");
						outFile.Write(kvp.Value.mWeight + ",");
						outFile.Write(kvp.Value.mExperience + ",");
						outFile.Write(kvp.Value.mVolatility + ",");
						outFile.Write(kvp.Value.mSolecismic + ",");
						outFile.Write(kvp.Value.m40Time.ToString("F2") + ",");
						outFile.Write(kvp.Value.mBench + ",");
						outFile.Write(kvp.Value.mAgility.ToString("F2") + ",");
						outFile.Write(kvp.Value.mBroadJump + ",");
						outFile.Write(kvp.Value.mPositionDrill + ",");
						outFile.Write(kvp.Value.mPercentDeveloped + ",");
						outFile.Write(kvp.Value.mFormations + ",");
						outFile.Write(kvp.Value.mPeakCurrent + ",");
						outFile.Write(kvp.Value.mPeakFuture);
						for (int i = 0; i < (kvp.Value.mAttribCount*2); ++i)
						{
							outFile.Write("," + kvp.Value.mAttributes[i]);
						}
						for (int j = 0; j < kvp.Value.mAttribCount; ++j)
						{
							outFile.Write("," + kvp.Value.mPeakAttributes[j]);
						}
						outFile.WriteLine();

						string posFileKey = kvp.Value.mPositionGroup;
						bool hasCombines = true;
						if (kvp.Value.mBench == 0 && kvp.Value.mBroadJump == 0)
						{
							posFileKey += "_NC";
							hasCombines = false;
						}
						StreamWriter posFile = posGroupOutFiles[posFileKey];
						posFile.Write(kvp.Value.mSolecismic + ",");
						if (hasCombines)
						{
							posFile.Write(kvp.Value.m40Time.ToString("F2") + ",");
							posFile.Write(kvp.Value.mBench + ",");
							posFile.Write(kvp.Value.mAgility.ToString("F2") + ",");
							posFile.Write(kvp.Value.mBroadJump + ",");
						}
						if (kvp.Value.mPositionGroup == "QB"
							|| kvp.Value.mPositionGroup == "RB"
							|| kvp.Value.mPositionGroup == "FB"
							|| kvp.Value.mPositionGroup == "WR"
							|| kvp.Value.mPositionGroup == "TE"
							|| kvp.Value.mPositionGroup == "CB"
							|| kvp.Value.mPositionGroup == "S"
							)
						{
							posFile.Write(kvp.Value.mPositionDrill + ",");
						}
						posFile.Write(kvp.Value.mPercentDeveloped + ",");
						if (kvp.Value.mPositionGroup == "QB")
						{
							posFile.Write(kvp.Value.mFormations + ",");
						}
						posFile.Write(kvp.Value.mAttributes[0]);
						for (int i = 1; i < (kvp.Value.mAttribCount * 2); ++i)
						{
							posFile.Write("," + kvp.Value.mAttributes[i]);
						}
						for (int j = 0; j < kvp.Value.mAttribCount; ++j)
						{
							posFile.Write("," + kvp.Value.mPeakAttributes[j]);
						}
						posFile.WriteLine();
					}
				}
			}
			foreach (StreamWriter posFile in posGroupOutFiles.Values)
			{
				posFile.Close();
			}
		}

		private void LoadDraftFile(string filename)
		{
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
			{
				System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

				string headerLine = inFile.ReadLine();
				while (!inFile.EndOfStream)
				{
					string curLine = inFile.ReadLine();
					string[] fields = DataReader.CSVHelper.ParseLine(curLine);

					PlayerData newData = new PlayerData();
					newData.mName = fields[0];
					newData.mPosition = fields[1];
					newData.mPositionGroup = fields[2];
					newData.mCollege = fields[3];
					newData.mBirthDate = fields[5];
					newData.mHomeTown = fields[6];
					newData.mHeight = Int32.Parse(fields[9], nfi);
					newData.mWeight = Int32.Parse(fields[10], nfi);
					newData.mVolatility = Int32.Parse(fields[12], nfi);
					newData.mSolecismic = Int32.Parse(fields[21], nfi);
					newData.m40Time = Double.Parse(fields[22], nfi);
					newData.mBench = Int32.Parse(fields[23], nfi);
					newData.mAgility = Double.Parse(fields[24], nfi);
					newData.mBroadJump = Int32.Parse(fields[25], nfi);
					newData.mPositionDrill = Int32.Parse(fields[26], nfi);
					newData.mPercentDeveloped = Int32.Parse(fields[27], nfi);
					int attributeStartField = 35;
					newData.mFormations = 0;
					if (newData.mPositionGroup == "QB")
					{
						newData.mFormations = Int32.Parse(fields[attributeStartField]);
						attributeStartField += 1;
					}
					int attributeIndex = 0;
					for (int i = 0; i < newData.mAttributes.Length; ++i)
					{
						newData.mAttributes[i] = 0;
					}
					newData.mAttribCount = 0;
					while (attributeStartField < fields.Length)
					{
						newData.mAttributes[attributeIndex++] = Int32.Parse(fields[attributeStartField++], nfi);
						newData.mAttribCount++;
					}
					newData.mAttribCount /= 2;
					for (int j = 0; j < newData.mPeakAttributes.Length; ++j)
					{
						newData.mPeakAttributes[j] = 0;
					}
					newData.mPeakCurrent = 0;
					newData.mPeakFuture = 0;
					newData.mExperience = 0;
					mPlayerData[newData.GetKey()] = newData;
				}
				inFile.Close();
			}
		}

		private void LoadRosterFile(string filename)
		{
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
			{
				System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;
				PlayerData newData = new PlayerData();

				string headerLine = inFile.ReadLine();
				while (!inFile.EndOfStream)
				{
					string curLine = inFile.ReadLine();
					string[] fields = DataReader.CSVHelper.ParseLine(curLine);

					newData.mName = fields[0];
					newData.mPosition = fields[1];
					newData.mPositionGroup = fields[2];
					newData.mCollege = fields[3];
					newData.mBirthDate = fields[5];
					newData.mHomeTown = fields[6];
					newData.mHeight = Int32.Parse(fields[9], nfi);
					newData.mWeight = Int32.Parse(fields[10], nfi);
					newData.mExperience = Int32.Parse(fields[11], nfi);
					newData.mVolatility = Int32.Parse(fields[12], nfi);
					newData.mSolecismic = Int32.Parse(fields[21], nfi);
					newData.m40Time = Double.Parse(fields[22], nfi);
					newData.mBench = Int32.Parse(fields[23], nfi);
					newData.mAgility = Double.Parse(fields[24], nfi);
					newData.mBroadJump = Int32.Parse(fields[25], nfi);
					newData.mPositionDrill = Int32.Parse(fields[26], nfi);
					newData.mPercentDeveloped = Int32.Parse(fields[27], nfi);
					// Fields 28 & 29 are interviewed fields
					newData.mPeakCurrent = Int32.Parse(fields[30], nfi);
					newData.mPeakFuture = Int32.Parse(fields[31], nfi);
					int attributeStartField = 35;
					newData.mFormations = 0;
					if (newData.mPositionGroup == "QB")
					{
						newData.mFormations = Int32.Parse(fields[attributeStartField]);
						attributeStartField += 1;
					}
					int attributeIndex = 0;
					for (int i = 0; i < newData.mAttributes.Length; ++i)
					{
						newData.mAttributes[i] = 0;
					}
					while (attributeStartField < fields.Length)
					{
						newData.mAttributes[attributeIndex++] = Int32.Parse(fields[attributeStartField++], nfi);
					}

					PlayerData draftData;
					if (newData.mExperience > 1 && mPlayerData.TryGetValue(newData.GetKey(), out draftData))
					{
						int newDiff = newData.mPeakFuture - newData.mPeakCurrent;
						int draftDiff = draftData.mPeakFuture - draftData.mPeakCurrent;
						bool copyData = (draftData.mPeakFuture == 0);
						if (newDiff < draftDiff)
						{
							copyData = true;
						}
						else if (newDiff == 0 && newData.mPeakFuture > draftData.mPeakFuture)
						{
							copyData = true;
						}
						if (copyData)
						{
							for (int i=0;i<kMaxAttributeCounts;++i)
							{
								draftData.mPeakAttributes[i] = newData.mAttributes[(i*2)+1];
							}
							draftData.mPeakCurrent = newData.mPeakCurrent;
							draftData.mPeakFuture = newData.mPeakFuture;
							draftData.mExperience = newData.mExperience;
						}
					}
				}
				inFile.Close();
			}
		}
	}
}

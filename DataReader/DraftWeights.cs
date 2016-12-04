using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataReader
{
	public class DraftWeights
	{
		public const int MaxAttributeCounts = 15;

		public class PositionWeights
		{
			public double Weight;
			public double Dash;
			public double Solecismic;
			public double Bench;
			public double Agility;
			public double BroadJump;
			public double PositionDrill;
			public double[] Attributes = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
		}

		public class PositionWeightInputs
		{
			public double Weight;
			public int Dash;
			public int Solecismic;
			public int Bench;
			public int Agility;
			public int BroadJump;
			public int PositionDrill;
			public int[] Attributes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		}

		public enum AttributeUsage
		{
			UseMin,
			UseAverage,
			UseMax
		}

		public class GlobalWeightData
		{
			public int Attributes = 100;
			public int Combines = 10;
			public int NoCombineAttributes = 100;
			public int NoCombineCombines = 10;
			public int Height = 0;
			public int Weight = 0;
			public int ScoutImpression = 0;
			public int Affinity = 10;
			public int Conflict = 30;
			public int RedFlag = 30;
			public int AvgDev = 30;
			public int DevWt = 10;
			public int CombineThresholdPenalty = -100;
			public AttributeUsage WhichAttributesToUse = AttributeUsage.UseAverage;
            public FOFData.DefensiveFront DefensiveFront = FOFData.DefensiveFront.True34;
		}

		private FOFData mFOFData = new FOFData();
		private Dictionary<string, PositionWeights> mPositionWeightsMap;
		private Dictionary<string, PositionWeights> mNoCombinePositionWeightsMap;
		private Dictionary<string, PositionWeightInputs> mPositionWeightsInputMap;
		private GlobalWeightData mGlobalWeights = new GlobalWeightData();

		public PositionWeights GetPositionWeight(string position)
		{
			return mPositionWeightsMap[position];
		}
		public PositionWeights GetNoCombinePositionWeight(string position)
		{
			return mNoCombinePositionWeightsMap[position];
		}
		public PositionWeightInputs GetPositionWeightInputs(string position)
		{
			return mPositionWeightsInputMap[position];
		}
		public string GetPositionGroup(string position)
		{
			return mFOFData.PositionToPositionGroupMap[position];
		}
		public GlobalWeightData GlobalWeights { get { return mGlobalWeights; } }
		public FOFData FOFData { get { return mFOFData; } }

		private const int kWeightsVersion = 7;

		public DraftWeights()
		{
			InitializeMap();
		}

		public void LoadData(string filePath)
		{
			if (System.IO.File.Exists(filePath))
			{
				Encoding windows1252Encoding = Encoding.GetEncoding(1252);
				System.IO.FileStream inStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

				long fileLength = inStream.Length;

				int version = inFile.ReadInt32();
				if (version >= 2)
				{
					mGlobalWeights.Attributes = inFile.ReadInt32();
					mGlobalWeights.Combines = inFile.ReadInt32();
					mGlobalWeights.Height = inFile.ReadInt32();
					mGlobalWeights.ScoutImpression = inFile.ReadInt32();
					mGlobalWeights.Weight = inFile.ReadInt32();
					mGlobalWeights.Affinity = inFile.ReadInt32();
					if (version >= 3)
					{
						mGlobalWeights.Conflict = inFile.ReadInt32();
						mGlobalWeights.RedFlag = inFile.ReadInt32();
						if (version >= 4)
						{
							mGlobalWeights.NoCombineAttributes = inFile.ReadInt32();
							mGlobalWeights.NoCombineCombines = inFile.ReadInt32();
						}
						if (version >= 5)
						{
							mGlobalWeights.AvgDev = inFile.ReadInt32();
							mGlobalWeights.DevWt = inFile.ReadInt32();
						}
						if (version >= 6)
						{
							mGlobalWeights.CombineThresholdPenalty = inFile.ReadInt32();
						}
					}
					else
					{
						mGlobalWeights.Conflict = mGlobalWeights.Affinity;
						mGlobalWeights.RedFlag = mGlobalWeights.Affinity;
					}
					mGlobalWeights.WhichAttributesToUse = (AttributeUsage)Enum.Parse(typeof(AttributeUsage), inFile.ReadString());
                    if (version >= 7)
                    {
                        mGlobalWeights.DefensiveFront = (FOFData.DefensiveFront)Enum.Parse(typeof(FOFData.DefensiveFront), inFile.ReadString());
                    }

                    while (inStream.Position < fileLength)
					{
						LoadPositionWeight(inFile);
					}
				}

				inFile.Close();
			}
		}

		public void SaveData(string filePath)
		{
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);

			System.IO.FileStream outStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
			System.IO.BinaryWriter outFile = new System.IO.BinaryWriter(outStream, windows1252Encoding);

			outFile.Write(kWeightsVersion);

			outFile.Write(mGlobalWeights.Attributes);
			outFile.Write(mGlobalWeights.Combines);
			outFile.Write(mGlobalWeights.Height);
			outFile.Write(mGlobalWeights.ScoutImpression);
			outFile.Write(mGlobalWeights.Weight);
			outFile.Write(mGlobalWeights.Affinity);
			outFile.Write(mGlobalWeights.Conflict);
			outFile.Write(mGlobalWeights.RedFlag);
			outFile.Write(mGlobalWeights.NoCombineAttributes);
			outFile.Write(mGlobalWeights.NoCombineCombines);
			outFile.Write(mGlobalWeights.AvgDev);
			outFile.Write(mGlobalWeights.DevWt);
			outFile.Write(mGlobalWeights.CombineThresholdPenalty);
			outFile.Write(mGlobalWeights.WhichAttributesToUse.ToString());
            outFile.Write(mGlobalWeights.DefensiveFront.ToString());

			SavePositionWeight(outFile, "QB");
			SavePositionWeight(outFile, "RB");
			SavePositionWeight(outFile, "FB");
			SavePositionWeight(outFile, "TE");
			SavePositionWeight(outFile, "FL");
			SavePositionWeight(outFile, "SE");
			SavePositionWeight(outFile, "C");
			SavePositionWeight(outFile, "RG");
			SavePositionWeight(outFile, "LG");
			SavePositionWeight(outFile, "RT");
			SavePositionWeight(outFile, "LT");
			SavePositionWeight(outFile, "P");
			SavePositionWeight(outFile, "K");
			SavePositionWeight(outFile, "LDE");
			SavePositionWeight(outFile, "RDE");
			SavePositionWeight(outFile, "LDT");
			SavePositionWeight(outFile, "RDT");
			SavePositionWeight(outFile, "NT");
			SavePositionWeight(outFile, "WILB");
			SavePositionWeight(outFile, "SILB");
			SavePositionWeight(outFile, "MLB");
			SavePositionWeight(outFile, "WLB");
			SavePositionWeight(outFile, "SLB");
			SavePositionWeight(outFile, "LCB");
			SavePositionWeight(outFile, "RCB");
			SavePositionWeight(outFile, "FS");
			SavePositionWeight(outFile, "SS");
			SavePositionWeight(outFile, "LS");

			outFile.Close();
		}

		private void SavePositionWeight(System.IO.BinaryWriter outFile, string position)
		{
			PositionWeightInputs posWeight = mPositionWeightsInputMap[position];

			outFile.Write(position);
			outFile.Write(posWeight.Weight);
			outFile.Write(posWeight.Dash);
			outFile.Write(posWeight.Solecismic);
			outFile.Write(posWeight.Bench);
			outFile.Write(posWeight.Agility);
			outFile.Write(posWeight.BroadJump);
			outFile.Write(posWeight.PositionDrill);
			for (int i = 0; i < posWeight.Attributes.Length; ++i)
			{
				outFile.Write(posWeight.Attributes[i]);
			}
		}

		private void LoadPositionWeight(System.IO.BinaryReader inFile)
		{
			string position = inFile.ReadString();
			PositionWeightInputs posWeight = mPositionWeightsInputMap[position];
			posWeight.Weight = inFile.ReadDouble();
			posWeight.Dash = inFile.ReadInt32();
			posWeight.Solecismic = inFile.ReadInt32();
			posWeight.Bench = inFile.ReadInt32();
			posWeight.Agility = inFile.ReadInt32();
			posWeight.BroadJump = inFile.ReadInt32();
			posWeight.PositionDrill = inFile.ReadInt32();
			for (int i = 0; i < posWeight.Attributes.Length; ++i)
			{
				posWeight.Attributes[i] = inFile.ReadInt32();
			}

			mPositionWeightsMap[position] = BuildWeightFromInput(posWeight);
			mNoCombinePositionWeightsMap[position] = BuildNoCombineWeightFromInput(posWeight);
		}

		PositionWeights BuildWeightFromInput(PositionWeightInputs weightInput)
		{
			PositionWeights newWeight = new PositionWeights();
			newWeight.Weight = weightInput.Weight;

			double combineWeight = (double)(mGlobalWeights.Combines);
			double combineTotal = (double)(weightInput.Agility + weightInput.Bench + weightInput.BroadJump + weightInput.Dash +
								weightInput.PositionDrill + weightInput.Solecismic);
			double combineFactor = 0.0;
			if (combineTotal > 0.0)
			{
				combineFactor = combineWeight / combineTotal;
			}
			newWeight.Agility = (double)weightInput.Agility * combineFactor;
			newWeight.Bench = (double)weightInput.Bench * combineFactor;
			newWeight.BroadJump = (double)weightInput.BroadJump * combineFactor;
			newWeight.Dash = (double)weightInput.Dash * combineFactor;
			newWeight.PositionDrill = (double)weightInput.PositionDrill * combineFactor;
			newWeight.Solecismic = (double)weightInput.Solecismic * combineFactor;

			double attributeWeight = (double)(mGlobalWeights.Attributes);
			double attributeTotal = 0.0;
			int attributeIndex;
			for (attributeIndex = 0; attributeIndex < MaxAttributeCounts; ++attributeIndex)
			{
				attributeTotal += (double)(weightInput.Attributes[attributeIndex]);
			}
			double attributeFactor = 0.0;
			if (attributeTotal > 0.0)
			{
				attributeFactor = attributeWeight / attributeTotal;
			}
			for (attributeIndex = 0; attributeIndex < MaxAttributeCounts; ++attributeIndex)
			{
				newWeight.Attributes[attributeIndex] = (double)weightInput.Attributes[attributeIndex] * attributeFactor;
			}

			return newWeight;
		}

		PositionWeights BuildNoCombineWeightFromInput(PositionWeightInputs weightInput)
		{
			PositionWeights newWeight = new PositionWeights();
			newWeight.Weight = weightInput.Weight;

			double combineWeight = (double)(mGlobalWeights.NoCombineCombines);
			double combineTotal = (double)(weightInput.PositionDrill + weightInput.Solecismic);
			double combineFactor = 0.0;
			if (combineTotal > 0.0)
			{
				combineFactor = combineWeight / combineTotal;
			}
			newWeight.Agility = 0.0;
			newWeight.Bench = 0.0;
			newWeight.BroadJump = 0.0;
			newWeight.Dash = 0.0;
			newWeight.PositionDrill = (double)weightInput.PositionDrill * combineFactor;
			newWeight.Solecismic = (double)weightInput.Solecismic * combineFactor;

			double attributeWeight = (double)(mGlobalWeights.NoCombineAttributes);
			double attributeTotal = 0.0;
			int attributeIndex;
			for (attributeIndex = 0; attributeIndex < MaxAttributeCounts; ++attributeIndex)
			{
				attributeTotal += (double)(weightInput.Attributes[attributeIndex]);
			}
			double attributeFactor = 0.0;
			if (attributeTotal > 0.0)
			{
				attributeFactor = attributeWeight / attributeTotal;
			}
			for (attributeIndex = 0; attributeIndex < MaxAttributeCounts; ++attributeIndex)
			{
				newWeight.Attributes[attributeIndex] = (double)weightInput.Attributes[attributeIndex] * attributeFactor;
			}

			return newWeight;
		}

		public void UpdatePositionWeights()
		{
			foreach (string positionName in mPositionWeightsInputMap.Keys)
			{
				mPositionWeightsMap[positionName] = BuildWeightFromInput(mPositionWeightsInputMap[positionName]);
				mNoCombinePositionWeightsMap[positionName] = BuildNoCombineWeightFromInput(mPositionWeightsInputMap[positionName]);
			}
		}

		public void Reset()
		{
			InitializeMap();
		}

		public void SetFOFDefaultWeights()
		{
			mPositionWeightsInputMap["QB"].Weight = 1.137;
			mPositionWeightsInputMap["RB"].Weight = 1.058;
			mPositionWeightsInputMap["FB"].Weight = 0.805;
			mPositionWeightsInputMap["TE"].Weight = 0.867;
			mPositionWeightsInputMap["FL"].Weight = 1.036;
			mPositionWeightsInputMap["SE"].Weight = 1.036;
			mPositionWeightsInputMap["C"].Weight = 0.856;
			mPositionWeightsInputMap["RG"].Weight = 0.945;
			mPositionWeightsInputMap["LG"].Weight = 0.945;
			mPositionWeightsInputMap["LT"].Weight = 1.095;
			mPositionWeightsInputMap["RT"].Weight = 1.095;
			mPositionWeightsInputMap["P"].Weight = 0.529;
			mPositionWeightsInputMap["K"].Weight = 0.591;
			mPositionWeightsInputMap["LDE"].Weight = 1.095;
			mPositionWeightsInputMap["RDE"].Weight = 1.095;
			mPositionWeightsInputMap["LDT"].Weight = 1.076;
			mPositionWeightsInputMap["RDT"].Weight = 1.076;
			mPositionWeightsInputMap["NT"].Weight = 1.076;
			mPositionWeightsInputMap["WILB"].Weight = 0.971;
			mPositionWeightsInputMap["SILB"].Weight = 0.971;
			mPositionWeightsInputMap["MLB"].Weight = 0.971;
			mPositionWeightsInputMap["SLB"].Weight = 0.955;
			mPositionWeightsInputMap["WLB"].Weight = 0.955;
			mPositionWeightsInputMap["LCB"].Weight = 1.027;
			mPositionWeightsInputMap["RCB"].Weight = 1.027;
			mPositionWeightsInputMap["SS"].Weight = 0.938;
			mPositionWeightsInputMap["FS"].Weight = 0.938;
			mPositionWeightsInputMap["LS"].Weight = 0.86;
		}

		public void SetAllOnesWeights()
		{
			mPositionWeightsInputMap["QB"].Weight = 1.0;
			mPositionWeightsInputMap["RB"].Weight = 1.0;
			mPositionWeightsInputMap["FB"].Weight = 1.0;
			mPositionWeightsInputMap["TE"].Weight = 1.0;
			mPositionWeightsInputMap["FL"].Weight = 1.0;
			mPositionWeightsInputMap["SE"].Weight = 1.0;
			mPositionWeightsInputMap["C"].Weight = 1.0;
			mPositionWeightsInputMap["RG"].Weight = 1.0;
			mPositionWeightsInputMap["LG"].Weight = 1.0;
			mPositionWeightsInputMap["LT"].Weight = 1.0;
			mPositionWeightsInputMap["RT"].Weight = 1.0;
			mPositionWeightsInputMap["P"].Weight = 1.0;
			mPositionWeightsInputMap["K"].Weight = 1.0;
			mPositionWeightsInputMap["LDE"].Weight = 1.0;
			mPositionWeightsInputMap["RDE"].Weight = 1.0;
			mPositionWeightsInputMap["LDT"].Weight = 1.0;
			mPositionWeightsInputMap["RDT"].Weight = 1.0;
			mPositionWeightsInputMap["NT"].Weight = 1.0;
			mPositionWeightsInputMap["WILB"].Weight = 1.0;
			mPositionWeightsInputMap["SILB"].Weight = 1.0;
			mPositionWeightsInputMap["MLB"].Weight = 1.0;
			mPositionWeightsInputMap["SLB"].Weight = 1.0;
			mPositionWeightsInputMap["WLB"].Weight = 1.0;
			mPositionWeightsInputMap["LCB"].Weight = 1.0;
			mPositionWeightsInputMap["RCB"].Weight = 1.0;
			mPositionWeightsInputMap["SS"].Weight = 1.0;
			mPositionWeightsInputMap["FS"].Weight = 1.0;
			mPositionWeightsInputMap["LS"].Weight = 1.0;
		}

		private void InitializeMap()
		{
			mGlobalWeights = new GlobalWeightData();

			mPositionWeightsMap = new Dictionary<string, PositionWeights>();
			mNoCombinePositionWeightsMap = new Dictionary<string, PositionWeights>();
			mPositionWeightsInputMap = new Dictionary<string, DataReader.DraftWeights.PositionWeightInputs>();

			DataReader.DraftWeights.PositionWeightInputs newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.137;
			newWeightInput.Dash = 0;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 20;
			newWeightInput.Agility = 10;
			newWeightInput.BroadJump = 20;
			newWeightInput.PositionDrill = 15;
			InitializeAttributeWeights("QB", newWeightInput);
			newWeightInput.Attributes[0] = 20; //"Screen Passes (Ag25)",
			newWeightInput.Attributes[1] = 19; //"Short Passes",
			newWeightInput.Attributes[2] = 22; //"Medium Passes (Bj66)",
			newWeightInput.Attributes[3] = 22; //"Long Passes (Bp50)",
			newWeightInput.Attributes[4] = 15; //"Deep Passes (Bp50)",
			newWeightInput.Attributes[5] = 24; //"Third Down Passing (Bj33)",
			newWeightInput.Attributes[6] = 23; //"Accuracy (PD50)",
			newWeightInput.Attributes[7] = 0; //"Timing (PD50)",
			newWeightInput.Attributes[8] = 8; //"Sense Rush (Ag75)",
			newWeightInput.Attributes[9] = 11; //"Read Defense (So10)",
			newWeightInput.Attributes[10] = 7; //"Two Minute Offense",
			newWeightInput.Attributes[11] = 0; //"Scramble Frequency (Ft85)",
			newWeightInput.Attributes[12] = 0; //"Kick Holding"
			mPositionWeightsInputMap.Add("QB", newWeightInput);
			mPositionWeightsMap["QB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["QB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.058;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 20;
			newWeightInput.Bench = 5;
			newWeightInput.Agility = 8;
			newWeightInput.BroadJump = 10;
			newWeightInput.PositionDrill = 5;
			InitializeAttributeWeights("RB", newWeightInput);
			newWeightInput.Attributes[0] = 5; //"Breakaway Speed (Ft80)",
			newWeightInput.Attributes[1] = 5; //"Power Inside (Bp100)",
			newWeightInput.Attributes[2] = 8;  //"Third Down Running (Ag33)",
			newWeightInput.Attributes[3] = 25;  //"Hole Recognition (So90)",
			newWeightInput.Attributes[4] = 8;  //"Elusiveness (Ag33)",
			newWeightInput.Attributes[5] = 5; //"Speed to Outside (Bj50/Ft20)",
			newWeightInput.Attributes[6] = 6;  //"Blitz Pickup (PD90)",
			newWeightInput.Attributes[7] = 6;  //"Avoid Drops",
			newWeightInput.Attributes[8] = 2;  //"Getting Downfield (Ag33)",
			newWeightInput.Attributes[9] = 6;  //"Route Running",
			newWeightInput.Attributes[10] = 2; //"Third Down Catching (PD05)",
			newWeightInput.Attributes[11] = 1; //"Punt Returns",
			newWeightInput.Attributes[12] = 1; //"Kick Returns",
			newWeightInput.Attributes[13] = 15; //"Endurance (Bj50)",
			newWeightInput.Attributes[14] = 2;//"Special Teams"
			mPositionWeightsInputMap.Add("RB", newWeightInput);
			mPositionWeightsMap["RB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["RB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.805;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 15;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 5;
			newWeightInput.BroadJump = 20;
			newWeightInput.PositionDrill = 15;
			InitializeAttributeWeights("FB", newWeightInput);
			newWeightInput.Attributes[0] = 22;  //"Run Blocking (Bj50)",
			newWeightInput.Attributes[1] = 5;  //"Pass Blocking",
			newWeightInput.Attributes[2] = 3; //"Blocking Strength",
			newWeightInput.Attributes[3] = 9;//"Power Inside (Bp100)",
			newWeightInput.Attributes[4] = 7;  //"Third Down Running (Ag33Bj50)",
			newWeightInput.Attributes[5] = 14;  //"Hole Recognition (So50)",
			newWeightInput.Attributes[6] = 12;  //"Blitz Pickup (PD50)",
			newWeightInput.Attributes[7] = 6;  //"Avoid Drops",
			newWeightInput.Attributes[8] = 8;  //"Route Running (PD50)",
			newWeightInput.Attributes[9] = 3;  //"Third Down Catching",
			newWeightInput.Attributes[10] = 5; //"Endurance",
			newWeightInput.Attributes[11] = 2;//"Special Teams"
			mPositionWeightsInputMap.Add("FB", newWeightInput);
			mPositionWeightsMap["FB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["FB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.867;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 15;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 10;
			newWeightInput.BroadJump = 20;
			newWeightInput.PositionDrill = 8;
			InitializeAttributeWeights("TE", newWeightInput);
			newWeightInput.Attributes[0] = 24; //"Run Blocking (Bj50)",
			newWeightInput.Attributes[1] = 20;  //"Pass Blocking",
			newWeightInput.Attributes[2] = 15; //"Blocking Strength (Bp100)",
			newWeightInput.Attributes[3] = 12;  //"Avoid Drops (PD50)",
			newWeightInput.Attributes[4] = 12;  //"Getting Downfield (Ft50Ag100)",
			newWeightInput.Attributes[5] = 25; //"Route Running (So50)",
			newWeightInput.Attributes[6] = 18; //"Third Down Catching (Bj50)",
			newWeightInput.Attributes[7] = 3; //"Big Play Receiving (Ft50)",
			newWeightInput.Attributes[8] = 5;  //"Courage",
			newWeightInput.Attributes[9] = 4;  //"Adjust to Ball (PD50)",
			newWeightInput.Attributes[10] = 8; //"Endurance",
			newWeightInput.Attributes[11] = 1; //"Special Teams",
			newWeightInput.Attributes[12] = 0;//"Long Snapping"
			mPositionWeightsInputMap.Add("TE", newWeightInput);
			mPositionWeightsMap["TE"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["TE"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.036;
			newWeightInput.Dash = 15;
			newWeightInput.Solecismic = 20;
			newWeightInput.Bench = 5;
			newWeightInput.Agility = 18;
			newWeightInput.BroadJump = 2;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("FL", newWeightInput);
			newWeightInput.Attributes[0] = 10;  //"Avoid Drops (PD65)",
			newWeightInput.Attributes[1] = 16;  //"Getting Downfield (Ag100)",
			newWeightInput.Attributes[2] = 24;  //"Route Running (So50)",
			newWeightInput.Attributes[3] = 13;  //"Third Down Catching",
			newWeightInput.Attributes[4] = 8; //"Big Play Receiving (Ft70)",
			newWeightInput.Attributes[5] = 6;  //"Courage (Bp100)",
			newWeightInput.Attributes[6] = 5;  //"Adjust to Ball (PD35)",
			newWeightInput.Attributes[7] = 5;  //"Punt Returns (Bj50)",
			newWeightInput.Attributes[8] = 5;   //"Kick Returns (Bj50)",
			newWeightInput.Attributes[9] = 4;   //"Endurance",
			newWeightInput.Attributes[10] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("FL", newWeightInput);
			mPositionWeightsMap["FL"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["FL"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.036;
			newWeightInput.Dash = 15;
			newWeightInput.Solecismic = 20;
			newWeightInput.Bench = 5;
			newWeightInput.Agility = 18;
			newWeightInput.BroadJump = 2;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("SE", newWeightInput);
			newWeightInput.Attributes[0] = 10;  //"Avoid Drops (PD65)",
			newWeightInput.Attributes[1] = 16;  //"Getting Downfield (Ag100)",
			newWeightInput.Attributes[2] = 24;  //"Route Running (So50)",
			newWeightInput.Attributes[3] = 13;  //"Third Down Catching",
			newWeightInput.Attributes[4] = 8; //"Big Play Receiving (Ft70)",
			newWeightInput.Attributes[5] = 6;  //"Courage (Bp100)",
			newWeightInput.Attributes[6] = 5;  //"Adjust to Ball (PD35)",
			newWeightInput.Attributes[7] = 5;  //"Punt Returns (Bj50)",
			newWeightInput.Attributes[8] = 5;   //"Kick Returns (Bj50)",
			newWeightInput.Attributes[9] = 4;   //"Endurance",
			newWeightInput.Attributes[10] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("SE", newWeightInput);
			mPositionWeightsMap["SE"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["SE"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.856;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 1;
			newWeightInput.Bench = 12;
			newWeightInput.Agility = 17;
			newWeightInput.BroadJump = 7;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("C", newWeightInput);
			newWeightInput.Attributes[0] = 24; //"Run Blocking (Ft100)",
			newWeightInput.Attributes[1] = 17; //"Pass Blocking (Ag100)",
			newWeightInput.Attributes[2] = 12;//"Blocking Strength (Bp100)",
			newWeightInput.Attributes[3] = 7; //"Endurance (Bj100)",
			newWeightInput.Attributes[4] = 0; //"Long Snapping"
			mPositionWeightsInputMap.Add("C", newWeightInput);
			mPositionWeightsMap["C"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["C"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.945;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 1;
			newWeightInput.Bench = 13;
			newWeightInput.Agility = 15;
			newWeightInput.BroadJump = 7;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("RG", newWeightInput);
			newWeightInput.Attributes[0] = 24; //"Run Blocking (Ft100)",
			newWeightInput.Attributes[1] = 15; //"Pass Blocking (Ag100)",
			newWeightInput.Attributes[2] = 13;//"Blocking Strength (Bp100)",
			newWeightInput.Attributes[3] = 7; //"Endurance (Bj100)",
			mPositionWeightsInputMap.Add("RG", newWeightInput);
			mPositionWeightsMap["RG"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["RG"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.945;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 1;
			newWeightInput.Bench = 13;
			newWeightInput.Agility = 15;
			newWeightInput.BroadJump = 7;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("LG", newWeightInput);
			newWeightInput.Attributes[0] = 24; //"Run Blocking (Ft100)",
			newWeightInput.Attributes[1] = 15; //"Pass Blocking (Ag100)",
			newWeightInput.Attributes[2] = 13;//"Blocking Strength (Bp100)",
			newWeightInput.Attributes[3] = 7; //"Endurance (Bj100)",
			mPositionWeightsInputMap.Add("LG", newWeightInput);
			mPositionWeightsMap["LG"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["LG"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.095;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 1;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 7;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("LT", newWeightInput);
			newWeightInput.Attributes[0] = 24; //"Run Blocking (Ft100)",
			newWeightInput.Attributes[1] = 23; //"Pass Blocking (Ag100)",
			newWeightInput.Attributes[2] = 13;//"Blocking Strength (Bp100)",
			newWeightInput.Attributes[3] = 8; //"Endurance (Bj100)",
			mPositionWeightsInputMap.Add("LT", newWeightInput);
			mPositionWeightsMap["LT"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["LT"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.095;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 1;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 7;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("RT", newWeightInput);
			newWeightInput.Attributes[0] = 24; //"Run Blocking (Ft100)",
			newWeightInput.Attributes[1] = 23; //"Pass Blocking (Ag100)",
			newWeightInput.Attributes[2] = 13;//"Blocking Strength (Bp100)",
			newWeightInput.Attributes[3] = 8; //"Endurance (Bj100)",
			mPositionWeightsInputMap.Add("RT", newWeightInput);
			mPositionWeightsMap["RT"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["RT"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.529;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 15;
			newWeightInput.Agility = 0;
			newWeightInput.BroadJump = 0;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("P", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Kicking Power (Ft100)",
			newWeightInput.Attributes[1] = 12; //"Punt Hang Time (Bp100)",
			newWeightInput.Attributes[2] = 8; //"Directional Punting (So50)",
			newWeightInput.Attributes[3] = 0; //"Kick Holding"
			mPositionWeightsInputMap.Add("P", newWeightInput);
			mPositionWeightsMap["P"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["P"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.591;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 20;
			newWeightInput.Bench = 15;
			newWeightInput.Agility = 0;
			newWeightInput.BroadJump = 7;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("K", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Kicking Accuracy (So50)",
			newWeightInput.Attributes[1] = 14; //"Kicking Power (Bp100Bj50)",
			newWeightInput.Attributes[2] = 3;  //"Kickoff Distance (Ft100)",
			newWeightInput.Attributes[3] = 2; //"Kickoff Hang Time (Bj50)"
			mPositionWeightsInputMap.Add("K", newWeightInput);
			mPositionWeightsMap["K"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["K"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.095;
			newWeightInput.Dash = 17;
			newWeightInput.Solecismic = 5;
			newWeightInput.Bench = 15;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 5;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("LDE", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 18; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 16;//"Pass Rush Strength (Bp50)",
			newWeightInput.Attributes[3] = 6; //"Play Diagnosis (So50)",
			newWeightInput.Attributes[4] = 3; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[5] = 6; //"Endurance (Bj100)"
			mPositionWeightsInputMap.Add("LDE", newWeightInput);
			mPositionWeightsMap["LDE"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["LDE"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.095;
			newWeightInput.Dash = 17;
			newWeightInput.Solecismic = 5;
			newWeightInput.Bench = 15;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 5;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("RDE", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 18; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 16;//"Pass Rush Strength (Bp50)",
			newWeightInput.Attributes[3] = 6; //"Play Diagnosis (So50)",
			newWeightInput.Attributes[4] = 3; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[5] = 6; //"Endurance (Bj100)"
			mPositionWeightsInputMap.Add("RDE", newWeightInput);
			mPositionWeightsMap["RDE"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["RDE"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.076;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 5;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 5;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("LDT", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 10; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 13;//"Pass Rush Strength (Bp50)",
			newWeightInput.Attributes[3] = 4;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[4] = 2; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[5] = 6; //"Endurance (Bj100)"
			mPositionWeightsInputMap.Add("LDT", newWeightInput);
			mPositionWeightsMap["LDT"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["LDT"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.076;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 5;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 5;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("RDT", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 10; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 13;//"Pass Rush Strength (Bp50)",
			newWeightInput.Attributes[3] = 4;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[4] = 2; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[5] = 6; //"Endurance (Bj100)"
			mPositionWeightsInputMap.Add("RDT", newWeightInput);
			mPositionWeightsMap["RDT"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["RDT"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.076;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 5;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 5;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("NT", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 10; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 13;//"Pass Rush Strength (Bp50)",
			newWeightInput.Attributes[3] = 4;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[4] = 2; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[5] = 6; //"Endurance (Bj100)"
			mPositionWeightsInputMap.Add("NT", newWeightInput);
			mPositionWeightsMap["NT"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["NT"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.971;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 10;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("WILB", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 5; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 8; //"Man-to-Man Defense (Bj100)",
			newWeightInput.Attributes[3] = 8; //"Zone Defense (PD50)",
			newWeightInput.Attributes[4] = 8;  //"Bump and Run Defense (Bp33)",
			newWeightInput.Attributes[5] = 2; //"Pass Rush Strength (Bp33)",
			newWeightInput.Attributes[6] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[7] = 4; //"Punishing Hitter (Bp33)",
			newWeightInput.Attributes[8] = 4;  //"Endurance",
			newWeightInput.Attributes[9] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("WILB", newWeightInput);
			mPositionWeightsMap["WILB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["WILB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.971;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 10;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("SILB", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 5; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 8; //"Man-to-Man Defense (Bj100)",
			newWeightInput.Attributes[3] = 8; //"Zone Defense (PD50)",
			newWeightInput.Attributes[4] = 8;  //"Bump and Run Defense (Bp33)",
			newWeightInput.Attributes[5] = 2; //"Pass Rush Strength (Bp33)",
			newWeightInput.Attributes[6] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[7] = 4; //"Punishing Hitter (Bp33)",
			newWeightInput.Attributes[8] = 4;  //"Endurance",
			newWeightInput.Attributes[9] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("SILB", newWeightInput);
			mPositionWeightsMap["SILB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["SILB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.971;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 10;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("MLB", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 5; //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 8; //"Man-to-Man Defense (Bj100)",
			newWeightInput.Attributes[3] = 8; //"Zone Defense (PD50)",
			newWeightInput.Attributes[4] = 8;  //"Bump and Run Defense (Bp33)",
			newWeightInput.Attributes[5] = 2; //"Pass Rush Strength (Bp33)",
			newWeightInput.Attributes[6] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[7] = 4; //"Punishing Hitter (Bp33)",
			newWeightInput.Attributes[8] = 4;  //"Endurance",
			newWeightInput.Attributes[9] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("MLB", newWeightInput);
			mPositionWeightsMap["MLB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["MLB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.955;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 8;
			newWeightInput.Bench = 12;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 12;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("SLB", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 8;  //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 10;  //"Man-to-Man Defense (Bj100)",
			newWeightInput.Attributes[3] = 10;  //"Zone Defense (PD50)",
			newWeightInput.Attributes[4] = 10;  //"Bump and Run Defense (Bp33)",
			newWeightInput.Attributes[5] = 2; //"Pass Rush Strength (Bp33)",
			newWeightInput.Attributes[6] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[7] = 4; //"Punishing Hitter (Bp33)",
			newWeightInput.Attributes[8] = 4;  //"Endurance",
			newWeightInput.Attributes[9] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("SLB", newWeightInput);
			mPositionWeightsMap["SLB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["SLB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.955;
			newWeightInput.Dash = 5;
			newWeightInput.Solecismic = 8;
			newWeightInput.Bench = 12;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 12;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("WLB", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 8;  //"Pass Rush Technique (Ft100)",
			newWeightInput.Attributes[2] = 10;  //"Man-to-Man Defense (Bj100)",
			newWeightInput.Attributes[3] = 10;  //"Zone Defense (PD50)",
			newWeightInput.Attributes[4] = 10;  //"Bump and Run Defense (Bp33)",
			newWeightInput.Attributes[5] = 2; //"Pass Rush Strength (Bp33)",
			newWeightInput.Attributes[6] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[7] = 4; //"Punishing Hitter (Bp33)",
			newWeightInput.Attributes[8] = 4;  //"Endurance",
			newWeightInput.Attributes[9] = 1; //"Special Teams"
			mPositionWeightsInputMap.Add("WLB", newWeightInput);
			mPositionWeightsMap["WLB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["WLB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.027;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 15;
			newWeightInput.BroadJump = 1;
			newWeightInput.PositionDrill = 20;
			InitializeAttributeWeights("LCB", newWeightInput);
			newWeightInput.Attributes[0] = 16; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 24; //"Man-to-Man Defense (Ft50)",
			newWeightInput.Attributes[2] = 22; //"Zone Defense (Ft50PD50)",
			newWeightInput.Attributes[3] = 24; //"Bump and Run Defense (Bp50)",
			newWeightInput.Attributes[4] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[5] = 2; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[6] = 9;  //"Interceptions (PD50)",
			newWeightInput.Attributes[7] = 1;  //"Punt Returns (Bj50)",
			newWeightInput.Attributes[8] = 1;  //"Kick Returns (Bj50)",
			newWeightInput.Attributes[9] = 5;  //"Endurance",
			newWeightInput.Attributes[10] = 1;//"Special Teams"
			mPositionWeightsInputMap.Add("LCB", newWeightInput);
			mPositionWeightsMap["LCB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["LCB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 1.027;
			newWeightInput.Dash = 20;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 15;
			newWeightInput.BroadJump = 1;
			newWeightInput.PositionDrill = 20;
			InitializeAttributeWeights("RCB", newWeightInput);
			newWeightInput.Attributes[0] = 16; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 24; //"Man-to-Man Defense (Ft50)",
			newWeightInput.Attributes[2] = 22; //"Zone Defense (Ft50PD50)",
			newWeightInput.Attributes[3] = 24; //"Bump and Run Defense (Bp50)",
			newWeightInput.Attributes[4] = 8;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[5] = 2; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[6] = 9;  //"Interceptions (PD50)",
			newWeightInput.Attributes[7] = 1;  //"Punt Returns (Bj50)",
			newWeightInput.Attributes[8] = 1;  //"Kick Returns (Bj50)",
			newWeightInput.Attributes[9] = 5;  //"Endurance",
			newWeightInput.Attributes[10] = 1;//"Special Teams"
			mPositionWeightsInputMap.Add("RCB", newWeightInput);
			mPositionWeightsMap["RCB"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["RCB"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.938;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 7;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 1;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("SS", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 11; //"Man-to-Man Defense (Ft50)",
			newWeightInput.Attributes[2] = 11; //"Zone Defense (Ft50PD50)",
			newWeightInput.Attributes[3] = 11; //"Bump and Run Defense (Bp50)",
			newWeightInput.Attributes[4] = 9;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[5] = 3; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[6] = 7;  //"Interceptions (PD50)",
			newWeightInput.Attributes[7] = 1;  //"Punt Returns (Bj50)",
			newWeightInput.Attributes[8] = 1;  //"Kick Returns (Bj50)",
			newWeightInput.Attributes[9] = 4;  //"Endurance",
			newWeightInput.Attributes[10] = 1;//"Special Teams"
			mPositionWeightsInputMap.Add("SS", newWeightInput);
			mPositionWeightsMap["SS"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["SS"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.938;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 7;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 20;
			newWeightInput.BroadJump = 1;
			newWeightInput.PositionDrill = 10;
			InitializeAttributeWeights("FS", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Run Defense (Ag100)",
			newWeightInput.Attributes[1] = 11; //"Man-to-Man Defense (Ft50)",
			newWeightInput.Attributes[2] = 11; //"Zone Defense (Ft50PD50)",
			newWeightInput.Attributes[3] = 11; //"Bump and Run Defense (Bp50)",
			newWeightInput.Attributes[4] = 9;  //"Play Diagnosis (So50)",
			newWeightInput.Attributes[5] = 3; //"Punishing Hitter (Bp50)",
			newWeightInput.Attributes[6] = 7;  //"Interceptions (PD50)",
			newWeightInput.Attributes[7] = 1;  //"Punt Returns (Bj50)",
			newWeightInput.Attributes[8] = 1;  //"Kick Returns (Bj50)",
			newWeightInput.Attributes[9] = 4;  //"Endurance",
			newWeightInput.Attributes[10] = 1;//"Special Teams"
			mPositionWeightsInputMap.Add("FS", newWeightInput);
			mPositionWeightsMap["FS"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["FS"] = BuildNoCombineWeightFromInput(newWeightInput);

			newWeightInput = new DataReader.DraftWeights.PositionWeightInputs();
			newWeightInput.Weight = 0.86;
			newWeightInput.Dash = 10;
			newWeightInput.Solecismic = 10;
			newWeightInput.Bench = 10;
			newWeightInput.Agility = 10;
			newWeightInput.BroadJump = 10;
			newWeightInput.PositionDrill = 0;
			InitializeAttributeWeights("LS", newWeightInput);
			newWeightInput.Attributes[0] = 25; //"Long Snapping"
			mPositionWeightsInputMap.Add("LS", newWeightInput);
			mPositionWeightsMap["LS"] = BuildWeightFromInput(newWeightInput);
			mNoCombinePositionWeightsMap["LS"] = BuildNoCombineWeightFromInput(newWeightInput);
		}

		void InitializeAttributeWeights(string position, DataReader.DraftWeights.PositionWeightInputs weightInput)
		{
			string positionGroup = mFOFData.PositionToPositionGroupMap[position];
			int attributeCount = mFOFData.PositionGroupAttributes[positionGroup].Length;
			for (int i = 0; i < MaxAttributeCounts; ++i)
			{
				if (i < attributeCount)
				{
					weightInput.Attributes[i] = 10;
				}
				else
				{
					weightInput.Attributes[i] = 0;
				}
			}
		}

	}
}

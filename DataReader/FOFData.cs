using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataReader
{
	public class FOFData
	{
		public enum ScoutBars
		{
			Screen_Passes = 0,
			Short_Passes,
			Medium_Passes,
			Long_Passes,
			Deep_Passes,
			Third_Down_Passes,
			Run_Frequency,
			Future_1,
			Accuracy,
			Timing,
			Sense_Rush,			// 10
			Read_Defense,
			Two_Minute_Offense,
			Future_2,
			Future_3,
			Future_4,
			Future_5,
			Future_6,
			Breakaway_Speed,
			Power_Inside,
			Third_Down_Runs,	// 20
			Hole_Recognition,
			Elusiveness,
			Speed_Outside,
			Blitz_Pickup,
			Avoid_Drops,
			Get_Downfield,
			Route_Running,
			Third_Down_Receiving,
			Big_Play_Receiving,
			Courage,			// 30
			Adjust_to_Ball,
			Punt_Returns,
			Kick_Returns,
			Future_7,
			Run_Blocking,
			Pass_Blocking,
			Blocking_Strength,
			Punting_Power,
			Hang_Time,
			Directional_Punting,// 40
			Kickoff_Distance,
			Kickoff_Hang_Time,
			Kicking_Accuracy,
			Kicking_Power,
			Run_Defense,
			Pass_Rush_Technique,
			Man_to_Man_Defense,
			Zone_Defense,
			Bump_and_Run_Defense,
			Pass_Rush_Strength,	// 50
			Play_Diagnosis,
			Punishing_Hitter,
			Intercepting,
			Endurance,
			Special_Teams,
			Long_Snapping,
			Kick_Holding,

			Count
		}

		public enum CombineOrder
		{
			Dash,
			Solecismic,
			Bench,
			Agility,
			BroadJump,
			PositionDrill
		}

		public ushort[,] CombineThresholds =
		{
			{ 0,28,10,780,0,0 },
			{ 465,0,0,735,114,17 },
			{ 478,0,20,0,104,22 },
			{ 478,0,22,775,102,0 },
			{ 451,0,0,720,0,42 },
			{ 531,0,25,800,0,0 },
			{ 527,0,27,790,0,0 },
			{ 527,0,28,780,84,0 },
			{ 497,0,10,0,0,0 },
			{ 0,23,9,0,104,0 },
			{ 485,0,27,760,0,0 },
			{ 0,0,28,780,0,0 },
			{ 0,0,21,760,107,0 },
			{ 0,0,18,740,108,22 },
			{ 452,0,12,720,0,37 },
			{ 459,0,15,735,0,37 },
		};

		public double[,] CombineAverages =
		{
			{4.79,28.0,9.5,7.87,102.6,71.2},
			{4.67,20.7,14.1,7.35,115.3,17.4},
			{4.81,21.9,19.8,7.58,103.9,23.4},
			{4.84,23.3,20.5,7.95,103.0,28.0},
			{4.56,21.0,9.9,7.26,108.1,40.3},
			{5.35,29.6,22.6,8.12,88.4,0.0},
			{5.29,28.0,26.7,8.02,90.0,0.0},
			{5.32,27.3,26.8,7.91,92.3,0.0},
			{5.09,25.6,10.1,7.69,106.8,0.0},
			{5.15,27.0,9.1,7.67,103.7,0.0},
			{4.90,23.6,25.7,7.68,107.8,0.0},
			{5.14,22.4,27.4,7.92,97.3,0.0},
			{4.88,27.1,20.3,7.68,104.4,23.4},
			{4.75,25.1,16.8,7.46,108.6,23.6},
			{4.55,22.2,10.9,7.26,107.2,35.0},
			{4.62,28.8,14.2,7.42,99.1,35.0},
		};

		public double[,] CombineStandardDeviations =
		{
			{0.151,7.21,2.19,0.253,6.01,9.55},
			{0.093,6.45,3.44,0.151,5.14,6.30},
			{0.078,5.75,3.90,0.199,5.07,7.51},
			{0.097,6.81,4.22,0.358,6.69,10.54},
			{0.104,6.99,3.09,0.194,4.92,9.94},
			{0.133,9.87,4.82,0.220,6.69,0.00},
			{0.135,9.76,4.27,0.268,6.32,0.00},
			{0.166,10.24,5.07,0.268,6.66,0.00},
			{0.179,7.44,3.62,0.338,7.04,0.00},
			{0.136,7.47,2.87,0.337,6.40,0.00},
			{0.140,6.82,3.93,0.300,7.34,0.00},
			{0.143,6.54,3.71,0.331,9.43,0.00},
			{0.111,7.09,3.53,0.240,5.93,7.67},
			{0.119,7.13,3.49,0.199,5.86,7.72},
			{0.079,6.90,2.56,0.201,4.35,9.42},
			{0.091,7.59,2.97,0.224,4.31,9.41},
		};

		public ushort[,] CombineColors = 
		{
			{ 454,474,497,45,34,20,17,13,9,728,763,805,117,112,103,90,77,62 },
			{ 446,458,473,33,24,14,22,18,13,705,725,753,129,124,115,34,25,15 },
			{ 458,471,485,34,25,16,28,24,18,721,747,785,117,112,103,42,31,19 },
			{ 460,472,487,37,27,17,30,26,20,739,763,806,122,113,103,49,40,25 },
			{ 438,446,463,35,24,14,17,14,9,693,711,736,132,125,115,62,51,35 },
			{ 507,523,538,39,30,20,33,29,22,773,791,816,106,99,90,0,0,0 },
			{ 504,519,536,38,28,18,37,31,25,763,781,815,106,99,90,0,0,0 },
			{ 505,519,540,37,28,17,37,32,25,753,771,805,109,101,92,0,0,0 },
			{ 481,498,520,39,28,17,18,13,8,741,767,805,117,112,103,0,0,0 },
			{ 495,506,523,40,29,19,17,12,7,741,767,805,117,112,103,0,0,0 },
			{ 468,479,497,36,27,16,35,30,24,721,747,785,125,117,107,0,0,0 },
			{ 494,502,520,35,25,15,37,32,26,741,767,768,116,109,96,0,0,0 },
			{ 468,480,494,41,30,20,29,25,19,730,750,781,120,114,105,44,35,22 },
			{ 455,465,481,39,28,18,25,21,15,713,731,756,124,118,109,44,35,22 },
			{ 440,447,460,36,25,15,22,15,10,693,711,736,132,125,115,54,45,30 },
			{ 444,454,468,43,32,21,22,18,13,705,725,753,128,121,110,54,45,30 },
		};

		public string[] AbilityMap =
			{
				"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
			};

		public string[] AbilityIndexMap =
			{
				"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"2"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"3"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"4"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
				,"5"
			};

		public string[] DraftableMap =
			{
				"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"0"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
				,"1"
			};

        public class PositionSizeRanges
        {
            public int MinWeight;
            public int WellBelowAverageWeightCap;
            public int BelowAverageWeightCap;
            public int AverageWeightCap;
            public int AboveAverageWeightCap;
            public int WellAboveAverageWeightCap;

            public int MinHeight;
            // 2 inches to either side is Above/Below Average.
            // Everything else is Significantly Above/Below.
            public int AverageHeight;
        }

        private Dictionary<string, int> m_PositionGroupOrderMap = new Dictionary<string, int>();
		private Dictionary<string, double> m_PositionWeightsInputMap = new Dictionary<string, double>();
		private Dictionary<string, int[]> m_PositionGroupAttributes;
		private Dictionary<string, string> mPositionToPositionGroupMap;
        private Dictionary<string, PositionSizeRanges> mPositionSizeRangesMap = new Dictionary<string, PositionSizeRanges>();

        public Dictionary<string, int> PositionGroupOrderMap { get { return m_PositionGroupOrderMap; } }
		public Dictionary<string, double> PositionWeightsInputMap { get { return m_PositionWeightsInputMap; } }
		public Dictionary<string, int[]> PositionGroupAttributes { get { return m_PositionGroupAttributes; } }
		public Dictionary<string, string> PositionToPositionGroupMap { get { return mPositionToPositionGroupMap; } }
        public Dictionary<string, PositionSizeRanges> PositionSizeRangesMap { get { return mPositionSizeRangesMap; } }

        public enum SizeBands
        {
            TooSmall,
            WellBelowAverage,
            BelowAverage,
            Average,
            AboveAverage,
            WellAboveAverage,
            TooBig
        }

        private string[] kHeightBands =
            {
                "too short"
                ,"well below average"
                ,"below average"
                ,"about average"
                ,"above average"
                ,"well above average"
                ,"too tall"
            };
        private string[] kWeightBands =
            {
                "too light"
                ,"well below average"
                ,"below average"
                ,"about average"
                ,"above average"
                ,"well above average"
                ,"too heavy"
            };

        public string GetHeightBandString(string playerPosition, int height)
        {
            return kHeightBands[(int)GetHeightBandIndex(playerPosition, height)];
        }

        public string GetWeightBandString(string playerPosition, int weight)
        {
            return kWeightBands[(int)GetWeightBandIndex(playerPosition, weight)];
        }

        public SizeBands GetHeightBandIndex(string playerPosition, int height)
        {
            PositionSizeRanges posRange = mPositionSizeRangesMap[playerPosition];
            if (height < posRange.MinHeight)
            {
                return SizeBands.TooSmall;
            }
            else if (height < (posRange.AverageHeight - 2))
            {
                return SizeBands.WellBelowAverage;
            }
            else if (height < posRange.AverageHeight)
            {
                return SizeBands.BelowAverage;
            }
            else if (height == posRange.AverageHeight)
            {
                return SizeBands.Average;
            }
            else if (height > (posRange.AverageHeight + 2))
            {
                return SizeBands.WellAboveAverage;
            }
            else if (height > posRange.AverageHeight)
            {
                return SizeBands.AboveAverage;
            }
            else
            {
                return SizeBands.TooBig;
            }
        }

        public SizeBands GetWeightBandIndex(string playerPosition, int weight)
        {
            PositionSizeRanges posRange = mPositionSizeRangesMap[playerPosition];
            if (weight <= posRange.MinWeight)
            {
                return SizeBands.TooSmall;
            }
            else if (weight <= posRange.WellBelowAverageWeightCap)
            {
                return SizeBands.WellBelowAverage;
            }
            else if (weight <= posRange.BelowAverageWeightCap)
            {
                return SizeBands.BelowAverage;
            }
            else if (weight <= posRange.AverageWeightCap)
            {
                return SizeBands.Average;
            }
            else if (weight <= posRange.AboveAverageWeightCap)
            {
                return SizeBands.AboveAverage;
            }
            else if (weight <= posRange.WellAboveAverageWeightCap)
            {
                return SizeBands.WellAboveAverage;
            }
            else
            {
                return SizeBands.TooBig;
            }
        }

        public FOFData()
		{
			mPositionToPositionGroupMap = new Dictionary<string, string>();
			mPositionToPositionGroupMap["QB"] = "QB";
			mPositionToPositionGroupMap["RB"] = "RB";
			mPositionToPositionGroupMap["FB"] = "FB";
			mPositionToPositionGroupMap["FL"] = "WR";
			mPositionToPositionGroupMap["SE"] = "WR";
			mPositionToPositionGroupMap["TE"] = "TE";
			mPositionToPositionGroupMap["LT"] = "T";
			mPositionToPositionGroupMap["RT"] = "T";
			mPositionToPositionGroupMap["LG"] = "G";
			mPositionToPositionGroupMap["RG"] = "G";
			mPositionToPositionGroupMap["C"] = "C";
			mPositionToPositionGroupMap["LDE"] = "DE";
			mPositionToPositionGroupMap["RDE"] = "DE";
			mPositionToPositionGroupMap["LDT"] = "DT";
			mPositionToPositionGroupMap["RDT"] = "DT";
			mPositionToPositionGroupMap["NT"] = "DT";
			mPositionToPositionGroupMap["WLB"] = "OLB";
			mPositionToPositionGroupMap["SLB"] = "OLB";
			mPositionToPositionGroupMap["WILB"] = "ILB";
			mPositionToPositionGroupMap["SILB"] = "ILB";
			mPositionToPositionGroupMap["MLB"] = "ILB";
			mPositionToPositionGroupMap["LCB"] = "CB";
			mPositionToPositionGroupMap["RCB"] = "CB";
			mPositionToPositionGroupMap["SS"] = "S";
			mPositionToPositionGroupMap["FS"] = "S";
			mPositionToPositionGroupMap["P"] = "P";
			mPositionToPositionGroupMap["K"] = "K";
			mPositionToPositionGroupMap["LS"] = "LS";

			int positionOrder = 0;
			m_PositionGroupOrderMap.Add("QB", positionOrder++);
			m_PositionGroupOrderMap.Add("RB", positionOrder++);
			m_PositionGroupOrderMap.Add("FB", positionOrder++);
			m_PositionGroupOrderMap.Add("TE", positionOrder++);
			m_PositionGroupOrderMap.Add("WR", positionOrder++);
			m_PositionGroupOrderMap.Add("C", positionOrder++);
			m_PositionGroupOrderMap.Add("G", positionOrder++);
			m_PositionGroupOrderMap.Add("T", positionOrder++);
			m_PositionGroupOrderMap.Add("P", positionOrder++);
			m_PositionGroupOrderMap.Add("K", positionOrder++);
			m_PositionGroupOrderMap.Add("DE", positionOrder++);
			m_PositionGroupOrderMap.Add("DT", positionOrder++);
			m_PositionGroupOrderMap.Add("ILB", positionOrder++);
			m_PositionGroupOrderMap.Add("OLB", positionOrder++);
			m_PositionGroupOrderMap.Add("CB", positionOrder++);
			m_PositionGroupOrderMap.Add("S", positionOrder++);
			m_PositionGroupOrderMap.Add("LS", positionOrder++);

			m_PositionWeightsInputMap["QB"] = 1.137;
			m_PositionWeightsInputMap["RB"] = 1.058;
			m_PositionWeightsInputMap["FB"] = 0.805;
			m_PositionWeightsInputMap["TE"] = 0.867;
			m_PositionWeightsInputMap["WR"] = 1.036;
			m_PositionWeightsInputMap["C"] = 0.856;
			m_PositionWeightsInputMap["G"] = 0.945;
			m_PositionWeightsInputMap["T"] = 1.095;
			m_PositionWeightsInputMap["P"] = 0.529;
			m_PositionWeightsInputMap["K"] = 0.591;
			m_PositionWeightsInputMap["DE"] = 1.095;
			m_PositionWeightsInputMap["DT"] = 1.076;
			m_PositionWeightsInputMap["ILB"] = 0.971;
			m_PositionWeightsInputMap["OLB"] = 0.955;
			m_PositionWeightsInputMap["CB"] = 1.027;
			m_PositionWeightsInputMap["S"] = 0.938;
			m_PositionWeightsInputMap["LS"] = 0.2;

			m_PositionGroupAttributes = new Dictionary<string, int[]>();

			// QB
			int[] attributeIndices = new int[]
			{
				(int)ScoutBars.Screen_Passes,
				(int)ScoutBars.Short_Passes,
				(int)ScoutBars.Medium_Passes,
				(int)ScoutBars.Long_Passes,
				(int)ScoutBars.Deep_Passes,
				(int)ScoutBars.Third_Down_Passes,
				(int)ScoutBars.Accuracy,
				(int)ScoutBars.Timing,
				(int)ScoutBars.Sense_Rush,
				(int)ScoutBars.Read_Defense,
				(int)ScoutBars.Two_Minute_Offense,
				(int)ScoutBars.Run_Frequency,
				(int)ScoutBars.Kick_Holding
			};
			m_PositionGroupAttributes.Add("QB", attributeIndices);

			// RB
			attributeIndices = new int[]
			{
				(int)ScoutBars.Breakaway_Speed,
				(int)ScoutBars.Power_Inside,
				(int)ScoutBars.Third_Down_Runs,
				(int)ScoutBars.Hole_Recognition,
				(int)ScoutBars.Elusiveness,
				(int)ScoutBars.Speed_Outside,
				(int)ScoutBars.Blitz_Pickup,
				(int)ScoutBars.Avoid_Drops,
				(int)ScoutBars.Get_Downfield,
				(int)ScoutBars.Route_Running,
				(int)ScoutBars.Third_Down_Receiving,
				(int)ScoutBars.Punt_Returns,
				(int)ScoutBars.Kick_Returns,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("RB", attributeIndices);

			// FB
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Blocking,
				(int)ScoutBars.Pass_Blocking,
				(int)ScoutBars.Blocking_Strength,
				(int)ScoutBars.Power_Inside,
				(int)ScoutBars.Third_Down_Runs,
				(int)ScoutBars.Hole_Recognition,
				(int)ScoutBars.Blitz_Pickup,
				(int)ScoutBars.Avoid_Drops,
				(int)ScoutBars.Route_Running,
				(int)ScoutBars.Third_Down_Receiving,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("FB", attributeIndices);

			// TE
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Blocking,
				(int)ScoutBars.Pass_Blocking,
				(int)ScoutBars.Blocking_Strength,
				(int)ScoutBars.Avoid_Drops,
				(int)ScoutBars.Get_Downfield,
				(int)ScoutBars.Route_Running,
				(int)ScoutBars.Third_Down_Receiving,
				(int)ScoutBars.Big_Play_Receiving,
				(int)ScoutBars.Courage,
				(int)ScoutBars.Adjust_to_Ball,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams,
				(int)ScoutBars.Long_Snapping
			};
			m_PositionGroupAttributes.Add("TE", attributeIndices);

			// WR
			attributeIndices = new int[]
			{
				(int)ScoutBars.Avoid_Drops,
				(int)ScoutBars.Get_Downfield,
				(int)ScoutBars.Route_Running,
				(int)ScoutBars.Third_Down_Receiving,
				(int)ScoutBars.Big_Play_Receiving,
				(int)ScoutBars.Courage,
				(int)ScoutBars.Adjust_to_Ball,
				(int)ScoutBars.Punt_Returns,
				(int)ScoutBars.Kick_Returns,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("WR", attributeIndices);

			// C
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Blocking,
				(int)ScoutBars.Pass_Blocking,
				(int)ScoutBars.Blocking_Strength,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Long_Snapping
			};
			m_PositionGroupAttributes.Add("C", attributeIndices);

			// G
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Blocking,
				(int)ScoutBars.Pass_Blocking,
				(int)ScoutBars.Blocking_Strength,
				(int)ScoutBars.Endurance
			};
			m_PositionGroupAttributes.Add("G", attributeIndices);

			// T
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Blocking,
				(int)ScoutBars.Pass_Blocking,
				(int)ScoutBars.Blocking_Strength,
				(int)ScoutBars.Endurance
			};
			m_PositionGroupAttributes.Add("T", attributeIndices);

			// P
			attributeIndices = new int[]
			{
				(int)ScoutBars.Punting_Power,
				(int)ScoutBars.Hang_Time,
				(int)ScoutBars.Directional_Punting,
				(int)ScoutBars.Kick_Holding
			};
			m_PositionGroupAttributes.Add("P", attributeIndices);

			// K
			attributeIndices = new int[]
			{
				(int)ScoutBars.Kicking_Accuracy,
				(int)ScoutBars.Kicking_Power,
				(int)ScoutBars.Kickoff_Distance,
				(int)ScoutBars.Kickoff_Hang_Time
			};
			m_PositionGroupAttributes.Add("K", attributeIndices);

			// DE
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Defense,
				(int)ScoutBars.Pass_Rush_Technique,
				(int)ScoutBars.Pass_Rush_Strength,
				(int)ScoutBars.Play_Diagnosis,
				(int)ScoutBars.Punishing_Hitter,
				(int)ScoutBars.Endurance
			};
			m_PositionGroupAttributes.Add("DE", attributeIndices);

			// DT
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Defense,
				(int)ScoutBars.Pass_Rush_Technique,
				(int)ScoutBars.Pass_Rush_Strength,
				(int)ScoutBars.Play_Diagnosis,
				(int)ScoutBars.Punishing_Hitter,
				(int)ScoutBars.Endurance
			};
			m_PositionGroupAttributes.Add("DT", attributeIndices);

			// ILB
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Defense,
				(int)ScoutBars.Pass_Rush_Technique,
				(int)ScoutBars.Pass_Rush_Strength,
				(int)ScoutBars.Man_to_Man_Defense,
				(int)ScoutBars.Zone_Defense,
				(int)ScoutBars.Bump_and_Run_Defense,
				(int)ScoutBars.Play_Diagnosis,
				(int)ScoutBars.Punishing_Hitter,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("ILB", attributeIndices);

			// OLB
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Defense,
				(int)ScoutBars.Pass_Rush_Technique,
				(int)ScoutBars.Pass_Rush_Strength,
				(int)ScoutBars.Man_to_Man_Defense,
				(int)ScoutBars.Zone_Defense,
				(int)ScoutBars.Bump_and_Run_Defense,
				(int)ScoutBars.Play_Diagnosis,
				(int)ScoutBars.Punishing_Hitter,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("OLB", attributeIndices);

			// CB
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Defense,
				(int)ScoutBars.Man_to_Man_Defense,
				(int)ScoutBars.Zone_Defense,
				(int)ScoutBars.Bump_and_Run_Defense,
				(int)ScoutBars.Play_Diagnosis,
				(int)ScoutBars.Punishing_Hitter,
				(int)ScoutBars.Intercepting,
				(int)ScoutBars.Punt_Returns,
				(int)ScoutBars.Kick_Returns,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("CB", attributeIndices);

			// S
			attributeIndices = new int[]
			{
				(int)ScoutBars.Run_Defense,
				(int)ScoutBars.Man_to_Man_Defense,
				(int)ScoutBars.Zone_Defense,
				(int)ScoutBars.Bump_and_Run_Defense,
				(int)ScoutBars.Play_Diagnosis,
				(int)ScoutBars.Punishing_Hitter,
				(int)ScoutBars.Intercepting,
				(int)ScoutBars.Punt_Returns,
				(int)ScoutBars.Kick_Returns,
				(int)ScoutBars.Endurance,
				(int)ScoutBars.Special_Teams
			};
			m_PositionGroupAttributes.Add("S", attributeIndices);

			// LS
			attributeIndices = new int[]
			{
				(int)ScoutBars.Long_Snapping
			};
			m_PositionGroupAttributes.Add("LS", attributeIndices);

            mPositionSizeRangesMap = new Dictionary<string, PositionSizeRanges>();
            PositionSizeRanges newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 210;
            newRanges.BelowAverageWeightCap = 216;
            newRanges.AverageWeightCap = 223;
            newRanges.AboveAverageWeightCap = 232;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 74;
            mPositionSizeRangesMap["QB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 208;
            newRanges.BelowAverageWeightCap = 214;
            newRanges.AverageWeightCap = 221;
            newRanges.AboveAverageWeightCap = 230;
            newRanges.WellAboveAverageWeightCap = 265;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 71;
            mPositionSizeRangesMap["RB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 210;
            newRanges.WellBelowAverageWeightCap = 232;
            newRanges.BelowAverageWeightCap = 239;
            newRanges.AverageWeightCap = 246;
            newRanges.AboveAverageWeightCap = 256;
            newRanges.WellAboveAverageWeightCap = 189;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 72;
            mPositionSizeRangesMap["FB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 210;
            newRanges.WellBelowAverageWeightCap = 245;
            newRanges.BelowAverageWeightCap = 252;
            newRanges.AverageWeightCap = 260;
            newRanges.AboveAverageWeightCap = 270;
            newRanges.WellAboveAverageWeightCap = 289;
            newRanges.MinHeight = 74;
            newRanges.AverageHeight = 76;
            mPositionSizeRangesMap["TE"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 188;
            newRanges.BelowAverageWeightCap = 195;
            newRanges.AverageWeightCap = 200;
            newRanges.AboveAverageWeightCap = 208;
            newRanges.WellAboveAverageWeightCap = 235;
            newRanges.MinHeight = 70;
            newRanges.AverageHeight = 72;
            mPositionSizeRangesMap["FL"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 187;
            newRanges.BelowAverageWeightCap = 193;
            newRanges.AverageWeightCap = 198;
            newRanges.AboveAverageWeightCap = 207;
            newRanges.WellAboveAverageWeightCap = 235;
            newRanges.MinHeight = 70;
            newRanges.AverageHeight = 72;
            mPositionSizeRangesMap["SE"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 276;
            newRanges.WellBelowAverageWeightCap = 298;
            newRanges.BelowAverageWeightCap = 307;
            newRanges.AverageWeightCap = 317;
            newRanges.AboveAverageWeightCap = 329;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 77;
            mPositionSizeRangesMap["LT"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 270;
            newRanges.WellBelowAverageWeightCap = 298;
            newRanges.BelowAverageWeightCap = 305;
            newRanges.AverageWeightCap = 315;
            newRanges.AboveAverageWeightCap = 327;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 76;
            mPositionSizeRangesMap["LG"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 265;
            newRanges.WellBelowAverageWeightCap = 279;
            newRanges.BelowAverageWeightCap = 288;
            newRanges.AverageWeightCap = 296;
            newRanges.AboveAverageWeightCap = 308;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 75;
            mPositionSizeRangesMap["C"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 270;
            newRanges.WellBelowAverageWeightCap = 301;
            newRanges.BelowAverageWeightCap = 313;
            newRanges.AverageWeightCap = 320;
            newRanges.AboveAverageWeightCap = 332;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 75;
            mPositionSizeRangesMap["RG"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 276;
            newRanges.WellBelowAverageWeightCap = 306;
            newRanges.BelowAverageWeightCap = 315;
            newRanges.AverageWeightCap = 325;
            newRanges.AboveAverageWeightCap = 336;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 77;
            mPositionSizeRangesMap["RT"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 201;
            newRanges.BelowAverageWeightCap = 207;
            newRanges.AverageWeightCap = 214;
            newRanges.AboveAverageWeightCap = 222;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 74;
            mPositionSizeRangesMap["P"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 191;
            newRanges.BelowAverageWeightCap = 197;
            newRanges.AverageWeightCap = 203;
            newRanges.AboveAverageWeightCap = 211;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 72;
            mPositionSizeRangesMap["K"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 255;
            newRanges.WellBelowAverageWeightCap = 266;
            newRanges.BelowAverageWeightCap = 273;
            newRanges.AverageWeightCap = 281;
            newRanges.AboveAverageWeightCap = 292;
            newRanges.WellAboveAverageWeightCap = 314;
            newRanges.MinHeight = 72;
            newRanges.AverageHeight = 76;
            mPositionSizeRangesMap["LDE"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 280;
            newRanges.WellBelowAverageWeightCap = 287;
            newRanges.BelowAverageWeightCap = 296;
            newRanges.AverageWeightCap = 305;
            newRanges.AboveAverageWeightCap = 317;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 72;
            newRanges.AverageHeight = 75;
            mPositionSizeRangesMap["LDT"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 280;
            newRanges.WellBelowAverageWeightCap = 289;
            newRanges.BelowAverageWeightCap = 298;
            newRanges.AverageWeightCap = 309;
            newRanges.AboveAverageWeightCap = 320;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 72;
            newRanges.AverageHeight = 75;
            mPositionSizeRangesMap["NT"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 280;
            newRanges.WellBelowAverageWeightCap = 287;
            newRanges.BelowAverageWeightCap = 296;
            newRanges.AverageWeightCap = 304;
            newRanges.AboveAverageWeightCap = 316;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 72;
            newRanges.AverageHeight = 75;
            mPositionSizeRangesMap["RDT"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 255;
            newRanges.WellBelowAverageWeightCap = 261;
            newRanges.BelowAverageWeightCap = 269;
            newRanges.AverageWeightCap = 277;
            newRanges.AboveAverageWeightCap = 288;
            newRanges.WellAboveAverageWeightCap = 314;
            newRanges.MinHeight = 73;
            newRanges.AverageHeight = 76;
            mPositionSizeRangesMap["RDE"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 220;
            newRanges.WellBelowAverageWeightCap = 230;
            newRanges.BelowAverageWeightCap = 237;
            newRanges.AverageWeightCap = 244;
            newRanges.AboveAverageWeightCap = 254;
            newRanges.WellAboveAverageWeightCap = 275;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 74;
            mPositionSizeRangesMap["SLB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 225;
            newRanges.WellBelowAverageWeightCap = 233;
            newRanges.BelowAverageWeightCap = 240;
            newRanges.AverageWeightCap = 247;
            newRanges.AboveAverageWeightCap = 257;
            newRanges.WellAboveAverageWeightCap = 280;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 73;
            mPositionSizeRangesMap["SILB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 225;
            newRanges.WellBelowAverageWeightCap = 238;
            newRanges.BelowAverageWeightCap = 242;
            newRanges.AverageWeightCap = 249;
            newRanges.AboveAverageWeightCap = 259;
            newRanges.WellAboveAverageWeightCap = 280;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 73;
            mPositionSizeRangesMap["MLB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 225;
            newRanges.WellBelowAverageWeightCap = 239;
            newRanges.BelowAverageWeightCap = 243;
            newRanges.AverageWeightCap = 250;
            newRanges.AboveAverageWeightCap = 260;
            newRanges.WellAboveAverageWeightCap = 280;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 73;
            mPositionSizeRangesMap["WILB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 220;
            newRanges.WellBelowAverageWeightCap = 227;
            newRanges.BelowAverageWeightCap = 234;
            newRanges.AverageWeightCap = 241;
            newRanges.AboveAverageWeightCap = 251;
            newRanges.WellAboveAverageWeightCap = 270;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 73;
            mPositionSizeRangesMap["WLB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 182;
            newRanges.BelowAverageWeightCap = 188;
            newRanges.AverageWeightCap = 193;
            newRanges.AboveAverageWeightCap = 201;
            newRanges.WellAboveAverageWeightCap = 225;
            newRanges.MinHeight = 69;
            newRanges.AverageHeight = 71;
            mPositionSizeRangesMap["LCB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 183;
            newRanges.BelowAverageWeightCap = 189;
            newRanges.AverageWeightCap = 194;
            newRanges.AboveAverageWeightCap = 202;
            newRanges.WellAboveAverageWeightCap = 225;
            newRanges.MinHeight = 69;
            newRanges.AverageHeight = 71;
            mPositionSizeRangesMap["RCB"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 197;
            newRanges.BelowAverageWeightCap = 203;
            newRanges.AverageWeightCap = 210;
            newRanges.AboveAverageWeightCap = 218;
            newRanges.WellAboveAverageWeightCap = 235;
            newRanges.MinHeight = 70;
            newRanges.AverageHeight = 72;
            mPositionSizeRangesMap["SS"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 0;
            newRanges.WellBelowAverageWeightCap = 195;
            newRanges.BelowAverageWeightCap = 201;
            newRanges.AverageWeightCap = 210;
            newRanges.AboveAverageWeightCap = 218;
            newRanges.WellAboveAverageWeightCap = 235;
            newRanges.MinHeight = 70;
            newRanges.AverageHeight = 72;
            mPositionSizeRangesMap["FS"] = newRanges;

            newRanges = new PositionSizeRanges();
            newRanges.MinWeight = 210;
            newRanges.WellBelowAverageWeightCap = 245;
            newRanges.BelowAverageWeightCap = 252;
            newRanges.AverageWeightCap = 278;
            newRanges.AboveAverageWeightCap = 308;
            newRanges.WellAboveAverageWeightCap = 999;
            newRanges.MinHeight = 0;
            newRanges.AverageHeight = 75;
            mPositionSizeRangesMap["LS"] = newRanges;
        }
    }
}

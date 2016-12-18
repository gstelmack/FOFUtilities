using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace PlayerTracker
{
	public class ReportGenerator
	{
		ProgressData m_ProgressData;

		public enum ReportType
		{
			Team,
			DraftYear,
			Position,
			Current,
			All,
			DraftAnalysis
		};

		public ReportType Type = ReportType.All;
		public byte TeamIndex = 99;
		public ushort DraftYear = ushort.MaxValue;
		public int StageIndex = -1;
		public string Position = "";
        public DataReader.FOFData.DefensiveFront DefensiveFront = DataReader.FOFData.DefensiveFront.True34;
		private DataReader.FOFData m_FOFData = new DataReader.FOFData();
		private DataReader.DraftWeights m_Weights;

		private const string kWeightsDataFileName = "DraftAnalyzer.weights";

		public ReportGenerator(ProgressData progressData)
		{
			m_ProgressData = progressData;

			m_Weights = new DataReader.DraftWeights();
			string dataFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kWeightsDataFileName);
			if (System.IO.File.Exists(dataFileName))
			{
				m_Weights.LoadData(dataFileName);
			}
		}

		public List<PlayerListData> Generate()
		{
			List<PlayerListData> playerList = new List<PlayerListData>();

			int teamIndex = -1;
			switch (this.Type)
			{
				case ReportType.Team:
					teamIndex = TeamIndex;
					if (teamIndex >= 32)
					{
						teamIndex = 99;
					}
					break;
			}
			foreach (PlayerRecord rec in m_ProgressData.PlayerRecords.Values)
			{
				if (rec.Entries.Count == 0)
				{
					continue;
				}

				int curOvr = -1;
				int curFut = -1;
				int peakOvr = -1;
				int peakFut = -1;
				int startOvr = -1;
				int startFut = -1;
				int lastOvr = 0;
				int lastFut = 0;
                ushort peakWeight = 0;
                ushort startWeight = 0;

				int lastStageIndex = rec.Entries[rec.Entries.Count - 1].StageIndex;

				byte[] peakBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
				for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
				{
					peakBars[i] = 0;
				}

				foreach (PlayerEntryRecord entry in rec.Entries)
				{
					if (startOvr < 0 && entry.CurOverall != Byte.MaxValue)
					{
						startOvr = entry.CurOverall;
						startFut = entry.FutOverall;
                        startWeight = entry.Weight;
					}
					if (entry.CurOverall > peakOvr && entry.CurOverall != Byte.MaxValue)
					{
						peakOvr = entry.CurOverall;
						peakFut = entry.FutOverall;
                        peakWeight = entry.Weight;
					}
					for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
					{
						peakBars[i] = Math.Max(peakBars[i],entry.CurBars[i]);
					}
					bool useEntry = false;
					if (entry.CurOverall != Byte.MaxValue)
					{
						switch (this.Type)
						{
							case ReportType.Team:
								if (entry.StageIndex == StageIndex && entry.Team == teamIndex)
								{
									useEntry = true;
								}
								break;
							case ReportType.DraftYear:
								if (rec.Draft_Year == DraftYear && entry.StageIndex == lastStageIndex)
								{
									useEntry = true;
								}
								break;
							case ReportType.Position:
								if (rec.Entries[0].Position_Group == Position && entry.StageIndex == lastStageIndex)
								{
									useEntry = true;
								}
								break;
							case ReportType.All:
								if (entry.StageIndex == lastStageIndex)
								{
									useEntry = true;
								}
								break;
							case ReportType.Current:
								if (entry.StageIndex == StageIndex)
								{
									useEntry = true;
								}
								break;
							case ReportType.DraftAnalysis:
								if (entry.StageIndex == lastStageIndex
									&& rec.Solecismic != Byte.MaxValue
									&& rec.Draft_Class != DraftYear
									)
								{
									useEntry = true;
								}
								break;
						}
					}
					if (useEntry)
					{
						curOvr = entry.CurOverall;
						curFut = entry.FutOverall;
						PlayerListData listData = new PlayerListData();
						listData.Name = rec.Last_Name + ", " + rec.First_Name;
                        listData.Pos = entry.Position;
						listData.PosGrp = m_FOFData.PositionToPositionGroupMap[entry.Position];
						listData.PosOrder = m_FOFData.PositionGroupOrderMap[listData.PosGrp];
						listData.PlayerRecord = rec;
						listData.PresentCur = (byte)curOvr;
						listData.PresentFut = (byte)curFut;
						listData.PeakCur = (byte)peakOvr;
						listData.PeakFut = (byte)peakFut;
						listData.StartingCur = (byte)startOvr;
						listData.StartingFut = (byte)startFut;
						listData.PreviousCur = (byte)lastOvr;
						listData.PreviousFut = (byte)lastFut;
						listData.ExpYears = entry.Experience;
                        listData.AgeYears = (byte)(m_ProgressData.StageRecords[lastStageIndex].Season - rec.Year_Born);
                        listData.ID = rec.Player_ID;
						listData.OverallChange = curFut - startFut;
						listData.RecentChange = curFut - lastFut;
						listData.TeamIndex = entry.Team;
                        listData.PeakWeight = peakWeight;
                        listData.CurWeight = entry.Weight;
                        listData.StartWeight = startWeight;
                        listData.CombineSolecismic = entry.Solecismic;
                        listData.CombineAgility = entry.Agility;
                        listData.CombineBench = entry.Strength;
                        listData.CombineBroadJump = entry.Jump;
                        listData.CombineForty = entry.Dash;
                        listData.CombinePositionDrill = entry.Position_Specific;
						listData.PlayerRecord = rec;
						for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
						{
							listData.CurrentBars[i] = entry.CurBars[i];
							listData.FutureBars[i] = entry.FutBars[i];
							listData.PeakBars[i] = peakBars[i];
						}
						CalculatePositionRating(listData);
                        ColorSize(listData);
						playerList.Add(listData);
					}
					if (entry.CurOverall != Byte.MaxValue)
					{
						lastOvr = entry.CurOverall;
						lastFut = entry.FutOverall;
    				}
				}
			}

			return playerList;
		}

        private void ColorSize(PlayerListData data)
        {
            var heightDiff = m_FOFData.GetHeightDifference(data.Pos, data.PlayerRecord.Height);
            if (heightDiff <= -2)
            {
                data.HeightForeground = Brushes.DarkSeaGreen;
            }
            else if (heightDiff > 3)
            {
                data.HeightForeground = Brushes.Red;
            }
            else if (heightDiff > 1)
            {
                data.HeightForeground = Brushes.Blue;
            }

            var weightDiff = Math.Abs(m_FOFData.GetWeightDifference(data.Pos, data.CurWeight, DefensiveFront));
            if (weightDiff < 4)
            {
                data.WeightForeground = Brushes.Red;
            }
            else if (weightDiff < 8)
            {
                data.WeightForeground = Brushes.Blue;
            }
            else if (weightDiff < 15)
            {
                data.WeightForeground = Brushes.Black;
            }
            else
            {
                data.WeightForeground = Brushes.DarkSeaGreen;
            }
        }
        private void ColorCombine(ushort value, int combineIndex, int positionIndex, out SolidColorBrush foreground)
        {
            foreground = Brushes.Black;
            int redIndex = combineIndex * 3;
            int blueIndex = redIndex + 1;
            int greenIndex = blueIndex + 1;
            if (positionIndex >= m_FOFData.CombineThresholds.GetLength(0))
            {
                foreground = Brushes.Black;
            }
            else if (combineIndex == 0 || combineIndex == 3)
            {
                if (value <= m_FOFData.CombineColors[positionIndex, redIndex])
                {
                    foreground = Brushes.Red;
                }
                else if (value <= m_FOFData.CombineColors[positionIndex, blueIndex])
                {
                    foreground = Brushes.Blue;
                }
                else if (value >= m_FOFData.CombineColors[positionIndex, greenIndex])
                {
                    foreground = Brushes.DarkSeaGreen;
                }
                else
                {
                    foreground = Brushes.Black;
                }
            }
            else
            {
                if (value >= m_FOFData.CombineColors[positionIndex, redIndex])
                {
                    foreground = Brushes.Red;
                }
                else if (value >= m_FOFData.CombineColors[positionIndex, blueIndex])
                {
                    foreground = Brushes.Blue;
                }
                else if (value <= m_FOFData.CombineColors[positionIndex, greenIndex])
                {
                    foreground = Brushes.DarkSeaGreen;
                }
                else
                {
                    foreground = Brushes.Black;
                }
            }
        }

        private void CalculateCombineRating(PlayerListData data, string position)
		{
			PlayerRecord rec = m_ProgressData.PlayerRecords[data.ID];

			data.SolecismicRating = 0.0;
			data.FortyRating = 0.0;
			data.AgilityRating = 0.0;
			data.BenchRating = 0.0;
			data.BroadJumpRating = 0.0;
			data.PositionDrillRating = 0.0;

            SolidColorBrush foreground;
			string positionGroup = m_FOFData.PositionToPositionGroupMap[position];

			int positionIndex = m_FOFData.PositionGroupOrderMap[positionGroup];
            ColorCombine(data.CombineSolecismic, (int)DataReader.FOFData.CombineOrder.Solecismic, positionIndex, out foreground);
            data.SolecismicForeground = foreground;
            ColorCombine(data.CombineAgility, (int)DataReader.FOFData.CombineOrder.Agility, positionIndex, out foreground);
            data.AgilityForeground = foreground;
            ColorCombine(data.CombineBench, (int)DataReader.FOFData.CombineOrder.Bench, positionIndex, out foreground);
            data.BenchForeground = foreground;
            ColorCombine(data.CombineBroadJump, (int)DataReader.FOFData.CombineOrder.BroadJump, positionIndex, out foreground);
            data.BroadJumpForeground = foreground;
            ColorCombine(data.CombineForty, (int)DataReader.FOFData.CombineOrder.Dash, positionIndex, out foreground);
            data.FortyForeground = foreground;
            ColorCombine(data.CombinePositionDrill, (int)DataReader.FOFData.CombineOrder.PositionDrill, positionIndex, out foreground);
            data.PositionDrillForeground = foreground;

            if (rec.Solecismic == Byte.MaxValue)
			{
				// Do nothing, no combines, leave at 0
			}
			else if (rec.Solecismic != 0 && rec.Dash != 0.0 && rec.Strength != 0 && rec.Agility != 0.0 && rec.Jump != 0)
			{
				DataReader.DraftWeights.PositionWeights posWeights = m_Weights.GetPositionWeight(position);
				int combineIndex = (int)DataReader.FOFData.CombineOrder.Solecismic;
				ushort threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				double average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				double stdDev = m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (threshold > 0 && rec.Solecismic < threshold)
				{
					data.SolecismicRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					data.SolecismicRating = ((((double)rec.Solecismic) - average) / stdDev) * posWeights.Solecismic;
				}

                combineIndex = (int)DataReader.FOFData.CombineOrder.Dash;
				threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				stdDev = -m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (threshold > 0 && rec.Dash > threshold)
				{
					data.FortyRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					data.FortyRating = ((((double)rec.Dash / 100.0) - average) / stdDev) * posWeights.Dash;
				}

                combineIndex = (int)DataReader.FOFData.CombineOrder.Bench;
				threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				stdDev = m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (threshold > 0 && rec.Strength < threshold)
				{
					data.BenchRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					data.BenchRating = ((((double)rec.Strength) - average) / stdDev) * posWeights.Bench;
				}

                combineIndex = (int)DataReader.FOFData.CombineOrder.Agility;
				threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				stdDev = -m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (threshold > 0 && rec.Agility > threshold)
				{
					data.AgilityRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					data.AgilityRating = ((((double)rec.Agility / 100.0) - average) / stdDev) * posWeights.Agility;
				}

                combineIndex = (int)DataReader.FOFData.CombineOrder.BroadJump;
				threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				stdDev = m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (threshold > 0 && rec.Jump < threshold)
				{
					data.BenchRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					data.BroadJumpRating = ((((double)rec.Jump) - average) / stdDev) * posWeights.BroadJump;
				}

                combineIndex = (int)DataReader.FOFData.CombineOrder.PositionDrill;
				threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				stdDev = m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (stdDev != 0.0)
				{
					if (threshold > 0 && rec.Position_Specific < threshold)
					{
						data.PositionDrillRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
					}
					else
					{
						data.PositionDrillRating = ((((double)rec.Position_Specific) - average) / stdDev) * posWeights.PositionDrill;
					}
				}
            }
            else
			{
				int combineIndex = (int)DataReader.FOFData.CombineOrder.Solecismic;
				ushort threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				double average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				double stdDev = m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				DataReader.DraftWeights.PositionWeights posWeights = m_Weights.GetNoCombinePositionWeight(position);
				if (stdDev != 0.0 && rec.Solecismic != 0)
				{
					if (threshold > 0 && rec.Solecismic < threshold)
					{
						data.SolecismicRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
					}
					else
					{
						data.SolecismicRating = ((((double)rec.Solecismic) - average) / stdDev) * posWeights.Solecismic;
					}
				}

                combineIndex = (int)DataReader.FOFData.CombineOrder.PositionDrill;
				threshold = m_FOFData.CombineThresholds[positionIndex, combineIndex];
				average = m_FOFData.CombineAverages[positionIndex, combineIndex];
				stdDev = m_FOFData.CombineStandardDeviations[positionIndex, combineIndex];
				if (stdDev != 0.0 && rec.Position_Specific != 0)
				{
					if (threshold > 0 && rec.Position_Specific < threshold)
					{
						data.PositionDrillRating = m_Weights.GlobalWeights.CombineThresholdPenalty;
					}
					else
					{
						data.PositionDrillRating = ((((double)rec.Position_Specific) - average) / stdDev) * posWeights.PositionDrill;
					}
				}
            }
            data.CombineScore = data.SolecismicRating + data.FortyRating + data.AgilityRating + data.BenchRating + data.BroadJumpRating + data.PositionDrillRating;
		}

		private void CalculatePositionRating(PlayerListData data)
		{
			PlayerRecord rec = m_ProgressData.PlayerRecords[data.ID];
			string position = rec.Entries[0].Position;
			if (position == "LS")
			{
				return;
			}
			CalculateCombineRating(data, position);

			DataReader.DraftWeights.GlobalWeightData globalData = m_Weights.GlobalWeights;
			DataReader.DraftWeights.PositionWeights posWeights = null;
			if (rec.Solecismic != 0 && rec.Dash != 0.0 && rec.Strength != 0 && rec.Agility != 0.0 && rec.Jump != 0)
			{
				posWeights = m_Weights.GetPositionWeight(position);
			}
			else
			{
				posWeights = m_Weights.GetNoCombinePositionWeight(position);
			}

			double attributesFactor = 0.0;
			int[] attributeIndices = m_FOFData.PositionGroupAttributes[rec.Entries[0].Position_Group];
			for (int attNum = 0; attNum < attributeIndices.Length; ++attNum)
			{
				int attIndex = attributeIndices[attNum];
                if (rec.DraftLowBars[attIndex] == 255 || rec.DraftHighBars[attIndex] == 255)
                {
                    continue;
                }
				switch (globalData.WhichAttributesToUse)
				{
					case DataReader.DraftWeights.AttributeUsage.UseMin:
						{
							attributesFactor += ((double)(rec.DraftLowBars[attIndex] * posWeights.Attributes[attNum])) * 0.01;
						}
						break;
					case DataReader.DraftWeights.AttributeUsage.UseAverage:
						{
							attributesFactor += ((double)((rec.DraftLowBars[attIndex] + rec.DraftHighBars[attIndex]) *
								posWeights.Attributes[attNum])) * 0.005;
						}
						break;
					case DataReader.DraftWeights.AttributeUsage.UseMax:
						{
							attributesFactor += ((double)(rec.DraftHighBars[attIndex] * posWeights.Attributes[attNum])) * 0.01;
						}
						break;
				}
			}
			data.AttributeScore = attributesFactor;

			data.PositionWeight = posWeights.Weight;
			data.OverallScore = data.CombineScore + data.AttributeScore;
		}
	}
}

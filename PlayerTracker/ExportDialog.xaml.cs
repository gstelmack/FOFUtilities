using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace PlayerTracker
{
	public class LeagueExportConfig
	{
		public string LeagueName;
		public string LeagueFriendlyName;
		public string TeamImageTemplate;
		public string TeamLinkTemplate;
		public string PlayerLinkTemplate;
	}
	public class ExportConfig
	{
		public LeagueExportConfig[] Leagues;

		public ExportConfig()
		{
			LeagueExportConfig foolConfig = new LeagueExportConfig();
			foolConfig.LeagueName = "foffool1";
			foolConfig.LeagueFriendlyName = "FOOL";
			foolConfig.TeamImageTemplate = "http://www.younglifenorthdekalb.com/fool/ben/slivers/helmet[teamid].png";
			foolConfig.TeamLinkTemplate = "http://www.younglifenorthdekalb.com/fool/ben/teampage.php?teamid=[teamid]";
			foolConfig.PlayerLinkTemplate = "http://www.younglifenorthdekalb.com/fool/ben/playercard.php?playerid=[playerid]";

			LeagueExportConfig woofConfig = new LeagueExportConfig();
			woofConfig.LeagueName = "wooffof7";
			woofConfig.LeagueFriendlyName = "WOOF";
			woofConfig.TeamImageTemplate = "http://www.fof-woof2.com/slivers/helmet[teamid].png";
			woofConfig.TeamLinkTemplate = "http://www.fof-woof2.com/teampage.php?teamid=[teamid]";
			woofConfig.PlayerLinkTemplate = "http://www.fof-woof2.com/playercard.php?playerid=[playerid]";

			LeagueExportConfig[] configs = {foolConfig, woofConfig};
			Leagues = configs;
		}

		public LeagueExportConfig GetLeagueExportConfig(string leagueName)
		{
			foreach (LeagueExportConfig config in Leagues)
			{
				if (String.Equals(config.LeagueName, leagueName, StringComparison.OrdinalIgnoreCase))
				{
					return config;
				}
			}

			return null;
		}
	}

	/// <summary>
	/// Interaction logic for ExportDialog.xaml
	/// </summary>
	public partial class ExportDialog : Window
	{
		DataReader.FOFData m_FOFData = new DataReader.FOFData();
		ProgressData m_ProgressData;
		string m_LeagueName;
		ushort m_StageIndex;
		ushort m_MaxDraftYear;
		string[] m_Teams;
		ExportConfig m_ExportConfig = new ExportConfig();

		public ExportDialog(ProgressData progressData, string leagueName, ushort minDraftYear, ushort maxDraftYear, ushort currentStageIndex, string[] teams)
		{
			InitializeComponent();

			m_ProgressData = progressData;
			m_LeagueName = leagueName;
			m_StageIndex = currentStageIndex;
			m_Teams = teams;
			m_MaxDraftYear = maxDraftYear;

			for (ushort draftYear = maxDraftYear; draftYear >= minDraftYear; --draftYear)
			{
				comboBoxDraftYear.Items.Add(draftYear);
			}
			comboBoxDraftYear.SelectedIndex = 0;

			XmlSerializer serializer = new XmlSerializer(typeof(ExportConfig));
			string configFilename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", "ExportConfig.xml");
			if (System.IO.File.Exists(configFilename))
			{
				using (System.IO.StreamReader reader = new System.IO.StreamReader(configFilename))
				{
					m_ExportConfig = (ExportConfig)serializer.Deserialize(reader);
					reader.Close();
				}
			}
			else
			{
				using (System.IO.StreamWriter writer = new System.IO.StreamWriter(configFilename))
				{
					serializer.Serialize(writer, m_ExportConfig);
					writer.Close();
				}
			}
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			ComboBoxItem selectedReport = (ComboBoxItem)comboBoxReportType.SelectedItem;
			if (selectedReport != null)
			{
				string selectedReportType = (string)selectedReport.Content;
				if (selectedReportType == "Current Players")
				{
					DoPlayerReport(false);
				}
				else if (selectedReportType == "All Players")
				{
					DoPlayerReport(true);
				}
				else if (selectedReportType == "Draft Class")
				{
					DoDraftClassReport();
				}
				else if (selectedReportType == "Draft Analysis")
				{
					DoDraftAnalysisReport();
				}
			}
			//Don't exit here so multiple reports can be run
			//this.DialogResult = true;
		}

		private string GetCombineString(ushort combineRating)
		{
			if (combineRating > 0)
			{
				return combineRating.ToString();
			}
			else
			{
				return "NA";
			}
		}

		class BlueBarAnalysis
		{
			public int PlayerCount = 0;
			public int[] BarCorrectCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			public int[] BarHighCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			public int[] BarLowCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			public int[] BarTotalError = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			public int[] BarHighError = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			public int[] BarLowError = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		};

		private void DoDraftAnalysisReport()
		{
			ReportGenerator generator = new ReportGenerator(m_ProgressData);
			generator.Type = ReportGenerator.ReportType.DraftAnalysis;
			generator.DraftYear = m_MaxDraftYear;

			Dictionary<string, System.IO.StreamWriter> positionFiles = new Dictionary<string, System.IO.StreamWriter>();
			Dictionary<string, System.IO.StreamWriter> positionNoCombineFiles = new Dictionary<string, System.IO.StreamWriter>();
			Dictionary<string, System.IO.StreamWriter> positionBarErrorFiles = new Dictionary<string, System.IO.StreamWriter>();
			Dictionary<string, BlueBarAnalysis> positionBarAnalysis = new Dictionary<string, BlueBarAnalysis>();
			foreach (string posGroup in m_FOFData.PositionGroupOrderMap.Keys)
			{
				string baseFilename = m_LeagueName + "_" + posGroup + ".csv";
				string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", baseFilename);
				System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename);
				positionFiles.Add(posGroup, outFile);

				outFile.Write("Sole,Bench,BrdJ,PosSpec,Agil,Dash,Devl,RawGrd,");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[posGroup])
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(attributeName + ",");
				}

				outFile.Write("SoleZ,BenchZ,BrdJZ,PosSpecZ,AgilZ,DashZ,DevlZ,RawGrdZ,");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[posGroup])
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(attributeName + "Z,");
				}

				outFile.WriteLine("PeakCur,PeakCurZ,Ability,Draftable");
			}
			foreach (string posGroup in m_FOFData.PositionGroupOrderMap.Keys)
			{
				string baseFilename = m_LeagueName + "_" + posGroup + "_NoCombine.csv";
				string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", baseFilename);
				System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename);
				positionNoCombineFiles.Add(posGroup, outFile);

				outFile.Write("Sole,Devl,RawGrd,");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[posGroup])
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(attributeName + ",");
				}

				outFile.Write("SoleZ,DevlZ,RawGrdZ,");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[posGroup])
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(attributeName + "Z,");
				}

				outFile.WriteLine("PeakCur,PeakCurZ,Ability,Draftable");
			}
			foreach (string posGroup in m_FOFData.PositionGroupOrderMap.Keys)
			{
				string baseFilename = m_LeagueName + "_" + posGroup + "_BarErrors.csv";
				string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", baseFilename);
				System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename);
				positionBarErrorFiles.Add(posGroup, outFile);

				outFile.Write("Analysis");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[posGroup])
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write("," + attributeName);
				}
				outFile.WriteLine();
				positionBarAnalysis.Add(posGroup, new BlueBarAnalysis());
			}
			generator.StageIndex = m_StageIndex;

			List<PlayerListData> reportPlayers = generator.Generate();

			foreach (PlayerListData data in reportPlayers)
			{
                if (data.PosGrp == "LS")
                {
                    continue;
                }

				System.IO.StreamWriter outFile;
				if (data.PlayerRecord.Strength != 0)
				{
					outFile = positionFiles[data.PosGrp];
					outFile.Write(data.PlayerRecord.Solecismic + ",");
					outFile.Write(data.PlayerRecord.Strength + ",");
					outFile.Write(data.PlayerRecord.Jump + ",");
					outFile.Write(data.PlayerRecord.Position_Specific + ",");
					outFile.Write(data.PlayerRecord.Agility + ",");
					outFile.Write(data.PlayerRecord.Dash + ",");
				}
				else
				{
					outFile = positionNoCombineFiles[data.PosGrp];
					outFile.Write(data.PlayerRecord.Solecismic + ",");
				}
				outFile.Write(data.PlayerRecord.Developed + ",");
				byte rawGrade = (byte)(data.PlayerRecord.Grade / m_FOFData.PositionWeightsInputMap[data.PosGrp]);
				outFile.Write(rawGrade + ",");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[data.PosGrp])
				{
					outFile.Write(data.PlayerRecord.DraftLowBars[attributeIndex] + ",");
				}

				int positionIndex = m_FOFData.PositionGroupOrderMap[data.PosGrp];

				double soleScore = (data.PlayerRecord.Solecismic - m_FOFData.CombineAverages[positionIndex, (int)DataReader.FOFData.CombineOrder.Solecismic])
					/ m_FOFData.CombineStandardDeviations[positionIndex, (int)DataReader.FOFData.CombineOrder.Solecismic];
				outFile.Write(soleScore + ",");

				if (data.PlayerRecord.Strength != 0)
				{
					double strengthScore = (data.PlayerRecord.Strength - m_FOFData.CombineAverages[positionIndex, (int)DataReader.FOFData.CombineOrder.Bench])
						/ m_FOFData.CombineStandardDeviations[positionIndex, (int)DataReader.FOFData.CombineOrder.Bench];
					double jumpScore = (data.PlayerRecord.Jump - m_FOFData.CombineAverages[positionIndex, (int)DataReader.FOFData.CombineOrder.BroadJump])
						/ m_FOFData.CombineStandardDeviations[positionIndex, (int)DataReader.FOFData.CombineOrder.BroadJump];
					double posSpecScore = (data.PlayerRecord.Position_Specific - m_FOFData.CombineAverages[positionIndex, (int)DataReader.FOFData.CombineOrder.PositionDrill])
						/ m_FOFData.CombineStandardDeviations[positionIndex, (int)DataReader.FOFData.CombineOrder.PositionDrill];
					double agilityScore = (m_FOFData.CombineAverages[positionIndex, (int)DataReader.FOFData.CombineOrder.Agility] - (data.PlayerRecord.Agility / 100.0))
						/ m_FOFData.CombineStandardDeviations[positionIndex, (int)DataReader.FOFData.CombineOrder.Agility];
					double dashScore = (m_FOFData.CombineAverages[positionIndex, (int)DataReader.FOFData.CombineOrder.Dash] - (data.PlayerRecord.Dash / 100.0))
						/ m_FOFData.CombineStandardDeviations[positionIndex, (int)DataReader.FOFData.CombineOrder.Dash];
					outFile.Write(strengthScore + ",");
					outFile.Write(jumpScore + ",");
					outFile.Write(posSpecScore + ",");
					outFile.Write(agilityScore + ",");
					outFile.Write(dashScore + ",");
				}
				outFile.Write((data.PlayerRecord.Developed/100.0).ToString("F2") + ",");
				outFile.Write((rawGrade/80.0).ToString("F2") + ",");

				foreach (int attributeIndex in m_FOFData.PositionGroupAttributes[data.PosGrp])
				{
					double barPercent = data.PlayerRecord.DraftLowBars[attributeIndex] / 100.0;
					outFile.Write(barPercent.ToString("F2") + ",");
				}

				outFile.Write(data.PeakCur + ",");
				double peakCurZ = data.PeakCur / 100.0;
				outFile.Write(peakCurZ.ToString("F2") + ",");
				outFile.Write(m_FOFData.AbilityIndexMap[data.PeakCur] + ",");
				outFile.Write(m_FOFData.DraftableMap[data.PeakCur]);
				outFile.WriteLine();

				// Only do this part for fully developed players
				if (data.PresentCur == data.PresentFut)
				{
					BlueBarAnalysis posAnalysis = positionBarAnalysis[data.PosGrp];
					int[] attributeIndices = m_FOFData.PositionGroupAttributes[data.PosGrp];
					posAnalysis.PlayerCount += 1;
					for (int i = 0; i < attributeIndices.Length; ++i)
					{
						int attributeIndex = attributeIndices[i];
						byte peak = data.PeakBars[attributeIndex];
						byte minVal = data.PlayerRecord.DraftLowBars[attributeIndex];
						byte maxVal = data.PlayerRecord.DraftHighBars[attributeIndex];
						if (peak < minVal)
						{
							int error = minVal - peak;
							posAnalysis.BarLowCount[i] += 1;
							posAnalysis.BarLowError[i] += error;
							posAnalysis.BarTotalError[i] += error;
						}
						else if (peak > maxVal)
						{
							int error = peak - maxVal;
							posAnalysis.BarHighCount[i] += 1;
							posAnalysis.BarHighError[i] += error;
							posAnalysis.BarTotalError[i] += error;
						}
						else
						{
							posAnalysis.BarCorrectCount[i] += 1;
						}
					}
				}
			}

			foreach (string posGroup in m_FOFData.PositionGroupOrderMap.Keys)
			{
				BlueBarAnalysis posAnalysis = positionBarAnalysis[posGroup];
				System.IO.StreamWriter outFile = positionBarErrorFiles[posGroup];
				int attributeCount = m_FOFData.PositionGroupAttributes[posGroup].Length;

				outFile.Write("Count");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					outFile.Write("," + posAnalysis.PlayerCount);
				}
				outFile.WriteLine();

				outFile.Write("Correct");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					outFile.Write("," + posAnalysis.BarCorrectCount[attributeIndex]);
				}
				outFile.WriteLine();

				outFile.Write("Correct Pct");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					float percent = (float)posAnalysis.BarCorrectCount[attributeIndex] / (float)posAnalysis.PlayerCount;
					outFile.Write("," + percent.ToString("P2"));
				}
				outFile.WriteLine();

				outFile.Write("Avg Error");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					int errorCount = posAnalysis.PlayerCount - posAnalysis.BarCorrectCount[attributeIndex];
					float error = 0.0f;
					if (errorCount > 0)
					{
						error = (float)posAnalysis.BarTotalError[attributeIndex] / (float)errorCount;
					}
					outFile.Write("," + error.ToString("F2"));
				}
				outFile.WriteLine();

				outFile.Write("High");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					outFile.Write("," + posAnalysis.BarHighCount[attributeIndex]);
				}
				outFile.WriteLine();

				outFile.Write("High Pct");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					float percent = (float)posAnalysis.BarHighCount[attributeIndex] / (float)posAnalysis.PlayerCount;
					outFile.Write("," + percent.ToString("P2"));
				}
				outFile.WriteLine();

				outFile.Write("Avg Error");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					int errorCount = posAnalysis.BarHighCount[attributeIndex];
					float error = 0.0f;
					if (errorCount > 0)
					{
						error = (float)posAnalysis.BarHighError[attributeIndex] / (float)errorCount;
					}
					outFile.Write("," + error.ToString("F2"));
				}
				outFile.WriteLine();

				outFile.Write("Low");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					outFile.Write("," + posAnalysis.BarLowCount[attributeIndex]);
				}
				outFile.WriteLine();

				outFile.Write("Low Pct");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					float percent = (float)posAnalysis.BarLowCount[attributeIndex] / (float)posAnalysis.PlayerCount;
					outFile.Write("," + percent.ToString("P2"));
				}
				outFile.WriteLine();

				outFile.Write("Avg Error");
				for (int attributeIndex = 0; attributeIndex < attributeCount; ++attributeIndex)
				{
					int errorCount = posAnalysis.BarLowCount[attributeIndex];
					float error = 0.0f;
					if (errorCount > 0)
					{
						error = (float)posAnalysis.BarLowError[attributeIndex] / (float)errorCount;
					}
					outFile.Write("," + error.ToString("F2"));
				}
				outFile.WriteLine();
			}

			labelStatus.Content = "Wrote Draft Analysis files";

			foreach (System.IO.StreamWriter outFile in positionFiles.Values)
			{
				outFile.Close();
			}
			foreach (System.IO.StreamWriter outFile in positionNoCombineFiles.Values)
			{
				outFile.Close();
			}
			foreach (System.IO.StreamWriter outFile in positionBarErrorFiles.Values)
			{
				outFile.Close();
			}
		}

		private void DoPlayerReport(bool allPlayers)
		{
			string baseFileName = m_LeagueName;
			ReportGenerator generator = new ReportGenerator(m_ProgressData);
			if (allPlayers)
			{
				generator.Type = ReportGenerator.ReportType.All;
				baseFileName += "_all.csv";
			}
			else
			{
				generator.Type = ReportGenerator.ReportType.Current;
				baseFileName += "_current.csv";
			}
			generator.StageIndex = m_StageIndex;

			List<PlayerListData> reportPlayers = generator.Generate();
			IComparer<PlayerListData> sorter = new TeamPositionSorter();
			reportPlayers.Sort(sorter);
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", baseFileName);
			using(System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename))
			{
				outFile.Write("Name,Pos,Team,ID,Exp,Ht,DWt,PWt,DraftAvail,Sole,Bench,BrdJ,PosSpec,Agil,Dash,Devl,RawGrd,AdjGrd,");
				outFile.Write("PresCur,PresFut,PrevCur,PrevFut,StartCur,StarFut,PeakCur,PeakFut,");
				outFile.Write("PofWkCt,DraftRd,DraftPos,DraftBy,DraftYr,DraftCls,Seasons");

				for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(",Current_" + attributeName);
				}
				for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(",Future_" + attributeName);
				}
				for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(",Peak_" + attributeName);
				}
				for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(",Low_" + attributeName);
				}
				for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
				{
					string attributeName = ((DataReader.FOFData.ScoutBars)attributeIndex).ToString();
					outFile.Write(",High_" + attributeName);
				}
				outFile.WriteLine();

				foreach (PlayerListData data in reportPlayers)
				{
					outFile.Write('\"' + data.Name + "\",");
					outFile.Write('\"' + data.PosGrp + "\",");
					outFile.Write(data.TeamIndex + ",");
					outFile.Write(data.ID + ",");
					outFile.Write(data.Exp + ",");
                    outFile.Write(data.PlayerRecord.Height + ",");
                    outFile.Write(data.StartWeight + ",");
                    outFile.Write(data.PeakWeight + ",");
					if (data.PlayerRecord.Solecismic != Byte.MaxValue)
					{
						outFile.Write("1,");
						outFile.Write(data.PlayerRecord.Solecismic + ",");
						outFile.Write(data.PlayerRecord.Strength + ",");
						outFile.Write(data.PlayerRecord.Jump + ",");
						outFile.Write(data.PlayerRecord.Position_Specific + ",");
						outFile.Write(data.PlayerRecord.Agility + ",");
						outFile.Write(data.PlayerRecord.Dash + ",");
						outFile.Write(data.PlayerRecord.Developed + ",");
						byte rawGrade = (byte)(data.PlayerRecord.Grade / m_FOFData.PositionWeightsInputMap[data.PosGrp]);
						outFile.Write(rawGrade + ",");
						outFile.Write(data.PlayerRecord.Grade + ",");
					}
					else
					{
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
						outFile.Write("0,");
					}
					outFile.Write(data.PresentCur + ",");
					outFile.Write(data.PresentFut + ",");
					outFile.Write(data.PreviousCur + ",");
					outFile.Write(data.PreviousFut + ",");
					outFile.Write(data.StartingCur + ",");
					outFile.Write(data.StartingFut + ",");
					outFile.Write(data.PeakCur + ",");
					outFile.Write(data.PeakFut + ",");
					outFile.Write(data.PlayerRecord.Player_of_the_Week_Count + ",");
					outFile.Write(data.PlayerRecord.Draft_Round + ",");
					outFile.Write(data.PlayerRecord.Drafted_Position + ",");
					outFile.Write(data.PlayerRecord.Drafted_By + ",");
					outFile.Write(data.PlayerRecord.Draft_Year + ",");
					outFile.Write(data.PlayerRecord.Draft_Class + ",");
					outFile.Write(data.PlayerRecord.Number_of_Seasons);

					for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
					{
						outFile.Write("," + data.CurrentBars[attributeIndex]);
					}
					for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
					{
						outFile.Write("," + data.FutureBars[attributeIndex]);
					}
					for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
					{
						outFile.Write("," + data.PeakBars[attributeIndex]);
					}
					for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
					{
						outFile.Write("," + data.PlayerRecord.DraftLowBars[attributeIndex]);
					}
					for (int attributeIndex = 0; attributeIndex < (int)DataReader.FOFData.ScoutBars.Count; ++attributeIndex)
					{
						outFile.Write("," + data.PlayerRecord.DraftHighBars[attributeIndex]);
					}
					outFile.WriteLine();
				}

				outFile.Close();
			}

			labelStatus.Content = "Wrote " + baseFileName;
			System.Diagnostics.Process.Start(filename);
		}

		private void DoDraftClassReport()
		{
			ReportGenerator generator = new ReportGenerator(m_ProgressData);
			if (comboBoxDraftYear.SelectedIndex >= 0)
			{
				LeagueExportConfig leagueExportConfig = m_ExportConfig.GetLeagueExportConfig(m_LeagueName);
				generator.DraftYear = (ushort)comboBoxDraftYear.SelectedItem;
				generator.StageIndex = m_StageIndex;
				generator.Type = ReportGenerator.ReportType.DraftYear;

				List<PlayerListData> reportPlayers = generator.Generate();
				IComparer<PlayerListData> sorter = new DescendingPeakSorter();
				reportPlayers.Sort(sorter);
				string tableTitle = "";
				if (leagueExportConfig != null)
				{
					tableTitle = leagueExportConfig.LeagueFriendlyName + " " + generator.DraftYear + " Draft Class";
				}
				else
				{
					tableTitle = m_LeagueName + " " + generator.DraftYear + " Draft Class";
				}

				string baseFileName = m_LeagueName + "_draft_" + generator.DraftYear + ".html";
				string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", baseFileName);
				using (System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename))
				{
					outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
					outFile.WriteLine("<HTML>");
					outFile.WriteLine("\t<HEAD>");
					outFile.WriteLine("\t\t<TITLE>" + tableTitle + "</TITLE>");
					outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
					outFile.WriteLine("\t\t\tTR.headers { color: white; background: #030260 }");
					outFile.WriteLine("\t\t\tTR.normal { color: black; background: #EAEAEA }");
					outFile.WriteLine("\t\t</STYLE>");
					outFile.WriteLine("\t</HEAD>");
					outFile.WriteLine("\t<BODY>");

					outFile.WriteLine("\t\t<TABLE summary=\"This shows the development of the draft class\" width = \"100%\" border = \"1\" cellpadding=\"0\" cellspacing = \"0\">");
					outFile.WriteLine("\t\t\t<CAPTION><EM>" + tableTitle + "</EM></CAPTION>");
					outFile.Write("\t\t\t<TR class=\"headers\">");
					outFile.Write("<TH>Name</TH>");
					outFile.Write("<TH>Pos</TH>");
					outFile.Write("<TH>Team</TH>");
					outFile.Write("<TH>Exp</TH>");
                    outFile.Write("<TH>Ht</TH>");
                    outFile.Write("<TH>DWt</TH>");
                    outFile.Write("<TH>PWt</TH>");
                    outFile.Write("<TH>Raw</TH>");
					outFile.Write("<TH>Adj</TH>");
					outFile.Write("<TH>Dash</TH>");
					outFile.Write("<TH>Sole</TH>");
					outFile.Write("<TH>Bench</TH>");
					outFile.Write("<TH>Agil</TH>");
					outFile.Write("<TH>BrdJ</TH>");
					outFile.Write("<TH>Pos</TH>");
					outFile.Write("<TH>%Dev</TH>");
					outFile.Write("<TH>StartCur</TH>");
					outFile.Write("<TH>StarFut</TH>");
					outFile.Write("<TH>PeakCur</TH>");
					outFile.Write("<TH>PeakFut</TH>");
					outFile.Write("<TH>PresCur</TH>");
					outFile.Write("<TH>PresFut</TH>");
					outFile.Write("<TH>DraftRd</TH>");
					outFile.Write("<TH>DraftPos</TH>");
					outFile.Write("<TH>DraftBy</TH>");
					outFile.WriteLine("</TR>");

					foreach (PlayerListData data in reportPlayers)
					{
						string teamEntry = "FA";
						string nameEntry = data.Name;

						string teamName = "FA";
						if (data.TeamIndex < m_Teams.Length)
						{
							teamName = m_Teams[data.TeamIndex];
							teamEntry = teamName;
						}
						string draftedTeamEntry = "Undrafted";
						string draftedTeamName = "Undrafted";
						if (data.PlayerRecord.Drafted_By < m_Teams.Length)
						{
							draftedTeamName = m_Teams[data.PlayerRecord.Drafted_By];
							draftedTeamEntry = draftedTeamName;
						}

						if (leagueExportConfig != null)
						{
							if (data.TeamIndex < m_Teams.Length)
							{
								string teamLink = leagueExportConfig.TeamLinkTemplate.Replace("[teamid]", data.TeamIndex.ToString());
								string teamImage = leagueExportConfig.TeamImageTemplate.Replace("[teamid]", data.TeamIndex.ToString());
								teamEntry = "<a title=\"" + teamName + "\" href=\"" + teamLink + "\"><img border=\"0\" src=\"" + teamImage + "\"></a>";
							}
							if (data.PlayerRecord.Drafted_By < m_Teams.Length)
							{
								string draftedTeamLink = leagueExportConfig.TeamLinkTemplate.Replace("[teamid]", data.PlayerRecord.Drafted_By.ToString());
								string draftedTeamImage = leagueExportConfig.TeamImageTemplate.Replace("[teamid]", data.PlayerRecord.Drafted_By.ToString());
								draftedTeamEntry = "<a title=\"" + draftedTeamName + "\" href=\"" + draftedTeamLink + "\"><img border=\"0\" src=\"" + draftedTeamImage + "\"></a>";
							}
							string nameLink = leagueExportConfig.PlayerLinkTemplate.Replace("[playerid]", data.ID.ToString());
							nameEntry = "<a title=\"" + data.Name + "\" href=\"" + nameLink + "\">" + data.Name + "</a>";
						}

						outFile.Write("\t\t\t<TR class=\"normal\">");
						outFile.Write("<TD>" + nameEntry + "</TD>");
						outFile.Write("<TD align=\"center\">" + data.PosGrp + "</TD>");
						outFile.Write("<TD align=\"center\">" + teamEntry + "</TD>");
						outFile.Write("<TD align=\"center\">" + data.ExpYears + "</TD>");
                        outFile.Write("<TD align=\"center\">" + data.PlayerRecord.Height + "</TD>");
                        outFile.Write("<TD align=\"center\">" + data.StartWeight + "</TD>");
                        outFile.Write("<TD align=\"center\">" + data.PeakWeight + "</TD>");
                        double adjustedGrade = (data.PlayerRecord.Grade / 10.0);
						double rawGrade = adjustedGrade / m_FOFData.PositionWeightsInputMap[data.PosGrp];
						outFile.Write("<TD align=\"center\">" + rawGrade.ToString("F1") + "</TD>");
						outFile.Write("<TD align=\"center\">" + adjustedGrade.ToString("F1") + "</TD>");
						outFile.Write("<TD align=\"center\"" + ColorCombine(data.PosGrp, data.PlayerRecord.Dash, DataReader.FOFData.CombineOrder.Dash) + "<b>" + (data.PlayerRecord.Dash / 100.0).ToString("F2") + "</b></font></TD>");
						outFile.Write("<TD align=\"center\"" + ColorCombine(data.PosGrp, data.PlayerRecord.Solecismic, DataReader.FOFData.CombineOrder.Solecismic) + "<b>" + data.PlayerRecord.Solecismic + "</b></font></TD>");
						outFile.Write("<TD align=\"center\"" + ColorCombine(data.PosGrp, data.PlayerRecord.Strength, DataReader.FOFData.CombineOrder.Bench) + "<b>" + data.PlayerRecord.Strength + "</b></font></TD>");
						outFile.Write("<TD align=\"center\"" + ColorCombine(data.PosGrp, data.PlayerRecord.Agility, DataReader.FOFData.CombineOrder.Agility) + "<b>" + (data.PlayerRecord.Agility / 100.0).ToString("F2") + "</b></font></TD>");
						outFile.Write("<TD align=\"center\"" + ColorCombine(data.PosGrp, data.PlayerRecord.Jump, DataReader.FOFData.CombineOrder.BroadJump) + "<b>" + data.PlayerRecord.Jump + "</b></font></TD>");
						outFile.Write("<TD align=\"center\"" + ColorCombine(data.PosGrp, data.PlayerRecord.Position_Specific, DataReader.FOFData.CombineOrder.PositionDrill) + "<b>" + data.PlayerRecord.Position_Specific + "</b></font></TD>");
						outFile.Write("<TD align=\"center\">" + data.PlayerRecord.Developed + "</TD>");
						outFile.Write("<TD align=\"center\" bgcolor=\"" + kAbilityMap[data.StartingCur] + "\"><b>" + data.StartingCur + "</b></TD>");
						outFile.Write("<TD align=\"center\" bgcolor=\"" + kAbilityMap[data.StartingFut] + "\"><b>" + data.StartingFut + "</b></TD>");
						outFile.Write("<TD align=\"center\" bgcolor=\"" + kAbilityMap[data.PeakCur] + "\"><b>" + data.PeakCur + "</b></TD>");
						outFile.Write("<TD align=\"center\" bgcolor=\"" + kAbilityMap[data.PeakFut] + "\"><b>" + data.PeakFut + "</b></TD>");
						outFile.Write("<TD align=\"center\" bgcolor=\"" + kAbilityMap[data.PresentCur] + "\"><b>" + data.PresentCur + "</b></TD>");
						outFile.Write("<TD align=\"center\" bgcolor=\"" + kAbilityMap[data.PresentFut] + "\"><b>" + data.PresentFut + "</b></TD>");
						outFile.Write("<TD align=\"center\">" + data.PlayerRecord.Draft_Round + "</TD>");
						outFile.Write("<TD align=\"center\">" + data.PlayerRecord.Drafted_Position + "</TD>");
						outFile.WriteLine("<TD align=\"center\">" + draftedTeamEntry + "</TD></TR>");
					}
					outFile.WriteLine("\t\t</TABLE>");
					outFile.WriteLine("\t</BODY>");
					outFile.WriteLine("</HTML>");

					outFile.Close();
				}
				labelStatus.Content = "Wrote " + baseFileName;
				System.Diagnostics.Process.Start(filename);
			}
			else
			{
				MessageBox.Show("Please select a draft year before running a Draft Class report", "Error Generating Report", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		string[] kAbilityMap =
			{
				"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#800080"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#0060A0"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#008040"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#00A066"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#FFFF00"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
				,"#D00000"
			};

		private string ColorCombine(string pos, ushort value, DataReader.FOFData.CombineOrder combine)
		{
			int combineIndex = (int)combine;
			int positionIndex = m_FOFData.PositionGroupOrderMap[pos];
			int redIndex = combineIndex * 3;
			int blueIndex = redIndex + 1;
			int greenIndex = blueIndex + 1;
			if (value == 0)
			{
				return " bgcolor=\"orange\"><font color=\"#000000\">";
			}
			else if (pos == "LS")
			{
				return " ><font color=\"#000000\">";
			}
			else if (combineIndex == 0 || combineIndex == 3)
			{
				if (m_FOFData.CombineThresholds[positionIndex, combineIndex] != 0 && value > m_FOFData.CombineThresholds[positionIndex, combineIndex])
				{
					return " bgcolor=\"red\"><font color=\"#000000\">";
				}
				else if (value <= m_FOFData.CombineColors[positionIndex, redIndex])
				{
					return " ><font color=\"#FF0000\">";
				}
				else if (value <= m_FOFData.CombineColors[positionIndex, blueIndex])
				{
					return " ><font color=\"#0000FF\">";
				}
				else if (value >= m_FOFData.CombineColors[positionIndex, greenIndex])
				{
					return " ><font color=\"#00FF00\">";
				}
				else
				{
					return " ><font color=\"#000000\">";
				}
			}
			else
			{
				if (m_FOFData.CombineThresholds[positionIndex, combineIndex] != 0 && value < m_FOFData.CombineThresholds[positionIndex, combineIndex])
				{
					return " bgcolor=\"red\"><font color=\"#000000\">";
				}
				else if (value >= m_FOFData.CombineColors[positionIndex, redIndex])
				{
					return " ><font color=\"#FF0000\">";
				}
				else if (value >= m_FOFData.CombineColors[positionIndex, blueIndex])
				{
					return " ><font color=\"#0000FF\">";
				}
				else if (value <= m_FOFData.CombineColors[positionIndex, greenIndex])
				{
					return " ><font color=\"#00FF00\">";
				}
				else
				{
					return " ><font color=\"#000000\">";
				}
			}
		}
	}
}

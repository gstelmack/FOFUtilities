using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayerTracker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string m_LeagueName = "";
		private string m_LeagueDataRootDir = "";
		private string m_LeagueDir = "";
		private string m_SavePath = "";
		private char[] commaDelim = new char[] { ',' };
		private ushort m_StageIndex = UInt16.MaxValue;
		private ProgressData m_ProgressData = null;
		private const int kTeamCount = 32;
		private string[] m_Teams = new string[kTeamCount];
		private ObservableCollection<PlayerListData> m_PlayerList = new ObservableCollection<PlayerListData>();
		private const int kMaxBarCount = 15;
		private Canvas[] m_BarCanvases = new Canvas[kMaxBarCount];
		private Label[] m_BarLabels = new Label[kMaxBarCount];
		private Label[] m_CurFutLabels = new Label[kMaxBarCount];
		private Label[] m_PeakLabels = new Label[kMaxBarCount];
		private Label[] m_DraftLabels = new Label[kMaxBarCount];
		private ReportGenerator m_ReportGenerator = null;
		private DataReader.FOFData m_FOFData = new DataReader.FOFData();

		public MainWindow()
		{
			InitializeComponent();

            System.Reflection.Assembly a = typeof(MainWindow).Assembly;
            Title += " v" + a.GetName().Version;

            CreateMaps();

			string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			m_LeagueDataRootDir = System.IO.Path.Combine(appData, "Solecismic Software", "Front Office Football Eight", "leaguedata");
			foreach (string curLeague in System.IO.Directory.EnumerateDirectories(m_LeagueDataRootDir))
			{
				comboBoxLeagues.Items.Add(System.IO.Path.GetFileNameWithoutExtension(curLeague));
			}
			buttonImport.IsEnabled = false;
			buttonExport.IsEnabled = false;
			comboBoxTeams.IsEnabled = false;
			comboBoxDraftYear.IsEnabled = false;
			comboBoxPosition.IsEnabled = false;

			string outputPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker");
			if (!System.IO.Directory.Exists(outputPath))
			{
				System.IO.Directory.CreateDirectory(outputPath);
			}

			listViewPlayers.ItemsSource = m_PlayerList;

			comboBoxPosition.Items.Add("QB");
			comboBoxPosition.Items.Add("RB");
			comboBoxPosition.Items.Add("FB");
			comboBoxPosition.Items.Add("TE");
			comboBoxPosition.Items.Add("WR");
			comboBoxPosition.Items.Add("C");
			comboBoxPosition.Items.Add("G");
			comboBoxPosition.Items.Add("T");
			comboBoxPosition.Items.Add("P");
			comboBoxPosition.Items.Add("K");
			comboBoxPosition.Items.Add("DE");
			comboBoxPosition.Items.Add("DT");
			comboBoxPosition.Items.Add("ILB");
			comboBoxPosition.Items.Add("OLB");
			comboBoxPosition.Items.Add("CB");
			comboBoxPosition.Items.Add("S");
			comboBoxPosition.Items.Add("LS");

            comboBoxDefensiveFront.ItemsSource = Enum.GetValues(typeof(DataReader.FOFData.DefensiveFront)).Cast<DataReader.FOFData.DefensiveFront>();
            comboBoxDefensiveFront.SelectedItem = DataReader.FOFData.DefensiveFront.True34;
        }

		private void comboBoxLeagues_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LoadLeagueData((string)comboBoxLeagues.SelectedValue);
		}

		private async void LoadLeagueData(string leagueName)
		{
			m_StageIndex = UInt16.MaxValue;
			m_LeagueName = leagueName;
			Cursor oldCursor = this.Cursor;
			this.Cursor = Cursors.Wait;
			buttonImport.IsEnabled = false;
			buttonExport.IsEnabled = false;
			comboBoxTeams.IsEnabled = false;
			comboBoxPosition.IsEnabled = false;
			comboBoxPosition.IsEnabled = false;

			await Task.Run(new Action(DoLeagueLoad));

			buttonImport.IsEnabled = true;
			buttonExport.IsEnabled = true;
			comboBoxPosition.IsEnabled = true;
			comboBoxTeams.IsEnabled = true;
			comboBoxTeams.Items.Clear();
			foreach (string team in m_Teams)
			{
				comboBoxTeams.Items.Add(team);
			}
			comboBoxTeams.Items.Add("Free Agents");
			FigureOutDraftYears();
			if (m_StageIndex != UInt16.MaxValue)
			{
				labelStatus.Content = "Last Import " + m_ProgressData.StageRecords[m_StageIndex].Season + " " + m_ProgressData.StageRecords[m_StageIndex].Stage;
			}
			else
			{
				labelStatus.Content = "Nothing imported yet";
			}
			this.Cursor = oldCursor;
		}

		private void DoLeagueLoad()
		{
			m_LeagueDir = System.IO.Path.Combine(m_LeagueDataRootDir, m_LeagueName);
			LoadTeams();
			m_ProgressData = new ProgressData();
			m_SavePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PlayerTracker", m_LeagueName + ".ptdat");
			if (System.IO.File.Exists(m_SavePath))
			{
				System.IO.FileStream inStream = new System.IO.FileStream(m_SavePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				Encoding windows1252Encoding = Encoding.GetEncoding(1252);
				using (System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding))
				{
					m_ProgressData.Read(inFile);
					inFile.Close();
					m_StageIndex = (ushort)(m_ProgressData.StageRecords.Count - 1);

					m_ReportGenerator = new ReportGenerator(m_ProgressData);
				}
			}
		}

		private void SaveProgressData()
		{
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);

			System.IO.FileStream outStream = new System.IO.FileStream(m_SavePath, System.IO.FileMode.Create);
			using (System.IO.BinaryWriter outFile = new System.IO.BinaryWriter(outStream, windows1252Encoding))
			{
				m_ProgressData.Write(outFile);
				outFile.Close();
			}
			m_ReportGenerator = new ReportGenerator(m_ProgressData);
		}

		private void LoadTeams()
		{
			string filePath = System.IO.Path.Combine(m_LeagueDir, "team_information.csv");
			if (!System.IO.File.Exists(filePath))
			{
				MessageBox.Show("Could not find team_information.csv, did you export league data?", "Error Reading League", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
			{
				inFile.ReadLine();	// skip the header line
				while (!inFile.EndOfStream)
				{
					string inLine = inFile.ReadLine();
					string[] tokens = inLine.Split(commaDelim);
					int teamIndex = Int32.Parse(tokens[0]);
					m_Teams[teamIndex] = tokens[14];
				}
			}
		}

		private void buttonReleaseNotes_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("pt_release_notes.txt");
		}

		private void buttonExport_Click(object sender, RoutedEventArgs e)
		{
			ExportDialog dlg = new ExportDialog(m_ProgressData, m_LeagueName, m_MinDraftYear, m_MaxDraftYear, m_StageIndex, m_Teams);
			dlg.ShowDialog();	// don't care about the result
		}

		private void buttonImport_Click(object sender, RoutedEventArgs e)
		{
			FigureOutCurrentStage();
			if (m_StageIndex != UInt16.MaxValue)
			{
				LoadPlayers();
				LoadRookies();
				LoadScouting();

				SaveProgressData();

				FigureOutDraftYears();
				labelStatus.Content = "Imported " + m_ProgressData.StageRecords[m_StageIndex].Season + " " + m_ProgressData.StageRecords[m_StageIndex].Stage;
				UpdatePlayerList();
			}
			else
			{
				m_StageIndex = (ushort)(m_ProgressData.StageRecords.Count - 1);
			}
		}

		private void LoadScouting()
		{
			string filePath = System.IO.Path.Combine(m_LeagueDir, "players_personal.csv");
			if (!System.IO.File.Exists(filePath))
			{
				MessageBox.Show("Could not find players_personal.csv, did you export personal league data?", "Error Reading League", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
			{
				inFile.ReadLine();	// skip the header line
				while (!inFile.EndOfStream)
				{
					string inLine = inFile.ReadLine();
					string[] tokens = inLine.Split(commaDelim);
					uint playerID = UInt32.Parse(tokens[0]);
					PlayerRecord rec = m_ProgressData.PlayerRecords[playerID];
					PlayerEntryRecord entry = rec.Entries[rec.Entries.Count - 1];
					int tokenIndex = 1;
					for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
					{
						entry.CurBars[i] = Byte.Parse(tokens[tokenIndex++]);
					}
					entry.CurOverall = Byte.Parse(tokens[tokenIndex++]);
					for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
					{
						entry.FutBars[i] = Byte.Parse(tokens[tokenIndex++]);
					}
					entry.FutOverall = Byte.Parse(tokens[tokenIndex++]);
				}
				inFile.Close();
			}
		}

		private void LoadRookies()
		{
			string filePath = System.IO.Path.Combine(m_LeagueDir, "rookies.csv");
			if (!System.IO.File.Exists(filePath))
			{
				return;
			}
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
			{
				inFile.ReadLine();	// skip the header line
				while (!inFile.EndOfStream)
				{
					string inLine = inFile.ReadLine();
					string[] tokens = inLine.Split(commaDelim);
					uint playerID = UInt32.Parse(tokens[0]);
					if (m_ProgressData.PlayerRecords.ContainsKey(playerID))
					{
						PlayerRecord rec = m_ProgressData.PlayerRecords[playerID];
						rec.Dash = UInt16.Parse(tokens[7]);
						rec.Solecismic = Byte.Parse(tokens[8]);
						rec.Strength = Byte.Parse(tokens[9]);
						rec.Agility = UInt16.Parse(tokens[10]);
						rec.Jump = Byte.Parse(tokens[11]);
						rec.Position_Specific = Byte.Parse(tokens[12]);
						rec.Developed = Byte.Parse(tokens[13]);
						rec.Grade = Byte.Parse(tokens[14]);
					}
				}
				inFile.Close();
			}
			filePath = System.IO.Path.Combine(m_LeagueDir, "draft_personal.csv");
			if (!System.IO.File.Exists(filePath))
			{
				return;
			}
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
			{
				inFile.ReadLine();	// skip the header line
				while (!inFile.EndOfStream)
				{
					string inLine = inFile.ReadLine();
					string[] tokens = inLine.Split(commaDelim);
					uint playerID = UInt32.Parse(tokens[0]);
					if (m_ProgressData.PlayerRecords.ContainsKey(playerID))
					{
						PlayerRecord rec = m_ProgressData.PlayerRecords[playerID];
						rec.Interviewed = Byte.Parse(tokens[1]);
						int tokenIndex = 2;
						for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
						{
							rec.DraftLowBars[i] = Byte.Parse(tokens[tokenIndex++]);
						}
						for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
						{
							rec.DraftHighBars[i] = Byte.Parse(tokens[tokenIndex++]);
						}
					}
				}
				inFile.Close();
			}
		}

		private void LoadPlayers()
		{
			string filePath = System.IO.Path.Combine(m_LeagueDir, "player_information.csv");
			if (!System.IO.File.Exists(filePath))
			{
				MessageBox.Show("Could not find player_information.csv, did you export league data?", "Error Reading League", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			ushort curSeason = m_ProgressData.StageRecords[m_StageIndex].Season;
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
			{
				inFile.ReadLine();	// skip the header line
				while (!inFile.EndOfStream)
				{
					string inLine = inFile.ReadLine();
					string[] tokens = inLine.Split(commaDelim);
					uint playerID = UInt32.Parse(tokens[0]);
					PlayerRecord rec = null;
					if (m_ProgressData.PlayerRecords.ContainsKey(playerID))
					{
						rec = m_ProgressData.PlayerRecords[playerID];
					}
					else
					{
						rec = new PlayerRecord();
						rec.Player_ID = playerID;
						for (int i = 0; i < (int)DataReader.FOFData.ScoutBars.Count; ++i)
						{
							rec.DraftLowBars[i] = Byte.MaxValue;
							rec.DraftHighBars[i] = Byte.MaxValue;
						}
						m_ProgressData.PlayerRecords.Add(playerID, rec);
					}
					rec.Last_Name = tokens[1];
					rec.First_Name = tokens[2];
					rec.Player_of_Game_Count = UInt16.Parse(tokens[6]);
					rec.Championship_Rings = Byte.Parse(tokens[7]);
					rec.Player_of_the_Week_Count = UInt16.Parse(tokens[8]);
					rec.Player_of_the_Week_Win = UInt16.Parse(tokens[9]);
					rec.Height = Byte.Parse(tokens[10]);
					rec.Weight = UInt16.Parse(tokens[11]);
					rec.Hall_of_Fame_Flag = Byte.Parse(tokens[12]);
					rec.Year_Inducted = UInt16.Parse(tokens[13]);
					rec.Percentage_of_Vote = Byte.Parse(tokens[14]);
					rec.Chosen_Team = Byte.Parse(tokens[15]);
					rec.Year_Born = UInt16.Parse(tokens[16]);
					rec.Month_Born = Byte.Parse(tokens[17]);
					rec.Day_Born = Byte.Parse(tokens[18]);
					rec.Draft_Round = Byte.Parse(tokens[20]);
					rec.Drafted_Position = Byte.Parse(tokens[21]);
					// This is sometimes -1?
					int draftedBy = Int32.Parse(tokens[22]);
					if (draftedBy < Byte.MinValue || draftedBy > Byte.MaxValue)
					{
						rec.Drafted_By = Byte.MaxValue;
					}
					else
					{
						rec.Drafted_By = (byte)draftedBy;
					}
					rec.Draft_Year = UInt16.Parse(tokens[23]);
					rec.Fourth_Quarter_Comebacks = UInt16.Parse(tokens[24]);
					rec.Quarterback_Wins = UInt16.Parse(tokens[25]);
					rec.Quarterback_Losses = UInt16.Parse(tokens[26]);
					rec.Quarterback_Ties = UInt16.Parse(tokens[27]);
					rec.Career_Games_Played = UInt16.Parse(tokens[30]);
					rec.Number_of_Seasons = Byte.Parse(tokens[31]);
					if (rec.Number_of_Seasons == 0)
					{
						rec.Draft_Class = curSeason;
					}
					else
					{
						rec.Draft_Class = UInt16.Parse(tokens[52]);
					}
				}
				inFile.Close();
			}
			filePath = System.IO.Path.Combine(m_LeagueDir, "player_record.csv");
			if (!System.IO.File.Exists(filePath))
			{
				MessageBox.Show("Could not find player_record.csv, did you export league data?", "Error Reading League", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
			{
				string headerLine = inFile.ReadLine();
                string[] headers = headerLine.Split(commaDelim);

                while (!inFile.EndOfStream)
				{
					string inLine = inFile.ReadLine();
					string[] tokens = inLine.Split(commaDelim);
					uint playerID = UInt32.Parse(tokens[0]);
					PlayerRecord rec = m_ProgressData.PlayerRecords[playerID];
					PlayerEntryRecord entry = new PlayerEntryRecord();
					entry.StageIndex = m_StageIndex;
					entry.Weight = rec.Weight;
					entry.Position = tokens[2];
					entry.Experience = Byte.Parse(tokens[3]);
					entry.Position_Group = tokens[4];
					entry.Team = Byte.Parse(tokens[5]);
					entry.Loyalty = Byte.Parse(tokens[9]);
					entry.Play_for_Winner = Byte.Parse(tokens[10]);
					entry.Personality_Strength = Byte.Parse(tokens[11]);
					entry.Leadership = Byte.Parse(tokens[12]);
					entry.Intelligence = Byte.Parse(tokens[13]);
					rec.Red_Flag = Byte.Parse(tokens[14]);
					entry.Mentor = Byte.Parse(tokens[15]);
					entry.Volatility = Byte.Parse(tokens[16]);
                    int popularity = Int32.Parse(tokens[20]);
                    if (popularity < Byte.MinValue)
                    {
                        popularity = Byte.MinValue;
                    }
                    else if (popularity > Byte.MaxValue)
                    {
                        popularity = Byte.MaxValue;
                    }
                    entry.Popularity = (Byte)popularity;
                    entry.Solecismic = Byte.Parse(tokens[228]);
                    entry.Dash = UInt16.Parse(tokens[229]);
                    entry.Strength = Byte.Parse(tokens[230]);
                    entry.Agility = UInt16.Parse(tokens[231]);
                    entry.Jump = Byte.Parse(tokens[232]);
                    entry.Position_Specific = Byte.Parse(tokens[233]);
					rec.Entries.Add(entry);
				}
				inFile.Close();
			}
		}

		private void FigureOutCurrentStage()
		{
			string gameStage = "Unk";
			ushort gameYear = 0;
			string universeInfoPath = System.IO.Path.Combine(m_LeagueDir, "universe_info.csv");
			using (System.IO.StreamReader uiFile = new System.IO.StreamReader(universeInfoPath))
			{
				while (!uiFile.EndOfStream)
				{
					string uiLine = uiFile.ReadLine();
					string[] uiTokens = uiLine.Split(commaDelim);
					if (uiTokens[0] == "Game Stage")
					{
						gameStage = uiTokens[1];
						break;
					}
				}
				uiFile.Close();
			}
			foreach (string transactionsFile in System.IO.Directory.EnumerateFiles(m_LeagueDir,"transactions_????.csv"))
			{
				string transactionFileName = System.IO.Path.GetFileNameWithoutExtension(transactionsFile);
				string yearString = transactionFileName.Substring(13);
				ushort year;
				if (UInt16.TryParse(yearString, out year) && year > gameYear)
				{
					gameYear = year;
				}
			}

			m_StageIndex = UInt16.MaxValue;
			if (m_ProgressData.StageRecords.Count > 0)
			{
				int checkIndex = m_ProgressData.StageRecords.Count - 1;
				StageData checkData = m_ProgressData.StageRecords[checkIndex];
				if (checkData.Season == gameYear && checkData.Stage == gameStage)
				{
					MessageBox.Show("You have already imported " + gameYear + " " + gameStage + ", skipping import");
					return;
				}
			}
			m_StageIndex = (ushort)m_ProgressData.StageRecords.Count;
			StageData stageData = new StageData();
			stageData.Season = gameYear;
			stageData.Stage = gameStage;
			m_ProgressData.StageRecords.Add(stageData);
		}

		ushort m_MinDraftYear;
		ushort m_MaxDraftYear;
		private void FigureOutDraftYears()
		{
			m_MinDraftYear = ushort.MaxValue;
			m_MaxDraftYear = ushort.MinValue;
			foreach (PlayerRecord rec in m_ProgressData.PlayerRecords.Values)
			{
				if (rec.Draft_Year > 0 && rec.Draft_Year != ushort.MaxValue)
				{
					m_MinDraftYear = Math.Min(m_MinDraftYear, rec.Draft_Year);
					m_MaxDraftYear = Math.Max(m_MaxDraftYear, rec.Draft_Year);
				}
			}

			comboBoxDraftYear.Items.Clear();
			if (m_MaxDraftYear > ushort.MinValue)
			{
				for (ushort year = m_MaxDraftYear; year >= m_MinDraftYear; --year)
				{
					comboBoxDraftYear.Items.Add(year);
				}
			}

			comboBoxDraftYear.IsEnabled = true;
		}

		enum PlayerDisplayType
		{
			Team,
			DraftYear,
			Position
		};

		PlayerDisplayType m_DisplayType = PlayerDisplayType.Team;
        DataReader.FOFData.DefensiveFront m_DefensiveFront = DataReader.FOFData.DefensiveFront.True34;

		private void comboBoxTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_DisplayType = PlayerDisplayType.Team;
			ClearPlayerView();
			UpdatePlayerList();
		}

        private void comboBoxDefensiveFront_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_DefensiveFront = (DataReader.FOFData.DefensiveFront)comboBoxDefensiveFront.SelectedItem;
            if (m_ReportGenerator != null)
            {
                ClearPlayerView();
                UpdatePlayerList();
            }
        }

        private void comboBoxDraftYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_DisplayType = PlayerDisplayType.DraftYear;
			ClearPlayerView();
			UpdatePlayerList();
		}

		private void comboBoxPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_DisplayType = PlayerDisplayType.Position;
			ClearPlayerView();
			UpdatePlayerList();
		}

		private void ClearPlayerView()
		{
			labelPlayer.Content = "";
			labelSolecismic.Content = "-";
			labelBench.Content = "-";
			labelBroadJump.Content = "-";
			labelPosDrill.Content = "-";
			labelAgility.Content = "-";
			labelForty.Content = "-";
			labelPctDevelop.Content = "-";
			labelGrade.Content = "-";

			canvasOverall.Children.Clear();

			for (int barIndex = 0; barIndex < kMaxBarCount; ++barIndex)
			{
				Label barLabel = m_BarLabels[barIndex];
				Canvas barCanvas = m_BarCanvases[barIndex];
				barCanvas.Children.Clear();
				barLabel.Content = "";
			}
		}

		private void UpdatePlayerList()
		{
			m_PlayerList.Clear();

			if (comboBoxDraftYear.SelectedIndex >= 0)
			{
				m_ReportGenerator.DraftYear = (ushort)comboBoxDraftYear.SelectedItem;
			}
			if (comboBoxTeams.SelectedIndex >= kTeamCount)
			{
				m_ReportGenerator.TeamIndex = 99;
			}
			else if (comboBoxTeams.SelectedIndex >= 0)
			{
				m_ReportGenerator.TeamIndex = (byte)comboBoxTeams.SelectedIndex;
			}
			if (comboBoxPosition.SelectedIndex >= 0)
			{
				m_ReportGenerator.Position = (string)comboBoxPosition.SelectedValue;
			}
			switch (m_DisplayType)
			{
				case PlayerDisplayType.DraftYear:
					m_ReportGenerator.Type = ReportGenerator.ReportType.DraftYear;
					break;
				case PlayerDisplayType.Team:
					m_ReportGenerator.Type = ReportGenerator.ReportType.Team;
					break;
				case PlayerDisplayType.Position:
					m_ReportGenerator.Type = ReportGenerator.ReportType.Position;
					break;
			}
            m_ReportGenerator.DefensiveFront = m_DefensiveFront;
			m_ReportGenerator.StageIndex = m_StageIndex;

			List<PlayerListData> reportPlayers = m_ReportGenerator.Generate();
			foreach (PlayerListData data in reportPlayers)
			{
				m_PlayerList.Add(data);
			}

			AutoSizeColumns(listViewPlayers);
			SortPlayerList("Pos");
		}

		private void listViewPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listViewPlayers.SelectedIndex < 0)
			{
				ClearPlayerView();
				return;
			}
			PlayerListData data = (PlayerListData)listViewPlayers.SelectedItem;
			PlayerRecord rec = m_ProgressData.PlayerRecords[data.ID];
			string exp = data.Exp.TrimStart(new char[] { '0' });
			labelPlayer.Content = data.Name + ", " + data.Pos + ", " + exp + "yrs";
			if (rec.Draft_Round > 0)
			{
				labelPlayer.Content += "    " + rec.Draft_Year + " " + rec.Draft_Round + "(" + rec.Drafted_Position + ") by " + m_Teams[rec.Drafted_By];
			}
			else if (rec.Draft_Class > 0)
			{
				labelPlayer.Content += "    Undrafted (" + rec.Draft_Class + ")";
			}
			else
			{
				labelPlayer.Content += "    Undrafted";
			}
			labelPlayer.Content += "     Peak " + data.Peak;

			if (rec.Solecismic != Byte.MaxValue && rec.Solecismic != 0)
			{
				labelSolecismic.Content = rec.Solecismic.ToString();
				labelSolecismicScore.Content = data.SolecismicRating.ToString("F1");
				ColorCombine(labelSolecismic, data.PosGrp, rec.Solecismic, DataReader.FOFData.CombineOrder.Solecismic);
			}
			else
			{
				labelSolecismic.Content = "-";
				labelSolecismic.Foreground = Brushes.Black;
				labelSolecismic.Background = this.Background;
				labelSolecismicScore.Content = "";
			}
			if (rec.Strength != Byte.MaxValue && rec.Strength != 0)
			{
				labelBench.Content = rec.Strength.ToString();
				labelBenchScore.Content = data.BenchRating.ToString("F1");
				ColorCombine(labelBench, data.PosGrp, rec.Strength, DataReader.FOFData.CombineOrder.Bench);
			}
			else
			{
				labelBench.Content = "-";
				labelBench.Foreground = Brushes.Black;
				labelBench.Background = this.Background;
				labelBenchScore.Content = "";
			}
			if (rec.Jump != Byte.MaxValue && rec.Jump != 0)
			{
				labelBroadJump.Content = rec.Jump.ToString();
				labelBroadJumpScore.Content = data.BroadJumpRating.ToString("F1");
				ColorCombine(labelBroadJump, data.PosGrp, rec.Jump, DataReader.FOFData.CombineOrder.BroadJump);
			}
			else
			{
				labelBroadJump.Content = "-";
				labelBroadJump.Foreground = Brushes.Black;
				labelBroadJump.Background = this.Background;
				labelBroadJumpScore.Content = "";
			}
			if (rec.Position_Specific != Byte.MaxValue && rec.Position_Specific != 0)
			{
				labelPosDrill.Content = rec.Position_Specific.ToString();
				labelPosDrillScore.Content = data.PositionDrillRating.ToString("F1");
				ColorCombine(labelPosDrill, data.PosGrp, rec.Position_Specific, DataReader.FOFData.CombineOrder.PositionDrill);
			}
			else
			{
				labelPosDrill.Content = "-";
				labelPosDrill.Foreground = Brushes.Black;
				labelPosDrill.Background = this.Background;
				labelPosDrillScore.Content = "";
			}
			if (rec.Agility != UInt16.MaxValue && rec.Agility != 0)
			{
				labelAgility.Content = (rec.Agility / 100.0).ToString("F2");
				labelAgilityScore.Content = data.AgilityRating.ToString("F1");
				ColorCombine(labelAgility, data.PosGrp, rec.Agility, DataReader.FOFData.CombineOrder.Agility);
			}
			else
			{
				labelAgility.Content = "-";
				labelAgility.Foreground = Brushes.Black;
				labelAgility.Background = this.Background;
				labelAgilityScore.Content = "";
			}
			if (rec.Dash != UInt16.MaxValue && rec.Dash != 0)
			{
				labelForty.Content = (rec.Dash / 100.0).ToString("F2");
				labelFortyScore.Content = data.FortyRating.ToString("F1");
				ColorCombine(labelForty, data.PosGrp, rec.Dash, DataReader.FOFData.CombineOrder.Dash);
			}
			else
			{
				labelForty.Content = "-";
				labelForty.Foreground = Brushes.Black;
				labelForty.Background = this.Background;
				labelFortyScore.Content = "";
			}
			if (rec.Developed != Byte.MaxValue)
			{
				labelPctDevelop.Content = rec.Developed.ToString() + "%";
			}
			else
			{
				labelPctDevelop.Content = "-";
			}
			if (rec.Grade != Byte.MaxValue)
			{
				labelGrade.Content = (rec.Grade / 10.0).ToString("F1");
			}
			else
			{
				labelGrade.Content = "-";
			}

			labelDraftWeights.Content = "Comb: " + data.CombineScore.ToString("F1") + " Bars: " + data.AttributeScore.ToString("F1") + 
				" Tot: " + data.OverallScore.ToString("F1");

            string heightString = m_FOFData.GetHeightDifference(data.Pos, data.PlayerRecord.Height).ToString("+#;-#;0") + "\"";
            var startIdealPos = m_FOFData.GetIdealPosition(data.Pos, data.StartWeight);
            string dWeightDiffString = m_FOFData.GetWeightDifference(data.Pos, data.StartWeight, m_DefensiveFront).ToString("+#;-#;0") + "lbs";
            string dWeightString = m_FOFData.GetWeightDifference(startIdealPos.Position, data.StartWeight, startIdealPos.Formation).ToString("+#;-#;0") + "lbs";
            var peakIdealPos = m_FOFData.GetIdealPosition(data.Pos, data.CurWeight);
            string pWeightDiffString = m_FOFData.GetWeightDifference(data.Pos, data.CurWeight, m_DefensiveFront).ToString("+#;-#;0") + "lbs";
            string pWeightString = m_FOFData.GetWeightDifference(peakIdealPos.Position, data.CurWeight, peakIdealPos.Formation).ToString("+#;-#;0") + "lbs";

            labelHeight.Content = "Ht: " + data.PlayerRecord.Height.ToString() + "\"" + " (" + heightString + ")";
            labelDWeight.Content = "Draft Wt: " + data.StartWeight + " (" + dWeightDiffString +  ") (" + startIdealPos.Display + " " + dWeightString + ")";
            labelPWeight.Content = "Cur Wt: " + data.CurWeight + " (" + pWeightDiffString + ") (" + peakIdealPos.Display + " " + pWeightString + ")";

			int[] barIndices = m_FOFData.PositionGroupAttributes[data.PosGrp];
			byte[] initialBars = new byte[barIndices.Length];
			byte[] peakBars = new byte[barIndices.Length];
			byte[] curBars = new byte[barIndices.Length];
			byte[] futBars = new byte[barIndices.Length];

			for (int i = 0; i < barIndices.Length; ++i )
			{
				initialBars[i] = Byte.MaxValue;
				curBars[i] = Byte.MaxValue;
				futBars[i] = Byte.MaxValue;
				peakBars[i] = 0;
			}

			canvasOverall.Children.Clear();
			int entryCount = rec.Entries.Count;
			const int kBarWidth = 12;
			double barX = 5;
			bool firstEntry = true;
			string lastSeason = "";
			int seasonEntry = 0;
			int charBase = (int)'a';

			foreach (PlayerEntryRecord entry in rec.Entries)
			{
				if (entry.CurOverall == Byte.MaxValue)
				{
					continue;
				}

				for (int barIndex = 0; barIndex < barIndices.Length; ++barIndex)
				{
					byte curValue = entry.CurBars[barIndices[barIndex]];
					if (curValue != Byte.MaxValue)
					{
						if (firstEntry)
						{
							initialBars[barIndex] = curValue;
						}
						curBars[barIndex] = curValue;
						futBars[barIndex] = entry.FutBars[barIndices[barIndex]];
						if (curValue > peakBars[barIndex])
						{
							peakBars[barIndex] = curValue;
						}
					}
				}
				firstEntry = false;

				Rectangle totalRect = new Rectangle();
				totalRect.Width = kBarWidth;
				totalRect.Height = 100;
				totalRect.Stroke = Brushes.Black;
				totalRect.StrokeThickness = 1;
				Canvas.SetLeft(totalRect, barX);
				Canvas.SetTop(totalRect, 0);
				canvasOverall.Children.Add(totalRect);

				double futureStart = 100 - entry.FutOverall;
				double curStart = 100 - entry.CurOverall;
				Rectangle futureRect = new Rectangle();
				futureRect.Width = kBarWidth;
				futureRect.Height = entry.FutOverall;
				futureRect.Stroke = Brushes.Black;
				futureRect.StrokeThickness = 1;
				futureRect.Fill = Brushes.Green;
				Canvas.SetLeft(futureRect, barX);
				Canvas.SetTop(futureRect, futureStart);
				canvasOverall.Children.Add(futureRect);

				Rectangle curRect = new Rectangle();
				curRect.Width = kBarWidth;
				curRect.Height = entry.CurOverall;
				curRect.Stroke = Brushes.Black;
				curRect.StrokeThickness = 1;
				curRect.Fill = Brushes.Red;
				Canvas.SetLeft(curRect, barX);
				Canvas.SetTop(curRect, curStart);
				canvasOverall.Children.Add(curRect);

				string yearString = (m_ProgressData.StageRecords[entry.StageIndex].Season % 100).ToString();
				if (yearString != lastSeason)
				{
					seasonEntry = 0;
				}
				else
				{
					seasonEntry++;
				}
				lastSeason = yearString;
				yearString += Convert.ToChar(charBase + seasonEntry);
				TextBlock seasonNote = new TextBlock();
				seasonNote.LayoutTransform = new RotateTransform(90.0);
				seasonNote.Width = 30;
				seasonNote.Height = kBarWidth;
				seasonNote.FontSize = kBarWidth-2;
				seasonNote.Text = yearString;
				seasonNote.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				seasonNote.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				Canvas.SetLeft(seasonNote, barX);
				Canvas.SetTop(seasonNote, 100);
				canvasOverall.Children.Add(seasonNote);

				barX += kBarWidth;
			}

			for (int barIndex=0;barIndex<kMaxBarCount;++barIndex)
			{
				Label barLabel = m_BarLabels[barIndex];
				Canvas barCanvas = m_BarCanvases[barIndex];
				Label curFutLabel = m_CurFutLabels[barIndex];
				Label peakLabel = m_PeakLabels[barIndex];
				Label draftLabel = m_DraftLabels[barIndex];

				barCanvas.Children.Clear();

				curFutLabel.Content = "";
				peakLabel.Content = "";
				draftLabel.Content = "";

				if (barIndex < barIndices.Length)
				{
					int positionBarIndex = barIndices[barIndex];

					string attributeName = ((DataReader.FOFData.ScoutBars)positionBarIndex).ToString().Replace('_', ' ');
					barLabel.Content = attributeName;

					byte draftMin = rec.DraftLowBars[positionBarIndex];
					byte draftMax = rec.DraftHighBars[positionBarIndex];
					byte cur = curBars[barIndex];
					byte fut = futBars[barIndex];
					byte peak = peakBars[barIndex];
					byte initial = initialBars[barIndex];

					if (draftMin != Byte.MaxValue && draftMax != 0)
					{
						Rectangle draft = new Rectangle();
						draft.Fill = Brushes.LightBlue;
						draft.Height = barCanvas.ActualHeight - 2;
						draft.Width = draftMax - draftMin + 1;
						Canvas.SetTop(draft, 1);
						Canvas.SetLeft(draft, 1 + draftMin);
						barCanvas.Children.Add(draft);
						draftLabel.Content = draftMin.ToString() + "/" + draftMax.ToString();
					}

					if (fut != Byte.MaxValue)
					{
						Rectangle draft = new Rectangle();
						draft.Fill = Brushes.Green;
						draft.Height = barCanvas.ActualHeight - 14;
						draft.Width = fut;
						Canvas.SetTop(draft, 7);
						Canvas.SetLeft(draft, 1);
						barCanvas.Children.Add(draft);
					}

					if (peak != Byte.MaxValue)
					{
						Rectangle peakBar = new Rectangle();
						peakBar.Fill = Brushes.Goldenrod;
						peakBar.Height = barCanvas.ActualHeight - 10;
						peakBar.Width = peak;
						Canvas.SetTop(peakBar, 5);
						Canvas.SetLeft(peakBar, 1);
						barCanvas.Children.Add(peakBar);
						peakLabel.Content = peak.ToString();
					}

					if (cur != Byte.MaxValue)
					{
						Rectangle curBar = new Rectangle();
						curBar.Fill = Brushes.Red;
						curBar.Height = barCanvas.ActualHeight - 10;
						curBar.Width = cur;
						Canvas.SetTop(curBar, 5);
						Canvas.SetLeft(curBar, 1);
						barCanvas.Children.Add(curBar);

						if (fut != Byte.MaxValue)
						{
							curFutLabel.Content = cur.ToString() + "/" + fut.ToString();
						}
						else
						{
							curFutLabel.Content = cur.ToString();
						}
					}

					Rectangle outline = new Rectangle();
					outline.Stroke = Brushes.Black;
					outline.StrokeThickness = 1;
					outline.Height = barCanvas.ActualHeight;
					outline.Width = 102;
					Canvas.SetTop(outline, 0);
					Canvas.SetLeft(outline, 0);
					barCanvas.Children.Add(outline);
				}
				else
				{
					barLabel.Content = "";
				}
			}
		}

		private void ColorCombine(Label label, string pos, ushort value, DataReader.FOFData.CombineOrder combine)
		{
			int combineIndex = (int)combine;
			int positionIndex = m_FOFData.PositionGroupOrderMap[pos];
			int redIndex = combineIndex * 3;
			int blueIndex = redIndex + 1;
			int greenIndex = blueIndex + 1;
			label.Background = this.Background;
			if (pos == "LS")
			{
				label.Foreground = Brushes.Black;
			}
			else if (combineIndex == 0 || combineIndex == 3)
			{
				if (m_FOFData.CombineThresholds[positionIndex, combineIndex] != 0 && value > m_FOFData.CombineThresholds[positionIndex, combineIndex])
				{
					label.Foreground = Brushes.Black;
					label.Background = Brushes.Red;
				}
				else if (value <= m_FOFData.CombineColors[positionIndex, redIndex])
				{
					label.Foreground = Brushes.Red;
				}
				else if (value <= m_FOFData.CombineColors[positionIndex, blueIndex])
				{
					label.Foreground = Brushes.Blue;
				}
				else if (value >= m_FOFData.CombineColors[positionIndex, greenIndex])
				{
					label.Foreground = Brushes.DarkSeaGreen;
				}
				else
				{
					label.Foreground = Brushes.Black;
				}
			}
			else
			{
				if (m_FOFData.CombineThresholds[positionIndex, combineIndex] != 0 && value < m_FOFData.CombineThresholds[positionIndex, combineIndex])
				{
					label.Foreground = Brushes.Black;
					label.Background = Brushes.Red;
				}
				else if (value >= m_FOFData.CombineColors[positionIndex, redIndex])
				{
					label.Foreground = Brushes.Red;
				}
				else if (value >= m_FOFData.CombineColors[positionIndex, blueIndex])
				{
					label.Foreground = Brushes.Blue;
				}
				else if (value <= m_FOFData.CombineColors[positionIndex, greenIndex])
				{
					label.Foreground = Brushes.DarkSeaGreen;
				}
				else
				{
					label.Foreground = Brushes.Black;
				}
			}
		}

		private void listViewPlayers_GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked =
				  e.OriginalSource as GridViewColumnHeader;

			if (headerClicked != null)
			{
				string header = headerClicked.Column.Header as string;
				SortPlayerList(header);
			}
		}

		private void SortPlayerList(string header)
		{
			ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(listViewPlayers.ItemsSource);
			view.SortDescriptions.Clear();
			view.CustomSort = null;
			if (header == "Recent")
			{
				view.CustomSort = new DescendingRecentSorter();
			}
			else if (header == "Overall")
			{
				view.CustomSort = new DescendingOverallSorter();
			}
			else if (header == "Pos")
			{
				view.CustomSort = new AscendingPositionSorter();
			}
			else if (header == "Name")
			{
				view.SortDescriptions.Add(new SortDescription(header, ListSortDirection.Ascending));
			}
            else if (header == "Ht")
            {
                view.CustomSort = new DescendingHeightSorter();
            }
            else if (header == "Wt")
            {
                view.CustomSort = new DescendingWeightSorter();
            }
            else if (header == "Comb")
			{
				view.CustomSort = new DescendingCombineScoreSorter();
			}
			else if (header == "Bars")
			{
				view.CustomSort = new DescendingAttributeScoreSorter();
			}
			else if (header == "Score")
			{
				view.CustomSort = new DescendingOverallScoreSorter();
			}
            else if (header == "Dash")
            {
                view.CustomSort = new AscendingFortySorter();
            }
            else if (header == "Sol")
            {
                view.CustomSort = new DescendingSolecismicSorter();
            }
            else if (header == "Agil")
            {
                view.CustomSort = new AscendingAgilitySorter();
            }
            else if (header == "Drill")
            {
                view.CustomSort = new DescendingPositionDrillSorter();
            }
            else if (header == "Jump")
            {
                view.CustomSort = new DescendingBroadJumpSorter();
            }
            else if (header == "Bench")
            {
                view.CustomSort = new DescendingBenchSorter();
            }
            else
            {
				view.SortDescriptions.Add(new SortDescription(header, ListSortDirection.Descending));
			}
			view.Refresh();
		}

		private void AutoSizeColumns(ListView listView)
		{
			GridView gv = listView.View as GridView;
			if (gv != null)
			{
				foreach (var c in gv.Columns)
				{
					// Code below was found in GridViewColumnHeader.OnGripperDoubleClicked() event handler (using Reflector)
					// i.e. it is the same code that is executed when the gripper is double clicked
					if (double.IsNaN(c.Width))
					{
						c.Width = c.ActualWidth;
					}
					c.Width = double.NaN;
				}
			}
		}

		private void CreateMaps()
		{
			m_BarLabels[0] = labelBar0;
			m_BarLabels[1] = labelBar1;
			m_BarLabels[2] = labelBar2;
			m_BarLabels[3] = labelBar3;
			m_BarLabels[4] = labelBar4;
			m_BarLabels[5] = labelBar5;
			m_BarLabels[6] = labelBar6;
			m_BarLabels[7] = labelBar7;
			m_BarLabels[8] = labelBar8;
			m_BarLabels[9] = labelBar9;
			m_BarLabels[10] = labelBar10;
			m_BarLabels[11] = labelBar11;
			m_BarLabels[12] = labelBar12;
			m_BarLabels[13] = labelBar13;
			m_BarLabels[14] = labelBar14;

			m_BarCanvases[0] = canvasBar0;
			m_BarCanvases[1] = canvasBar1;
			m_BarCanvases[2] = canvasBar2;
			m_BarCanvases[3] = canvasBar3;
			m_BarCanvases[4] = canvasBar4;
			m_BarCanvases[5] = canvasBar5;
			m_BarCanvases[6] = canvasBar6;
			m_BarCanvases[7] = canvasBar7;
			m_BarCanvases[8] = canvasBar8;
			m_BarCanvases[9] = canvasBar9;
			m_BarCanvases[10] = canvasBar10;
			m_BarCanvases[11] = canvasBar11;
			m_BarCanvases[12] = canvasBar12;
			m_BarCanvases[13] = canvasBar13;
			m_BarCanvases[14] = canvasBar14;

			m_CurFutLabels[0] = labelCurFut0;
			m_CurFutLabels[1] = labelCurFut1;
			m_CurFutLabels[2] = labelCurFut2;
			m_CurFutLabels[3] = labelCurFut3;
			m_CurFutLabels[4] = labelCurFut4;
			m_CurFutLabels[5] = labelCurFut5;
			m_CurFutLabels[6] = labelCurFut6;
			m_CurFutLabels[7] = labelCurFut7;
			m_CurFutLabels[8] = labelCurFut8;
			m_CurFutLabels[9] = labelCurFut9;
			m_CurFutLabels[10] = labelCurFut10;
			m_CurFutLabels[11] = labelCurFut11;
			m_CurFutLabels[12] = labelCurFut12;
			m_CurFutLabels[13] = labelCurFut13;
			m_CurFutLabels[14] = labelCurFut14;

			m_PeakLabels[0] = labelPeak0;
			m_PeakLabels[1] = labelPeak1;
			m_PeakLabels[2] = labelPeak2;
			m_PeakLabels[3] = labelPeak3;
			m_PeakLabels[4] = labelPeak4;
			m_PeakLabels[5] = labelPeak5;
			m_PeakLabels[6] = labelPeak6;
			m_PeakLabels[7] = labelPeak7;
			m_PeakLabels[8] = labelPeak8;
			m_PeakLabels[9] = labelPeak9;
			m_PeakLabels[10] = labelPeak10;
			m_PeakLabels[11] = labelPeak11;
			m_PeakLabels[12] = labelPeak12;
			m_PeakLabels[13] = labelPeak13;
			m_PeakLabels[14] = labelPeak14;

			m_DraftLabels[0] = labelDraft0;
			m_DraftLabels[1] = labelDraft1;
			m_DraftLabels[2] = labelDraft2;
			m_DraftLabels[3] = labelDraft3;
			m_DraftLabels[4] = labelDraft4;
			m_DraftLabels[5] = labelDraft5;
			m_DraftLabels[6] = labelDraft6;
			m_DraftLabels[7] = labelDraft7;
			m_DraftLabels[8] = labelDraft8;
			m_DraftLabels[9] = labelDraft9;
			m_DraftLabels[10] = labelDraft10;
			m_DraftLabels[11] = labelDraft11;
			m_DraftLabels[12] = labelDraft12;
			m_DraftLabels[13] = labelDraft13;
			m_DraftLabels[14] = labelDraft14;
		}
    }
}

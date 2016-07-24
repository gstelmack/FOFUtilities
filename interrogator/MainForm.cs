using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DataReader;

namespace Interrogator
{
	public partial class MainForm : Form
	{
		delegate void AddStatusTextCallback(string text);
		delegate void WorkCompletedCallback();

		private UniverseData mUniverseData;
		private LeagueData mLeagueData;

		private int mSavedGameIndex = -1;

		private const string kSettingsRoot = "Interrogator";
		private const string kUseProcessSeason = "UseProcessSeason";
		private const string kSeasonToProcessFrom = "SeasonToProcessFrom";
		private const string kOutputDirectory = "OutputDirectory";

		private WindowsUtilities.XMLSettings mSettings;

		private Cursor mOldCursor = null;

		public MainForm()
		{
			InitializeComponent();

			Assembly a = typeof(MainForm).Assembly;
			Text += " v" + a.GetName().Version;

			mUniverseData = new UniverseData();
			mStartingSeason = 0;

			comboBoxSavedGame.Items.Add("(None)");
			for (int i = 0; i < mUniverseData.SavedGames.Count; i++)
			{
				comboBoxSavedGame.Items.Add(mUniverseData.SavedGames[i].GameName);
			}
			if (mUniverseData.SavedGames.Count > 0)
			{
				comboBoxSavedGame.SelectedIndex = 0;
			}

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "Interrogator.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);
			checkBoxProcessFromSeason.Checked = mSettings.ReadXMLbool(kSettingsRoot, kUseProcessSeason, false);
			textBoxStartingSeason.Text = mSettings.ReadXMLString(kSettingsRoot, kSeasonToProcessFrom, "");
			buttonOutputDirectory.Text = mSettings.ReadXMLString(kSettingsRoot, kOutputDirectory, "");
		}

		private void UpdateThreadTask()
		{
			AddStatusString("Loading game data...");

			LeagueData.DumpFieldGoalPlays = checkBoxDumpFieldGoalPlays.Checked;
			LeagueData.DumpInfoPlays = checkBoxDumpInfoPlays.Checked;
			LeagueData.DumpOnsideKickPlays = checkBoxDumpOnsideKickPlays.Checked;
			LeagueData.DumpPuntPlays = checkBoxDumpPuntPlays.Checked;
			LeagueData.DumpPassPlays = checkBoxDumpPassPlays.Checked;
			LeagueData.DumpRunPlays = checkBoxDumpRunPlays.Checked;

			mLeagueData = new LeagueData(mUniverseData.SavedGames[mSavedGameIndex].PathPrefix, mUniverseData, mStartingSeason, OnFileRead, true);
			CreateDataTables();
			CreateGameDataTables();

			AddStatusString("Finished!");

			WorkCompleted();
		}


		private void AddStatusString(string newText)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.labelOutputStatus.InvokeRequired)
			{
				AddStatusTextCallback d = new AddStatusTextCallback(AddStatusString);
				this.Invoke(d, new object[] { newText });
			}
			else
			{
				labelOutputStatus.Text = newText;
				Refresh();
			}
		}

		private void WorkCompleted()
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.InvokeRequired)
			{
				WorkCompletedCallback d = new WorkCompletedCallback(WorkCompleted);
				this.Invoke(d);
			}
			else
			{
				Cursor = mOldCursor;
				buttonGenerateCSV.Enabled = true;
			}
		}

		private void OnFileRead(string fileName)
		{
			AddStatusString("Reading file " + fileName + System.Environment.NewLine);
		}

		private void WriteString(System.IO.StreamWriter outFile, string item)
		{
			string tmpItem = ((string)item).Replace('"', '\'');
			outFile.Write("\"" + tmpItem + "\"");
		}

		private void CreateCityDataTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Cities.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Index,");
			outFile.Write("Name,");
			outFile.Write("Abbrev,");
			outFile.Write("Pop,");
			outFile.Write("Growth,");
			outFile.Write("Avg Inc,");
			outFile.Write("Pov Lvl,");
			outFile.Write("Ent Comp,");
			outFile.Write("Sept Hi,");
			outFile.Write("Sept Low,");
			outFile.Write("Sept Hum,");
			outFile.Write("Dec Hi,");
			outFile.Write("Dec Low,");
			outFile.Write("Dec Hum,");
			outFile.Write("90Deg Days,");
			outFile.Write("Snow Days,");
			outFile.Write("Storm Days,");
			outFile.Write("Elev,");
			outFile.Write("Long,");
			outFile.Write("Lat,");
			outFile.Write("Region,");
			outFile.Write("State,");
			outFile.WriteLine("WantsTeam");

			int i = 0;
			foreach (UniverseData.CityRecord cityRecord in mUniverseData.CityRecords)
			{
				outFile.Write(i + ",");
				WriteString(outFile, cityRecord.Name);
				outFile.Write(",");
				WriteString(outFile, cityRecord.Abbrev);
				outFile.Write(",");
				outFile.Write(cityRecord.Population * 1000 + ",");
				outFile.Write((((double)cityRecord.GrowthRate - 100.0)/10.0) + ",");
				outFile.Write(cityRecord.AverageIncome * 100 + ",");
				outFile.Write(((double)cityRecord.PovertyLevel/10.0) + ",");
				outFile.Write(cityRecord.EntertainmentCompetiton + ",");
				outFile.Write(cityRecord.SeptemberHigh + ",");
				outFile.Write(cityRecord.SeptemberLow + ",");
				outFile.Write(cityRecord.SeptemberHumidity + ",");
				outFile.Write(cityRecord.DecemberHigh + ",");
				outFile.Write(cityRecord.DecemberLow + ",");
				outFile.Write(cityRecord.DecemberHumidity + ",");
				outFile.Write(cityRecord.NinetyDegreeDays + ",");
				outFile.Write(cityRecord.SnowDays + ",");
				outFile.Write(cityRecord.StormyDays + ",");
				outFile.Write(cityRecord.Elevation + ",");
				outFile.Write(((double)cityRecord.Longitude/100.0) + ",");
				outFile.Write(((double)cityRecord.Latitude/100.0) + ",");
				outFile.Write(cityRecord.Region + ",");
				outFile.Write(cityRecord.State + ",");
				outFile.WriteLine(cityRecord.WantsNewTeam);
				++i;
			}

			outFile.Close();
		}

		private void CreateTeamsDataTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Teams.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Index,");
			outFile.Write("CityIndex,");
			outFile.Write("TeamName,");
			outFile.Write("Conf,");
			outFile.Write("Div,");
			outFile.Write("Year?,");
			outFile.Write("Human Control,");
			outFile.Write("Cap Loss This Year,");
			outFile.WriteLine("Cap Loss Next Year");

			int i = 0;
			foreach (UniverseData.TeamRecord teamRecord in mUniverseData.TeamRecords)
			{
				outFile.Write(i + ",");
				outFile.Write(teamRecord.CityIndex + ",");
				WriteString(outFile,teamRecord.Name);
				outFile.Write(",");
				outFile.Write(teamRecord.ConferenceID + ",");
				outFile.Write(teamRecord.DivisionID + ",");
				outFile.Write(teamRecord.Year + ",");
				outFile.Write(mLeagueData.TeamRecords[i].HumanControlled + ",");
				outFile.Write(mLeagueData.TeamRecords[i].CapLossThisYear * 10000 + ",");
				outFile.WriteLine(mLeagueData.TeamRecords[i].CapLossNextYear * 10000);
				++i;
			}

			outFile.Close();
		}

		private void CreateInjuriesDataTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Injuries.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.WriteLine("Index,Name");

			int i = 0;
			foreach (UniverseData.InjuryRecord injuryRecord in mUniverseData.InjuryRecords)
			{
				outFile.Write(i + ",");
				WriteString(outFile, injuryRecord.Name);
				outFile.WriteLine();
				++i;
			}

			outFile.Close();
		}

		private void CreateCollegeNamesDataTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Colleges.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.WriteLine("Index,Name");

			int i = 0;
			foreach (string collegeName in mUniverseData.CollegeNames)
			{
				outFile.Write(i + ",");
				WriteString(outFile, collegeName);
				outFile.WriteLine();
				++i;
			}

			outFile.Close();
		}

		private void CreateMappingsDataTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Mappings.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Index,");
			outFile.Write("Position,");
			outFile.Write("PositionGroup,");
			outFile.Write("TransactionType,");
			outFile.Write("Precipitation,");
			outFile.Write("PlayerStatus,");
			outFile.Write("StaffRole,");
			outFile.Write("Playoffs,");
			outFile.Write("Ability,");
			outFile.Write("StadiumType,");
			outFile.Write("ConstructionType,");
			outFile.WriteLine("DriveResult");

			for (int i=0;i<101;i++)
			{
				outFile.Write(i + ",");

				if (i < mUniverseData.PositionMap.Length)
				{
					WriteString(outFile, mUniverseData.PositionMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.PositionGroupMap.Length)
				{
					WriteString(outFile, mUniverseData.PositionGroupMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.TransactionTypeMap.Length)
				{
					WriteString(outFile, mUniverseData.TransactionTypeMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.PrecipMap.Length)
				{
					WriteString(outFile, mUniverseData.PrecipMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.PlayerStatusMap.Length)
				{
					WriteString(outFile, mUniverseData.PlayerStatusMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.StaffRoleMap.Length)
				{
					WriteString(outFile, mUniverseData.StaffRoleMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.PlayoffsMap.Length)
				{
					WriteString(outFile, mUniverseData.PlayoffsMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.AbilityMap.Length)
				{
					WriteString(outFile, mUniverseData.AbilityMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.StadiumTypeMap.Length)
				{
					WriteString(outFile, mUniverseData.StadiumTypeMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.ConstructionTypeMap.Length)
				{
					WriteString(outFile, mUniverseData.ConstructionTypeMap[i]);
				}
				outFile.Write(",");

				if (i < mUniverseData.DriveResultMap.Length)
				{
					WriteString(outFile, mUniverseData.DriveResultMap[i]);
				}
				outFile.WriteLine();
			}

			outFile.Close();
		}

		private void CreateHometownNamesDataTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Hometowns.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.WriteLine("Index,Name");

			int i = 0;
			foreach (string townName in mUniverseData.HometownNames)
			{
				outFile.Write(i + ","); 
				WriteString(outFile,townName);
				outFile.WriteLine();
				++i;
			}

			outFile.Close();
		}

		private void CreateGameInfoTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Game Info.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("StartYear,");
			outFile.Write("CurYear,");
			outFile.Write("Stage,");
			outFile.Write("Week,");
			outFile.Write("FA Stage,");
			outFile.Write("Player Team,");
			outFile.Write("Team Count,");
			outFile.Write("Salary Cap,");
			outFile.WriteLine("Min Salary");

			outFile.Write(mLeagueData.StartingYear + ",");
			outFile.Write(mLeagueData.CurrentYear + ",");
			outFile.Write(mLeagueData.GameStage + ",");
			outFile.Write(mLeagueData.CurrentWeek + ",");
			outFile.Write(mLeagueData.FAStage + ",");
			outFile.Write(mLeagueData.PlayersTeam + ",");
			outFile.Write(mLeagueData.NumberOfTeams + ",");
			outFile.Write(mLeagueData.SalaryCap * 10000 + ",");
			outFile.WriteLine(mLeagueData.MinSalary * 10000);

			outFile.Close();
		}

		private void CreateGameDraftTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Draft.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Year,");
			outFile.Write("Round,");
			outFile.Write("Pick,");
			outFile.WriteLine("Team");

			int year = 0;
			int round = 0;
			int pick = 0;
			foreach (LeagueData.DraftYear yearRec in mLeagueData.DraftYears)
			{
				round = 0;
				foreach (LeagueData.DraftRound roundRec in yearRec.DraftRounds)
				{
					pick = 0;
					foreach (short teamIndex in roundRec.PickTeam)
					{
						outFile.Write(year + ",");
						outFile.Write(round + 1 + ",");
						outFile.Write(pick + 1 + ",");
						outFile.WriteLine(teamIndex);
						++pick;
					}
					++round;
				}
				++year;
			}

			outFile.Close();
		}

		private void CreateDraftOrderTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Draft Order.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			int pickIndex = 1;
			for (int round = 0; round < 7; round++)
			{
				for (int pick = 0; pick < 32; pick++)
				{
					short zeroIndex = mLeagueData.DraftYears[1].DraftRounds[round].PickTeam[pick];
					short teamIndex = mLeagueData.DraftYears[0].DraftRounds[round].PickTeam[zeroIndex];
					outFile.Write(pickIndex + ",");
					outFile.WriteLine(teamIndex);
					++pickIndex;
				}
			}
			outFile.WriteLine("225,33");
			outFile.Close();
		}

		private void CreateGameTransactionTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Transactions.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Trans Index,");
			outFile.Write("Player Index,");
			outFile.Write("Salary,");
			outFile.Write("Trans Type,");
			outFile.Write("Team 1 Index,");
			outFile.Write("Team 2 Index,");
			outFile.Write("Position,");
			outFile.Write("Years,");
			outFile.Write("Stage,");
			outFile.WriteLine("Season");

			int transactionIndex = 0;
			for (int yearIndex = 0; yearIndex < mLeagueData.SeasonsPlayed; ++yearIndex)
			{
				int season = yearIndex + mLeagueData.StartingYear;
				if (mLeagueData.Transactions[yearIndex] != null && season >= mStartingSeason)
				{
					foreach (LeagueData.TransactionRecord rec in mLeagueData.Transactions[yearIndex])
					{
						outFile.Write(transactionIndex + ",");
						outFile.Write(rec.PlayerRec2Index + ",");
						outFile.Write(rec.Salary + ",");
						outFile.Write(rec.TransactionType + ",");
						outFile.Write(rec.Team1Index + ",");
						outFile.Write(rec.Team2Index + ",");
						outFile.Write(rec.Position + ",");
						outFile.Write(rec.Years + ",");
						outFile.Write(rec.Stage + ",");
						outFile.WriteLine(season);
						++transactionIndex;
					}
				}
			}
			outFile.Close();
		}

		private void CreateGamePlayerHistoricalTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Player Historical.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Index,");
			outFile.Write("PlayerID,");
			outFile.Write("Position,");
			outFile.Write("LastName,");
			outFile.Write("FirstName,");
			outFile.Write("NickName,");
			outFile.Write("Experience,");
			outFile.Write("Height,");
			outFile.Write("Weight,");
			outFile.Write("InHallOfFame,");
			outFile.Write("HallOfFameYear,");
			outFile.Write("HallOfFameVote,");
			outFile.Write("BirthYear,");
			outFile.Write("BirthMonth,");
			outFile.Write("BirthDay,");
			outFile.Write("College,");
			outFile.Write("DraftRound,");
			outFile.Write("DraftPick,");
			outFile.Write("HomeTown,");
			outFile.Write("DraftedBy,");
			outFile.Write("YearDraft,");
			outFile.Write("PoG,");
			outFile.Write("Rings,");
			outFile.Write("PoWMentions,");
			outFile.Write("PoWWins,");
			outFile.Write("4QHero,");
			outFile.Write("QBWins,");
			outFile.Write("QBLosses,");
			outFile.Write("QBTies,");
			outFile.Write("YearCount");
			for (int year = 0; year < LeagueData.MaxPlayerHistoricalYearCount; year++)
			{
				outFile.Write(",Year" + year.ToString());
			}
			outFile.WriteLine();

			int i = 0;
			foreach (LeagueData.PlayerHistoricalRecord rec in mLeagueData.PlayerHistoricalRecords)
			{
				outFile.Write(i + ",");
				outFile.Write(rec.PlayerID + ",");
				outFile.Write(rec.Position + ",");
				WriteString(outFile,rec.LastName);
				outFile.Write(",");
				WriteString(outFile,rec.FirstName);
				outFile.Write(",");
				WriteString(outFile,rec.NickName);
				outFile.Write(",");
				outFile.Write(rec.Experience + ",");
				outFile.Write(rec.Height + ",");
				outFile.Write(rec.Weight + ",");
				outFile.Write(rec.InHallOfFame + ",");
				outFile.Write(rec.HallOfFameYear + ",");
				outFile.Write(rec.HallOfFameVote + ",");
				outFile.Write(rec.BirthYear + ",");
				outFile.Write(rec.BirthMonth + ",");
				outFile.Write(rec.BirthDay + ",");
				outFile.Write(rec.College + ",");
				outFile.Write(rec.DraftRound + ",");
				outFile.Write(rec.DraftPick + ",");
				outFile.Write(rec.HomeTown + ",");
				outFile.Write(rec.DraftedBy + ",");
				outFile.Write(rec.YearDrafted + ",");
				outFile.Write(rec.PlayerOfTheGame + ",");
				outFile.Write(rec.ChampionshipRings + ",");
				outFile.Write(rec.PlayerOfTheWeekMentions + ",");
				outFile.Write(rec.PlayerOfTheWeekWins + ",");
				outFile.Write(rec.FourthQuarterHeroics + ",");
				outFile.Write(rec.QBWins + ",");
				outFile.Write(rec.QBLosses + ",");
				outFile.Write(rec.QBTies + ",");
				outFile.Write(rec.YearsInLeagueCount + ",");
				outFile.Write(rec.YearsInLeague[0] + ",");
				outFile.Write(rec.YearsInLeague[1] + ",");
				outFile.Write(rec.YearsInLeague[2] + ",");
				outFile.Write(rec.YearsInLeague[3] + ",");
				outFile.Write(rec.YearsInLeague[4] + ",");
				outFile.Write(rec.YearsInLeague[5] + ",");
				outFile.Write(rec.YearsInLeague[6] + ",");
				outFile.Write(rec.YearsInLeague[7] + ",");
				outFile.Write(rec.YearsInLeague[8] + ",");
				outFile.Write(rec.YearsInLeague[9] + ",");
				outFile.Write(rec.YearsInLeague[10] + ",");
				outFile.Write(rec.YearsInLeague[11] + ",");
				outFile.Write(rec.YearsInLeague[12] + ",");
				outFile.Write(rec.YearsInLeague[13] + ",");
				outFile.Write(rec.YearsInLeague[14] + ",");
				outFile.Write(rec.YearsInLeague[15] + ",");
				outFile.Write(rec.YearsInLeague[16] + ",");
				outFile.Write(rec.YearsInLeague[17] + ",");
				outFile.Write(rec.YearsInLeague[18] + ",");
				outFile.WriteLine(rec.YearsInLeague[19]);
				++i;
			}

			outFile.Close();
		}

		private void CreateGamePlayerActiveTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Player Active.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Index,");
			outFile.Write("PlayerID,");
			outFile.Write("Position,");
			outFile.Write("Experience,");
			outFile.Write("Number,");
			outFile.Write("PositionGroup,");
			outFile.Write("Team,");
			outFile.Write("InjuryLength,");
			outFile.Write("Loyalty,");
			outFile.Write("PlaysToWin,");
			outFile.Write("Personality,");
			outFile.Write("Leadership,");
			outFile.Write("Intelligence,");
			outFile.Write("RedFlagMarker,");
			outFile.Write("Volatility,");
			outFile.Write("JoinedTeam,");
			outFile.Write("UFAYear,");
			outFile.Write("Popularity,");
			outFile.Write("ContractLength,");
			outFile.Write("Salary1,");
			outFile.Write("Salary2,");
			outFile.Write("Salary3,");
			outFile.Write("Salary4,");
			outFile.Write("Salary5,");
			outFile.Write("Salary6,");
			outFile.Write("Salary7,");
			outFile.Write("Bonus1,");
			outFile.Write("Bonus2,");
			outFile.Write("Bonus3,");
			outFile.Write("Bonus4,");
			outFile.Write("Bonus5,");
			outFile.Write("Bonus6,");
			outFile.WriteLine("Bonus7");

			int i = 0;
			foreach (LeagueData.PlayerActiveRecord rec in mLeagueData.PlayerActiveRecords)
			{
				outFile.Write(i + ",");
				outFile.Write(rec.PlayerID + ",");
				outFile.Write(rec.Position + ",");
				outFile.Write(rec.Experience + ",");
				outFile.Write(rec.Number + ",");
				outFile.Write(rec.PositionGroup + ",");
				outFile.Write(rec.Team + ",");
				outFile.Write(rec.InjuryLength + ",");
				outFile.Write(rec.Loyalty + ",");
				outFile.Write(rec.PlaysToWin + ",");
				outFile.Write(rec.Personality + ",");
				outFile.Write(rec.Leadership + ",");
				outFile.Write(rec.Intelligence + ",");
				outFile.Write(rec.RedFlagMarker + ",");
				outFile.Write(rec.Volatility + ",");
				outFile.Write(rec.JoinedTeam + ",");
				outFile.Write(rec.UFAYear + ",");
				outFile.Write(rec.Popularity + ",");
				outFile.Write(rec.ContractLength + ",");
				outFile.Write(rec.Salary[0] + ",");
				outFile.Write(rec.Salary[1] + ",");
				outFile.Write(rec.Salary[2] + ",");
				outFile.Write(rec.Salary[3] + ",");
				outFile.Write(rec.Salary[4] + ",");
				outFile.Write(rec.Salary[5] + ",");
				outFile.Write(rec.Salary[6] + ",");
				outFile.Write(rec.Bonus[0] + ",");
				outFile.Write(rec.Bonus[1] + ",");
				outFile.Write(rec.Bonus[2] + ",");
				outFile.Write(rec.Bonus[3] + ",");
				outFile.Write(rec.Bonus[4] + ",");
				outFile.Write(rec.Bonus[5] + ",");
				outFile.WriteLine(rec.Bonus[6]);
				++i;
			}

			outFile.Close();
		}

		private void CreateSeasonsTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Seasons.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Year,");
			outFile.Write("Player Eval,");
			outFile.Write("Player Team,");
			outFile.Write("Wins,");
			outFile.Write("Losses,");
			outFile.WriteLine("Ties");

			for (int i = 0; i < mLeagueData.SeasonRecords.Length; i++)
			{
				LeagueData.SeasonRecord rec = mLeagueData.SeasonRecords[i];

				outFile.Write(rec.Year + ",");
				outFile.Write(rec.PlayerEval + ",");
				outFile.Write(rec.PlayerTeam + ",");
				outFile.Write(rec.Wins + ",");
				outFile.Write(rec.Losses + ",");
				outFile.WriteLine(rec.Ties);
			}

			outFile.Close();
		}

		private void CreateFranchisePerformancesTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Franchise Performances.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("TeamIndex,");
			outFile.Write("Year,");
			outFile.Write("FranchiseValue,");
			outFile.Write("ProfitScore,");
			outFile.Write("PerformanceScore,");
			outFile.Write("RosterScore,");
			outFile.Write("Playoffs,");
			outFile.Write("Wins,");
			outFile.Write("Losses,");
			outFile.Write("Ties,");
			outFile.Write("PointsFor,");
			outFile.Write("PointsAgainst,");
			outFile.Write("ConfWins,");
			outFile.Write("ConfLoss,");
			outFile.Write("ConfTies,");
			outFile.Write("DivWin,");
			outFile.Write("DivLoss,");
			outFile.Write("DivTie,");
			outFile.Write("Attendance,");
			outFile.Write("StadiumCapacity,");
			outFile.Write("TVRevenue,");
			outFile.Write("TicketRevenue,");
			outFile.Write("SuiteRevenue,");
			outFile.Write("PlayerSalaries,");
			outFile.Write("PlayerBonuses,");
			outFile.Write("StadiumPayment,");
			outFile.Write("Concessions,");
			outFile.Write("Parking,");
			outFile.Write("Advertising,");
			outFile.Write("Training,");
			outFile.Write("Coaching,");
			outFile.Write("Scouting,");
			outFile.WriteLine("Maintenance");

			for (int i = 0; i < mLeagueData.FranchisePerformanceRecords.Length; i++)
			{
				LeagueData.FranchisePerformanceRecord rec = mLeagueData.FranchisePerformanceRecords[i];

				if (rec.Year >= mStartingSeason)
				{
					outFile.Write(i%32 + ",");
					outFile.Write(rec.Year + ",");
					outFile.Write(rec.FranchiseValue + ",");
					outFile.Write(rec.ProfitScore + ",");
					outFile.Write(rec.PerformanceScore + ",");
					outFile.Write(rec.RosterScore + ",");
					outFile.Write(rec.Playoffs + ",");
					outFile.Write(rec.Wins + ",");
					outFile.Write(rec.Losses + ",");
					outFile.Write(rec.Ties + ",");
					outFile.Write(rec.PointsFor + ",");
					outFile.Write(rec.PointsAgainst + ",");
					outFile.Write(rec.ConfWins + ",");
					outFile.Write(rec.ConfLoss + ",");
					outFile.Write(rec.ConfTies + ",");
					outFile.Write(rec.DivWin + ",");
					outFile.Write(rec.DivLoss + ",");
					outFile.Write(rec.DivTie + ",");
					outFile.Write(rec.Attendance * 100 + ",");
					outFile.Write(rec.StadiumCapacity * 100 + ",");
					outFile.Write((Int64)(rec.TVRevenue*10000) + ",");
					outFile.Write((Int64)(rec.TicketRevenue*10000) + ",");
					outFile.Write((Int64)(rec.SuiteRevenue*10000) + ",");
					outFile.Write((Int64)(rec.PlayerSalaries*10000) + ",");
					outFile.Write((Int64)(rec.PlayerBonuses*10000) + ",");
					outFile.Write((Int64)(rec.StadiumPayment*10000) + ",");
					outFile.Write((Int64)(rec.Concessions*10000) + ",");
					outFile.Write((Int64)(rec.Parking*10000) + ",");
					outFile.Write((Int64)(rec.Advertising*10000) + ",");
					outFile.Write((Int64)(rec.Training*10000) + ",");
					outFile.Write((Int64)(rec.Coaching*10000) + ",");
					outFile.Write((Int64)(rec.Scouting*10000) + ",");
					outFile.WriteLine((Int64)(rec.Maintenance*10000));
				}
			}
			outFile.Close();
		}

		private void CreateGameResultsTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Game Results.csv");
			AddStatusString("Writing game results and drives");
			System.IO.StreamWriter resultsFile = new System.IO.StreamWriter(filename, false);

			resultsFile.Write("GameID,");
			resultsFile.Write("Year,");
			resultsFile.Write("Week,");
			resultsFile.Write("AwayScore,");
			resultsFile.Write("AwayTeam,");
			resultsFile.Write("HomeScore,");
			resultsFile.Write("HomeTeam,");
			resultsFile.Write("Attendance,");
			resultsFile.Write("Temperature,");
			resultsFile.Write("Precip,");
			resultsFile.Write("Wind,");
			resultsFile.Write("AwayPassingLeaderPlayerID,");
			resultsFile.Write("HomePassingLeaderPlayerID,");
			resultsFile.Write("AwayRushingLeaderPlayerID,");
			resultsFile.Write("HomeRushingLeaderPlayerID,");
			resultsFile.Write("AwayReceivingLeaderPlayerID,");
			resultsFile.Write("HomeReceivingLeaderPlayerID,");
			resultsFile.Write("AwayPassAttempts,");
			resultsFile.Write("AwayPassCompletions,");
			resultsFile.Write("AwayPassYards,");
			resultsFile.Write("HomePassAttempts,");
			resultsFile.Write("HomePassCompletions,");
			resultsFile.Write("HomePassYards,");
			resultsFile.Write("AwayRushAttempts,");
			resultsFile.Write("AwayRushYards,");
			resultsFile.Write("HomeRushAttempts,");
			resultsFile.Write("HomeRushYards,");
			resultsFile.Write("AwayReceptions,");
			resultsFile.Write("AwayReceivingYards,");
			resultsFile.Write("HomeReceptions,");
			resultsFile.Write("HomeReceivingYards,");
			resultsFile.Write("PlayerOfTheGame,");
			resultsFile.Write("HomeLeftRushAttempts,");
			resultsFile.Write("HomeLeftRushYards,");
			resultsFile.Write("HomeLongAttempts,");
			resultsFile.Write("HomeLongCompletions,");
			resultsFile.Write("HomeLongYards,");
			resultsFile.Write("HomeMediumAttempts,");
			resultsFile.Write("HomeMediumCompletions,");
			resultsFile.Write("HomeMediumYards,");
			resultsFile.Write("HomeMiddleRushAttempts,");
			resultsFile.Write("HomeMiddleRushYards,");
			resultsFile.Write("HomeOtherAttempts,");
			resultsFile.Write("HomeOtherCompletions,");
			resultsFile.Write("HomeOtherYards,");
			resultsFile.Write("HomeOtherRushAttempts,");
			resultsFile.Write("HomeOtherRushYards,");
			resultsFile.Write("HomeRedZoneAttempts,");
			resultsFile.Write("HomeRedZoneFGs,");
			resultsFile.Write("HomeRedZoneTDs,");
			resultsFile.Write("HomeRightRushAttempts,");
			resultsFile.Write("HomeRightRushYards,");
			resultsFile.Write("HomeScreenAttempts,");
			resultsFile.Write("HomeScreenCompletions,");
			resultsFile.Write("HomeScreenYards,");
			resultsFile.Write("HomeShortAttempts,");
			resultsFile.Write("HomeShortCompletions,");
			resultsFile.Write("HomeShortYards,");
			resultsFile.Write("HomeTimeOfPossession,");
			resultsFile.Write("AwayLeftRushAttempts,");
			resultsFile.Write("AwayLeftRushYards,");
			resultsFile.Write("AwayLongAttempts,");
			resultsFile.Write("AwayLongCompletions,");
			resultsFile.Write("AwayLongYards,");
			resultsFile.Write("AwayMediumAttempts,");
			resultsFile.Write("AwayMediumCompletions,");
			resultsFile.Write("AwayMediumYards,");
			resultsFile.Write("AwayMiddleRushAttempts,");
			resultsFile.Write("AwayMiddleRushYards,");
			resultsFile.Write("AwayOtherAttempts,");
			resultsFile.Write("AwayOtherCompletions,");
			resultsFile.Write("AwayOtherYards,");
			resultsFile.Write("AwayOtherRushAttempts,");
			resultsFile.Write("AwayOtherRushYards,");
			resultsFile.Write("AwayRedZoneAttempts,");
			resultsFile.Write("AwayRedZonefGs,");
			resultsFile.Write("AwayRedZonetDs,");
			resultsFile.Write("AwayRightRushAttempts,");
			resultsFile.Write("AwayRightRushYards,");
			resultsFile.Write("AwayScreenAttempts,");
			resultsFile.Write("AwayScreenCompletions,");
			resultsFile.Write("AwayScreenYards,");
			resultsFile.Write("AwayShortAttempts,");
			resultsFile.Write("AwayShortCompletions,");
			resultsFile.Write("AwayShortYards,");
			resultsFile.Write("AwayTimeOfPossession,");
			resultsFile.Write("TotalCapacity,");
			resultsFile.Write("UpperDeckAttendance,");
			resultsFile.Write("UpperDeckCapacity,");
			resultsFile.Write("EndZoneAttendance,");
			resultsFile.Write("EndZoneCapacity,");
			resultsFile.Write("MezzanineAttendance,");
			resultsFile.Write("MezzanineCapacity,");
			resultsFile.Write("SidelineAttendance,");
			resultsFile.Write("SidelineCapacity,");
			resultsFile.Write("ClubAttendance,");
			resultsFile.Write("ClubCapacity,");
			resultsFile.Write("BoxAttendance,");
			resultsFile.WriteLine("BoxCapacity");

			filename = System.IO.Path.Combine(outputDir, "Game Drives.csv");
			System.IO.StreamWriter drivesFile = new System.IO.StreamWriter(filename, false);

			drivesFile.Write("EndMinutes,");
			drivesFile.Write("EndQuarter,");
			drivesFile.Write("EndSeconds,");
			drivesFile.Write("Gameid,");
			drivesFile.Write("PlayCount,");
			drivesFile.Write("StartMinutes,");
			drivesFile.Write("StartQuarter,");
			drivesFile.Write("StartSeconds,");
			drivesFile.Write("StartYardsFromGoal,");
			drivesFile.Write("Team,");
			drivesFile.Write("YardsGained,");
			drivesFile.WriteLine("Result");

			LeagueData.GameLog matchingGameLog = null;

			for (int i = 0; i < mLeagueData.GameResultRecords.Length; i++)
			{
				LeagueData.GameResultRecord rec = mLeagueData.GameResultRecords[i];

				if (rec.Week > 0 && rec.Year >= mStartingSeason)
				{
					int precip = rec.Weather / 7800;
					int windSpeed = ((rec.Weather % 7800) / 120);
					int temperature = ((rec.Weather % 120) - 10);

					foreach (LeagueData.GameWeekRecord gameWeekRec in mLeagueData.AvailableGameWeeks)
					{
						if (gameWeekRec.Year == rec.Year && gameWeekRec.Week == rec.Week)
						{
							foreach (LeagueData.GameLog gameLog in gameWeekRec.GameLogs)
							{
								if (gameLog.AwayTeam.TeamIndex == rec.AwayTeam && gameLog.HomeTeam.TeamIndex == rec.HomeTeam)
								{
									matchingGameLog = gameLog;

									break;
								}
							}

							break;
						}
					}

					resultsFile.Write(i + ",");
					resultsFile.Write(rec.Year + ",");
					resultsFile.Write(rec.Week + ",");
					resultsFile.Write(rec.AwayScore + ",");
					resultsFile.Write(rec.AwayTeam + ",");
					resultsFile.Write(rec.HomeScore + ",");
					resultsFile.Write(rec.HomeTeam + ",");
					resultsFile.Write(rec.Attendance*100 + ",");
					resultsFile.Write(temperature + ",");
					resultsFile.Write(precip + ",");
					resultsFile.Write(windSpeed + ",");
					resultsFile.Write(rec.AwayPassingLeaderPlayerID + ",");
					resultsFile.Write(rec.HomePassingLeaderPlayerID + ",");
					resultsFile.Write(rec.AwayRushingLeaderPlayerID + ",");
					resultsFile.Write(rec.HomeRushingLeaderPlayerID + ",");
					resultsFile.Write(rec.AwayReceivingLeaderPlayerID + ",");
					resultsFile.Write(rec.HomeReceivingLeaderPlayerID + ",");
					resultsFile.Write(rec.AwayPassAttempts + ",");
					resultsFile.Write(rec.AwayPassCompletions + ",");
					resultsFile.Write(rec.AwayPassYards + ",");
					resultsFile.Write(rec.HomePassAttempts + ",");
					resultsFile.Write(rec.HomePassCompletions + ",");
					resultsFile.Write(rec.HomePassYards + ",");
					resultsFile.Write(rec.AwayRushAttempts + ",");
					resultsFile.Write(rec.AwayRushYards + ",");
					resultsFile.Write(rec.HomeRushAttempts + ",");
					resultsFile.Write(rec.HomeRushYards + ",");
					resultsFile.Write(rec.AwayReceptions + ",");
					resultsFile.Write(rec.AwayReceivingYards + ",");
					resultsFile.Write(rec.HomeReceptions + ",");
					resultsFile.Write(rec.HomeReceivingYards + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.PlayerOfTheGameID : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.LeftAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.LeftYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.LongAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.LongCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.LongYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.MediumAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.MediumCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.MediumYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.MiddleAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.MiddleYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.OtherAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.OtherCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.OtherYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.OtherAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.OtherYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePossessions.RedZoneAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePossessions.RedZoneFieldGoals : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePossessions.RedZoneTouchdowns : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.RightAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomeRushing.RightYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.ScreenAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.ScreenCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.ScreenYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.ShortAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.ShortCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePassing.ShortYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.HomePossessions.TimeOfPossession : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.LeftAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.LeftYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.LongAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.LongCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.LongYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.MediumAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.MediumCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.MediumYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.MiddleAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.MiddleYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.OtherAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.OtherCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.OtherYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.OtherAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.OtherYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPossessions.RedZoneAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPossessions.RedZoneFieldGoals : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPossessions.RedZoneTouchdowns : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.RightAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayRushing.RightYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.ScreenAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.ScreenCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.ScreenYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.ShortAttempts : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.ShortCompletions : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPassing.ShortYards : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.AwayPossessions.TimeOfPossession : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.TotalCapacity : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.UpperDeckAttendance : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.UpperDeckCapacity : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.EndZoneCapacity : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.EndZoneAttendance : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.MezzanineAttendance : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.MezzanineCapacity : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.SidelineAttendance : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.SidelineCapacity : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.ClubAttendance : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.ClubCapacity : 0) + ",");
					resultsFile.Write((matchingGameLog != null ? matchingGameLog.BoxAttendance : 0) + ",");
					resultsFile.WriteLine((matchingGameLog != null ? matchingGameLog.BoxCapacity : 0));

					if (matchingGameLog != null)
					{
						foreach (LeagueData.GameDriveInfo homeDriveInfo in matchingGameLog.HomeDrives)
						{
							drivesFile.Write(homeDriveInfo.DriveEndMinutes + ",");
							drivesFile.Write(homeDriveInfo.DriveEndQuarter + ",");
							drivesFile.Write(homeDriveInfo.DriveEndSeconds + ",");
							drivesFile.Write(i + ",");
							drivesFile.Write(homeDriveInfo.Plays + ",");
							drivesFile.Write(homeDriveInfo.DriveStartMinutes + ",");
							drivesFile.Write(homeDriveInfo.DriveStartQuarter + ",");
							drivesFile.Write(homeDriveInfo.DriveStartSeconds + ",");
							drivesFile.Write(homeDriveInfo.YardsFromGoalStart + ",");
							drivesFile.Write(matchingGameLog.HomeTeam.TeamIndex + ",");
							drivesFile.Write(homeDriveInfo.YardsGained + ",");
							drivesFile.WriteLine(homeDriveInfo.Result);
						}
						foreach (LeagueData.GameDriveInfo awayDriveInfo in matchingGameLog.AwayDrives)
						{
							drivesFile.Write(awayDriveInfo.DriveEndMinutes + ",");
							drivesFile.Write(awayDriveInfo.DriveEndQuarter + ",");
							drivesFile.Write(awayDriveInfo.DriveEndSeconds + ",");
							drivesFile.Write(i + ",");
							drivesFile.Write(awayDriveInfo.Plays + ",");
							drivesFile.Write(awayDriveInfo.DriveStartMinutes + ",");
							drivesFile.Write(awayDriveInfo.DriveStartQuarter + ",");
							drivesFile.Write(awayDriveInfo.DriveStartSeconds + ",");
							drivesFile.Write(awayDriveInfo.YardsFromGoalStart + ",");
							drivesFile.Write(matchingGameLog.AwayTeam.TeamIndex + ",");
							drivesFile.Write(awayDriveInfo.YardsGained + ",");
							drivesFile.WriteLine(awayDriveInfo.Result);
						}
					}
				}
			}

			drivesFile.Close();
			resultsFile.Close();
		}

		private void CreateTeamGamesTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Team Schedule.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("TeamIndex,");
			outFile.Write("Week,");
			outFile.Write("Away,");
			outFile.Write("Conf,");
			outFile.Write("Div,");
			outFile.Write("Opponent,");
			outFile.Write("Score,");
			outFile.Write("OppScore,");
			outFile.Write("Attendance,");
			outFile.Write("Temperature,");
			outFile.Write("Precip,");
			outFile.Write("Wind,");
			outFile.WriteLine("Year");

			for (int i = 0; i < mLeagueData.TeamScheduleGameRecords.Length; i++)
			{
				LeagueData.TeamScheduleGameRecord rec = mLeagueData.TeamScheduleGameRecords[i];
				if (rec.Week != 999)
				{
					int precip = rec.Weather / 7800;
					int windSpeed = ((rec.Weather % 7800) / 120);
					int temperature = ((rec.Weather % 120) - 10);

					outFile.Write(rec.TeamIndex + ",");
					outFile.Write(rec.Week + ",");
					outFile.Write(rec.Away + ",");
					outFile.Write(rec.ConferenceGame + ",");
					outFile.Write(rec.DivisionGame + ",");
					outFile.Write(rec.Opponent + ",");
					outFile.Write(rec.Score + ",");
					outFile.Write(rec.OppScore + ",");
					outFile.Write(rec.Attendance*100 + ",");
					outFile.Write(temperature + ",");
					outFile.Write(precip + ",");
					outFile.Write(windSpeed + ",");
					outFile.WriteLine(mLeagueData.CurrentYear);
				}
			}

			outFile.Close();
		}

		private void CreatePlayerGameStatsTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Player Game Stats.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("PlayerID,");
			outFile.Write("Year,");
			outFile.Write("Week,");
			outFile.Write("Team,");
			outFile.Write("GamePlayed,");
			outFile.Write("GameStarted,");
			outFile.Write("PassAttempts,");
			outFile.Write("PassCompletions,");
			outFile.Write("PassYards,");
			outFile.Write("LongestPass,");
			outFile.Write("TDPasses,");
			outFile.Write("INTThrown,");
			outFile.Write("TimesSacked,");
			outFile.Write("SackedYards,");
			outFile.Write("RushAttempts,");
			outFile.Write("RushingYards,");
			outFile.Write("LongestRun,");
			outFile.Write("RushTD,");
			outFile.Write("Catches,");
			outFile.Write("ReceivingYards,");
			outFile.Write("LongestReception,");
			outFile.Write("ReceivingTDs,");
			outFile.Write("PassTargets,");
			outFile.Write("YardsAfterCatch,");
			outFile.Write("PassDrops,");
			outFile.Write("PuntReturns,");
			outFile.Write("PuntReturnYards,");
			outFile.Write("PuntReturnTDs,");
			outFile.Write("KickReturns,");
			outFile.Write("KickReturnYards,");
			outFile.Write("KickReturnTDs,");
			outFile.Write("Fumbles,");
			outFile.Write("FumbleRecoveries,");
			outFile.Write("ForcedFumbles,");
			outFile.Write("MiscTD,");
			outFile.Write("KeyRunBlock,");
			outFile.Write("KeyRunBlockOpportunites,");
			outFile.Write("SacksAllowed,");
			outFile.Write("Tackles,");
			outFile.Write("Assists,");
			outFile.Write("Sacks,");
			outFile.Write("INTs,");
			outFile.Write("INTReturnYards,");
			outFile.Write("INTReturnTDs,");
			outFile.Write("PassesDefended,");
			outFile.Write("PassesBlocked,");
			outFile.Write("QBHurries,");
			outFile.Write("PassesCaught,");
			outFile.Write("PassPlays,");
			outFile.Write("RunPlays,");
			outFile.Write("FGMade,");
			outFile.Write("FGAttempted,");
			outFile.Write("FGLong,");
			outFile.Write("PAT,");
			outFile.Write("PATAttempted,");
			outFile.Write("Punts,");
			outFile.Write("PuntYards,");
			outFile.Write("PuntLong,");
			outFile.Write("PuntIn20,");
			outFile.Write("Points,");
			outFile.Write("OpposingTeamID,");
			outFile.Write("ThirdDownRushes,");
			outFile.Write("ThirdDownRushConversions,");
			outFile.Write("ThirdDownPassAttempts,");
			outFile.Write("ThirdDownPassCompletions,");
			outFile.Write("ThirdDownPassConversions,");
			outFile.Write("ThirdDownReceivingTargets,");
			outFile.Write("ThirdDownReceivingCatches,");
			outFile.Write("ThirdDownReceivingConversions,");
			outFile.Write("FirstDownRushes,");
			outFile.Write("FirstDownPasses,");
			outFile.Write("FirstDownCatches,");
			outFile.Write("FG40PlusAttempts,");
			outFile.Write("FG40PlusMade,");
			outFile.Write("FG50PlusAttempts,");
			outFile.Write("FG50PlusMade,");
			outFile.Write("PuntNetYards,");
			outFile.Write("SpecialTeamsTackles,");
			outFile.Write("Unknown14,");
			outFile.Write("TimesKnockedDown,");
			outFile.Write("RedZoneRushes,");
			outFile.Write("RedZoneRushingYards,");
			outFile.Write("RedZonePassAttempts,");
			outFile.Write("RedZonePassCompletions,");
			outFile.Write("RedZonePassingYards,");
			outFile.Write("RedZoneReceivingTargets,");
			outFile.Write("RedZoneReceivingCatches,");
			outFile.Write("RedZoneReceivingYards,");
			outFile.Write("TotalTDs,");
			outFile.Write("TwoPointConversions,");
			outFile.Write("PancakeBlocks,");
			outFile.Write("QBKnockdowns,");
			outFile.Write("Unknown23,");
			outFile.Write("SpecialTeamsPlays,");
			outFile.Write("RushingGamesOver100Yards,");
			outFile.Write("ReceivingGamesOver100Yards,");
			outFile.Write("PassingGamesOver300Yards,");
			outFile.Write("RunsOf10YardsPlus,");
			outFile.Write("CatchesOf20YardsPlus,");
			outFile.Write("ThrowsOf20YardsPlus,");
			outFile.Write("AllPurposeYards,");
			outFile.Write("YardsFromScrimmage,");
			outFile.Write("DoubleCoveragesThrownInto,");
			outFile.Write("DoubleCoveragesAvoided,");
			outFile.Write("BadPasses,");
			outFile.Write("RunsForLoss,");
			outFile.Write("RunsOf20YardsPlus,");
			outFile.Write("FumblesLost,");
			outFile.Write("HasKeyCoverage,");
			outFile.Write("ThrownAt,");
			outFile.Write("TacklesForLoss,");
			outFile.Write("AssistedTacklesForLoss,");
			outFile.Write("ReceptionsOf20YardsPlusGivenUp,");
			outFile.Write("Kickoffs,");
			outFile.Write("KickoffYards,");
			outFile.Write("KickoffTouchbacks,");
			outFile.Write("TotalFieldPositionAfterKickoff,");
			outFile.Write("OffensivePassPlays,");
			outFile.Write("OffensiveRunPlays,");
			outFile.Write("DefensivePassPlays,");
			outFile.Write("DefensiveRunPlays,");
			outFile.Write("SuccessfulPasses,");
			outFile.Write("SuccessfulCatches,");
			outFile.Write("SuccessfulRuns,");
			outFile.WriteLine("BadPassesCaught");

			for (int yearIndex = 0; yearIndex < mLeagueData.SeasonsPlayed; ++yearIndex)
			{
				if (mLeagueData.PlayerGameStatsRecords[yearIndex] != null)
				{
					foreach (LeagueData.PlayerGameStatsRecord rec in mLeagueData.PlayerGameStatsRecords[yearIndex])
					{
						if (rec.Team != 99 && rec.PlayerID != -1 && rec.Year >= mStartingSeason)
						{
							outFile.Write(rec.PlayerID + ",");
							outFile.Write(rec.Year + ",");
							outFile.Write(rec.Week + ",");
							outFile.Write(rec.Team + ",");
							outFile.Write(rec.GamePlayed + ",");
							outFile.Write(rec.GameStarted + ",");
							outFile.Write(rec.PassAttempts + ",");
							outFile.Write(rec.PassCompletions + ",");
							outFile.Write(rec.PassYards + ",");
							outFile.Write(rec.LongestPass + ",");
							outFile.Write(rec.TDPasses + ",");
							outFile.Write(rec.INTThrown + ",");
							outFile.Write(rec.TimesSacked + ",");
							outFile.Write(rec.SackedYards + ",");
							outFile.Write(rec.RushAttempts + ",");
							outFile.Write(rec.RushingYards + ",");
							outFile.Write(rec.LongestRun + ",");
							outFile.Write(rec.RushTD + ",");
							outFile.Write(rec.Catches + ",");
							outFile.Write(rec.ReceivingYards + ",");
							outFile.Write(rec.LongestReception + ",");
							outFile.Write(rec.ReceivingTDs + ",");
							outFile.Write(rec.PassTargets + ",");
							outFile.Write(rec.YardsAfterCatch + ",");
							outFile.Write(rec.PassDrops + ",");
							outFile.Write(rec.PuntReturns + ",");
							outFile.Write(rec.PuntReturnYards + ",");
							outFile.Write(rec.PuntReturnTDs + ",");
							outFile.Write(rec.KickReturns + ",");
							outFile.Write(rec.KickReturnYards + ",");
							outFile.Write(rec.KickReturnTDs + ",");
							outFile.Write(rec.Fumbles + ",");
							outFile.Write(rec.FumbleRecoveries + ",");
							outFile.Write(rec.ForcedFumbles + ",");
							outFile.Write(rec.MiscTD + ",");
							outFile.Write(rec.KeyRunBlock + ",");
							outFile.Write(rec.KeyRunBlockOpportunites + ",");
							outFile.Write(rec.SacksAllowed + ",");
							outFile.Write(rec.Tackles + ",");
							outFile.Write(rec.Assists + ",");
							outFile.Write(rec.Sacks + ",");
							outFile.Write(rec.INTs + ",");
							outFile.Write(rec.INTReturnYards + ",");
							outFile.Write(rec.INTReturnTDs + ",");
							outFile.Write(rec.PassesDefended + ",");
							outFile.Write(rec.PassesBlocked + ",");
							outFile.Write(rec.QBHurries + ",");
							outFile.Write(rec.PassesCaught + ",");
							outFile.Write(rec.PassPlays + ",");
							outFile.Write(rec.RunPlays + ",");
							outFile.Write(rec.FGMade + ",");
							outFile.Write(rec.FGAttempted + ",");
							outFile.Write(rec.FGLong + ",");
							outFile.Write(rec.PAT + ",");
							outFile.Write(rec.PATAttempted + ",");
							outFile.Write(rec.Punts + ",");
							outFile.Write(rec.PuntYards + ",");
							outFile.Write(rec.PuntLong + ",");
							outFile.Write(rec.PuntIn20 + ",");
							outFile.Write(rec.Points + ",");
							outFile.Write(rec.OpposingTeamID + ",");
							outFile.Write(rec.ThirdDownRushes + ",");
							outFile.Write(rec.ThirdDownRushConversions + ",");
							outFile.Write(rec.ThirdDownPassAttempts + ",");
							outFile.Write(rec.ThirdDownPassCompletions + ",");
							outFile.Write(rec.ThirdDownPassConversions + ",");
							outFile.Write(rec.ThirdDownReceivingTargets + ",");
							outFile.Write(rec.ThirdDownReceivingCatches + ",");
							outFile.Write(rec.ThirdDownReceivingConversions + ",");
							outFile.Write(rec.FirstDownRushes + ",");
							outFile.Write(rec.FirstDownPasses + ",");
							outFile.Write(rec.FirstDownCatches + ",");
							outFile.Write(rec.FG40PlusAttempts + ",");
							outFile.Write(rec.FG40PlusMade + ",");
							outFile.Write(rec.FG50PlusAttempts + ",");
							outFile.Write(rec.FG50PlusMade + ",");
							outFile.Write(rec.PuntNetYards + ",");
							outFile.Write(rec.SpecialTeamsTackles + ",");
							outFile.Write(rec.Unknown14 + ",");
							outFile.Write(rec.TimesKnockedDown + ",");
							outFile.Write(rec.RedZoneRushes + ",");
							outFile.Write(rec.RedZoneRushingYards + ",");
							outFile.Write(rec.RedZonePassAttempts + ",");
							outFile.Write(rec.RedZonePassCompletions + ",");
							outFile.Write(rec.RedZonePassingYards + ",");
							outFile.Write(rec.RedZoneReceivingTargets + ",");
							outFile.Write(rec.RedZoneReceivingCatches + ",");
							outFile.Write(rec.RedZoneReceivingYards + ",");
							outFile.Write(rec.TotalTDs + ",");
							outFile.Write(rec.TwoPointConversions + ",");
							outFile.Write(rec.PancakeBlocks + ",");
							outFile.Write(rec.QBKnockdowns + ",");
							outFile.Write(rec.Unknown23 + ",");
							outFile.Write(rec.SpecialTeamsPlays + ",");
							outFile.Write(rec.RushingGamesOver100Yards + ",");
							outFile.Write(rec.ReceivingGamesOver100Yards + ",");
							outFile.Write(rec.PassingGamesOver300Yards + ",");
							outFile.Write(rec.RunsOf10YardsPlus + ",");
							outFile.Write(rec.CatchesOf20YardsPlus + ",");
							outFile.Write(rec.ThrowsOf20YardsPlus + ",");
							outFile.Write(rec.AllPurposeYards + ",");
							outFile.Write(rec.YardsFromScrimmage + ",");
							outFile.Write(rec.DoubleCoveragesThrownInto + ",");
							outFile.Write(rec.DoubleCoveragesAvoided + ",");
							outFile.Write(rec.BadPasses + ",");
							outFile.Write(rec.RunsForLoss + ",");
							outFile.Write(rec.RunsOf20YardsPlus + ",");
							outFile.Write(rec.FumblesLost + ",");
							outFile.Write(rec.HasKeyCoverage + ",");
							outFile.Write(rec.ThrownAt + ",");
							outFile.Write(rec.TacklesForLoss + ",");
							outFile.Write(rec.AssistedTacklesForLoss + ",");
							outFile.Write(rec.ReceptionsOf20YardsPlusGivenUp + ",");
							outFile.Write(rec.Kickoffs + ",");
							outFile.Write(rec.KickoffYards + ",");
							outFile.Write(rec.KickoffTouchbacks + ",");
							outFile.Write(rec.TotalFieldPositionAfterKickoff + ",");
							outFile.Write(rec.OffensivePassPlays + ",");
							outFile.Write(rec.OffensiveRunPlays + ",");
							outFile.Write(rec.DefensivePassPlays + ",");
							outFile.Write(rec.DefensiveRunPlays + ",");
							outFile.Write(rec.SuccessfulPasses + ",");
							outFile.Write(rec.SuccessfulCatches + ",");
							outFile.Write(rec.SuccessfulRuns + ",");
							outFile.WriteLine(rec.BadPassesCaught);
						}
					}
				}
			}

			outFile.Close();
		}

		private void CreateTeamStadiumsTable()
		{
			string outputDir = buttonOutputDirectory.Text;
			string filename = System.IO.Path.Combine(outputDir, "Team Stadiums.csv");
			AddStatusString("Writing " + System.IO.Path.GetFileName(filename));
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.Write("Team,");
			outFile.Write("StadiumType,");
			outFile.Write("YearStadiumBuilt,");
			outFile.Write("TotalCapacity,");
			outFile.Write("LuxuryBoxes,");
			outFile.Write("ClubSeats,");
			outFile.Write("FanLoyalty,");
			outFile.Write("PublicSupportForStadium,");
			outFile.Write("UpperDeckPrice,");
			outFile.Write("EndZonePrice,");
			outFile.Write("MezzaninePrice,");
			outFile.Write("SidelinesPrice,");
			outFile.Write("ClubSeatsPrice,");
			outFile.Write("LuxuryBoxPrice,");
			outFile.Write("ConstructionCompletionYear,");
			outFile.Write("ConstructionType,");
			outFile.Write("ConstructionCapacity,");
			outFile.Write("ConstructionLuxuryBoxes,");
			outFile.Write("ConstructionClubSeats,");
			outFile.Write("ConstructionStadiumType,");
			outFile.WriteLine("PriorYearAttendance");

			for (int i = 0; i < mLeagueData.TeamStadiumBlocks.Length; i++)
			{
				LeagueData.TeamStadiumBlock rec = mLeagueData.TeamStadiumBlocks[i];

				outFile.Write(i + ",");
				outFile.Write(rec.StadiumType + ",");
				outFile.Write(rec.YearStadiumBuilt + ",");
				outFile.Write((rec.TotalCapacity*100) + ",");
				outFile.Write(rec.LuxuryBoxes + ",");
				outFile.Write((rec.ClubSeats*100) + ",");
				outFile.Write(rec.FanLoyalty + ",");
				outFile.Write(rec.PublicSupportForStadium + ",");
				outFile.Write(rec.UpperDeckPrice + ",");
				outFile.Write(rec.EndZonePrice + ",");
				outFile.Write(rec.MezzaninePrice + ",");
				outFile.Write(rec.SidelinesPrice + ",");
				outFile.Write(rec.ClubSeatsPrice + ",");
				outFile.Write((rec.LuxuryBoxPrice*1000) + ",");
				outFile.Write(rec.ConstructionCompletionYear + ",");
				outFile.Write(rec.ConstructionType + ",");
				outFile.Write((rec.ConstructionCapacity*100) + ",");
				outFile.Write(rec.ConstructionLuxuryBoxes + ",");
				outFile.Write((rec.ConstructionClubSeats*100) + ",");
				outFile.Write(rec.ConstructionStadiumType + ",");
				outFile.WriteLine(rec.PriorYearAttendance * 100);
			}

			outFile.Close();
		}

		private void CreateGameDataTables()
		{
			CreateGameInfoTable();
			CreateGameDraftTable();
			CreateDraftOrderTable();
			CreateGameTransactionTable();
			CreateGamePlayerHistoricalTable();
			CreateGamePlayerActiveTable();
			CreateSeasonsTable();
			CreateFranchisePerformancesTable();
			CreateGameResultsTable();
			CreateTeamGamesTable();
			CreatePlayerGameStatsTable();
			CreateTeamStadiumsTable();
		}

		private void CreateDataTables()
		{
			CreateCityDataTable();
			CreateTeamsDataTable();
			CreateInjuriesDataTable();
			CreateCollegeNamesDataTable();
			CreateMappingsDataTable();
			CreateHometownNamesDataTable();
		}

		private void buttonOutputDirectory_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = buttonOutputDirectory.Text;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonOutputDirectory.Text = dlg.SelectedPath;
				mSettings.WriteXMLString(kSettingsRoot, kOutputDirectory, dlg.SelectedPath);
			}
		}

		private int mStartingSeason;
		private void buttonGenerateCSV_Click(object sender, EventArgs e)
		{
			mSavedGameIndex = comboBoxSavedGame.SelectedIndex - 1;
			if (mSavedGameIndex >= 0 && mSavedGameIndex < mUniverseData.SavedGames.Count)
			{
				mStartingSeason = 0;
				if (checkBoxProcessFromSeason.Checked)
				{
					if (!Int32.TryParse(textBoxStartingSeason.Text, out mStartingSeason))
					{
						mStartingSeason = 0;
					}
				}

				mOldCursor = Cursor;
				Cursor = Cursors.WaitCursor;
				buttonGenerateCSV.Enabled = false;
				System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateThreadTask));
				updateThread.IsBackground = true;
				updateThread.Start();
			}
		}

		private void checkBoxProcessFromSeason_CheckedChanged(object sender, EventArgs e)
		{
			mSettings.WriteXMLValue(kSettingsRoot, kUseProcessSeason, checkBoxProcessFromSeason.Checked);
		}

		private void textBoxStartingSeason_TextChanged(object sender, EventArgs e)
		{
			mSettings.WriteXMLString(kSettingsRoot, kSeasonToProcessFrom, textBoxStartingSeason.Text);
		}
	}
}
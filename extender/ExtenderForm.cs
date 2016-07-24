using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Extender
{
	public partial class ExtenderForm : Form
	{
		delegate void DisplayPageCallback(string pageFilename, WebBrowser browserWindow);
		delegate void AddStatusTextCallback(string text);
		delegate void WorkCompletedCallback();

		private DataReader.UniverseData mUniverseData;
		private DataReader.LeagueData mCurrentLeagueData;
		private string mLeaguePrefix;
		private string mPathPrefix;
		private bool mRunCareerReports = false;
		private Cursor mOldCursor = null;

		public ExtenderForm()
		{
			InitializeComponent();

			Assembly a = typeof(ExtenderForm).Assembly;
			Text += " v" + a.GetName().Version;

			mUniverseData = new DataReader.UniverseData();
			InitializeTeamImageMap();
			InitializeStatWeights();
			InitializePositionGroupMap();
		}

		private void chooseFOFGameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GameSelector dlg = new GameSelector(mUniverseData);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				mOldCursor = Cursor;
				Cursor = Cursors.WaitCursor;
				mPathPrefix = dlg.SelectedEntry.PathPrefix;
				mLeaguePrefix = System.IO.Path.GetFileName(mPathPrefix);
				mRunCareerReports = dlg.RunCareerReports;
				System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateThreadTask));
				updateThread.IsBackground = true;
				updateThread.Start();
			}
		}

		private void UpdateThreadTask()
		{
			int seasonToRun = DataReader.LeagueData.LoadAllSeasons;
			if (!mRunCareerReports)
			{
				seasonToRun = DataReader.LeagueData.LoadCurrentSeasonOnly;
			}

			mCurrentLeagueData = new DataReader.LeagueData(mPathPrefix, mUniverseData, seasonToRun, OnFileRead, true);
			mCurrentLeagueData.SortPlayerArrays();
			FillInData();

			AddStatusString("Finished!");

			WorkCompleted();
		}

		private void DisplayPage(string pageFilename, WebBrowser browserWindow)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (browserWindow.InvokeRequired)
			{
				DisplayPageCallback d = new DisplayPageCallback(DisplayPage);
				this.Invoke(d, new object[] { pageFilename, browserWindow });
			}
			else
			{
				browserWindow.Navigate(pageFilename);
			}
		}

		private void AddStatusString(string newText)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.labelStatus.InvokeRequired)
			{
				AddStatusTextCallback d = new AddStatusTextCallback(AddStatusString);
				this.Invoke(d, new object[] { newText });
			}
			else
			{
				labelStatus.Text = newText;
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
			}
		}

		private void OnFileRead(string fileName)
		{
			AddStatusString("Reading file " + fileName + System.Environment.NewLine);
		}

		private enum PlayerStatCategory
		{
			Pass,
			Rush,
			Recv,
			Block,
			MiscOff,
			PRush,
			PDef,
			MiscDef,
			Kick,
			Punt,
			SpecT,

			Count
		}

		private class PlayerStatEntry
		{
			public PlayerStatEntry()
			{
				StatRec = new DataReader.LeagueData.PlayerGameStatsRecord();
				CategoryScore = new double[(int)PlayerStatCategory.Count];
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						CategoryScore[(int)category] = 0.0;
					}
				}
			}

			public double OffenseScore
			{
				get
				{
					return CategoryScore[(int)PlayerStatCategory.Block] +
						CategoryScore[(int)PlayerStatCategory.Pass] +
						CategoryScore[(int)PlayerStatCategory.Recv] +
						CategoryScore[(int)PlayerStatCategory.Rush] +
						CategoryScore[(int)PlayerStatCategory.MiscOff];
				}
			}

			public double DefenseScore
			{
				get
				{
					return CategoryScore[(int)PlayerStatCategory.PRush] +
						CategoryScore[(int)PlayerStatCategory.MiscDef] +
						CategoryScore[(int)PlayerStatCategory.PDef];
				}
			}

			public double SpecialTeamsScore
			{
				get
				{
					return CategoryScore[(int)PlayerStatCategory.Kick] +
						CategoryScore[(int)PlayerStatCategory.Punt] +
						CategoryScore[(int)PlayerStatCategory.SpecT];
				}
			}

			public DataReader.LeagueData.PlayerGameStatsRecord StatRec;
			public double[] CategoryScore;
		}

		private class PlayerCareerData
		{
			public string PlayerName = "";
			public string PlayerPosition = "";
			public string PlayerPositionGroup = "";
			public string MostPlayedForTeam = "";
			public string RookiePlayedForTeam = "";
			public string DraftedByTeam = "";
			public int MostPlayedForTeamID = -1;
			public int RookiePlayedForTeamID = -1;
			public int DraftedByTeamID = -1;
			public int PlayerID = -1;
			public int PlayerExperience = 0;
			public int OriginalIndex = 0;
			public int DraftPos = 0;
			public int DraftYear = 0;
			public double Score = 0;
			public PlayerStatEntry RegularSeasonStats = new PlayerStatEntry();
			public PlayerStatEntry PlayoffsStats = new PlayerStatEntry();
			public PlayerStatEntry RookieStats = new PlayerStatEntry();
		}

		private PlayerCareerData[] mPlayerCareerData;
		private Dictionary<string, PlayerCareerData> mBestSeasonsData;

		private class PlayerSeasonData
		{
			public string PlayerName = "";
			public string PlayerPosition = "";
			public string PlayerTeam = "";
			public int PlayerTeamID = -1;
			public int PlayerID = -1;
			public int PlayerExperience = 0;
			public PlayerStatEntry WeekStats = new PlayerStatEntry();
			public PlayerStatEntry SeasonStats = new PlayerStatEntry();
			public PlayerStatEntry PlayoffsStats = new PlayerStatEntry();
			public bool IsRookie = false;
			public int OriginalIndex = 0;
			public int DraftPos = 0;
			public int DraftYear = 0;
			public long CapCost = 0;

			public double WeekOffenseScore { get { return WeekStats.OffenseScore; } }
			public double WeekDefenseScore { get { return WeekStats.DefenseScore; } }
			public double WeekSpecialTeamsScore { get { return WeekStats.SpecialTeamsScore; } }

			public double SeasonOffenseScore { get { return SeasonStats.OffenseScore; } }
			public double SeasonDefenseScore { get { return SeasonStats.DefenseScore; } }
			public double SeasonSpecialTeamsScore { get { return SeasonStats.SpecialTeamsScore; } }

			public double PlayoffsOffenseScore { get { return PlayoffsStats.OffenseScore; } }
			public double PlayoffsDefenseScore { get { return PlayoffsStats.DefenseScore; } }
			public double PlayoffsSpecialTeamsScore { get { return PlayoffsStats.SpecialTeamsScore; } }
		}

		private PlayerSeasonData[] mPlayerSeasonData;

		private const int kLeaderCount = 5;
		private PlayerSeasonData[] mWeekOffenseLeaders;
		private PlayerSeasonData[] mWeekDefenseLeaders;
		private PlayerSeasonData[] mWeekSpecialTeamsLeaders;
		private PlayerSeasonData[] mWeekRookieOffenseLeaders;
		private PlayerSeasonData[] mWeekRookieDefenseLeaders;
		private PlayerSeasonData[] mSeasonOffenseLeaders;
		private PlayerSeasonData[] mSeasonDefenseLeaders;
		private PlayerSeasonData[] mSeasonSpecialTeamsLeaders;
		private PlayerSeasonData[] mSeasonRookieOffenseLeaders;
		private PlayerSeasonData[] mSeasonRookieDefenseLeaders;
		private PlayerSeasonData[] mPlayoffsOffenseLeaders;
		private PlayerSeasonData[] mPlayoffsDefenseLeaders;
		private PlayerSeasonData[] mPlayoffsSpecialTeamsLeaders;

		private enum AwardPeriod
		{
			Week,
			Season,
			Playoffs
		}
		private enum CareerAwardPeriod
		{
			RegularSeason,
			Rookie,
			Playoffs
		}
		private enum AwardSpecialty
		{
			Offense,
			Defense,
			SpecialTeams
		}

		private const string kSeasonAwardsFile = "SeasonAwards.html";
		private const string kSeasonAllProFile = "SeasonAllProTeam.html";
		private const string kSeasonAllRookieFile = "SeasonAllRookieTeam.html";
		private const string kAllTimeTeamFile = "AllTimeTeam.html";
		private const string kAllTimePlayoffsTeamFile = "AllTimePlayoffsTeam.html";
		private const string kAllTimeRookieTeamFile = "AllTimeRookieTeam.html";
		private const string kBestSeasonsFile = "BestSeasons.html";
		private void FillInData()
		{
			AddStatusString("Collecting players");
			CollectPlayers();
			if (mPlayerSeasonData != null)
			{
				AddStatusString("Calculating season points");
				CalculatePlayerSeasonPoints();
				AddStatusString("Finding season leaders");
				FindLeaders();
				AddStatusString("Writing season results");
				WriteSeasonStats();
				WriteSeasonLeaders();
				WriteSeasonAllProTeam();
				WriteSeasonAllRookieTeam();

				string browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kSeasonAwardsFile);
				DisplayPage(browsePath, webBrowserSeasonAwards);

				browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kSeasonAllProFile);
				DisplayPage(browsePath, webBrowserAllProTeam);

				browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kSeasonAllRookieFile);
				DisplayPage(browsePath, webBrowserAllRookieTeam);
			}
			if (mPlayerCareerData != null && mRunCareerReports)
			{
				AddStatusString("Calculating career points");
				CalculatePlayerCareerPoints();
				AddStatusString("Writing career results");
				WriteCareerStats();
				WriteAllTimeTeam();
				WriteAllTimePlayoffsTeam();
				WriteAllTimeRookieTeam();
				WriteBestSeasons();

				string browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kAllTimeTeamFile);
				DisplayPage(browsePath, webBrowserAllTimeTeam);

				browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kAllTimePlayoffsTeamFile);
				DisplayPage(browsePath, webBrowserAllTimePlayoffsTeam);

				browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kAllTimeRookieTeamFile);
				DisplayPage(browsePath, webBrowserAllTimeRookieTeam);

				browsePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kBestSeasonsFile);
				DisplayPage(browsePath, webBrowserBestSeasons);
			}
		}

		private bool mInPlayoffs;

		private string GenerateSeasonStatString(PlayerSeasonData playerData, AwardPeriod awardPeriod, AwardSpecialty awardSpecialty)
		{
			string resultString = "";

			DataReader.LeagueData.PlayerGameStatsRecord rec = null;
			switch (awardPeriod)
			{
				case AwardPeriod.Week:
					rec = playerData.WeekStats.StatRec;
					break;
				case AwardPeriod.Season:
					rec = playerData.SeasonStats.StatRec;
					break;
				case AwardPeriod.Playoffs:
					rec = playerData.PlayoffsStats.StatRec;
					break;
			}

			switch (awardSpecialty)
			{
				case AwardSpecialty.Offense:
					resultString = GenerateOffensiveString(rec);
					break;
				case AwardSpecialty.Defense:
					resultString = GenerateDefensiveString(rec);
					break;
				case AwardSpecialty.SpecialTeams:
					resultString = GenerateSpecialTeamsString(rec);
					break;
			}

			switch (awardPeriod)
			{
				case AwardPeriod.Week:
					switch (awardSpecialty)
					{
						case AwardSpecialty.Offense:
							resultString += " ( " + playerData.WeekOffenseScore + " )";
							break;
						case AwardSpecialty.Defense:
							resultString += " ( " + playerData.WeekDefenseScore + " )";
							break;
						case AwardSpecialty.SpecialTeams:
							resultString += " ( " + playerData.WeekSpecialTeamsScore + " )";
							break;
					}
					break;
				case AwardPeriod.Season:
					switch (awardSpecialty)
					{
						case AwardSpecialty.Offense:
							resultString += " ( " + playerData.SeasonOffenseScore + " )";
							break;
						case AwardSpecialty.Defense:
							resultString += " ( " + playerData.SeasonDefenseScore + " )";
							break;
						case AwardSpecialty.SpecialTeams:
							resultString += " ( " + playerData.SeasonSpecialTeamsScore + " )";
							break;
					}
					break;
				case AwardPeriod.Playoffs:
					switch (awardSpecialty)
					{
						case AwardSpecialty.Offense:
							resultString += " ( " + playerData.PlayoffsOffenseScore + " )";
							break;
						case AwardSpecialty.Defense:
							resultString += " ( " + playerData.PlayoffsDefenseScore + " )";
							break;
						case AwardSpecialty.SpecialTeams:
							resultString += " ( " + playerData.PlayoffsSpecialTeamsScore + " )";
							break;
					}
					break;
			}

			return resultString;
		}

		private string GenerateCareerStatString(PlayerCareerData playerData, CareerAwardPeriod awardPeriod, AwardSpecialty awardSpecialty)
		{
			string resultString = "";

			DataReader.LeagueData.PlayerGameStatsRecord rec = null;
			switch (awardPeriod)
			{
				case CareerAwardPeriod.Playoffs:
					rec = playerData.PlayoffsStats.StatRec;
					break;
				case CareerAwardPeriod.RegularSeason:
					rec = playerData.RegularSeasonStats.StatRec;
					break;
				case CareerAwardPeriod.Rookie:
					rec = playerData.RookieStats.StatRec;
					break;
			}

			switch (awardSpecialty)
			{
				case AwardSpecialty.Offense:
					resultString = GenerateOffensiveString(rec);
					break;
				case AwardSpecialty.Defense:
					resultString = GenerateDefensiveString(rec);
					break;
				case AwardSpecialty.SpecialTeams:
					resultString = GenerateSpecialTeamsString(rec);
					break;
			}

			return resultString;
		}

		private void WritePlayerTeamCells(System.IO.StreamWriter outFile, string teamName, int teamID, string playerName, int playerID)
		{
			string teamImageUrl = "";
			string teamLinkUrl = "";
			if (mTeamImageMap.ContainsKey(teamName))
			{
				teamImageUrl = mTeamImageMap[teamName];
				if (mTeamLinkMap.ContainsKey(mLeaguePrefix))
				{
					teamLinkUrl = mTeamLinkMap[mLeaguePrefix].Replace("{TeamID}", teamID.ToString());
				}
			}
			if (teamLinkUrl.Length > 0)
			{
				outFile.WriteLine("            <td><a href=\"" + teamLinkUrl + "\" target=\"_blank\"><img src=\"" + teamImageUrl + "\" /></a></td>");
			}
			else
			{
				outFile.WriteLine("            <td><img src=\"" + teamImageUrl + "\" /></td>");
			}
			string playerLinkUrl = "";
			if (mPlayerLinkMap.ContainsKey(mLeaguePrefix))
			{
				playerLinkUrl = mPlayerLinkMap[mLeaguePrefix].Replace("{PlayerID}", playerID.ToString());
				outFile.WriteLine("            <td><a href=\"" + playerLinkUrl + "\" target=\"_blank\">" + playerName + "</a></td>");
			}
			else
			{
				outFile.WriteLine("            <td>" + playerName + "</td>");
			}
		}

		private void WriteSeasonLeaderTable(System.IO.StreamWriter outFile, PlayerSeasonData[] leaders, string caption,
			AwardPeriod awardPeriod, AwardSpecialty awardSpecialty)
		{
			outFile.WriteLine("    <table border=\"1\" cellpadding=\"4\" style=\"background-color: #DDDDDD; color:Black; width: 600px\">");
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>" + caption + "</strong></td></tr>");
			foreach (PlayerSeasonData leaderData in leaders)
			{
				if (leaderData != null)
				{
					outFile.WriteLine("        <tr style=\"font-size: 14px; font-family: 'Times New Roman'\">");
					string playerString = leaderData.PlayerPosition + " " + leaderData.PlayerName;
					WritePlayerTeamCells(outFile, leaderData.PlayerTeam, leaderData.PlayerTeamID, playerString, leaderData.PlayerID);
					outFile.WriteLine("            <td valign=\"middle\">" + leaderData.PlayerTeam + "</td>");
					outFile.WriteLine("            <td>" + GenerateSeasonStatString(leaderData, awardPeriod, awardSpecialty) + "</td>");
					outFile.WriteLine("        </tr>");
				}
			}
			outFile.WriteLine("    </table>");
		}

		private void WriteSeasonLeaders()
		{
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kSeasonAwardsFile);
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			outFile.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			outFile.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
			outFile.WriteLine("<body>");

			outFile.WriteLine("<head><title>Season Awards</title></head>");

			WriteSeasonLeaderTable(outFile, mWeekOffenseLeaders, "Offensive Player of the Week Candidates", AwardPeriod.Week, AwardSpecialty.Offense);
			WriteSeasonLeaderTable(outFile, mWeekDefenseLeaders, "Defensive Player of the Week Candidates", AwardPeriod.Week, AwardSpecialty.Defense);
			WriteSeasonLeaderTable(outFile, mWeekRookieOffenseLeaders, "Offensive Rookie of the Week Candidates", AwardPeriod.Week, AwardSpecialty.Offense);
			WriteSeasonLeaderTable(outFile, mWeekRookieDefenseLeaders, "Defensive Rookie of the Week Candidates", AwardPeriod.Week, AwardSpecialty.Defense);
			WriteSeasonLeaderTable(outFile, mWeekSpecialTeamsLeaders, "Special Teams Player of the Week Candidates", AwardPeriod.Week, AwardSpecialty.SpecialTeams);
			if (mInPlayoffs)
			{
				WriteSeasonLeaderTable(outFile, mPlayoffsOffenseLeaders, "Playoffs Offensive MVP Candidates", AwardPeriod.Playoffs, AwardSpecialty.Offense);
				WriteSeasonLeaderTable(outFile, mPlayoffsDefenseLeaders, "Playoffs Defensive MVP Candidates", AwardPeriod.Playoffs, AwardSpecialty.Defense);
				WriteSeasonLeaderTable(outFile, mPlayoffsSpecialTeamsLeaders, "Playoffs Special Teams MVP Candidates", AwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);
			}
			else
			{
				WriteSeasonLeaderTable(outFile, mSeasonOffenseLeaders, "Offensive Player of the Year Candidates", AwardPeriod.Season, AwardSpecialty.Offense);
				WriteSeasonLeaderTable(outFile, mSeasonDefenseLeaders, "Defensive Player of the Year Candidates", AwardPeriod.Season, AwardSpecialty.Defense);
				WriteSeasonLeaderTable(outFile, mSeasonRookieOffenseLeaders, "Offensive Rookie of the Year Candidates", AwardPeriod.Season, AwardSpecialty.Offense);
				WriteSeasonLeaderTable(outFile, mSeasonRookieDefenseLeaders, "Defensive Rookie of the Year Candidates", AwardPeriod.Season, AwardSpecialty.Defense);
				WriteSeasonLeaderTable(outFile, mSeasonSpecialTeamsLeaders, "Special Teams Player of the Year Candidates", AwardPeriod.Season, AwardSpecialty.SpecialTeams);
			}

			outFile.WriteLine("</body>");
			outFile.WriteLine("</html>");

			outFile.Close();
		}

		private int FindTopCareerPlayer(string[] validPositions, int startIndex, ref int nextIndex)
		{
			int foundIndex = -1;
			for (int curIndex = startIndex; curIndex < mPlayerCareerData.Length; ++curIndex)
			{
				bool goodPosition = false;
				foreach (string checkPosition in validPositions)
				{
					if (checkPosition == mPlayerCareerData[curIndex].PlayerPosition)
					{
						goodPosition = true;
						break;
					}
				}
				if (goodPosition)
				{
					nextIndex = curIndex + 1;
					foundIndex = mPlayerCareerData[curIndex].OriginalIndex;
					break;
				}
			}
			return foundIndex;
		}

		private void WriteBestSeasonPlayer(System.IO.StreamWriter outFile, string group, AwardSpecialty specialty)
		{
			if (mBestSeasonsData.ContainsKey(group))
			{
				PlayerCareerData careerData = mBestSeasonsData[group];
				string playerString = careerData.PlayerPosition + " " + careerData.PlayerName;
				playerString += "<br>" + careerData.DraftYear;

				outFile.WriteLine("        <tr style=\"font-size: 14px; font-family: 'Times New Roman'\">");
				outFile.WriteLine("            <td>" + group + "</td>");
				WritePlayerTeamCells(outFile, careerData.MostPlayedForTeam, careerData.MostPlayedForTeamID, playerString, careerData.PlayerID);
				outFile.WriteLine("            <td>" + GenerateCareerStatString(careerData, CareerAwardPeriod.RegularSeason, specialty) +
					" (" + careerData.Score + ")</td>");
				outFile.WriteLine("        </tr>");
			}
		}

		private void WriteAllTimePlayer(System.IO.StreamWriter outFile, string group, int index, 
			CareerAwardPeriod awardPeriod, AwardSpecialty specialty)
		{
			if (index >= 0)
			{
				string teamName = mPlayerCareerData[index].MostPlayedForTeam;
				int teamID = mPlayerCareerData[index].MostPlayedForTeamID;
				string playerString = mPlayerCareerData[index].PlayerPosition + " " + mPlayerCareerData[index].PlayerName;
				if (awardPeriod == CareerAwardPeriod.Rookie)
				{
					teamName = mPlayerCareerData[index].DraftedByTeam;
					teamID = mPlayerCareerData[index].DraftedByTeamID;
					playerString += "<br>" + mPlayerCareerData[index].DraftYear + " - ";
					if (mPlayerCareerData[index].DraftPos < 9999)
					{
						int orgDraftPos = mPlayerCareerData[index].DraftPos - 1;
						int draftRound = (orgDraftPos / 32) + 1;
						int draftPos = (orgDraftPos % 32) + 1;
						playerString += draftRound + "." + draftPos;
					}
					else
					{
						playerString += "Undrafted";
					}
				}
				
				outFile.WriteLine("        <tr style=\"font-size: 14px; font-family: 'Times New Roman'\">");
				outFile.WriteLine("            <td>" + group + "</td>");
				WritePlayerTeamCells(outFile, teamName, teamID, playerString, mPlayerCareerData[index].PlayerID);
				outFile.WriteLine("            <td>" + GenerateCareerStatString(mPlayerCareerData[index],awardPeriod,specialty) + "</td>");
				outFile.WriteLine("        </tr>");
			}
		}

		private void WriteBestSeasons()
		{
			string fullPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kBestSeasonsFile);
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(fullPath, false);

			outFile.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			outFile.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
			outFile.WriteLine("<body>");

			outFile.WriteLine("<head><title>Best Seasons</title></head>");

			outFile.WriteLine("    <table border=\"1\" cellpadding=\"4\" style=\"background-color: #DDDDDD; color:Black; width: 600px\">");
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>Best Seasons</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteBestSeasonPlayer(outFile, "QB", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "RB", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "FB", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "TE", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "WR", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "T", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "G", AwardSpecialty.Offense);
			WriteBestSeasonPlayer(outFile, "C", AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteBestSeasonPlayer(outFile, "DE", AwardSpecialty.Defense);
			WriteBestSeasonPlayer(outFile, "DT", AwardSpecialty.Defense);

			WriteBestSeasonPlayer(outFile, "OLB", AwardSpecialty.Defense);
			WriteBestSeasonPlayer(outFile, "ILB", AwardSpecialty.Defense);

			WriteBestSeasonPlayer(outFile, "CB", AwardSpecialty.Defense);
			WriteBestSeasonPlayer(outFile, "S", AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteBestSeasonPlayer(outFile, "P", AwardSpecialty.SpecialTeams);
			WriteBestSeasonPlayer(outFile, "K", AwardSpecialty.SpecialTeams);

			outFile.WriteLine("    </table>");

			outFile.WriteLine("</body>");
			outFile.WriteLine("</html>");

			outFile.Close();
		}

		private void WriteAllTimeTeam()
		{
			for (int i = 0; i < mPlayerCareerData.Length; ++i)
			{
				mPlayerCareerData[i].OriginalIndex = i;
			}

			int nextIndex = 0;
			Array.Sort(mPlayerCareerData, new CareerQBComparer());
			int qb1Index = FindTopCareerPlayer(new string[] { "QB" }, 0, ref nextIndex);
			int qb2Index = FindTopCareerPlayer(new string[] { "QB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRBComparer());
			int rb1Index = FindTopCareerPlayer(new string[] { "RB" }, 0, ref nextIndex);
			int rb2Index = FindTopCareerPlayer(new string[] { "RB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerFBComparer());
			int fb1Index = FindTopCareerPlayer(new string[] { "FB" }, 0, ref nextIndex);
			int fb2Index = FindTopCareerPlayer(new string[] { "FB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerTEComparer());
			int te1Index = FindTopCareerPlayer(new string[] { "TE" }, 0, ref nextIndex);
			int te2Index = FindTopCareerPlayer(new string[] { "TE" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerWRComparer());
			int wr1Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, 0, ref nextIndex);
			int wr2Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			int wr3Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			int wr4Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerOLComparer());
			int ot1Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, 0, ref nextIndex);
			int ot2Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int ot3Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int ot4Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int og1Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, 0, ref nextIndex);
			int og2Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int og3Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int og4Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int c1Index = FindTopCareerPlayer(new string[] { "C" }, 0, ref nextIndex);
			int c2Index = FindTopCareerPlayer(new string[] { "C" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerDLComparer());
			int de1Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, 0, ref nextIndex);
			int de2Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int de3Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int de4Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int dt1Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, 0, ref nextIndex);
			int dt2Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			int dt3Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			int dt4Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerLBComparer());
			int olb1Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, 0, ref nextIndex);
			int olb2Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int olb3Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int olb4Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int ilb1Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, 0, ref nextIndex);
			int ilb2Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			int ilb3Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			int ilb4Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerCBComparer());
			int cb1Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, 0, ref nextIndex);
			int cb2Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			int cb3Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			int cb4Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerSComparer());
			int s1Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, 0, ref nextIndex);
			int s2Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);
			int s3Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);
			int s4Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerSTComparer());
			int st1Index = FindTopCareerPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, 0, ref nextIndex);
			int st2Index = FindTopCareerPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPComparer());
			int p1Index = FindTopCareerPlayer(new string[] { "P", "K" }, 0, ref nextIndex);
			int p2Index = FindTopCareerPlayer(new string[] { "P", "K" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerKComparer());
			int k1Index = FindTopCareerPlayer(new string[] { "K", "P" }, 0, ref nextIndex);
			int k2Index = FindTopCareerPlayer(new string[] { "K", "P" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerOrgIndexComparer());

			string fullPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kAllTimeTeamFile);
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(fullPath, false);

			outFile.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			outFile.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
			outFile.WriteLine("<body>");

			outFile.WriteLine("<head><title>All-Time Team</title></head>");

			outFile.WriteLine("    <table border=\"1\" cellpadding=\"4\" style=\"background-color: #DDDDDD; color:Black; width: 600px\">");
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>All-Time Team</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>First String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "QB", qb1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "RB", rb1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "FB", fb1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "TE", te1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "C", c1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "DE", de1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DE", de2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "OLB", olb1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "OLB", olb2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "CB", cb1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "CB", cb2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllTimePlayer(outFile, "ST", st1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "P", p1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "K", k1Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.SpecialTeams);

			// Second string
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Second String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "QB", qb2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "RB", rb2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "FB", fb2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "TE", te2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "C", c2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "DE", de3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DE", de4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "OLB", olb3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "OLB", olb4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "CB", cb3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "CB", cb4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s3Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s4Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllTimePlayer(outFile, "ST", st2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "P", p2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "K", k2Index, CareerAwardPeriod.RegularSeason, AwardSpecialty.SpecialTeams);

			outFile.WriteLine("    </table>");

			outFile.WriteLine("</body>");
			outFile.WriteLine("</html>");

			outFile.Close();
		}

		private void WriteAllTimePlayoffsTeam()
		{
			for (int i = 0; i < mPlayerCareerData.Length; ++i)
			{
				mPlayerCareerData[i].OriginalIndex = i;
			}

			int nextIndex = 0;
			Array.Sort(mPlayerCareerData, new CareerPlayoffsQBComparer());
			int qb1Index = FindTopCareerPlayer(new string[] { "QB" }, 0, ref nextIndex);
			int qb2Index = FindTopCareerPlayer(new string[] { "QB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsRBComparer());
			int rb1Index = FindTopCareerPlayer(new string[] { "RB" }, 0, ref nextIndex);
			int rb2Index = FindTopCareerPlayer(new string[] { "RB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsFBComparer());
			int fb1Index = FindTopCareerPlayer(new string[] { "FB" }, 0, ref nextIndex);
			int fb2Index = FindTopCareerPlayer(new string[] { "FB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsTEComparer());
			int te1Index = FindTopCareerPlayer(new string[] { "TE" }, 0, ref nextIndex);
			int te2Index = FindTopCareerPlayer(new string[] { "TE" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsWRComparer());
			int wr1Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, 0, ref nextIndex);
			int wr2Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			int wr3Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			int wr4Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsOLComparer());
			int ot1Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, 0, ref nextIndex);
			int ot2Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int ot3Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int ot4Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int og1Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, 0, ref nextIndex);
			int og2Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int og3Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int og4Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int c1Index = FindTopCareerPlayer(new string[] { "C" }, 0, ref nextIndex);
			int c2Index = FindTopCareerPlayer(new string[] { "C" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerPlayoffsDLComparer());
			int de1Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, 0, ref nextIndex);
			int de2Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int de3Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int de4Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int dt1Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, 0, ref nextIndex);
			int dt2Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			int dt3Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			int dt4Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsLBComparer());
			int olb1Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, 0, ref nextIndex);
			int olb2Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int olb3Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int olb4Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int ilb1Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, 0, ref nextIndex);
			int ilb2Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			int ilb3Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			int ilb4Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsCBComparer());
			int cb1Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, 0, ref nextIndex);
			int cb2Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			int cb3Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			int cb4Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsSComparer());
			int s1Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, 0, ref nextIndex);
			int s2Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);
			int s3Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);
			int s4Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerPlayoffsSTComparer());
			int st1Index = FindTopCareerPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, 0, ref nextIndex);
			int st2Index = FindTopCareerPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsPComparer());
			int p1Index = FindTopCareerPlayer(new string[] { "P", "K" }, 0, ref nextIndex);
			int p2Index = FindTopCareerPlayer(new string[] { "P", "K" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerPlayoffsKComparer());
			int k1Index = FindTopCareerPlayer(new string[] { "K", "P" }, 0, ref nextIndex);
			int k2Index = FindTopCareerPlayer(new string[] { "K", "P" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerOrgIndexComparer());

			string fullPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kAllTimePlayoffsTeamFile);
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(fullPath, false);

			outFile.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			outFile.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
			outFile.WriteLine("<body>");

			outFile.WriteLine("<head><title>All-Time Playoffs Team</title></head>");

			outFile.WriteLine("    <table border=\"1\" cellpadding=\"4\" style=\"background-color: #DDDDDD; color:Black; width: 600px\">");
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>All-Time Playoffs Team</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>First String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "QB", qb1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "RB", rb1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "FB", fb1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "TE", te1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "C", c1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "DE", de1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DE", de2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "OLB", olb1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "OLB", olb2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "CB", cb1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "CB", cb2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllTimePlayer(outFile, "ST", st1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "P", p1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "K", k1Index, CareerAwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);

			// Second string
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Second String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "QB", qb2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "RB", rb2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "FB", fb2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "TE", te2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "C", c2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "DE", de3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DE", de4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "OLB", olb3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "OLB", olb4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "CB", cb3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "CB", cb4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s3Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s4Index, CareerAwardPeriod.Playoffs, AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllTimePlayer(outFile, "ST", st2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "P", p2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "K", k2Index, CareerAwardPeriod.Playoffs, AwardSpecialty.SpecialTeams);

			outFile.WriteLine("    </table>");

			outFile.WriteLine("</body>");
			outFile.WriteLine("</html>");

			outFile.Close();
		}

		private void WriteAllTimeRookieTeam()
		{
			for (int i = 0; i < mPlayerCareerData.Length; ++i)
			{
				mPlayerCareerData[i].OriginalIndex = i;
			}

			int nextIndex = 0;
			Array.Sort(mPlayerCareerData, new CareerRookieQBComparer());
			int qb1Index = FindTopCareerPlayer(new string[] { "QB" }, 0, ref nextIndex);
			int qb2Index = FindTopCareerPlayer(new string[] { "QB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieRBComparer());
			int rb1Index = FindTopCareerPlayer(new string[] { "RB" }, 0, ref nextIndex);
			int rb2Index = FindTopCareerPlayer(new string[] { "RB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieFBComparer());
			int fb1Index = FindTopCareerPlayer(new string[] { "FB" }, 0, ref nextIndex);
			int fb2Index = FindTopCareerPlayer(new string[] { "FB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieTEComparer());
			int te1Index = FindTopCareerPlayer(new string[] { "TE" }, 0, ref nextIndex);
			int te2Index = FindTopCareerPlayer(new string[] { "TE" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieWRComparer());
			int wr1Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, 0, ref nextIndex);
			int wr2Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			int wr3Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			int wr4Index = FindTopCareerPlayer(new string[] { "FL", "SE" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieOLComparer());
			int ot1Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, 0, ref nextIndex);
			int ot2Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int ot3Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int ot4Index = FindTopCareerPlayer(new string[] { "LT", "RT" }, nextIndex, ref nextIndex);
			int og1Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, 0, ref nextIndex);
			int og2Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int og3Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int og4Index = FindTopCareerPlayer(new string[] { "LG", "RG" }, nextIndex, ref nextIndex);
			int c1Index = FindTopCareerPlayer(new string[] { "C" }, 0, ref nextIndex);
			int c2Index = FindTopCareerPlayer(new string[] { "C" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerRookieDLComparer());
			int de1Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, 0, ref nextIndex);
			int de2Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int de3Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int de4Index = FindTopCareerPlayer(new string[] { "LDE", "RDE" }, nextIndex, ref nextIndex);
			int dt1Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, 0, ref nextIndex);
			int dt2Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			int dt3Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			int dt4Index = FindTopCareerPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieLBComparer());
			int olb1Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, 0, ref nextIndex);
			int olb2Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int olb3Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int olb4Index = FindTopCareerPlayer(new string[] { "WLB", "SLB" }, nextIndex, ref nextIndex);
			int ilb1Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, 0, ref nextIndex);
			int ilb2Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			int ilb3Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			int ilb4Index = FindTopCareerPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieCBComparer());
			int cb1Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, 0, ref nextIndex);
			int cb2Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			int cb3Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			int cb4Index = FindTopCareerPlayer(new string[] { "LCB", "RCB" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieSComparer());
			int s1Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, 0, ref nextIndex);
			int s2Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);
			int s3Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);
			int s4Index = FindTopCareerPlayer(new string[] { "FS", "SS" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerRookieSTComparer());
			int st1Index = FindTopCareerPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, 0, ref nextIndex);
			int st2Index = FindTopCareerPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookiePComparer());
			int p1Index = FindTopCareerPlayer(new string[] { "P", "K" }, 0, ref nextIndex);
			int p2Index = FindTopCareerPlayer(new string[] { "P", "K" }, nextIndex, ref nextIndex);
			Array.Sort(mPlayerCareerData, new CareerRookieKComparer());
			int k1Index = FindTopCareerPlayer(new string[] { "K", "P" }, 0, ref nextIndex);
			int k2Index = FindTopCareerPlayer(new string[] { "K", "P" }, nextIndex, ref nextIndex);

			Array.Sort(mPlayerCareerData, new CareerOrgIndexComparer());

			string fullPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), kAllTimeRookieTeamFile);
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(fullPath, false);

			outFile.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			outFile.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
			outFile.WriteLine("<body>");

			outFile.WriteLine("<head><title>All-Time Rookie Team</title></head>");

			outFile.WriteLine("    <table border=\"1\" cellpadding=\"4\" style=\"background-color: #DDDDDD; color:Black; width: 600px\">");
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>All-Time Rookie Team</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>First String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "QB", qb1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "RB", rb1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "FB", fb1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "TE", te1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "C", c1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "DE", de1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DE", de2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "OLB", olb1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "OLB", olb2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "CB", cb1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "CB", cb2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s1Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllTimePlayer(outFile, "ST", st1Index, CareerAwardPeriod.Rookie, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "P", p1Index, CareerAwardPeriod.Rookie, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "K", k1Index, CareerAwardPeriod.Rookie, AwardSpecialty.SpecialTeams);

			// Second string
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Second String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "QB", qb2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "RB", rb2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "FB", fb2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "TE", te2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "WR", wr4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "T", ot4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "G", og4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);
			WriteAllTimePlayer(outFile, "C", c2Index, CareerAwardPeriod.Rookie, AwardSpecialty.Offense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllTimePlayer(outFile, "DE", de3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DE", de4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "DT", dt4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "OLB", olb3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "OLB", olb4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "ILB", ilb4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);

			WriteAllTimePlayer(outFile, "CB", cb3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "CB", cb4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s3Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);
			WriteAllTimePlayer(outFile, "S", s4Index, CareerAwardPeriod.Rookie, AwardSpecialty.Defense);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllTimePlayer(outFile, "ST", st2Index, CareerAwardPeriod.Rookie, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "P", p2Index, CareerAwardPeriod.Rookie, AwardSpecialty.SpecialTeams);
			WriteAllTimePlayer(outFile, "K", k2Index, CareerAwardPeriod.Rookie, AwardSpecialty.SpecialTeams);

			outFile.WriteLine("    </table>");

			outFile.WriteLine("</body>");
			outFile.WriteLine("</html>");

			outFile.Close();
		}

		private int FindTopSeasonPlayer(string[] validPositions, int startIndex, bool rookiesOnly, ref int nextIndex)
		{
			int foundIndex = -1;
			for (int curIndex = startIndex; curIndex < mPlayerSeasonData.Length; ++curIndex)
			{
				bool goodPosition = false;
				foreach (string checkPosition in validPositions)
				{
					if (checkPosition == mPlayerSeasonData[curIndex].PlayerPosition)
					{
						goodPosition = true;
						break;
					}
				}
				if (goodPosition && (false == rookiesOnly || mPlayerSeasonData[curIndex].PlayerExperience == 1))
				{
					nextIndex = curIndex + 1;
					foundIndex = mPlayerSeasonData[curIndex].OriginalIndex;
					break;
				}
			}
			return foundIndex;
		}

		private void WriteAllProOffensivePlayer(System.IO.StreamWriter outFile, string group, int index)
		{
			if (index >= 0)
			{
				string playerString = mPlayerSeasonData[index].PlayerPosition + " " + mPlayerSeasonData[index].PlayerName;
				outFile.WriteLine("        <tr style=\"font-size: 14px; font-family: 'Times New Roman'\">");
				outFile.WriteLine("            <td>" + group + "</td>");
				WritePlayerTeamCells(outFile, mPlayerSeasonData[index].PlayerTeam, mPlayerSeasonData[index].PlayerTeamID, playerString, mPlayerSeasonData[index].PlayerID);
				outFile.WriteLine("            <td>" + GenerateOffensiveString(mPlayerSeasonData[index].SeasonStats.StatRec) + "</td>");
				outFile.WriteLine("        </tr>");
			}
		}

		private void WriteAllProDefensivePlayer(System.IO.StreamWriter outFile, string group, int index)
		{
			if (index >= 0)
			{
				string playerString = mPlayerSeasonData[index].PlayerPosition + " " + mPlayerSeasonData[index].PlayerName;
				outFile.WriteLine("        <tr style=\"font-size: 14px; font-family: 'Times New Roman'\">");
				outFile.WriteLine("            <td>" + group + "</td>");
				WritePlayerTeamCells(outFile, mPlayerSeasonData[index].PlayerTeam, mPlayerSeasonData[index].PlayerTeamID, playerString, mPlayerSeasonData[index].PlayerID);
				outFile.WriteLine("            <td>" + GenerateDefensiveString(mPlayerSeasonData[index].SeasonStats.StatRec) + "</td>");
				outFile.WriteLine("        </tr>");
			}
		}

		private void WriteAllProSpecialTeamsPlayer(System.IO.StreamWriter outFile, string group, int index)
		{
			if (index >= 0)
			{
				string playerString = mPlayerSeasonData[index].PlayerPosition + " " + mPlayerSeasonData[index].PlayerName;
				outFile.WriteLine("        <tr style=\"font-size: 14px; font-family: 'Times New Roman'\">");
				outFile.WriteLine("            <td>" + group + "</td>");
				WritePlayerTeamCells(outFile, mPlayerSeasonData[index].PlayerTeam, mPlayerSeasonData[index].PlayerTeamID, playerString, mPlayerSeasonData[index].PlayerID);
				outFile.WriteLine("            <td>" + GenerateSpecialTeamsString(mPlayerSeasonData[index].SeasonStats.StatRec) + "</td>");
				outFile.WriteLine("        </tr>");
			}
		}

		private void WriteSeasonAllProTeam()
		{
			WriteAllProTeam(kSeasonAllProFile, false);
		}

		private void WriteSeasonAllRookieTeam()
		{
			WriteAllProTeam(kSeasonAllRookieFile, true);
		}

		private void WriteAllProTeam(string filename, bool rookiesOnly)
		{
			for (int i = 0; i < mPlayerSeasonData.Length; ++i)
			{
				mPlayerSeasonData[i].OriginalIndex = i;
			}

			int nextIndex = 0;
			Array.Sort(mPlayerSeasonData, new SeasonQBComparer());
			int qb1Index = FindTopSeasonPlayer(new string[] { "QB" }, 0, rookiesOnly, ref nextIndex);
			int qb2Index = FindTopSeasonPlayer(new string[] { "QB" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonRBComparer());
			int rb1Index = FindTopSeasonPlayer(new string[] { "RB" }, 0, rookiesOnly, ref nextIndex);
			int rb2Index = FindTopSeasonPlayer(new string[] { "RB" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonFBComparer());
			int fb1Index = FindTopSeasonPlayer(new string[] { "FB" }, 0, rookiesOnly, ref nextIndex);
			int fb2Index = FindTopSeasonPlayer(new string[] { "FB" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonTEComparer());
			int te1Index = FindTopSeasonPlayer(new string[] { "TE" }, 0, rookiesOnly, ref nextIndex);
			int te2Index = FindTopSeasonPlayer(new string[] { "TE" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonWRComparer());
			int wr1Index = FindTopSeasonPlayer(new string[] { "FL", "SE" }, 0, rookiesOnly, ref nextIndex);
			int wr2Index = FindTopSeasonPlayer(new string[] { "FL", "SE" }, nextIndex, rookiesOnly, ref nextIndex);
			int wr3Index = FindTopSeasonPlayer(new string[] { "FL", "SE" }, nextIndex, rookiesOnly, ref nextIndex);
			int wr4Index = FindTopSeasonPlayer(new string[] { "FL", "SE" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonOLComparer());
			int ot1Index = FindTopSeasonPlayer(new string[] { "LT", "RT" }, 0, rookiesOnly, ref nextIndex);
			int ot2Index = FindTopSeasonPlayer(new string[] { "LT", "RT" }, nextIndex, rookiesOnly, ref nextIndex);
			int ot3Index = FindTopSeasonPlayer(new string[] { "LT", "RT" }, nextIndex, rookiesOnly, ref nextIndex);
			int ot4Index = FindTopSeasonPlayer(new string[] { "LT", "RT" }, nextIndex, rookiesOnly, ref nextIndex);
			int og1Index = FindTopSeasonPlayer(new string[] { "LG", "RG" }, 0, rookiesOnly, ref nextIndex);
			int og2Index = FindTopSeasonPlayer(new string[] { "LG", "RG" }, nextIndex, rookiesOnly, ref nextIndex);
			int og3Index = FindTopSeasonPlayer(new string[] { "LG", "RG" }, nextIndex, rookiesOnly, ref nextIndex);
			int og4Index = FindTopSeasonPlayer(new string[] { "LG", "RG" }, nextIndex, rookiesOnly, ref nextIndex);
			int c1Index = FindTopSeasonPlayer(new string[] { "C" }, 0, rookiesOnly, ref nextIndex);
			int c2Index = FindTopSeasonPlayer(new string[] { "C" }, nextIndex, rookiesOnly, ref nextIndex);

			Array.Sort(mPlayerSeasonData, new SeasonDLComparer());
			int de1Index = FindTopSeasonPlayer(new string[] { "LDE", "RDE" }, 0, rookiesOnly, ref nextIndex);
			int de2Index = FindTopSeasonPlayer(new string[] { "LDE", "RDE" }, nextIndex, rookiesOnly, ref nextIndex);
			int de3Index = FindTopSeasonPlayer(new string[] { "LDE", "RDE" }, nextIndex, rookiesOnly, ref nextIndex);
			int de4Index = FindTopSeasonPlayer(new string[] { "LDE", "RDE" }, nextIndex, rookiesOnly, ref nextIndex);
			int dt1Index = FindTopSeasonPlayer(new string[] { "LDT", "RDT", "NT" }, 0, rookiesOnly, ref nextIndex);
			int dt2Index = FindTopSeasonPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, rookiesOnly, ref nextIndex);
			int dt3Index = FindTopSeasonPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, rookiesOnly, ref nextIndex);
			int dt4Index = FindTopSeasonPlayer(new string[] { "LDT", "RDT", "NT" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonLBComparer());
			int olb1Index = FindTopSeasonPlayer(new string[] { "WLB", "SLB" }, 0, rookiesOnly, ref nextIndex);
			int olb2Index = FindTopSeasonPlayer(new string[] { "WLB", "SLB" }, nextIndex, rookiesOnly, ref nextIndex);
			int olb3Index = FindTopSeasonPlayer(new string[] { "WLB", "SLB" }, nextIndex, rookiesOnly, ref nextIndex);
			int olb4Index = FindTopSeasonPlayer(new string[] { "WLB", "SLB" }, nextIndex, rookiesOnly, ref nextIndex);
			int ilb1Index = FindTopSeasonPlayer(new string[] { "WILB", "SILB", "MLB" }, 0, rookiesOnly, ref nextIndex);
			int ilb2Index = FindTopSeasonPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, rookiesOnly, ref nextIndex);
			int ilb3Index = FindTopSeasonPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, rookiesOnly, ref nextIndex);
			int ilb4Index = FindTopSeasonPlayer(new string[] { "WILB", "SILB", "MLB" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonCBComparer());
			int cb1Index = FindTopSeasonPlayer(new string[] { "LCB", "RCB" }, 0, rookiesOnly, ref nextIndex);
			int cb2Index = FindTopSeasonPlayer(new string[] { "LCB", "RCB" }, nextIndex, rookiesOnly, ref nextIndex);
			int cb3Index = FindTopSeasonPlayer(new string[] { "LCB", "RCB" }, nextIndex, rookiesOnly, ref nextIndex);
			int cb4Index = FindTopSeasonPlayer(new string[] { "LCB", "RCB" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonSComparer());
			int s1Index = FindTopSeasonPlayer(new string[] { "FS", "SS" }, 0, rookiesOnly, ref nextIndex);
			int s2Index = FindTopSeasonPlayer(new string[] { "FS", "SS" }, nextIndex, rookiesOnly, ref nextIndex);
			int s3Index = FindTopSeasonPlayer(new string[] { "FS", "SS" }, nextIndex, rookiesOnly, ref nextIndex);
			int s4Index = FindTopSeasonPlayer(new string[] { "FS", "SS" }, nextIndex, rookiesOnly, ref nextIndex);

			Array.Sort(mPlayerSeasonData, new SeasonSTComparer());
			int st1Index = FindTopSeasonPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, 0, rookiesOnly, ref nextIndex);
			int st2Index = FindTopSeasonPlayer(new string[] { "LCB", "RCB", "FL", "SE", "RB", "SS", "FS" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonPComparer());
			int p1Index = FindTopSeasonPlayer(new string[] { "P", "K" }, 0, rookiesOnly, ref nextIndex);
			int p2Index = FindTopSeasonPlayer(new string[] { "P", "K" }, nextIndex, rookiesOnly, ref nextIndex);
			Array.Sort(mPlayerSeasonData, new SeasonKComparer());
			int k1Index = FindTopSeasonPlayer(new string[] { "K", "P" }, 0, rookiesOnly, ref nextIndex);
			int k2Index = FindTopSeasonPlayer(new string[] { "K", "P" }, nextIndex, rookiesOnly, ref nextIndex);

			Array.Sort(mPlayerSeasonData, new SeasonOrgIndexComparer());

			string fullPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), filename);
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(fullPath, false);

			outFile.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			outFile.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
			outFile.WriteLine("<body>");

			if (rookiesOnly)
			{
				outFile.WriteLine("<head><title>All-Pro Team</title></head>");
			}
			else
			{
				outFile.WriteLine("<head><title>All-Rookie Team</title></head>");
			}

			outFile.WriteLine("    <table border=\"1\" cellpadding=\"4\" style=\"background-color: #DDDDDD; color:Black; width: 600px\">");
			if (rookiesOnly)
			{
				outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>All-Rookie Team</strong></td></tr>");
			}
			else
			{
				outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 18px; font-family: 'Times New Roman'\"><strong>All-Pro Team</strong></td></tr>");
			}

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>First String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllProOffensivePlayer(outFile, "QB", qb1Index);
			WriteAllProOffensivePlayer(outFile, "RB", rb1Index);
			WriteAllProOffensivePlayer(outFile, "FB", fb1Index);
			WriteAllProOffensivePlayer(outFile, "TE", te1Index);
			WriteAllProOffensivePlayer(outFile, "WR", wr1Index);
			WriteAllProOffensivePlayer(outFile, "WR", wr2Index);
			WriteAllProOffensivePlayer(outFile, "T", ot1Index);
			WriteAllProOffensivePlayer(outFile, "T", ot2Index);
			WriteAllProOffensivePlayer(outFile, "G", og1Index);
			WriteAllProOffensivePlayer(outFile, "G", og2Index);
			WriteAllProOffensivePlayer(outFile, "C", c1Index);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllProDefensivePlayer(outFile, "DE", de1Index);
			WriteAllProDefensivePlayer(outFile, "DE", de2Index);
			WriteAllProDefensivePlayer(outFile, "DT", dt1Index);
			WriteAllProDefensivePlayer(outFile, "DT", dt2Index);

			WriteAllProDefensivePlayer(outFile, "OLB", olb1Index);
			WriteAllProDefensivePlayer(outFile, "OLB", olb2Index);
			WriteAllProDefensivePlayer(outFile, "ILB", ilb1Index);
			WriteAllProDefensivePlayer(outFile, "ILB", ilb2Index);

			WriteAllProDefensivePlayer(outFile, "CB", cb1Index);
			WriteAllProDefensivePlayer(outFile, "CB", cb2Index);
			WriteAllProDefensivePlayer(outFile, "S", s1Index);
			WriteAllProDefensivePlayer(outFile, "S", s2Index);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllProSpecialTeamsPlayer(outFile, "ST", st1Index);
			WriteAllProSpecialTeamsPlayer(outFile, "P", p1Index);
			WriteAllProSpecialTeamsPlayer(outFile, "K", k1Index);

			// Second string
			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Second String</strong></td></tr>");

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Offense</strong></td></tr>");

			WriteAllProOffensivePlayer(outFile, "QB", qb2Index);
			WriteAllProOffensivePlayer(outFile, "RB", rb2Index);
			WriteAllProOffensivePlayer(outFile, "FB", fb2Index);
			WriteAllProOffensivePlayer(outFile, "TE", te2Index);
			WriteAllProOffensivePlayer(outFile, "WR", wr3Index);
			WriteAllProOffensivePlayer(outFile, "WR", wr4Index);
			WriteAllProOffensivePlayer(outFile, "T", ot3Index);
			WriteAllProOffensivePlayer(outFile, "T", ot4Index);
			WriteAllProOffensivePlayer(outFile, "G", og3Index);
			WriteAllProOffensivePlayer(outFile, "G", og4Index);
			WriteAllProOffensivePlayer(outFile, "C", c2Index);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Defense</strong></td></tr>");

			WriteAllProDefensivePlayer(outFile, "DE", de3Index);
			WriteAllProDefensivePlayer(outFile, "DE", de4Index);
			WriteAllProDefensivePlayer(outFile, "DT", dt3Index);
			WriteAllProDefensivePlayer(outFile, "DT", dt4Index);

			WriteAllProDefensivePlayer(outFile, "OLB", olb3Index);
			WriteAllProDefensivePlayer(outFile, "OLB", olb4Index);
			WriteAllProDefensivePlayer(outFile, "ILB", ilb3Index);
			WriteAllProDefensivePlayer(outFile, "ILB", ilb4Index);

			WriteAllProDefensivePlayer(outFile, "CB", cb3Index);
			WriteAllProDefensivePlayer(outFile, "CB", cb4Index);
			WriteAllProDefensivePlayer(outFile, "S", s3Index);
			WriteAllProDefensivePlayer(outFile, "S", s4Index);

			outFile.WriteLine("        <tr><td colspan=\"4\" style=\"background-color: #FFFFFF; color:Black; text-align: center; font-size: 14px; font-family: 'Times New Roman'\"><strong>Special Teams</strong></td></tr>");

			WriteAllProSpecialTeamsPlayer(outFile, "ST", st2Index);
			WriteAllProSpecialTeamsPlayer(outFile, "P", p2Index);
			WriteAllProSpecialTeamsPlayer(outFile, "K", k2Index);

			outFile.WriteLine("    </table>");

			outFile.WriteLine("</body>");
			outFile.WriteLine("</html>");

			outFile.Close();
		}

		private void WriteStatsHeader(System.IO.StreamWriter outFile)
		{
			outFile.Write("Player,Position,Team,Exp");
			foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
			{
				if (category != PlayerStatCategory.Count)
				{
					outFile.Write("," + category);
				}
			}
			outFile.Write(",Offense,Defense,SpecialTeams");

			outFile.Write(",PassTargets,Catches,ReceivingYards,ReceivingTDs,CatchesOf20YardsPlus,FirstDownCatches,PassDrops,ReceivingGamesOver100Yards" +
				",ThirdDownReceivingConversions,YardsAfterCatch,SuccessfulCatches,BadPassesCaught");
			outFile.Write(",PassCompletions,PassAttempts,PassYards,TDPasses,INTThrown,FirstDownPasses,PassingGamesOver300Yards" +
				",TimesSacked,SackedYards,ThirdDownPassConversions,ThrowsOf20YardsPlus,DoubleCoveragesThrownInto,DoubleCoveragesAvoided,BadPasses,SuccessfulPasses");
			outFile.Write(",RushAttempts,RushingYards,RushTD,FirstDownRushes,RunsOf10YardsPlus,RushingGamesOver100Yards" +
				",ThirdDownRushConversions,RunsForLoss,RunsOf20YardsPlus,FumblesLost,SuccessfulRuns");
			outFile.Write(",KeyRunBlock,RunBlockOpportunity,PancakeBlocks,SacksAllowed");
			outFile.Write(",Fumbles,TwoPointConversions");

			outFile.Write(",Tackles,Assists,Sacks,PassesBlocked,QBHurries,QBKnockdowns,TacklesForLoss,AssistedTacklesForLoss");
			outFile.Write(",INTs,INTReturnYards,INTReturnTDs,PassesDefended,PassesCaught,HasKeyCoverage,ThrownAt,ReceptionsOf20YardsPlusGivenUp");
			outFile.Write(",ForcedFumbles,FumbleRecoveries,MiscTD");

			outFile.Write(",FGMade,FG40PlusMade,FG50PlusMade,Kickoffs,KickoffYards,KickoffTouchbacks,TotalFieldPositionAfterKickoff");
			outFile.Write(",PuntIn20,PuntNetYards");
			outFile.Write(",KickReturnTDs,KickReturnYards,PuntReturnTDs,PuntReturnYards,SpecialTeamsTackles");

			outFile.Write(",PassPlays,RunPlays,OffensivePassPlays,OffensiveRunPlays,DefensivePassPlays,DefensiveRunPlays,DraftPos,DraftYear");
		}

		private void WriteKeyStats(System.IO.StreamWriter outFile, DataReader.LeagueData.PlayerGameStatsRecord gameRec)
		{
			outFile.Write("," + gameRec.PassTargets);
			outFile.Write("," + gameRec.Catches);
			outFile.Write("," + gameRec.ReceivingYards);
			outFile.Write("," + gameRec.ReceivingTDs);
			outFile.Write("," + gameRec.CatchesOf20YardsPlus);
			outFile.Write("," + gameRec.FirstDownCatches);
			outFile.Write("," + gameRec.PassDrops);
			outFile.Write("," + gameRec.ReceivingGamesOver100Yards);
			outFile.Write("," + gameRec.ThirdDownReceivingConversions);
			outFile.Write("," + gameRec.YardsAfterCatch);
			outFile.Write("," + gameRec.SuccessfulCatches);
			outFile.Write("," + gameRec.BadPassesCaught);
			outFile.Write("," + gameRec.PassCompletions);
			outFile.Write("," + gameRec.PassAttempts);
			outFile.Write("," + gameRec.PassYards);
			outFile.Write("," + gameRec.TDPasses);
			outFile.Write("," + gameRec.INTThrown);
			outFile.Write("," + gameRec.FirstDownPasses);
			outFile.Write("," + gameRec.PassingGamesOver300Yards);
			outFile.Write("," + gameRec.TimesSacked);
			outFile.Write("," + gameRec.SackedYards);
			outFile.Write("," + gameRec.ThirdDownPassConversions);
			outFile.Write("," + gameRec.ThrowsOf20YardsPlus);
			outFile.Write("," + gameRec.DoubleCoveragesThrownInto);
			outFile.Write("," + gameRec.DoubleCoveragesAvoided);
			outFile.Write("," + gameRec.BadPasses);
			outFile.Write("," + gameRec.SuccessfulPasses);
			outFile.Write("," + gameRec.RushAttempts);
			outFile.Write("," + gameRec.RushingYards);
			outFile.Write("," + gameRec.RushTD);
			outFile.Write("," + gameRec.FirstDownRushes);
			outFile.Write("," + gameRec.RunsOf10YardsPlus);
			outFile.Write("," + gameRec.RushingGamesOver100Yards);
			outFile.Write("," + gameRec.ThirdDownRushConversions);
			outFile.Write("," + gameRec.RunsForLoss);
			outFile.Write("," + gameRec.RunsOf20YardsPlus);
			outFile.Write("," + gameRec.FumblesLost);
			outFile.Write("," + gameRec.SuccessfulRuns);
			outFile.Write("," + gameRec.KeyRunBlock);
			outFile.Write("," + gameRec.KeyRunBlockOpportunites);
			outFile.Write("," + gameRec.PancakeBlocks);
			outFile.Write("," + gameRec.SacksAllowed);
			outFile.Write("," + gameRec.Fumbles);
			outFile.Write("," + gameRec.TwoPointConversions);
			outFile.Write("," + gameRec.Tackles);
			outFile.Write("," + gameRec.Assists);
			outFile.Write("," + gameRec.Sacks);
			outFile.Write("," + gameRec.PassesBlocked);
			outFile.Write("," + gameRec.QBHurries);
			outFile.Write("," + gameRec.QBKnockdowns);
			outFile.Write("," + gameRec.TacklesForLoss);
			outFile.Write("," + gameRec.AssistedTacklesForLoss);
			outFile.Write("," + gameRec.INTs);
			outFile.Write("," + gameRec.INTReturnYards);
			outFile.Write("," + gameRec.INTReturnTDs);
			outFile.Write("," + gameRec.PassesDefended);
			outFile.Write("," + gameRec.PassesCaught);
			outFile.Write("," + gameRec.HasKeyCoverage);
			outFile.Write("," + gameRec.ThrownAt);
			outFile.Write("," + gameRec.ReceptionsOf20YardsPlusGivenUp);
			outFile.Write("," + gameRec.ForcedFumbles);
			outFile.Write("," + gameRec.FumbleRecoveries);
			outFile.Write("," + gameRec.MiscTD);
			outFile.Write("," + gameRec.FGMade);
			outFile.Write("," + gameRec.FG40PlusMade);
			outFile.Write("," + gameRec.FG50PlusMade);
			outFile.Write("," + gameRec.Kickoffs);
			outFile.Write("," + gameRec.KickoffYards);
			outFile.Write("," + gameRec.KickoffTouchbacks);
			outFile.Write("," + gameRec.TotalFieldPositionAfterKickoff);
			outFile.Write("," + gameRec.PuntIn20);
			outFile.Write("," + gameRec.PuntNetYards);
			outFile.Write("," + gameRec.KickReturnTDs);
			outFile.Write("," + gameRec.KickReturnYards);
			outFile.Write("," + gameRec.PuntReturnTDs);
			outFile.Write("," + gameRec.PuntReturnYards);
			outFile.Write("," + gameRec.SpecialTeamsTackles);
			outFile.Write("," + gameRec.PassPlays);
			outFile.Write("," + gameRec.RunPlays);
			outFile.Write("," + gameRec.OffensivePassPlays);
			outFile.Write("," + gameRec.OffensiveRunPlays);
			outFile.Write("," + gameRec.DefensivePassPlays);
			outFile.Write("," + gameRec.DefensiveRunPlays);
		}

		private void WriteCareerStats()
		{
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_CareerAwardsRegularSeason.csv");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.WriteLine();

			foreach (PlayerCareerData player in mPlayerCareerData)
			{
				outFile.Write("\"" + player.PlayerName + "\"");
				outFile.Write(",\"" + player.PlayerPosition + "\"");
				outFile.Write(",\"" + player.MostPlayedForTeam + "\"");
				outFile.Write("," + player.PlayerExperience);
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						outFile.Write("," + player.RegularSeasonStats.CategoryScore[(int)category]);
					}
				}
				outFile.Write("," + player.RegularSeasonStats.OffenseScore + "," +
					player.RegularSeasonStats.DefenseScore + "," + player.RegularSeasonStats.SpecialTeamsScore);

				WriteKeyStats(outFile, player.RegularSeasonStats.StatRec);

				outFile.Write("," + player.DraftPos);
				outFile.Write("," + player.DraftYear);

				outFile.WriteLine();
			}

			outFile.Close();

			filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_CareerAwardsPlayoffs.csv");
			outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.WriteLine();

			foreach (PlayerCareerData player in mPlayerCareerData)
			{
				outFile.Write("\"" + player.PlayerName + "\"");
				outFile.Write(",\"" + player.PlayerPosition + "\"");
				outFile.Write(",\"" + player.MostPlayedForTeam + "\"");
				outFile.Write("," + player.PlayerExperience);
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						outFile.Write("," + player.PlayoffsStats.CategoryScore[(int)category]);
					}
				}
				outFile.Write("," + player.PlayoffsStats.OffenseScore + "," +
					player.PlayoffsStats.DefenseScore + "," + player.PlayoffsStats.SpecialTeamsScore);

				WriteKeyStats(outFile, player.PlayoffsStats.StatRec);

				outFile.Write("," + player.DraftPos);
				outFile.Write("," + player.DraftYear);

				outFile.WriteLine();
			}

			outFile.Close();

			filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_CareerAwardsRookie.csv");
			outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.WriteLine();

			foreach (PlayerCareerData player in mPlayerCareerData)
			{
				outFile.Write("\"" + player.PlayerName + "\"");
				outFile.Write(",\"" + player.PlayerPosition + "\"");
				outFile.Write(",\"" + player.DraftedByTeam + "\"");
				outFile.Write("," + player.PlayerExperience);
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						outFile.Write("," + player.RookieStats.CategoryScore[(int)category]);
					}
				}
				outFile.Write("," + player.RookieStats.OffenseScore + "," +
					player.RookieStats.DefenseScore + "," + player.RookieStats.SpecialTeamsScore);

				WriteKeyStats(outFile, player.RookieStats.StatRec);

				outFile.Write("," + player.DraftPos);
				outFile.Write("," + player.DraftYear);

				outFile.WriteLine();
			}

			outFile.Close();
		}

		private void WriteSeasonStats()
		{
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_SeasonAwardsWeek.csv");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.Write(",CapCost");
			outFile.WriteLine();

			foreach (PlayerSeasonData player in mPlayerSeasonData)
			{
				outFile.Write("\"" + player.PlayerName + "\"");
				outFile.Write(",\"" + player.PlayerPosition + "\"");
				outFile.Write(",\"" + player.PlayerTeam + "\"");
				outFile.Write("," + player.PlayerExperience);
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						outFile.Write("," + player.WeekStats.CategoryScore[(int)category]);
					}
				}
				outFile.Write("," + player.WeekOffenseScore + "," + player.WeekDefenseScore + "," + player.WeekSpecialTeamsScore);

				WriteKeyStats(outFile, player.WeekStats.StatRec);

				outFile.WriteLine();
			}

			outFile.Close();

			filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_SeasonAwardsSeason.csv");
			outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.Write(",CapCost");
			outFile.WriteLine();

			foreach (PlayerSeasonData player in mPlayerSeasonData)
			{
				outFile.Write("\"" + player.PlayerName + "\"");
				outFile.Write(",\"" + player.PlayerPosition + "\"");
				outFile.Write(",\"" + player.PlayerTeam + "\"");
				outFile.Write("," + player.PlayerExperience);
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						outFile.Write("," + player.SeasonStats.CategoryScore[(int)category]);
					}
				}
				outFile.Write("," + player.SeasonOffenseScore + "," + player.SeasonDefenseScore + "," + player.SeasonSpecialTeamsScore);

				WriteKeyStats(outFile, player.SeasonStats.StatRec);

				outFile.Write("," + player.DraftPos);
				outFile.Write("," + player.DraftYear);
				outFile.Write("," + player.CapCost);

				outFile.WriteLine();
			}

			outFile.Close();

			filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_SeasonAwardsPlayoffs.csv");
			outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.Write(",CapCost");
			outFile.WriteLine();

			foreach (PlayerSeasonData player in mPlayerSeasonData)
			{
				outFile.Write("\"" + player.PlayerName + "\"");
				outFile.Write(",\"" + player.PlayerPosition + "\"");
				outFile.Write(",\"" + player.PlayerTeam + "\"");
				outFile.Write("," + player.PlayerExperience);
				foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
				{
					if (category != PlayerStatCategory.Count)
					{
						outFile.Write("," + player.PlayoffsStats.CategoryScore[(int)category]);
					}
				}
				outFile.Write("," + player.PlayoffsOffenseScore + "," + player.PlayoffsDefenseScore + "," + player.PlayoffsSpecialTeamsScore);

				WriteKeyStats(outFile, player.PlayoffsStats.StatRec);

				outFile.Write("," + player.DraftPos);
				outFile.Write("," + player.DraftYear);
				outFile.Write("," + player.CapCost);

				outFile.WriteLine();
			}

			outFile.Close();
		}

		private void FillLeaderArray(PlayerSeasonData[] leaderArray)
		{
			for (int i = 0; i < kLeaderCount; ++i)
			{
				leaderArray[i] = mPlayerSeasonData[i];
			}
		}

		private void FillRookieLeaderArray(PlayerSeasonData[] leaderArray)
		{
			int leaderIndex = 0;
			int playerIndex = 0;
			while (playerIndex < mPlayerSeasonData.Length && leaderIndex < kLeaderCount)
			{
				if (mPlayerSeasonData[playerIndex].IsRookie)
				{
					leaderArray[leaderIndex] = mPlayerSeasonData[playerIndex];
					++leaderIndex;
				}
				++playerIndex;
			}
		}

		private void FindLeaders()
		{
			mWeekOffenseLeaders = new PlayerSeasonData[kLeaderCount];
			mWeekDefenseLeaders = new PlayerSeasonData[kLeaderCount];
			mWeekSpecialTeamsLeaders = new PlayerSeasonData[kLeaderCount];
			mWeekRookieOffenseLeaders = new PlayerSeasonData[kLeaderCount];
			mWeekRookieDefenseLeaders = new PlayerSeasonData[kLeaderCount];
			mSeasonOffenseLeaders = new PlayerSeasonData[kLeaderCount];
			mSeasonDefenseLeaders = new PlayerSeasonData[kLeaderCount];
			mSeasonSpecialTeamsLeaders = new PlayerSeasonData[kLeaderCount];
			mSeasonRookieOffenseLeaders = new PlayerSeasonData[kLeaderCount];
			mSeasonRookieDefenseLeaders = new PlayerSeasonData[kLeaderCount];
			mPlayoffsOffenseLeaders = new PlayerSeasonData[kLeaderCount];
			mPlayoffsDefenseLeaders = new PlayerSeasonData[kLeaderCount];
			mPlayoffsSpecialTeamsLeaders = new PlayerSeasonData[kLeaderCount];

			Array.Sort(mPlayerSeasonData, new WeekOffenseComparer());
			FillLeaderArray(mWeekOffenseLeaders);
			FillRookieLeaderArray(mWeekRookieOffenseLeaders);
			Array.Sort(mPlayerSeasonData, new WeekDefenseComparer());
			FillLeaderArray(mWeekDefenseLeaders);
			FillRookieLeaderArray(mWeekRookieDefenseLeaders);
			Array.Sort(mPlayerSeasonData, new WeekSpecialTeamsComparer());
			FillLeaderArray(mWeekSpecialTeamsLeaders);
			Array.Sort(mPlayerSeasonData, new SeasonOffenseComparer());
			FillLeaderArray(mSeasonOffenseLeaders);
			FillRookieLeaderArray(mSeasonRookieOffenseLeaders);
			Array.Sort(mPlayerSeasonData, new SeasonDefenseComparer());
			FillLeaderArray(mSeasonDefenseLeaders);
			FillRookieLeaderArray(mSeasonRookieDefenseLeaders);
			Array.Sort(mPlayerSeasonData, new SeasonSpecialTeamsComparer());
			FillLeaderArray(mSeasonSpecialTeamsLeaders);
			Array.Sort(mPlayerSeasonData, new PlayoffsOffenseComparer());
			FillLeaderArray(mPlayoffsOffenseLeaders);
			Array.Sort(mPlayerSeasonData, new PlayoffsDefenseComparer());
			FillLeaderArray(mPlayoffsDefenseLeaders);
			Array.Sort(mPlayerSeasonData, new PlayoffsSpecialTeamsComparer());
			FillLeaderArray(mPlayoffsSpecialTeamsLeaders);
		}

		private void CollectSeasonPlayers()
		{
			int curGameWeek = 0;
			if (mCurrentLeagueData.GameStage == 1 || mCurrentLeagueData.GameStage == 2)
			{
				curGameWeek = mCurrentLeagueData.CurrentWeek - 1;
			}
			else if (mCurrentLeagueData.GameStage == 3)
			{
				curGameWeek = 26;	// Bowl Game
			}
			else
			{
				mPlayerSeasonData = null;
				return;
			}

			if (curGameWeek >= 23)
			{
				mInPlayoffs = true;
			}
			else
			{
				mInPlayoffs = false;
			}

			mPlayerSeasonData = new PlayerSeasonData[mCurrentLeagueData.PlayerActiveRecords.Length];

			int seasonIndex = mCurrentLeagueData.SeasonsPlayed - 1;
			int curGameStatIndex = 0;
			int curPlayerIndex = 0;
			int curPlayerHistoricalIndex = 0;
			while (curPlayerIndex < mCurrentLeagueData.PlayerActiveRecords.Length)
			{
				mPlayerSeasonData[curPlayerIndex] = new PlayerSeasonData();
				int playerID = mCurrentLeagueData.PlayerActiveRecords[curPlayerIndex].PlayerID;
				// Find the player in the historical index
				while (mCurrentLeagueData.PlayerHistoricalRecords[curPlayerHistoricalIndex].PlayerID < playerID
					&& curPlayerHistoricalIndex < mCurrentLeagueData.PlayerHistoricalRecords.Length
					)
				{
					++curPlayerHistoricalIndex;
				}
				if (curPlayerHistoricalIndex >= mCurrentLeagueData.PlayerHistoricalRecords.Length)
				{
					// Past the end
					break;
				}

				DataReader.LeagueData.PlayerActiveRecord activeRec = mCurrentLeagueData.PlayerActiveRecords[curPlayerIndex];
				DataReader.LeagueData.PlayerHistoricalRecord historicalRec = mCurrentLeagueData.PlayerHistoricalRecords[curPlayerHistoricalIndex];
				if (historicalRec.PlayerID == playerID)
				{
					// Get to the start in the game records
					while (mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex].PlayerID < playerID
						&& curGameStatIndex < mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex].Length
						)
					{
						++curGameStatIndex;
					}
					if (curGameStatIndex >= mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex].Length)
					{
						// Past the end
						break;
					}

					if (mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex].PlayerID == playerID)
					{
						mPlayerSeasonData[curPlayerIndex].PlayerName = historicalRec.FirstName;
						if (activeRec.Experience < 2)
						{
							mPlayerSeasonData[curPlayerIndex].IsRookie = true;
						}
						else
						{
							mPlayerSeasonData[curPlayerIndex].IsRookie = false;
						}
						if (historicalRec.NickName.Length > 0)
						{
							mPlayerSeasonData[curPlayerIndex].PlayerName += " '" + historicalRec.NickName + "'";
						}
						mPlayerSeasonData[curPlayerIndex].PlayerName += " " + historicalRec.LastName;
						mPlayerSeasonData[curPlayerIndex].PlayerPosition = mUniverseData.PositionMap[activeRec.Position];
						if (activeRec.Team >= 0 && activeRec.Team < mCurrentLeagueData.TeamRecords.Length)
						{
							mPlayerSeasonData[curPlayerIndex].PlayerTeam = mUniverseData.TeamCityName(activeRec.Team);
						}
						else
						{
							mPlayerSeasonData[curPlayerIndex].PlayerTeam = "FA";
						}
						mPlayerSeasonData[curPlayerIndex].PlayerID = playerID;
						mPlayerSeasonData[curPlayerIndex].PlayerTeamID = activeRec.Team;
						mPlayerSeasonData[curPlayerIndex].PlayerExperience = activeRec.Experience;
						mPlayerSeasonData[curPlayerIndex].DraftYear = historicalRec.YearDrafted;
						if (historicalRec.YearDrafted == 0)
						{
							mPlayerSeasonData[curPlayerIndex].DraftPos = 9999;
						}
						else
						{
							mPlayerSeasonData[curPlayerIndex].DraftPos = ((historicalRec.DraftRound-1) * 32) + historicalRec.DraftPick;
						}
						mPlayerSeasonData[curPlayerIndex].CapCost = (activeRec.Bonus[0] + activeRec.Salary[0]) * 10000;

						// Now process each of this player's games
						while (curGameStatIndex < mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex].Length
							&& mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex].PlayerID == playerID
							)
						{
							DataReader.LeagueData.PlayerGameStatsRecord gameRec = mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex];
							if (gameRec.Week <= 22)
							{
								AddToSeasonStats(gameRec, mPlayerSeasonData[curPlayerIndex].SeasonStats.StatRec);
							}
							else
							{
								AddToSeasonStats(gameRec, mPlayerSeasonData[curPlayerIndex].PlayoffsStats.StatRec);
							}
							if (gameRec.Week == curGameWeek)
							{
								mPlayerSeasonData[curPlayerIndex].WeekStats.StatRec = gameRec;
							}

							++curGameStatIndex;
						}
					}
				}

				++curPlayerIndex;
			}
		}

		private void CheckSeasonBest(PlayerStatEntry statEntry, PlayerCareerData careerData, short season, int[] gamesPlayedForTeam)
		{
			int mostGamesPlayed = 0;
			int commonTeamIndex = -1;
			int mostPlayedForTeamID = -1;
			for (int teamIndex = 0; teamIndex < gamesPlayedForTeam.Length; ++teamIndex)
			{
				if (gamesPlayedForTeam[teamIndex] > mostGamesPlayed)
				{
					commonTeamIndex = teamIndex;
					mostGamesPlayed = gamesPlayedForTeam[teamIndex];
				}
			}
			string mostPlayedForTeam = "FA";
			if (commonTeamIndex >= 0)
			{
				mostPlayedForTeam = mUniverseData.TeamCityName(commonTeamIndex);
				mostPlayedForTeamID = commonTeamIndex;
			}

			bool doReplace = false;
			double score1 = 0.0;
			double score2 = 0.0;
			if (!mBestSeasonsData.ContainsKey(careerData.PlayerPositionGroup))
			{
				doReplace = true;
			}
			else
			{
				PlayerCareerData bestData = mBestSeasonsData[careerData.PlayerPositionGroup];
				if (careerData.PlayerPositionGroup == "QB")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Pass];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Pass];
				}
				else if (careerData.PlayerPositionGroup == "RB")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Rush] +
						statEntry.CategoryScore[(int)PlayerStatCategory.Recv] +
						statEntry.CategoryScore[(int)PlayerStatCategory.MiscOff];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				}
				else if (careerData.PlayerPositionGroup == "FB")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Rush] +
						statEntry.CategoryScore[(int)PlayerStatCategory.Recv] +
						statEntry.CategoryScore[(int)PlayerStatCategory.Block] +
						statEntry.CategoryScore[(int)PlayerStatCategory.MiscOff];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				}
				else if (careerData.PlayerPositionGroup == "TE")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Recv] +
						statEntry.CategoryScore[(int)PlayerStatCategory.Block] +
						statEntry.CategoryScore[(int)PlayerStatCategory.MiscOff];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				}
				else if (careerData.PlayerPositionGroup == "WR")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Recv] +
						statEntry.CategoryScore[(int)PlayerStatCategory.MiscOff];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				}
				else if (careerData.PlayerPositionGroup == "T" || careerData.PlayerPositionGroup == "G" || careerData.PlayerPositionGroup == "C")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Block];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block];
				}
				else if (careerData.PlayerPositionGroup == "DE" || careerData.PlayerPositionGroup == "DT")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.MiscDef] +
						statEntry.CategoryScore[(int)PlayerStatCategory.PRush];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PRush];
				}
				else if (careerData.PlayerPositionGroup == "OLB" || careerData.PlayerPositionGroup == "ILB")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.MiscDef] +
						statEntry.CategoryScore[(int)PlayerStatCategory.PDef] +
						statEntry.CategoryScore[(int)PlayerStatCategory.PRush];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PRush];
				}
				else if (careerData.PlayerPositionGroup == "CB")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.PDef];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				}
				else if (careerData.PlayerPositionGroup == "S")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.MiscDef] +
						statEntry.CategoryScore[(int)PlayerStatCategory.PDef];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
						bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				}
				else if (careerData.PlayerPositionGroup == "P")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Punt];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Punt];
				}
				else if (careerData.PlayerPositionGroup == "K")
				{
					score1 = statEntry.CategoryScore[(int)PlayerStatCategory.Kick];
					score2 = bestData.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Kick];
				}

				if (score1 > score2)
				{
					doReplace = true;
				}
				else
				{
					bestData.Score = score2;
				}
			}
			if (doReplace)
			{
				PlayerCareerData newData = new PlayerCareerData();
				newData.RegularSeasonStats = statEntry;
				newData.DraftYear = season;
				newData.MostPlayedForTeam = mostPlayedForTeam;
				newData.OriginalIndex = careerData.OriginalIndex;
				newData.PlayerName = careerData.PlayerName;
				newData.PlayerPosition = careerData.PlayerPosition;
				newData.PlayerPositionGroup = careerData.PlayerPositionGroup;
				newData.Score = score1;
				newData.PlayerID = careerData.PlayerID;
				newData.MostPlayedForTeamID = mostPlayedForTeamID;
				mBestSeasonsData[careerData.PlayerPositionGroup] = newData;
			}
		}

		private void CollectCareerPlayers()
		{
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), mLeaguePrefix + "_BestSeasons.csv");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(filename, false);

			WriteStatsHeader(outFile);
			outFile.Write(",Season");
			outFile.WriteLine();

			mPlayerCareerData = new PlayerCareerData[mCurrentLeagueData.PlayerHistoricalRecords.Length];
			mBestSeasonsData = new Dictionary<string, PlayerCareerData>();

			int careerIndex = 0;
			int curGameStatIndex = 0;
			int seasonIndex = mCurrentLeagueData.SeasonsPlayed - 1;
			int[] gamesPlayedForTeam = new int[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			int[] seasonGamesPlayedForTeam = new int[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			int[] rookieGamesPlayedForTeam = new int[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

			foreach (DataReader.LeagueData.PlayerHistoricalRecord historicalRec in mCurrentLeagueData.PlayerHistoricalRecords)
			{
				mPlayerCareerData[careerIndex] = new PlayerCareerData();
				mPlayerCareerData[careerIndex].PlayerName = historicalRec.FirstName;
				if (historicalRec.NickName.Length > 0)
				{
					mPlayerCareerData[careerIndex].PlayerName += " '" + historicalRec.NickName + "'";
				}
				mPlayerCareerData[careerIndex].PlayerName += " " + historicalRec.LastName;

				mPlayerCareerData[careerIndex].PlayerID = historicalRec.PlayerID;
				mPlayerCareerData[careerIndex].PlayerExperience = historicalRec.Experience;
				mPlayerCareerData[careerIndex].PlayerPosition = mUniverseData.PositionMap[historicalRec.Position];
				mPlayerCareerData[careerIndex].PlayerPositionGroup = mPositionGroupMap[mPlayerCareerData[careerIndex].PlayerPosition];
				mPlayerCareerData[careerIndex].DraftYear = historicalRec.YearDrafted;
				if (historicalRec.YearDrafted == 0)
				{
					mPlayerCareerData[careerIndex].DraftPos = 9999;
				}
				else
				{
					mPlayerCareerData[careerIndex].DraftPos = ((historicalRec.DraftRound - 1) * 32) + historicalRec.DraftPick;
				}

				PlayerStatEntry seasonStatEntry = new PlayerStatEntry();
				for (int j = 0; j < historicalRec.YearsInLeagueCount; ++j)
				{
					seasonStatEntry = new PlayerStatEntry();
					int yearIndex = historicalRec.YearsInLeague[j] - mCurrentLeagueData.StartingYear;
					if (mCurrentLeagueData.PlayerGameStatsRecords[yearIndex] != null)
					{
						int gameRecordIndex = historicalRec.YearDataIndex[j];
						for (int k = 0; k < mCurrentLeagueData.PlayerGamesPerSeason; ++k)
						{
							DataReader.LeagueData.PlayerGameStatsRecord gameRec = mCurrentLeagueData.PlayerGameStatsRecords[yearIndex][gameRecordIndex + k];
							if (gameRec.Team >= 0 && gameRec.Team < gamesPlayedForTeam.Length)
							{
								if (gameRec.Week <= 22)
								{
									AddToSeasonStats(gameRec, seasonStatEntry.StatRec);
									AddToSeasonStats(gameRec, mPlayerCareerData[careerIndex].RegularSeasonStats.StatRec);
									if (   historicalRec.YearsInLeague[j] == historicalRec.YearDrafted
										&& historicalRec.YearDrafted != mCurrentLeagueData.StartingYear
										)
									{
										AddToSeasonStats(gameRec, mPlayerCareerData[careerIndex].RookieStats.StatRec);
										mPlayerCareerData[careerIndex].DraftYear = historicalRec.YearsInLeague[j];
										rookieGamesPlayedForTeam[gameRec.Team] += 1;
									}
								}
								else
								{
									AddToSeasonStats(gameRec, mPlayerCareerData[careerIndex].PlayoffsStats.StatRec);
								}
								gamesPlayedForTeam[gameRec.Team] += 1;
								seasonGamesPlayedForTeam[gameRec.Team] += 1;
							}
						}

						CalculatePoints(seasonStatEntry, mPlayerCareerData[careerIndex].PlayerPosition);
						WriteBestSeasonStats(outFile, mPlayerCareerData[careerIndex], seasonStatEntry, historicalRec.YearsInLeague[j], seasonGamesPlayedForTeam);
						CheckSeasonBest(seasonStatEntry, mPlayerCareerData[careerIndex], historicalRec.YearsInLeague[j], seasonGamesPlayedForTeam);
						seasonGamesPlayedForTeam = new int[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
					}
				}

				// Get to the start in the game records
				while (curGameStatIndex < mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex].Length
					&& mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex].PlayerID < historicalRec.PlayerID
					)
				{
					++curGameStatIndex;
				}

				// Now process each of this player's games
				seasonStatEntry = new PlayerStatEntry();
				while (curGameStatIndex < mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex].Length
					&& mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex].PlayerID == historicalRec.PlayerID
					)
				{
					DataReader.LeagueData.PlayerGameStatsRecord gameRec = mCurrentLeagueData.PlayerGameStatsRecords[seasonIndex][curGameStatIndex];
					if (gameRec.Team >= 0 && gameRec.Team < gamesPlayedForTeam.Length)
					{
						if (gameRec.Week <= 22)
						{
							AddToSeasonStats(gameRec, seasonStatEntry.StatRec);
							AddToSeasonStats(gameRec, mPlayerCareerData[careerIndex].RegularSeasonStats.StatRec);
							if (   mCurrentLeagueData.CurrentYear == historicalRec.YearDrafted
								&& historicalRec.Experience == 1
								)
							{
								AddToSeasonStats(gameRec, mPlayerCareerData[careerIndex].RookieStats.StatRec);
								mPlayerCareerData[careerIndex].DraftYear = mCurrentLeagueData.CurrentYear;
								rookieGamesPlayedForTeam[gameRec.Team] += 1;
							}
						}
						else
						{
							AddToSeasonStats(gameRec, mPlayerCareerData[careerIndex].PlayoffsStats.StatRec);
						}
						gamesPlayedForTeam[gameRec.Team] += 1;
						seasonGamesPlayedForTeam[gameRec.Team] += 1;
					}

					++curGameStatIndex;
				}
				CalculatePoints(seasonStatEntry, mPlayerCareerData[careerIndex].PlayerPosition);
				WriteBestSeasonStats(outFile, mPlayerCareerData[careerIndex], seasonStatEntry, mCurrentLeagueData.CurrentYear, seasonGamesPlayedForTeam);
				CheckSeasonBest(seasonStatEntry, mPlayerCareerData[careerIndex], mCurrentLeagueData.CurrentYear, seasonGamesPlayedForTeam);
				seasonGamesPlayedForTeam = new int[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

				int mostGamesPlayed = 0;
				int commonTeamIndex = -1;
				int rookieMostGamesPlayed = 0;
				int rookieTeamIndex = -1;
				for (int teamIndex = 0; teamIndex < gamesPlayedForTeam.Length; ++teamIndex)
				{
					if (gamesPlayedForTeam[teamIndex] > mostGamesPlayed)
					{
						commonTeamIndex = teamIndex;
						mostGamesPlayed = gamesPlayedForTeam[teamIndex];
					}
					if (rookieGamesPlayedForTeam[teamIndex] > rookieMostGamesPlayed)
					{
						rookieTeamIndex = teamIndex;
						rookieMostGamesPlayed = rookieGamesPlayedForTeam[teamIndex];
					}
					// Reset for next player
					gamesPlayedForTeam[teamIndex] = 0;
					rookieGamesPlayedForTeam[teamIndex] = 0;
				}
				if (commonTeamIndex >= 0)
				{
					mPlayerCareerData[careerIndex].MostPlayedForTeam = mUniverseData.TeamCityName(commonTeamIndex);
				}
				else
				{
					mPlayerCareerData[careerIndex].MostPlayedForTeam = "FA";
				}
				mPlayerCareerData[careerIndex].MostPlayedForTeamID = commonTeamIndex;

				if (historicalRec.DraftedBy >= 0 && historicalRec.DraftedBy <= 31 && historicalRec.YearDrafted > 0)
				{
					mPlayerCareerData[careerIndex].DraftedByTeam = mUniverseData.TeamCityName(historicalRec.DraftedBy);
					mPlayerCareerData[careerIndex].DraftedByTeamID = historicalRec.DraftedBy;
				}
				else
				{
					mPlayerCareerData[careerIndex].DraftedByTeam = "FA";
				}

				if (rookieTeamIndex >= 0)
				{
					mPlayerCareerData[careerIndex].RookiePlayedForTeam = mUniverseData.TeamCityName(rookieTeamIndex);
				}
				else
				{
					mPlayerCareerData[careerIndex].RookiePlayedForTeam = "FA";
				}
				mPlayerCareerData[careerIndex].RookiePlayedForTeamID = rookieTeamIndex;

				careerIndex += 1;
			}

			outFile.Close();
		}

		private void WriteBestSeasonStats(System.IO.StreamWriter outFile, PlayerCareerData player, PlayerStatEntry statEntry, short season, int[] gamesPlayedForTeam)
		{
			int mostGamesPlayed = 0;
			int commonTeamIndex = -1;
			for (int teamIndex = 0; teamIndex < gamesPlayedForTeam.Length; ++teamIndex)
			{
				if (gamesPlayedForTeam[teamIndex] > mostGamesPlayed)
				{
					commonTeamIndex = teamIndex;
					mostGamesPlayed = gamesPlayedForTeam[teamIndex];
				}
			}
			string mostPlayedForTeam = "FA";
			if (commonTeamIndex >= 0)
			{
				mostPlayedForTeam = mUniverseData.TeamCityName(commonTeamIndex);
			}

			outFile.Write("\"" + player.PlayerName + "\"");
			outFile.Write(",\"" + player.PlayerPosition + "\"");
			outFile.Write(",\"" + mostPlayedForTeam + "\"");
			outFile.Write("," + player.PlayerExperience);
			foreach (PlayerStatCategory category in Enum.GetValues(typeof(PlayerStatCategory)))
			{
				if (category != PlayerStatCategory.Count)
				{
					outFile.Write("," + statEntry.CategoryScore[(int)category]);
				}
			}
			outFile.Write("," + statEntry.OffenseScore + "," +
				statEntry.DefenseScore + "," + statEntry.SpecialTeamsScore);

			WriteKeyStats(outFile, statEntry.StatRec);

			outFile.Write("," + player.DraftPos);
			outFile.Write("," + player.DraftYear);

			outFile.Write("," + season);
			outFile.WriteLine();
		}

		private void CollectPlayers()
		{
			CollectSeasonPlayers();
			if (mRunCareerReports)
			{
				CollectCareerPlayers();
			}
		}

		private void AddToSeasonStats(DataReader.LeagueData.PlayerGameStatsRecord srcRec, DataReader.LeagueData.PlayerGameStatsRecord targetRec)
		{
			targetRec.GamePlayed += srcRec.GamePlayed;
			targetRec.GameStarted += srcRec.GameStarted;
			targetRec.PassAttempts += srcRec.PassAttempts;
			targetRec.PassCompletions += srcRec.PassCompletions;
			targetRec.PassYards += srcRec.PassYards;
			targetRec.LongestPass += srcRec.LongestPass;
			targetRec.TDPasses += srcRec.TDPasses;
			targetRec.INTThrown += srcRec.INTThrown;
			targetRec.TimesSacked += srcRec.TimesSacked;
			targetRec.SackedYards += srcRec.SackedYards;
			targetRec.RushAttempts += srcRec.RushAttempts;
			targetRec.RushingYards += srcRec.RushingYards;
			targetRec.LongestRun += srcRec.LongestRun;
			targetRec.RushTD += srcRec.RushTD;
			targetRec.Catches += srcRec.Catches;
			targetRec.ReceivingYards += srcRec.ReceivingYards;
			targetRec.LongestReception += srcRec.LongestReception;
			targetRec.ReceivingTDs += srcRec.ReceivingTDs;
			targetRec.PassTargets += srcRec.PassTargets;
			targetRec.YardsAfterCatch += srcRec.YardsAfterCatch;
			targetRec.PassDrops += srcRec.PassDrops;
			targetRec.PuntReturns += srcRec.PuntReturns;
			targetRec.PuntReturnYards += srcRec.PuntReturnYards;
			targetRec.PuntReturnTDs += srcRec.PuntReturnTDs;
			targetRec.KickReturns += srcRec.KickReturns;
			targetRec.KickReturnYards += srcRec.KickReturnYards;
			targetRec.KickReturnTDs += srcRec.KickReturnTDs;
			targetRec.Fumbles += srcRec.Fumbles;
			targetRec.FumbleRecoveries += srcRec.FumbleRecoveries;
			targetRec.ForcedFumbles += srcRec.ForcedFumbles;
			targetRec.MiscTD += srcRec.MiscTD;
			targetRec.KeyRunBlock += srcRec.KeyRunBlock;
			targetRec.KeyRunBlockOpportunites += srcRec.KeyRunBlockOpportunites;
			targetRec.SacksAllowed += srcRec.SacksAllowed;
			targetRec.Tackles += srcRec.Tackles;
			targetRec.Assists += srcRec.Assists;
			targetRec.Sacks += srcRec.Sacks;
			targetRec.INTs += srcRec.INTs;
			targetRec.INTReturnYards += srcRec.INTReturnYards;
			targetRec.INTReturnTDs += srcRec.INTReturnTDs;
			targetRec.PassesDefended += srcRec.PassesDefended;
			targetRec.PassesBlocked += srcRec.PassesBlocked;
			targetRec.QBHurries += srcRec.QBHurries;
			targetRec.PassesCaught += srcRec.PassesCaught;
			targetRec.PassPlays += srcRec.PassPlays;
			targetRec.RunPlays += srcRec.RunPlays;
			targetRec.FGMade += srcRec.FGMade;
			targetRec.FGAttempted += srcRec.FGAttempted;
			targetRec.FGLong += srcRec.FGLong;
			targetRec.PAT += srcRec.PAT;
			targetRec.PATAttempted += srcRec.PATAttempted;
			targetRec.Punts += srcRec.Punts;
			targetRec.PuntYards += srcRec.PuntYards;
			targetRec.PuntLong += srcRec.PuntLong;
			targetRec.PuntIn20 += srcRec.PuntIn20;
			targetRec.Points += srcRec.Points;
			targetRec.OpposingTeamID += srcRec.OpposingTeamID;
			targetRec.ThirdDownRushes += srcRec.ThirdDownRushes;
			targetRec.ThirdDownRushConversions += srcRec.ThirdDownRushConversions;
			targetRec.ThirdDownPassAttempts += srcRec.ThirdDownPassAttempts;
			targetRec.ThirdDownPassCompletions += srcRec.ThirdDownPassCompletions;
			targetRec.ThirdDownPassConversions += srcRec.ThirdDownPassConversions;
			targetRec.ThirdDownReceivingTargets += srcRec.ThirdDownReceivingTargets;
			targetRec.ThirdDownReceivingCatches += srcRec.ThirdDownReceivingCatches;
			targetRec.ThirdDownReceivingConversions += srcRec.ThirdDownReceivingConversions;
			targetRec.FirstDownRushes += srcRec.FirstDownRushes;
			targetRec.FirstDownPasses += srcRec.FirstDownPasses;
			targetRec.FirstDownCatches += srcRec.FirstDownCatches;
			targetRec.FG40PlusAttempts += srcRec.FG40PlusAttempts;
			targetRec.FG40PlusMade += srcRec.FG40PlusMade;
			targetRec.FG50PlusAttempts += srcRec.FG50PlusAttempts;
			targetRec.FG50PlusMade += srcRec.FG50PlusMade;
			targetRec.PuntNetYards += srcRec.PuntNetYards;
			targetRec.SpecialTeamsTackles += srcRec.SpecialTeamsTackles;
			targetRec.Unknown14 += srcRec.Unknown14;
			targetRec.TimesKnockedDown += srcRec.TimesKnockedDown;
			targetRec.RedZoneRushes += srcRec.RedZoneRushes;
			targetRec.RedZoneRushingYards += srcRec.RedZoneRushingYards;
			targetRec.RedZonePassAttempts += srcRec.RedZonePassAttempts;
			targetRec.RedZonePassCompletions += srcRec.RedZonePassCompletions;
			targetRec.RedZonePassingYards += srcRec.RedZonePassingYards;
			targetRec.RedZoneReceivingTargets += srcRec.RedZoneReceivingTargets;
			targetRec.RedZoneReceivingCatches += srcRec.RedZoneReceivingCatches;
			targetRec.RedZoneReceivingYards += srcRec.RedZoneReceivingYards;
			targetRec.TotalTDs += srcRec.TotalTDs;
			targetRec.TwoPointConversions += srcRec.TwoPointConversions;
			targetRec.PancakeBlocks += srcRec.PancakeBlocks;
			targetRec.QBKnockdowns += srcRec.QBKnockdowns;
			targetRec.Unknown23 += srcRec.Unknown23;
			targetRec.SpecialTeamsPlays += srcRec.SpecialTeamsPlays;
			targetRec.RushingGamesOver100Yards += srcRec.RushingGamesOver100Yards;
			targetRec.ReceivingGamesOver100Yards += srcRec.ReceivingGamesOver100Yards;
			targetRec.PassingGamesOver300Yards += srcRec.PassingGamesOver300Yards;
			targetRec.RunsOf10YardsPlus += srcRec.RunsOf10YardsPlus;
			targetRec.CatchesOf20YardsPlus += srcRec.CatchesOf20YardsPlus;
			targetRec.ThrowsOf20YardsPlus += srcRec.ThrowsOf20YardsPlus;
			targetRec.AllPurposeYards += srcRec.AllPurposeYards;
			targetRec.YardsFromScrimmage += srcRec.YardsFromScrimmage;
			targetRec.DoubleCoveragesThrownInto += srcRec.DoubleCoveragesThrownInto;
			targetRec.DoubleCoveragesAvoided += srcRec.DoubleCoveragesAvoided;
			targetRec.BadPasses += srcRec.BadPasses;
			targetRec.RunsForLoss += srcRec.RunsForLoss;
			targetRec.RunsOf20YardsPlus += srcRec.RunsOf20YardsPlus;
			targetRec.FumblesLost += srcRec.FumblesLost;
			targetRec.HasKeyCoverage += srcRec.HasKeyCoverage;
			targetRec.ThrownAt += srcRec.ThrownAt;
			targetRec.TacklesForLoss += srcRec.TacklesForLoss;
			targetRec.AssistedTacklesForLoss += srcRec.AssistedTacklesForLoss;
			targetRec.ReceptionsOf20YardsPlusGivenUp += srcRec.ReceptionsOf20YardsPlusGivenUp;
			targetRec.TotalFieldPositionAfterKickoff += srcRec.Kickoffs;
			targetRec.Kickoffs += srcRec.Kickoffs;
			targetRec.KickoffYards += srcRec.KickoffYards;
			targetRec.KickoffTouchbacks += srcRec.KickoffTouchbacks;
			targetRec.OffensivePassPlays += srcRec.OffensivePassPlays;
			targetRec.OffensiveRunPlays += srcRec.OffensiveRunPlays;
			targetRec.DefensivePassPlays += srcRec.DefensivePassPlays;
			targetRec.DefensiveRunPlays += srcRec.DefensiveRunPlays;
			targetRec.SuccessfulPasses += srcRec.SuccessfulPasses;
			targetRec.SuccessfulCatches += srcRec.SuccessfulCatches;
			targetRec.SuccessfulRuns += srcRec.SuccessfulRuns;
			targetRec.BadPassesCaught += srcRec.BadPassesCaught;
		}

		private void CalculatePlayerSeasonPoints()
		{
			for (int i = 0; i < mPlayerSeasonData.Length; ++i)
			{
				CalculatePoints(mPlayerSeasonData[i].WeekStats, mPlayerSeasonData[i].PlayerPosition);
				CalculatePoints(mPlayerSeasonData[i].SeasonStats, mPlayerSeasonData[i].PlayerPosition);
				CalculatePoints(mPlayerSeasonData[i].PlayoffsStats, mPlayerSeasonData[i].PlayerPosition);
			}
		}

		private void CalculatePlayerCareerPoints()
		{
			for (int i = 0; i < mPlayerCareerData.Length; ++i)
			{
				CalculatePoints(mPlayerCareerData[i].RegularSeasonStats, mPlayerCareerData[i].PlayerPosition);
				CalculatePoints(mPlayerCareerData[i].PlayoffsStats, mPlayerCareerData[i].PlayerPosition);
				CalculatePoints(mPlayerCareerData[i].RookieStats, mPlayerCareerData[i].PlayerPosition);
			}
		}

		private void CalculatePoints(PlayerStatEntry statEntry, string playerPosition)
		{
			if (playerPosition.Length > 0)
			{
				Type gameRecType = typeof(DataReader.LeagueData.PlayerGameStatsRecord);
				DataReader.LeagueData.PlayerGameStatsRecord gameRec = statEntry.StatRec;
				int posGroupIndex = mPositionToIndexMap[playerPosition];
				foreach (StatWeight curWeight in mStatWeights)
				{
					if (curWeight.Weight[posGroupIndex] != 0.0)
					{
						PlayerStatCategory category = (PlayerStatCategory)Enum.Parse(typeof(PlayerStatCategory), curWeight.Category[posGroupIndex]);
						if (category != PlayerStatCategory.Count)
						{
							FieldInfo fieldInfo = gameRecType.GetField(curWeight.FieldName);
							int statValue = (int)fieldInfo.GetValue(gameRec);
							statEntry.CategoryScore[(int)category] += (statValue * curWeight.Weight[posGroupIndex]);
						}
					}
				}
			}
		}

		private double ClampRatingScore(double calc)
		{
			if (calc < 0.0)
			{
				return 0.0;
			}
			else if (calc > 2.375)
			{
				return 2.375;
			}
			else
			{
				return calc;
			}
		}
		private string GenerateOffensiveString(DataReader.LeagueData.PlayerGameStatsRecord rec)
		{
			string output = "";
			if (rec.PassAttempts > 0)
			{
				int completionPercent = (rec.PassCompletions * 100) / rec.PassAttempts;
				double averagePerAttempt = (double)rec.PassYards / (double)rec.PassAttempts;
				double completionScore = ClampRatingScore((((double)rec.PassCompletions / (double)rec.PassAttempts) - 0.3) / 0.2);
				double yardsPerAttemptScore = ClampRatingScore((((double)rec.PassYards / (double)rec.PassAttempts) - 3.0) / 4.0);
				double tdPerAttemptScore = ClampRatingScore((((double)rec.TDPasses / (double)rec.PassAttempts) - 0.0) / 0.05);
				double intPerAttemptScore = ClampRatingScore((0.095 - ((double)rec.INTThrown / (double)rec.PassAttempts)) / 0.04);
				double ratingTotal = completionScore + yardsPerAttemptScore + tdPerAttemptScore + intPerAttemptScore;
				ratingTotal *= 100.0;
				ratingTotal /= 6.0;
				output += rec.PassCompletions + "-" + rec.PassAttempts + 
					" (" + completionPercent + "%, " + rec.BadPasses + " bad, " + rec.SuccessfulPasses + " suc) " +
					rec.PassYards + " yds pass (" + averagePerAttempt.ToString("F2") + " ypa), " +
					rec.TDPasses + " TD, " + rec.INTThrown + " INT, " + rec.TimesSacked + " sacked, " +
					ratingTotal.ToString("F1") + " rat";
			}
			if (rec.RushAttempts > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				double rushAverage = (double)rec.RushingYards / (double)rec.RushAttempts;
				output += rec.RushAttempts + "-" + rec.RushingYards + " yds rush (" + rushAverage.ToString("F2") +
					" ypc), " + rec.RushTD + " TD, " + rec.RunsForLoss + " forloss, " + rec.SuccessfulRuns + " suc";
			}
			if (rec.Catches > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				double catchAverage = (double)rec.ReceivingYards / (double)rec.Catches;
				double yardsPerTarget = (double)rec.ReceivingYards / (double)rec.PassTargets;
				output += rec.Catches + "-" + rec.ReceivingYards + " yds catch (" + catchAverage.ToString("F2") +
					" ypc, " + rec.YardsAfterCatch + " yac, " + rec.PassTargets + " tgt, " + yardsPerTarget.ToString("F2") + " ypt), " +
					rec.ReceivingTDs + " TD, " + rec.PassDrops + " drp, " + rec.BadPassesCaught + " bdC";
			}
			if (rec.KeyRunBlock > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}

				int blockPercent = (rec.KeyRunBlock * 100) / rec.KeyRunBlockOpportunites;
				output += rec.KeyRunBlock + " KRB, " + rec.KeyRunBlockOpportunites + " KRO (" + blockPercent + "%)";
			}
			if (rec.PancakeBlocks > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.PancakeBlocks + " PNK";
			}
			if (rec.Fumbles > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.Fumbles + " fumb (" + rec.FumblesLost + " lost)";
			}
			if (rec.SacksAllowed > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.SacksAllowed + " sack allowed";
			}
			if (rec.TwoPointConversions > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.TwoPointConversions + " 2Pt";
			}

			return output;
		}

		private string GenerateDefensiveString(DataReader.LeagueData.PlayerGameStatsRecord rec)
		{
			string output = rec.Tackles + " tkl";

			if (rec.TacklesForLoss > 0)
			{
				output += " (" + rec.TacklesForLoss + " fl)";
			}
			if (rec.Sacks > 0)
			{
				output += ", " + (double)(rec.Sacks / 10.0) + " sacks";
			}
			if (rec.INTs > 0)
			{
				output += ", " + rec.INTs + " INT (" + rec.INTReturnYards + " yds, " + rec.INTReturnTDs + " TD)";
			}
			if (rec.ForcedFumbles > 0)
			{
				output += ", " + rec.ForcedFumbles + " ff";
			}
			if (rec.FumbleRecoveries > 0)
			{
				output += ", " + rec.FumbleRecoveries + " fr";
			}
			if (rec.MiscTD > 0)
			{
				output += ", " + rec.MiscTD + " miscTD";
			}
			if (rec.PassesBlocked > 0)
			{
				output += ", " + rec.PassesBlocked + " passblk";
			}
			if (rec.HasKeyCoverage > 0)
			{
				output += ", " + rec.HasKeyCoverage + " kcvr (" + rec.ThrownAt + " thrat, " + rec.PassesCaught + " ct, " 
					+ rec.PassesDefended + " def, " + rec.ReceptionsOf20YardsPlusGivenUp + " 20+ct)";
			}
			if (rec.QBHurries > 0)
			{
				output += ", " + rec.QBHurries + " hurries";
			}
			if (rec.QBKnockdowns > 0)
			{
				output += ", " + rec.QBKnockdowns + " QBKnock";
			}

			return output;
		}

		private string GenerateSpecialTeamsString(DataReader.LeagueData.PlayerGameStatsRecord rec)
		{
			string output = "";
			if (rec.FGAttempted > 0)
			{
				int fgPercent = (rec.FGMade * 100) / rec.FGAttempted;
				output += rec.FGMade + "-" + rec.FGAttempted + " FG (" + fgPercent + "%, " +
					rec.FG40PlusMade + " 40+, " + rec.FG50PlusMade + " 50+)";
			}
			if (rec.PATAttempted > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				int patPercent = (rec.PAT * 100) / rec.PATAttempted;
				output += rec.PAT + "-" + rec.PATAttempted + " PAT (" + patPercent + "%)";
			}
			if (rec.Kickoffs > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				double kickoffAverage = (double)rec.KickoffYards / (double)rec.Kickoffs;
				double fpAverage = (double)rec.TotalFieldPositionAfterKickoff / (double)rec.Kickoffs;
				output += rec.Kickoffs + " ko (" + rec.KickoffTouchbacks + " tb, "
					+ kickoffAverage.ToString("F2") + " avg, " + fpAverage.ToString("F0") + " fpos)";
			}
			if (rec.PuntIn20 > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.PuntIn20 + " in20";
			}
			if (rec.PuntNetYards > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.PuntNetYards + " puntnet";
			}
			if (rec.KickReturnYards > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.KickReturnYards + " kickretyrds (" + rec.KickReturnTDs + " TD)";
			}
			if (rec.PuntReturnTDs > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.PuntReturnYards + " puntretyrds (" + rec.PuntReturnTDs + " TD)";
			}
			if (rec.SpecialTeamsTackles > 0)
			{
				if (output.Length > 0)
				{
					output += ", ";
				}
				output += rec.SpecialTeamsTackles + " specTkl";
			}

			return output;
		}

		private class CareerOrgIndexComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				PlayerCareerData p1 = (PlayerCareerData)x;
				PlayerCareerData p2 = (PlayerCareerData)y;

				return p1.OriginalIndex.CompareTo(p2.OriginalIndex);
			}
		}

		private class CareerQBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Pass];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Pass];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerFBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerTEComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerWRComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerOLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Block];

				return score1.CompareTo(score2);
			}
		}

		private class CareerDLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PRush];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PRush];

				return score1.CompareTo(score2);
			}
		}

		private class CareerLBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerCBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerSComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Punt];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Punt];

				return score1.CompareTo(score2);
			}
		}

		private class CareerKComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Kick];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.Kick];

				return score1.CompareTo(score2);
			}
		}

		private class CareerSTComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.SpecT];
				double score2 = p2.RegularSeasonStats.CategoryScore[(int)PlayerStatCategory.SpecT];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsQBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Pass];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Pass];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsRBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsFBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsTEComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsWRComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsOLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Block];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Block];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsDLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PRush];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PRush];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsLBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsCBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsSComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsPComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Punt];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Punt];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsKComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Kick];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.Kick];

				return score1.CompareTo(score2);
			}
		}

		private class CareerPlayoffsSTComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.SpecT];
				double score2 = p2.PlayoffsStats.CategoryScore[(int)PlayerStatCategory.SpecT];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieQBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Pass];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Pass];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieRBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieFBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieTEComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieWRComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieOLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Block];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Block];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieDLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.PRush];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.PRush];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieLBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieCBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieSComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.RookieStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.RookieStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookiePComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Punt];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Punt];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieKComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.Kick];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.Kick];

				return score1.CompareTo(score2);
			}
		}

		private class CareerRookieSTComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerCareerData p1 = (PlayerCareerData)y;
				PlayerCareerData p2 = (PlayerCareerData)x;

				double score1 = p1.RookieStats.CategoryScore[(int)PlayerStatCategory.SpecT];
				double score2 = p2.RookieStats.CategoryScore[(int)PlayerStatCategory.SpecT];

				return score1.CompareTo(score2);
			}
		}

		private class WeekOffenseComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.WeekOffenseScore.CompareTo(p2.WeekOffenseScore);
			}
		}

		private class WeekDefenseComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.WeekDefenseScore.CompareTo(p2.WeekDefenseScore);
			}
		}

		private class WeekSpecialTeamsComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.WeekSpecialTeamsScore.CompareTo(p2.WeekSpecialTeamsScore);
			}
		}

		private class SeasonOffenseComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.SeasonOffenseScore.CompareTo(p2.SeasonOffenseScore);
			}
		}

		private class SeasonDefenseComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.SeasonDefenseScore.CompareTo(p2.SeasonDefenseScore);
			}
		}

		private class SeasonSpecialTeamsComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.SeasonSpecialTeamsScore.CompareTo(p2.SeasonSpecialTeamsScore);
			}
		}

		private class PlayoffsOffenseComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.PlayoffsOffenseScore.CompareTo(p2.PlayoffsOffenseScore);
			}
		}

		private class PlayoffsDefenseComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.PlayoffsDefenseScore.CompareTo(p2.PlayoffsDefenseScore);
			}
		}

		private class PlayoffsSpecialTeamsComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				return p1.PlayoffsSpecialTeamsScore.CompareTo(p2.PlayoffsSpecialTeamsScore);
			}
		}

		private class SeasonQBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Pass];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Pass];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonRBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonFBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Rush] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonTEComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Block] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonWRComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Recv] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscOff];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonOLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Block];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Block];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonDLComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.PRush];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.PRush];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonLBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.PRush] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonCBComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonSComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.MiscDef] +
					p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.PDef];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonPComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Punt];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Punt];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonKComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.Kick];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.Kick];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonSTComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				// Want a descending sort, so reverse
				PlayerSeasonData p1 = (PlayerSeasonData)y;
				PlayerSeasonData p2 = (PlayerSeasonData)x;

				double score1 = p1.SeasonStats.CategoryScore[(int)PlayerStatCategory.SpecT];
				double score2 = p2.SeasonStats.CategoryScore[(int)PlayerStatCategory.SpecT];

				return score1.CompareTo(score2);
			}
		}

		private class SeasonOrgIndexComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				PlayerSeasonData p1 = (PlayerSeasonData)x;
				PlayerSeasonData p2 = (PlayerSeasonData)y;

				return p1.OriginalIndex.CompareTo(p2.OriginalIndex);
			}
		}

		private Dictionary<string, string> mTeamImageMap;
		private Dictionary<string, string> mTeamLinkMap;
		private Dictionary<string, string> mPlayerLinkMap;
		private void InitializeTeamImageMap()
		{
			mTeamImageMap = new Dictionary<string, string>();
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "TeamImageMap.csv");
			if (System.IO.File.Exists(filename))
			{
				using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
				{
					while (!inFile.EndOfStream)
					{
						string curLine = inFile.ReadLine();
						string[] fields = DataReader.CSVHelper.ParseLine(curLine);

						mTeamImageMap[fields[0]] = fields[1];
					}
					inFile.Close();
				}
			}

			mPlayerLinkMap = new Dictionary<string, string>();
			mTeamLinkMap = new Dictionary<string, string>();
			filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "LeagueSiteLinks.csv");
			if (System.IO.File.Exists(filename))
			{
				using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
				{
					while (!inFile.EndOfStream)
					{
						string curLine = inFile.ReadLine();
						string[] fields = DataReader.CSVHelper.ParseLine(curLine);

						mPlayerLinkMap[fields[0]] = fields[1];
						mTeamLinkMap[fields[0]] = fields[2];
					}
					inFile.Close();
				}
			}
		}

		private Dictionary<string, string> mPositionGroupMap;
		private void InitializePositionGroupMap()
		{
			mPositionGroupMap = new Dictionary<string, string>();
			mPositionGroupMap["QB"] = "QB";
			mPositionGroupMap["RB"] = "RB";
			mPositionGroupMap["FB"] = "FB";
			mPositionGroupMap["TE"] = "TE";
			mPositionGroupMap["FL"] = "WR";
			mPositionGroupMap["SE"] = "WR";
			mPositionGroupMap["C"] = "C";
			mPositionGroupMap["LG"] = "G";
			mPositionGroupMap["RG"] = "G";
			mPositionGroupMap["LT"] = "T";
			mPositionGroupMap["RT"] = "T";
			mPositionGroupMap["P"] = "P";
			mPositionGroupMap["K"] = "K";
			mPositionGroupMap["LDE"] = "DE";
			mPositionGroupMap["RDE"] = "DE";
			mPositionGroupMap["LDT"] = "DT";
			mPositionGroupMap["NT"] = "DT";
			mPositionGroupMap["RDT"] = "DT";
			mPositionGroupMap["WILB"] = "ILB";
			mPositionGroupMap["MLB"] = "ILB";
			mPositionGroupMap["SILB"] = "ILB";
			mPositionGroupMap["SLB"] = "OLB";
			mPositionGroupMap["WLB"] = "OLB";
			mPositionGroupMap["RCB"] = "CB";
			mPositionGroupMap["LCB"] = "CB";
			mPositionGroupMap["SS"] = "S";
			mPositionGroupMap["FS"] = "S";
		}

		private class StatWeight
		{
			public string FieldName;
			public string[] Category;
			public double[] Weight;
		}
		private List<StatWeight> mStatWeights;
		private Dictionary<string, int> mPositionToIndexMap;
		private void InitializeStatWeights()
		{
			mPositionToIndexMap = new Dictionary<string, int>();
			mPositionToIndexMap["QB"] = 0;
			mPositionToIndexMap["RB"] = 1;
			mPositionToIndexMap["FB"] = 2;
			mPositionToIndexMap["TE"] = 3;
			mPositionToIndexMap["FL"] = 4;
			mPositionToIndexMap["SE"] = 4;
			mPositionToIndexMap["C"] = 5;
			mPositionToIndexMap["LG"] = 6;
			mPositionToIndexMap["RG"] = 6;
			mPositionToIndexMap["LT"] = 7;
			mPositionToIndexMap["RT"] = 7;
			mPositionToIndexMap["P"] = 8;
			mPositionToIndexMap["K"] = 9;
			mPositionToIndexMap["LDE"] = 10;
			mPositionToIndexMap["RDE"] = 10;
			mPositionToIndexMap["LDT"] = 11;
			mPositionToIndexMap["NT"] = 11;
			mPositionToIndexMap["RDT"] = 11;
			mPositionToIndexMap["WILB"] = 12;
			mPositionToIndexMap["MLB"] = 12;
			mPositionToIndexMap["SILB"] = 12;
			mPositionToIndexMap["SLB"] = 13;
			mPositionToIndexMap["WLB"] = 13;
			mPositionToIndexMap["RCB"] = 14;
			mPositionToIndexMap["LCB"] = 14;
			mPositionToIndexMap["SS"] = 15;
			mPositionToIndexMap["FS"] = 15;

			mStatWeights = new List<StatWeight>();
			string filename = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "ExtenderWeights.csv");
			if (!System.IO.File.Exists(filename))
			{
				filename = "ExtenderWeights.csv";
			}

			try
			{
				if (System.IO.File.Exists(filename))
				{
					using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
					{
						// Skip the header line
						inFile.ReadLine();
						// Read the file
						while (!inFile.EndOfStream)
						{
							string curLine = inFile.ReadLine();
							string[] fields = DataReader.CSVHelper.ParseLine(curLine);

							StatWeight newWeight = new StatWeight();
							newWeight.FieldName = fields[0];
							int count = (fields.Length - 1) / 2;
							newWeight.Category = new string[count];
							newWeight.Weight = new double[count];
							int curField = 1;
							for (int posGroup = 0; posGroup < count; ++posGroup)
							{
								newWeight.Category[posGroup] = fields[curField++];
								newWeight.Weight[posGroup] = Double.Parse(fields[curField++]);
							}
							mStatWeights.Add(newWeight);
						}
						inFile.Close();
					}
				}
			}
			catch (System.Exception e)
			{
				MessageBox.Show(e.ToString() + Environment.NewLine + e.InnerException.ToString() + Environment.NewLine,
					"Error reading ExtenderWeights.csv", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
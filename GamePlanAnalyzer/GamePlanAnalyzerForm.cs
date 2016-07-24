using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DataReader;

namespace GamePlanAnalyzer
{
	public partial class GamePlanAnalyzerForm : Form
	{
		delegate void DisplayPageCallback(string pageFilename, WebBrowser browserWindow);
		delegate void AddStatusTextCallback(string text);
		delegate void WorkCompletedCallback();

		private UniverseData mUniverseData;
		private LeagueData mLeagueData;

		private Cursor mOldCursor = null;

		public GamePlanAnalyzerForm()
		{
			InitializeComponent();

			try
			{
				Text += " v" + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
			}
			catch
			{
				Text += " DEBUG";
			}

			mUniverseData = new UniverseData();

			buttonAnalyze.Enabled = false;

			comboBoxLeagues.Items.Add("(Select League)");
			for (int i = 0; i < mUniverseData.SavedGames.Count; i++)
			{
				comboBoxLeagues.Items.Add(mUniverseData.SavedGames[i].GameName);
			}
			if (mUniverseData.SavedGames.Count > 0)
			{
				comboBoxLeagues.SelectedIndex = 0;
			}
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
				buttonAnalyze.Enabled = true;
			}
		}

		private void OnFileRead(string fileName)
		{
			AddStatusString("Reading file " + fileName + System.Environment.NewLine);
		}

		private void comboBoxLeagues_SelectedIndexChanged(object sender, EventArgs e)
		{
			int savedGameIndex = comboBoxLeagues.SelectedIndex - 1;
			if (savedGameIndex >= 0 && savedGameIndex < mUniverseData.SavedGames.Count)
			{
				// Read the league file to get correct cities.
				Cursor oldCusor = Cursor;
				Cursor = Cursors.WaitCursor;
				bool readGameLogs = false;
				AddStatusString("Reading league data for team list...");
				mLeagueData = new LeagueData(mUniverseData.SavedGames[savedGameIndex].PathPrefix, mUniverseData, mStartingSeason, null, readGameLogs);
				AddStatusString("Ready to pick Team");
				Cursor = oldCusor;

				comboBoxTeams.Items.Clear();
				for (int i = 0; i < mLeagueData.TeamInformationRecords.Length; ++i)
				{
					comboBoxTeams.Items.Add(mLeagueData.TeamInformationRecords[i].CityName);
				}
				comboBoxTeams.Items.Add("ALL TEAMS");

				buttonAnalyze.Enabled = true;
			}
		}

		private int mSavedGameIndex = -1;
		private int mStartingSeason = LeagueData.LoadCurrentSeasonOnly;
		private int mSelectedTeam = -1;
		private string mSelectedTeamName = "";

		private void buttonAnalyze_Click(object sender, EventArgs e)
		{
			mSavedGameIndex = comboBoxLeagues.SelectedIndex - 1;
			if (mSavedGameIndex >= 0 && mSavedGameIndex < mUniverseData.SavedGames.Count)
			{
				mSelectedTeam = comboBoxTeams.SelectedIndex;
				mSelectedTeamName = comboBoxTeams.Text;
				buttonAnalyze.Enabled = false;
				mOldCursor = Cursor;
				Cursor = Cursors.WaitCursor;

				Int32.TryParse(textBoxStartingSeason.Text, out mStartingSeason);

				System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateThreadTask));
				updateThread.IsBackground = true;
				updateThread.Start();
			}
		}

		private void UpdateThreadTask()
		{
			DateTime startTime = DateTime.Now;

			AddStatusString("Loading game data..." + System.Environment.NewLine);

			bool readGameLogs = true;

			mLeagueData = new LeagueData(mUniverseData.SavedGames[mSavedGameIndex].PathPrefix, mUniverseData, mStartingSeason, OnFileRead, readGameLogs);

			AddStatusString("Game data loaded, analyzing stats..." + System.Environment.NewLine);

			AnalyzeStats();

			DateTime endTime = DateTime.Now;
			TimeSpan totalTime = endTime - startTime;
			int minutes = totalTime.Minutes;
			int seconds = totalTime.Seconds;
			AddStatusString("Finished! Run Time: " + minutes.ToString() + "min " + seconds.ToString() + "sec" + System.Environment.NewLine);

			WorkCompleted();
		}

		private class RushingStats
		{
			public ulong Attempts = 0;
			public long Yards = 0;
			public ulong SuccessfulRuns = 0;
			public ulong StuffedRuns = 0;
			public ulong Fumbles = 0;
			public ulong LostFumbles = 0;
			public ulong Touchdowns = 0;

			public void AddToMe(RushingStats otherStats)
			{
				Attempts += otherStats.Attempts;
				Yards += otherStats.Yards;
				SuccessfulRuns += otherStats.SuccessfulRuns;
				StuffedRuns += otherStats.StuffedRuns;
				Fumbles += otherStats.Fumbles;
				LostFumbles += otherStats.LostFumbles;
				Touchdowns += otherStats.Touchdowns;
			}
		}

		private class PassingStats
		{
			public ulong DropBacks = 0;
			public ulong Attempts = 0;
			public ulong Completions = 0;
			public long TotalYards = 0;
			public long CompletionYards = 0;
			public long YardsAfterCompletion = 0;
			public ulong SuccessfulPasses = 0;
			public ulong FirstDowns = 0;
			public ulong Sacked = 0;
			public long SackedYards = 0;
			public ulong Scrambled = 0;
			public long ScrambleYards = 0;
			public ulong Interceptions = 0;
			public ulong Touchdowns = 0;
			public ulong ShortPasses = 0;
			public ulong SuccessfulShortPasses = 0;
			public ulong LongPasses = 0;
			public ulong SuccessfulLongPasses = 0;

			public void AddToMe(PassingStats otherStats)
			{
				DropBacks += otherStats.DropBacks;
				Attempts += otherStats.Attempts;
				Completions += otherStats.Completions;
				TotalYards += otherStats.TotalYards;
				CompletionYards += otherStats.CompletionYards;
				YardsAfterCompletion += otherStats.YardsAfterCompletion;
				SuccessfulPasses += otherStats.SuccessfulPasses;
				FirstDowns += otherStats.FirstDowns;
				Sacked += otherStats.Sacked;
				SackedYards += otherStats.SackedYards;
				Scrambled += otherStats.Scrambled;
				ScrambleYards += otherStats.ScrambleYards;
				Interceptions += otherStats.Interceptions;
				Touchdowns += otherStats.Touchdowns;
				ShortPasses += otherStats.ShortPasses;
				SuccessfulShortPasses += otherStats.SuccessfulShortPasses;
				LongPasses += otherStats.LongPasses;
				SuccessfulLongPasses += otherStats.SuccessfulLongPasses;
			}
		}

		private PassingStats mAllPassingStats;
		private PassingStats[] mPassingStatsVsDefensePlayCall;
		private PassingStats[] mPassingStatsVsDoubleCoverage;
		private PassingStats[] mPassingStatsVsDefensiveCoverage;
		private PassingStats[] mPassingStatsVsDefenseFamiliar;
		private PassingStats[,] mPassingStatsVsDoubleCoverageAndPlayCall;
		private PassingStats[,] mPassingStatsVsDoubleCoverageAndCoverage;
		private PassingStats[] mPassingStatsAtDistance;
		private PassingStats[,] mPassingStatsAtDistanceVsPlayCall;
		private PassingStats[,] mPassingStatsAtDistanceVsCoverage;
		private PassingStats[] mPassingStatsFromOffensiveFormation;
		private PassingStats[,] mPassingStatsFromOffensiveFormationVsPlayCall;
		private PassingStats[,] mPassingStatsAtDistanceFromFormation;
		private PassingStats[, ,] mPassingStatsAtDistanceFromFormationVsCoverage;

		private enum DoubleCoverageType
		{
			None = 0,
			Avoid,
			Into,
			Count
		}

		private enum DefensivePlayCall
		{
			RunAggressive = 0,
			RunNormal,
			PassNormal,
			PassAggressive,
			Count
		}

		private RushingStats mAllRushingStats;
		private RushingStats[] mRushingStatsVsDefensePlayCall;
		private RushingStats[] mRushingStatsVsDefensiveCoverage;
		private RushingStats[] mRushingStatsVsDefenseFamiliar;
		private RushingStats[] mRushingStatsFromOffensiveFormation;
		private RushingStats[,] mRushingStatsFromOffensiveFormationVsPlayCall;
		private RushingStats[,] mRushingStatsFromOffensiveFormationVsCoverage;
		private RushingStats[] mRushingStatsInDirection;
		private RushingStats[,] mRushingStatsInDrectionFromFormation;

		private class OffenseStats
		{
			public ulong Plays = 0;
			public ulong RushingAttempts = 0;
			public long RushingYards = 0;
			public ulong SuccessfulRushes = 0;
			public ulong StuffedRushes = 0;
			public ulong DropBacks = 0;
			public ulong PassAttempts = 0;
			public ulong PassCompletions = 0;
			public ulong Interceptions = 0;
			public long PassingYards = 0;
			public ulong Sacks = 0;
			public ulong SuccessfulPasses = 0;
			public ulong ShortPasses = 0;
			public ulong SuccessfulShortPasses = 0;
			public ulong LongPasses = 0;
			public ulong SuccessfulLongPasses = 0;

			public void AddToMe(RushingStats stats)
			{
				Plays += stats.Attempts;
				RushingAttempts += stats.Attempts;
				RushingYards += stats.Yards;
				SuccessfulRushes += stats.SuccessfulRuns;
				StuffedRushes += stats.StuffedRuns;
			}

			public void AddToMe(PassingStats stats)
			{
				Plays += stats.DropBacks;
				DropBacks += stats.DropBacks;
				PassAttempts += stats.Attempts;
				PassCompletions += stats.Completions;
				Interceptions += stats.Interceptions;
				PassingYards += stats.TotalYards;
				Sacks += stats.Sacked;
				SuccessfulPasses += stats.SuccessfulPasses;
				ShortPasses += stats.ShortPasses;
				SuccessfulShortPasses += stats.SuccessfulShortPasses;
				LongPasses += stats.LongPasses;
				SuccessfulLongPasses += stats.SuccessfulLongPasses;
			}
		}

		private const int NumberOfToGoDistances = 8;
		static private int[] MaxYardsToGo = { 1, 2, 3, 5, 7, 10, 16, 1000 };
		static private string[] MaxYardsToGoStrings = { "1", "2", "3", "4-5", "6-7", "8-10", "11-16", "17+" };
		private const int NumberOfYardBands = 4;
		static private int[] MaxFieldPosition = { 6, 49, 79, 1000 };
		static private string[] OffenseYardBandStrings = { "Own 1-6", "Own 7-49", "Opp 50-21", "Opp 20-1" };
		static private string[] DefenseYardBandStrings = { "Opp 1-6", "Opp 7-49", "Own 50-21", "Own 20-1" };
		private const int NumberOfDowns = 4;
		private const int NumberOfBlitzers = 4;

		private OffenseStats mAllOffensivePlays;
		private OffenseStats[,] mOffenseByDownDistance;
		private OffenseStats[,] mOffenseByDownDistanceFirstHalf;
		private OffenseStats[] mOffenseByFormation;
		private OffenseStats[] mOffenseByFieldPosition;
		private OffenseStats[] mOffenseRunsByDirection;
		private OffenseStats[] mOffensePassesByDistance;

		private OffenseStats mAllDefensivePlays;
		private OffenseStats[,] mDefenseByDownDistance;
		private OffenseStats[] mDefenseByCall;
		private OffenseStats[] mDefenseByCoverage;
		private OffenseStats[] mDefenseByFieldPosition;
		private OffenseStats[,] mDefenseByCallAndPersonnel;
		private OffenseStats[,] mDefenseByCallAndBlitzers;

		private OffenseStats[] mOffenseInWeather;

		private LeagueData.GameLog mCurrentGameLog;
		private bool mGameInvolvesSelectedTeam;
		private bool mSelectedTeamIsHomeTeam;
		private bool mSelectedTeamIsAwayTeam;

		private class InjuryStats
		{
			public ulong Plays = 0;
			public ulong Injuries = 0;

			public void AddToMe(InjuryStats stats)
			{
				Plays += stats.Plays;
				Injuries += stats.Injuries;
			}
		}

		private InjuryStats mAllInjuries;
		private InjuryStats mSpecialTeamsInjuries;
		private InjuryStats mNormalPlayInjuries;
		private InjuryStats[] mInjuriesInWeather;

		private class IntTDStats
		{
			public ulong Plays = 0;
			public ulong Ints = 0;
			public ulong IntTDs = 0;

			public void AddToMe(IntTDStats stats)
			{
				Plays += stats.Plays;
				Ints += stats.Ints;
				IntTDs += stats.IntTDs;
			}
		}
		private IntTDStats[] mIntTDsAtDistance;

		private int mPrecipitation;

		private void AnalyzeStats()
		{
			InitializeStatsArrays();

			foreach (LeagueData.GameWeekRecord weekRec in mLeagueData.AvailableGameWeeks)
			{
				if (weekRec.Week < 6 && !checkBoxAnalyzePreseason.Checked)
				{
					continue;
				}
				if (weekRec.Week > 22 && !checkBoxAnalyzePostseason.Checked)
				{
					continue;
				}

				foreach (LeagueData.GameLog gameLog in weekRec.GameLogs)
				{
					mCurrentGameLog = gameLog;
					mPrecipitation = mCurrentGameLog.Weather;

					mGameInvolvesSelectedTeam = false;
					mSelectedTeamIsHomeTeam = false;
					mSelectedTeamIsAwayTeam = false;

					if (mSelectedTeam >= mLeagueData.NumberOfTeams)
					{
						mGameInvolvesSelectedTeam = true;
						mSelectedTeamIsHomeTeam = true;
						mSelectedTeamIsAwayTeam = true;
					}
					else if (mSelectedTeam == mCurrentGameLog.AwayTeam.TeamIndex)
					{
						mGameInvolvesSelectedTeam = true;
						mSelectedTeamIsHomeTeam = false;
						mSelectedTeamIsAwayTeam = true;
					}
					else if (mSelectedTeam == mCurrentGameLog.HomeTeam.TeamIndex)
					{
						mGameInvolvesSelectedTeam = true;
						mSelectedTeamIsHomeTeam = true;
						mSelectedTeamIsAwayTeam = false;
					}

					foreach (LeagueData.GamePlay gamePlay in gameLog.Plays)
					{
						InjuryStats playInjuries = new InjuryStats();
						playInjuries.Plays = 1;
						if (gamePlay.InjuryType != 0)
						{
							playInjuries.Injuries = 1;
						}

						LeagueData.PlayType curPlayType = (LeagueData.PlayType)gamePlay.PlayType;
						if (curPlayType == LeagueData.PlayType.Pass || curPlayType == LeagueData.PlayType.Run)
						{
							mAllInjuries.AddToMe(playInjuries);
							mNormalPlayInjuries.AddToMe(playInjuries);
							mInjuriesInWeather[mPrecipitation].AddToMe(playInjuries);
						}
						else if (curPlayType == LeagueData.PlayType.Punt || curPlayType == LeagueData.PlayType.OnsideKick
							|| curPlayType == LeagueData.PlayType.Kickoff || curPlayType == LeagueData.PlayType.FG)
						{
							mAllInjuries.AddToMe(playInjuries);
							mSpecialTeamsInjuries.AddToMe(playInjuries);
							mInjuriesInWeather[mPrecipitation].AddToMe(playInjuries);
						}

						if (gamePlay.PlayType == (int)LeagueData.PlayType.Pass
							&& (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsPenaltyAccepted] == 0
								|| gamePlay.EffectOnPlay == 2
								)
							)
						{
							if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.PassDistance] != (int)LeagueData.PassDistance.Spike)
							{
								AnalyzePassPlay(gamePlay);
							}
						}
						else if (gamePlay.PlayType == (int)LeagueData.PlayType.Run
								&& (gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.IsPenaltyAccepted] == 0
									|| gamePlay.EffectOnPlay == 2
									)
								)
						{
							if (gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.RunDirection] != (int)LeagueData.RunDirection.KneelDown)
							{
								AnalyzeRunPlay(gamePlay);
							}
						}
					}
				}
			}

			WriteLeaguePassingStats();
			WriteLeagueRushingStats();
			WriteMiscellaneousStats();
			if (mSelectedTeam >= 0)
			{
				WriteTeamOffenseStats();
				WriteTeamDefenseStats();
				WriteOffensivePlaycalling();
			}
		}

		private void AnalyzeRunPlay(LeagueData.GamePlay gamePlay)
		{
			RushingStats playStats = new RushingStats();
			playStats.Attempts += 1;

			bool isLostFumble = false;
			if (gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.IsFumble] != 0)
			{
				playStats.Fumbles += 1;
				if (gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.FumbleRecoveryTeam] != gamePlay.Possession)
				{
					playStats.LostFumbles += 1;
					isLostFumble = true;
				}
			}

			if (!isLostFumble)
			{
				int yardsGained = gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.YardsGained];
				playStats.Yards += yardsGained;
				if (IsSuccessful(gamePlay, yardsGained))
				{
					playStats.SuccessfulRuns += 1;
				}
				else if (yardsGained <= 0)
				{
					playStats.StuffedRuns += 1;
				}
				if (gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.IsTouchdown] != 0)
				{
					playStats.Touchdowns += 1;
				}
			}

			LeagueData.OffensiveFormation offFormation = (LeagueData.OffensiveFormation)gamePlay.OffensiveFormation;

			DefensivePlayCall defPlayCall = DefensivePlayCall.Count;
			if (gamePlay.DefensiveRunAggressive != 0)
			{
				defPlayCall = DefensivePlayCall.RunAggressive;
			}
			else if (gamePlay.DefensivePassAggressive != 0)
			{
				defPlayCall = DefensivePlayCall.PassAggressive;
			}
			else if (gamePlay.DefensiveRunPass == (int)LeagueData.DefensiveRunPass.Run)
			{
				defPlayCall = DefensivePlayCall.RunNormal;
			}
			else
			{
				defPlayCall = DefensivePlayCall.PassNormal;
			}

			LeagueData.DefensiveCoverage defCoverage = (LeagueData.DefensiveCoverage)gamePlay.DefensiveCoverage;
			LeagueData.DefenseFamiliar defFamiliar = (LeagueData.DefenseFamiliar)gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.DefenseFamiliar];
			LeagueData.RunDirection runDir = (LeagueData.RunDirection)gamePlay.TypeSpecificData[(int)LeagueData.RunPlayFields.RunDirection];
			LeagueData.DefensivePersonnel defPersonnel = (LeagueData.DefensivePersonnel)gamePlay.DefensivePersonnel;

			mAllRushingStats.AddToMe(playStats);
			mRushingStatsVsDefensePlayCall[(int)defPlayCall].AddToMe(playStats);
			mRushingStatsVsDefensiveCoverage[(int)defCoverage].AddToMe(playStats);
			mRushingStatsVsDefenseFamiliar[(int)defFamiliar].AddToMe(playStats);
			mRushingStatsFromOffensiveFormation[(int)offFormation].AddToMe(playStats);
			mRushingStatsFromOffensiveFormationVsPlayCall[(int)offFormation, (int)defPlayCall].AddToMe(playStats);
			mRushingStatsFromOffensiveFormationVsCoverage[(int)offFormation, (int)defCoverage].AddToMe(playStats);
			mRushingStatsInDirection[(int)runDir].AddToMe(playStats);
			mRushingStatsInDrectionFromFormation[(int)runDir, (int)offFormation].AddToMe(playStats);

			if (mGameInvolvesSelectedTeam)
			{
				bool addToOffenseStats = false;
				bool addToDefenseStats = false;

				if (mSelectedTeamIsAwayTeam)
				{
					if (gamePlay.Possession == 1)
					{
						addToOffenseStats = true;
					}
					else
					{
						addToDefenseStats = true;
					}
				}

				if (mSelectedTeamIsHomeTeam)
				{
					if (gamePlay.Possession == 0)
					{
						addToOffenseStats = true;
					}
					else
					{
						addToDefenseStats = true;
					}
				}

				int yardBand = GetYardBand(gamePlay);
				int toGoIndex = GetToGoIndex(gamePlay);
				if (addToDefenseStats)
				{
					mAllDefensivePlays.AddToMe(playStats);
					mDefenseByCall[(int)defPlayCall].AddToMe(playStats);
					mDefenseByCoverage[(int)defCoverage].AddToMe(playStats);
					mDefenseByCallAndPersonnel[(int)defPlayCall,(int)defPersonnel].AddToMe(playStats);
					mDefenseByCallAndBlitzers[(int)defPlayCall,gamePlay.DefensiveBlitzCount].AddToMe(playStats);
					mDefenseByDownDistance[gamePlay.Down-1,toGoIndex].AddToMe(playStats);
					mDefenseByFieldPosition[yardBand].AddToMe(playStats);
				}

				if (addToOffenseStats)
				{
					mAllOffensivePlays.AddToMe(playStats);
					mOffenseInWeather[mPrecipitation].AddToMe(playStats);
					mOffenseByFormation[(int)offFormation].AddToMe(playStats);
					mOffenseRunsByDirection[(int)runDir].AddToMe(playStats);
					mOffenseByDownDistance[gamePlay.Down-1,toGoIndex].AddToMe(playStats);
					if (gamePlay.Quarter <= 2)
					{
						mOffenseByDownDistanceFirstHalf[gamePlay.Down - 1, toGoIndex].AddToMe(playStats);
					}
					mOffenseByFieldPosition[yardBand].AddToMe(playStats);
				}
			}
		}

		private void AnalyzePassPlay(LeagueData.GamePlay gamePlay)
		{
			PassingStats playStats = new PassingStats();
			playStats.DropBacks += 1;
			IntTDStats intTDStats = new IntTDStats();

			LeagueData.PassDistance passDistance = (LeagueData.PassDistance)gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.PassDistance];
			bool isShortPass = ((int)passDistance <= (int)LeagueData.PassDistance.P5To8) && passDistance != LeagueData.PassDistance.Count;
			bool isLongPass = ((int)passDistance > (int)LeagueData.PassDistance.P5To8) && passDistance != LeagueData.PassDistance.Count;

			if (isShortPass)
			{
				playStats.ShortPasses += 1;
			}
			if (isLongPass)
			{
				playStats.LongPasses += 1;
			}
			if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsQBSacked] != 0)
			{
				playStats.Sacked += 1;
				playStats.SackedYards += gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.QBSackYards];
			}
			else if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsQBScramble] != 0)
			{
				playStats.Scrambled += 1;
				playStats.ScrambleYards += gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.QBScrambleYards];
			}
			else
			{
				playStats.Attempts += 1;
				intTDStats.Plays += 1;
				if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsInterception] != 0)
				{
					playStats.Interceptions += 1;
					intTDStats.Ints += 1;
				}
				if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsTouchdown] != 0)
				{
					playStats.Touchdowns += 1;
				}
				if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsComplete] != 0)
				{
					playStats.Completions += 1;
					short totalYards = gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.YardsGained];
					short yardsAfterCatch = gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.YardsAfterCatch];
					playStats.CompletionYards += (totalYards - yardsAfterCatch);
					playStats.YardsAfterCompletion += yardsAfterCatch;
					playStats.TotalYards += totalYards;
					if (IsSuccessful(gamePlay, totalYards))
					{
						playStats.SuccessfulPasses += 1;
						if (isShortPass)
						{
							playStats.SuccessfulShortPasses += 1;
						}
						if (isLongPass)
						{
							playStats.SuccessfulLongPasses += 1;
						}
					}
				}
				if (gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.IsInterceptedForTD] != 0)
				{
					intTDStats.IntTDs += 1;
				}
			}

			mAllPassingStats.AddToMe(playStats);

			DoubleCoverageType playDoubleCoverage = DoubleCoverageType.None;
			short doubleCoverage = gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.DoubleCoverage];
			if (doubleCoverage == (int)LeagueData.DoubleCoverageType.ThrewAwayFrom)
			{
				playDoubleCoverage = DoubleCoverageType.Avoid;
			}
			else if (doubleCoverage == (int)LeagueData.DoubleCoverageType.Primary || doubleCoverage == (int)LeagueData.DoubleCoverageType.Secondary)
			{
				playDoubleCoverage = DoubleCoverageType.Into;
			}

			LeagueData.OffensiveFormation offFormation = (LeagueData.OffensiveFormation)gamePlay.OffensiveFormation;

			DefensivePlayCall defPlayCall = DefensivePlayCall.Count;
			if (gamePlay.DefensiveRunAggressive != 0)
			{
				defPlayCall = DefensivePlayCall.RunAggressive;
			}
			else if (gamePlay.DefensivePassAggressive != 0)
			{
				defPlayCall = DefensivePlayCall.PassAggressive;
			}
			else if (gamePlay.DefensiveRunPass == (int)LeagueData.DefensiveRunPass.Run)
			{
				defPlayCall = DefensivePlayCall.RunNormal;
			}
			else
			{
				defPlayCall = DefensivePlayCall.PassNormal;
			}

			LeagueData.DefensiveCoverage defCoverage = (LeagueData.DefensiveCoverage)gamePlay.DefensiveCoverage;
			LeagueData.DefenseFamiliar defFamiliar = (LeagueData.DefenseFamiliar)gamePlay.TypeSpecificData[(int)LeagueData.PassPlayFields.DefenseFamiliar];
			LeagueData.DefensivePersonnel defPersonnel = (LeagueData.DefensivePersonnel)gamePlay.DefensivePersonnel;

			mPassingStatsVsDefensePlayCall[(int)defPlayCall].AddToMe(playStats);
			mPassingStatsVsDefenseFamiliar[(int)defFamiliar].AddToMe(playStats);
			mPassingStatsVsDoubleCoverage[(int)playDoubleCoverage].AddToMe(playStats);
			mPassingStatsVsDefensiveCoverage[(int)defCoverage].AddToMe(playStats);
			mPassingStatsVsDoubleCoverageAndPlayCall[(int)playDoubleCoverage, (int)defPlayCall].AddToMe(playStats);
			mPassingStatsVsDoubleCoverageAndCoverage[(int)playDoubleCoverage, (int)defCoverage].AddToMe(playStats);
			mPassingStatsAtDistance[(int)passDistance].AddToMe(playStats);
			mPassingStatsAtDistanceVsPlayCall[(int)passDistance, (int)defPlayCall].AddToMe(playStats);
			mPassingStatsAtDistanceVsCoverage[(int)passDistance, (int)defCoverage].AddToMe(playStats);
			mPassingStatsFromOffensiveFormation[(int)offFormation].AddToMe(playStats);
			mPassingStatsFromOffensiveFormationVsPlayCall[(int)offFormation, (int)defPlayCall].AddToMe(playStats);
			mPassingStatsAtDistanceFromFormation[(int)passDistance, (int)offFormation].AddToMe(playStats);
			mPassingStatsAtDistanceFromFormationVsCoverage[(int)passDistance, (int)offFormation, (int)defCoverage].AddToMe(playStats);
			mIntTDsAtDistance[(int)passDistance].AddToMe(intTDStats);

			if (mGameInvolvesSelectedTeam)
			{
				bool addToOffenseStats = false;
				bool addToDefenseStats = false;

				if (mSelectedTeamIsAwayTeam)
				{
					if (gamePlay.Possession == 1)
					{
						addToOffenseStats = true;
					}
					else
					{
						addToDefenseStats = true;
					}
				}

				if (mSelectedTeamIsHomeTeam)
				{
					if (gamePlay.Possession == 0)
					{
						addToOffenseStats = true;
					}
					else
					{
						addToDefenseStats = true;
					}
				}

				int yardBand = GetYardBand(gamePlay);
				int toGoIndex = GetToGoIndex(gamePlay);
				if (addToDefenseStats)
				{
					mAllDefensivePlays.AddToMe(playStats);
					mDefenseByCall[(int)defPlayCall].AddToMe(playStats);
					mDefenseByCoverage[(int)defCoverage].AddToMe(playStats);
					mDefenseByCallAndPersonnel[(int)defPlayCall, (int)defPersonnel].AddToMe(playStats);
					mDefenseByCallAndBlitzers[(int)defPlayCall, gamePlay.DefensiveBlitzCount].AddToMe(playStats);
					mDefenseByDownDistance[gamePlay.Down - 1, toGoIndex].AddToMe(playStats);
					mDefenseByFieldPosition[yardBand].AddToMe(playStats);
				}

				if (addToOffenseStats)
				{
					mAllOffensivePlays.AddToMe(playStats);
					mOffenseInWeather[mPrecipitation].AddToMe(playStats);
					mOffenseByFormation[(int)offFormation].AddToMe(playStats);
					mOffenseByDownDistance[gamePlay.Down - 1, toGoIndex].AddToMe(playStats);
					if (gamePlay.Quarter <= 2)
					{
						mOffenseByDownDistanceFirstHalf[gamePlay.Down - 1, toGoIndex].AddToMe(playStats);
					}
					mOffenseByFieldPosition[yardBand].AddToMe(playStats);
					mOffensePassesByDistance[(int)passDistance].AddToMe(playStats);
				}
			}
		}

		private bool IsSuccessful(LeagueData.GamePlay gamePlay, int yardsGained)
		{
			int yardsNeeded = gamePlay.YardsToGo;
			if (gamePlay.Down == 1)
			{
				yardsNeeded = (gamePlay.YardsToGo * 4) / 10;
			}
			else if (gamePlay.Down == 2)
			{
				yardsNeeded = (gamePlay.YardsToGo * 6) / 10;
			}

			return (yardsGained >= yardsNeeded);
		}

		private int GetYardBand(LeagueData.GamePlay gamePlay)
		{
			int yardLine = gamePlay.YardLine;
			if (gamePlay.Possession == 0)
			{
				yardLine = 100 - yardLine;
			}
			int indexForPlay = 0;
			for (int bandIndex = 0; bandIndex < NumberOfYardBands; ++bandIndex)
			{
				if (yardLine <= MaxFieldPosition[bandIndex])
				{
					indexForPlay = bandIndex;
					break;
				}
			}

			return indexForPlay;
		}

		private int GetToGoIndex(LeagueData.GamePlay gamePlay)
		{
			int yardsToGo = gamePlay.YardsToGo;
			int indexForPlay = 0;
			for (int toGoIndex = 0; toGoIndex < NumberOfToGoDistances; ++toGoIndex)
			{
				if (yardsToGo <= MaxYardsToGo[toGoIndex])
				{
					indexForPlay = toGoIndex;
					break;
				}
			}

			return indexForPlay;
		}

		private void WritePlaycallingTendency(System.IO.StreamWriter outFile, OffenseStats stats, int downIndex)
		{
			double runPercent = stats.Plays > 0 ? ((double)stats.RushingAttempts / (double)stats.Plays) : 0.0;
			double shortPassPercent = stats.Plays > 0 ? ((double)stats.ShortPasses / (double)stats.Plays) : 0.0;
			double longPassPercent = stats.Plays > 0 ? ((double)stats.LongPasses / (double)stats.Plays) : 0.0;

			if (downIndex % 2 == 0)
			{
				outFile.Write("<TD align=\"right\" class=\"highlight\">" + runPercent.ToString("0.0%"));
				outFile.Write("<TD align=\"right\" class=\"highlight\">" + shortPassPercent.ToString("0.0%"));
				outFile.Write("<TD align=\"right\" class=\"highlight\">" + longPassPercent.ToString("0.0%"));
			}
			else
			{
				outFile.Write("<TD align=\"right\" class=\"normal\">" + runPercent.ToString("0.0%"));
				outFile.Write("<TD align=\"right\" class=\"normal\">" + shortPassPercent.ToString("0.0%"));
				outFile.Write("<TD align=\"right\" class=\"normal\">" + longPassPercent.ToString("0.0%"));
			}
		}

		private void WriteOffensivePlaycalling()
		{
			string outFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "OffensivePlaycalling.html");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(outFileName, false);

			outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
			outFile.WriteLine("<HTML>");
			outFile.WriteLine("\t<HEAD>");
			outFile.WriteLine("\t\t<TITLE>Team Offensive Playcalling</TITLE>");
			outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
			outFile.WriteLine("\t\t\tTD.highlight { color: black; background: yellow }");
			outFile.WriteLine("\t\t\tTD.normal { color: black; background: white }");
			outFile.WriteLine("\t\t</STYLE>");
			outFile.WriteLine("\t</HEAD>");
			outFile.WriteLine("\t<BODY>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows offensive playcalling tendencies for the selected team\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>" + mSelectedTeamName + " Offensive Playcalling</EM></CAPTION>");
			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH colspan=\"3\">First Down");
			outFile.Write("<TH colspan=\"3\">Second Down");
			outFile.Write("<TH colspan=\"3\">Third Down");
			outFile.Write("<TH colspan=\"3\">Fourth Down");
			outFile.WriteLine();

			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			for (int downHeader = 0; downHeader < NumberOfDowns; ++downHeader)
			{
				outFile.Write("<TH>Runs");
				outFile.Write("<TH>SPass");
				outFile.Write("<TH>LPass");
			}
			outFile.WriteLine();

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"13\">Playcalling Tendencies By Down & Distance");
			for (int yardsToGoIndex = 0; yardsToGoIndex < NumberOfToGoDistances; ++yardsToGoIndex)
			{
				outFile.Write("\t\t\t<TR><TD class=\"normal\">" + MaxYardsToGoStrings[yardsToGoIndex]);
				for (int downIndex = 0; downIndex < NumberOfDowns; ++downIndex)
				{
					WritePlaycallingTendency(outFile, mOffenseByDownDistance[downIndex, yardsToGoIndex], downIndex);
				}
				outFile.WriteLine();
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"13\">First Half Tendencies By Down & Distance");
			for (int yardsToGoIndex = 0; yardsToGoIndex < NumberOfToGoDistances; ++yardsToGoIndex)
			{
				outFile.Write("\t\t\t<TR><TD class=\"normal\">" + MaxYardsToGoStrings[yardsToGoIndex]);
				for (int downIndex = 0; downIndex < NumberOfDowns; ++downIndex)
				{
					WritePlaycallingTendency(outFile, mOffenseByDownDistanceFirstHalf[downIndex, yardsToGoIndex], downIndex);
				}
				outFile.WriteLine();
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows offensive playcalling tendencies for the selected team\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>" + mSelectedTeamName + " Offensive Playcalling</EM></CAPTION>");
			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH colspan=\"6\">First Down");
			outFile.Write("<TH colspan=\"6\">Second Down");
			outFile.Write("<TH colspan=\"6\">Third Down");
			outFile.Write("<TH colspan=\"6\">Fourth Down");
			outFile.WriteLine();

			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			for (int downHeader = 0; downHeader < NumberOfDowns; ++downHeader)
			{
				outFile.Write("<TH>Runs");
				outFile.Write("<TH>Succ");
				outFile.Write("<TH>SPass");
				outFile.Write("<TH>Succ");
				outFile.Write("<TH>LPass");
				outFile.Write("<TH>Succ");
			}
			outFile.WriteLine();

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Playcalling with Success By Down & Distance");
			for (int yardsToGoIndex = 0; yardsToGoIndex < NumberOfToGoDistances; ++yardsToGoIndex)
			{
				outFile.Write("\t\t\t<TR><TD class=\"normal\">" + MaxYardsToGoStrings[yardsToGoIndex]);
				for (int downIndex = 0; downIndex < NumberOfDowns; ++downIndex)
				{
					OffenseStats stats = mOffenseByDownDistance[downIndex, yardsToGoIndex];
					double runPercent = stats.Plays > 0 ? ((double)stats.RushingAttempts / (double)stats.Plays) : 0.0;
					double runSuccessPercent = stats.RushingAttempts > 0 ? ((double)stats.SuccessfulRushes / (double)stats.RushingAttempts) : 0.0;
					double shortPassPercent = stats.Plays > 0 ? ((double)stats.ShortPasses / (double)stats.Plays) : 0.0;
					double shortPassSuccessPercent = stats.ShortPasses > 0 ? ((double)stats.SuccessfulShortPasses / (double)stats.ShortPasses) : 0.0;
					double longPassPercent = stats.Plays > 0 ? ((double)stats.LongPasses / (double)stats.Plays) : 0.0;
					double longPassSuccessPercent = stats.LongPasses > 0 ? ((double)stats.SuccessfulLongPasses / (double)stats.LongPasses) : 0.0;

					outFile.Write("<TD align=\"right\" class=\"highlight\">" + runPercent.ToString("0.0%"));
					outFile.Write("<TD align=\"right\" class=\"normal\">" + runSuccessPercent.ToString("0.0%"));
					outFile.Write("<TD align=\"right\" class=\"highlight\">" + shortPassPercent.ToString("0.0%"));
					outFile.Write("<TD align=\"right\" class=\"normal\">" + shortPassSuccessPercent.ToString("0.0%"));
					outFile.Write("<TD align=\"right\" class=\"highlight\">" + longPassPercent.ToString("0.0%"));
					outFile.Write("<TD align=\"right\" class=\"normal\">" + longPassSuccessPercent.ToString("0.0%"));
				}
				outFile.WriteLine();
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.Close();
			DisplayPage(outFileName, webBrowserOffensivePlaycalling);
		}

		private void WriteTeamOffenseStats()
		{
			string outFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "TeamOffenseStats.html");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(outFileName, false);

			outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
			outFile.WriteLine("<HTML>");
			outFile.WriteLine("\t<HEAD>");
			outFile.WriteLine("\t\t<TITLE>Team Offense Statistics</TITLE>");
			outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
			outFile.WriteLine("\t\t\tTD.highlight { color: black; background: yellow }");
			outFile.WriteLine("\t\t\tTD.normal { color: black; background: white }");
			outFile.WriteLine("\t\t</STYLE>");
			outFile.WriteLine("\t</HEAD>");
			outFile.WriteLine("\t<BODY>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows offense statistics for the selected team\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>" + mSelectedTeamName + " Offense Stats</EM></CAPTION>");

			WriteTeamOffenseHeaders(outFile);

			WriteOffenseStatsLine(outFile, "All", mAllOffensivePlays, mAllOffensivePlays.Plays);

			int i,j;

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Offense In Weather");
			for (i = 0; i < mUniverseData.PrecipMap.Length; ++i)
			{
				WriteOffenseStatsLine(outFile, mUniverseData.PrecipMap[i], mOffenseInWeather[i], mAllOffensivePlays.Plays);
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Offense By Down & Distance");
			for (i=0;i<NumberOfDowns;++i)
			{
				for (j=0;j<NumberOfToGoDistances;++j)
				{
					WriteOffenseStatsLine(outFile, (i + 1).ToString() + " & " + MaxYardsToGoStrings[j], mOffenseByDownDistance[i, j], mAllOffensivePlays.Plays);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Offense from Offensive Formation");
			WriteTeamOffenseHeaders(outFile);
			foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
			{
				if (offForm != LeagueData.OffensiveFormation.Count)
				{
					WriteOffenseStatsLine(outFile, offForm.ToString(), mOffenseByFormation[(int)offForm], mAllOffensivePlays.Plays);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Offense By Field Position");
			WriteTeamOffenseHeaders(outFile);
			for (i = 0; i < NumberOfYardBands; ++i)
			{
				WriteOffenseStatsLine(outFile, OffenseYardBandStrings[i], mOffenseByFieldPosition[i], mAllOffensivePlays.Plays);
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Runs By Direction");
			WriteTeamOffenseHeaders(outFile);
			foreach (LeagueData.RunDirection runDir in Enum.GetValues(typeof(LeagueData.RunDirection)))
			{
				if (runDir != LeagueData.RunDirection.Count && runDir != LeagueData.RunDirection.KneelDown)
				{
					WriteOffenseStatsLine(outFile, runDir.ToString(), mOffenseRunsByDirection[(int)runDir], mAllOffensivePlays.RushingAttempts);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Pass By Distance");
			WriteTeamOffenseHeaders(outFile);
			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				{
					WriteOffenseStatsLine(outFile, passDist.ToString(), mOffensePassesByDistance[(int)passDist], mAllOffensivePlays.PassAttempts);
				}
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.Close();
			DisplayPage(outFileName, webBrowserTeamOffense);
		}

		private void WriteTeamDefenseStats()
		{
			string outFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "TeamDefenseStats.html");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(outFileName, false);

			outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
			outFile.WriteLine("<HTML>");
			outFile.WriteLine("\t<HEAD>");
			outFile.WriteLine("\t\t<TITLE>Team Defense Statistics</TITLE>");
			outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
			outFile.WriteLine("\t\t\tTD.highlight { color: black; background: yellow }");
			outFile.WriteLine("\t\t\tTD.normal { color: black; background: white }");
			outFile.WriteLine("\t\t</STYLE>");
			outFile.WriteLine("\t</HEAD>");
			outFile.WriteLine("\t<BODY>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows defense statistics for the selected team\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>" + mSelectedTeamName + " Defense Stats</EM></CAPTION>");
			outFile.Write("\t\t\t<TR>");

			WriteTeamOffenseHeaders(outFile);

			WriteOffenseStatsLine(outFile, "All", mAllDefensivePlays, mAllDefensivePlays.Plays);

			int i,j;

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Defense By Down & Distance");
			for (i=0;i<NumberOfDowns;++i)
			{
				for (j=0;j<NumberOfToGoDistances;++j)
				{
					WriteOffenseStatsLine(outFile, (i + 1).ToString() + " & " + MaxYardsToGoStrings[j], mDefenseByDownDistance[i, j], mAllDefensivePlays.Plays);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Defense By Field Position");
			WriteTeamOffenseHeaders(outFile);
			for (i = 0; i < NumberOfYardBands; ++i)
			{
				WriteOffenseStatsLine(outFile, DefenseYardBandStrings[i], mDefenseByFieldPosition[i], mAllDefensivePlays.Plays);
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Defense By Coverage");
			WriteTeamOffenseHeaders(outFile);
			foreach (LeagueData.DefensiveCoverage defCov in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
			{
				if (defCov != LeagueData.DefensiveCoverage.Count)
				{
					WriteOffenseStatsLine(outFile, defCov.ToString(), mDefenseByCoverage[(int)defCov], mAllDefensivePlays.Plays);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Defense By Play Call");
			WriteTeamOffenseHeaders(outFile);
			foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
			{
				if (defCall != DefensivePlayCall.Count)
				{
					WriteOffenseStatsLine(outFile, defCall.ToString(), mDefenseByCall[(int)defCall], mAllDefensivePlays.Plays);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Defense By Play Call and Personnel");
			WriteTeamOffenseHeaders(outFile);
			foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
			{
				if (defCall != DefensivePlayCall.Count)
				{
					foreach (LeagueData.DefensivePersonnel defPersonnel in Enum.GetValues(typeof(LeagueData.DefensivePersonnel)))
					{
						if (defPersonnel != LeagueData.DefensivePersonnel.Count)
						{
							WriteOffenseStatsLine(outFile, defCall.ToString() + " & " + defPersonnel.ToString(), mDefenseByCallAndPersonnel[(int)defCall, (int)defPersonnel], mAllDefensivePlays.Plays);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"27\">Defense By Play Call and Blitzers");
			WriteTeamOffenseHeaders(outFile);
			foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
			{
				if (defCall != DefensivePlayCall.Count)
				{
					for (j=0;j<NumberOfBlitzers;++j)
					{
						WriteOffenseStatsLine(outFile, defCall.ToString() + " & " + j.ToString() + " bltzr", mDefenseByCallAndBlitzers[(int)defCall, j], mAllDefensivePlays.Plays);
					}
				}
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.Close();
			DisplayPage(outFileName, webBrowserTeamDefense);
		}

		private void WriteTeamOffenseHeaders(System.IO.StreamWriter outFile)
		{
			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH>Plays");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>YPP");
			outFile.Write("<TH>Succ");
			outFile.Write("<TH>Pct");

			outFile.Write("<TH>RushP");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>YPR");
			outFile.Write("<TH>Succ");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Stuf");
			outFile.Write("<TH>Pct");

			outFile.Write("<TH>DrpB");
			outFile.Write("<TH>Pass");
			outFile.Write("<TH>Cmp");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Int");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>YPA");
			outFile.Write("<TH>YPC");

			outFile.Write("<TH>Succ");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Sack");
			outFile.Write("<TH>Pct");

			outFile.WriteLine();
		}

		private void WriteOffenseStatsLine(System.IO.StreamWriter outFile, string caption, OffenseStats stats, ulong totalPlays)
		{
			ulong totalSuccesses = stats.SuccessfulRushes + stats.SuccessfulPasses;
			long totalYards = stats.RushingYards + stats.PassingYards;

			double playPercent = totalPlays > 0 ? ((double)stats.Plays / (double)totalPlays) : 0.0;
			double successPercent = stats.Plays > 0 ? ((double)totalSuccesses / (double)stats.Plays) : 0.0;
			double yardsPerPlay = stats.Plays > 0 ? ((double)(totalYards) / (double)stats.Plays) : 0.0;
			double rushingSuccessPercent = stats.RushingAttempts > 0 ? ((double)stats.SuccessfulRushes / (double)stats.RushingAttempts) : 0.0;
			double rushingStuffedPercent = stats.RushingAttempts > 0 ? ((double)stats.StuffedRushes / (double)stats.RushingAttempts) : 0.0;
			double yardsPerRush = stats.RushingAttempts > 0 ? ((double)stats.RushingYards / (double)stats.RushingAttempts) : 0.0;
			double passingSuccessPercent = stats.DropBacks > 0 ? ((double)stats.SuccessfulPasses / (double)stats.DropBacks) : 0.0;
			double completionPercent = stats.PassAttempts > 0 ? ((double)stats.PassCompletions / (double)stats.PassAttempts) : 0.0;
			double interceptionPercent = stats.PassAttempts > 0 ? ((double)stats.Interceptions / (double)stats.PassAttempts) : 0.0;
			double yardsPerPass = stats.PassAttempts > 0 ? ((double)stats.PassingYards / (double)stats.PassAttempts) : 0.0;
			double yardsPerCompletion = stats.PassCompletions > 0 ? ((double)stats.PassingYards / (double)stats.PassCompletions) : 0.0;
			double sackPercent = stats.DropBacks > 0 ? ((double)stats.Sacks / (double)stats.DropBacks) : 0.0;

			outFile.Write("\t\t\t<TR><TD class=\"highlight\">" + caption);
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Plays.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + playPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + totalYards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + yardsPerPlay.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + totalSuccesses.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + successPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.RushingAttempts.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.RushingYards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + yardsPerRush.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.SuccessfulRushes.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + rushingSuccessPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.StuffedRushes.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + rushingStuffedPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.DropBacks.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.PassAttempts.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.PassCompletions.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + completionPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Interceptions.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + interceptionPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.PassingYards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + yardsPerPass.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + yardsPerCompletion.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.SuccessfulPasses.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + passingSuccessPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Sacks.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + sackPercent.ToString("0.0%"));
			outFile.WriteLine();

		}

		private void WriteLeagueRushingStats()
		{
			string outFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "LeagueRushingStats.html");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(outFileName, false);

			outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
			outFile.WriteLine("<HTML>");
			outFile.WriteLine("\t<HEAD>");
			outFile.WriteLine("\t\t<TITLE>League Rushing Statistics</TITLE>");
			outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
			outFile.WriteLine("\t\t\tTD.highlight { color: black; background: yellow }");
			outFile.WriteLine("\t\t\tTD.normal { color: black; background: white }");
			outFile.WriteLine("\t\t</STYLE>");
			outFile.WriteLine("\t</HEAD>");
			outFile.WriteLine("\t<BODY>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows some rushing statistics\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>League Rushing Stats</EM></CAPTION>");

			WriteRushStatsHeader(outFile);

			WriteRushStatsLine(outFile, "All", mAllRushingStats);

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush Versus Defensive Familiarity");
			foreach (LeagueData.DefenseFamiliar defFam in Enum.GetValues(typeof(LeagueData.DefenseFamiliar)))
			{
				if (defFam != LeagueData.DefenseFamiliar.Count)
				{
					WriteRushStatsLine(outFile, defFam.ToString(), mRushingStatsVsDefenseFamiliar[(int)defFam]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush Versus Defensive Play Call");
			WriteRushStatsHeader(outFile);
			foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
			{
				if (defCall != DefensivePlayCall.Count)
				{
					WriteRushStatsLine(outFile, defCall.ToString(), mRushingStatsVsDefensePlayCall[(int)defCall]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush Versus Defensive Coverage");
			WriteRushStatsHeader(outFile);
			foreach (LeagueData.DefensiveCoverage defCov in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
			{
				if (defCov != LeagueData.DefensiveCoverage.Count)
				{
					WriteRushStatsLine(outFile, defCov.ToString(), mRushingStatsVsDefensiveCoverage[(int)defCov]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush from Offensive Formation");
			WriteRushStatsHeader(outFile);
			foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
			{
				if (offForm != LeagueData.OffensiveFormation.Count)
				{
					WriteRushStatsLine(outFile, offForm.ToString(), mRushingStatsFromOffensiveFormation[(int)offForm]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush In Direction");
			WriteRushStatsHeader(outFile);
			foreach (LeagueData.RunDirection runDir in Enum.GetValues(typeof(LeagueData.RunDirection)))
			{
				if (runDir != LeagueData.RunDirection.Count && runDir != LeagueData.RunDirection.KneelDown)
				{
					WriteRushStatsLine(outFile, runDir.ToString(), mRushingStatsInDirection[(int)runDir]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush from Offensive Formation Vs PlayCall");
			WriteRushStatsHeader(outFile);
			foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
			{
				if (offForm != LeagueData.OffensiveFormation.Count)
				{
					foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
					{
						if (defCall != DefensivePlayCall.Count)
						{
							WriteRushStatsLine(outFile, offForm.ToString() + " & " + defCall.ToString(), mRushingStatsFromOffensiveFormationVsPlayCall[(int)offForm, (int)defCall]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush from Offensive Formation Vs Coverage");
			WriteRushStatsHeader(outFile);
			foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
			{
				if (offForm != LeagueData.OffensiveFormation.Count)
				{
					foreach (LeagueData.DefensiveCoverage defCov in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
					{
						if (defCov != LeagueData.DefensiveCoverage.Count)
						{
							WriteRushStatsLine(outFile, offForm.ToString() + " & " + defCov.ToString(), mRushingStatsFromOffensiveFormationVsCoverage[(int)offForm, (int)defCov]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"14\">Rush In Direction From Formation");
			WriteRushStatsHeader(outFile);
			foreach (LeagueData.RunDirection runDir in Enum.GetValues(typeof(LeagueData.RunDirection)))
			{
				if (runDir != LeagueData.RunDirection.Count && runDir != LeagueData.RunDirection.KneelDown)
				{
					foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
					{
						if (offForm != LeagueData.OffensiveFormation.Count)
						{
							WriteRushStatsLine(outFile, runDir.ToString() + " & " + offForm.ToString(), mRushingStatsInDrectionFromFormation[(int)runDir, (int)offForm]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.Close();

			DisplayPage(outFileName, webBrowserLeagueRushingOffense);
		}

		private void WriteRushStatsHeader(System.IO.StreamWriter outFile)
		{
			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH>Att");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>YPA");
			outFile.Write("<TH>Succ");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Stuf");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Fmb");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>LFmb");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>TD");
			outFile.Write("<TH>Pct");
			outFile.WriteLine();
		}

		private void WriteRushStatsLine(System.IO.StreamWriter outFile, string caption, RushingStats stats)
		{
			double successPercent = stats.Attempts > 0 ? ((double)stats.SuccessfulRuns / (double)stats.Attempts) : 0.0;
			double stuffedPercent = stats.Attempts > 0 ? ((double)stats.StuffedRuns / (double)stats.Attempts) : 0.0;
			double fumblePercent = stats.Attempts > 0 ? ((double)stats.Fumbles / (double)stats.Attempts) : 0.0;
			double lostFumblePercent = stats.Attempts > 0 ? ((double)stats.LostFumbles / (double)stats.Attempts) : 0.0;
			double tdPercent = stats.Attempts > 0 ? ((double)stats.Touchdowns / (double)stats.Attempts) : 0.0;
			double yardsPerAttempt = stats.Attempts > 0 ? ((double)stats.Yards / (double)stats.Attempts) : 0.0;

			outFile.Write("\t\t\t<TR><TD class=\"highlight\">" + caption);
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Attempts.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Yards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + yardsPerAttempt.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.SuccessfulRuns.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + successPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.StuffedRuns.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + stuffedPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Fumbles.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + fumblePercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.LostFumbles.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + lostFumblePercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Touchdowns.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + tdPercent.ToString("0.0%"));
			outFile.WriteLine();
		}

		private void WriteLeaguePassingStats()
		{
			string outFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "LeaguePassingStats.html");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(outFileName, false);

			outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
			outFile.WriteLine("<HTML>");
			outFile.WriteLine("\t<HEAD>");
			outFile.WriteLine("\t\t<TITLE>League Passing Statistics</TITLE>");
			outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
			outFile.WriteLine("\t\t\tTD.highlight { color: black; background: yellow }");
			outFile.WriteLine("\t\t\tTD.normal { color: black; background: white }");
			outFile.WriteLine("\t\t</STYLE>");
			outFile.WriteLine("\t</HEAD>");
			outFile.WriteLine("\t<BODY>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows some passing statistics\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>Passing Stats</EM></CAPTION>");

			WritePassStatsHeader(outFile);

			WritePassStatsLine(outFile, "All", mAllPassingStats);

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass Versus Defensive Familiarity");
			foreach (LeagueData.DefenseFamiliar defFam in Enum.GetValues(typeof(LeagueData.DefenseFamiliar)))
			{
				if (defFam != LeagueData.DefenseFamiliar.Count)
				{
					WritePassStatsLine(outFile, defFam.ToString(), mPassingStatsVsDefenseFamiliar[(int)defFam]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass Versus Defensive Play Call");
			WritePassStatsHeader(outFile);
			foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
			{
				if (defCall != DefensivePlayCall.Count)
				{
					WritePassStatsLine(outFile, defCall.ToString(), mPassingStatsVsDefensePlayCall[(int)defCall]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass Versus Double Coverage");
			WritePassStatsHeader(outFile);
			PassingStats allDC = new PassingStats();
			allDC.AddToMe(mPassingStatsVsDoubleCoverage[(int)DoubleCoverageType.Into]);
			allDC.AddToMe(mPassingStatsVsDoubleCoverage[(int)DoubleCoverageType.Avoid]);
			WritePassStatsLine(outFile, "Normal", mPassingStatsVsDoubleCoverage[(int)DoubleCoverageType.None]);
			WritePassStatsLine(outFile, "Doubled", allDC);
			WritePassStatsLine(outFile, "Into", mPassingStatsVsDoubleCoverage[(int)DoubleCoverageType.Into]);
			WritePassStatsLine(outFile, "Avoid", mPassingStatsVsDoubleCoverage[(int)DoubleCoverageType.Avoid]);

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass Versus Double Coverage and Defensive Playcall");
			WritePassStatsHeader(outFile);
			foreach (DoubleCoverageType dblCov in Enum.GetValues(typeof(DoubleCoverageType)))
			{
				if (dblCov != DoubleCoverageType.Count && dblCov != DoubleCoverageType.None)
				{
					foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
					{
						if (defCall != DefensivePlayCall.Count && defCall != DefensivePlayCall.RunNormal && defCall != DefensivePlayCall.RunAggressive)
						{
							WritePassStatsLine(outFile, dblCov.ToString() + " & " + defCall.ToString(), mPassingStatsVsDoubleCoverageAndPlayCall[(int)dblCov, (int)defCall]);
						}
					}

				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass Versus Defensive Coverage");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.DefensiveCoverage defCov in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
			{
				if (defCov != LeagueData.DefensiveCoverage.Count)
				{
					WritePassStatsLine(outFile, defCov.ToString(), mPassingStatsVsDefensiveCoverage[(int)defCov]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass Versus Double Coverage and Defensive Coverage");
			WritePassStatsHeader(outFile);
			foreach (DoubleCoverageType dblCov in Enum.GetValues(typeof(DoubleCoverageType)))
			{
				if (dblCov != DoubleCoverageType.Count && dblCov != DoubleCoverageType.None)
				{
					foreach (LeagueData.DefensiveCoverage defCall in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
					{
						if (defCall != LeagueData.DefensiveCoverage.Count)
						{
							WritePassStatsLine(outFile, dblCov.ToString() + " & " + defCall.ToString(), mPassingStatsVsDoubleCoverageAndCoverage[(int)dblCov, (int)defCall]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass At Distance");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				{
					WritePassStatsLine(outFile, passDist.ToString(), mPassingStatsAtDistance[(int)passDist]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass At Distance Vs PlayCall");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				{
					foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
					{
						if (defCall != DefensivePlayCall.Count)
						{
							WritePassStatsLine(outFile, passDist.ToString() + " & " + defCall.ToString(), mPassingStatsAtDistanceVsPlayCall[(int)passDist, (int)defCall]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass At Distance Vs Defensive Coverage");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				{
					foreach (LeagueData.DefensiveCoverage defCov in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
					{
						if (defCov != LeagueData.DefensiveCoverage.Count)
						{
							WritePassStatsLine(outFile, passDist.ToString() + " & " + defCov.ToString(), mPassingStatsAtDistanceVsCoverage[(int)passDist, (int)defCov]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass from Offensive Formation");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
			{
				if (offForm != LeagueData.OffensiveFormation.Count)
				{
					WritePassStatsLine(outFile, offForm.ToString(), mPassingStatsFromOffensiveFormation[(int)offForm]);
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass from Offensive Formation Vs PlayCall");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
			{
				if (offForm != LeagueData.OffensiveFormation.Count)
				{
					foreach (DefensivePlayCall defCall in Enum.GetValues(typeof(DefensivePlayCall)))
					{
						if (defCall != DefensivePlayCall.Count)
						{
							WritePassStatsLine(outFile, offForm.ToString() + " & " + defCall.ToString(), mPassingStatsFromOffensiveFormationVsPlayCall[(int)offForm, (int)defCall]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass At Distance From Formation");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				{
					foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
					{
						if (offForm != LeagueData.OffensiveFormation.Count)
						{
							WritePassStatsLine(outFile, passDist.ToString() + " & " + offForm.ToString(), mPassingStatsAtDistanceFromFormation[(int)passDist, (int)offForm]);
						}
					}
				}
			}

			outFile.WriteLine("\t\t\t<TR><TH colspan=\"25\">Pass At Distance From Formation Vs Coverage");
			WritePassStatsHeader(outFile);
			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				{
					foreach (LeagueData.OffensiveFormation offForm in Enum.GetValues(typeof(LeagueData.OffensiveFormation)))
					{
						if (offForm != LeagueData.OffensiveFormation.Count)
						{
							foreach (LeagueData.DefensiveCoverage defCov in Enum.GetValues(typeof(LeagueData.DefensiveCoverage)))
							{
								if (defCov != LeagueData.DefensiveCoverage.Count)
								{
									WritePassStatsLine(outFile, passDist.ToString() + " & " + offForm.ToString() + " & " + defCov.ToString(), mPassingStatsAtDistanceFromFormationVsCoverage[(int)passDist, (int)offForm, (int)defCov]);
								}
							}
						}
					}
				}
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.Close();

			DisplayPage(outFileName, webBrowserLeaguePassingOffense);
		}

		private void WritePassStatsHeader(System.IO.StreamWriter outFile)
		{
			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH>DrpBk");
			outFile.Write("<TH>Att");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Cmp");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>YPA");
			outFile.Write("<TH>YPC");
			outFile.Write("<TH>YAC");
			outFile.Write("<TH>YACPC");
			outFile.Write("<TH>Succ");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Sack");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>Avg");
			outFile.Write("<TH>Scrm");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Yds");
			outFile.Write("<TH>Avg");
			outFile.Write("<TH>INT");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>TD");
			outFile.Write("<TH>Pct");
			outFile.WriteLine();
		}

		private void WritePassStatsLine(System.IO.StreamWriter outFile, string caption, PassingStats stats)
		{
			double attemptsPercent = stats.DropBacks > 0 ? ((double)stats.Attempts / (double)stats.DropBacks) : 0.0;
			double passPercent = stats.Attempts > 0 ? ((double)stats.Completions / (double)stats.Attempts) : 0.0;
			double successPercent = stats.DropBacks > 0 ? ((double)stats.SuccessfulPasses / (double)stats.DropBacks) : 0.0;
			double sackPercent = stats.DropBacks > 0 ? ((double)stats.Sacked / (double)stats.DropBacks) : 0.0;
			double scramblePercent = stats.DropBacks > 0 ? ((double)stats.Scrambled / (double)stats.DropBacks) : 0.0;
			double yardsPerCompletion = stats.Completions > 0 ? ((double)stats.TotalYards / (double)stats.Completions) : 0.0;
			double yardsPerAttempt = stats.Attempts > 0 ? ((double)stats.TotalYards / (double)stats.Attempts) : 0.0;
			double yacPerCompletion = stats.Completions > 0 ? ((double)stats.YardsAfterCompletion / (double)stats.Completions) : 0.0;
			double thrownYardsPerCompletion = stats.Completions > 0 ? ((double)stats.CompletionYards / (double)stats.Completions) : 0.0;
			double thrownYardsPerAttempt = stats.Attempts > 0 ? ((double)stats.CompletionYards / (double)stats.Attempts) : 0.0;
			double receiverYardsPerCompletion = stats.Completions > 0 ? ((double)stats.YardsAfterCompletion / (double)stats.Completions) : 0.0;
			double receiverYardsPerAttempt = stats.Attempts > 0 ? ((double)stats.YardsAfterCompletion / (double)stats.Attempts) : 0.0;
			double yardsPerSack = stats.Sacked > 0 ? ((double)stats.SackedYards / (double)stats.Sacked) : 0.0;
			double yardsPerScramble = stats.Scrambled > 0 ? ((double)stats.ScrambleYards / (double)stats.Scrambled) : 0.0;
			double interceptionPercent = stats.Attempts > 0 ? ((double)stats.Interceptions / (double)stats.Attempts) : 0.0;
			double tdPercent = stats.Attempts > 0 ? ((double)stats.Touchdowns / (double)stats.Attempts) : 0.0;

			outFile.Write("\t\t\t<TR><TD class=\"highlight\">" + caption);
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.DropBacks.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Attempts.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + attemptsPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Completions.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + passPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.TotalYards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + yardsPerAttempt.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + yardsPerCompletion.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.YardsAfterCompletion.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + yacPerCompletion.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.SuccessfulPasses.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + successPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Sacked.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + sackPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.SackedYards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + yardsPerSack.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Scrambled.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + scramblePercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.ScrambleYards.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + yardsPerScramble.ToString("F2"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Interceptions.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + interceptionPercent.ToString("0.0%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Touchdowns.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + tdPercent.ToString("0.0%"));
			outFile.WriteLine();
		}

		private void WriteInjuryStatsLine(System.IO.StreamWriter outFile, string caption, InjuryStats stats)
		{
			double injuryPercent = stats.Plays > 0 ? ((double)stats.Injuries / (double)stats.Plays) : 0.0;

			outFile.Write("\t\t\t<TR><TD class=\"highlight\">" + caption);
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Plays.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Injuries.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + injuryPercent.ToString("0.0%"));
			outFile.WriteLine();
		}

		private void WriteIntTDLine(System.IO.StreamWriter outFile, string caption, IntTDStats stats)
		{
			double pick6Pct = stats.Plays > 0 ? ((double)stats.IntTDs / (double)stats.Plays) : 0.0;
			double intPct = stats.Plays > 0 ? ((double)stats.Ints / (double)stats.Plays) : 0.0;
			double pick6IntPct = stats.Ints > 0 ? ((double)stats.IntTDs / (double)stats.Ints) : 0.0;

			outFile.Write("\t\t\t<TR><TD class=\"highlight\">" + caption);
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Plays.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.Ints.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + intPct.ToString("0.00%"));
			outFile.Write("<TD align=\"right\" class=\"normal\">" + stats.IntTDs.ToString("N0"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + pick6Pct.ToString("0.00%"));
			outFile.Write("<TD align=\"right\" class=\"highlight\">" + pick6IntPct.ToString("0.00%"));
			outFile.WriteLine();
		}

		private void WriteMiscellaneousStats()
		{
			string outFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "MiscellaneousStats.html");
			System.IO.StreamWriter outFile = new System.IO.StreamWriter(outFileName, false);

			outFile.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
			outFile.WriteLine("<HTML>");
			outFile.WriteLine("\t<HEAD>");
			outFile.WriteLine("\t\t<TITLE>Miscellaneous Statistics</TITLE>");
			outFile.WriteLine("\t\t<STYLE TYPE=\"text/css\">");
			outFile.WriteLine("\t\t\tTD.highlight { color: black; background: yellow }");
			outFile.WriteLine("\t\t\tTD.normal { color: black; background: white }");
			outFile.WriteLine("\t\t</STYLE>");
			outFile.WriteLine("\t</HEAD>");
			outFile.WriteLine("\t<BODY>");

			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows injury statistics\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>League Injury Stats</EM></CAPTION>");

			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH>Plays");
			outFile.Write("<TH>Injuries");
			outFile.Write("<TH>Pct");
			outFile.WriteLine();

			WriteInjuryStatsLine(outFile, "Normal", mNormalPlayInjuries);
			WriteInjuryStatsLine(outFile, "SpecTeam", mSpecialTeamsInjuries);
			for (int i = 0; i < mInjuriesInWeather.Length; ++i)
			{
				WriteInjuryStatsLine(outFile, mUniverseData.PrecipMap[i], mInjuriesInWeather[i]);
			}
			WriteInjuryStatsLine(outFile, "All", mAllInjuries);

			outFile.WriteLine("\t\t</TABLE>");
			outFile.WriteLine("\t\t<TABLE border=\"1\" summary=\"This table shows Pick6 statistics\">");
			outFile.WriteLine("\t\t\t<CAPTION><EM>League Interceptions for TDs</EM></CAPTION>");

			outFile.Write("\t\t\t<TR>");
			outFile.Write("<TH>");
			outFile.Write("<TH>Plays");
			outFile.Write("<TH>Ints");
			outFile.Write("<TH>Pct");
			outFile.Write("<TH>Pick6");
			outFile.Write("<TH>PctOfAll");
			outFile.Write("<TH>PctOfInts");
			outFile.WriteLine();

			foreach (LeagueData.PassDistance passDist in Enum.GetValues(typeof(LeagueData.PassDistance)))
			{
				if (passDist != LeagueData.PassDistance.Count && passDist != LeagueData.PassDistance.Spike)
				WriteIntTDLine(outFile, passDist.ToString(), mIntTDsAtDistance[(int)passDist]);
			}

			outFile.WriteLine("\t\t</TABLE>");

			outFile.Close();

			DisplayPage(outFileName, webBrowserMiscellaneous);
		}

		private void InitializeStatsArrays()
		{
			int i, j, k;
			mAllPassingStats = new PassingStats();
			mPassingStatsVsDefensePlayCall = new PassingStats[(int)DefensivePlayCall.Count];
			for (i = 0; i < (int)DefensivePlayCall.Count; ++i)
			{
				mPassingStatsVsDefensePlayCall[i] = new PassingStats();
			}
			mPassingStatsVsDoubleCoverage = new PassingStats[(int)DoubleCoverageType.Count];
			for (i = 0; i < (int)DoubleCoverageType.Count; ++i)
			{
				mPassingStatsVsDoubleCoverage[i] = new PassingStats();
			}
			mPassingStatsVsDefensiveCoverage = new PassingStats[(int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)LeagueData.DefensiveCoverage.Count; ++i)
			{
				mPassingStatsVsDefensiveCoverage[i] = new PassingStats();
			}
			mPassingStatsVsDefenseFamiliar = new PassingStats[(int)LeagueData.DefenseFamiliar.Count];
			for (i = 0; i < (int)LeagueData.DefenseFamiliar.Count; ++i)
			{
				mPassingStatsVsDefenseFamiliar[i] = new PassingStats();
			}
			mPassingStatsVsDoubleCoverageAndPlayCall = new PassingStats[(int)DoubleCoverageType.Count, (int)DefensivePlayCall.Count];
			for (i = 0; i < (int)DoubleCoverageType.Count; ++i)
			{
				for (j = 0; j < (int)DefensivePlayCall.Count; ++j)
				{
					mPassingStatsVsDoubleCoverageAndPlayCall[i, j] = new PassingStats();
				}
			}
			mPassingStatsVsDoubleCoverageAndCoverage = new PassingStats[(int)DoubleCoverageType.Count, (int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)DoubleCoverageType.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.DefensiveCoverage.Count; ++j)
				{
					mPassingStatsVsDoubleCoverageAndCoverage[i, j] = new PassingStats();
				}
			}
			mPassingStatsAtDistance = new PassingStats[(int)LeagueData.PassDistance.Count];
			for (i = 0; i < (int)LeagueData.PassDistance.Count; ++i)
			{
				mPassingStatsAtDistance[i] = new PassingStats();
			}
			mPassingStatsAtDistanceVsPlayCall = new PassingStats[(int)LeagueData.PassDistance.Count, (int)DefensivePlayCall.Count];
			for (i = 0; i < (int)LeagueData.PassDistance.Count; ++i)
			{
				for (j = 0; j < (int)DefensivePlayCall.Count; ++j)
				{
					mPassingStatsAtDistanceVsPlayCall[i, j] = new PassingStats();
				}
			}
			mPassingStatsAtDistanceVsCoverage = new PassingStats[(int)LeagueData.PassDistance.Count, (int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)LeagueData.PassDistance.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.DefensiveCoverage.Count; ++j)
				{
					mPassingStatsAtDistanceVsCoverage[i, j] = new PassingStats();
				}
			}
			mPassingStatsFromOffensiveFormation = new PassingStats[(int)LeagueData.OffensiveFormation.Count];
			for (i = 0; i < (int)LeagueData.OffensiveFormation.Count; ++i)
			{
				mPassingStatsFromOffensiveFormation[i] = new PassingStats();
			}
			mPassingStatsFromOffensiveFormationVsPlayCall = new PassingStats[(int)LeagueData.OffensiveFormation.Count, (int)DefensivePlayCall.Count];
			for (i = 0; i < (int)LeagueData.OffensiveFormation.Count; ++i)
			{
				for (j = 0; j < (int)DefensivePlayCall.Count; ++j)
				{
					mPassingStatsFromOffensiveFormationVsPlayCall[i, j] = new PassingStats();
				}
			}
			mPassingStatsAtDistanceFromFormation = new PassingStats[(int)LeagueData.PassDistance.Count, (int)LeagueData.OffensiveFormation.Count];
			for (i = 0; i < (int)LeagueData.PassDistance.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.OffensiveFormation.Count; ++j)
				{
					mPassingStatsAtDistanceFromFormation[i, j] = new PassingStats();
				}
			}
			mPassingStatsAtDistanceFromFormationVsCoverage = new PassingStats[(int)LeagueData.PassDistance.Count, (int)LeagueData.OffensiveFormation.Count, (int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)LeagueData.PassDistance.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.OffensiveFormation.Count; ++j)
				{
					for (k = 0; k < (int)LeagueData.DefensiveCoverage.Count; ++k)
					{
						mPassingStatsAtDistanceFromFormationVsCoverage[i, j, k] = new PassingStats();
					}
				}
			}

			mAllRushingStats = new RushingStats();
			mRushingStatsVsDefensePlayCall = new RushingStats[(int)DefensivePlayCall.Count];
			for (i = 0; i < (int)DefensivePlayCall.Count; ++i)
			{
				mRushingStatsVsDefensePlayCall[i] = new RushingStats();
			}
			mRushingStatsVsDefensiveCoverage = new RushingStats[(int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)LeagueData.DefensiveCoverage.Count; ++i)
			{
				mRushingStatsVsDefensiveCoverage[i] = new RushingStats();
			}
			mRushingStatsVsDefenseFamiliar = new RushingStats[(int)LeagueData.DefenseFamiliar.Count];
			for (i = 0; i < (int)LeagueData.DefenseFamiliar.Count; ++i)
			{
				mRushingStatsVsDefenseFamiliar[i] = new RushingStats();
			}
			mRushingStatsFromOffensiveFormation = new RushingStats[(int)LeagueData.OffensiveFormation.Count];
			for (i = 0; i < (int)LeagueData.OffensiveFormation.Count; ++i)
			{
				mRushingStatsFromOffensiveFormation[i] = new RushingStats();
			}
			mRushingStatsFromOffensiveFormationVsPlayCall = new RushingStats[(int)LeagueData.OffensiveFormation.Count, (int)DefensivePlayCall.Count];
			for (i = 0; i < (int)LeagueData.OffensiveFormation.Count; ++i)
			{
				for (j = 0; j < (int)DefensivePlayCall.Count; ++j)
				{
					mRushingStatsFromOffensiveFormationVsPlayCall[i, j] = new RushingStats();
				}
			}
			mRushingStatsFromOffensiveFormationVsCoverage = new RushingStats[(int)LeagueData.OffensiveFormation.Count, (int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)LeagueData.OffensiveFormation.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.DefensiveCoverage.Count; ++j)
				{
					mRushingStatsFromOffensiveFormationVsCoverage[i, j] = new RushingStats();
				}
			}
			mRushingStatsInDirection = new RushingStats[(int)LeagueData.RunDirection.Count];
			for (i = 0; i < (int)LeagueData.RunDirection.Count; ++i)
			{
				mRushingStatsInDirection[i] = new RushingStats();
			}
			mRushingStatsInDrectionFromFormation = new RushingStats[(int)LeagueData.RunDirection.Count, (int)LeagueData.OffensiveFormation.Count];
			for (i = 0; i < (int)LeagueData.RunDirection.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.OffensiveFormation.Count; ++j)
				{
					mRushingStatsInDrectionFromFormation[i, j] = new RushingStats();
				}
			}


			mAllOffensivePlays = new OffenseStats();

			mOffenseByDownDistance = new OffenseStats[NumberOfDowns, NumberOfToGoDistances];
			mOffenseByDownDistanceFirstHalf = new OffenseStats[NumberOfDowns, NumberOfToGoDistances];
			for (i = 0; i < NumberOfDowns; ++i)
			{
				for (j = 0; j < NumberOfToGoDistances; ++j)
				{
					mOffenseByDownDistance[i, j] = new OffenseStats();
					mOffenseByDownDistanceFirstHalf[i, j] = new OffenseStats();
				}
			}

			mOffenseByFormation = new OffenseStats[(int)LeagueData.OffensiveFormation.Count];
			for (i = 0; i < (int)LeagueData.OffensiveFormation.Count; ++i)
			{
				mOffenseByFormation[i] = new OffenseStats();
			}

			mOffenseByFieldPosition = new OffenseStats[NumberOfYardBands];
			for (i = 0; i < NumberOfYardBands; ++i)
			{
				mOffenseByFieldPosition[i] = new OffenseStats();
			}

			mOffenseRunsByDirection = new OffenseStats[(int)LeagueData.RunDirection.Count];
			for (i = 0; i < (int)LeagueData.RunDirection.Count; ++i)
			{
				mOffenseRunsByDirection[i] = new OffenseStats();
			}

			mOffensePassesByDistance = new OffenseStats[(int)LeagueData.PassDistance.Count];
			mIntTDsAtDistance = new IntTDStats[(int)LeagueData.PassDistance.Count];
			for (i = 0; i < (int)LeagueData.PassDistance.Count; ++i)
			{
				mOffensePassesByDistance[i] = new OffenseStats();
				mIntTDsAtDistance[i] = new IntTDStats();
			}

			mAllDefensivePlays = new OffenseStats();

			mDefenseByDownDistance = new OffenseStats[NumberOfDowns, NumberOfToGoDistances];
			for (i = 0; i < NumberOfDowns; ++i)
			{
				for (j = 0; j < NumberOfToGoDistances; ++j)
				{
					mDefenseByDownDistance[i, j] = new OffenseStats();
				}
			}

			mDefenseByCall = new OffenseStats[(int)DefensivePlayCall.Count];
			for (i = 0; i < (int)DefensivePlayCall.Count; ++i)
			{
				mDefenseByCall[i] = new OffenseStats();
			}

			mDefenseByCoverage = new OffenseStats[(int)LeagueData.DefensiveCoverage.Count];
			for (i = 0; i < (int)LeagueData.DefensiveCoverage.Count; ++i)
			{
				mDefenseByCoverage[i] = new OffenseStats();
			}

			mDefenseByFieldPosition = new OffenseStats[NumberOfYardBands];
			for (i = 0; i < NumberOfYardBands; ++i)
			{
				mDefenseByFieldPosition[i] = new OffenseStats();
			}

			mDefenseByCallAndPersonnel = new OffenseStats[(int)DefensivePlayCall.Count, (int)LeagueData.DefensivePersonnel.Count];
			for (i = 0; i < (int)DefensivePlayCall.Count; ++i)
			{
				for (j = 0; j < (int)LeagueData.DefensivePersonnel.Count; ++j)
				{
					mDefenseByCallAndPersonnel[i, j] = new OffenseStats();
				}
			}

			mDefenseByCallAndBlitzers = new OffenseStats[(int)DefensivePlayCall.Count, NumberOfBlitzers];
			for (i = 0; i < (int)DefensivePlayCall.Count; ++i)
			{
				for (j = 0; j < NumberOfBlitzers; ++j)
				{
					mDefenseByCallAndBlitzers[i, j] = new OffenseStats();
				}
			}

			mAllInjuries = new InjuryStats();
			mSpecialTeamsInjuries = new InjuryStats();
			mNormalPlayInjuries = new InjuryStats();

			mInjuriesInWeather = new InjuryStats[(int)mUniverseData.PrecipMap.Length];
			mOffenseInWeather = new OffenseStats[(int)mUniverseData.PrecipMap.Length];
			for (i = 0; i < mOffenseInWeather.Length; ++i)
			{
				mOffenseInWeather[i] = new OffenseStats();
				mInjuriesInWeather[i] = new InjuryStats();
			}
		}
	}
}

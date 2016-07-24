using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DataReader;
using DBUpdater.Tables;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;

namespace DBUpdater
{
	public partial class DBUpdaterForm : Form
	{
		delegate void AddStatusTextCallback(string text);
		delegate void WorkCompletedCallback();

		private UniverseData mUniverseData;
		private LeagueData mLeagueData;

		private const string kSettingsRoot = "DBUpdater";

		private WindowsUtilities.XMLSettings mSettings;
		public DBUpdaterForm()
		{
			InitializeComponent();

			Assembly a = typeof(DBUpdaterForm).Assembly;
			Text += " v" + a.GetName().Version;

			mUniverseData = new UniverseData();

			comboBoxLeague.Items.Add("(Select League)");
			for (int i = 0; i < mUniverseData.SavedGames.Count; i++)
			{
				comboBoxLeague.Items.Add(mUniverseData.SavedGames[i].GameName);
			}
			if (mUniverseData.SavedGames.Count > 0)
			{
				comboBoxLeague.SelectedIndex = 0;
			}

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DBUpdater.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);

			CreateMaps();
		}

		private void AddStatusString(string newText)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.textBoxProgress.InvokeRequired)
			{
				AddStatusTextCallback d = new AddStatusTextCallback(AddStatusString);
				this.Invoke(d, new object[] { newText });
			}
			else
			{
				textBoxProgress.Text += newText;
				textBoxProgress.SelectionStart = textBoxProgress.Text.Length;
				textBoxProgress.ScrollToCaret();
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
				buttonUpdateDatabase.Enabled = true;
			}
		}


		private void OnFileRead(string fileName)
		{
			AddStatusString("     Reading file " + fileName + System.Environment.NewLine);
		}

		// define the local key and vector byte arrays
		byte[] key = {206, 16, 17, 254, 193, 209, 226, 238, 207, 246, 186, 10, 
              86, 116, 207, 252, 199, 191, 12, 48, 162, 246, 106, 243};
		byte[] iv = { 146, 89, 142, 175, 158, 60, 207, 95 };

		private void comboBoxLeague_SelectedIndexChanged(object sender, EventArgs e)
		{
			int savedGameIndex = comboBoxLeague.SelectedIndex - 1;
			if (savedGameIndex >= 0 && savedGameIndex < mUniverseData.SavedGames.Count)
			{
				string gameKey = mUniverseData.SavedGames[savedGameIndex].GameName.Replace(' ', '_');
				textBoxServer.Text = mSettings.ReadXMLString(kSettingsRoot, gameKey + "_Server", "");
				textBoxPort.Text = mSettings.ReadXMLString(kSettingsRoot, gameKey + "_Port", "3306");
				textBoxDatabase.Text = mSettings.ReadXMLString(kSettingsRoot, gameKey + "_Database", "");
				textBoxUser.Text = mSettings.ReadXMLString(kSettingsRoot, gameKey + "_User", "");

				string encryptedPassword = mSettings.ReadXMLString(kSettingsRoot, gameKey + "_Password", "");
				if (encryptedPassword.Length > 0)
				{
					TripleDESWrapper desWrap = new TripleDESWrapper(key, iv);
					textBoxPassword.Text = desWrap.Decrypt(encryptedPassword);
				}
				else
				{
					textBoxPassword.Text = "";
				}
			}
		}

		Cursor mOldCursor = null;

		private int mSavedGameIndex = -1;
		private void buttonUpdateDatabase_Click(object sender, EventArgs e)
		{
			mSavedGameIndex = comboBoxLeague.SelectedIndex - 1;
			if (mSavedGameIndex >= 0 && mSavedGameIndex < mUniverseData.SavedGames.Count)
			{
				string gameKey = mUniverseData.SavedGames[mSavedGameIndex].GameName.Replace(' ', '_');
				mSettings.WriteXMLString(kSettingsRoot, gameKey + "_Server", textBoxServer.Text);
				mSettings.WriteXMLString(kSettingsRoot, gameKey + "_Port", textBoxPort.Text);
				mSettings.WriteXMLString(kSettingsRoot, gameKey + "_Database", textBoxDatabase.Text);
				mSettings.WriteXMLString(kSettingsRoot, gameKey + "_User", textBoxUser.Text);
				string decryptedPassword = textBoxPassword.Text;
				if (decryptedPassword.Length > 0)
				{
					TripleDESWrapper desWrap = new TripleDESWrapper(key, iv);
					mSettings.WriteXMLString(kSettingsRoot, gameKey + "_Password", desWrap.Encrypt(decryptedPassword));
				}
				else
				{
					mSettings.WriteXMLString(kSettingsRoot, gameKey + "_Password", "");
				}

				textBoxProgress.Text = "";
				buttonUpdateDatabase.Enabled = false;
				mOldCursor = Cursor;
				Cursor = Cursors.WaitCursor;

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
			mLeagueData = new LeagueData(mUniverseData.SavedGames[mSavedGameIndex].PathPrefix, mUniverseData, LeagueData.LoadCurrentSeasonOnly, OnFileRead, readGameLogs);

			AddStatusString("Game data loaded, processing database..." + System.Environment.NewLine);

			CheckDatabase();

			if (mDBConnectionIsGood)
			{
				RunUpdate();
			}

			AddStatusString("Finished!" + System.Environment.NewLine);

			DateTime endTime = DateTime.Now;
			TimeSpan totalTime = endTime - startTime;
			int minutes = totalTime.Minutes;
			int seconds = totalTime.Seconds;
			AddStatusString("Run Time: " + minutes.ToString() + "min " + seconds.ToString() +"sec" + System.Environment.NewLine);

			WorkCompleted();
		}

		private int mLastGameStageIndex;
		private int mCurrentGameStageIndex;
		private int mLastYear;
		private int mOldSchemaVersion;
		private int mNewSchemaVersion;

		private int CalcGameStage(int stage, int week, int faStage)
		{
			return (stage * 10000 + week * 100 + faStage);
		}

		private void RunUpdate()
		{
			try
			{
				OpenNHibernateConnection();

				DoStageUpdate();
				DoStageNamesUpdate();
				DoMappingsUpdate();
				DoInjuriesUpdate();
				DoCollegesUpdate();
				DoHometownsUpdate();
				DoCitiesUpdate();
				DoTeamUpdate();
				DoTeamStadiumsUpdate();
				DoFranchiseScoresUpdate();
				DoTransactionUpdates();
				DoTeamScheduleUpdates();
				DoFutureDraftsUpdates();
				DoGameResultUpdates();
				DoPlayerGameStatsUpdates();
				DoPlayerActiveUpdates();
				DoPlayerHistoricalUpdates();
			}
			catch(System.Exception e)
			{
				AddStatusString("     Exception Hit, aborted!" + System.Environment.NewLine);
				AddStatusString(e.ToString() + System.Environment.NewLine);
				if (e.InnerException != null)
				{
					AddStatusString(e.InnerException.ToString() + System.Environment.NewLine);
				}
			}
			finally
			{
				CloseNHibernateConnection();
			}
		}

		private List<int> mRetiredPlayerIDs;

		private void CopyPlayerHistorical(LeagueData.PlayerHistoricalRecord fofRec, FofPlayerhistorical dbRec)
		{
			dbRec.BirthDay = (byte)fofRec.BirthDay;
			dbRec.BirthMonth = (byte)fofRec.BirthMonth;
			dbRec.BirthYear = fofRec.BirthYear;
			dbRec.College = fofRec.College;
			dbRec.DraftedBy = (byte)fofRec.DraftedBy;
			dbRec.DraftPick = (byte)fofRec.DraftPick;
			dbRec.DraftRound = (byte)fofRec.DraftRound;
			dbRec.Experience = (byte)fofRec.Experience;
			dbRec.FirstName = fofRec.FirstName;
			dbRec.FourqHero = fofRec.FourthQuarterHeroics;
			dbRec.HallOfFameVote = (byte)fofRec.HallOfFameVote;
			dbRec.HallOfFameYear = fofRec.HallOfFameYear;
			dbRec.Height = (byte)fofRec.Height;
			dbRec.HomeTown = fofRec.HomeTown;
			dbRec.Id = fofRec.PlayerID;
			dbRec.InHallOfFame = (byte)fofRec.InHallOfFame;
			dbRec.LastName = fofRec.LastName;
			dbRec.NickName = fofRec.NickName;
			dbRec.Pog = fofRec.PlayerOfTheGame;
			dbRec.Position = (byte)fofRec.Position;
			dbRec.PowMentions = fofRec.PlayerOfTheWeekMentions;
			dbRec.PowWins = fofRec.PlayerOfTheWeekWins;
			dbRec.QbLosses = fofRec.QBLosses;
			dbRec.QbTies = fofRec.QBTies;
			dbRec.QbWins = fofRec.QBWins;
			dbRec.Rings = fofRec.ChampionshipRings;
			dbRec.Weight = fofRec.Weight;
			dbRec.YearDraft = fofRec.YearDrafted;
		}

		private void DoPlayerHistoricalUpdates()
		{
			int fofIndex = 0;
			int dbIndex = 0;

			int newCount = 0;
			int updatedCount = 0;

			AddStatusString("     Querying historical players...");
			IList<FofPlayerhistorical> dbRecs = session.CreateCriteria(typeof(FofPlayerhistorical))
				.Add(Expression.Eq("_YearRetired", (short)0))
				.AddOrder(Order.Asc("_Id")).List<FofPlayerhistorical>();

			AddStatusString("Updating historical players...");
			transaction = session.BeginTransaction();

			while (fofIndex < mLeagueData.PlayerHistoricalRecords.Length || dbIndex < dbRecs.Count)
			{
				// First move the fof Index until it matches or is past the dbRec player.
				// Any of these are new historical records that must be created as long as
				// they are rookies (retired players won't be in the query and should not be
				// created again)
				while (fofIndex < mLeagueData.PlayerHistoricalRecords.Length
					&& (dbIndex >= dbRecs.Count || mLeagueData.PlayerHistoricalRecords[fofIndex].PlayerID < dbRecs[dbIndex].Id)
					)
				{
					if (mLeagueData.PlayerHistoricalRecords[fofIndex].Experience < 2 || mStartFromScratch)
					{
						FofPlayerhistorical newRec = new FofPlayerhistorical();
						CopyPlayerHistorical(mLeagueData.PlayerHistoricalRecords[fofIndex], newRec);
						session.Save(newRec);
						++newCount;
					}
					++fofIndex;
				}

				// Now move the DB index until it matches or is past the fofRec player.
				// Any of these are ones that FOF has abandoned. If they have not already
				// been marked as retired, do so now.
				while (dbIndex < dbRecs.Count
					&& (fofIndex >= mLeagueData.PlayerHistoricalRecords.Length || dbRecs[dbIndex].Id < mLeagueData.PlayerHistoricalRecords[fofIndex].PlayerID)
					)
				{
					if (dbRecs[dbIndex].YearRetired == 0 || mRetiredPlayerIDs.IndexOf(dbRecs[dbIndex].Id) >= 0)
					{
						dbRecs[dbIndex].YearRetired = mGameInfo.CurYear;
						++updatedCount;
						session.Update(dbRecs[dbIndex]);
					}

					++dbIndex;
				}

				// Now if they are equal, update the dbRec and advance both
				if (fofIndex < mLeagueData.PlayerHistoricalRecords.Length && dbIndex < dbRecs.Count)
				{
					if (mLeagueData.PlayerHistoricalRecords[fofIndex].PlayerID == dbRecs[dbIndex].Id)
					{
						CopyPlayerHistorical(mLeagueData.PlayerHistoricalRecords[fofIndex], dbRecs[dbIndex]);
						if (mRetiredPlayerIDs.IndexOf(dbRecs[dbIndex].Id) >= 0)
						{
							dbRecs[dbIndex].YearRetired = mGameInfo.CurYear;
						}

						if (dbRecs[dbIndex].IsChanged)
						{
							session.Update(dbRecs[dbIndex]);
							++updatedCount;
						}
						++fofIndex;
						++dbIndex;
					}
				}
			}

			transaction.Commit();
			AddStatusString("Done! ("+newCount.ToString()+" new, "+updatedCount.ToString()+" updated)"+System.Environment.NewLine);
		}

		private void CopyPlayerActive(LeagueData.PlayerActiveRecord fofRec, FofPlayeractive dbRec)
		{
			dbRec.Bonus1 = fofRec.Bonus[0] * 10000;
			dbRec.Bonus2 = fofRec.Bonus[1] * 10000;
			dbRec.Bonus3 = fofRec.Bonus[2] * 10000;
			dbRec.Bonus4 = fofRec.Bonus[3] * 10000;
			dbRec.Bonus5 = fofRec.Bonus[4] * 10000;
			dbRec.Bonus6 = fofRec.Bonus[5] * 10000;
			dbRec.Bonus7 = fofRec.Bonus[6] * 10000;
			dbRec.ContractLength = (byte)fofRec.ContractLength;
			dbRec.Experience = (byte)fofRec.Experience;
			dbRec.Id = fofRec.PlayerID;
			dbRec.InjuryLength = fofRec.InjuryLength;
			dbRec.Intelligence = (byte)fofRec.Intelligence;
			dbRec.JoinedTeam = fofRec.JoinedTeam;
			dbRec.Leadership = (byte)fofRec.Leadership;
			dbRec.Loyalty = (byte)fofRec.Loyalty;
			dbRec.Number = (byte)fofRec.Number;
			dbRec.Personality = (byte)fofRec.Personality;
			dbRec.PlaysToWin = (byte)fofRec.PlaysToWin;
			dbRec.Popularity = (byte)fofRec.Popularity;
			dbRec.Position = (byte)fofRec.Position;
			dbRec.PositionGroup = (byte)fofRec.PositionGroup;
			dbRec.RedFlagMarker = (byte)fofRec.RedFlagMarker;
			dbRec.Salary1 = fofRec.Salary[0] * 10000;
			dbRec.Salary2 = fofRec.Salary[1] * 10000;
			dbRec.Salary3 = fofRec.Salary[2] * 10000;
			dbRec.Salary4 = fofRec.Salary[3] * 10000;
			dbRec.Salary5 = fofRec.Salary[4] * 10000;
			dbRec.Salary6 = fofRec.Salary[5] * 10000;
			dbRec.Salary7 = fofRec.Salary[6] * 10000;
			dbRec.Team = (byte)fofRec.Team;
			dbRec.UfaYear = fofRec.UFAYear;
			dbRec.Volatility = (byte)fofRec.Volatility;
		}

		private void DoPlayerActiveUpdates()
		{
			mRetiredPlayerIDs = new List<int>();

			int fofIndex = 0;
			int dbIndex = 0;

			int newCount = 0;
			int updatedCount = 0;
			int deletedCount = 0;

			AddStatusString("     Querying active players...");
			IList<FofPlayeractive> dbRecs = session.CreateCriteria(typeof(FofPlayeractive)).AddOrder(Order.Asc("_Id")).List<FofPlayeractive>();

			AddStatusString("Updating active players...");
			transaction = session.BeginTransaction();

			while (fofIndex < mLeagueData.PlayerActiveRecords.Length || dbIndex < dbRecs.Count)
			{
				// First move the fof Index until it matches or is past the dbRec player.
				// Any of these are new active records that must be created.
				while (fofIndex < mLeagueData.PlayerActiveRecords.Length
					&& (dbIndex >= dbRecs.Count || mLeagueData.PlayerActiveRecords[fofIndex].PlayerID < dbRecs[dbIndex].Id)
					)
				{
					FofPlayeractive newRec = new FofPlayeractive();
					CopyPlayerActive(mLeagueData.PlayerActiveRecords[fofIndex], newRec);
					session.Save(newRec);
					++newCount;
					++fofIndex;
				}

				// Now move the DB index until it matches or is past the fofRec player.
				// Any of these are ones that need to be deleted and should be marked
				// as retired in the historical data.
				while (dbIndex < dbRecs.Count
					&& (fofIndex >= mLeagueData.PlayerActiveRecords.Length || dbRecs[dbIndex].Id < mLeagueData.PlayerActiveRecords[fofIndex].PlayerID)
					)
				{
					mRetiredPlayerIDs.Add(dbRecs[dbIndex].Id);
					session.Delete(dbRecs[dbIndex]);
					++deletedCount;
					++dbIndex;
				}

				// Now if they are equal, update the dbRec and advance both
				if ( fofIndex < mLeagueData.PlayerActiveRecords.Length && dbIndex < dbRecs.Count )
				{
					System.Diagnostics.Debug.Assert(mLeagueData.PlayerActiveRecords[fofIndex].PlayerID == dbRecs[dbIndex].Id,
						"FOF and DB active records don't line up!");
					if (mLeagueData.PlayerActiveRecords[fofIndex].PlayerID == dbRecs[dbIndex].Id)
					{
						CopyPlayerActive(mLeagueData.PlayerActiveRecords[fofIndex], dbRecs[dbIndex]);
						if (dbRecs[dbIndex].IsChanged)
						{
							session.Update(dbRecs[dbIndex]);
							++updatedCount;
						}
					}
					++fofIndex;
					++dbIndex;
				}
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " new, " + updatedCount.ToString() + " updated, " + deletedCount.ToString() +
				" deleted)" + System.Environment.NewLine);
		}

		private void DoPlayerGameStatsUpdates()
		{
			AddStatusString("     Writing player game stats...");

			int lastWeekWritten = 0;
			if (!mStartFromScratch && (mLastYear == mGameInfo.CurYear))
			{
				System.Collections.IList results = session.CreateCriteria(typeof(FofPlayergamestats))
					.SetProjection(Projections.Max("_Week"))
					.Add(Expression.Eq("_Year", mGameInfo.CurYear))
					.List();
				if (results.Count > 0 && results[0] != null)
				{
					lastWeekWritten = System.Convert.ToInt32(results[0]);
				}
			}

			transaction = session.BeginTransaction();

			int newCount = 0;

			int yearIndex = mGameInfo.CurYear - mGameInfo.StartYear;
			for (int i = 0; i < mLeagueData.PlayerGameStatsRecords[yearIndex].Length; ++i)
			{
				LeagueData.PlayerGameStatsRecord fofRec = mLeagueData.PlayerGameStatsRecords[yearIndex][i];

				if (fofRec.Week > lastWeekWritten && fofRec.PlayerID >=0 && fofRec.Team != 99)
				{
					FofPlayergamestats dbRec = new FofPlayergamestats();
					dbRec.AllPurposeYards = (short)fofRec.AllPurposeYards;
					dbRec.Assists = (byte)fofRec.Assists;
					dbRec.Catches = (byte)fofRec.Catches;
					dbRec.CatchesOf20YardsPlus = (byte)fofRec.CatchesOf20YardsPlus;
					dbRec.Fg40PlusAttempts = (byte)fofRec.FG40PlusAttempts;
					dbRec.Fg40PlusMade = (byte)fofRec.FG40PlusMade;
					dbRec.Fg50PlusAttempts = (byte)fofRec.FG50PlusAttempts;
					dbRec.Fg50PlusMade = (byte)fofRec.FG50PlusMade;
					dbRec.FgAttempted = (byte)fofRec.FGAttempted;
					dbRec.FgLong = (byte)fofRec.FGLong;
					dbRec.FgMade = (byte)fofRec.FGMade;
					dbRec.FirstDownCatches = (byte)fofRec.FirstDownCatches;
					dbRec.FirstDownPasses = (byte)fofRec.FirstDownPasses;
					dbRec.FirstDownRushes = (byte)fofRec.FirstDownRushes;
					dbRec.ForcedFumbles = (byte)fofRec.ForcedFumbles;
					dbRec.FumbleRecoveries = (byte)fofRec.FumbleRecoveries;
					dbRec.Fumbles = (byte)fofRec.Fumbles;
					dbRec.GamePlayed = (byte)fofRec.GamePlayed;
					dbRec.GameStarted = (byte)fofRec.GameStarted;
					dbRec.IntReturntDs = (byte)fofRec.INTReturnTDs;
					dbRec.IntReturnYards = (short)fofRec.INTReturnYards;
					dbRec.InTs = (byte)fofRec.INTs;
					dbRec.IntThrown = (byte)fofRec.INTThrown;
					dbRec.KeyRunBlock = (byte)fofRec.KeyRunBlock;
					dbRec.KeyRunBlockOpportunites = (byte)fofRec.KeyRunBlockOpportunites;
					dbRec.KickReturns = (byte)fofRec.KickReturns;
					dbRec.KickReturntDs = (byte)fofRec.KickReturnTDs;
					dbRec.KickReturnYards = (short)fofRec.KickReturnYards;
					dbRec.LongestPass = (short)fofRec.LongestPass;
					dbRec.LongestReception = (short)fofRec.LongestReception;
					dbRec.LongestRun = (short)fofRec.LongestRun;
					dbRec.Misctd = (byte)fofRec.MiscTD;
					dbRec.OpposingTeamid = (byte)fofRec.OpposingTeamID;
					dbRec.PancakeBlocks = (byte)fofRec.PancakeBlocks;
					dbRec.PassAttempts = (byte)fofRec.PassAttempts;
					dbRec.PassCompletions = (byte)fofRec.PassCompletions;
					dbRec.PassDrops = (byte)fofRec.PassDrops;
					dbRec.PassesBlocked = (byte)fofRec.PassesBlocked;
					dbRec.PassesCaught = (byte)fofRec.PassesCaught;
					dbRec.PassesDefended = (byte)fofRec.PassesDefended;
					dbRec.PassingGamesOver300Yards = (byte)fofRec.PassingGamesOver300Yards;
					dbRec.PassPlays = (byte)fofRec.PassPlays;
					dbRec.PassTargets = (byte)fofRec.PassTargets;
					dbRec.PassYards = (short)fofRec.PassYards;
					dbRec.Pat = (byte)fofRec.PAT;
					dbRec.PatAttempted = (byte)fofRec.PATAttempted;
					dbRec.Playerid = fofRec.PlayerID;
					dbRec.Points = (byte)fofRec.Points;
					dbRec.PuntIn20 = (byte)fofRec.PuntIn20;
					dbRec.PuntLong = (short)fofRec.PuntLong;
					dbRec.PuntNetYards = (short)fofRec.PuntNetYards;
					dbRec.PuntReturns = (byte)fofRec.PuntReturns;
					dbRec.PuntReturntDs = (byte)fofRec.PuntReturnTDs;
					dbRec.PuntReturnYards = (short)fofRec.PuntReturnYards;
					dbRec.Punts = (byte)fofRec.Punts;
					dbRec.PuntYards = (short)fofRec.PuntYards;
					dbRec.QbHurries = (byte)fofRec.QBHurries;
					dbRec.QbKnockdowns = (byte)fofRec.QBKnockdowns;
					dbRec.ReceivingGamesOver100Yards = (byte)fofRec.ReceivingGamesOver100Yards;
					dbRec.ReceivingtDs = (byte)fofRec.ReceivingTDs;
					dbRec.ReceivingYards = (short)fofRec.ReceivingYards;
					dbRec.RedZonePassAttempts = (byte)fofRec.RedZonePassAttempts;
					dbRec.RedZonePassCompletions = (byte)fofRec.RedZonePassCompletions;
					dbRec.RedZonePassingYards = (short)fofRec.RedZonePassingYards;
					dbRec.RedZoneReceivingCatches = (byte)fofRec.RedZoneReceivingCatches;
					dbRec.RedZoneReceivingTargets = (byte)fofRec.RedZoneReceivingTargets;
					dbRec.RedZoneReceivingYards = (short)fofRec.RedZoneReceivingYards;
					dbRec.RedZoneRushes = (byte)fofRec.RedZoneRushes;
					dbRec.RedZoneRushingYards = (short)fofRec.RedZoneRushingYards;
					dbRec.RunPlays = (byte)fofRec.RunPlays;
					dbRec.RunsOf10YardsPlus = (byte)fofRec.RunsOf10YardsPlus;
					dbRec.RushAttempts = (byte)fofRec.RushAttempts;
					dbRec.RushingGamesOver100Yards = (byte)fofRec.RushingGamesOver100Yards;
					dbRec.RushingYards = (short)fofRec.RushingYards;
					dbRec.Rushtd = (byte)fofRec.RushTD;
					dbRec.SackedYards = (short)fofRec.SackedYards;
					dbRec.Sacks = (short)fofRec.Sacks;
					dbRec.SacksAllowed = (byte)fofRec.SacksAllowed;
					dbRec.SpecialTeamsPlays = (byte)fofRec.SpecialTeamsPlays;
					dbRec.SpecialTeamsTackles = (byte)fofRec.SpecialTeamsTackles;
					dbRec.Tackles = (byte)fofRec.Tackles;
					dbRec.TdPasses = (byte)fofRec.TDPasses;
					dbRec.Team = (byte)fofRec.Team;
					dbRec.ThirdDownPassAttempts = (byte)fofRec.ThirdDownPassAttempts;
					dbRec.ThirdDownPassCompletions = (byte)fofRec.ThirdDownPassCompletions;
					dbRec.ThirdDownPassConversions = (byte)fofRec.ThirdDownPassConversions;
					dbRec.ThirdDownReceivingCatches = (byte)fofRec.ThirdDownReceivingCatches;
					dbRec.ThirdDownReceivingConversions = (byte)fofRec.ThirdDownReceivingConversions;
					dbRec.ThirdDownReceivingTargets = (byte)fofRec.ThirdDownReceivingTargets;
					dbRec.ThirdDownRushConversions = (byte)fofRec.ThirdDownRushConversions;
					dbRec.ThirdDownRushes = (byte)fofRec.ThirdDownRushes;
					dbRec.ThrowsOf20YardsPlus = (byte)fofRec.ThrowsOf20YardsPlus;
					dbRec.TimesKnockedDown = (byte)fofRec.TimesKnockedDown;
					dbRec.TimesSacked = (byte)fofRec.TimesSacked;
					dbRec.TotaltDs = (byte)fofRec.TotalTDs;
					dbRec.TwoPointConversions = (byte)fofRec.TwoPointConversions;
					dbRec.Week = (byte)fofRec.Week;
					dbRec.YardsAfterCatch = (short)fofRec.YardsAfterCatch;
					dbRec.YardsFromScrimmage = (short)fofRec.YardsFromScrimmage;
					dbRec.Year = fofRec.Year;
					dbRec.DoubleCoveragesThrownInto = (byte)fofRec.DoubleCoveragesThrownInto;
					dbRec.DoubleCoveragesAvoided = (byte)fofRec.DoubleCoveragesAvoided;
					dbRec.BadPasses = (byte)fofRec.BadPasses;
					dbRec.RunsForLoss = (byte)fofRec.RunsForLoss;
					dbRec.RunsOf20YardsPlus = (byte)fofRec.RunsOf20YardsPlus;
					dbRec.FumblesLost = (byte)fofRec.FumblesLost;
					dbRec.HasKeyCoverage = (byte)fofRec.HasKeyCoverage;
					dbRec.ThrownAt = (byte)fofRec.ThrownAt;
					dbRec.TacklesForLoss = (byte)fofRec.TacklesForLoss;
					dbRec.AssistedTacklesForLoss = (byte)fofRec.AssistedTacklesForLoss;
					dbRec.ReceptionsOf20YardsPlusGivenUp = (byte)fofRec.ReceptionsOf20YardsPlusGivenUp;
					dbRec.Kickoffs = (byte)fofRec.Kickoffs;
					dbRec.KickoffYards = (short)fofRec.KickoffYards;
					dbRec.KickoffTouchbacks = (byte)fofRec.KickoffTouchbacks;
					dbRec.TotalFieldPositionAfterKickoff = (short)fofRec.TotalFieldPositionAfterKickoff;
					dbRec.OffensivePassPlays = (byte)fofRec.OffensivePassPlays;
					dbRec.OffensiveRunPlays = (byte)fofRec.OffensiveRunPlays;
					dbRec.DefensivePassPlays = (byte)fofRec.DefensivePassPlays;
					dbRec.DefensiveRunPlays = (byte)fofRec.DefensiveRunPlays;
					dbRec.SuccessfulPasses = (byte)fofRec.SuccessfulPasses;
					dbRec.SuccessfulCatches = (byte)fofRec.SuccessfulCatches;
					dbRec.SuccessfulRuns = (byte)fofRec.SuccessfulRuns;
					dbRec.BadPassesCaught = (byte)fofRec.BadPassesCaught;

					session.Save(dbRec);
					++newCount;
					if (newCount % 1000 == 0)
					{
						transaction.Commit();
						transaction = session.BeginTransaction();
					}
				}
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " added)" + System.Environment.NewLine);
		}

		private void DoGameResultUpdates()
		{
			AddStatusString("     Writing game results...");

			int newCount = 0;
			int driveCount = 0;

			LeagueData.GameLog matchingGameLog = null;

			int lastWeekWritten = 0;
			if (!mStartFromScratch)
			{
				if (mLastYear == mGameInfo.CurYear)
				{
					System.Collections.IList results = session.CreateCriteria(typeof(FofGameresults))
						.SetProjection(Projections.Max("_Week"))
						.Add(Expression.Eq("_Year", mGameInfo.CurYear))
						.List();
					if (results.Count > 0 && results[0] != null)
					{
						lastWeekWritten = System.Convert.ToInt32(results[0]);
					}
				}
			}

			transaction = session.BeginTransaction();

			for (int i = 0; i < mLeagueData.GameResultRecords.Length; ++i)
			{
				LeagueData.GameResultRecord fofRec = mLeagueData.GameResultRecords[i];
				matchingGameLog = null;

				if (fofRec.Week > lastWeekWritten && fofRec.Year == mGameInfo.CurYear)
				{
					int precip = fofRec.Weather / 7800;
					int windSpeed = ((fofRec.Weather % 7800) / 120);
					int temperature = ((fofRec.Weather % 120) - 10);

					FofGameresults dbRec = new FofGameresults();
					dbRec.Attendance = fofRec.Attendance * 100;
					dbRec.AwayPassAttempts = (byte)fofRec.AwayPassAttempts;
					dbRec.AwayPassCompletions = (byte)fofRec.AwayPassCompletions;
					dbRec.AwayPassingLeaderPlayerid = fofRec.AwayPassingLeaderPlayerID;
					dbRec.AwayPassYards = fofRec.AwayPassYards;
					dbRec.AwayReceivingLeaderPlayerid = fofRec.AwayReceivingLeaderPlayerID;
					dbRec.AwayReceivingYards = fofRec.AwayReceivingYards;
					dbRec.AwayReceptions = (byte)fofRec.AwayReceptions;
					dbRec.AwayRushAttempts = (byte)fofRec.AwayRushAttempts;
					dbRec.AwayRushingLeaderPlayerid = fofRec.AwayRushingLeaderPlayerID;
					dbRec.AwayRushYards = fofRec.AwayRushYards;
					dbRec.AwayScore = (byte)fofRec.AwayScore;
					dbRec.AwayTeam = (byte)fofRec.AwayTeam;
					dbRec.HomePassAttempts = (byte)fofRec.HomePassAttempts;
					dbRec.HomePassCompletions = (byte)fofRec.HomePassCompletions;
					dbRec.HomePassingLeaderPlayerid = fofRec.HomePassingLeaderPlayerID;
					dbRec.HomePassYards = fofRec.HomePassYards;
					dbRec.HomeReceivingLeaderPlayerid = fofRec.HomeReceivingLeaderPlayerID;
					dbRec.HomeReceivingYards = fofRec.HomeReceivingYards;
					dbRec.HomeReceptions = (byte)fofRec.HomeReceptions;
					dbRec.HomeRushAttempts = (byte)fofRec.HomeRushAttempts;
					dbRec.HomeRushingLeaderPlayerid = fofRec.HomeRushingLeaderPlayerID;
					dbRec.HomeRushYards = fofRec.HomeRushYards;
					dbRec.HomeScore = (byte)fofRec.HomeScore;
					dbRec.HomeTeam = (byte)fofRec.HomeTeam;
					dbRec.Precip = (byte)precip;
					dbRec.Temperature = (short)temperature;
					dbRec.Week = (byte)fofRec.Week;
					dbRec.Wind = (byte)windSpeed;
					dbRec.Year = fofRec.Year;

					foreach (LeagueData.GameWeekRecord gameWeekRec in mLeagueData.AvailableGameWeeks)
					{
						if (gameWeekRec.Year == dbRec.Year && gameWeekRec.Week == dbRec.Week)
						{
							foreach (LeagueData.GameLog gameLog in gameWeekRec.GameLogs)
							{
								if (gameLog.AwayTeam.TeamIndex == dbRec.AwayTeam && gameLog.HomeTeam.TeamIndex == dbRec.HomeTeam)
								{
									matchingGameLog = gameLog;

									dbRec.PlayerOfTheGame = gameLog.PlayerOfTheGameID;

									dbRec.HomeLeftRushAttempts = (byte)gameLog.HomeRushing.LeftAttempts;
									dbRec.HomeLeftRushYards = gameLog.HomeRushing.LeftYards;
									dbRec.HomeLongAttempts = (byte)gameLog.HomePassing.LongAttempts;
									dbRec.HomeLongCompletions = (byte)gameLog.HomePassing.LongCompletions;
									dbRec.HomeLongYards = gameLog.HomePassing.LongYards;
									dbRec.HomeMediumAttempts = (byte)gameLog.HomePassing.MediumAttempts;
									dbRec.HomeMediumCompletions = (byte)gameLog.HomePassing.MediumCompletions;
									dbRec.HomeMediumYards = gameLog.HomePassing.MediumYards;
									dbRec.HomeMiddleRushAttempts = (byte)gameLog.HomeRushing.MiddleAttempts;
									dbRec.HomeMiddleRushYards = gameLog.HomeRushing.MiddleYards;
									dbRec.HomeOtherAttempts = (byte)gameLog.HomePassing.OtherAttempts;
									dbRec.HomeOtherCompletions = (byte)gameLog.HomePassing.OtherCompletions;
									dbRec.HomeOtherYards = gameLog.HomePassing.OtherYards;
									dbRec.HomeOtherRushAttempts = (byte)gameLog.HomeRushing.OtherAttempts;
									dbRec.HomeOtherRushYards = gameLog.HomeRushing.OtherYards;
									dbRec.HomeRedZoneAttempts = (byte)gameLog.HomePossessions.RedZoneAttempts;
									dbRec.HomeRedZonefGs = (byte)gameLog.HomePossessions.RedZoneFieldGoals;
									dbRec.HomeRedZonetDs = (byte)gameLog.HomePossessions.RedZoneTouchdowns;
									dbRec.HomeRightRushAttempts = (byte)gameLog.HomeRushing.RightAttempts;
									dbRec.HomeRightRushYards = (byte)gameLog.HomeRushing.RightYards;
									dbRec.HomeScreenAttempts = (byte)gameLog.HomePassing.ScreenAttempts;
									dbRec.HomeScreenCompletions = (byte)gameLog.HomePassing.ScreenCompletions;
									dbRec.HomeScreenYards = gameLog.HomePassing.ScreenYards;
									dbRec.HomeShortAttempts = (byte)gameLog.HomePassing.ShortAttempts;
									dbRec.HomeShortCompletions = (byte)gameLog.HomePassing.ShortCompletions;
									dbRec.HomeShortYards = gameLog.HomePassing.ShortYards;
									dbRec.HomeTimeOfPossession = gameLog.HomePossessions.TimeOfPossession;

									dbRec.AwayLeftRushAttempts = (byte)gameLog.AwayRushing.LeftAttempts;
									dbRec.AwayLeftRushYards = gameLog.AwayRushing.LeftYards;
									dbRec.AwayLongAttempts = (byte)gameLog.AwayPassing.LongAttempts;
									dbRec.AwayLongCompletions = (byte)gameLog.AwayPassing.LongCompletions;
									dbRec.AwayLongYards = gameLog.AwayPassing.LongYards;
									dbRec.AwayMediumAttempts = (byte)gameLog.AwayPassing.MediumAttempts;
									dbRec.AwayMediumCompletions = (byte)gameLog.AwayPassing.MediumCompletions;
									dbRec.AwayMediumYards = gameLog.AwayPassing.MediumYards;
									dbRec.AwayMiddleRushAttempts = (byte)gameLog.AwayRushing.MiddleAttempts;
									dbRec.AwayMiddleRushYards = gameLog.AwayRushing.MiddleYards;
									dbRec.AwayOtherAttempts = (byte)gameLog.AwayPassing.OtherAttempts;
									dbRec.AwayOtherCompletions = (byte)gameLog.AwayPassing.OtherCompletions;
									dbRec.AwayOtherYards = gameLog.AwayPassing.OtherYards;
									dbRec.AwayOtherRushAttempts = (byte)gameLog.AwayRushing.OtherAttempts;
									dbRec.AwayOtherRushYards = gameLog.AwayRushing.OtherYards;
									dbRec.AwayRedZoneAttempts = (byte)gameLog.AwayPossessions.RedZoneAttempts;
									dbRec.AwayRedZonefGs = (byte)gameLog.AwayPossessions.RedZoneFieldGoals;
									dbRec.AwayRedZonetDs = (byte)gameLog.AwayPossessions.RedZoneTouchdowns;
									dbRec.AwayRightRushAttempts = (byte)gameLog.AwayRushing.RightAttempts;
									dbRec.AwayRightRushYards = (byte)gameLog.AwayRushing.RightYards;
									dbRec.AwayScreenAttempts = (byte)gameLog.AwayPassing.ScreenAttempts;
									dbRec.AwayScreenCompletions = (byte)gameLog.AwayPassing.ScreenCompletions;
									dbRec.AwayScreenYards = gameLog.AwayPassing.ScreenYards;
									dbRec.AwayShortAttempts = (byte)gameLog.AwayPassing.ShortAttempts;
									dbRec.AwayShortCompletions = (byte)gameLog.AwayPassing.ShortCompletions;
									dbRec.AwayShortYards = gameLog.AwayPassing.ShortYards;
									dbRec.AwayTimeOfPossession = gameLog.AwayPossessions.TimeOfPossession;

									dbRec.TotalCapacity = gameLog.TotalCapacity;
									dbRec.UpperDeckAttendance = gameLog.UpperDeckAttendance;
									dbRec.UpperDeckCapacity = gameLog.UpperDeckCapacity;
									dbRec.EndZoneAttendance = gameLog.EndZoneCapacity;
									dbRec.EndZoneCapacity = gameLog.EndZoneAttendance;
									dbRec.MezzanineAttendance = gameLog.MezzanineAttendance;
									dbRec.MezzanineCapacity = gameLog.MezzanineCapacity;
									dbRec.SidelineAttendance = gameLog.SidelineAttendance;
									dbRec.SidelineCapacity = gameLog.SidelineCapacity;
									dbRec.ClubAttendance = gameLog.ClubAttendance;
									dbRec.ClubCapacity = gameLog.ClubCapacity;
									dbRec.BoxAttendance = gameLog.BoxAttendance;
									dbRec.BoxCapacity = gameLog.BoxCapacity;

									break;
								}
							}

							break;
						}
					}

					session.Save(dbRec);
					++newCount;

					if (matchingGameLog != null)
					{
						foreach (LeagueData.GameDriveInfo homeDriveInfo in matchingGameLog.HomeDrives)
						{
							FofGamedrive newDrive = new FofGamedrive();

							newDrive.EndMinutes = (byte)homeDriveInfo.DriveEndMinutes;
							newDrive.EndQuarter = (byte)homeDriveInfo.DriveEndQuarter;
							newDrive.EndSeconds = (byte)homeDriveInfo.DriveEndSeconds;
							newDrive.Gameid = dbRec.Id;
							newDrive.PlayCount = (byte)homeDriveInfo.Plays;
							newDrive.StartMinutes = (byte)homeDriveInfo.DriveStartMinutes;
							newDrive.StartQuarter = (byte)homeDriveInfo.DriveStartQuarter;
							newDrive.StartSeconds = (byte)homeDriveInfo.DriveStartSeconds;
							newDrive.StartYardsFromGoal = (byte)homeDriveInfo.YardsFromGoalStart;
							newDrive.Team = (byte)matchingGameLog.HomeTeam.TeamIndex;
							newDrive.YardsGained = homeDriveInfo.YardsGained;
							newDrive.Result = (byte)homeDriveInfo.Result;

							session.Save(newDrive);
							++driveCount;
						}
						foreach (LeagueData.GameDriveInfo awayDriveInfo in matchingGameLog.AwayDrives)
						{
							FofGamedrive newDrive = new FofGamedrive();

							newDrive.EndMinutes = (byte)awayDriveInfo.DriveEndMinutes;
							newDrive.EndQuarter = (byte)awayDriveInfo.DriveEndQuarter;
							newDrive.EndSeconds = (byte)awayDriveInfo.DriveEndSeconds;
							newDrive.Gameid = dbRec.Id;
							newDrive.PlayCount = (byte)awayDriveInfo.Plays;
							newDrive.StartMinutes = (byte)awayDriveInfo.DriveStartMinutes;
							newDrive.StartQuarter = (byte)awayDriveInfo.DriveStartQuarter;
							newDrive.StartSeconds = (byte)awayDriveInfo.DriveStartSeconds;
							newDrive.StartYardsFromGoal = (byte)awayDriveInfo.YardsFromGoalStart;
							newDrive.Team = (byte)matchingGameLog.AwayTeam.TeamIndex;
							newDrive.YardsGained = awayDriveInfo.YardsGained;
							newDrive.Result = (byte)awayDriveInfo.Result;

							session.Save(newDrive);
							++driveCount;
						}
					}
				}
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " added, " + driveCount.ToString() + " drives added)" + System.Environment.NewLine);
		}

		private void DoFutureDraftsUpdates()
		{
			AddStatusString("     Writing future drafts...");

			IList<FofFutureDrafts> futureDrafts = session.CreateCriteria(typeof(FofFutureDrafts))
				.AddOrder(Order.Asc("_Year"))
				.AddOrder(Order.Asc("_Round"))
				.AddOrder(Order.Asc("_Pick"))
				.List<FofFutureDrafts>();

			transaction = session.BeginTransaction();

			int curDBIndex = 0;
			int newCount = 0;
			int updatedCount = 0;
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
						bool isNew = false;
						FofFutureDrafts dbDraft = null;
						if (curDBIndex >= futureDrafts.Count)
						{
							dbDraft = new FofFutureDrafts();
							isNew = true;
						}
						else
						{
							dbDraft = futureDrafts[curDBIndex++];
						}

						dbDraft.Year = (short)year;
						dbDraft.Round = (short)(round + 1);
						dbDraft.Pick = (short)(pick + 1);
						dbDraft.TeamID = teamIndex;

						if (isNew)
						{
							session.Save(dbDraft);
							++newCount;
						}
						else if (dbDraft.IsChanged)
						{
							session.Update(dbDraft);
							++updatedCount;
						}

						++pick;
					}
					++round;
				}
				++year;
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " new, " + updatedCount.ToString() + " updated)" + System.Environment.NewLine);
		}

		private void DoTeamScheduleUpdates()
		{
			AddStatusString("     Writing team schedules...");

			IList<FofTeamschedule> schedules = session.CreateCriteria(typeof(FofTeamschedule))
				.Add(Expression.Eq("_Year",mLeagueData.CurrentYear))
				.AddOrder(Order.Asc("_Teamid"))
				.AddOrder(Order.Asc("_Week"))
				.List<FofTeamschedule>();

			transaction = session.BeginTransaction();

			int curDBIndex = 0;
			int curFOFIndex = 0;
			int newCount = 0;
			int updatedCount = 0;
			while (curFOFIndex < mLeagueData.TeamScheduleGameRecords.Length)
			{
				bool isNew = false;
				LeagueData.TeamScheduleGameRecord gameRec = mLeagueData.TeamScheduleGameRecords[curFOFIndex];
				if (gameRec.Week < 999)
				{
					FofTeamschedule dbSchedule = null;
					if (curDBIndex >= schedules.Count || schedules[curDBIndex].Teamid > gameRec.TeamIndex)
					{
						dbSchedule = new FofTeamschedule();
						isNew = true;
					}
					else
					{
						dbSchedule = schedules[curDBIndex++];
					}

					int precip = gameRec.Weather / 7800;
					int windSpeed = ((gameRec.Weather % 7800) / 120);
					int temperature = ((gameRec.Weather % 120) - 10);

					dbSchedule.Attendance = gameRec.Attendance * 100;
					dbSchedule.Away = (byte)gameRec.Away;
					dbSchedule.ConferenceGame = (byte)Math.Min(gameRec.ConferenceGame,Byte.MaxValue);
					dbSchedule.DivisionGame = (byte)Math.Min(gameRec.DivisionGame, Byte.MaxValue);
					dbSchedule.Opponentid = (byte)gameRec.Opponent;
					dbSchedule.OppScore = (byte)gameRec.OppScore;
					dbSchedule.Precip = (byte)precip;
					dbSchedule.Score = (byte)gameRec.Score;
					dbSchedule.Teamid = (byte)gameRec.TeamIndex;
					dbSchedule.Temperature = (short)temperature;
					dbSchedule.Week = (byte)gameRec.Week;
					dbSchedule.Wind = (byte)windSpeed;
					dbSchedule.Year = mGameInfo.CurYear;

					if (isNew)
					{
						session.Save(dbSchedule);
						++newCount;
					}
					else if (dbSchedule.IsChanged)
					{
						session.Update(dbSchedule);
						++updatedCount;
					}
				}

				++curFOFIndex;
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " new, " + updatedCount.ToString() + " updated)" + System.Environment.NewLine);
		}

		private void DoTransactionUpdates()
		{
			int maxOldIndex = -1;
			if (!mStartFromScratch && (mLastYear == mGameInfo.CurYear))
			{
				IList<int> results = session.CreateCriteria(typeof(FofTransactions))
					.SetProjection(Projections.Max("_InSeasonIndex"))
					.Add(Expression.Eq("_Season", mGameInfo.CurYear))
					.List<int>();
				if (results.Count > 0)
				{
					maxOldIndex = results[0];
				}
			}

			AddStatusString("     Writing new transactions...");
			transaction = session.BeginTransaction();

			int newCount = 0;

			int yearIndex = mGameInfo.CurYear - mGameInfo.StartYear;
			for (int i = (maxOldIndex + 1); i < mLeagueData.Transactions[yearIndex].Length; ++i)
			{
				LeagueData.TransactionRecord curTrans = mLeagueData.Transactions[yearIndex][i];

				FofTransactions newTrans = new FofTransactions();
				newTrans.InSeasonIndex = i;
				newTrans.Position = curTrans.Position;
				newTrans.PrimaryIndex = curTrans.PlayerRec2Index;
				newTrans.Salary = curTrans.Salary;
				newTrans.Season = mGameInfo.CurYear;
				newTrans.Stage = curTrans.Stage;
				newTrans.Team1id = curTrans.Team1Index;
				newTrans.Team2id = curTrans.Team2Index;
				newTrans.Type = (byte)curTrans.TransactionType;
				newTrans.Years = curTrans.Years;

				if (newTrans.Type == 1 || newTrans.Type == 2 || newTrans.Type == 3 || newTrans.Type == 4 ||
					newTrans.Type == 6 || newTrans.Type == 7 || newTrans.Type == 8 || newTrans.Type == 9 || newTrans.Type == 10 ||
					newTrans.Type == 13 || newTrans.Type == 14 || newTrans.Type == 15 || newTrans.Type == 16 || newTrans.Type == 20 ||
					newTrans.Type == 21 || newTrans.Type == 22 || newTrans.Type == 23 || newTrans.Type == 24 || newTrans.Type == 25 || 
					newTrans.Type == 26 || newTrans.Type == 27 || newTrans.Type == 28
					)
				{
					newTrans.Playerid = mLeagueData.PlayerHistoricalRecords[curTrans.PlayerRec2Index].PlayerID;
				}
				else
				{
					newTrans.Playerid = 0;
				}

				// Determine injury length
				if (newTrans.Type == 28)
				{
					// Salary has injury severity for an injury transaction. Stuff player injury length into it.
					for (int activeIndex = 0; activeIndex < mLeagueData.PlayerActiveRecords.Length; ++activeIndex)
					{
						if (mLeagueData.PlayerActiveRecords[activeIndex].PlayerID == newTrans.Playerid)
						{
							int injuryLength = mLeagueData.PlayerActiveRecords[activeIndex].InjuryLength % 1000;
							newTrans.Salary += (injuryLength * 1000);
							break;
						}
					}
				}

				session.Save(newTrans);
				++newCount;
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " added)" + System.Environment.NewLine);
		}

		Tables.FofGameinfo mGameInfo;

		private void DoFranchiseScoresUpdate()
		{
			short maxSeason = 0;
			if (!mStartFromScratch)
			{
				object results = session.CreateCriteria(typeof(FofFranchise))
					.SetProjection(Projections.Max("_Year"))
					.UniqueResult();
				if (results != null)
				{
					maxSeason = Convert.ToInt16(results);
				}
			}

			int newCount = 0;
			AddStatusString("     Writing franchise performances...");
			transaction = session.BeginTransaction();

			for (int i = 0; i < mLeagueData.FranchisePerformanceRecords.Length; ++i)
			{
				DataReader.LeagueData.FranchisePerformanceRecord rec = mLeagueData.FranchisePerformanceRecords[i];
				if (rec.Year != mGameInfo.CurYear && rec.Year > maxSeason)
				{
					FofFranchise curFranchise = new FofFranchise();
					curFranchise.Advertising = rec.Advertising * 10000;
					curFranchise.Attendance = rec.Attendance * 100;
					curFranchise.Coaching = rec.Coaching * 10000;
					curFranchise.Concessions = rec.Concessions * 10000;
					curFranchise.ConfLoss = (byte)rec.ConfLoss;
					curFranchise.ConfTies = (byte)rec.ConfTies;
					curFranchise.ConfWins = (byte)rec.ConfWins;
					curFranchise.DivLoss = (byte)rec.DivLoss;
					curFranchise.DivTie = (byte)rec.DivTie;
					curFranchise.DivWin = (byte)rec.DivWin;
					curFranchise.FranchiseValue = (byte)rec.FranchiseValue;
					curFranchise.Losses = (byte)rec.Losses;
					curFranchise.Maintenance = rec.Maintenance * 10000;
					curFranchise.Parking = rec.Parking * 10000;
					curFranchise.PerformanceScore = (byte)rec.PerformanceScore;
					curFranchise.PlayerBonuses = rec.PlayerBonuses * 10000;
					curFranchise.PlayerSalaries = rec.PlayerSalaries * 10000;
					curFranchise.Playoffs = (byte)rec.Playoffs;
					curFranchise.PointsAgainst = rec.PointsAgainst;
					curFranchise.PointsFor = rec.PointsFor;
					curFranchise.ProfitScore = (byte)rec.ProfitScore;
					curFranchise.RosterScore = (byte)rec.RosterScore;
					curFranchise.Scouting = rec.Scouting * 10000;
					curFranchise.StadiumCapacity = rec.StadiumCapacity * 100;
					curFranchise.StadiumPayment = rec.StadiumPayment * 10000;
					curFranchise.SuiteRevenue = rec.SuiteRevenue * 10000;
					curFranchise.TeamIndex = (byte)(i % 32);
					curFranchise.TicketRevenue = rec.TicketRevenue * 10000;
					curFranchise.Ties = (byte)rec.Ties;
					curFranchise.Training = rec.Training * 10000;
					curFranchise.TvRevenue = rec.TVRevenue * 10000;
					curFranchise.Wins = (byte)rec.Wins;
					curFranchise.Year = rec.Year;

					session.Save(curFranchise);
					++newCount;
				}
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " added)" + System.Environment.NewLine);
		}

		private void DoTeamStadiumsUpdate()
		{
			AddStatusString("     Writing team stadiums...");
			transaction = session.BeginTransaction();

			IList<FofTeamstadium> stadiums = session.CreateCriteria(typeof(FofTeamstadium)).AddOrder(Order.Asc("_Teamid")).List<FofTeamstadium>();
			for (int i = 0; i < mLeagueData.TeamStadiumBlocks.Length; ++i)
			{
				bool isNewStadium = false;
				FofTeamstadium curStadium = null;
				if (i >= stadiums.Count)
				{
					curStadium = new FofTeamstadium();
					curStadium.Teamid = (short)i;
					isNewStadium = true;
				}
				else
				{
					curStadium = stadiums[i];
					isNewStadium = false;
				}

				curStadium.ClubSeats = mLeagueData.TeamStadiumBlocks[i].ClubSeats * 100;
				curStadium.ClubSeatsPrice = mLeagueData.TeamStadiumBlocks[i].ClubSeatsPrice;
				curStadium.ConstructionCapacity = mLeagueData.TeamStadiumBlocks[i].ConstructionCapacity * 100;
				curStadium.ConstructionClubSeats = mLeagueData.TeamStadiumBlocks[i].ConstructionClubSeats * 100;
				curStadium.ConstructionCompleteYear = mLeagueData.TeamStadiumBlocks[i].ConstructionCompletionYear;
				curStadium.ConstructionLuxuryBoxes = mLeagueData.TeamStadiumBlocks[i].ConstructionLuxuryBoxes;
				curStadium.ConstructionStadiumType = (byte)mLeagueData.TeamStadiumBlocks[i].ConstructionStadiumType;
				curStadium.ConstructionType = (byte)mLeagueData.TeamStadiumBlocks[i].ConstructionType;
				curStadium.EndZonePrice = mLeagueData.TeamStadiumBlocks[i].EndZonePrice;
				curStadium.FanLoyalty = (byte)mLeagueData.TeamStadiumBlocks[i].FanLoyalty;
				curStadium.LuxuryBoxes = mLeagueData.TeamStadiumBlocks[i].LuxuryBoxes;
				curStadium.LuxuryBoxPrice = mLeagueData.TeamStadiumBlocks[i].LuxuryBoxPrice * 1000;
				curStadium.MezzaninePrice = mLeagueData.TeamStadiumBlocks[i].MezzaninePrice;
				curStadium.PriorYearAttendance = mLeagueData.TeamStadiumBlocks[i].PriorYearAttendance * 100;
				curStadium.PublicSupport = (byte)mLeagueData.TeamStadiumBlocks[i].PublicSupportForStadium;
				curStadium.SidelinesPrice = mLeagueData.TeamStadiumBlocks[i].SidelinesPrice;
				curStadium.StadiumType = (byte)mLeagueData.TeamStadiumBlocks[i].StadiumType;
				curStadium.TotalCapacity = mLeagueData.TeamStadiumBlocks[i].TotalCapacity * 100;
				curStadium.UpperDeckPrice = mLeagueData.TeamStadiumBlocks[i].UpperDeckPrice;
				curStadium.YearStadiumBuilt = mLeagueData.TeamStadiumBlocks[i].YearStadiumBuilt;

				if (isNewStadium)
				{
					session.Save(curStadium);
				}
				else if (curStadium.IsChanged)
				{
					session.Update(curStadium);
				}
			}

			transaction.Commit();
			AddStatusString("Done!" + System.Environment.NewLine);
		}

		private void DoTeamUpdate()
		{
			AddStatusString("     Writing teams...");
			transaction = session.BeginTransaction();

			IList<FofTeams> teams = session.CreateCriteria(typeof(FofTeams)).AddOrder(Order.Asc("_Id")).List<FofTeams>();
			for (int i = 0; i < mLeagueData.TeamRecords.Length; ++i)
			{
				bool isNewTeam = false;
				FofTeams curTeam = null;
				if (i >= teams.Count)
				{
					curTeam = new FofTeams();
					curTeam.Id = (short)i;
					isNewTeam = true;
				}
				else
				{
					curTeam = teams[i];
					isNewTeam = false;
				}

				curTeam.Abbrev = mUniverseData.TeamCityAbbrev(i);
				curTeam.CapLossNextYear = mLeagueData.TeamRecords[i].CapLossNextYear * 10000;
				curTeam.CapLossThisYear = mLeagueData.TeamRecords[i].CapLossThisYear * 10000;
				curTeam.Cityid = mUniverseData.TeamRecords[i].CityIndex;
				curTeam.CityName = mUniverseData.TeamCityName(i);
				curTeam.Conference = (byte)mUniverseData.TeamRecords[i].ConferenceID;
				curTeam.Division = (byte)mUniverseData.TeamRecords[i].DivisionID;
				curTeam.Nickname = mUniverseData.TeamRecords[i].Name;

				if (isNewTeam)
				{
					session.Save(curTeam);
				}
				else if (curTeam.IsChanged)
				{
					session.Update(curTeam);
				}
			}

			transaction.Commit();
			AddStatusString("Done!" + System.Environment.NewLine);
		}

		private void DoCitiesUpdate()
		{
			if (mLastYear == mGameInfo.CurYear)
			{
				return;
			}

			int newCount = 0;
			int updatedCount = 0;

			AddStatusString("     Writing cities...");
			transaction = session.BeginTransaction();

			IList<FofCities> cities = session.CreateCriteria(typeof(FofCities)).AddOrder(Order.Asc("_Id")).List<FofCities>();
			for (int i = 0; i < mUniverseData.CityRecords.Length; ++i)
			{
				bool isNewCity = false;
				FofCities curCity = null;
				if (i >= cities.Count)
				{
					curCity = new FofCities();
					curCity.Id = i;
					isNewCity = true;
				}
				else
				{
					curCity = cities[i];
					isNewCity = false;
				}
				curCity.Abbrev = mUniverseData.CityRecords[i].Abbrev;
				curCity.AvgInc = mUniverseData.CityRecords[i].AverageIncome * 100;
				curCity.DecHi = (byte)mUniverseData.CityRecords[i].DecemberHigh;
				curCity.DecHum = (byte)mUniverseData.CityRecords[i].DecemberHumidity;
				curCity.DecLo = (byte)mUniverseData.CityRecords[i].DecemberLow;
				curCity.Elev = mUniverseData.CityRecords[i].Elevation;
				curCity.EntComp = (byte)mUniverseData.CityRecords[i].EntertainmentCompetiton;
				curCity.Growth = ((float)mUniverseData.CityRecords[i].GrowthRate - 100.0f) / 10.0f;
				curCity.Latitude = ((float)mUniverseData.CityRecords[i].Latitude / 100.0f);
				curCity.Longitude = ((float)mUniverseData.CityRecords[i].Longitude / 100.0f);
				curCity.Name = mUniverseData.CityRecords[i].Name;
				curCity.NinetyDegDays = mUniverseData.CityRecords[i].NinetyDegreeDays;
				curCity.Pop = mUniverseData.CityRecords[i].Population * 1000;
				curCity.PovLevel = ((float)mUniverseData.CityRecords[i].PovertyLevel / 10.0f);
				curCity.SeptHi = (byte)mUniverseData.CityRecords[i].SeptemberHigh;
				curCity.SeptHum = (byte)mUniverseData.CityRecords[i].SeptemberLow;
				curCity.SeptLo = (byte)mUniverseData.CityRecords[i].SeptemberHumidity;
				curCity.SnowDays = mUniverseData.CityRecords[i].SnowDays;
				curCity.State = (byte)mUniverseData.CityRecords[i].State;
				curCity.StormDays = mUniverseData.CityRecords[i].StormyDays;
				curCity.WantsTeam = (byte)mUniverseData.CityRecords[i].WantsNewTeam;

				if (isNewCity)
				{
					session.Save(curCity);
					++newCount;
				}
				else
				{
					session.Update(curCity);
					++updatedCount;
				}
			}

			transaction.Commit();
			AddStatusString("Done! (" + newCount.ToString() + " new, " + updatedCount.ToString() + " updated)" + System.Environment.NewLine);
		}

		private void DoHometownsUpdate()
		{
			if (!mStartFromScratch)
			{
				return;
			}

			AddStatusString("     Writing home towns...");
			transaction = session.BeginTransaction();

			Tables.FofHometowns[] hometowns = new Tables.FofHometowns[mUniverseData.HometownNames.Count];
			for (int i = 0; i < mUniverseData.HometownNames.Count; ++i)
			{
				hometowns[i] = new FofHometowns();
				hometowns[i].Id = (short)i;
				hometowns[i].Name = mUniverseData.HometownNames[i];
				session.Save(hometowns[i]);
			}

			transaction.Commit();
			AddStatusString("Done! (" + mUniverseData.HometownNames.Count.ToString() + " added)" + System.Environment.NewLine);
		}

		private void DoCollegesUpdate()
		{
			if (!mStartFromScratch)
			{
				return;
			}

			AddStatusString("     Writing colleges...");
			transaction = session.BeginTransaction();

			Tables.FofColleges[] colleges = new Tables.FofColleges[mUniverseData.CollegeNames.Length];
			for (int i = 0; i < mUniverseData.CollegeNames.Length; ++i)
			{
				colleges[i] = new FofColleges();
				colleges[i].Id = (short)i;
				colleges[i].Name = mUniverseData.CollegeNames[i];
				session.Save(colleges[i]);
			}

			transaction.Commit();
			AddStatusString("Done! (" + mUniverseData.CollegeNames.Length.ToString() + " added)" + System.Environment.NewLine);
		}

		private void DoInjuriesUpdate()
		{
			if (!mStartFromScratch)
			{
				return;
			}

			AddStatusString("     Writing injury mappings...");
			transaction = session.BeginTransaction();

			Tables.FofInjuries[] injuries = new Tables.FofInjuries[mUniverseData.InjuryRecords.Length];
			for (int i = 0; i < mUniverseData.InjuryRecords.Length; ++i)
			{
				injuries[i] = new FofInjuries();
				injuries[i].Id = (short)i;
				injuries[i].Name = mUniverseData.InjuryRecords[i].Name;
				session.Save(injuries[i]);
			}

			transaction.Commit();

			AddStatusString("Done!" + System.Environment.NewLine);
		}

		private void DoMappingsUpdate()
		{
			if (!mStartFromScratch && mOldSchemaVersion >= 7)
			{
				return;
			}

			AddStatusString("     Writing constant mappings...");
			transaction = session.BeginTransaction();

			FofMappings mappings = null;
			IList<FofMappings> mappingsList = null;
			if (!mStartFromScratch)
			{
				mappingsList = session.CreateCriteria(typeof(FofMappings)).AddOrder(Order.Asc("_Id")).List<FofMappings>();
			}
			for (int i = 0; i < mUniverseData.AbilityMap.Length; ++i)
			{
				if (mappingsList != null)
				{
					mappings = mappingsList[i];
				}
				else
				{
					mappings = new Tables.FofMappings();
					mappings.Id = (short)i;
				}
				mappings.Ability = mUniverseData.AbilityMap[i];
				if (i < mUniverseData.PositionMap.Length)
				{
					mappings.Position = mUniverseData.PositionMap[i];
				}
				else
				{
					mappings.Position = "";
				}
				if (i < mUniverseData.PositionGroupMap.Length)
				{
					mappings.PositionGroup = mUniverseData.PositionGroupMap[i];
				}
				else
				{
					mappings.PositionGroup = "";
				}
				if (i < mUniverseData.TransactionTypeMap.Length)
				{
					mappings.TransactionType = mUniverseData.TransactionTypeMap[i];
				}
				else
				{
					mappings.TransactionType = "";
				}
				if (i < mUniverseData.PrecipMap.Length)
				{
					mappings.Precipitation = mUniverseData.PrecipMap[i];
				}
				else
				{
					mappings.Precipitation = "";
				}
				if (i < mUniverseData.PlayerStatusMap.Length)
				{
					mappings.PlayerStatus = mUniverseData.PlayerStatusMap[i];
				}
				else
				{
					mappings.PlayerStatus = "";
				}
				if (i < mUniverseData.StaffRoleMap.Length)
				{
					mappings.StaffRole = mUniverseData.StaffRoleMap[i];
				}
				else
				{
					mappings.StaffRole = "";
				}
				if (i < mUniverseData.PlayoffsMap.Length)
				{
					mappings.Playoffs = mUniverseData.PlayoffsMap[i];
				}
				else
				{
					mappings.Playoffs = "";
				}
				if (i < mUniverseData.StadiumTypeMap.Length)
				{
					mappings.StadiumType = mUniverseData.StadiumTypeMap[i];
				}
				else
				{
					mappings.StadiumType = "";
				}
				if (i < mUniverseData.ConstructionTypeMap.Length)
				{
					mappings.ConstructionType = mUniverseData.ConstructionTypeMap[i];
				}
				else
				{
					mappings.ConstructionType = "";
				}
				if (i < mUniverseData.DriveResultMap.Length)
				{
					mappings.DriveResult = mUniverseData.DriveResultMap[i];
				}
				else
				{
					mappings.DriveResult = "";
				}

				if (mappingsList != null)
				{
					session.Update(mappings);
				}
				else
				{
					session.Save(mappings);
				}
			}
			transaction.Commit();

			AddStatusString("Done!" + System.Environment.NewLine);
		}

		private void DoStageNamesUpdate()
		{
			if (!mStartFromScratch && mOldSchemaVersion >= 3)
			{
				return;
			}

			AddStatusString("     Writing stage names...");
			transaction = session.BeginTransaction();

			for (int i = 0; i < mFOFStageNames.Count; ++i)
			{
				session.Save(mFOFStageNames[i]);
			}

			transaction.Commit();

			AddStatusString("Done!" + System.Environment.NewLine);
		}

		private void DoStageUpdate()
		{
			int tmpStage = CalcGameStage(mLeagueData.GameStage, mLeagueData.CurrentWeek, mLeagueData.FAStage);
			mCurrentGameStageIndex = mGameStageDataToStageIndex[tmpStage];
			mLastGameStageIndex = QueryLastGameStageIndex();

			if (mStartFromScratch || mLastYear < mLeagueData.CurrentYear)
			{
				mLastGameStageIndex = -1;
			}

			mGameInfo.CurYear = mLeagueData.CurrentYear;
			mGameInfo.FaStage = (byte)mLeagueData.FAStage;
			mGameInfo.MinSalary = mLeagueData.MinSalary * 10000;
			mGameInfo.PlayerTeam = (byte)mLeagueData.PlayersTeam;
			mGameInfo.SalaryCap = mLeagueData.SalaryCap * 10000;
			mGameInfo.Stage = (byte)mLeagueData.GameStage;
			mGameInfo.StartYear = mLeagueData.StartingYear;
			mGameInfo.TeamCount = (byte)mLeagueData.NumberOfTeams;
			mGameInfo.Week = (byte)mLeagueData.CurrentWeek;
			mGameInfo.SchemaVersion = (short)mNewSchemaVersion;

			transaction = session.BeginTransaction();
			session.SaveOrUpdate(mGameInfo);
			transaction.Commit();

			AddStatusString("     Game Stage set to " + mGameInfo.CurYear.ToString() + " " +
				mStageNames[mCurrentGameStageIndex] + System.Environment.NewLine);
		}

		private int QueryLastGameStageIndex()
		{
			IList<FofGameinfo> currentGameInfo = session.CreateCriteria(typeof(FofGameinfo)).List<FofGameinfo>();
			if (currentGameInfo.Count > 0)
			{
				mGameInfo = currentGameInfo[0];
			}
			else
			{
				mGameInfo = new FofGameinfo();
				return -1;
			}

			mLastYear = mGameInfo.CurYear;
			int stage = mGameInfo.Stage;
			int week = mGameInfo.Week;
			int fastage = mGameInfo.FaStage;
			int tmpStage = CalcGameStage(stage, week, fastage);
			return mGameStageDataToStageIndex[tmpStage];
		}

		private NHibernate.Cfg.Configuration cfg;
		ISessionFactory factory;
		ISession session;
		ITransaction transaction;

		private void OpenNHibernateConnection()
		{
			MySql.Data.MySqlClient.MySqlConnectionStringBuilder connectionBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
			connectionBuilder.Server = textBoxServer.Text;
			connectionBuilder.Database = textBoxDatabase.Text;
			connectionBuilder.UserID = textBoxUser.Text;
			connectionBuilder.Password = textBoxPassword.Text;
			connectionBuilder.Port = UInt32.Parse(textBoxPort.Text);

			cfg = new NHibernate.Cfg.Configuration();
			cfg.Properties.Add("hibernate.dialect", "NHibernate.Dialect.MySQL5Dialect");
			cfg.Properties.Add("hibernate.connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			cfg.Properties.Add("hibernate.connection.driver_class", "NHibernate.Driver.MySqlDataDriver");
			cfg.Properties.Add("hibernate.connection.connection_string", connectionBuilder.ConnectionString);
			cfg.AddAssembly(System.Reflection.Assembly.GetExecutingAssembly());

			factory = cfg.BuildSessionFactory();
			session = factory.OpenSession();

			AddStatusString("     DB connection opened." + System.Environment.NewLine);
		}

		private void CloseNHibernateConnection()
		{
			session.Close();
			AddStatusString("     DB connection closed." + System.Environment.NewLine);
		}

		private MySql.Data.MySqlClient.MySqlConnection mDBConnection = null;
		private bool mStartFromScratch = false;
		private bool mDBConnectionIsGood = false;

		private void CloseDatabase()
		{
			if (mDBConnection != null)
			{
				mDBConnection.Close();
				mDBConnection = null;
				AddStatusString("     MySQL connection closed." + System.Environment.NewLine);
			}
		}

		private void CheckDatabase()
		{
			mStartFromScratch = false;
			mDBConnectionIsGood = false;
			mOldSchemaVersion = -1;
			mNewSchemaVersion = 8;

			MySql.Data.MySqlClient.MySqlConnectionStringBuilder connectionBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
			connectionBuilder.Server = textBoxServer.Text;
			connectionBuilder.Database = textBoxDatabase.Text;
			connectionBuilder.UserID = textBoxUser.Text;
			connectionBuilder.Password = textBoxPassword.Text;
			connectionBuilder.Port = UInt32.Parse(textBoxPort.Text);

			mDBConnection = new MySql.Data.MySqlClient.MySqlConnection(connectionBuilder.ConnectionString);
			try
			{
				mDBConnection.Open();
			}
			catch (System.InvalidOperationException e)
			{
				AddStatusString("Invalid MySQL connection: " + e.ToString() + System.Environment.NewLine);
			}
			catch (MySql.Data.MySqlClient.MySqlException e)
			{
				AddStatusString("Could not connect to MySQL server: " + e.ToString() + System.Environment.NewLine);
			}

			if (mDBConnection.State == ConnectionState.Open)
			{
				try
				{
					AddStatusString("     Connected to MySQL server " + textBoxServer.Text + " version: " + mDBConnection.ServerVersion + System.Environment.NewLine);

					MySql.Data.MySqlClient.MySqlCommand sqlCommand = new MySql.Data.MySqlClient.MySqlCommand("show tables like 'fof_gameinfo'", mDBConnection);
					MySql.Data.MySqlClient.MySqlDataReader sqlReader = sqlCommand.ExecuteReader();
					bool tableExists = sqlReader.HasRows;
					sqlReader.Close();
					if (tableExists)
					{
						mStartFromScratch = false;
						sqlCommand.CommandText = "select SchemaVersion from fof_gameinfo";
						object result = sqlCommand.ExecuteScalar();
						if (result != null)
						{
							mOldSchemaVersion = Convert.ToInt32(result);

							for (int i = 2; i <= mNewSchemaVersion; ++i)
							{
								CheckForSchemaUpdate(i,sqlCommand);
							}
						}
					}
					else
					{
						mStartFromScratch = true;
						AddStatusString("     MySQL server does not have correct tables, adding now...");

						using (System.IO.StreamReader inFile = new System.IO.StreamReader("DBCreate.sql"))
						{
							sqlCommand.CommandText = inFile.ReadToEnd();
						}
						sqlCommand.ExecuteNonQuery();

						AddStatusString("Done!" + System.Environment.NewLine);
					}
					mDBConnectionIsGood = true;
				}
				catch (System.Exception e)
				{
					AddStatusString("Error while processing SQL query: " + e.ToString());
				}
				CloseDatabase();
			}
			else
			{
				AddStatusString("Failed to connect to MySQL server, stopping" + System.Environment.NewLine);
			}
		}

		private void CheckForSchemaUpdate(int versionToCheck, MySql.Data.MySqlClient.MySqlCommand sqlCommand)
		{
			if (mOldSchemaVersion < versionToCheck)
			{
				AddStatusString("     MySQL server has schema " + mOldSchemaVersion.ToString() + ", updating to version " + versionToCheck.ToString() + "...");

				string fileName = "V" + versionToCheck.ToString() + "Schema.sql";
				using (System.IO.StreamReader inFile = new System.IO.StreamReader(fileName))
				{
					sqlCommand.CommandText = inFile.ReadToEnd();
				}
				sqlCommand.ExecuteNonQuery();

				AddStatusString("Done!" + System.Environment.NewLine);
			}
		}

		private System.Collections.Generic.Dictionary<int, int> mGameStageDataToStageIndex;
		private System.Collections.Generic.Dictionary<int, int> mTransactionStageIDToStageIndex;

		private List<FofStagenames> mFOFStageNames;

		private void CreateMaps()
		{
			mGameStageDataToStageIndex = new Dictionary<int, int>();
			mTransactionStageIDToStageIndex = new Dictionary<int, int>();
			mFOFStageNames = new List<FofStagenames>();
			FofStagenames newStage;

			int stageIndex = 0;
			// Offseason
			newStage = new FofStagenames();
			newStage.GameInfofaStage = 0;
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 0;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 999;
			mFOFStageNames.Add(newStage);
			mTransactionStageIDToStageIndex.Add(999, stageIndex++);
			// Staff Hiring 1
			newStage = new FofStagenames();
			newStage.GameInfoStage = 10;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1001;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(100101, stageIndex);
			mTransactionStageIDToStageIndex.Add(1001, stageIndex++);
			// Staff Hiring 2
			newStage = new FofStagenames();
			newStage.GameInfoStage = 10;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 2;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1002;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(100102, stageIndex);
			mTransactionStageIDToStageIndex.Add(1002, stageIndex++);
			// Staff Hiring 3
			newStage = new FofStagenames();
			newStage.GameInfoStage = 10;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 3;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1003;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(100103, stageIndex);
			mTransactionStageIDToStageIndex.Add(1003, stageIndex++);
			// Ticket Prices
			newStage = new FofStagenames();
			newStage.GameInfoStage = 4;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 400;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(040101, stageIndex);
			mTransactionStageIDToStageIndex.Add(400, stageIndex++);
			// FA 1
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 501;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050101, stageIndex);
			mTransactionStageIDToStageIndex.Add(501, stageIndex++);
			// FA 2
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 2;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 502;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050102, stageIndex);
			mTransactionStageIDToStageIndex.Add(502, stageIndex++);
			// FA 3
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 3;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 503;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050103, stageIndex);
			mTransactionStageIDToStageIndex.Add(503, stageIndex++);
			// FA 4
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 4;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 504;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050104, stageIndex);
			mTransactionStageIDToStageIndex.Add(504, stageIndex++);
			// FA 5
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 5;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 505;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050105, stageIndex);
			mTransactionStageIDToStageIndex.Add(505, stageIndex++);
			// FA 6
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 506;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050106, stageIndex);
			mTransactionStageIDToStageIndex.Add(506, stageIndex++);
			// FA 7
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 7;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 507;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050107, stageIndex);
			mTransactionStageIDToStageIndex.Add(507, stageIndex++);
			// FA 8
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 8;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 508;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050108, stageIndex);
			mTransactionStageIDToStageIndex.Add(508, stageIndex++);
			// FA 9
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 9;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 509;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050109, stageIndex);
			mTransactionStageIDToStageIndex.Add(509, stageIndex++);
			// FA 10
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 10;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 510;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050110, stageIndex);
			mTransactionStageIDToStageIndex.Add(510, stageIndex++);
			// FA 11
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 11;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 511;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050111, stageIndex);
			mTransactionStageIDToStageIndex.Add(511, stageIndex++);
			// FA 12
			newStage = new FofStagenames();
			newStage.GameInfoStage = 5;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 12;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 512;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(050112, stageIndex);
			mTransactionStageIDToStageIndex.Add(512, stageIndex++);
			// Begin Draft
			newStage = new FofStagenames();
			newStage.GameInfoStage = 9;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 13;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 0;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(090113, stageIndex++);
			// Draft
			newStage = new FofStagenames();
			newStage.GameInfoStage = 6;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 13;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 700;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(060113, stageIndex);
			mTransactionStageIDToStageIndex.Add(700, stageIndex++);
			// Draft Complete
			newStage = new FofStagenames();
			newStage.GameInfoStage = 7;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 13;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 0;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(070113, stageIndex++);
			// Late FA 1
			newStage = new FofStagenames();
			newStage.GameInfoStage = 13;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1301;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(130101, stageIndex);
			mTransactionStageIDToStageIndex.Add(1301, stageIndex++);
			// Late FA 2
			newStage = new FofStagenames();
			newStage.GameInfoStage = 13;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 2;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1302;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(130102, stageIndex);
			mTransactionStageIDToStageIndex.Add(1302, stageIndex++);
			// Late FA 3
			newStage = new FofStagenames();
			newStage.GameInfoStage = 13;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 3;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1303;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(130103, stageIndex);
			mTransactionStageIDToStageIndex.Add(1303, stageIndex++);
			// Late FA 4
			newStage = new FofStagenames();
			newStage.GameInfoStage = 13;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 4;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1304;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(130104, stageIndex);
			mTransactionStageIDToStageIndex.Add(1304, stageIndex++);
			// Late FA 5
			newStage = new FofStagenames();
			newStage.GameInfoStage = 13;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 5;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1305;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(130105, stageIndex);
			mTransactionStageIDToStageIndex.Add(1305, stageIndex++);
			// Training Camp
			newStage = new FofStagenames();
			newStage.GameInfoStage = 12;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1200;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(120101, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 12;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1200;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(120106, stageIndex);
			mTransactionStageIDToStageIndex.Add(1200, stageIndex++);
			// Preseason week 1
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000101, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 1;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 1;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000106, stageIndex);
			mTransactionStageIDToStageIndex.Add(1, stageIndex++);
			// Preseason week 2
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 2;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 2;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000201, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 2;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 2;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000206, stageIndex);
			mTransactionStageIDToStageIndex.Add(2, stageIndex++);
			// Preseason week 3
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 3;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 3;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000301, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 3;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 3;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000306, stageIndex);
			mTransactionStageIDToStageIndex.Add(3, stageIndex++);
			// Preseason week 4
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 4;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 4;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000401, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 4;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 4;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000406, stageIndex);
			mTransactionStageIDToStageIndex.Add(4, stageIndex++);
			// Preseason week 5
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 5;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 5;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000501, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 0;
			newStage.GameInfoWeek = 5;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 5;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(000506, stageIndex);
			mTransactionStageIDToStageIndex.Add(5, stageIndex++);
			// Week 1
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 6;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 6;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010601, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 6;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 6;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010606, stageIndex);
			mTransactionStageIDToStageIndex.Add(6, stageIndex++);
			// Week 2
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 7;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 7;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010701, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 7;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 7;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010706, stageIndex);
			mTransactionStageIDToStageIndex.Add(7, stageIndex++);
			// Week 3
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 8;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 8;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010801, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 8;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 8;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010806, stageIndex);
			mTransactionStageIDToStageIndex.Add(8, stageIndex++);
			// Week 4
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 9;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 9;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010901, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 9;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 9;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(010906, stageIndex);
			mTransactionStageIDToStageIndex.Add(9, stageIndex++);
			// Week 5
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 10;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 10;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(011001, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 1;
			newStage.GameInfoWeek = 10;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 10;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(011006, stageIndex);
			mTransactionStageIDToStageIndex.Add(10, stageIndex++);
			// Week 6
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 11;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 11;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021101, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 11;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 11;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021106, stageIndex);
			mTransactionStageIDToStageIndex.Add(11, stageIndex++);
			// Week 7
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 12;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 12;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021201, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 12;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 12;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021206, stageIndex);
			mTransactionStageIDToStageIndex.Add(12, stageIndex++);
			// Week 8
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 13;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 13;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021301, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 13;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 13;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021306, stageIndex);
			mTransactionStageIDToStageIndex.Add(13, stageIndex++);
			// Week 9
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 14;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 14;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021401, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 14;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 14;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021406, stageIndex);
			mTransactionStageIDToStageIndex.Add(14, stageIndex++);
			// Week 10
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 15;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 15;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021501, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 15;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 15;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021506, stageIndex);
			mTransactionStageIDToStageIndex.Add(15, stageIndex++);
			// Week 11
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 16;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 16;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021601, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 16;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 16;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021606, stageIndex);
			mTransactionStageIDToStageIndex.Add(16, stageIndex++);
			// Week 12
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 17;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 17;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021701, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 17;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 17;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021706, stageIndex);
			mTransactionStageIDToStageIndex.Add(17, stageIndex++);
			// Week 13
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 18;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 18;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021801, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 18;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 18;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021806, stageIndex);
			mTransactionStageIDToStageIndex.Add(18, stageIndex++);
			// Week 14
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 19;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 19;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021901, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 19;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 19;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(021906, stageIndex);
			mTransactionStageIDToStageIndex.Add(19, stageIndex++);
			// Week 15
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 20;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 20;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022001, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 20;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 20;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022006, stageIndex);
			mTransactionStageIDToStageIndex.Add(20, stageIndex++);
			// Week 16
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 21;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 21;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022101, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 21;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 21;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022106, stageIndex);
			mTransactionStageIDToStageIndex.Add(21, stageIndex++);
			// Week 17
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 22;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 22;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022201, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 22;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 22;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022206, stageIndex);
			mTransactionStageIDToStageIndex.Add(22, stageIndex++);
			// Wildcard
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 23;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 23;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022301, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 23;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 23;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022306, stageIndex);
			mTransactionStageIDToStageIndex.Add(23, stageIndex++);
			// Division
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 24;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 24;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022401, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 24;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 24;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022406, stageIndex);
			mTransactionStageIDToStageIndex.Add(24, stageIndex++);
			// Conference
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 25;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 25;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022501, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 25;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 25;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022506, stageIndex);
			mTransactionStageIDToStageIndex.Add(25, stageIndex++);
			// Bowl
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 26;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 26;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022601, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 2;
			newStage.GameInfoWeek = 26;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 26;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(022606, stageIndex);
			mTransactionStageIDToStageIndex.Add(26, stageIndex++);
			// End Season
			newStage = new FofStagenames();
			newStage.GameInfoStage = 3;
			newStage.GameInfoWeek = 27;
			newStage.GameInfofaStage = 1;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 0;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(032701, stageIndex);
			newStage = new FofStagenames();
			newStage.GameInfoStage = 3;
			newStage.GameInfoWeek = 27;
			newStage.GameInfofaStage = 6;
			newStage.StageIndex = (byte)stageIndex;
			newStage.StageName = mStageNames[stageIndex];
			newStage.TransactionStage = 0;
			mFOFStageNames.Add(newStage);
			mGameStageDataToStageIndex.Add(032706, stageIndex);
		}

		private string[] mStageNames = 
			{
				"Offseason",
				"Staff Hiring 1",
				"Staff Hiring 2",
				"Staff Hiring 3",
				"Ticket Prices",
				"FA 1",
				"FA 2",
				"FA 3",
				"FA 4",
				"FA 5",
				"FA 6",
				"FA 7",
				"FA 8",
				"FA 9",
				"FA 10",
				"FA 11",
				"FA 12",
				"Begin Draft",
				"Draft",
				"Draft Complete",
				"Late FA1",
				"Late FA2",
				"Late FA3",
				"Late FA4",
				"Late FA5",
				"Training Camp",
				"Preseason Week 1",
				"Preseason Week 2",
				"Preseason Week 3",
				"Preseason Week 4",
				"Preseason Week 5",
				"Week 1",
				"Week 2",
				"Week 3",
				"Week 4",
				"Week 5",
				"Week 6",
				"Week 7",
				"Week 8",
				"Week 9",
				"Week 10",
				"Week 11",
				"Week 12",
				"Week 13",
				"Week 14",
				"Week 15",
				"Week 16",
				"Week 17",
				"Wildcard",
				"Division",
				"Conference",
				"Bowl",
				"Season End"
			};
	}
}
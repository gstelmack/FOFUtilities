using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataReader
{
	public class LeagueData
	{
		public const int LoadCurrentSeasonOnly = -1;
		public const int LoadAllSeasons = 0;
		public delegate void FileReadDelegate(string filename);
		public FileReadDelegate FileReadCallback = null;
		static public bool DumpInfoPlays = false;
		static public bool DumpOnsideKickPlays = false;
		static public bool DumpPuntPlays = false;
		static public bool DumpFieldGoalPlays = false;
		static public bool DumpRunPlays = false;
		static public bool DumpPassPlays = false;

		public LeagueData(string pathPrefix,UniverseData universeData, int startingSeason, FileReadDelegate readCallback, bool loadGameList)
		{
			FileReadCallback = readCallback;
			mUniverseData = universeData;
			mStartingSeason = startingSeason;
			LoadLeagueFile(Path.ChangeExtension(pathPrefix, ".fju"));
			LoadPlayerFile(Path.ChangeExtension(pathPrefix, ".fpd"));
			LoadSeasonData(pathPrefix);
			if (loadGameList)
			{
				LoadGameList(pathPrefix);
			}
		}

		private class PlayerActiveSorter : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				PlayerActiveRecord i1 = (PlayerActiveRecord)x;
				PlayerActiveRecord i2 = (PlayerActiveRecord)y;

				return i1.PlayerID.CompareTo(i2.PlayerID);
			}
		}

		private class PlayerHistoricalSorter : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				PlayerHistoricalRecord i1 = (PlayerHistoricalRecord)x;
				PlayerHistoricalRecord i2 = (PlayerHistoricalRecord)y;

				return i1.PlayerID.CompareTo(i2.PlayerID);
			}
		}

		private class PlayerGameStatSorter : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				PlayerGameStatsRecord i1 = (PlayerGameStatsRecord)x;
				PlayerGameStatsRecord i2 = (PlayerGameStatsRecord)y;

				if (i1.PlayerID == i2.PlayerID)
				{
					return i1.Week.CompareTo(i2.Week);
				}
				else
				{
					return i1.PlayerID.CompareTo(i2.PlayerID);
				}
			}
		}

		public void SortPlayerArrays()
		{
			Array.Sort(mPlayerActiveRecords, new PlayerActiveSorter());
			Array.Sort(mPlayerHistoricalRecords, new PlayerHistoricalSorter());
			Array.Sort(mPlayerGameStatsRecords[mSeasonsPlayed - 1], new PlayerGameStatSorter());
		}

		private void LoadPlayerFile(string fileName)
		{
			if (FileReadCallback != null)
			{
				FileReadCallback(System.IO.Path.GetFileName(fileName));
			}

			System.IO.FileStream inStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

			//BinaryHelper.SetupTracer(Path.GetFileName(fileName));

			BinaryHelper.ExtractString(inFile, 16, "Header");

			// Skip the encrypted free agent records
			long skipAmount = mFreeAgentRecordCount * 1696;
			inStream.Seek(skipAmount, SeekOrigin.Current);

			BinaryHelper.TracerWriteLine("Active Players");
			BinaryHelper.TracerIndent();
			mPlayerActiveRecords = new PlayerActiveRecord[mActivePlayerCount];
			for (int playerIndex = 0; playerIndex < mActivePlayerCount; playerIndex++)
			{
				BinaryHelper.TracerWriteLine("Player " + playerIndex);
				BinaryHelper.TracerIndent();

				mPlayerActiveRecords[playerIndex] = new PlayerActiveRecord();
				mPlayerActiveRecords[playerIndex].PlayerID = BinaryHelper.ReadInt32(inFile,"PlayerID");
				mPlayerActiveRecords[playerIndex].Position = BinaryHelper.ReadInt16(inFile,"Position");
				mPlayerActiveRecords[playerIndex].Experience = BinaryHelper.ReadInt16(inFile,"Experience");
				mPlayerActiveRecords[playerIndex].Number = BinaryHelper.ReadInt16(inFile,"Number");
				mPlayerActiveRecords[playerIndex].PositionGroup = BinaryHelper.ReadInt16(inFile,"PositionGroup");
				mPlayerActiveRecords[playerIndex].Team = BinaryHelper.ReadInt16(inFile,"Team");
				mPlayerActiveRecords[playerIndex].InjuryLength = BinaryHelper.ReadInt16(inFile,"InjuryLength");
				BinaryHelper.ProbeBytes(inFile, 2);
				mPlayerActiveRecords[playerIndex].Loyalty = BinaryHelper.ReadInt16(inFile,"Loyalty");
				mPlayerActiveRecords[playerIndex].PlaysToWin = BinaryHelper.ReadInt16(inFile,"PlaysToWin");
				BinaryHelper.ProbeBytes(inFile, 2);
				mPlayerActiveRecords[playerIndex].Personality = BinaryHelper.ReadInt16(inFile, "Personality");
				mPlayerActiveRecords[playerIndex].Leadership = BinaryHelper.ReadInt16(inFile,"Leadership");
				mPlayerActiveRecords[playerIndex].Intelligence = BinaryHelper.ReadInt16(inFile,"Intelligence");
				mPlayerActiveRecords[playerIndex].RedFlagMarker = BinaryHelper.ReadInt16(inFile,"RedFlagMarker");
				BinaryHelper.ProbeBytes(inFile, 4);
				mPlayerActiveRecords[playerIndex].Volatility = BinaryHelper.ReadInt16(inFile, "Volatility");
				BinaryHelper.ProbeBytes(inFile, 14);
				mPlayerActiveRecords[playerIndex].JoinedTeam = BinaryHelper.ReadInt16(inFile, "JoinedTeam");
				mPlayerActiveRecords[playerIndex].UFAYear = BinaryHelper.ReadInt16(inFile, "UFAYear");
				mPlayerActiveRecords[playerIndex].Popularity = BinaryHelper.ReadInt16(inFile, "Popularity");
				BinaryHelper.ProbeBytes(inFile, 8);
				mPlayerActiveRecords[playerIndex].ContractLength = BinaryHelper.ReadInt16(inFile, "ContractLength");
				BinaryHelper.ProbeBytes(inFile, 6);
				mPlayerActiveRecords[playerIndex].Salary = new int[MaxContractYears];
				int contractIndex;
				for (contractIndex = 0; contractIndex < MaxContractYears; contractIndex++)
				{
					mPlayerActiveRecords[playerIndex].Salary[contractIndex] = BinaryHelper.ReadCodedInt32(inFile, "Salary" + contractIndex.ToString());
				}
				mPlayerActiveRecords[playerIndex].Bonus = new int[MaxContractYears];
				for (contractIndex = 0; contractIndex < MaxContractYears; contractIndex++)
				{
					mPlayerActiveRecords[playerIndex].Bonus[contractIndex] = BinaryHelper.ReadCodedInt32(inFile, "Bonus" + contractIndex.ToString());
				}
				BinaryHelper.ProbeBytes(inFile, 30);

				BinaryHelper.TracerWriteLine("Interview Markers");
				BinaryHelper.TracerIndent();
				mPlayerActiveRecords[playerIndex].InterviewMarkers = new short[kTeamCount];
				for (int teamIndex = 0; teamIndex < kTeamCount; teamIndex++)
				{
					mPlayerActiveRecords[playerIndex].InterviewMarkers[teamIndex] = BinaryHelper.ReadInt16(inFile, "Team" + teamIndex.ToString());
				}
				BinaryHelper.TracerOutdent();

				BinaryHelper.ProbeBytes(inFile, 4);

				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.ProbeBytes(inFile, 500);

			BinaryHelper.ClearTracer();
			inFile.Close();

			// Correct active player index to player id for the current season game stats.
			int yearIndex = mSeasonsPlayed - 1;
			for (int playerGameIndex = 0; playerGameIndex < mPlayerGameStatsRecords[yearIndex].Length; ++playerGameIndex)
			{
				int activePlayerIndex = mPlayerGameStatsRecords[yearIndex][playerGameIndex].PlayerID;
				mPlayerGameStatsRecords[yearIndex][playerGameIndex].PlayerID = mPlayerActiveRecords[activePlayerIndex].PlayerID;
			}
		}

		private void LoadLeagueFile(string fileName)
		{
			if (FileReadCallback != null)
			{
				FileReadCallback(System.IO.Path.GetFileName(fileName));
			}

			System.IO.FileStream inStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

			//BinaryHelper.SetupTracer(Path.GetFileName(fileName));

			BinaryHelper.ExtractString(inFile, 16, "Header");

			mCurrentYear = BinaryHelper.ReadInt16(inFile,"Current Year");
			mStartingYear = mCurrentYear;
			if (mStartingSeason == LoadCurrentSeasonOnly)
			{
				mStartingSeason = mStartingYear;
			}
			else if (mStartingSeason < 0)
			{
				mStartingSeason = mStartingYear + mStartingSeason + 1;
			}
			mGameStage = BinaryHelper.ReadInt16(inFile, "Game Stage");
			mCurrentWeek = BinaryHelper.ReadInt16(inFile, "Current Week");
			mFAStage = BinaryHelper.ReadInt16(inFile, "FA Stage");
			BinaryHelper.ProbeBytes(inFile, 6);
			mPlayersTeam = BinaryHelper.ReadInt16(inFile, "Player's Team");
			mNumberOfTeams = BinaryHelper.ReadInt16(inFile, "Team Count");
			BinaryHelper.ProbeBytes(inFile, 4);
			BinaryHelper.TracerIndent();
			for (int teamControl = 0; teamControl < NumberOfTeams; teamControl++)
			{
				BinaryHelper.TracerWriteLine("Team " + teamControl + " control");
				BinaryHelper.TracerIndent();
				mTeamRecords[teamControl] = new TeamRecord();
				short humanControlled = BinaryHelper.ReadInt16(inFile,"Human Controlled");
				if (humanControlled == 0)
				{
					mTeamRecords[teamControl].HumanControlled = false;
				}
				else
				{
					mTeamRecords[teamControl].HumanControlled = true;
				}
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();
			mNextPlayerID = BinaryHelper.ReadInt32(inFile, "Next Player ID");
			BinaryHelper.ProbeBytes(inFile,134);
			mSalaryCap = BinaryHelper.ReadCodedInt32(inFile, "Salary Cap");
			mMinSalary = BinaryHelper.ReadCodedInt32(inFile, "Min Salary");
			BinaryHelper.ProbeBytes(inFile, 8);

			BinaryHelper.TracerIndent();
			int i;
			for (i = 0; i < kTeamCount; i++)
			{
				BinaryHelper.TracerWriteLine("Team " + i + " Cap Loss");
				BinaryHelper.TracerIndent();
				mTeamRecords[i].CapLossThisYear = BinaryHelper.ReadCodedInt32(inFile, "CapLossThisYear");
				mTeamRecords[i].CapLossNextYear = BinaryHelper.ReadCodedInt32(inFile, "CapLossNextYear");
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.ProbeBytes(inFile, 108);

			BinaryHelper.TracerIndent();
			for (int year = 0; year < 4; year++)
			{
				BinaryHelper.TracerWriteLine("Draft Year " + year);
				mDraftYears[year] = new DraftYear();
				mDraftYears[year].DraftRounds = new DraftRound[7];
				BinaryHelper.TracerIndent();
				for (int round = 0; round < 7; round++)
				{
					BinaryHelper.TracerWriteLine("Draft Round " + round);
					mDraftYears[year].DraftRounds[round] = new DraftRound();
					mDraftYears[year].DraftRounds[round].PickTeam = new short[kTeamCount];
					BinaryHelper.TracerIndent();
					for (int pick = 0; pick < kTeamCount; pick++)
					{
						mDraftYears[year].DraftRounds[round].PickTeam[pick] = BinaryHelper.ReadInt16(inFile, "PickTeam");
					}
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.TracerWriteLine("Reading transactions...");
			BinaryHelper.TracerIndent();
			TransactionRecord[] curTransactions = new TransactionRecord[kTransactionRecordCount];
			for (int transIndex = 0; transIndex < kTransactionRecordCount; transIndex++)
			{
				BinaryHelper.TracerWriteLine("Transaction " + transIndex);
				BinaryHelper.TracerIndent();

				curTransactions[transIndex] = new TransactionRecord();
				curTransactions[transIndex].PlayerRec2Index = BinaryHelper.ReadInt32(inFile, "Player Index"); ;
				curTransactions[transIndex].Salary = BinaryHelper.ReadCodedInt32(inFile, "Salary");
				curTransactions[transIndex].TransactionType = BinaryHelper.ReadInt16(inFile, "Type");
				curTransactions[transIndex].Team1Index = BinaryHelper.ReadInt16(inFile, "Team 1 Index");
				curTransactions[transIndex].Team2Index = BinaryHelper.ReadInt16(inFile, "Team 2 Index");
				curTransactions[transIndex].Position = BinaryHelper.ReadInt16(inFile, "Position");
				curTransactions[transIndex].Years = BinaryHelper.ReadInt16(inFile, "Years");
				curTransactions[transIndex].Stage = BinaryHelper.ReadInt16(inFile, "Stage");
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			short validTransactionCount = BinaryHelper.ReadInt16(inFile, "Valid Transaction Count");
			Array.Resize(ref curTransactions, validTransactionCount);

			BinaryHelper.ProbeTo(inFile, 0x31AA8);

			BinaryHelper.TracerWriteLine("Reading emails...");
			BinaryHelper.TracerIndent();
			short stringLength;
			for (int emailIndex = 0; emailIndex < kEmailRecordCount; emailIndex++)
			{
				BinaryHelper.TracerWriteLine("Email " + emailIndex);
				BinaryHelper.TracerIndent();

				mEmails[emailIndex] = new EmailRecord();
				mEmails[emailIndex].Flag = BinaryHelper.ReadInt16(inFile, "Flag");
				stringLength = BinaryHelper.ReadInt16(inFile,"From Length");
				mEmails[emailIndex].From = BinaryHelper.ExtractString(inFile, stringLength, "From");
				stringLength = BinaryHelper.ReadInt16(inFile, "Subject Length");
				mEmails[emailIndex].Subject = BinaryHelper.ExtractString(inFile, stringLength, "Subject");
				stringLength = BinaryHelper.ReadInt16(inFile, "Body Length");
				mEmails[emailIndex].Message = BinaryHelper.ExtractString(inFile, stringLength, "Body");

				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.ProbeBytes(inFile, 24);

			mFreeAgentRecordCount = BinaryHelper.ReadInt16(inFile, "Free Agent Record Count");
			mActivePlayerCount = BinaryHelper.ReadInt16(inFile, "Active Player Count");
			int historicalPlayerCount = BinaryHelper.ReadInt32(inFile, "Historical Player Count");
			BinaryHelper.ProbeBytes(inFile, 8);
			mSeasonsPlayed = BinaryHelper.ReadInt16(inFile, "Seasons Played");
			mTransactions = new TransactionRecord[mSeasonsPlayed][];
			int curSeasonIndex = mSeasonsPlayed - 1;
			mTransactions[curSeasonIndex] = curTransactions;
			BinaryHelper.ProbeBytes(inFile, 4);

			string playerNameFile = Path.ChangeExtension(fileName, ".ffn");
			System.IO.FileStream nameStream = new System.IO.FileStream(playerNameFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			System.IO.BinaryReader nameFile = new System.IO.BinaryReader(nameStream, windows1252Encoding);
			string nameHeader = BinaryHelper.ExtractString(nameFile, 16, "Name File Header");

			BinaryHelper.TracerWriteLine("Reading historical player records...");
			BinaryHelper.TracerIndent();
			mPlayerHistoricalRecords = new PlayerHistoricalRecord[historicalPlayerCount];
			for (int record2Index = 0; record2Index < mPlayerHistoricalRecords.Length; ++record2Index)
			{
				BinaryHelper.TracerWriteLine("Historical Player Record " + record2Index);
				BinaryHelper.TracerIndent();

				mPlayerHistoricalRecords[record2Index] = new PlayerHistoricalRecord();

				mPlayerHistoricalRecords[record2Index].PlayerID = BinaryHelper.ReadInt32(inFile, "PlayerID");
				mPlayerHistoricalRecords[record2Index].Position = BinaryHelper.ReadInt16(inFile, "Position");
				mPlayerHistoricalRecords[record2Index].Experience = BinaryHelper.ReadInt16(inFile, "Experience");
				stringLength = BinaryHelper.ReadInt16(inFile, "Last Name Length");
				mPlayerHistoricalRecords[record2Index].LastName = BinaryHelper.ExtractString(nameFile, stringLength, "Last Name");
				stringLength = BinaryHelper.ReadInt16(inFile, "First Name Length");
				mPlayerHistoricalRecords[record2Index].FirstName = BinaryHelper.ExtractString(nameFile, stringLength, "First Name");
				stringLength = BinaryHelper.ReadInt16(inFile, "Nickname Length");
				mPlayerHistoricalRecords[record2Index].NickName = BinaryHelper.ExtractString(nameFile, stringLength, "Nickname");
				BinaryHelper.ProbeBytes(inFile, 2);
				mPlayerHistoricalRecords[record2Index].PlayerOfTheGame = BinaryHelper.ReadInt16(inFile, "Player of Game");
				mPlayerHistoricalRecords[record2Index].ChampionshipRings = BinaryHelper.ReadInt16(inFile, "Champ Rings");
				short playerOfTheWeek = BinaryHelper.ReadInt16(inFile, "Player of the Week");
				mPlayerHistoricalRecords[record2Index].PlayerOfTheWeekMentions = (short)(playerOfTheWeek % 200);
				mPlayerHistoricalRecords[record2Index].PlayerOfTheWeekWins = (short)(playerOfTheWeek / 200);
				mPlayerHistoricalRecords[record2Index].Height = BinaryHelper.ReadInt16(inFile, "Height");
				mPlayerHistoricalRecords[record2Index].Weight = BinaryHelper.ReadInt16(inFile, "Weight");
				short hofBool = BinaryHelper.ReadInt16(inFile, "HOF?");
				short hofYear = BinaryHelper.ReadInt16(inFile, "HOF Year");
				short hofVote = BinaryHelper.ReadInt16(inFile, "HOF Vote");
				if (hofYear <= mCurrentYear)	// no peeking!
				{
					mPlayerHistoricalRecords[record2Index].InHallOfFame = hofBool;
					mPlayerHistoricalRecords[record2Index].HallOfFameYear = hofYear;
					mPlayerHistoricalRecords[record2Index].HallOfFameVote = hofVote;
				}
				else
				{
					mPlayerHistoricalRecords[record2Index].InHallOfFame = 0;
					mPlayerHistoricalRecords[record2Index].HallOfFameYear = 0;
					mPlayerHistoricalRecords[record2Index].HallOfFameVote = 0;
				}
				BinaryHelper.ProbeBytes(inFile, 2);
				mPlayerHistoricalRecords[record2Index].BirthYear = BinaryHelper.ReadInt16(inFile, "Birth Year");
				short birthday = BinaryHelper.ReadInt16(inFile, "Birth Date");
				mPlayerHistoricalRecords[record2Index].BirthMonth = (short)(birthday / 100);
				mPlayerHistoricalRecords[record2Index].BirthDay = (short)(birthday % 100);
				mPlayerHistoricalRecords[record2Index].College = BinaryHelper.ReadInt16(inFile, "College");
				short draftPosition = BinaryHelper.ReadInt16(inFile, "Draft Position");
				mPlayerHistoricalRecords[record2Index].DraftRound = (short)(draftPosition / 100);
				mPlayerHistoricalRecords[record2Index].DraftPick = (short)(draftPosition % 100);
				mPlayerHistoricalRecords[record2Index].FourthQuarterHeroics = BinaryHelper.ReadInt16(inFile, "4Q Hero");
				mPlayerHistoricalRecords[record2Index].QBWins = BinaryHelper.ReadInt16(inFile, "QB Wins");
				mPlayerHistoricalRecords[record2Index].QBLosses = BinaryHelper.ReadInt16(inFile, "QB Losses");
				mPlayerHistoricalRecords[record2Index].QBTies = BinaryHelper.ReadInt16(inFile, "QB Ties");
				mPlayerHistoricalRecords[record2Index].HomeTown = BinaryHelper.ReadInt16(inFile, "Home Town");
				BinaryHelper.ProbeBytes(inFile, 2);
				mPlayerHistoricalRecords[record2Index].DraftedBy = BinaryHelper.ReadInt16(inFile, "Drafted By");
				mPlayerHistoricalRecords[record2Index].YearDrafted = BinaryHelper.ReadInt16(inFile, "Year Drafted");
				mPlayerHistoricalRecords[record2Index].YearsInLeagueCount = BinaryHelper.ReadInt16(inFile, "Years In League Count");
				mPlayerHistoricalRecords[record2Index].YearsInLeague = new short[MaxPlayerHistoricalYearCount];
				mPlayerHistoricalRecords[record2Index].YearDataIndex = new int[MaxPlayerHistoricalYearCount];
				for (int gameRecordIndex = 0; gameRecordIndex < MaxPlayerHistoricalYearCount; gameRecordIndex++)
				{
					short first = BinaryHelper.ReadInt16(inFile, "Year Data Index First");
					short second = BinaryHelper.ReadInt16(inFile, "Year Data Index Second");
					mPlayerHistoricalRecords[record2Index].YearDataIndex[gameRecordIndex] = first;
					if (second > 0)
					{
						mPlayerHistoricalRecords[record2Index].YearDataIndex[gameRecordIndex] += (32768 * second);
					}
				}
				for (int yearIndex = 0; yearIndex < MaxPlayerHistoricalYearCount; yearIndex++)
				{
					short testYear = BinaryHelper.ReadInt16(inFile, "Year");
					mPlayerHistoricalRecords[record2Index].YearsInLeague[yearIndex] = testYear;
					if (testYear < mStartingYear && yearIndex < mPlayerHistoricalRecords[record2Index].YearsInLeagueCount)
					{
						mStartingYear = testYear;
					}
				}
				BinaryHelper.ProbeBytes(inFile, 2);
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();
			nameFile.Close();

			mSeasonRecords = new SeasonRecord[mSeasonsPlayed];
			mFranchisePerformanceRecords = new FranchisePerformanceRecord[mSeasonsPlayed * kTeamCount];
			mGameResultRecords = new GameResultRecord[kSeasonGameCount * mSeasonsPlayed];

			BinaryHelper.TracerWriteLine("Reading season records...");
			BinaryHelper.TracerIndent();
			int seasonIndex;
			for (seasonIndex = 0; seasonIndex < mSeasonsPlayed; seasonIndex++)
			{
				BinaryHelper.TracerWriteLine("Reading season " + seasonIndex.ToString() + "...");
				BinaryHelper.TracerIndent();

				mSeasonRecords[seasonIndex] = new SeasonRecord();
				mSeasonRecords[seasonIndex].PlayerEval = BinaryHelper.ReadInt16(inFile, "Player Eval");
				mSeasonRecords[seasonIndex].PlayerTeam = BinaryHelper.ReadInt16(inFile, "Player Team");
				mSeasonRecords[seasonIndex].Wins = BinaryHelper.ReadInt16(inFile, "Wins");
				mSeasonRecords[seasonIndex].Losses = BinaryHelper.ReadInt16(inFile, "Losses");
				mSeasonRecords[seasonIndex].Ties = BinaryHelper.ReadInt16(inFile, "Ties");
				mSeasonRecords[seasonIndex].Year = BinaryHelper.ReadInt16(inFile, "Year");
				if (seasonIndex == 0 && mSeasonRecords[seasonIndex].Year == 0)
				{
					mSeasonRecords[seasonIndex].Year = mCurrentYear;
				}

				BinaryHelper.ProbeBytes(inFile, 80);

				int franchiseIndex;
				int franchiseStartIndex = seasonIndex*kTeamCount;
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex] = new FranchisePerformanceRecord();
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].Year = mSeasonRecords[seasonIndex].Year;
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].FranchiseValue = BinaryHelper.ReadInt16(inFile,"Franchise Value");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].ProfitScore = BinaryHelper.ReadInt16(inFile,"Profit Score");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].PerformanceScore = BinaryHelper.ReadInt16(inFile,"Performance Score");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].RosterScore = BinaryHelper.ReadInt16(inFile,"Roster Score");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].Playoffs = BinaryHelper.ReadInt16(inFile,"Playoffs");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					short tempTeamIndex = BinaryHelper.ReadInt16(inFile,"Team Index");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].Wins = BinaryHelper.ReadInt16(inFile,"Wins");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].Losses = BinaryHelper.ReadInt16(inFile,"Losses");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].Ties = BinaryHelper.ReadInt16(inFile,"Ties");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].Unknown = BinaryHelper.ReadInt16(inFile,"Unknown");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].PointsFor = BinaryHelper.ReadInt16(inFile,"PointsFor");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].PointsAgainst = BinaryHelper.ReadInt16(inFile,"PointsAgainst");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].ConfWins = BinaryHelper.ReadInt16(inFile,"ConfWins");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].ConfLoss = BinaryHelper.ReadInt16(inFile,"ConfLoss");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].ConfTies = BinaryHelper.ReadInt16(inFile,"ConfTies");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].DivWin = BinaryHelper.ReadInt16(inFile,"DivWin");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].DivLoss = BinaryHelper.ReadInt16(inFile,"DivLoss");
					mFranchisePerformanceRecords[franchiseStartIndex+tempTeamIndex].DivTie = BinaryHelper.ReadInt16(inFile,"DivTie");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].Attendance = BinaryHelper.ReadInt16(inFile,"Attendance");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex+franchiseIndex].StadiumCapacity = BinaryHelper.ReadInt16(inFile,"StadiumCapacity");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].TVRevenue = BinaryHelper.ReadCodedInt32(inFile, "TVRevenue");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].TicketRevenue = BinaryHelper.ReadCodedInt32(inFile, "TicketRevenue");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].SuiteRevenue = BinaryHelper.ReadCodedInt32(inFile, "SuiteRevenue");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].PlayerSalaries = BinaryHelper.ReadCodedInt32(inFile, "PlayerSalaries");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].PlayerBonuses = BinaryHelper.ReadCodedInt32(inFile, "PlayerBonuses");
				}
				for (franchiseIndex = 0; franchiseIndex < kTeamCount; ++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Concessions = BinaryHelper.ReadCodedInt32(inFile, "Concessions");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Parking = BinaryHelper.ReadCodedInt32(inFile, "Parking");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Advertising = BinaryHelper.ReadCodedInt32(inFile, "Advertising");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Training = BinaryHelper.ReadCodedInt32(inFile, "Training");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Coaching = BinaryHelper.ReadCodedInt32(inFile, "Coaching");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Scouting = BinaryHelper.ReadCodedInt32(inFile, "Scouting");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].Maintenance = BinaryHelper.ReadCodedInt32(inFile, "Maintenance");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex = 0; franchiseIndex < kTeamCount; ++franchiseIndex)
				{
					mFranchisePerformanceRecords[franchiseStartIndex + franchiseIndex].StadiumPayment = BinaryHelper.ReadCodedInt32(inFile, "StadiumPayment");
				}
				for (franchiseIndex = 0; franchiseIndex < kTeamCount; ++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				for (franchiseIndex=0;franchiseIndex<kTeamCount;++franchiseIndex)
				{
					BinaryHelper.ReadCodedInt32(inFile, "Unknown");
				}
				BinaryHelper.ProbeBytes(inFile,18);

				BinaryHelper.TracerWriteLine("Reading Game Result Entries...");
				BinaryHelper.TracerIndent();
				int gameIndex;
				int gameStartIndex = kSeasonGameCount*seasonIndex;
				for (gameIndex=0;gameIndex<kSeasonGameCount;++gameIndex)
				{
					BinaryHelper.TracerWriteLine("Reading game "+gameIndex.ToString()+"...");
					BinaryHelper.TracerIndent();
					mGameResultRecords[gameStartIndex + gameIndex] = new GameResultRecord();
					mGameResultRecords[gameStartIndex+gameIndex].Year = mSeasonRecords[seasonIndex].Year;
					mGameResultRecords[gameStartIndex+gameIndex].Week = BinaryHelper.ReadInt16(inFile,"Week");
					mGameResultRecords[gameStartIndex+gameIndex].AwayScore = BinaryHelper.ReadInt16(inFile,"AwayScore");
					mGameResultRecords[gameStartIndex+gameIndex].AwayTeam = BinaryHelper.ReadInt16(inFile,"AwayTeam");
					mGameResultRecords[gameStartIndex+gameIndex].HomeScore = BinaryHelper.ReadInt16(inFile,"HomeScore");
					mGameResultRecords[gameStartIndex+gameIndex].HomeTeam = BinaryHelper.ReadInt16(inFile,"HomeTeam");
					mGameResultRecords[gameStartIndex+gameIndex].Attendance = BinaryHelper.ReadInt16(inFile,"Attendance");
					mGameResultRecords[gameStartIndex + gameIndex].Weather = BinaryHelper.ReadInt16(inFile, "Weather");
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();

				BinaryHelper.ProbeBytes(inFile,128);

				BinaryHelper.TracerWriteLine("Reading Game Leader Entries...");
				BinaryHelper.TracerIndent();
				for (gameIndex=0;gameIndex<kSeasonGameCount;++gameIndex)
				{
					BinaryHelper.TracerWriteLine("Reading game "+gameIndex.ToString()+"...");
					BinaryHelper.TracerIndent();
					mGameResultRecords[gameStartIndex+gameIndex].AwayPassingLeaderPlayerID = BinaryHelper.ReadInt32(inFile,"AwayPassingLeaderPlayerID");
					mGameResultRecords[gameStartIndex+gameIndex].HomePassingLeaderPlayerID = BinaryHelper.ReadInt32(inFile,"HomePassingLeaderPlayerID");
					mGameResultRecords[gameStartIndex+gameIndex].AwayRushingLeaderPlayerID = BinaryHelper.ReadInt32(inFile,"AwayRushingLeaderPlayerID");
					mGameResultRecords[gameStartIndex+gameIndex].HomeRushingLeaderPlayerID = BinaryHelper.ReadInt32(inFile,"HomeRushingLeaderPlayerID");
					mGameResultRecords[gameStartIndex+gameIndex].AwayReceivingLeaderPlayerID = BinaryHelper.ReadInt32(inFile,"AwayReceivingLeaderPlayerID");
					mGameResultRecords[gameStartIndex+gameIndex].HomeReceivingLeaderPlayerID = BinaryHelper.ReadInt32(inFile,"HomeReceivingLeaderPlayerID");
					mGameResultRecords[gameStartIndex+gameIndex].AwayPassAttempts = BinaryHelper.ReadInt16(inFile,"AwayPassAttempts");
					mGameResultRecords[gameStartIndex+gameIndex].AwayPassCompletions = BinaryHelper.ReadInt16(inFile,"AwayPassCompletions");
					mGameResultRecords[gameStartIndex+gameIndex].AwayPassYards = BinaryHelper.ReadInt16(inFile,"AwayPassYards");
					mGameResultRecords[gameStartIndex+gameIndex].HomePassAttempts = BinaryHelper.ReadInt16(inFile,"HomePassAttempts");
					mGameResultRecords[gameStartIndex+gameIndex].HomePassCompletions = BinaryHelper.ReadInt16(inFile,"HomePassCompletions");
					mGameResultRecords[gameStartIndex+gameIndex].HomePassYards = BinaryHelper.ReadInt16(inFile,"HomePassYards");
					mGameResultRecords[gameStartIndex+gameIndex].AwayRushAttempts = BinaryHelper.ReadInt16(inFile,"AwayRushAttempts");
					mGameResultRecords[gameStartIndex+gameIndex].AwayRushYards = BinaryHelper.ReadInt16(inFile,"AwayRushYards");
					mGameResultRecords[gameStartIndex+gameIndex].HomeRushAttempts = BinaryHelper.ReadInt16(inFile,"HomeRushAttempts");
					mGameResultRecords[gameStartIndex+gameIndex].HomeRushYards = BinaryHelper.ReadInt16(inFile,"HomeRushYards");
					mGameResultRecords[gameStartIndex+gameIndex].AwayReceptions = BinaryHelper.ReadInt16(inFile,"AwayReceptions");
					mGameResultRecords[gameStartIndex+gameIndex].AwayReceivingYards = BinaryHelper.ReadInt16(inFile,"AwayReceivingYards");
					mGameResultRecords[gameStartIndex+gameIndex].HomeReceptions = BinaryHelper.ReadInt16(inFile,"HomeReceptions");
					mGameResultRecords[gameStartIndex+gameIndex].HomeReceivingYards = BinaryHelper.ReadInt16(inFile,"HomeReceivingYards");
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();

				//long UnknownLong1;
				//short UnknownShort1;
				//short UnknownShort2;
				//long UnknownLong3;
				//long Unknown_Set16[160];	// 32*5
				//short Unknown_Set17[128];	// 32*4
				BinaryHelper.ProbeBytes(inFile,908);

				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			for (seasonIndex = 0; seasonIndex < mSeasonsPlayed; seasonIndex++)
			{
				BinaryHelper.ProbeBytes(inFile, 4640 * 2);
			}
			mTeamStadiumBlocks = new TeamStadiumBlock[kTeamCount];
			for (i = 0; i < kTeamCount; i++)
			{
				mTeamStadiumBlocks[i] = new TeamStadiumBlock();
				BinaryHelper.ProbeBytes(inFile, 109 * 2);
				mTeamStadiumBlocks[i].StadiumType = BinaryHelper.ReadInt16(inFile,"StadiumType");
				mTeamStadiumBlocks[i].YearStadiumBuilt = BinaryHelper.ReadInt16(inFile,"YearStadiumBuilt");
				mTeamStadiumBlocks[i].TotalCapacity = BinaryHelper.ReadInt16(inFile,"TotalCapacity");
				mTeamStadiumBlocks[i].LuxuryBoxes = BinaryHelper.ReadInt16(inFile,"LuxuryBoxes");
				mTeamStadiumBlocks[i].ClubSeats = BinaryHelper.ReadInt16(inFile,"ClubSeats");
				mTeamStadiumBlocks[i].Unknown1 = BinaryHelper.ReadInt16(inFile,"Unknown1");
				mTeamStadiumBlocks[i].Unknown2 = BinaryHelper.ReadInt16(inFile,"Unknown2");
				mTeamStadiumBlocks[i].FanLoyalty = BinaryHelper.ReadInt16(inFile,"FanLoyalty");
				mTeamStadiumBlocks[i].PublicSupportForStadium = BinaryHelper.ReadInt16(inFile,"PublicSupportForStadium");
				BinaryHelper.ProbeBytes(inFile, 1 * 2);
				mUniverseData.TeamRecords[i].CityIndex = BinaryHelper.ReadInt16(inFile, "CityIndex");
				BinaryHelper.ProbeBytes(inFile, 2 * 2);
				mTeamStadiumBlocks[i].UpperDeckPrice = BinaryHelper.ReadInt16(inFile,"UpperDeckPrice");
				mTeamStadiumBlocks[i].EndZonePrice = BinaryHelper.ReadInt16(inFile,"EndZonePrice");
				mTeamStadiumBlocks[i].MezzaninePrice = BinaryHelper.ReadInt16(inFile,"MezzaninePrice");
				mTeamStadiumBlocks[i].SidelinesPrice = BinaryHelper.ReadInt16(inFile,"SidelinesPrice");
				mTeamStadiumBlocks[i].ClubSeatsPrice = BinaryHelper.ReadInt16(inFile,"ClubSeatsPrice");
				mTeamStadiumBlocks[i].LuxuryBoxPrice = BinaryHelper.ReadInt16(inFile,"LuxuryBoxPrice");
				mTeamStadiumBlocks[i].Unknown3 = BinaryHelper.ReadInt16(inFile,"Unknown3");
				mTeamStadiumBlocks[i].Unknown4 = BinaryHelper.ReadInt16(inFile,"Unknown4");
				mTeamStadiumBlocks[i].ConstructionCompletionYear = BinaryHelper.ReadInt16(inFile,"ConstructionCompletionYear");
				mTeamStadiumBlocks[i].ConstructionType = BinaryHelper.ReadInt16(inFile,"ConstructionType");
				mTeamStadiumBlocks[i].Unknown5 = BinaryHelper.ReadInt16(inFile,"Unknown5");
				mTeamStadiumBlocks[i].ConstructionCapacity = BinaryHelper.ReadInt16(inFile,"ConstructionCapacity");
				mTeamStadiumBlocks[i].ConstructionLuxuryBoxes = BinaryHelper.ReadInt16(inFile,"ConstructionLuxuryBoxes");
				mTeamStadiumBlocks[i].ConstructionClubSeats = BinaryHelper.ReadInt16(inFile,"ConstructionClubSeats");
				mTeamStadiumBlocks[i].ConstructionStadiumType = BinaryHelper.ReadInt16(inFile,"ConstructionStadiumType");
				mTeamStadiumBlocks[i].Unknown7 = BinaryHelper.ReadInt16(inFile,"Unknown7");
				mTeamStadiumBlocks[i].PriorYearAttendance = BinaryHelper.ReadInt16(inFile,"PriorYearAttendance");
				BinaryHelper.ProbeBytes(inFile, 87 * 2);
			}

			BinaryHelper.TracerIndent();
			mTeamScheduleGameRecords = new TeamScheduleGameRecord[26 * kTeamCount];
			for (i = 0; i < mTeamScheduleGameRecords.Length; i++)
			{
				BinaryHelper.TracerIndent();
				BinaryHelper.TracerWriteLine("Team Game Record " + i.ToString());
				mTeamScheduleGameRecords[i] = new TeamScheduleGameRecord();
				mTeamScheduleGameRecords[i].TeamIndex = (short)(i/26);
				mTeamScheduleGameRecords[i].Week = BinaryHelper.ReadInt16(inFile, "Week");
				mTeamScheduleGameRecords[i].Away = BinaryHelper.ReadInt16(inFile, "Away");
				mTeamScheduleGameRecords[i].ConferenceGame = BinaryHelper.ReadInt16(inFile, "ConferenceGame");
				mTeamScheduleGameRecords[i].DivisionGame = BinaryHelper.ReadInt16(inFile, "DivisionGame");
				mTeamScheduleGameRecords[i].Opponent = BinaryHelper.ReadInt16(inFile, "Opponent");
				mTeamScheduleGameRecords[i].Score = BinaryHelper.ReadInt16(inFile, "Score");
				mTeamScheduleGameRecords[i].OppScore = BinaryHelper.ReadInt16(inFile, "OppScore");
				mTeamScheduleGameRecords[i].Unknown1 = BinaryHelper.ReadInt16(inFile, "Unknown1");
				mTeamScheduleGameRecords[i].Attendance = BinaryHelper.ReadInt16(inFile, "Attendance");
				mTeamScheduleGameRecords[i].Unknown2 = BinaryHelper.ReadInt16(inFile, "Unknown2");
				mTeamScheduleGameRecords[i].Weather = BinaryHelper.ReadInt16(inFile, "Weather");	// ? Temp appears in here, but not the usual encoding
				mTeamScheduleGameRecords[i].Unknown3 = BinaryHelper.ReadInt16(inFile, "Unknown3");
				mTeamScheduleGameRecords[i].Unknown4 = BinaryHelper.ReadInt16(inFile, "Unknown4");
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.TracerIndent();
			for (i = 0; i < mUniverseData.CityRecords.Length; i++)
			{
				BinaryHelper.TracerWriteLine("City = " + i);
				BinaryHelper.TracerIndent();
				mUniverseData.CityRecords[i].Population = BinaryHelper.ReadInt16(inFile, "Population");
				mUniverseData.CityRecords[i].GrowthRate = BinaryHelper.ReadInt16(inFile, "Growth Rate");
				mUniverseData.CityRecords[i].AverageIncome = BinaryHelper.ReadInt16(inFile, "Avg Income");
				mUniverseData.CityRecords[i].PovertyLevel = BinaryHelper.ReadInt16(inFile, "Poverty Level");
				mUniverseData.CityRecords[i].EntertainmentCompetiton = BinaryHelper.ReadInt16(inFile, "Entertainment Competition");
				mUniverseData.CityRecords[i].SeptemberHigh = BinaryHelper.ReadInt16(inFile, "September High");
				mUniverseData.CityRecords[i].SeptemberLow = BinaryHelper.ReadInt16(inFile, "September Low");
				mUniverseData.CityRecords[i].SeptemberHumidity = BinaryHelper.ReadInt16(inFile, "September Humidity");
				mUniverseData.CityRecords[i].DecemberHigh = BinaryHelper.ReadInt16(inFile, "December High");
				mUniverseData.CityRecords[i].DecemberLow = BinaryHelper.ReadInt16(inFile, "December Low");
				mUniverseData.CityRecords[i].DecemberHumidity = BinaryHelper.ReadInt16(inFile, "December Humidity");
				mUniverseData.CityRecords[i].NinetyDegreeDays = BinaryHelper.ReadInt16(inFile, "90 Degree Days");
				mUniverseData.CityRecords[i].SnowDays = BinaryHelper.ReadInt16(inFile, "Snow Days");
				mUniverseData.CityRecords[i].StormyDays = BinaryHelper.ReadInt16(inFile, "Stormy Days");
				mUniverseData.CityRecords[i].Elevation = BinaryHelper.ReadInt16(inFile, "Elevation");
				mUniverseData.CityRecords[i].Longitude = BinaryHelper.ReadInt16(inFile, "Longitude");
				mUniverseData.CityRecords[i].Latitude = BinaryHelper.ReadInt16(inFile, "Latitude");
				mUniverseData.CityRecords[i].HasTeam = BinaryHelper.ReadInt16(inFile, "Has Team");
				mUniverseData.CityRecords[i].TrendSetting = BinaryHelper.ReadInt16(inFile, "TrendSetting");
				mUniverseData.CityRecords[i].Region = BinaryHelper.ReadInt16(inFile, "Region");
				mUniverseData.CityRecords[i].WantsNewTeam = BinaryHelper.ReadInt16(inFile, "WantsNewTeam");
				mUniverseData.CityRecords[i].State = BinaryHelper.ReadInt16(inFile, "State");
				BinaryHelper.ProbeBytes(inFile, 2);
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.ProbeBytes(inFile, 883 * 2);

			BinaryHelper.TracerWriteLine("Player Game Stats");
			BinaryHelper.TracerIndent();
			int playerGameCount = kPlayerGamesPerSeason * mActivePlayerCount;
			mPlayerGameStatsRecords = new PlayerGameStatsRecord[mSeasonsPlayed][];
			mPlayerGameStatsRecords[curSeasonIndex] = new PlayerGameStatsRecord[playerGameCount];
			for (int playerGameIndex = 0; playerGameIndex < playerGameCount; ++playerGameIndex)
			{
				BinaryHelper.TracerWriteLine("Record " + playerGameIndex.ToString());
				BinaryHelper.TracerIndent();
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex] = new PlayerGameStatsRecord();
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PlayerID = playerGameIndex / kPlayerGamesPerSeason;
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Week = (short)((playerGameIndex % kPlayerGamesPerSeason)+6);
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Year = BinaryHelper.ReadInt16(inFile, "Year");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Team = BinaryHelper.ReadInt16(inFile, "Team");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].GamePlayed = BinaryHelper.ReadInt16(inFile, "GamePlayed");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].GameStarted = BinaryHelper.ReadInt16(inFile, "GameStarted");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassAttempts = BinaryHelper.ReadInt16(inFile, "PassAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassCompletions = BinaryHelper.ReadInt16(inFile, "PassCompletions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassYards = BinaryHelper.ReadInt16(inFile, "PassYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].LongestPass = BinaryHelper.ReadInt16(inFile, "LongestPass");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TDPasses = BinaryHelper.ReadInt16(inFile, "TDPasses");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTThrown = BinaryHelper.ReadInt16(inFile, "INTThrown");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TimesSacked = BinaryHelper.ReadInt16(inFile, "TimesSacked");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SackedYards = BinaryHelper.ReadInt16(inFile, "SackedYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushAttempts = BinaryHelper.ReadInt16(inFile, "RushAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushingYards = BinaryHelper.ReadInt16(inFile, "RushingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].LongestRun = BinaryHelper.ReadInt16(inFile, "LongestRun");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushTD = BinaryHelper.ReadInt16(inFile, "RushTD");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Catches = BinaryHelper.ReadInt16(inFile, "Catches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ReceivingYards = BinaryHelper.ReadInt16(inFile, "ReceivingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].LongestReception = BinaryHelper.ReadInt16(inFile, "LongestReception");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ReceivingTDs = BinaryHelper.ReadInt16(inFile, "ReceivingTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassTargets = BinaryHelper.ReadInt16(inFile, "PassTargets");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].YardsAfterCatch = BinaryHelper.ReadInt16(inFile, "YardsAfterCatch");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassDrops = BinaryHelper.ReadInt16(inFile, "PassDrops");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntReturns = BinaryHelper.ReadInt16(inFile, "PuntReturns");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntReturnYards = BinaryHelper.ReadInt16(inFile, "PuntReturnYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntReturnTDs = BinaryHelper.ReadInt16(inFile, "PuntReturnTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KickReturns = BinaryHelper.ReadInt16(inFile, "KickReturns");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KickReturnYards = BinaryHelper.ReadInt16(inFile, "KickReturnYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KickReturnTDs = BinaryHelper.ReadInt16(inFile, "KickReturnTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Fumbles = BinaryHelper.ReadInt16(inFile, "Fumbles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FumbleRecoveries = BinaryHelper.ReadInt16(inFile, "FumbleRecoveries");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ForcedFumbles = BinaryHelper.ReadInt16(inFile, "ForcedFumbles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].MiscTD = BinaryHelper.ReadInt16(inFile, "MiscTD");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KeyRunBlock = BinaryHelper.ReadInt16(inFile, "KeyRunBlock");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KeyRunBlockOpportunites = BinaryHelper.ReadInt16(inFile, "KeyRunBlockOpportunites");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SacksAllowed = BinaryHelper.ReadInt16(inFile, "SacksAllowed");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Tackles = BinaryHelper.ReadInt16(inFile, "Tackles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Assists = BinaryHelper.ReadInt16(inFile, "Assists");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Sacks = BinaryHelper.ReadInt16(inFile, "Sacks");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTs = BinaryHelper.ReadInt16(inFile, "INTs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTReturnYards = BinaryHelper.ReadInt16(inFile, "INTReturnYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTReturnTDs = BinaryHelper.ReadInt16(inFile, "INTReturnTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassesDefended = BinaryHelper.ReadInt16(inFile, "PassesDefended");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassesBlocked = BinaryHelper.ReadInt16(inFile, "PassesBlocked");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].QBHurries = BinaryHelper.ReadInt16(inFile, "QBHurries");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassesCaught = BinaryHelper.ReadInt16(inFile, "PassesCaught");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassPlays = BinaryHelper.ReadInt16(inFile, "PassPlays");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RunPlays = BinaryHelper.ReadInt16(inFile, "RunPlays");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FGMade = BinaryHelper.ReadInt16(inFile, "FGMade");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FGAttempted = BinaryHelper.ReadInt16(inFile, "FGAttempted");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FGLong = BinaryHelper.ReadInt16(inFile, "FGLong");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PAT = BinaryHelper.ReadInt16(inFile, "PAT");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PATAttempted = BinaryHelper.ReadInt16(inFile, "PATAttempted");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Punts = BinaryHelper.ReadInt16(inFile, "Punts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntYards = BinaryHelper.ReadInt16(inFile, "PuntYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntLong = BinaryHelper.ReadInt16(inFile, "PuntLong");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntIn20 = BinaryHelper.ReadInt16(inFile, "PuntIn20");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Points = BinaryHelper.ReadInt16(inFile, "Points");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].OpposingTeamID = BinaryHelper.ReadInt16(inFile, "Unknown1");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownRushes = BinaryHelper.ReadInt16(inFile, "ThirdDownRushes");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownRushConversions = BinaryHelper.ReadInt16(inFile, "ThirdDownRushConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownPassAttempts = BinaryHelper.ReadInt16(inFile, "ThirdDownPassAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownPassCompletions = BinaryHelper.ReadInt16(inFile, "ThirdDownPassCompletions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownPassConversions = BinaryHelper.ReadInt16(inFile, "ThirdDownPassConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownReceivingTargets = BinaryHelper.ReadInt16(inFile, "ThirdDownReceivingTargets");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownReceivingCatches = BinaryHelper.ReadInt16(inFile, "ThirdDownReceivingCatches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownReceivingConversions = BinaryHelper.ReadInt16(inFile, "ThirdDownReceivingConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FirstDownRushes = BinaryHelper.ReadInt16(inFile, "FirstDownRushes");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FirstDownPasses = BinaryHelper.ReadInt16(inFile, "FirstDownPasses");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FirstDownCatches = BinaryHelper.ReadInt16(inFile, "FirstDownCatches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG40PlusAttempts = BinaryHelper.ReadInt16(inFile, "FG40PlusAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG40PlusMade = BinaryHelper.ReadInt16(inFile, "FG40PlusMade");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG50PlusAttempts = BinaryHelper.ReadInt16(inFile, "FG50PlusAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG50PlusMade = BinaryHelper.ReadInt16(inFile, "FG50PlusMade");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntNetYards = BinaryHelper.ReadInt16(inFile, "PuntNetYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SpecialTeamsTackles = BinaryHelper.ReadInt16(inFile, "SpecialTeamsTackles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Unknown14 = BinaryHelper.ReadInt16(inFile, "Unknown14");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TimesKnockedDown = BinaryHelper.ReadInt16(inFile, "TimesKnockedDown");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneRushes = BinaryHelper.ReadInt16(inFile, "RedZoneRushes");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneRushingYards = BinaryHelper.ReadInt16(inFile, "RedZoneRushingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZonePassAttempts = BinaryHelper.ReadInt16(inFile, "RedZonePassAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZonePassCompletions = BinaryHelper.ReadInt16(inFile, "RedZonePassCompletions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZonePassingYards = BinaryHelper.ReadInt16(inFile, "RedZonePassingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneReceivingTargets = BinaryHelper.ReadInt16(inFile, "RedZoneReceivingTargets");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneReceivingCatches = BinaryHelper.ReadInt16(inFile, "RedZoneReceivingCatches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneReceivingYards = BinaryHelper.ReadInt16(inFile, "RedZoneReceivingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TotalTDs = BinaryHelper.ReadInt16(inFile, "TotalTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TwoPointConversions = BinaryHelper.ReadInt16(inFile, "TwoPointConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PancakeBlocks = BinaryHelper.ReadInt16(inFile, "PancakeBlocks");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].QBKnockdowns = BinaryHelper.ReadInt16(inFile, "QBKnockdowns");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Unknown23 = BinaryHelper.ReadInt16(inFile, "Unknown23");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SpecialTeamsPlays = BinaryHelper.ReadInt16(inFile, "SpecialTeamsPlays");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushingGamesOver100Yards = BinaryHelper.ReadInt16(inFile, "RushingGamesOver100Yards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ReceivingGamesOver100Yards = BinaryHelper.ReadInt16(inFile, "ReceivingGamesOver100Yards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassingGamesOver300Yards = BinaryHelper.ReadInt16(inFile, "PassingGamesOver300Yards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RunsOf10YardsPlus = BinaryHelper.ReadInt16(inFile, "RunsOf10YardsPlus");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].CatchesOf20YardsPlus = BinaryHelper.ReadInt16(inFile, "CatchesOf20YardsPlus");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThrowsOf20YardsPlus = BinaryHelper.ReadInt16(inFile, "ThrowsOf20YardsPlus");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].YardsFromScrimmage = BinaryHelper.ReadInt16(inFile, "YardsFromScrimmage");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].AllPurposeYards = BinaryHelper.ReadInt16(inFile, "AllPurposeYards");
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			BinaryHelper.ClearTracer();
			inFile.Close();
		}

		private void LoadSeasonData(string pathPrefix)
		{
			for (int yearIndex = 0; yearIndex < (mSeasonsPlayed - 1); ++yearIndex)
			{
				int year = mStartingYear + yearIndex;
				if (year >= mStartingSeason)
				{
					LoadSeasonFile(System.IO.Path.ChangeExtension(pathPrefix, year.ToString()), yearIndex);
				}
			}
			SetGameStatPlayerIDs();
		}

		private void LoadSeasonFile(string fileName, int curSeasonIndex)
		{
			if (!System.IO.File.Exists(fileName))
			{
				return;
			}

			if (FileReadCallback != null)
			{
				FileReadCallback(System.IO.Path.GetFileName(fileName));
			}

			System.IO.FileStream inStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

			long endPosition = inStream.Length;

			//BinaryHelper.SetupTracer(Path.GetFileName(fileName));

			BinaryHelper.ExtractString(inFile, 16, "Header");

			short firstCount = BinaryHelper.ReadInt16(inFile, "First Count");
			short secondCount = BinaryHelper.ReadInt16(inFile, "Second Count");
			int playerGameCount = firstCount + (secondCount * 32768);
			mPlayerGameStatsRecords[curSeasonIndex] = new PlayerGameStatsRecord[playerGameCount];

			BinaryHelper.TracerWriteLine("Player Game Stats");
			BinaryHelper.TracerIndent();
			for (int playerGameIndex = 0; playerGameIndex < playerGameCount; ++playerGameIndex)
			{
				BinaryHelper.TracerWriteLine("Record " + playerGameIndex.ToString());
				BinaryHelper.TracerIndent();

				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex] = new PlayerGameStatsRecord();
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PlayerID = -1;
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Week = (short)((playerGameIndex % kPlayerGamesPerSeason)+6);
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Year = BinaryHelper.ReadInt16(inFile, "Year");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Team = BinaryHelper.ReadInt16(inFile, "Team");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].GamePlayed = BinaryHelper.ReadInt16(inFile, "GamePlayed");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].GameStarted = BinaryHelper.ReadInt16(inFile, "GameStarted");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassAttempts = BinaryHelper.ReadInt16(inFile, "PassAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassCompletions = BinaryHelper.ReadInt16(inFile, "PassCompletions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassYards = BinaryHelper.ReadInt16(inFile, "PassYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].LongestPass = BinaryHelper.ReadInt16(inFile, "LongestPass");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TDPasses = BinaryHelper.ReadInt16(inFile, "TDPasses");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTThrown = BinaryHelper.ReadInt16(inFile, "INTThrown");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TimesSacked = BinaryHelper.ReadInt16(inFile, "TimesSacked");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SackedYards = BinaryHelper.ReadInt16(inFile, "SackedYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushAttempts = BinaryHelper.ReadInt16(inFile, "RushAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushingYards = BinaryHelper.ReadInt16(inFile, "RushingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].LongestRun = BinaryHelper.ReadInt16(inFile, "LongestRun");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushTD = BinaryHelper.ReadInt16(inFile, "RushTD");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Catches = BinaryHelper.ReadInt16(inFile, "Catches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ReceivingYards = BinaryHelper.ReadInt16(inFile, "ReceivingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].LongestReception = BinaryHelper.ReadInt16(inFile, "LongestReception");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ReceivingTDs = BinaryHelper.ReadInt16(inFile, "ReceivingTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassTargets = BinaryHelper.ReadInt16(inFile, "PassTargets");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].YardsAfterCatch = BinaryHelper.ReadInt16(inFile, "YardsAfterCatch");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassDrops = BinaryHelper.ReadInt16(inFile, "PassDrops");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntReturns = BinaryHelper.ReadInt16(inFile, "PuntReturns");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntReturnYards = BinaryHelper.ReadInt16(inFile, "PuntReturnYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntReturnTDs = BinaryHelper.ReadInt16(inFile, "PuntReturnTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KickReturns = BinaryHelper.ReadInt16(inFile, "KickReturns");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KickReturnYards = BinaryHelper.ReadInt16(inFile, "KickReturnYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KickReturnTDs = BinaryHelper.ReadInt16(inFile, "KickReturnTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Fumbles = BinaryHelper.ReadInt16(inFile, "Fumbles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FumbleRecoveries = BinaryHelper.ReadInt16(inFile, "FumbleRecoveries");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ForcedFumbles = BinaryHelper.ReadInt16(inFile, "ForcedFumbles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].MiscTD = BinaryHelper.ReadInt16(inFile, "MiscTD");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KeyRunBlock = BinaryHelper.ReadInt16(inFile, "KeyRunBlock");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].KeyRunBlockOpportunites = BinaryHelper.ReadInt16(inFile, "KeyRunBlockOpportunites");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SacksAllowed = BinaryHelper.ReadInt16(inFile, "SacksAllowed");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Tackles = BinaryHelper.ReadInt16(inFile, "Tackles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Assists = BinaryHelper.ReadInt16(inFile, "Assists");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Sacks = BinaryHelper.ReadInt16(inFile, "Sacks");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTs = BinaryHelper.ReadInt16(inFile, "INTs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTReturnYards = BinaryHelper.ReadInt16(inFile, "INTReturnYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].INTReturnTDs = BinaryHelper.ReadInt16(inFile, "INTReturnTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassesDefended = BinaryHelper.ReadInt16(inFile, "PassesDefended");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassesBlocked = BinaryHelper.ReadInt16(inFile, "PassesBlocked");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].QBHurries = BinaryHelper.ReadInt16(inFile, "QBHurries");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassesCaught = BinaryHelper.ReadInt16(inFile, "PassesCaught");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassPlays = BinaryHelper.ReadInt16(inFile, "PassPlays");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RunPlays = BinaryHelper.ReadInt16(inFile, "RunPlays");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FGMade = BinaryHelper.ReadInt16(inFile, "FGMade");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FGAttempted = BinaryHelper.ReadInt16(inFile, "FGAttempted");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FGLong = BinaryHelper.ReadInt16(inFile, "FGLong");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PAT = BinaryHelper.ReadInt16(inFile, "PAT");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PATAttempted = BinaryHelper.ReadInt16(inFile, "PATAttempted");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Punts = BinaryHelper.ReadInt16(inFile, "Punts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntYards = BinaryHelper.ReadInt16(inFile, "PuntYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntLong = BinaryHelper.ReadInt16(inFile, "PuntLong");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntIn20 = BinaryHelper.ReadInt16(inFile, "PuntIn20");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Points = BinaryHelper.ReadInt16(inFile, "Points");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].OpposingTeamID = BinaryHelper.ReadInt16(inFile, "Unknown1");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownRushes = BinaryHelper.ReadInt16(inFile, "ThirdDownRushes");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownRushConversions = BinaryHelper.ReadInt16(inFile, "ThirdDownRushConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownPassAttempts = BinaryHelper.ReadInt16(inFile, "ThirdDownPassAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownPassCompletions = BinaryHelper.ReadInt16(inFile, "ThirdDownPassCompletions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownPassConversions = BinaryHelper.ReadInt16(inFile, "ThirdDownPassConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownReceivingTargets = BinaryHelper.ReadInt16(inFile, "ThirdDownReceivingTargets");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownReceivingCatches = BinaryHelper.ReadInt16(inFile, "ThirdDownReceivingCatches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThirdDownReceivingConversions = BinaryHelper.ReadInt16(inFile, "ThirdDownReceivingConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FirstDownRushes = BinaryHelper.ReadInt16(inFile, "FirstDownRushes");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FirstDownPasses = BinaryHelper.ReadInt16(inFile, "FirstDownPasses");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FirstDownCatches = BinaryHelper.ReadInt16(inFile, "FirstDownCatches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG40PlusAttempts = BinaryHelper.ReadInt16(inFile, "FG40PlusAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG40PlusMade = BinaryHelper.ReadInt16(inFile, "FG40PlusMade");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG50PlusAttempts = BinaryHelper.ReadInt16(inFile, "FG50PlusAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].FG50PlusMade = BinaryHelper.ReadInt16(inFile, "FG50PlusMade");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PuntNetYards = BinaryHelper.ReadInt16(inFile, "PuntNetYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SpecialTeamsTackles = BinaryHelper.ReadInt16(inFile, "SpecialTeamsTackles");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Unknown14 = BinaryHelper.ReadInt16(inFile, "Unknown14");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TimesKnockedDown = BinaryHelper.ReadInt16(inFile, "TimesKnockedDown");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneRushes = BinaryHelper.ReadInt16(inFile, "RedZoneRushes");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneRushingYards = BinaryHelper.ReadInt16(inFile, "RedZoneRushingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZonePassAttempts = BinaryHelper.ReadInt16(inFile, "RedZonePassAttempts");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZonePassCompletions = BinaryHelper.ReadInt16(inFile, "RedZonePassCompletions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZonePassingYards = BinaryHelper.ReadInt16(inFile, "RedZonePassingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneReceivingTargets = BinaryHelper.ReadInt16(inFile, "RedZoneReceivingTargets");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneReceivingCatches = BinaryHelper.ReadInt16(inFile, "RedZoneReceivingCatches");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RedZoneReceivingYards = BinaryHelper.ReadInt16(inFile, "RedZoneReceivingYards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TotalTDs = BinaryHelper.ReadInt16(inFile, "TotalTDs");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].TwoPointConversions = BinaryHelper.ReadInt16(inFile, "TwoPointConversions");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PancakeBlocks = BinaryHelper.ReadInt16(inFile, "PancakeBlocks");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].QBKnockdowns = BinaryHelper.ReadInt16(inFile, "QBKnockdowns");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].Unknown23 = BinaryHelper.ReadInt16(inFile, "Unknown23");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].SpecialTeamsPlays = BinaryHelper.ReadInt16(inFile, "SpecialTeamsPlays");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RushingGamesOver100Yards = BinaryHelper.ReadInt16(inFile, "RushingGamesOver100Yards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ReceivingGamesOver100Yards = BinaryHelper.ReadInt16(inFile, "ReceivingGamesOver100Yards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].PassingGamesOver300Yards = BinaryHelper.ReadInt16(inFile, "PassingGamesOver300Yards");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].RunsOf10YardsPlus = BinaryHelper.ReadInt16(inFile, "RunsOf10YardsPlus");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].CatchesOf20YardsPlus = BinaryHelper.ReadInt16(inFile, "CatchesOf20YardsPlus");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].ThrowsOf20YardsPlus = BinaryHelper.ReadInt16(inFile, "ThrowsOf20YardsPlus");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].YardsFromScrimmage = BinaryHelper.ReadInt16(inFile, "YardsFromScrimmage");
				mPlayerGameStatsRecords[curSeasonIndex][playerGameIndex].AllPurposeYards = BinaryHelper.ReadInt16(inFile, "AllPurposeYards");
				BinaryHelper.TracerOutdent();
			}
			BinaryHelper.TracerOutdent();

			short transactionCount = 0;
			if (inStream.Position < endPosition)
			{
				transactionCount = BinaryHelper.ReadInt16(inFile, "Transaction Count");
			}

			BinaryHelper.TracerWriteLine("Transactions");
			BinaryHelper.TracerIndent();
			mTransactions[curSeasonIndex] = new TransactionRecord[transactionCount];
			int lastIndex = 0;
			for (int transIndex = 0; transIndex < transactionCount; ++transIndex)
			{
				BinaryHelper.TracerWriteLine("Transaction " + transIndex.ToString());
				BinaryHelper.TracerIndent();

				mTransactions[curSeasonIndex][transIndex] = new TransactionRecord();
				mTransactions[curSeasonIndex][transIndex].PlayerRec2Index = BinaryHelper.ReadInt32(inFile, "Player Index"); ;
				mTransactions[curSeasonIndex][transIndex].Salary = BinaryHelper.ReadCodedInt32(inFile, "Salary");
				mTransactions[curSeasonIndex][transIndex].TransactionType = BinaryHelper.ReadInt16(inFile, "Type");
				mTransactions[curSeasonIndex][transIndex].Team1Index = BinaryHelper.ReadInt16(inFile, "Team 1 Index");
				mTransactions[curSeasonIndex][transIndex].Team2Index = BinaryHelper.ReadInt16(inFile, "Team 2 Index");
				mTransactions[curSeasonIndex][transIndex].Position = BinaryHelper.ReadInt16(inFile, "Position");
				mTransactions[curSeasonIndex][transIndex].Years = BinaryHelper.ReadInt16(inFile, "Years");
				mTransactions[curSeasonIndex][transIndex].Stage = BinaryHelper.ReadInt16(inFile, "Stage");

				BinaryHelper.TracerOutdent();

				lastIndex = transIndex;
				if (inStream.Position >= endPosition)
				{
					break;
				}
			}
			BinaryHelper.TracerOutdent();

			int finalSize = lastIndex + 1;
			if (mTransactions[curSeasonIndex].Length > finalSize)
			{
				Array.Resize(ref mTransactions[curSeasonIndex], lastIndex + 1);
			}

			BinaryHelper.ClearTracer();
			inFile.Close();
		}

		private void SetGameStatPlayerIDs()
		{
			for (int i = 0; i < mPlayerHistoricalRecords.Length; ++i)
			{
				for (int j = 0; j < mPlayerHistoricalRecords[i].YearsInLeagueCount; ++j)
				{
					int yearIndex = mPlayerHistoricalRecords[i].YearsInLeague[j] - mStartingYear;
					if (mPlayerGameStatsRecords[yearIndex] != null)
					{
						int gameRecordIndex = mPlayerHistoricalRecords[i].YearDataIndex[j];
						for (int k = 0; k < kPlayerGamesPerSeason; ++k)
						{
							mPlayerGameStatsRecords[yearIndex][gameRecordIndex + k].PlayerID = mPlayerHistoricalRecords[i].PlayerID;
						}
					}
				}
			}
		}

		private GameTeamEntry LoadGameTeamEntry(System.IO.BinaryReader inFile, int week)
		{
			GameTeamEntry newEntry = new GameTeamEntry();
			newEntry.TeamIndex = BinaryHelper.ReadInt16(inFile, "Team Index");
			short stringLength = BinaryHelper.ReadInt16(inFile,"City Length");
			newEntry.CityName = BinaryHelper.ExtractString(inFile,stringLength,"City Name");
			stringLength = BinaryHelper.ReadInt16(inFile,"Nickname Length");
			newEntry.NickName = BinaryHelper.ExtractString(inFile,stringLength,"Nickname");
			stringLength = BinaryHelper.ReadInt16(inFile,"Abbreviation Length");
			newEntry.Abbreviation = BinaryHelper.ExtractString(inFile,stringLength,"Abbreviation");
			newEntry.ActivePlayerIDs = new int[kNumActivePlayers];
			newEntry.PlayerStats = new PlayerGameStatsRecord[kNumActivePlayers];
			newEntry.PlayerHistorical = new PlayerHistoricalRecord[kNumActivePlayers];
			BinaryHelper.TracerWriteLine("Active Player IDs");
			BinaryHelper.TracerIndent();
			int i;
			for (i = 0; i < newEntry.ActivePlayerIDs.Length; ++i)
			{
				BinaryHelper.ReadInt32(inFile, "UnknownIndex");
				newEntry.ActivePlayerIDs[i] = BinaryHelper.ReadInt32(inFile, "Player ID "+i.ToString("D2"));
				newEntry.PlayerStats[i] = FindPlayerGameStats(newEntry.ActivePlayerIDs[i], week);
				newEntry.PlayerHistorical[i] = FindPlayerHistoricalRecord(newEntry.ActivePlayerIDs[i]);
			}
			BinaryHelper.TracerOutdent();
			newEntry.DepthChartEntries = new short[kNumDepthChartEntries];
			BinaryHelper.TracerWriteLine("Depth Chart Entries");
			BinaryHelper.TracerIndent();
			for (i = 0; i < newEntry.DepthChartEntries.Length; ++i)
			{
				newEntry.DepthChartEntries[i] = BinaryHelper.ReadInt16(inFile, "Depth Chart Entry "+i.ToString("D2"));
			}
			BinaryHelper.TracerOutdent();

			return newEntry;
		}

		private void LoadFGPlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Field Goal Play");
			BinaryHelper.TracerIndent();

			if (gFGPlayDumpFile != null)
			{
				int weekNum = gCurGameLog.Week - 1;
				string gameID = gCurGameLog.Year + weekNum.ToString("D2") + gCurGameLog.AwayTeam.TeamIndex.ToString("D2") + gCurGameLog.HomeTeam.TeamIndex.ToString("D2");
				gFGPlayDumpFile.Write("," + gameID + "," + CurPlayID + "," + gCurGameLog.AwayTeam.Abbreviation + "," + gCurGameLog.HomeTeam.Abbreviation + "," + playData.Quarter + "," + playData.Minutes + "," + playData.Seconds + "," + playData.Possession);
			}

			for (int i = 0; i < 63; ++i)
			{
				short data = BinaryHelper.ReadInt16(inFile, "FGData" + i);
				if (gFGPlayDumpFile != null)
				{
					gFGPlayDumpFile.Write("," + data);
				}
			}

			BinaryHelper.TracerOutdent();
		}

		private void LoadPassPlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Pass Play");
			BinaryHelper.TracerIndent();

			playData.TypeSpecificData[(int)PassPlayFields.Minute] = BinaryHelper.ReadInt16(inFile, "Minutes");   // 0-15 (Minute)
			playData.TypeSpecificData[(int)PassPlayFields.Seconds] = BinaryHelper.ReadInt16(inFile,"Seconds");  // 0-59 (Second)
			short dat02 = BinaryHelper.ReadInt16(inFile, "dat02");
			short dat03 = BinaryHelper.ReadInt16(inFile, "dat03");
			playData.TypeSpecificData[(int)PassPlayFields.IsComplete] = BinaryHelper.ReadInt16(inFile, "IsComplete");   // 0-1 (1: Completion)
			playData.TypeSpecificData[(int)PassPlayFields.YardsGained] = BinaryHelper.ReadInt16(inFile,"DesignedYardage"); // ??-?? (Designed Yardage: i.e. either the yardage of the pass if complete, or what it would have been if incomplete (assumed, but likely))
			playData.TypeSpecificData[(int)PassPlayFields.IsTouchdown] = BinaryHelper.ReadInt16(inFile,"IsTouchdown");  // 0-1 (1: Touchdown)
			playData.TypeSpecificData[(int)PassPlayFields.PassTarget] = BinaryHelper.ReadInt16(inFile,"PassTarget");   // 1-10 (Pass Target, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.Tackler] = BinaryHelper.ReadInt16(inFile,"Tackler");      // 0-10 (Player Tackler, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.IsFumble] = BinaryHelper.ReadInt16(inFile,"IsFumble");     // 0-1 (1: Fumble)
			playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoveryTeam] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveryTeam");   // 0-1 (Fumble Recovered by which team)
			playData.TypeSpecificData[(int)PassPlayFields.FumbleReturnYards] = BinaryHelper.ReadInt16(inFile,"FumbleReturnYards");    // ??-?? (Fumble Return yards)
			playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoveryTackler] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveryTackler");    // 0-10 (Tackler of Fumble Recover, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.IsFumbleRecoveredForTD] = BinaryHelper.ReadInt16(inFile,"IsFumbleRecoveredForTD");   // 0-1 (1: Fumble Recovered for TD)
			playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoverer] = BinaryHelper.ReadInt16(inFile,"FumbleRecoverer");  // 0-10 (Fumble Recoverer)
			playData.TypeSpecificData[(int)PassPlayFields.IsInterception] = BinaryHelper.ReadInt16(inFile,"IsInterception");   // 0-1 (1: Interception)
			playData.TypeSpecificData[(int)PassPlayFields.InterceptingPlayer] = BinaryHelper.ReadInt16(inFile,"InterceptingPlayer");   // 0-10 (Player Intercepting, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.InterceptionReturnYards] = BinaryHelper.ReadInt16(inFile,"InterceptionReturnYards");  // ??-?? (Interception return yards)
			playData.TypeSpecificData[(int)PassPlayFields.IsInterceptedForTD] = BinaryHelper.ReadInt16(inFile,"IsInterceptedForTD");   // 0-1 (1: Interception for touchdown)
			playData.TypeSpecificData[(int)PassPlayFields.InterceptedTackler] = BinaryHelper.ReadInt16(inFile,"InterceptedTackler");   // 0-10 (Player tackling interceptor, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.InterceptionYardLine] = BinaryHelper.ReadInt16(inFile,"InterceptionYardLine"); // 0-100 (Interception yard line)
			playData.TypeSpecificData[(int)PassPlayFields.IsPenaltyAccepted] = BinaryHelper.ReadInt16(inFile,"IsPenaltyAccepted");    // 0-1 (1: Accepted Penalty)
			playData.TypeSpecificData[(int)PassPlayFields.IsDefensiveTD] = BinaryHelper.ReadInt16(inFile,"IsDefensiveTD");    // 0-1 (1: Defensive TD)
			short dat23 = BinaryHelper.ReadInt16(inFile, "dat23");
			short dat24 = BinaryHelper.ReadInt16(inFile, "dat24");
			playData.TypeSpecificData[(int)PassPlayFields.IsQBScramble] = BinaryHelper.ReadInt16(inFile, "IsQBScramble");     // 0-1 (1: QB Scramble (NOTE: Statistically a run))
			playData.TypeSpecificData[(int)PassPlayFields.QBScrambleYards] = BinaryHelper.ReadInt16(inFile,"QBScrambleYards");  // ??-?? (QB Scramble Yards)
			playData.TypeSpecificData[(int)PassPlayFields.IsQBSacked] = BinaryHelper.ReadInt16(inFile,"IsQBSacked");       // 0-1 (1: QB Sack)
			playData.TypeSpecificData[(int)PassPlayFields.QBSackYards] = BinaryHelper.ReadInt16(inFile,"QBSackYards");      // ??-?? (QB Sack Yards)
			playData.TypeSpecificData[(int)PassPlayFields.IsQBSackedForSafety] = BinaryHelper.ReadInt16(inFile,"IsQBSackedForSafety");  // 0-1 (1: QB Sacked for Safety)
			playData.TypeSpecificData[(int)PassPlayFields.SackingPlayer] = BinaryHelper.ReadInt16(inFile,"SackingPlayer");    // 0-10 (Sacking Player, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.IsForcedOOB] = BinaryHelper.ReadInt16(inFile,"IsForcedOOB");      // 0-1 (1: Forced OOB)
			playData.TypeSpecificData[(int)PassPlayFields.InterceptedInEndZone] = BinaryHelper.ReadInt16(inFile,"InterceptedInEndZone"); // 0-1 (1: Interception in End Zone)
			playData.TypeSpecificData[(int)PassPlayFields.IsHalfSack] = BinaryHelper.ReadInt16(inFile,"IsHalfSack");       // 0-1 (1: Halved Sack)
			playData.TypeSpecificData[(int)PassPlayFields.AssistantSacker] = BinaryHelper.ReadInt16(inFile,"AssistantSacker");  // 0-10 (Assistant Sacker, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.AssistantTackler] = BinaryHelper.ReadInt16(inFile,"AssistantTackler"); // 0-10 (Assistant Tackler, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.IsAssistedTackle] = BinaryHelper.ReadInt16(inFile,"IsAssistedTackle"); // 0-1 (1: Assisted Tackle)
			playData.TypeSpecificData[(int)PassPlayFields.WhoAllowedQBSack] = BinaryHelper.ReadInt16(inFile,"WhoAllowedQBSack"); // 0-10 (QB Sack Allowed By, based on position in formation)
			playData.TypeSpecificData[(int)PassPlayFields.IncompletionType] = BinaryHelper.ReadInt16(inFile,"IncompletionType");  // 0-4 (Incompletion Type, 0: Drop, 1: Incomplete (possibly evading rush with later key), 2: Pass Defended, 3: Blocked at the line, 4: Hurried)
			playData.TypeSpecificData[(int)PassPlayFields.IsOverMiddleOfField] = BinaryHelper.ReadInt16(inFile,"IsOverMiddleOfField");  // 0-1 (1: Over Middle of the Field)
			playData.TypeSpecificData[(int)PassPlayFields.YardsAfterCatch] = BinaryHelper.ReadInt16(inFile,"YardsAfterCatch");  // ??-?? (Yards After Catch)
			playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage1Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage1Type");  // Game Log Message re: Sack (" ran right past ")
			playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage2Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage2Type");  // Game Log Message re: Knockdown (" knocked the ball right out of his grasp ")
			playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage3Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage3Type");  // Game Log Message 
			playData.TypeSpecificData[(int)PassPlayFields.DoubleCoverage] = BinaryHelper.ReadInt16(inFile,"DoubleCoverage");  // 0-3 (1: threw into double coverage (primary), 2: threw into double coverage (secondary), 3: threw away from double coverage)
			playData.TypeSpecificData[(int)PassPlayFields.PrePlayDown] = BinaryHelper.ReadInt16(inFile,"PrePlayDown");      // 1-4 (Pre-Play Down)
			playData.TypeSpecificData[(int)PassPlayFields.PrePlayYardsToGo] = BinaryHelper.ReadInt16(inFile,"PrePlayYardsToGo"); // 1-?? (Pre-Play Yards To Go)
			playData.TypeSpecificData[(int)PassPlayFields.PrePlayYardLine] = BinaryHelper.ReadInt16(inFile,"PrePlayYardLine");  // 0-100 (Pre-Play Yard Line)
			playData.TypeSpecificData[(int)PassPlayFields.PrePlayTeamPossession] = BinaryHelper.ReadInt16(inFile,"PrePlayTeamPossession");    // 0-1 (Pre-Play Possession)
			playData.TypeSpecificData[(int)PassPlayFields.PostPlayDown] = BinaryHelper.ReadInt16(inFile,"PostPlayDown");     // 1-4 (Post-Play Down)
			playData.TypeSpecificData[(int)PassPlayFields.PostPlayYardsToGo] = BinaryHelper.ReadInt16(inFile,"PostPlayYardsToGo");    // 1-?? (Post-Play Yards to Go)
			playData.TypeSpecificData[(int)PassPlayFields.PostPlayYardLine] = BinaryHelper.ReadInt16(inFile,"PostPlayYardLine"); // 0-100 (Post-Play Yard Line)
			playData.TypeSpecificData[(int)PassPlayFields.IsTurnover] = BinaryHelper.ReadInt16(inFile,"IsTurnover");       // 0-1 (1: Turnover)
			short dat53 = BinaryHelper.ReadInt16(inFile, "dat53");
			playData.TypeSpecificData[(int)PassPlayFields.IsTurnoverOnDowns] = BinaryHelper.ReadInt16(inFile, "IsTurnoverOnDowns");    // 0-1 (1: Turnover on Downs)
			short dat55 = BinaryHelper.ReadInt16(inFile, "dat55");
			short dat56 = BinaryHelper.ReadInt16(inFile, "dat56");
			short dat57 = BinaryHelper.ReadInt16(inFile, "dat57");
			playData.TypeSpecificData[(int)PassPlayFields.PassDistance] = BinaryHelper.ReadInt16(inFile,"PassDistance");      // 0-8 (Pass Distance, 0: Screen ... 8: Spike)
			playData.TypeSpecificData[(int)PassPlayFields.DefenseFamiliar] = BinaryHelper.ReadInt16(inFile,"DefenseFamiliar");// 0-2 (Defense Familiar: 0: None, 1: very familiar, 2: extremely familiar - not always displayed, depends on play result)
			playData.TypeSpecificData[(int)PassPlayFields.EvadedRushToAvoidSafety] = BinaryHelper.ReadInt16(inFile,"EvadedRushToAvoidSafety");  // 0-1 (1: Evades Rush to avoid the safety)
			playData.TypeSpecificData[(int)PassPlayFields.FieldCondition] = BinaryHelper.ReadInt16(inFile,"FieldCondition");  // 0-5 (Field Condition, 0: Norm, 1: cold, 2: hot, 3: wet, 4: snowy, 5: soaked - not always displayed, depends on play result)
			playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage4Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage4Type");  // Game Log Message re: Turnover on Downs

			if (gPassPlayDumpFile != null)
			{
				int weekNum = gCurGameLog.Week - 1;
				string gameID = gCurGameLog.Year + weekNum.ToString("D2") + gCurGameLog.AwayTeam.TeamIndex.ToString("D2") + gCurGameLog.HomeTeam.TeamIndex.ToString("D2");
				gPassPlayDumpFile.Write("," + gameID + "," + CurPlayID + "," + gCurGameLog.AwayTeam.Abbreviation + "," + gCurGameLog.HomeTeam.Abbreviation + "," + playData.Quarter + "," + playData.Minutes + "," + playData.Seconds + "," + playData.Possession);
				gPassPlayDumpFile.Write("," + dat02);
				gPassPlayDumpFile.Write("," + dat03);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsComplete]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.YardsGained]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsTouchdown]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PassTarget]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.Tackler]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsFumble]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoveryTeam]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.FumbleReturnYards]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoveryTackler]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsFumbleRecoveredForTD]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoverer]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsInterception]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.InterceptingPlayer]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.InterceptionReturnYards]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsInterceptedForTD]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.InterceptedTackler]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.InterceptionYardLine]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsPenaltyAccepted]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsDefensiveTD]);
				gPassPlayDumpFile.Write("," + dat23);
				gPassPlayDumpFile.Write("," + dat24);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsQBScramble]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.QBScrambleYards]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsQBSacked]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.QBSackYards]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsQBSackedForSafety]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.SackingPlayer]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsForcedOOB]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.InterceptedInEndZone]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsHalfSack]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.AssistantSacker]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.AssistantTackler]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsAssistedTackle]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.WhoAllowedQBSack]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IncompletionType]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsOverMiddleOfField]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.YardsAfterCatch]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage1Type]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage2Type]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage3Type]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.DoubleCoverage]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PrePlayDown]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PrePlayYardsToGo]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PrePlayYardLine]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PrePlayTeamPossession]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PostPlayDown]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PostPlayYardsToGo]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PostPlayYardLine]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsTurnover]);
				gPassPlayDumpFile.Write("," + dat53);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.IsTurnoverOnDowns]);
				gPassPlayDumpFile.Write("," + dat55);
				gPassPlayDumpFile.Write("," + dat56);
				gPassPlayDumpFile.Write("," + dat57);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.PassDistance]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.DefenseFamiliar]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.EvadedRushToAvoidSafety]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.FieldCondition]);
				gPassPlayDumpFile.Write("," + playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage4Type]);
			}

			BinaryHelper.TracerOutdent();
		}

		private void LoadRunPlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Run Play");
			BinaryHelper.TracerIndent();

			if (gRunPlayDumpFile != null)
			{
				int weekNum = gCurGameLog.Week - 1;
				string gameID = gCurGameLog.Year + weekNum.ToString("D2") + gCurGameLog.AwayTeam.TeamIndex.ToString("D2") + gCurGameLog.HomeTeam.TeamIndex.ToString("D2");
				gRunPlayDumpFile.Write("," + gameID + "," + CurPlayID + "," + gCurGameLog.AwayTeam.Abbreviation + "," + gCurGameLog.HomeTeam.Abbreviation + "," + playData.Quarter + "," + playData.Minutes + "," + playData.Seconds + "," + playData.Possession);
			}

			playData.TypeSpecificData[(int)RunPlayFields.Minute] = BinaryHelper.ReadInt16(inFile,"Minute");   // 0-15 (Minute)
			playData.TypeSpecificData[(int)RunPlayFields.Seconds] = BinaryHelper.ReadInt16(inFile,"Seconds");  // 0-59 (Second)
			short dat02 = BinaryHelper.ReadInt16(inFile, "Dat02");
			short dat03 = BinaryHelper.ReadInt16(inFile, "Dat03");
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + dat02);
				gRunPlayDumpFile.Write("," + dat03);
			}
			playData.TypeSpecificData[(int)RunPlayFields.YardsGained] = BinaryHelper.ReadInt16(inFile,"YardsGained");
			playData.TypeSpecificData[(int)RunPlayFields.IsTouchdown] = BinaryHelper.ReadInt16(inFile,"IsTouchdown");  //	0-1 (1: Touchdown)
			playData.TypeSpecificData[(int)RunPlayFields.Rusher] = BinaryHelper.ReadInt16(inFile,"Rusher");       // 0-10 (Rusher, based on position in formation)
			playData.TypeSpecificData[(int)RunPlayFields.IsFumble] = BinaryHelper.ReadInt16(inFile,"IsFumble");     // 0-1 (1: Fumble)
			playData.TypeSpecificData[(int)RunPlayFields.Tackler] = BinaryHelper.ReadInt16(inFile,"Tackler");      // 0-10 (Player Tackler, based on position in formation)
			playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryTeam] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveryTeam");   // 0-1 (Fumble Recovered by which team)
			playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryYards] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveryYards");  // ??-?? (Fumble Return yards)
			playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveredForTD] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveredForTD"); // 0-1 (1: Fumble Recovered for TD)
			playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryTackler] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveryTackler");    // 0-10 (Tackler of Fumble Recover, based on position in formation)
			playData.TypeSpecificData[(int)RunPlayFields.IsPenaltyAccepted] = BinaryHelper.ReadInt16(inFile,"IsPenaltyAccepted");    // 0-1 (1: Accepted Penalty)
			playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoverer] = BinaryHelper.ReadInt16(inFile,"FumbleRecoverer");      // 0-10 (Fumble Recoverer)
			playData.TypeSpecificData[(int)RunPlayFields.TrueRushYardLine] = BinaryHelper.ReadInt16(inFile,"TrueRushYardLine");     // ??-?? (True Yard Line: interesting piece of data, is how far a rush would have ended without an end zone.  So a run from the 1 yard line for a TD could have a value here of 117, meaning it would have been an 18-yard rush if somewhere else on the field.  Haven't done anything with this, but there's potential for figuring out a 'true' yards/carry)
			short dat16 = BinaryHelper.ReadInt16(inFile, "Dat16");
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + playData.TypeSpecificData[(int)RunPlayFields.YardsGained] + "," + playData.TypeSpecificData[(int)RunPlayFields.IsTouchdown]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.Rusher] + "," + playData.TypeSpecificData[(int)RunPlayFields.IsFumble]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.Tackler] + "," + playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryTeam]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryYards] + "," + playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveredForTD]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryTackler] + "," + playData.TypeSpecificData[(int)RunPlayFields.IsPenaltyAccepted]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoverer] + "," + playData.TypeSpecificData[(int)RunPlayFields.TrueRushYardLine]);
				gRunPlayDumpFile.Write("," + dat16);
			}
			playData.TypeSpecificData[(int)RunPlayFields.IsForcedOOB] = BinaryHelper.ReadInt16(inFile, "IsForcedOOB");          // 0-1 (1: Forced OOB)
			playData.TypeSpecificData[(int)RunPlayFields.IsSafety] = BinaryHelper.ReadInt16(inFile,"IsSafety");             // 0-1 (1: Safety)
			playData.TypeSpecificData[(int)RunPlayFields.AssistantTackler] = BinaryHelper.ReadInt16(inFile,"AssistantTackler");     // 0-10 (Player Assisting on Tackle, based on position in formation)
			playData.TypeSpecificData[(int)RunPlayFields.WasTackleAssisted] = BinaryHelper.ReadInt16(inFile,"WasTackleAssisted");    // 0-1 (1: Assisted Tackle)
			playData.TypeSpecificData[(int)RunPlayFields.KeyRunBlocker] = BinaryHelper.ReadInt16(inFile,"KeyRunBlocker");        // 0-10 (0: No Key Run Block, 1-10: Player getting the KRB, based on position in formation)
			playData.TypeSpecificData[(int)RunPlayFields.KeyRunBlockOpportunity] = BinaryHelper.ReadInt16(inFile,"KeyRunBlockOpportunity");   // 0-10 (0: No Key Run Block Opportunity, 1-10: Player getting the KRBO, based on position in formation)
			short dat23 = BinaryHelper.ReadInt16(inFile, "Dat23");
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + playData.TypeSpecificData[(int)RunPlayFields.IsForcedOOB] + "," + playData.TypeSpecificData[(int)RunPlayFields.IsSafety]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.AssistantTackler] + "," + playData.TypeSpecificData[(int)RunPlayFields.WasTackleAssisted]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.KeyRunBlocker]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.KeyRunBlockOpportunity]);
				gRunPlayDumpFile.Write("," + dat23);
			}
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage1Type] = BinaryHelper.ReadInt16(inFile, "GameLogMessage1Type");  // Game Log Message Type re: Making a Move (" made a great move on ")
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage1Player] = BinaryHelper.ReadInt16(inFile,"GameLogMessage1Player");// Player referenced in Play24
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage2Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage2Type");  // Game Log Message re: Key Run Block (" ran over ")
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage2Player] = BinaryHelper.ReadInt16(inFile,"GameLogMessage2Player");// Player referenced in Play26
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage3Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage3Type");  // Game Log Message re: Failed Key Run Block (" to break up the play ")
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage3Player] = BinaryHelper.ReadInt16(inFile,"GameLogMessage3Player");// Player referenced in Play28
			playData.TypeSpecificData[(int)RunPlayFields.PrePlayDown] = BinaryHelper.ReadInt16(inFile,"PrePlayDown");          // 1-4 (Pre-Play Down)
			playData.TypeSpecificData[(int)RunPlayFields.PrePlayYardsToGo] = BinaryHelper.ReadInt16(inFile,"PrePlayYardsToGo");     // 1-?? (Pre-Play Yards To Go)
			playData.TypeSpecificData[(int)RunPlayFields.PrePlayYardLine] = BinaryHelper.ReadInt16(inFile,"PrePlayYardLine");      // 0-100 (Pre-Play Yard Line)
			playData.TypeSpecificData[(int)RunPlayFields.PrePlayTeamPossession] = BinaryHelper.ReadInt16(inFile,"PrePlayTeamPossession");// 0-1 (Pre-Play Possession)
			playData.TypeSpecificData[(int)RunPlayFields.PostPlayDown] = BinaryHelper.ReadInt16(inFile,"PostPlayDown");         // 1-4 (Post-Play Down)
			playData.TypeSpecificData[(int)RunPlayFields.PostPlayYardsToGo] = BinaryHelper.ReadInt16(inFile,"PostPlayYardsToGo");    // 1-?? (Post-Play Yards To Go)
			playData.TypeSpecificData[(int)RunPlayFields.PostPlayYardLine] = BinaryHelper.ReadInt16(inFile,"PostPlayYardLine");     // 0-100 (Post-Play Yard Line)
			playData.TypeSpecificData[(int)RunPlayFields.IsTurnover] = BinaryHelper.ReadInt16(inFile,"IsTurnover");           // 0-1 (Turnover Indicator)
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage1Type] + "," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage1Player]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage2Type] + "," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage2Player]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage3Type] + "," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage3Player]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.PrePlayDown] + "," + playData.TypeSpecificData[(int)RunPlayFields.PrePlayYardsToGo]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.PrePlayYardLine] + "," + playData.TypeSpecificData[(int)RunPlayFields.PrePlayTeamPossession]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.PostPlayDown] + "," + playData.TypeSpecificData[(int)RunPlayFields.PostPlayYardsToGo]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.PostPlayYardLine] + "," + playData.TypeSpecificData[(int)RunPlayFields.IsTurnover]
					);
			}
			short dat38 = BinaryHelper.ReadInt16(inFile, "Dat38");
			playData.TypeSpecificData[(int)RunPlayFields.TurnoverOnDowns] = BinaryHelper.ReadInt16(inFile, "TurnoverOnDowns");      // 0-1 (1: Turnover on Downs)
			short dat40 = BinaryHelper.ReadInt16(inFile, "Dat40");
			short dat41 = BinaryHelper.ReadInt16(inFile, "Dat41");
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + dat38 + "," + playData.TypeSpecificData[(int)RunPlayFields.TurnoverOnDowns]
					+ "," + dat40 + "," + dat41
					);
			}
			playData.TypeSpecificData[(int)RunPlayFields.RunDirection] = BinaryHelper.ReadInt16(inFile, "RunDirection");  // 0-9 (Run Direction, 0: around left end, 1: outside LT, 2: inside LT ... 7: around RE, 8: left reverse (finesse only), 9: right reverse (finesse only))
			playData.TypeSpecificData[(int)RunPlayFields.IsFinesseRun] = BinaryHelper.ReadInt16(inFile,"IsFinesseRun");         // 0-1 (1: Finesse Run, different messages used)
			playData.TypeSpecificData[(int)RunPlayFields.FieldCondition] = BinaryHelper.ReadInt16(inFile,"FieldCondition");  // 0-5 (Field Condition, 0: Norm, 1: cold, 2: hot, 3: wet, 4: snowy, 5: soaked - not always displayed, depends on play result)
			playData.TypeSpecificData[(int)RunPlayFields.DefenseFamiliar] = BinaryHelper.ReadInt16(inFile,"DefenseFamiliar");// 0-2 (Defense Familiar: 0: None, 1: very familiar, 2: extremely familiar - not always displayed, depends on play result)
			playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage4Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage4Type");  // Game Log Message re: Turnover on Downs
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + playData.TypeSpecificData[(int)RunPlayFields.RunDirection] + "," + playData.TypeSpecificData[(int)RunPlayFields.IsFinesseRun]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.FieldCondition] + "," + playData.TypeSpecificData[(int)RunPlayFields.DefenseFamiliar]
					+ "," + playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage4Type]
					);
			}
			short dat47 = BinaryHelper.ReadInt16(inFile, "Dat47");
			short dat48 = BinaryHelper.ReadInt16(inFile, "Dat48");
			short dat49 = BinaryHelper.ReadInt16(inFile, "Dat49");
			short dat50 = BinaryHelper.ReadInt16(inFile, "Dat50");
			short dat51 = BinaryHelper.ReadInt16(inFile, "Dat51");
			short dat52 = BinaryHelper.ReadInt16(inFile, "Dat52");
			short dat53 = BinaryHelper.ReadInt16(inFile, "Dat53");
			short dat54 = BinaryHelper.ReadInt16(inFile, "Dat54");
			short dat55 = BinaryHelper.ReadInt16(inFile, "Dat55");
			short dat56 = BinaryHelper.ReadInt16(inFile, "Dat56");
			short dat57 = BinaryHelper.ReadInt16(inFile, "Dat57");
			short dat58 = BinaryHelper.ReadInt16(inFile, "Dat58");
			short dat59 = BinaryHelper.ReadInt16(inFile, "Dat59");
			short dat60 = BinaryHelper.ReadInt16(inFile, "Dat60");
			short dat61 = BinaryHelper.ReadInt16(inFile, "Dat61");
			short dat62 = BinaryHelper.ReadInt16(inFile, "Dat62");
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Write("," + dat47 + "," + dat48 + "," + dat49 + "," + dat50 + "," + dat51 + "," + dat52 + "," + dat53 + "," + dat54
					+ "," + dat55 + "," + dat56 + "," + dat57 + "," + dat58 + "," + dat59 + "," + dat60 + "," + dat61 + "," + dat62
					);
			}

			BinaryHelper.TracerOutdent();
		}

		private void LoadKickoffPlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Kickoff Play");
			BinaryHelper.TracerIndent();

			playData.TypeSpecificData[(int)KickoffPlayFields.Minute] = BinaryHelper.ReadInt16(inFile,"Minute");
			playData.TypeSpecificData[(int)KickoffPlayFields.Second] = BinaryHelper.ReadInt16(inFile,"Second");
			BinaryHelper.ProbeBytes(inFile, 4);
			playData.TypeSpecificData[(int)KickoffPlayFields.KickingTeam] = BinaryHelper.ReadInt16(inFile,"KickingTeam");
			playData.TypeSpecificData[(int)KickoffPlayFields.PenaltyAccepted] = BinaryHelper.ReadInt16(inFile,"PenaltyAccepted");
			playData.TypeSpecificData[(int)KickoffPlayFields.ReturnYardLine] = BinaryHelper.ReadInt16(inFile,"ReturnYardLine");
			BinaryHelper.ProbeBytes(inFile, 4);
			//short Unknown = BinaryHelper.ReadInt16(inFile,"Unknown");   // 0-1 Range
			//short Unknown = BinaryHelper.ReadInt16(inFile,"Unknown");   // 0, 9-10 Range (Could be returner?  Always matches 'Returner', except sometimes is 0)
			playData.TypeSpecificData[(int)KickoffPlayFields.ReturnYards] = BinaryHelper.ReadInt16(inFile,"ReturnYards");
			playData.TypeSpecificData[(int)KickoffPlayFields.FumbleOnReturn] = BinaryHelper.ReadInt16(inFile,"FumbleOnReturn");
			playData.TypeSpecificData[(int)KickoffPlayFields.Turnover] = BinaryHelper.ReadInt16(inFile,"Turnover");
			playData.TypeSpecificData[(int)KickoffPlayFields.FumbleTouchdown] = BinaryHelper.ReadInt16(inFile,"FumbleTouchdown");
			playData.TypeSpecificData[(int)KickoffPlayFields.FumbleReturnYards] = BinaryHelper.ReadInt16(inFile,"FumbleReturnYards");
			playData.TypeSpecificData[(int)KickoffPlayFields.FumbleRecoverer] = BinaryHelper.ReadInt16(inFile,"FumbleRecoverer");
			BinaryHelper.ProbeBytes(inFile, 4);
			playData.TypeSpecificData[(int)KickoffPlayFields.Tackler] = BinaryHelper.ReadInt16(inFile,"Tackler");
			playData.TypeSpecificData[(int)KickoffPlayFields.Touchdown] = BinaryHelper.ReadInt16(inFile,"Touchdown");
			BinaryHelper.ProbeBytes(inFile, 2);
			playData.TypeSpecificData[(int)KickoffPlayFields.KickoffYardLine] = BinaryHelper.ReadInt16(inFile,"KickoffYardLine");
			BinaryHelper.ProbeBytes(inFile, 8);
			playData.TypeSpecificData[(int)KickoffPlayFields.KickoffDistance] = BinaryHelper.ReadInt16(inFile,"KickoffDistance");
			playData.TypeSpecificData[(int)KickoffPlayFields.Touchback] = BinaryHelper.ReadInt16(inFile,"Touchback");
			playData.TypeSpecificData[(int)KickoffPlayFields.Returner] = BinaryHelper.ReadInt16(inFile,"Returner");
			playData.TypeSpecificData[(int)KickoffPlayFields.FumbleRecoveryYardLine] = BinaryHelper.ReadInt16(inFile,"FumbleRecoveryYardLine");
			BinaryHelper.ProbeBytes(inFile, 68);

			BinaryHelper.TracerOutdent();
		}

		private void LoadOnsidePlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Onside Play");
			BinaryHelper.TracerIndent();

			if (gOnsideKickPlayDumpFile != null)
			{
				int weekNum = gCurGameLog.Week - 1;
				string gameID = gCurGameLog.Year + weekNum.ToString("D2") + gCurGameLog.AwayTeam.TeamIndex.ToString("D2") + gCurGameLog.HomeTeam.TeamIndex.ToString("D2");
				gOnsideKickPlayDumpFile.Write("," + gameID + "," + CurPlayID + "," + gCurGameLog.AwayTeam.Abbreviation + "," + gCurGameLog.HomeTeam.Abbreviation + "," + playData.Quarter + "," + playData.Minutes + "," + playData.Seconds + "," + playData.Possession);
			}

			for (int i = 0; i < 63; ++i)
			{
				short data = BinaryHelper.ReadInt16(inFile, "OnsideData" + i);
				if (gOnsideKickPlayDumpFile != null)
				{
					gOnsideKickPlayDumpFile.Write("," + data);
				}
			}

			BinaryHelper.TracerOutdent();
		}

		private void LoadPuntPlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Punt Play");
			BinaryHelper.TracerIndent();

			if (gPuntPlayDumpFile != null)
			{
				int weekNum = gCurGameLog.Week - 1;
				string gameID = gCurGameLog.Year + weekNum.ToString("D2") + gCurGameLog.AwayTeam.TeamIndex.ToString("D2") + gCurGameLog.HomeTeam.TeamIndex.ToString("D2");
				gPuntPlayDumpFile.Write("," + gameID + "," + CurPlayID + "," + gCurGameLog.AwayTeam.Abbreviation + "," + gCurGameLog.HomeTeam.Abbreviation + "," + playData.Quarter + "," + playData.Minutes + "," + playData.Seconds + "," + playData.Possession);
			}

			for (int i = 0; i < 63; ++i)
			{
				short data = BinaryHelper.ReadInt16(inFile, "PuntData" + i);
				if (gPuntPlayDumpFile != null)
				{
					gPuntPlayDumpFile.Write("," + data);
				}
			}

			BinaryHelper.TracerOutdent();
		}

		private void LoadInfoPlayData(System.IO.BinaryReader inFile, GamePlay playData)
		{
			BinaryHelper.TracerWriteLine("Info Play");
			BinaryHelper.TracerIndent();

			if (gInfoPlayDumpFile != null)
			{
				int weekNum = gCurGameLog.Week - 1;
				string gameID = gCurGameLog.Year + weekNum.ToString("D2") + gCurGameLog.AwayTeam.TeamIndex.ToString("D2") + gCurGameLog.HomeTeam.TeamIndex.ToString("D2");
				gInfoPlayDumpFile.Write("," + gameID + "," + CurPlayID + "," + gCurGameLog.AwayTeam.Abbreviation + "," + gCurGameLog.HomeTeam.Abbreviation + "," + playData.Quarter + "," + playData.Minutes + "," + playData.Seconds + "," + playData.Possession);
			}

			for (int i = 0; i < 63; ++i)
			{
				short data = BinaryHelper.ReadInt16(inFile,"InfoData"+i);
				if (gInfoPlayDumpFile != null && i >= 2 && i <= 18)
				{
					gInfoPlayDumpFile.Write("," + data);
				}
			}
			//BinaryHelper.ProbeBytes(inFile, 63 * 2);

			BinaryHelper.TracerOutdent();
		}

		private GamePlay LoadPlayData(System.IO.BinaryReader inFile)
		{
			BinaryHelper.TracerWriteLine("Play Data");
			BinaryHelper.TracerIndent();

			GamePlay playData = new GamePlay();

			// Main Play Data
			playData.Quarter = BinaryHelper.ReadInt16(inFile, "Quarter");
			playData.Minutes = BinaryHelper.ReadInt16(inFile, "Minutes");
			playData.Seconds = BinaryHelper.ReadInt16(inFile, "Seconds");
			playData.Possession = BinaryHelper.ReadInt16(inFile, "Possession");
			playData.Down = BinaryHelper.ReadInt16(inFile, "Down");
			playData.YardsToGo = BinaryHelper.ReadInt16(inFile, "YardsToGo");
			playData.YardLine = BinaryHelper.ReadInt16(inFile, "YardLine");
			playData.HomeTimeouts = BinaryHelper.ReadInt16(inFile, "Home Timeouts");
			playData.AwayTimeouts = BinaryHelper.ReadInt16(inFile, "Away Timeouts");
			playData.PlayType = BinaryHelper.ReadInt16(inFile, "Play Type");

			// Formation Data, mostly valid for playtype 5 or 6
			playData.OffensiveFormation = BinaryHelper.ReadInt16(inFile, "Offensive Formation");	
			playData.OffensiveStrength = BinaryHelper.ReadInt16(inFile, "Offensive Strength");
			playData.DefensiveAlignment = BinaryHelper.ReadInt16(inFile, "Defensive Alignment");
			playData.DefensivePersonnel = BinaryHelper.ReadInt16(inFile, "Defensive Personnel");
			playData.DefensiveCoverage = BinaryHelper.ReadInt16(inFile, "Defensive Coverage");
			playData.DefensiveRunPass = BinaryHelper.ReadInt16(inFile, "Defensive RunPass");
			playData.DefensiveRunAggressive = BinaryHelper.ReadInt16(inFile, "Defensive RunAggressive");
			playData.DefensivePassAggressive = BinaryHelper.ReadInt16(inFile, "Defensive PassAggressive");
			playData.DefensiveBlitzCount = BinaryHelper.ReadInt16(inFile, "Defensive Blitzing");    // 0-3, how many blitzing
			for (int blitzer=0;blitzer<10;++blitzer)
			{
				playData.DefensiveBlitzers[blitzer] = BinaryHelper.ReadInt16(inFile, "Defensive Blitzer "+blitzer.ToString());
			}

			// Penalty Data
			playData.IsDefensivePenalty = BinaryHelper.ReadInt16(inFile, "IsDefensivePenalty");
			playData.IsOffensivePenalty = BinaryHelper.ReadInt16(inFile, "IsOffensivePenalty");
			playData.PenaltyYardage = BinaryHelper.ReadInt16(inFile, "PenaltyYardage");
			playData.HappenedOnPuntOrKick = BinaryHelper.ReadInt16(inFile, "HappenedOnPuntOrKick");
			playData.ResultsInFirstDown = BinaryHelper.ReadInt16(inFile, "ResultsInFirstDown");
			playData.ResultsInLossOfDown = BinaryHelper.ReadInt16(inFile, "ResultsInLossOfDown");
			BinaryHelper.ProbeBytes(inFile, 4);
			playData.InEndZone = BinaryHelper.ReadInt16(inFile, "InEndZone");
			playData.YardLineIfAccepted = BinaryHelper.ReadInt16(inFile, "YardLineIfAccepted");
			playData.DownIfAccepted = BinaryHelper.ReadInt16(inFile, "DownIfAccepted");
			playData.YardsToGoIfAccepted = BinaryHelper.ReadInt16(inFile, "YardsToGoIfAccepted");
			playData.YardLineIfDeclined = BinaryHelper.ReadInt16(inFile, "YardLineIfDeclined");
			playData.DownIfDeclined = BinaryHelper.ReadInt16(inFile, "DownIfDeclined");
			playData.YardsToGoIfDeclined = BinaryHelper.ReadInt16(inFile, "YardsToGoIfDeclined");
			BinaryHelper.ProbeBytes(inFile, 8);
			playData.PenaltyType = BinaryHelper.ReadInt16(inFile, "Penalty Type");
			playData.IsDefensiveEndOfHalfPenalty = BinaryHelper.ReadInt16(inFile, "IsDefensiveEndOfHalfPenalty");
			BinaryHelper.ProbeBytes(inFile, 10);
			playData.EffectOnPlay = BinaryHelper.ReadInt16(inFile, "EffectOnPlay");

			// Injury?
			playData.InjuryType = BinaryHelper.ReadInt16(inFile, "InjuryType");
			playData.InjuredPlayer = BinaryHelper.ReadInt16(inFile, "InjuredPlayer");

			// Specific play data based on play type
			if (playData.PlayType == 1)
			{
				LoadFGPlayData(inFile, playData);
			}
			else if (playData.PlayType == 2)
			{
				LoadKickoffPlayData(inFile, playData);
			}
			else if (playData.PlayType == 3)
			{
				LoadOnsidePlayData(inFile, playData);
			}
			else if (playData.PlayType == 4)
			{
				LoadPuntPlayData(inFile, playData);
			}
			else if (playData.PlayType == 5)
			{
				LoadRunPlayData(inFile, playData);
			}
			else if (playData.PlayType == 6)
			{
				LoadPassPlayData(inFile, playData);
			}
			else
			{
				LoadInfoPlayData(inFile, playData);
			}

			// Offensive Lineup
			for (int offensivePlayer = 0; offensivePlayer < playData.OffensivePlayers.Length; ++offensivePlayer)
			{
				playData.OffensivePlayers[offensivePlayer] = BinaryHelper.ReadInt16(inFile, "Offensive Player " + offensivePlayer.ToString());
				if (playData.OffensivePlayers[offensivePlayer] >= kNumActivePlayers)
				{
					playData.OffensivePlayers[offensivePlayer] -= kNumActivePlayers;
				}
			}

			// Defensive Lineup
			for (int defensivePlayer = 0; defensivePlayer < playData.DefensivePlayers.Length; ++defensivePlayer)
			{
				playData.DefensivePlayers[defensivePlayer] = BinaryHelper.ReadInt16(inFile, "DefensivePlayers " + defensivePlayer.ToString());
				if (playData.DefensivePlayers[defensivePlayer] >= kNumActivePlayers)
				{
					playData.DefensivePlayers[defensivePlayer] -= kNumActivePlayers;
				}
			}

			// Finish off the info play lines to CSV by writing out the players involved in the play.
			if (playData.PlayType >= 7 && gInfoPlayDumpFile != null)
			{
				WritePlayersOnField(playData, gInfoPlayDumpFile);
			}
			else if (playData.PlayType == 1 && gFGPlayDumpFile != null)
			{
				WritePlayersOnField(playData, gFGPlayDumpFile);
			}
			else if (playData.PlayType == 3 && gOnsideKickPlayDumpFile != null)
			{
				WritePlayersOnField(playData, gOnsideKickPlayDumpFile);
			}
			else if (playData.PlayType == 4 && gPuntPlayDumpFile != null)
			{
				WritePlayersOnField(playData, gPuntPlayDumpFile);
			}
			else if (playData.PlayType == 5 && gRunPlayDumpFile != null)
			{
				WritePlayersOnField(playData, gRunPlayDumpFile);
			}
			else if (playData.PlayType == 6 && gPassPlayDumpFile != null)
			{
				WritePlayersOnField(playData, gPassPlayDumpFile);
			}

			BinaryHelper.TracerOutdent();

			return playData;
		}

		private void WritePlayersOnField(GamePlay playData, System.IO.StreamWriter dumpFile)
		{
			for (short off = 0; off < 11; ++off)
			{
				PlayerHistoricalRecord playerRec = GetOffensivePlayerHistoricalFromPlay(gCurGameLog, playData, off);
				if (playerRec != null)
				{
					dumpFile.Write(",\"" + playerRec.LastName + "\"");
				}
				else
				{
					dumpFile.Write(",");
				}
			}
			for (short def = 0; def < 11; ++def)
			{
				PlayerHistoricalRecord playerRec = GetDefensivePlayerHistoricalFromPlay(gCurGameLog, playData, def);
				if (playerRec != null)
				{
					dumpFile.Write(",\"" + playerRec.LastName + "\"");
				}
				else
				{
					dumpFile.Write(",");
				}
			}
			dumpFile.WriteLine();
		}

		private void LoadGameDriveInfo(System.IO.BinaryReader inFile, GameDriveInfo driveInfo)
		{
			BinaryHelper.TracerIndent();

			driveInfo.DriveStartQuarter = BinaryHelper.ReadInt16(inFile, "StartQuarter");
			driveInfo.DriveStartMinutes = BinaryHelper.ReadInt16(inFile, "StartMinutes");
			driveInfo.DriveStartSeconds = BinaryHelper.ReadInt16(inFile, "StartSeconds");
			driveInfo.DriveEndQuarter = BinaryHelper.ReadInt16(inFile, "EndQuarter");
			driveInfo.DriveEndMinutes = BinaryHelper.ReadInt16(inFile, "EndMinutes");
			driveInfo.DriveEndSeconds = BinaryHelper.ReadInt16(inFile, "EndSeconds");
			driveInfo.YardsFromGoalStart = BinaryHelper.ReadInt16(inFile, "YardsFromGoal");
			driveInfo.Plays = BinaryHelper.ReadInt16(inFile, "Plays");
			driveInfo.YardsGained = BinaryHelper.ReadInt16(inFile, "YardsGained");
			driveInfo.Result = BinaryHelper.ReadInt16(inFile, "Result");

			BinaryHelper.TracerOutdent();
		}

		private void LoadGamePassingInfo(System.IO.BinaryReader inFile, ref GamePassInfo passInfo)
		{
			BinaryHelper.TracerIndent();

			passInfo.ScreenAttempts = BinaryHelper.ReadInt16(inFile, "ScreenAttempts");
			passInfo.ScreenCompletions = BinaryHelper.ReadInt16(inFile, "ScreenCompletions");
			passInfo.ScreenYards = BinaryHelper.ReadInt16(inFile, "ScreenYards");
			passInfo.ShortAttempts = BinaryHelper.ReadInt16(inFile, "ShortAttempts");
			passInfo.ShortCompletions = BinaryHelper.ReadInt16(inFile, "ShortCompletions");
			passInfo.ShortYards = BinaryHelper.ReadInt16(inFile, "ShortYards");
			passInfo.MediumAttempts = BinaryHelper.ReadInt16(inFile, "MediumAttempts");
			passInfo.MediumCompletions = BinaryHelper.ReadInt16(inFile, "MediumCompletions");
			passInfo.MediumYards = BinaryHelper.ReadInt16(inFile, "MediumYards");
			passInfo.LongAttempts = BinaryHelper.ReadInt16(inFile, "LongAttempts");
			passInfo.LongCompletions = BinaryHelper.ReadInt16(inFile, "LongCompletions");
			passInfo.LongYards = BinaryHelper.ReadInt16(inFile, "LongYards");
			passInfo.OtherAttempts = BinaryHelper.ReadInt16(inFile, "OtherAttempts");
			passInfo.OtherCompletions = BinaryHelper.ReadInt16(inFile, "OtherCompletions");
			passInfo.OtherYards = BinaryHelper.ReadInt16(inFile, "OtherYards");

			BinaryHelper.TracerOutdent();
		}

		private void LoadGameRushingInfo(System.IO.BinaryReader inFile, ref GameRushInfo rushInfo)
		{
			BinaryHelper.TracerIndent();

			rushInfo.LeftAttempts = BinaryHelper.ReadInt16(inFile, "LeftAttempts");
			rushInfo.LeftYards = BinaryHelper.ReadInt16(inFile, "LeftYards");
			rushInfo.MiddleAttempts = BinaryHelper.ReadInt16(inFile, "MiddleAttempts");
			rushInfo.MiddleYards = BinaryHelper.ReadInt16(inFile, "MiddleYards");
			rushInfo.RightAttempts = BinaryHelper.ReadInt16(inFile, "RightAttempts");
			rushInfo.RightYards = BinaryHelper.ReadInt16(inFile, "RightYards");
			rushInfo.OtherAttempts = BinaryHelper.ReadInt16(inFile, "OtherAttempts");
			rushInfo.OtherYards = BinaryHelper.ReadInt16(inFile, "OtherYards");

			BinaryHelper.TracerOutdent();
		}

		private void LoadGamePossessionInfo(System.IO.BinaryReader inFile, ref GamePossessionInfo possessionInfo)
		{
			BinaryHelper.TracerIndent();

			possessionInfo.TimeOfPossession = BinaryHelper.ReadInt16(inFile, "TimeOfPossession");
			possessionInfo.RedZoneAttempts = BinaryHelper.ReadInt16(inFile, "RedZoneAttempts");
			possessionInfo.RedZoneTouchdowns = BinaryHelper.ReadInt16(inFile, "RedZoneTDs");
			possessionInfo.RedZoneFieldGoals = BinaryHelper.ReadInt16(inFile, "RedZoneFGs");

			BinaryHelper.TracerOutdent();
		}

		private void LoadBoxScoreData(System.IO.BinaryReader inFile, GameLog newLog)
		{
			BinaryHelper.TracerIndent();

			newLog.PlayerOfTheGameID = BinaryHelper.ReadInt32(inFile, "PlayerOfTheGameID");
			if (newLog.Year == mCurrentYear)
			{
				newLog.PlayerOfTheGameID = mPlayerActiveRecords[newLog.PlayerOfTheGameID].PlayerID;
			}
			short homeDriveCount = BinaryHelper.ReadInt16(inFile, "HomeDriveCount");
			short awayDriveCount = BinaryHelper.ReadInt16(inFile, "AwayDriveCount");
			newLog.HomeDrives = new GameDriveInfo[homeDriveCount];
			newLog.AwayDrives = new GameDriveInfo[awayDriveCount];
			short i;
			for (i = 0; i < homeDriveCount; ++i)
			{
				newLog.HomeDrives[i] = new GameDriveInfo();
				LoadGameDriveInfo(inFile, newLog.HomeDrives[i]);
			}
			for (i = 0; i < awayDriveCount; ++i)
			{
				newLog.AwayDrives[i] = new GameDriveInfo();
				LoadGameDriveInfo(inFile, newLog.AwayDrives[i]);
			}
			LoadGamePassingInfo(inFile, ref newLog.HomePassing);
			LoadGamePassingInfo(inFile, ref newLog.AwayPassing);
			LoadGameRushingInfo(inFile, ref newLog.HomeRushing);
			LoadGameRushingInfo(inFile, ref newLog.AwayRushing);
			LoadGamePossessionInfo(inFile, ref newLog.HomePossessions);
			LoadGamePossessionInfo(inFile, ref newLog.AwayPossessions);

			BinaryHelper.TracerOutdent();
		}

		System.IO.StreamWriter gInfoPlayDumpFile = null;
		System.IO.StreamWriter gFGPlayDumpFile = null;
		System.IO.StreamWriter gPuntPlayDumpFile = null;
		System.IO.StreamWriter gOnsideKickPlayDumpFile = null;
		System.IO.StreamWriter gPassPlayDumpFile = null;
		System.IO.StreamWriter gRunPlayDumpFile = null;
		GameLog gCurGameLog = null;

		private GameLog LoadGameLog(System.IO.BinaryReader inFile)
		{
			GameLog newLog = new GameLog();
			gCurGameLog = newLog;
			newLog.Plays = new List<GamePlay>();

			BinaryHelper.TracerIndent();

			newLog.Year = BinaryHelper.ReadInt16(inFile, "Year");
			newLog.Week = BinaryHelper.ReadInt16(inFile, "Week");
			short stringLength = BinaryHelper.ReadInt16(inFile, "Location Length");
			newLog.Location = BinaryHelper.ExtractString(inFile, stringLength, "Location");
			stringLength = BinaryHelper.ReadInt16(inFile, "Description Length");
			newLog.Description = BinaryHelper.ExtractString(inFile, stringLength, "Description");
			newLog.TotalAttendance = BinaryHelper.ReadInt16(inFile, "Total Attendance") * 100;
			BinaryHelper.ProbeBytes(inFile, 2);
			newLog.UpperDeckAttendance = BinaryHelper.ReadInt16(inFile, "Upper Deck Attendance") * 100;
			newLog.UpperDeckCapacity = BinaryHelper.ReadInt16(inFile, "Upper Deck Capacity") * 100;
			newLog.EndZoneAttendance = BinaryHelper.ReadInt16(inFile, "End Zone Attendance") * 100;
			newLog.EndZoneCapacity = BinaryHelper.ReadInt16(inFile, "End Zone Capacity") * 100;
			newLog.MezzanineAttendance = BinaryHelper.ReadInt16(inFile, "Mezzanine Attendance") * 100;
			newLog.MezzanineCapacity = BinaryHelper.ReadInt16(inFile, "Mezzanine Capacity") * 100;
			newLog.SidelineAttendance = BinaryHelper.ReadInt16(inFile, "Sideline Attendance") * 100;
			newLog.SidelineCapacity = BinaryHelper.ReadInt16(inFile, "Sideline Capacity") * 100;
			newLog.ClubAttendance = BinaryHelper.ReadInt16(inFile, "Club Attendance") * 100;
			newLog.ClubCapacity = BinaryHelper.ReadInt16(inFile, "Club Capacity") * 100;
			newLog.BoxAttendance = BinaryHelper.ReadInt16(inFile, "Box Attendance") * 100;
			newLog.BoxCapacity = BinaryHelper.ReadInt16(inFile, "Box Capacity") * 100;
			BinaryHelper.ProbeBytes(inFile, 4);
			newLog.Temperature = BinaryHelper.ReadInt16(inFile, "Temperature");
			newLog.Weather = BinaryHelper.ReadInt16(inFile, "Weather");
			newLog.TotalCapacity = BinaryHelper.ReadInt16(inFile, "Total Capacity") * 100;
			newLog.WindStrength = BinaryHelper.ReadInt16(inFile, "Wind Strength");
			BinaryHelper.TracerWriteLine("Home Team");
			BinaryHelper.TracerIndent();
			newLog.HomeTeam = LoadGameTeamEntry(inFile, newLog.Week);
			BinaryHelper.TracerOutdent();
			BinaryHelper.TracerWriteLine("Away Team");
			BinaryHelper.TracerIndent();
			newLog.AwayTeam = LoadGameTeamEntry(inFile, newLog.Week);
			BinaryHelper.TracerOutdent();

			BinaryHelper.ProbeBytes(inFile, 2);

			CurPlayID = 0;
			while (true)
			{
				string nextHeader = BinaryHelper.ExtractString(inFile, 4, "Header");
				if (nextHeader == "PD06")
				{
					GamePlay playData = LoadPlayData(inFile);
					if (newLog.Week > 5)
					{
						ExtractStatsFromPlay(newLog, playData);
					}
					newLog.Plays.Add(playData);
					++CurPlayID;
				}
				else if (nextHeader == "ND06")
				{
					LoadBoxScoreData(inFile, newLog);
					break;
				}
				else
				{
					break;
				}
			}

			BinaryHelper.TracerOutdent();

			return newLog;
		}

		private GameWeekRecord LoadGameWeek(short week, short year, string logPath)
		{
			if (FileReadCallback != null)
			{
				FileReadCallback(System.IO.Path.GetFileName(logPath));
			}

			GameWeekRecord newRec = new GameWeekRecord();
			newRec.Week = week;
			newRec.Year = year;
			newRec.FullPath = logPath;
			newRec.GameLogs = new List<GameLog>();

			System.IO.FileStream inStream = new System.IO.FileStream(logPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

			//BinaryHelper.SetupTracer(logPath);

			while (inFile.PeekChar() >= 0)
			{
				string header = BinaryHelper.ExtractString(inFile, 4, "Header");
				if (header == "WD06")
				{
					GameLog newLog = LoadGameLog(inFile);
					newRec.GameLogs.Add(newLog);
				}
			}

			BinaryHelper.ClearTracer();
			
			inFile.Close();

			return newRec;
		}

		private int CurPlayID = 0;
		private void LoadGameList(string pathPrefix)
		{
			string gamePath = Path.GetDirectoryName(pathPrefix);
			string gameID = Path.GetFileNameWithoutExtension(pathPrefix);

			if (DumpInfoPlays)
			{
				gInfoPlayDumpFile = new System.IO.StreamWriter(System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "InfoPlays.csv"));
				gInfoPlayDumpFile.Write("FOFText,GameID,PlayID,Awy,Hom,Qtr,Min,Sec,Pos");
				gInfoPlayDumpFile.Write(",Dat02");
				gInfoPlayDumpFile.Write(",Dat03");
				gInfoPlayDumpFile.Write(",Kick");
				gInfoPlayDumpFile.Write(",Good");
				gInfoPlayDumpFile.Write(",Blkd");
				gInfoPlayDumpFile.Write(",Run2");
				gInfoPlayDumpFile.Write(",Good");
				gInfoPlayDumpFile.Write(",Pas2");
				gInfoPlayDumpFile.Write(",Good");
				gInfoPlayDumpFile.Write(",Dat11");
				gInfoPlayDumpFile.Write(",QBKp");
				gInfoPlayDumpFile.Write(",BdSnpTyp");
				gInfoPlayDumpFile.Write(",Type");
				gInfoPlayDumpFile.Write(",Dat15");
				gInfoPlayDumpFile.Write(",Dat16");
				gInfoPlayDumpFile.Write(",Plr1");
				gInfoPlayDumpFile.Write(",Plr2");
				gInfoPlayDumpFile.Write(",Off0,Off1,Off2,Off3,Off4,Off5,Off6,Off7,Off8,Off9,Off10,Def0,Def1,Def2,Def3,Def4,Def5,Def6,Def7,Def8,Def9,Def10");
				gInfoPlayDumpFile.WriteLine();
			}

			if (DumpFieldGoalPlays)
			{
				gFGPlayDumpFile = new System.IO.StreamWriter(System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "FGPlays.csv"));
				gFGPlayDumpFile.Write("FOFText,GameID,PlayID,Awy,Hom,Qtr,Min,Sec,Pos");
				for (int i = 0; i < 63; ++i)
				{
					gFGPlayDumpFile.Write("," + i.ToString("D2"));
				}
				gFGPlayDumpFile.Write(",Off0,Off1,Off2,Off3,Off4,Off5,Off6,Off7,Off8,Off9,Off10,Def0,Def1,Def2,Def3,Def4,Def5,Def6,Def7,Def8,Def9,Def10");
				gFGPlayDumpFile.WriteLine();
			}

			if (DumpPuntPlays)
			{
				gPuntPlayDumpFile = new System.IO.StreamWriter(System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PuntPlays.csv"));
				gPuntPlayDumpFile.Write("FOFText,GameID,PlayID,Awy,Hom,Qtr,Min,Sec,Pos");
				for (int i = 0; i < 63; ++i)
				{
					gPuntPlayDumpFile.Write("," + i.ToString("D2"));
				}
				gPuntPlayDumpFile.Write(",Off0,Off1,Off2,Off3,Off4,Off5,Off6,Off7,Off8,Off9,Off10,Def0,Def1,Def2,Def3,Def4,Def5,Def6,Def7,Def8,Def9,Def10");
				gPuntPlayDumpFile.WriteLine();
			}

			if (DumpOnsideKickPlays)
			{
				gOnsideKickPlayDumpFile = new System.IO.StreamWriter(System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "OnsidePlays.csv"));
				gOnsideKickPlayDumpFile.Write("FOFText,GameID,PlayID,Awy,Hom,Qtr,Min,Sec,Pos");
				for (int i = 0; i < 63; ++i)
				{
					gOnsideKickPlayDumpFile.Write("," + i.ToString("D2"));
				}
				gOnsideKickPlayDumpFile.Write(",Off0,Off1,Off2,Off3,Off4,Off5,Off6,Off7,Off8,Off9,Off10,Def0,Def1,Def2,Def3,Def4,Def5,Def6,Def7,Def8,Def9,Def10");
				gOnsideKickPlayDumpFile.WriteLine();
			}

			if (DumpPassPlays)
			{
				gPassPlayDumpFile = new System.IO.StreamWriter(System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "PassPlays.csv"));
				gPassPlayDumpFile.Write("FOFText,GameID,PlayID,Awy,Hom,Qtr,Min,Sec,Pos");
				gPassPlayDumpFile.Write(",dat02");
				gPassPlayDumpFile.Write(",dat03");
				gPassPlayDumpFile.Write(",IsComplete");
				gPassPlayDumpFile.Write(",YardsGained");
				gPassPlayDumpFile.Write(",IsTouchdown");
				gPassPlayDumpFile.Write(",PassTarget");
				gPassPlayDumpFile.Write(",Tackler");
				gPassPlayDumpFile.Write(",IsFumble");
				gPassPlayDumpFile.Write(",FumbleRecoveryTeam");
				gPassPlayDumpFile.Write(",FumbleReturnYards");
				gPassPlayDumpFile.Write(",FumbleRecoveryTackler");
				gPassPlayDumpFile.Write(",IsFumbleRecoveredForTD");
				gPassPlayDumpFile.Write(",FumbleRecoverer");
				gPassPlayDumpFile.Write(",IsInterception");
				gPassPlayDumpFile.Write(",InterceptingPlayer");
				gPassPlayDumpFile.Write(",InterceptionReturnYards");
				gPassPlayDumpFile.Write(",IsInterceptedForTD");
				gPassPlayDumpFile.Write(",InterceptedTackler");
				gPassPlayDumpFile.Write(",InterceptionYardLine");
				gPassPlayDumpFile.Write(",IsPenaltyAccepted");
				gPassPlayDumpFile.Write(",IsDefensiveTD");
				gPassPlayDumpFile.Write(",dat23");
				gPassPlayDumpFile.Write(",dat24");
				gPassPlayDumpFile.Write(",IsQBScramble");
				gPassPlayDumpFile.Write(",QBScrambleYards");
				gPassPlayDumpFile.Write(",IsQBSacked");
				gPassPlayDumpFile.Write(",QBSackYards");
				gPassPlayDumpFile.Write(",IsQBSackedForSafety");
				gPassPlayDumpFile.Write(",SackingPlayer");
				gPassPlayDumpFile.Write(",IsForcedOOB");
				gPassPlayDumpFile.Write(",InterceptedInEndZone");
				gPassPlayDumpFile.Write(",IsHalfSack");
				gPassPlayDumpFile.Write(",AssistantSacker");
				gPassPlayDumpFile.Write(",AssistantTackler");
				gPassPlayDumpFile.Write(",IsAssistedTackle");
				gPassPlayDumpFile.Write(",WhoAllowedQBSack");
				gPassPlayDumpFile.Write(",IncompletionType");
				gPassPlayDumpFile.Write(",IsOverMiddleOfField");
				gPassPlayDumpFile.Write(",YardsAfterCatch");
				gPassPlayDumpFile.Write(",GameLogMessage1Type");
				gPassPlayDumpFile.Write(",GameLogMessage2Type");
				gPassPlayDumpFile.Write(",GameLogMessage3Type");
				gPassPlayDumpFile.Write(",DoubleCoverage");
				gPassPlayDumpFile.Write(",PrePlayDown");
				gPassPlayDumpFile.Write(",PrePlayYardsToGo");
				gPassPlayDumpFile.Write(",PrePlayYardLine");
				gPassPlayDumpFile.Write(",PrePlayTeamPossession");
				gPassPlayDumpFile.Write(",PostPlayDown");
				gPassPlayDumpFile.Write(",PostPlayYardsToGo");
				gPassPlayDumpFile.Write(",PostPlayYardLine");
				gPassPlayDumpFile.Write(",IsTurnover");
				gPassPlayDumpFile.Write(",dat53");
				gPassPlayDumpFile.Write(",IsTurnoverOnDowns");
				gPassPlayDumpFile.Write(",dat55");
				gPassPlayDumpFile.Write(",dat56");
				gPassPlayDumpFile.Write(",dat57");
				gPassPlayDumpFile.Write(",PassDistance");
				gPassPlayDumpFile.Write(",DefenseFamiliar");
				gPassPlayDumpFile.Write(",EvadedRushToAvoidSafety");
				gPassPlayDumpFile.Write(",FieldCondition");
				gPassPlayDumpFile.Write(",GameLogMessage4Type");
				gPassPlayDumpFile.Write(",Off0,Off1,Off2,Off3,Off4,Off5,Off6,Off7,Off8,Off9,Off10,Def0,Def1,Def2,Def3,Def4,Def5,Def6,Def7,Def8,Def9,Def10");
				gPassPlayDumpFile.WriteLine();
			}

			if (DumpRunPlays)
			{
				gRunPlayDumpFile = new System.IO.StreamWriter(System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "RunPlays.csv"));
				gRunPlayDumpFile.Write("FOFText,GameID,PlayID,Awy,Hom,Qtr,Min,Sec,Pos");
				gRunPlayDumpFile.Write(",dat02");
				gRunPlayDumpFile.Write(",dat03");
				gRunPlayDumpFile.Write(",YardsGained],IsTouchdown,Rusher,IsFumble,Tackler,FumbleRecoveryTeam,FumbleRecoveryYards,FumbleRecoveredForTD,FumbleRecoveryTackler,IsPenaltyAccepted,FumbleRecoverer,TrueRushYardLine");
				gRunPlayDumpFile.Write(",dat16");
				gRunPlayDumpFile.Write(",IsForcedOOB,IsSafety,AssistantTackler,WasTackleAssisted,KeyRunBlocker,KeyRunBlockOpportunity");
				gRunPlayDumpFile.Write(",dat23");
				gRunPlayDumpFile.Write(",GameLogMessage1Type,GameLogMessage1Player,GameLogMessage2Type,GameLogMessage2Player,GameLogMessage3Type,GameLogMessage3Player,PrePlayDown,PrePlayYardsToGo,PrePlayYardLine,PrePlayTeamPossession,PostPlayDown,PostPlayYardsToGo,PostPlayYardLine,IsTurnover");
				gRunPlayDumpFile.Write(",dat38,TurnoverOnDowns,dat40,dat41");
				gRunPlayDumpFile.Write(",RunDirection,IsFinesseRun,FieldCondition,DefenseFamiliar,GameLogMessage4Type");
				gRunPlayDumpFile.Write(",dat47,dat48,dat49,dat50,dat51,dat52,dat53,dat54,dat55,dat56,dat57,dat58,dat59,dat60,dat61,dat62");
				gRunPlayDumpFile.Write(",Off0,Off1,Off2,Off3,Off4,Off5,Off6,Off7,Off8,Off9,Off10,Def0,Def1,Def2,Def3,Def4,Def5,Def6,Def7,Def8,Def9,Def10");
				gRunPlayDumpFile.WriteLine();
			}

			mAvailableGameWeeks = new List<GameWeekRecord>();
			for (short year = (short)mStartingSeason; year <= mCurrentYear; ++year)
			{
				mGameListCurrentSeason = year;
				for (short week = 1; week <= 26; ++week)
				{
					string logPath = Path.Combine(gamePath, gameID + week.ToString() + "." + year.ToString());
					if (File.Exists(logPath))
					{
						GameWeekRecord newRec = LoadGameWeek(week, year, logPath);
						mAvailableGameWeeks.Add(newRec);
					}
				}
			}

			if (gOnsideKickPlayDumpFile != null)
			{
				gOnsideKickPlayDumpFile.Close();
			}
			if (gFGPlayDumpFile != null)
			{
				gFGPlayDumpFile.Close();
			}
			if (gPuntPlayDumpFile != null)
			{
				gPuntPlayDumpFile.Close();
			}
			if (gInfoPlayDumpFile != null)
			{
				gInfoPlayDumpFile.Close();
			}
			if (gPassPlayDumpFile != null)
			{
				gPassPlayDumpFile.Close();
			}
			if (gRunPlayDumpFile != null)
			{
				gRunPlayDumpFile.Close();
			}
		}

		PlayerGameStatsRecord GetOffensivePlayerStatsFromPlay(GameLog gameLog, GamePlay playData, short playPlayerIndex)
		{
			int teamPlayerIndex = playData.OffensivePlayers[playPlayerIndex];
			if (teamPlayerIndex >= 0)
			{
				if (playData.Possession == 0)
				{
					return gameLog.HomeTeam.PlayerStats[teamPlayerIndex];
				}
				else
				{
					return gameLog.AwayTeam.PlayerStats[teamPlayerIndex];
				}
			}
			else
			{
				return null;
			}
		}

		PlayerGameStatsRecord GetDefensivePlayerStatsFromPlay(GameLog gameLog, GamePlay playData, short playPlayerIndex)
		{
			int teamPlayerIndex = playData.DefensivePlayers[playPlayerIndex];
			if (teamPlayerIndex >= 0)
			{
				if (playData.Possession == 0)
				{
					return gameLog.AwayTeam.PlayerStats[teamPlayerIndex];
				}
				else
				{
					return gameLog.HomeTeam.PlayerStats[teamPlayerIndex];
				}
			}
			else
			{
				return null;
			}
		}

		PlayerHistoricalRecord GetOffensivePlayerHistoricalFromPlay(GameLog gameLog, GamePlay playData, short playPlayerIndex)
		{
			int teamPlayerIndex = playData.OffensivePlayers[playPlayerIndex];
			if (teamPlayerIndex >= 0)
			{
				if (playData.Possession == 0)
				{
					return gameLog.HomeTeam.PlayerHistorical[teamPlayerIndex];
				}
				else
				{
					return gameLog.AwayTeam.PlayerHistorical[teamPlayerIndex];
				}
			}
			else
			{
				return null;
			}
		}

		PlayerHistoricalRecord GetDefensivePlayerHistoricalFromPlay(GameLog gameLog, GamePlay playData, short playPlayerIndex)
		{
			int teamPlayerIndex = playData.DefensivePlayers[playPlayerIndex];
			if (teamPlayerIndex >= 0)
			{
				if (playData.Possession == 0)
				{
					return gameLog.AwayTeam.PlayerHistorical[teamPlayerIndex];
				}
				else
				{
					return gameLog.HomeTeam.PlayerHistorical[teamPlayerIndex];
				}
			}
			else
			{
				return null;
			}
		}

		bool IsPlaySuccessful(short yardsGained, short down, short distance)
		{
			short yardsNeeded = distance;
			if (down == 1)
			{
				yardsNeeded = (short)((distance * 4) / 10);
			}
			else if (down == 2)
			{
				yardsNeeded = (short)((distance * 6) / 10);
			}

			return (yardsGained >= yardsNeeded);
		}

		void ExtractPassStatsFromPlay(GameLog gameLog, GamePlay playData)
		{
			if (playData.TypeSpecificData[(int)PassPlayFields.IsPenaltyAccepted] !=0 && playData.EffectOnPlay != 2)
			{
				return;
			}

			short doubleCoverage = playData.TypeSpecificData[(int)PassPlayFields.DoubleCoverage];
			bool badPass = playData.TypeSpecificData[(int)PassPlayFields.IncompletionType] == 1 && playData.TypeSpecificData[(int)PassPlayFields.EvadedRushToAvoidSafety] == 0;
			bool isComplete = playData.TypeSpecificData[(int)PassPlayFields.IsComplete] != 0;
			bool lostFumble = playData.TypeSpecificData[(int)PassPlayFields.IsFumble] != 0
					&& playData.TypeSpecificData[(int)PassPlayFields.FumbleRecoveryTeam] != playData.Possession;
			bool isIntercepted = playData.TypeSpecificData[(int)PassPlayFields.IsInterception] != 0;
			bool isTurnover = lostFumble || isIntercepted;
			bool isSpike = playData.TypeSpecificData[(int)PassPlayFields.PassDistance] == (short)PassDistance.Spike;
			bool isScramble = playData.TypeSpecificData[(int)PassPlayFields.IsQBScramble] != 0;
			bool throwOccurred = playData.TypeSpecificData[(int)PassPlayFields.IsQBSacked] == 0 && !isScramble;
			short playYardage = playData.TypeSpecificData[(int)PassPlayFields.YardsGained];
			bool isPlaySuccessful = IsPlaySuccessful(playYardage, playData.Down, playData.YardsToGo) && !isTurnover && isComplete;

			for (short i = 0; i < playData.OffensivePlayers.Length; ++i)
			{
				PlayerGameStatsRecord offenseRecord = GetOffensivePlayerStatsFromPlay(gameLog, playData, i);
				if (offenseRecord != null)
				{
					if (isScramble)
					{
						offenseRecord.OffensiveRunPlays += 1;
					}
					else
					{
						offenseRecord.OffensivePassPlays += 1;
					}
				}
			}

			for (short i = 0; i < playData.DefensivePlayers.Length; ++i)
			{
				PlayerGameStatsRecord defenseRecord = GetDefensivePlayerStatsFromPlay(gameLog, playData, i);
				if (defenseRecord != null)
				{
					if (isScramble)
					{
						defenseRecord.DefensiveRunPlays += 1;
					}
					else
					{
						defenseRecord.DefensivePassPlays += 1;
					}
				}
			}

			PlayerGameStatsRecord coverageRecord = GetDefensivePlayerStatsFromPlay(gameLog, playData, playData.TypeSpecificData[(int)PassPlayFields.InterceptingPlayer]);
			if (coverageRecord != null && !isSpike)
			{
				coverageRecord.HasKeyCoverage += 1;
				if (throwOccurred)
				{
					coverageRecord.ThrownAt += 1;
					if (playYardage >= 20 && playData.TypeSpecificData[(int)PassPlayFields.IsComplete] != 0)
					{
						coverageRecord.ReceptionsOf20YardsPlusGivenUp += 1;
					}
				}
			}

			PlayerGameStatsRecord qbRecord = GetOffensivePlayerStatsFromPlay(gameLog, playData, 0);
			PlayerGameStatsRecord receiverRecord = GetOffensivePlayerStatsFromPlay(gameLog, playData, playData.TypeSpecificData[(int)PassPlayFields.PassTarget]);
			if (qbRecord != null)
			{
				if (doubleCoverage == 3)
				{
					qbRecord.DoubleCoveragesAvoided += 1;
				}
				else if (doubleCoverage == 1 || doubleCoverage == 2)
				{
					qbRecord.DoubleCoveragesThrownInto += 1;
				}

				if (badPass)
				{
					qbRecord.BadPasses += 1;
					if (isComplete && receiverRecord != null)
					{
						receiverRecord.BadPassesCaught += 1;
					}
				}

				if (isPlaySuccessful)
				{
					if (throwOccurred)
					{
						qbRecord.SuccessfulPasses += 1;
						if (receiverRecord != null && isComplete)
						{
							receiverRecord.SuccessfulCatches += 1;
						}
					}
					else if (isScramble)
					{
						qbRecord.SuccessfulRuns += 1;
					}
				}
			}

			if (playYardage < 0)
			{
				if (   playData.TypeSpecificData[(int)PassPlayFields.IsTouchdown] == 0 
					&& playData.TypeSpecificData[(int)PassPlayFields.IsForcedOOB] == 0
					&& playData.TypeSpecificData[(int)PassPlayFields.IsQBSacked] == 0
					)
				{
					PlayerGameStatsRecord tacklerRecord = GetDefensivePlayerStatsFromPlay(gameLog, playData, playData.TypeSpecificData[(int)PassPlayFields.Tackler]);
					if (tacklerRecord != null)
					{
						tacklerRecord.TacklesForLoss += 1;
					}
					if (playData.TypeSpecificData[(int)PassPlayFields.IsAssistedTackle] != 0)
					{
						PlayerGameStatsRecord asstTacklerRecord = GetDefensivePlayerStatsFromPlay(gameLog, playData, playData.TypeSpecificData[(int)PassPlayFields.AssistantTackler]);
						if (asstTacklerRecord != null)
						{
							asstTacklerRecord.AssistedTacklesForLoss += 1;
						}
					}
				}
			}
		}

		void ExtractRunStatsFromPlay(GameLog gameLog, GamePlay playData)
		{
			if (playData.TypeSpecificData[(int)RunPlayFields.IsPenaltyAccepted] != 0 && playData.EffectOnPlay != 2)
			{
				return;
			}

			// Ignore kneeldowns
			if (playData.TypeSpecificData[(int)RunPlayFields.RunDirection] == (short)RunDirection.KneelDown)
			{
				return;
			}

			for (short i = 0; i < playData.OffensivePlayers.Length; ++i)
			{
				PlayerGameStatsRecord offenseRecord = GetOffensivePlayerStatsFromPlay(gameLog, playData, i);
				if (offenseRecord != null)
				{
					offenseRecord.OffensiveRunPlays += 1;
				}
			}

			for (short i = 0; i < playData.DefensivePlayers.Length; ++i)
			{
				PlayerGameStatsRecord defenseRecord = GetDefensivePlayerStatsFromPlay(gameLog, playData, i);
				if (defenseRecord != null)
				{
					defenseRecord.DefensiveRunPlays += 1;
				}
			}

			short yardsGained = playData.TypeSpecificData[(int)RunPlayFields.YardsGained];
			bool lostFumble = playData.TypeSpecificData[(int)RunPlayFields.IsFumble] != 0
					&& playData.TypeSpecificData[(int)RunPlayFields.FumbleRecoveryTeam] != playData.Possession;
			bool isPlaySuccessful = IsPlaySuccessful(yardsGained, playData.Down, playData.YardsToGo) && !lostFumble;

			PlayerGameStatsRecord rushRecord = GetOffensivePlayerStatsFromPlay(gameLog, playData, playData.TypeSpecificData[(int)RunPlayFields.Rusher]);
			if (rushRecord != null)
			{
				if (yardsGained >= 20)
				{
					rushRecord.RunsOf20YardsPlus += 1;
				}
				else if (yardsGained < 0)
				{
					rushRecord.RunsForLoss += 1;
				}

				if (lostFumble)
				{
					rushRecord.FumblesLost += 1;
				}

				if (isPlaySuccessful)
				{
					rushRecord.SuccessfulRuns += 1;
				}
			}

			if (yardsGained < 0)
			{
				if (playData.TypeSpecificData[(int)RunPlayFields.IsTouchdown] == 0 && playData.TypeSpecificData[(int)RunPlayFields.IsForcedOOB] == 0)
				{
					PlayerGameStatsRecord tacklerRecord = GetDefensivePlayerStatsFromPlay(gameLog, playData, playData.TypeSpecificData[(int)RunPlayFields.Tackler]);
					if (tacklerRecord != null)
					{
						tacklerRecord.TacklesForLoss += 1;
					}
				}
			}
		}

		void ExtractKickoffStatsFromPlay(GameLog gameLog, GamePlay playData)
		{
			if (playData.TypeSpecificData[(int)KickoffPlayFields.PenaltyAccepted] != 0 && playData.EffectOnPlay != 2)
			{
				return;
			}

			PlayerGameStatsRecord kickerRecord = GetOffensivePlayerStatsFromPlay(gameLog, playData, 0);
			if (kickerRecord != null)
			{
				bool isTouchback = playData.TypeSpecificData[(int)KickoffPlayFields.Touchback] != 0;
				if (isTouchback)
				{
					kickerRecord.KickoffTouchbacks += 1;
				}
				kickerRecord.KickoffYards += playData.TypeSpecificData[(int)KickoffPlayFields.KickoffDistance];

				int finalFieldPosition = playData.TypeSpecificData[(int)KickoffPlayFields.ReturnYardLine] % 100;
				if (playData.TypeSpecificData[(int)KickoffPlayFields.KickingTeam] == 1)
				{
					finalFieldPosition = 100 - finalFieldPosition;
				}
				System.Diagnostics.Debug.Assert(finalFieldPosition >= 0);

				kickerRecord.TotalFieldPositionAfterKickoff += finalFieldPosition;

				kickerRecord.Kickoffs += 1;
			}
		}

		void ExtractStatsFromPlay(GameLog gameLog, GamePlay playData)
		{
			if (playData.PlayType == (short)PlayType.Pass)
			{
				ExtractPassStatsFromPlay(gameLog,playData);
			}
			else if (playData.PlayType == (short)PlayType.Run)
			{
				ExtractRunStatsFromPlay(gameLog, playData);
			}
			else if (playData.PlayType == (short)PlayType.Kickoff)
			{
				ExtractKickoffStatsFromPlay(gameLog, playData);
			}
		}

		PlayerGameStatsRecord FindPlayerGameStats(int activePlayerID, int week)
		{
			int seasonIndex = mGameListCurrentSeason - mStartingYear;
			foreach (PlayerGameStatsRecord curRecord in mPlayerGameStatsRecords[seasonIndex])
			{
				if (curRecord.Week == week && curRecord.PlayerID == activePlayerID)
				{
					System.Diagnostics.Debug.Assert(curRecord.Year == mGameListCurrentSeason);
					return curRecord;
				}
			}

			return null;
		}

		PlayerHistoricalRecord FindPlayerHistoricalRecord(int playerID)
		{
			foreach (PlayerHistoricalRecord curRecord in mPlayerHistoricalRecords)
			{
				if (curRecord.PlayerID == playerID)
				{
					return curRecord;
				}
			}

			return null;
		}

		private const int kTeamCount = 32;
		private const int kSeasonGameCount = 350;
		private const int kSeasonWeekCount = 26;	// 5 preseason + 17 season + 4 playoffs

		private UniverseData mUniverseData;
		private int mStartingSeason;
		private short mGameListCurrentSeason;

		private short mStartingYear;
		private short mCurrentYear;
		private short mGameStage;
		private short mCurrentWeek;
		private short mFAStage;
		private short mPlayersTeam;
		private short mNumberOfTeams;
		private short mSeasonsPlayed;
		private int mSalaryCap;
		private int mMinSalary;
		private int mNextPlayerID;

		public short StartingYear { get { return mStartingYear; } }
		public short CurrentYear { get { return mCurrentYear; } }
		public int SeasonsPlayed { get { return mSeasonsPlayed; } }
		public short GameStage { get { return mGameStage; } }
		public short CurrentWeek { get { return mCurrentWeek; } }
		public short FAStage { get { return mFAStage; } }
		public short PlayersTeam { get { return mPlayersTeam; } }
		public short NumberOfTeams { get { return mNumberOfTeams; } }
		public int SalaryCap { get { return mSalaryCap; } }
		public int MinSalary { get { return mMinSalary; } }
		public int NextPlayerID { get { return mNextPlayerID; } }

		public class TeamRecord
		{
			public bool HumanControlled;
			public int CapLossThisYear;
			public int CapLossNextYear;
		}
		private TeamRecord[] mTeamRecords = new TeamRecord[kTeamCount];
		public TeamRecord[] TeamRecords { get { return mTeamRecords; } }

		public class DraftRound
		{
			public short[] PickTeam;
		}
		public class DraftYear
		{
			public DraftRound[] DraftRounds;
		}
		private DraftYear[] mDraftYears = new DraftYear[4];
		public DraftYear[] DraftYears { get { return mDraftYears; } }

		public class TransactionRecord
		{
			public int PlayerRec2Index;
			public int Salary;
			public short TransactionType;
			public short Team1Index;
			public short Team2Index;
			public short Position;
			public short Years;
			public short Stage;
		}
		private const int kTransactionRecordCount = 10000;
		private TransactionRecord[][] mTransactions;
		public TransactionRecord[][] Transactions { get { return mTransactions; } }

		public class EmailRecord
		{
			public short Flag;
			public string From;
			public string Subject;
			public string Message;
		}
		private const int kEmailRecordCount = 60;
		private EmailRecord[] mEmails = new EmailRecord[kEmailRecordCount];
		public EmailRecord[] Emails { get { return mEmails; } }

		private int mFreeAgentRecordCount;
		private int mActivePlayerCount;
		public const int MaxPlayerHistoricalYearCount = 20;
		public class PlayerHistoricalRecord
		{
			public int PlayerID;
			public short Position;
			public string LastName;
			public string FirstName;
			public string NickName;
			public short Experience;
			public short Height;
			public short Weight;
			public short InHallOfFame;
			public short HallOfFameYear;
			public short HallOfFameVote;
			public short BirthYear;
			public short BirthMonth;
			public short BirthDay;
			public short College;
			public short DraftRound;
			public short DraftPick;
			public short HomeTown;
			public short DraftedBy;
			public short YearDrafted;
			public short PlayerOfTheGame;
			public short ChampionshipRings;
			public short PlayerOfTheWeekMentions;
			public short PlayerOfTheWeekWins;
			public short FourthQuarterHeroics;
			public short QBWins;
			public short QBLosses;
			public short QBTies;
			public short YearsInLeagueCount;
			public short[] YearsInLeague;
			public int[] YearDataIndex;
		}
		private PlayerHistoricalRecord[] mPlayerHistoricalRecords;
		public PlayerHistoricalRecord[] PlayerHistoricalRecords { get { return mPlayerHistoricalRecords; } }

		public const int MaxContractYears = 7;
		public class PlayerActiveRecord
		{
			public int PlayerID;
			public short Position;
			public short Experience;
			public short Number;
			public short PositionGroup;
			public short Team;
			public short InjuryLength;
			public short Loyalty;
			public short PlaysToWin;
			public short Personality;
			public short Leadership;
			public short Intelligence;
			public short RedFlagMarker;
			public short Volatility;
			public short JoinedTeam;
			public short UFAYear;
			public short Popularity;
			public short ContractLength;
			public int[] Salary;
			public int[] Bonus;
			public short[] InterviewMarkers;
		}
		private PlayerActiveRecord[] mPlayerActiveRecords;
		public PlayerActiveRecord[] PlayerActiveRecords { get { return mPlayerActiveRecords; } }

		public class SeasonRecord
		{
			public short PlayerEval;
			public short PlayerTeam;
			public short Wins;
			public short Losses;
			public short Ties;
			public short Year;
		};
		private SeasonRecord[] mSeasonRecords;
		public SeasonRecord[] SeasonRecords { get { return mSeasonRecords; } }

		public class FranchisePerformanceRecord
		{
			public short Year;
			public short FranchiseValue;
			public short ProfitScore;
			public short PerformanceScore;
			public short RosterScore;
			public short Playoffs;
			public short Wins;
			public short Losses;
			public short Ties;
			public short Unknown;
			public short PointsFor;
			public short PointsAgainst;
			public short ConfWins;
			public short ConfLoss;
			public short ConfTies;
			public short DivWin;
			public short DivLoss;
			public short DivTie;
			public short Attendance;
			public short StadiumCapacity;
			public long TVRevenue;
			public long TicketRevenue;
			public long SuiteRevenue;
			public long PlayerSalaries;
			public long PlayerBonuses;
			public long StadiumPayment;
			public long Concessions;
			public long Parking;
			public long Advertising;
			public long Training;
			public long Coaching;
			public long Scouting;
			public long Maintenance;
		};
		private FranchisePerformanceRecord[] mFranchisePerformanceRecords;
		public FranchisePerformanceRecord[] FranchisePerformanceRecords { get { return mFranchisePerformanceRecords; } }

		public class GameResultRecord
		{
			public short Year;
			public short Week;
			public short AwayScore;
			public short AwayTeam;
			public short HomeScore;
			public short HomeTeam;
			public short Attendance;
			public short Weather;
			public int AwayPassingLeaderPlayerID;
			public int HomePassingLeaderPlayerID;
			public int AwayRushingLeaderPlayerID;
			public int HomeRushingLeaderPlayerID;
			public int AwayReceivingLeaderPlayerID;
			public int HomeReceivingLeaderPlayerID;
			public short AwayPassAttempts;
			public short AwayPassCompletions;
			public short AwayPassYards;
			public short HomePassAttempts;
			public short HomePassCompletions;
			public short HomePassYards;
			public short AwayRushAttempts;
			public short AwayRushYards;
			public short HomeRushAttempts;
			public short HomeRushYards;
			public short AwayReceptions;
			public short AwayReceivingYards;
			public short HomeReceptions;
			public short HomeReceivingYards;
		};
		private GameResultRecord[] mGameResultRecords;
		public GameResultRecord[] GameResultRecords { get { return mGameResultRecords; } }

		public class TeamStadiumBlock
		{
			public short StadiumType;	// 0=Outdoor/Grass,1=Outdoor/Turf,2=Dome/Turf,3=RetRoof/Grass
			public short YearStadiumBuilt;
			public short TotalCapacity;
			public short LuxuryBoxes;
			public short ClubSeats;
			public short Unknown1;
			public short Unknown2;
			public short FanLoyalty;
			public short PublicSupportForStadium;
			public short UpperDeckPrice;
			public short EndZonePrice;
			public short MezzaninePrice;
			public short SidelinesPrice;
			public short ClubSeatsPrice;
			public short LuxuryBoxPrice;
			public short Unknown3;
			public short Unknown4;
			public short ConstructionCompletionYear;
			public short ConstructionType;	// 1 = renovation, 2 = new stadium
			public short Unknown5;
			public short ConstructionCapacity;
			public short ConstructionLuxuryBoxes;
			public short ConstructionClubSeats;
			public short ConstructionStadiumType;
			public short Unknown7;
			public short PriorYearAttendance;
		};
		private TeamStadiumBlock[] mTeamStadiumBlocks;
		public TeamStadiumBlock[] TeamStadiumBlocks { get { return mTeamStadiumBlocks; } }

		public class TeamScheduleGameRecord
		{
			public short TeamIndex;
			public short Week;
			public short Away;
			public short ConferenceGame;
			public short DivisionGame;
			public short Opponent;
			public short Score;
			public short OppScore;
			public short Unknown1;
			public short Attendance;
			public short Unknown2;
			public short Weather;	// ? Temp appears in here, but not the usual encoding
			public short Unknown3;
			public short Unknown4;
		};
		private TeamScheduleGameRecord[] mTeamScheduleGameRecords;
		public TeamScheduleGameRecord[] TeamScheduleGameRecords { get { return mTeamScheduleGameRecords; } }

		public class PlayerGameStatsRecord
		{
			public PlayerGameStatsRecord()
			{
				PlayerID = 0;
				Year = 0;
				Week = 0;
				Team = 0;
				GamePlayed = 0;
				GameStarted = 0;
				PassAttempts = 0;
				PassCompletions = 0;
				PassYards = 0;
				LongestPass = 0;
				TDPasses = 0;
				INTThrown = 0;
				TimesSacked = 0;
				SackedYards = 0;
				RushAttempts = 0;
				RushingYards = 0;
				LongestRun = 0;
				RushTD = 0;
				Catches = 0;
				ReceivingYards = 0;
				LongestReception = 0;
				ReceivingTDs = 0;
				PassTargets = 0;
				YardsAfterCatch = 0;
				PassDrops = 0;
				PuntReturns = 0;
				PuntReturnYards = 0;
				PuntReturnTDs = 0;
				KickReturns = 0;
				KickReturnYards = 0;
				KickReturnTDs = 0;
				Fumbles = 0;
				FumbleRecoveries = 0;
				ForcedFumbles = 0;
				MiscTD = 0;
				KeyRunBlock = 0;
				KeyRunBlockOpportunites = 0;
				SacksAllowed = 0;
				Tackles = 0;
				Assists = 0;
				Sacks = 0;
				INTs = 0;
				INTReturnYards = 0;
				INTReturnTDs = 0;
				PassesDefended = 0;
				PassesBlocked = 0;
				QBHurries = 0;
				PassesCaught = 0;
				PassPlays = 0;
				RunPlays = 0;
				FGMade = 0;
				FGAttempted = 0;
				FGLong = 0;
				PAT = 0;
				PATAttempted = 0;
				Punts = 0;
				PuntYards = 0;
				PuntLong = 0;
				PuntIn20 = 0;
				Points = 0;
				OpposingTeamID = 0;
				ThirdDownRushes = 0;
				ThirdDownRushConversions = 0;
				ThirdDownPassAttempts = 0;
				ThirdDownPassCompletions = 0;
				ThirdDownPassConversions = 0;
				ThirdDownReceivingTargets = 0;
				ThirdDownReceivingCatches = 0;
				ThirdDownReceivingConversions = 0;
				FirstDownRushes = 0;
				FirstDownPasses = 0;
				FirstDownCatches = 0;
				FG40PlusAttempts = 0;
				FG40PlusMade = 0;
				FG50PlusAttempts = 0;
				FG50PlusMade = 0;
				PuntNetYards = 0;
				SpecialTeamsTackles = 0;
				Unknown14 = 0;
				TimesKnockedDown = 0;
				RedZoneRushes = 0;
				RedZoneRushingYards = 0;
				RedZonePassAttempts = 0;
				RedZonePassCompletions = 0;
				RedZonePassingYards = 0;
				RedZoneReceivingTargets = 0;
				RedZoneReceivingCatches = 0;
				RedZoneReceivingYards = 0;
				TotalTDs = 0;
				TwoPointConversions = 0;
				PancakeBlocks = 0;
				QBKnockdowns = 0;
				Unknown23 = 0;
				SpecialTeamsPlays = 0;
				RushingGamesOver100Yards = 0;
				ReceivingGamesOver100Yards = 0;
				PassingGamesOver300Yards = 0;
				RunsOf10YardsPlus = 0;
				CatchesOf20YardsPlus = 0;
				ThrowsOf20YardsPlus = 0;
				AllPurposeYards = 0;
				YardsFromScrimmage = 0;
				DoubleCoveragesThrownInto = 0;
				DoubleCoveragesAvoided = 0;
				BadPasses = 0;
				RunsForLoss = 0;
				RunsOf20YardsPlus = 0;
				FumblesLost = 0;
				HasKeyCoverage = 0;
				ThrownAt = 0;
				TacklesForLoss = 0;
				AssistedTacklesForLoss = 0;
				ReceptionsOf20YardsPlusGivenUp = 0;
				Kickoffs = 0;
				KickoffYards = 0;
				KickoffTouchbacks = 0;
				TotalFieldPositionAfterKickoff = 0;
				OffensivePassPlays = 0;
				OffensiveRunPlays = 0;
				DefensivePassPlays = 0;
				DefensiveRunPlays = 0;
				SuccessfulPasses = 0;
				SuccessfulCatches = 0;
				SuccessfulRuns = 0;
				BadPassesCaught = 0;
			}

			public int PlayerID;
			public short Year;
			public short Week;
			public short Team;
			public short GamePlayed;
			public short GameStarted;
			public int PassAttempts;
			public int PassCompletions;
			public int PassYards;
			public int LongestPass;
			public int TDPasses;
			public int INTThrown;
			public int TimesSacked;
			public int SackedYards;
			public int RushAttempts;
			public int RushingYards;
			public int LongestRun;
			public int RushTD;
			public int Catches;
			public int ReceivingYards;
			public int LongestReception;
			public int ReceivingTDs;
			public int PassTargets;
			public int YardsAfterCatch;
			public int PassDrops;
			public int PuntReturns;
			public int PuntReturnYards;
			public int PuntReturnTDs;
			public int KickReturns;
			public int KickReturnYards;
			public int KickReturnTDs;
			public int Fumbles;
			public int FumbleRecoveries;
			public int ForcedFumbles;
			public int MiscTD;
			public int KeyRunBlock;
			public int KeyRunBlockOpportunites;
			public int SacksAllowed;
			public int Tackles;
			public int Assists;
			public int Sacks;
			public int INTs;
			public int INTReturnYards;
			public int INTReturnTDs;
			public int PassesDefended;
			public int PassesBlocked;
			public int QBHurries;
			public int PassesCaught;
			public int PassPlays;
			public int RunPlays;
			public int FGMade;
			public int FGAttempted;
			public int FGLong;
			public int PAT;
			public int PATAttempted;
			public int Punts;
			public int PuntYards;
			public int PuntLong;
			public int PuntIn20;
			public int Points;
			public int OpposingTeamID;
			public int ThirdDownRushes;
			public int ThirdDownRushConversions;
			public int ThirdDownPassAttempts;
			public int ThirdDownPassCompletions;
			public int ThirdDownPassConversions;
			public int ThirdDownReceivingTargets;
			public int ThirdDownReceivingCatches;
			public int ThirdDownReceivingConversions;
			public int FirstDownRushes;
			public int FirstDownPasses;
			public int FirstDownCatches;
			public int FG40PlusAttempts;
			public int FG40PlusMade;
			public int FG50PlusAttempts;
			public int FG50PlusMade;
			public int PuntNetYards;
			public int SpecialTeamsTackles;
			public int Unknown14;
			public int TimesKnockedDown;
			public int RedZoneRushes;
			public int RedZoneRushingYards;
			public int RedZonePassAttempts;
			public int RedZonePassCompletions;
			public int RedZonePassingYards;
			public int RedZoneReceivingTargets;
			public int RedZoneReceivingCatches;
			public int RedZoneReceivingYards;
			public int TotalTDs;
			public int TwoPointConversions;
			public int PancakeBlocks;
			public int QBKnockdowns;
			public int Unknown23;
			public int SpecialTeamsPlays;
			public int RushingGamesOver100Yards;
			public int ReceivingGamesOver100Yards;
			public int PassingGamesOver300Yards;
			public int RunsOf10YardsPlus;
			public int CatchesOf20YardsPlus;
			public int ThrowsOf20YardsPlus;
			public int AllPurposeYards;
			public int YardsFromScrimmage;
			// New ones from game logs
			public int DoubleCoveragesThrownInto;
			public int DoubleCoveragesAvoided;
			public int BadPasses;
			public int RunsForLoss;
			public int RunsOf20YardsPlus;
			public int FumblesLost;
			public int HasKeyCoverage;
			public int ThrownAt;
			public int TacklesForLoss;
			public int AssistedTacklesForLoss;
			public int ReceptionsOf20YardsPlusGivenUp;
			public int Kickoffs;
			public int KickoffYards;
			public int KickoffTouchbacks;
			public int TotalFieldPositionAfterKickoff;
			public int OffensivePassPlays;
			public int OffensiveRunPlays;
			public int DefensivePassPlays;
			public int DefensiveRunPlays;
			public int SuccessfulPasses;
			public int SuccessfulCatches;
			public int SuccessfulRuns;
			public int BadPassesCaught;
		};
		private const int kPlayerGamesPerSeason = 21;
		public int PlayerGamesPerSeason { get { return kPlayerGamesPerSeason; } }
		private PlayerGameStatsRecord[][] mPlayerGameStatsRecords;
		public PlayerGameStatsRecord[][] PlayerGameStatsRecords { get { return mPlayerGameStatsRecords; } }

		private const int kNumActivePlayers = 46;
		private const int kNumDepthChartEntries = 107;
		public class GameTeamEntry
		{
			public short TeamIndex;
			public string CityName;
			public string NickName;
			public string Abbreviation;
			public int[] ActivePlayerIDs;
			public PlayerGameStatsRecord[] PlayerStats;
			public PlayerHistoricalRecord[] PlayerHistorical;
			public short[] DepthChartEntries;
		}

		public class GameDriveInfo
		{
			public short DriveStartQuarter;
			public short DriveStartMinutes;
			public short DriveStartSeconds;
			public short DriveEndQuarter;
			public short DriveEndMinutes;
			public short DriveEndSeconds;
			public short YardsFromGoalStart;
			public short Plays;
			public short YardsGained;
			public short Result;
		};

		public struct GamePassInfo
		{
			public short ScreenAttempts;
			public short ScreenCompletions;
			public short ScreenYards;
			public short ShortAttempts;
			public short ShortCompletions;
			public short ShortYards;
			public short MediumAttempts;
			public short MediumCompletions;
			public short MediumYards;
			public short LongAttempts;
			public short LongCompletions;
			public short LongYards;
			public short OtherAttempts;
			public short OtherCompletions;
			public short OtherYards;
		};

		public struct GameRushInfo
		{
			public short LeftAttempts;
			public short LeftYards;
			public short MiddleAttempts;
			public short MiddleYards;
			public short RightAttempts;
			public short RightYards;
			public short OtherAttempts;
			public short OtherYards;
		};

		public struct GamePossessionInfo
		{
			public short TimeOfPossession;
			public short RedZoneAttempts;
			public short RedZoneTouchdowns;
			public short RedZoneFieldGoals;
		};

		public enum PassPlayFields
		{
			Minute = 0,
			Seconds,
			IsComplete,
			YardsGained,
			IsTouchdown,
			PassTarget,
			Tackler,
			IsFumble,
			FumbleRecoveryTeam,
			FumbleReturnYards,
			FumbleRecoveryTackler,
			IsFumbleRecoveredForTD,
			FumbleRecoverer,
			IsInterception,
			InterceptingPlayer,
			InterceptionReturnYards,
			IsInterceptedForTD,
			InterceptedTackler,
			InterceptionYardLine,
			IsPenaltyAccepted,
			IsDefensiveTD,
			IsQBScramble,
			QBScrambleYards,
			IsQBSacked,
			QBSackYards,
			IsQBSackedForSafety,
			SackingPlayer,
			IsForcedOOB,
			InterceptedInEndZone,
			IsHalfSack,
			AssistantSacker,
			AssistantTackler,
			IsAssistedTackle,
			WhoAllowedQBSack,
			IncompletionType,
			IsOverMiddleOfField,
			YardsAfterCatch,
			GameLogMessage1Type,
			GameLogMessage2Type,
			GameLogMessage3Type,
			DoubleCoverage,
			PrePlayDown,
			PrePlayYardsToGo,
			PrePlayYardLine,
			PrePlayTeamPossession,
			PostPlayDown,
			PostPlayYardsToGo,
			PostPlayYardLine,
			IsTurnover,
			IsTurnoverOnDowns,
			PassDistance,
			DefenseFamiliar,
			EvadedRushToAvoidSafety,
			FieldCondition,
			GameLogMessage4Type
		};

		public enum RunPlayFields
		{
			Minute = 0,
			Seconds,
			YardsGained,
			IsTouchdown,
			Rusher,
			IsFumble,
			Tackler,
			FumbleRecoveryTeam,
			FumbleRecoveryYards,
			FumbleRecoveredForTD,
			FumbleRecoveryTackler,
			IsPenaltyAccepted,
			FumbleRecoverer,
			TrueRushYardLine,
			IsForcedOOB,
			IsSafety,
			AssistantTackler,
			WasTackleAssisted,
			KeyRunBlocker,
			KeyRunBlockOpportunity,
			GameLogMessage1Type,
			GameLogMessage1Player,
			GameLogMessage2Type,
			GameLogMessage2Player,
			GameLogMessage3Type,
			GameLogMessage3Player,
			PrePlayDown,
			PrePlayYardsToGo,
			PrePlayYardLine,
			PrePlayTeamPossession,
			PostPlayDown,
			PostPlayYardsToGo,
			PostPlayYardLine,
			IsTurnover,
			TurnoverOnDowns,
			RunDirection,
			IsFinesseRun,
			FieldCondition,
			DefenseFamiliar,
			GameLogMessage4Type
		};

		public enum KickoffPlayFields
		{
			Minute = 0,
			Second,
			KickingTeam,
			PenaltyAccepted,
			ReturnYardLine,
			ReturnYards,
			FumbleOnReturn,
			Turnover,
			FumbleTouchdown,
			FumbleReturnYards,
			FumbleRecoverer,
			Tackler,
			Touchdown,
			KickoffYardLine,
			KickoffDistance,
			Touchback,
			Returner,
			FumbleRecoveryYardLine
		};

		public enum PlayType
		{
			FG = 1,
			Kickoff = 2,
			OnsideKick = 3,
			Punt = 4,
			Run = 5,
			Pass = 6,
			Info = 7
		};

		public enum OffensiveFormation
		{
			IFormNormal = 0,
			IFormTEPairs = 1,
			IFormWRToSlot = 2,
			ProNormal = 3,
			ProTEPairs = 4,
			ProWRToSlot = 5,
			WeakNormal = 6,
			WeakTEPairs = 7,
			WeakThreeWR = 8,
			StrongNormal = 9,
			StrongTEPairs = 10,
			StrongThreeWR = 11,
			SingleBackNormal = 12,
			SingleBackTEPairs = 13,
			SingleBackTripsWR = 14,
			SingleBackFourWR = 15,
			FiveWRSpread = 16,
			GoalLineFormation = 17,
			Count
		};

		public enum DefensivePersonnel
		{
			Normal = 0,
			Nickel = 1,
			Dime = 2,
			Prevent = 3,
			GoalLinePersonnel = 4,
			Count
		};

		public enum DefensiveCoverage
		{
			OneDeepLooseMan = 0,
			OneDeepBumpAndRun = 1,
			TwoDeepLooseMan = 2,
			TwoDeepBumpAndRun = 3,
			ThreeDeepZone = 4,
			FourDeepZone = 5,
			StrongSideMan = 6,
			WeakSideMan = 7,
			Count
		};

		public enum RunDirection
		{
			AroundLeftEnd = 0,
			OutsideLT = 1,
			InsideLT = 2,
			InsideLG = 3,
			InsideRG = 4,
			InsideRT = 5,
			OutsideRT = 6,
			AroundRightEnd = 7,
			KneelDown = 8,
			Count
		};

		public enum IncompletionType
		{
			Drop = 0,
			MaybeAvoidingRush = 1,
			PassDefended = 2,
			BlockedAtTheLine = 3,
			Hurried = 4,
			Count
		};

		public enum DoubleCoverageType
		{
			None = 0,
			Primary = 1,
			Secondary = 2,
			ThrewAwayFrom = 3,
			Count
		};

		public enum PassDistance
		{
			Screen = 0,
			P0To4 = 1,
			P5To8 = 2,
			P9To12 = 3,
			P13To18 = 4,
			P19To26 = 5,
			P27To39 = 6,
			P40Plus = 7,
			Spike = 8,
			Count
		};

		public enum DefensiveRunPass
		{
			Run = 0,
			Pass = 1,
			Count
		};

		public enum DefenseFamiliar
		{
			None = 0,
			Very = 1,
			Extremely = 2,
			Count
		}

		public class GamePlay
		{
			// Main Play Data
			public short Quarter;
			public short Minutes;
			public short Seconds;
			public short Possession;
			public short Down;
			public short YardsToGo;
			public short YardLine;
			public short HomeTimeouts;
			public short AwayTimeouts;
			public short PlayType;

			// Formation Data, mostly valid for playtype 5 or 6
			public short OffensiveFormation;
			public short OffensiveStrength;
			public short DefensiveAlignment;
			public short DefensivePersonnel;
			public short DefensiveCoverage;
			public short DefensiveRunPass;
			public short DefensiveRunAggressive;
			public short DefensivePassAggressive;
			public short DefensiveBlitzCount;
			public short[] DefensiveBlitzers = new short[10];

			// Penalty Data
			public short IsDefensivePenalty;
			public short IsOffensivePenalty;
			public short PenaltyYardage;
			public short HappenedOnPuntOrKick;
			public short ResultsInFirstDown;
			public short ResultsInLossOfDown;
			public short InEndZone;
			public short YardLineIfAccepted;
			public short DownIfAccepted;
			public short YardsToGoIfAccepted;
			public short YardLineIfDeclined;
			public short DownIfDeclined;
			public short YardsToGoIfDeclined;
			public short PenaltyType;
			public short IsDefensiveEndOfHalfPenalty;
			public short EffectOnPlay;

			// Injury?
			public short InjuryType;
			public short InjuredPlayer;

			// Play-specific data
			public short[] TypeSpecificData = new short[63];

			// Offensive Lineup
			public short[] OffensivePlayers = new short[11];

			// Defensive Lineup
			public short[] DefensivePlayers = new short[11];
		};

		public class GameLog
		{
			public short Year;
			public short Week;
			public string Location;
			public string Description;
			public int TotalAttendance;
			public int UpperDeckAttendance;
			public int UpperDeckCapacity;
			public int EndZoneAttendance;
			public int EndZoneCapacity;
			public int MezzanineAttendance;
			public int MezzanineCapacity;
			public int SidelineAttendance;
			public int SidelineCapacity;
			public int ClubAttendance;
			public int ClubCapacity;
			public int BoxAttendance;
			public int BoxCapacity;
			public short Temperature;
			public short Weather;
			public int TotalCapacity;
			public short WindStrength;
			public GameTeamEntry HomeTeam;
			public GameTeamEntry AwayTeam;
			public int PlayerOfTheGameID;
			public GameDriveInfo[] HomeDrives;
			public GameDriveInfo[] AwayDrives;
			public GamePassInfo HomePassing;
			public GamePassInfo AwayPassing;
			public GameRushInfo HomeRushing;
			public GameRushInfo AwayRushing;
			public GamePossessionInfo HomePossessions;
			public GamePossessionInfo AwayPossessions;
			public List<GamePlay> Plays;
		}

		public class GameWeekRecord
		{
			public short Week;
			public short Year;
			public string FullPath;
			public List<GameLog> GameLogs;
		}
		private List<GameWeekRecord> mAvailableGameWeeks;
		public List<GameWeekRecord> AvailableGameWeeks { get { return mAvailableGameWeeks; } }
	}
}

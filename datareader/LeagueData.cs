using System;
using System.Collections;
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

        private char[] commaDelim = new char[] { ',' };

        public LeagueData(string pathPrefix,UniverseData universeData, int startingSeason, FileReadDelegate readCallback, bool loadGameList)
        {
            FileReadCallback = readCallback;
            mUniverseData = universeData;
            mStartingSeason = startingSeason;
            mSavedGamePath = pathPrefix;
            mLeagueID = Path.GetFileName(pathPrefix);
            mExportPath = Path.Combine(universeData.ExportDirectory, mLeagueID);
            LoadTeamInformation();
            LoadActivePlayerData();
            if (loadGameList)
            {
                LoadGameList();
            }
        }

        private void LoadActivePlayerData()
        {
            var filePath = System.IO.Path.Combine(mExportPath, "player_record.csv");
            if (FileReadCallback != null)
            {
                FileReadCallback("player_record.csv");
            }
            using (System.IO.StreamReader inFile = new System.IO.StreamReader(filePath))
            {
                var activePlayerIDs = new ArrayList();
                string headerLine = inFile.ReadLine();

                while (!inFile.EndOfStream)
                {
                    string inLine = inFile.ReadLine();
                    string[] tokens = inLine.Split(commaDelim);
                    activePlayerIDs.Add(Int32.Parse(tokens[0]));
                }

                mActivePlayerIDs = new int[activePlayerIDs.Count];
                activePlayerIDs.CopyTo(mActivePlayerIDs);
            }
        }

        private void LoadTeamInformation()
        {
            string teamInformationPath = System.IO.Path.Combine(mExportPath, "team_information.csv");
            // read position and birthdate from player_information.csv
            if (FileReadCallback != null)
            {
                FileReadCallback("team_information.csv");
            }
            using (System.IO.StreamReader teamInformationFile = new System.IO.StreamReader(teamInformationPath))
            {
                System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

                string headerLine = teamInformationFile.ReadLine();
                while (!teamInformationFile.EndOfStream)
                {
                    string teamLine = teamInformationFile.ReadLine();
                    string[] fields = teamLine.Split(commaDelim);
                    int teamID = Int32.Parse(fields[0]);
                    mTeamInformationRecords[teamID] = new TeamInformationRecord();
                    mTeamInformationRecords[teamID].Wins = Int32.Parse(fields[1]);
                    mTeamInformationRecords[teamID].Losses = Int32.Parse(fields[2]);
                    mTeamInformationRecords[teamID].Ties = Int32.Parse(fields[3]);
                    mTeamInformationRecords[teamID].PlayoffAppearances = Int32.Parse(fields[4]);
                    mTeamInformationRecords[teamID].BowlAppearances = Int32.Parse(fields[5]);
                    mTeamInformationRecords[teamID].BowlWins = Int32.Parse(fields[6]);
                    mTeamInformationRecords[teamID].PlayoffWins = Int32.Parse(fields[7]);
                    mTeamInformationRecords[teamID].PlayoffLosses = Int32.Parse(fields[8]);
                    mTeamInformationRecords[teamID].TurfType = fields[9];
                    mTeamInformationRecords[teamID].YearStadiumBuilt = Int32.Parse(fields[10]);
                    mTeamInformationRecords[teamID].StadiumCapacity = Int32.Parse(fields[11]);
                    mTeamInformationRecords[teamID].LuxuryBoxes = Int32.Parse(fields[12]);
                    mTeamInformationRecords[teamID].ClubSeats = Int32.Parse(fields[13]);
                    mTeamInformationRecords[teamID].CityName = fields[14];
                    mTeamInformationRecords[teamID].UpperDeckTickets = Int32.Parse(fields[15]);
                    mTeamInformationRecords[teamID].EndZoneTickets = Int32.Parse(fields[16]);
                    mTeamInformationRecords[teamID].MezzanineTickets = Int32.Parse(fields[17]);
                    mTeamInformationRecords[teamID].SidelineTickets = Int32.Parse(fields[18]);
                    mTeamInformationRecords[teamID].ClubSeatTickets = Int32.Parse(fields[19]);
                    mTeamInformationRecords[teamID].LuxuryBoxTickets = Int32.Parse(fields[20]);
                    mTeamInformationRecords[teamID].LostSalaryCapThisSeason = Int32.Parse(fields[21]);
                    mTeamInformationRecords[teamID].LostSalaryCapNextSeason = Int32.Parse(fields[22]);
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
            BinaryHelper.TracerWriteLine("Active Player IDs");
            BinaryHelper.TracerIndent();
            int i;
            for (i = 0; i < newEntry.ActivePlayerIDs.Length; ++i)
            {
                var activePlayerIndex = BinaryHelper.ReadInt32(inFile, "Player Index " + i.ToString("D2"));
                newEntry.ActivePlayerIDs[i] = BinaryHelper.ReadCodedInt32(inFile, "Player ID " + i.ToString("D2"));
                if (activePlayerIndex >= 65535)
                {
                    newEntry.ActivePlayerIDs[i] = -1;
                }
                newEntry.PlayerStats[i] = new PlayerGameStatsRecord();
                newEntry.PlayerStats[i].PlayerID = newEntry.ActivePlayerIDs[i];
                newEntry.PlayerStats[i].Week = (short)week;
                newEntry.PlayerStats[i].Team = newEntry.TeamIndex;
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

            BinaryHelper.ProbeBytes(inFile, 63 * 2);

            BinaryHelper.TracerOutdent();
        }

        private void LoadPassPlayData(System.IO.BinaryReader inFile, GamePlay playData)
        {
            BinaryHelper.TracerWriteLine("Pass Play");
            BinaryHelper.TracerIndent();

            playData.TypeSpecificData[(int)PassPlayFields.Minute] = BinaryHelper.ReadInt16(inFile, "Minutes");   // 0-15 (Minute)
            playData.TypeSpecificData[(int)PassPlayFields.Seconds] = BinaryHelper.ReadInt16(inFile,"Seconds");  // 0-59 (Second)
            playData.TypeSpecificData[(int)PassPlayFields.PenaltyPlayer] = BinaryHelper.ReadInt16(inFile, "PenaltyPlayer");
            BinaryHelper.ProbeBytes(inFile, 2 * 2);
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
            playData.TypeSpecificData[(int)PassPlayFields.PassDistance] = BinaryHelper.ReadInt16(inFile,"PassDistance");      // 0-8 (Pass Distance, 0: Screen ... 8: Spike)
            playData.TypeSpecificData[(int)PassPlayFields.DefenseFamiliar] = BinaryHelper.ReadInt16(inFile,"DefenseFamiliar");// 0-2 (Defense Familiar: 0: None, 1: very familiar, 2: extremely familiar - not always displayed, depends on play result)
            playData.TypeSpecificData[(int)PassPlayFields.EvadedRushToAvoidSafety] = BinaryHelper.ReadInt16(inFile,"EvadedRushToAvoidSafety");  // 0-1 (1: Evades Rush to avoid the safety)
            playData.TypeSpecificData[(int)PassPlayFields.FieldCondition] = BinaryHelper.ReadInt16(inFile,"FieldCondition");  // 0-5 (Field Condition, 0: Norm, 1: cold, 2: hot, 3: wet, 4: snowy, 5: soaked - not always displayed, depends on play result)
            playData.TypeSpecificData[(int)PassPlayFields.GameLogMessage4Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage4Type");  // Game Log Message re: Turnover on Downs

            BinaryHelper.TracerOutdent();
        }

        private void LoadRunPlayData(System.IO.BinaryReader inFile, GamePlay playData)
        {
            BinaryHelper.TracerWriteLine("Run Play");
            BinaryHelper.TracerIndent();

            playData.TypeSpecificData[(int)RunPlayFields.Minute] = BinaryHelper.ReadInt16(inFile,"Minute");   // 0-15 (Minute)
            playData.TypeSpecificData[(int)RunPlayFields.Seconds] = BinaryHelper.ReadInt16(inFile,"Seconds");  // 0-59 (Second)
            playData.TypeSpecificData[(int)PassPlayFields.PenaltyPlayer] = BinaryHelper.ReadInt16(inFile, "PenaltyPlayer");
            BinaryHelper.ProbeBytes(inFile, 2 * 2);
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
            playData.TypeSpecificData[(int)RunPlayFields.IsForcedOOB] = BinaryHelper.ReadInt16(inFile, "IsForcedOOB");          // 0-1 (1: Forced OOB)
            playData.TypeSpecificData[(int)RunPlayFields.IsSafety] = BinaryHelper.ReadInt16(inFile,"IsSafety");             // 0-1 (1: Safety)
            playData.TypeSpecificData[(int)RunPlayFields.AssistantTackler] = BinaryHelper.ReadInt16(inFile,"AssistantTackler");     // 0-10 (Player Assisting on Tackle, based on position in formation)
            playData.TypeSpecificData[(int)RunPlayFields.WasTackleAssisted] = BinaryHelper.ReadInt16(inFile,"WasTackleAssisted");    // 0-1 (1: Assisted Tackle)
            playData.TypeSpecificData[(int)RunPlayFields.KeyRunBlocker] = BinaryHelper.ReadInt16(inFile,"KeyRunBlocker");        // 0-10 (0: No Key Run Block, 1-10: Player getting the KRB, based on position in formation)
            playData.TypeSpecificData[(int)RunPlayFields.KeyRunBlockOpportunity] = BinaryHelper.ReadInt16(inFile,"KeyRunBlockOpportunity");   // 0-10 (0: No Key Run Block Opportunity, 1-10: Player getting the KRBO, based on position in formation)
            short dat23 = BinaryHelper.ReadInt16(inFile, "Dat23");
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
            short dat38 = BinaryHelper.ReadInt16(inFile, "Dat38");
            playData.TypeSpecificData[(int)RunPlayFields.TurnoverOnDowns] = BinaryHelper.ReadInt16(inFile, "TurnoverOnDowns");      // 0-1 (1: Turnover on Downs)
            short dat40 = BinaryHelper.ReadInt16(inFile, "Dat40");
            short dat41 = BinaryHelper.ReadInt16(inFile, "Dat41");
            playData.TypeSpecificData[(int)RunPlayFields.RunDirection] = BinaryHelper.ReadInt16(inFile, "RunDirection");  // 0-9 (Run Direction, 0: around left end, 1: outside LT, 2: inside LT ... 7: around RE, 8: left reverse (finesse only), 9: right reverse (finesse only))
            playData.TypeSpecificData[(int)RunPlayFields.IsFinesseRun] = BinaryHelper.ReadInt16(inFile,"IsFinesseRun");         // 0-1 (1: Finesse Run, different messages used)
            playData.TypeSpecificData[(int)RunPlayFields.FieldCondition] = BinaryHelper.ReadInt16(inFile,"FieldCondition");  // 0-5 (Field Condition, 0: Norm, 1: cold, 2: hot, 3: wet, 4: snowy, 5: soaked - not always displayed, depends on play result)
            playData.TypeSpecificData[(int)RunPlayFields.DefenseFamiliar] = BinaryHelper.ReadInt16(inFile,"DefenseFamiliar");// 0-2 (Defense Familiar: 0: None, 1: very familiar, 2: extremely familiar - not always displayed, depends on play result)
            playData.TypeSpecificData[(int)RunPlayFields.GameLogMessage4Type] = BinaryHelper.ReadInt16(inFile,"GameLogMessage4Type");  // Game Log Message re: Turnover on Downs
            BinaryHelper.ProbeBytes(inFile, 15 * 2);

            BinaryHelper.TracerOutdent();
        }

        private void LoadKickoffPlayData(System.IO.BinaryReader inFile, GamePlay playData)
        {
            BinaryHelper.TracerWriteLine("Kickoff Play");
            BinaryHelper.TracerIndent();

            playData.TypeSpecificData[(int)KickoffPlayFields.Minute] = BinaryHelper.ReadInt16(inFile,"Minute");
            playData.TypeSpecificData[(int)KickoffPlayFields.Second] = BinaryHelper.ReadInt16(inFile,"Second");
            BinaryHelper.ProbeBytes(inFile, 6);
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
            BinaryHelper.ProbeBytes(inFile, 66);

            BinaryHelper.TracerOutdent();
        }

        private void LoadOnsidePlayData(System.IO.BinaryReader inFile, GamePlay playData)
        {
            BinaryHelper.TracerWriteLine("Onside Play");
            BinaryHelper.TracerIndent();

            BinaryHelper.ProbeBytes(inFile, 63 * 2);

            BinaryHelper.TracerOutdent();
        }

        private void LoadPuntPlayData(System.IO.BinaryReader inFile, GamePlay playData)
        {
            BinaryHelper.TracerWriteLine("Punt Play");
            BinaryHelper.TracerIndent();

            BinaryHelper.ProbeBytes(inFile, 63 * 2);

            BinaryHelper.TracerOutdent();
        }

        private void LoadInfoPlayData(System.IO.BinaryReader inFile, GamePlay playData)
        {
            BinaryHelper.TracerWriteLine("Info Play");
            BinaryHelper.TracerIndent();

            BinaryHelper.ProbeBytes(inFile, 63 * 2);

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
            playData.PenaltyLocation = BinaryHelper.ReadInt16(inFile, "PenaltyLocation");
            BinaryHelper.ProbeBytes(inFile, 2);
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

            BinaryHelper.ProbeBytes(inFile, 36);

            // Offensive Grades
            for (int offensivePlayer = 0; offensivePlayer < playData.OffensiveGrade.Length; ++offensivePlayer)
            {
                playData.OffensiveGrade[offensivePlayer] = BinaryHelper.ReadInt16(inFile, "Offensive Grade " + offensivePlayer.ToString());
            }

            BinaryHelper.ProbeBytes(inFile, 26);

            // Defensive Grades
            for (int defensivePlayer = 0; defensivePlayer < playData.DefensiveGrade.Length; ++defensivePlayer)
            {
                playData.DefensiveGrade[defensivePlayer] = BinaryHelper.ReadInt16(inFile, "Defensive Grade " + defensivePlayer.ToString());
            }

            BinaryHelper.ProbeBytes(inFile, 56);

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

            BinaryHelper.TracerOutdent();

            return playData;
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
            rushInfo.OtherAttempts = (short)(BinaryHelper.ReadInt16(inFile, "OtherAttempts") % 100);
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

            var pogActivePlayerIndex = BinaryHelper.ReadInt32(inFile, "PlayerOfTheGameActivePlayerIndex");
            if (pogActivePlayerIndex < 0 || pogActivePlayerIndex > mActivePlayerIDs.Length)
            {
                newLog.PlayerOfTheGamePlayerID = -1;
            }
            else
            {
                newLog.PlayerOfTheGamePlayerID = mActivePlayerIDs[pogActivePlayerIndex];
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
                if (nextHeader == "PD08")
                {
                    GamePlay playData = LoadPlayData(inFile);
                    ExtractStatsFromPlay(newLog, playData);
                    newLog.Plays.Add(playData);
                    ++CurPlayID;
                }
                else if (nextHeader == "ND08")
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

            BinaryHelper.SetupTracer(logPath);

            while (inFile.PeekChar() >= 0)
            {
                string header = BinaryHelper.ExtractString(inFile, 4, "Header");
                if (header == "WD08")
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
        private void LoadGameList()
        {
            mAvailableGameWeeks = new List<GameWeekRecord>();
            // Read all files that have {GameID}{Week}.{Year} as a format
            // Want to process by year then by week
            string[] gameFiles = System.IO.Directory.GetFiles(mSavedGamePath,mLeagueID+"*.*");
            foreach (string curFile in gameFiles)
            {
                // game ID is 8, week is 1 or 2, 1 for the extension, 4 for the year. No other files follow this pattern/length,
                // unless other people put them there, so we'll do some extra checking.
                string fileName = System.IO.Path.GetFileName(curFile);
                if (fileName.Length >= (8 + 1 + 1 + 4))
                {
                    // strip the period
                    string yearString = System.IO.Path.GetExtension(fileName).Substring(1);
                    short year = 0;
                    if (Int16.TryParse(yearString, out year))
                    {
                        if (year >= mStartingSeason)
                        {
                            string weekString = System.IO.Path.GetFileNameWithoutExtension(fileName).Substring(8);
                            short week;
                            if (Int16.TryParse(weekString, out week))
                            {
                                GameWeekRecord newRec = LoadGameWeek(week, year, curFile);
                                mAvailableGameWeeks.Add(newRec);
                            }
                        }
                    }
                }
            }
            mAvailableGameWeeks.Sort(new GameWeekComparer());
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
                if (finalFieldPosition < 0)
                {
                    finalFieldPosition *= -1;
                }
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
            var assignPerformance = false;

            short penaltyPlayer = -1;
            var acceptedPenalty = false;
            if (playData.PlayType == (short)PlayType.Pass)
            {
                ExtractPassStatsFromPlay(gameLog,playData);
                if (playData.EffectOnPlay != 3)
                {
                    assignPerformance = true;
                }
                penaltyPlayer = playData.TypeSpecificData[(int)PassPlayFields.PenaltyPlayer];
                acceptedPenalty = playData.TypeSpecificData[(int)PassPlayFields.IsPenaltyAccepted] != 0;
            }
            else if (playData.PlayType == (short)PlayType.Run)
            {
                ExtractRunStatsFromPlay(gameLog, playData);
                if (playData.TypeSpecificData[(int)RunPlayFields.RunDirection] != (short)RunDirection.KneelDown
                    && playData.EffectOnPlay != 3)
                {
                    assignPerformance = true;
                }
                penaltyPlayer = playData.TypeSpecificData[(int)RunPlayFields.PenaltyPlayer];
                acceptedPenalty = playData.TypeSpecificData[(int)RunPlayFields.IsPenaltyAccepted] != 0;
            }
            else if (playData.PlayType == (short)PlayType.Kickoff)
            {
                ExtractKickoffStatsFromPlay(gameLog, playData);
            }

            if (penaltyPlayer >= 0)
            {
                if (playData.IsDefensivePenalty != 0)
                {
                    var defRec = GetDefensivePlayerStatsFromPlay(gameLog, playData, penaltyPlayer);
                    defRec.PenaltiesCommitted += 1;
                    if (acceptedPenalty)
                    {
                        defRec.PenaltiesAccepted += 1;
                    }
                }
                else if (playData.IsOffensivePenalty != 0)
                {
                    var offRec = GetOffensivePlayerStatsFromPlay(gameLog, playData, penaltyPlayer);
                    offRec.PenaltiesCommitted += 1;
                    if (acceptedPenalty)
                    {
                        offRec.PenaltiesAccepted += 1;
                    }
                }
            }

            if (assignPerformance)
            {
                for (short i = 0; i < playData.DefensiveGrade.Length; ++i)
                {
                    var offRec = GetOffensivePlayerStatsFromPlay(gameLog, playData, i);
                    if (playData.OffensiveGrade[i] == 1)
                    {
                        offRec.PlusPlays += 1;
                    }
                    else if (playData.OffensiveGrade[i] == 2)
                    {
                        offRec.MinusPlays += 1;
                    }
                    var defRec = GetDefensivePlayerStatsFromPlay(gameLog, playData, i);
                    if (playData.DefensiveGrade[i] == 1)
                    {
                        defRec.PlusPlays += 1;
                    }
                    else if (playData.DefensiveGrade[i] == 2)
                    {
                        defRec.MinusPlays += 1;
                    }

                }
            }
        }

        private const int kTeamCount = 32;
        private const int kSeasonGameCount = 350;
        private const int kSeasonWeekCount = 26;	// 5 preseason + 17 season + 4 playoffs

        private UniverseData mUniverseData;
        private int mStartingSeason;
        private string mLeagueID;
        private string mSavedGamePath;
        private string mExportPath;

        public int NumberOfTeams { get { return kTeamCount; } }

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
                PlusPlays = 0;
                MinusPlays = 0;
                PenaltiesCommitted = 0;
                PenaltiesAccepted = 0;
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
            public int PlusPlays;
            public int MinusPlays;
            public int PenaltiesCommitted;
            public int PenaltiesAccepted;
        };

        private const int kNumActivePlayers = 46;
        private const int kNumDepthChartEntries = 129;
        public class GameTeamEntry
        {
            public short TeamIndex;
            public string CityName;
            public string NickName;
            public string Abbreviation;
            public int[] ActivePlayerIDs;
            public PlayerGameStatsRecord[] PlayerStats;
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
            GameLogMessage4Type,
            PenaltyPlayer
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
            GameLogMessage4Type,
            PenaltyPlayer
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
            public short PenaltyLocation;
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
            public short[] OffensiveGrade = new short[11];
            public short[] DefensiveGrade = new short[11];

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
            public int PlayerOfTheGamePlayerID;
            public GameDriveInfo[] HomeDrives;
            public GameDriveInfo[] AwayDrives;
            public GamePassInfo HomePassing;
            public GamePassInfo AwayPassing;
            public GameRushInfo HomeRushing;
            public GameRushInfo AwayRushing;
            public GamePossessionInfo HomePossessions;
            public GamePossessionInfo AwayPossessions;
            public short[] HomeQuarterScore = new short[5];
            public short[] AwayQuarterScore = new short[5];
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

        public class GameWeekComparer : Comparer<GameWeekRecord>
        {
            public override int Compare(GameWeekRecord rec1, GameWeekRecord rec2)
            {
                if (rec1.Year == rec2.Year)
                {
                    return rec1.Week.CompareTo(rec2.Week);
                }
                else
                {
                    return rec1.Year.CompareTo(rec2.Year);
                }
            }
        }

        public class TeamInformationRecord
        {
            public int Wins;
            public int Losses;
            public int Ties;
            public int PlayoffAppearances;
            public int BowlAppearances;
            public int BowlWins;
            public int PlayoffWins;
            public int PlayoffLosses;
            public string TurfType;
            public int YearStadiumBuilt;
            public int StadiumCapacity;
            public int LuxuryBoxes;
            public int ClubSeats;
            public string CityName;
            public int UpperDeckTickets;
            public int EndZoneTickets;
            public int MezzanineTickets;
            public int SidelineTickets;
            public int ClubSeatTickets;
            public int LuxuryBoxTickets;
            public int LostSalaryCapThisSeason;
            public int LostSalaryCapNextSeason;
        }
        private TeamInformationRecord[] mTeamInformationRecords = new TeamInformationRecord[kTeamCount];
        public TeamInformationRecord[] TeamInformationRecords { get { return mTeamInformationRecords; } }

        private int[] mActivePlayerIDs;
    }
}

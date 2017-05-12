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

        private const string kUseProcessSeason = "UseProcessSeason";
        private const string kSeasonToProcessFrom = "SeasonToProcessFrom";
        private const string kFileToOpenWhenDone = "FileToOpenWhenDone";
        private const string kArguments = "Arguments";

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
        }

        private void UpdateThreadTask()
        {
            AddStatusString("Loading game data...");

            mLeagueData = new LeagueData(mUniverseData.SavedGames[mSavedGameIndex].PathPrefix, mUniverseData, mStartingSeason, OnFileRead, true);
            CreateDataTables();

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
                var fileToOpen = textBoxFileToOpen.Text;
                if (fileToOpen != null && System.IO.File.Exists(fileToOpen))
                {
                    var arguments = textBoxArguments.Text;
                    if (arguments != null && arguments.Length > 0)
                    {
                        System.Diagnostics.Process.Start(fileToOpen, arguments);
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(fileToOpen);
                    }
                }
            }
        }

        private void OnFileRead(string fileName)
        {
            AddStatusString("Reading file " + fileName + System.Environment.NewLine);
        }

        private void WriteString(System.IO.StreamWriter statsFile, string item)
        {
            string tmpItem = ((string)item).Replace('"', '\'');
            statsFile.Write("\"" + tmpItem + "\"");
        }

        private void CreateMappingsDataTable()
        {
            string outputDir = System.IO.Path.Combine(mUniverseData.ExportDirectory, mLeagueID);
            string filename = System.IO.Path.Combine(outputDir, "mappings.csv");
            AddStatusString("Writing mappings");
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

            for (int i = 0; i < 101; i++)
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

        private void CreateGameResultsTable()
        {
            string outputDir = System.IO.Path.Combine(mUniverseData.ExportDirectory, mLeagueID);
            string filename = System.IO.Path.Combine(outputDir, "game_information_extra.csv");
            AddStatusString("Writing game results and drives");
            System.IO.StreamWriter resultsFile = new System.IO.StreamWriter(filename, false);

            resultsFile.Write("Year,");
            resultsFile.Write("Week,");
            resultsFile.Write("GameID,");
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
            resultsFile.Write("BoxCapacity,");
            resultsFile.WriteLine("PoGID");

            filename = System.IO.Path.Combine(outputDir, "drive_results.csv");
            System.IO.StreamWriter drivesFile = new System.IO.StreamWriter(filename, false);

            drivesFile.Write("Year,");
            drivesFile.Write("GameID,");
            drivesFile.Write("DriveID,");
            drivesFile.Write("EndMinutes,");
            drivesFile.Write("EndQuarter,");
            drivesFile.Write("EndSeconds,");
            drivesFile.Write("PlayCount,");
            drivesFile.Write("StartMinutes,");
            drivesFile.Write("StartQuarter,");
            drivesFile.Write("StartSeconds,");
            drivesFile.Write("StartYardsFromGoal,");
            drivesFile.Write("Team,");
            drivesFile.Write("YardsGained,");
            drivesFile.WriteLine("Result");

            System.IO.StreamWriter statsFile = null;

            int oldYear = 0;

            int gameID = 0;
            foreach (LeagueData.GameWeekRecord gameWeekRec in mLeagueData.AvailableGameWeeks)
            {
                if (gameWeekRec.Year != oldYear)
                {
                    if (statsFile != null)
                    {
                        statsFile.Close();
                    }

                    filename = System.IO.Path.Combine(outputDir, "player_season_extra_" + gameWeekRec.Year + ".csv");
                    statsFile = new System.IO.StreamWriter(filename, false);

                    statsFile.Write("PlayerID,");
                    statsFile.Write("Week,");
                    statsFile.Write("Team,");
                    statsFile.Write("DoubleCoveragesThrownInto,");
                    statsFile.Write("DoubleCoveragesAvoided,");
                    statsFile.Write("BadPasses,");
                    statsFile.Write("RunsForLoss,");
                    statsFile.Write("RunsOf20YardsPlus,");
                    statsFile.Write("FumblesLost,");
                    statsFile.Write("HasKeyCoverage,");
                    statsFile.Write("ThrownAt,");
                    statsFile.Write("TacklesForLoss,");
                    statsFile.Write("AssistedTacklesForLoss,");
                    statsFile.Write("ReceptionsOf20YardsPlusGivenUp,");
                    statsFile.Write("Kickoffs,");
                    statsFile.Write("KickoffYards,");
                    statsFile.Write("KickoffTouchbacks,");
                    statsFile.Write("TotalFieldPositionAfterKickoff,");
                    statsFile.Write("OffensivePassPlays,");
                    statsFile.Write("OffensiveRunPlays,");
                    statsFile.Write("DefensivePassPlays,");
                    statsFile.Write("DefensiveRunPlays,");
                    statsFile.Write("SuccessfulPasses,");
                    statsFile.Write("SuccessfulCatches,");
                    statsFile.Write("SuccessfulRuns,");
                    statsFile.Write("BadPassesCaught,");
                    statsFile.Write("PlusPlays,");
                    statsFile.Write("MinusPlays,");
                    statsFile.Write("PenaltiesCommitted,");
                    statsFile.WriteLine("PenaltiesAccepted");

                    oldYear = gameWeekRec.Year;
                    gameID = 0;
                }
                foreach (LeagueData.GameLog curLog in gameWeekRec.GameLogs)
                {
                    resultsFile.Write(gameWeekRec.Year + ",");
                    resultsFile.Write(gameWeekRec.Week + ",");
                    resultsFile.Write(gameID + ",");
                    resultsFile.Write(curLog.HomeRushing.LeftAttempts + ",");
                    resultsFile.Write(curLog.HomeRushing.LeftYards + ",");
                    resultsFile.Write(curLog.HomePassing.LongAttempts + ",");
                    resultsFile.Write(curLog.HomePassing.LongCompletions + ",");
                    resultsFile.Write(curLog.HomePassing.LongYards + ",");
                    resultsFile.Write(curLog.HomePassing.MediumAttempts + ",");
                    resultsFile.Write(curLog.HomePassing.MediumCompletions + ",");
                    resultsFile.Write(curLog.HomePassing.MediumYards + ",");
                    resultsFile.Write(curLog.HomeRushing.MiddleAttempts + ",");
                    resultsFile.Write(curLog.HomeRushing.MiddleYards + ",");
                    resultsFile.Write(curLog.HomePassing.OtherAttempts + ",");
                    resultsFile.Write(curLog.HomePassing.OtherCompletions + ",");
                    resultsFile.Write(curLog.HomePassing.OtherYards + ",");
                    resultsFile.Write(curLog.HomeRushing.OtherAttempts + ",");
                    resultsFile.Write(curLog.HomeRushing.OtherYards + ",");
                    resultsFile.Write(curLog.HomePossessions.RedZoneAttempts + ",");
                    resultsFile.Write(curLog.HomePossessions.RedZoneFieldGoals + ",");
                    resultsFile.Write(curLog.HomePossessions.RedZoneTouchdowns + ",");
                    resultsFile.Write(curLog.HomeRushing.RightAttempts + ",");
                    resultsFile.Write(curLog.HomeRushing.RightYards + ",");
                    resultsFile.Write(curLog.HomePassing.ScreenAttempts + ",");
                    resultsFile.Write(curLog.HomePassing.ScreenCompletions + ",");
                    resultsFile.Write(curLog.HomePassing.ScreenYards + ",");
                    resultsFile.Write(curLog.HomePassing.ShortAttempts + ",");
                    resultsFile.Write(curLog.HomePassing.ShortCompletions + ",");
                    resultsFile.Write(curLog.HomePassing.ShortYards + ",");
                    resultsFile.Write(curLog.HomePossessions.TimeOfPossession + ",");
                    resultsFile.Write(curLog.AwayRushing.LeftAttempts + ",");
                    resultsFile.Write(curLog.AwayRushing.LeftYards + ",");
                    resultsFile.Write(curLog.AwayPassing.LongAttempts + ",");
                    resultsFile.Write(curLog.AwayPassing.LongCompletions + ",");
                    resultsFile.Write(curLog.AwayPassing.LongYards + ",");
                    resultsFile.Write(curLog.AwayPassing.MediumAttempts + ",");
                    resultsFile.Write(curLog.AwayPassing.MediumCompletions + ",");
                    resultsFile.Write(curLog.AwayPassing.MediumYards + ",");
                    resultsFile.Write(curLog.AwayRushing.MiddleAttempts + ",");
                    resultsFile.Write(curLog.AwayRushing.MiddleYards + ",");
                    resultsFile.Write(curLog.AwayPassing.OtherAttempts + ",");
                    resultsFile.Write(curLog.AwayPassing.OtherCompletions + ",");
                    resultsFile.Write(curLog.AwayPassing.OtherYards + ",");
                    resultsFile.Write(curLog.AwayRushing.OtherAttempts + ",");
                    resultsFile.Write(curLog.AwayRushing.OtherYards + ",");
                    resultsFile.Write(curLog.AwayPossessions.RedZoneAttempts + ",");
                    resultsFile.Write(curLog.AwayPossessions.RedZoneFieldGoals + ",");
                    resultsFile.Write(curLog.AwayPossessions.RedZoneTouchdowns + ",");
                    resultsFile.Write(curLog.AwayRushing.RightAttempts + ",");
                    resultsFile.Write(curLog.AwayRushing.RightYards + ",");
                    resultsFile.Write(curLog.AwayPassing.ScreenAttempts + ",");
                    resultsFile.Write(curLog.AwayPassing.ScreenCompletions + ",");
                    resultsFile.Write(curLog.AwayPassing.ScreenYards + ",");
                    resultsFile.Write(curLog.AwayPassing.ShortAttempts + ",");
                    resultsFile.Write(curLog.AwayPassing.ShortCompletions + ",");
                    resultsFile.Write(curLog.AwayPassing.ShortYards + ",");
                    resultsFile.Write(curLog.AwayPossessions.TimeOfPossession + ",");
                    resultsFile.Write(curLog.TotalCapacity + ",");
                    resultsFile.Write(curLog.UpperDeckAttendance + ",");
                    resultsFile.Write(curLog.UpperDeckCapacity + ",");
                    resultsFile.Write(curLog.EndZoneCapacity + ",");
                    resultsFile.Write(curLog.EndZoneAttendance + ",");
                    resultsFile.Write(curLog.MezzanineAttendance + ",");
                    resultsFile.Write(curLog.MezzanineCapacity + ",");
                    resultsFile.Write(curLog.SidelineAttendance + ",");
                    resultsFile.Write(curLog.SidelineCapacity + ",");
                    resultsFile.Write(curLog.ClubAttendance + ",");
                    resultsFile.Write(curLog.ClubCapacity + ",");
                    resultsFile.Write(curLog.BoxAttendance + ",");
                    resultsFile.Write(curLog.BoxCapacity + ",");
                    resultsFile.WriteLine(curLog.PlayerOfTheGamePlayerID);

                    int driveID = 0;
                    foreach (LeagueData.GameDriveInfo homeDriveInfo in curLog.HomeDrives)
                    {
                        drivesFile.Write(gameWeekRec.Year + ",");
                        drivesFile.Write(gameID + ",");
                        drivesFile.Write(driveID + ",");
                        drivesFile.Write(homeDriveInfo.DriveEndMinutes + ",");
                        drivesFile.Write(homeDriveInfo.DriveEndQuarter + ",");
                        drivesFile.Write(homeDriveInfo.DriveEndSeconds + ",");
                        drivesFile.Write(homeDriveInfo.Plays + ",");
                        drivesFile.Write(homeDriveInfo.DriveStartMinutes + ",");
                        drivesFile.Write(homeDriveInfo.DriveStartQuarter + ",");
                        drivesFile.Write(homeDriveInfo.DriveStartSeconds + ",");
                        drivesFile.Write(homeDriveInfo.YardsFromGoalStart + ",");
                        drivesFile.Write(curLog.HomeTeam.TeamIndex + ",");
                        drivesFile.Write(homeDriveInfo.YardsGained + ",");
                        drivesFile.WriteLine(homeDriveInfo.Result);

                        driveID += 1;
                    }
                    foreach (LeagueData.GameDriveInfo awayDriveInfo in curLog.AwayDrives)
                    {
                        drivesFile.Write(gameWeekRec.Year + ",");
                        drivesFile.Write(gameID + ",");
                        drivesFile.Write(driveID + ",");
                        drivesFile.Write(awayDriveInfo.DriveEndMinutes + ",");
                        drivesFile.Write(awayDriveInfo.DriveEndQuarter + ",");
                        drivesFile.Write(awayDriveInfo.DriveEndSeconds + ",");
                        drivesFile.Write(awayDriveInfo.Plays + ",");
                        drivesFile.Write(awayDriveInfo.DriveStartMinutes + ",");
                        drivesFile.Write(awayDriveInfo.DriveStartQuarter + ",");
                        drivesFile.Write(awayDriveInfo.DriveStartSeconds + ",");
                        drivesFile.Write(awayDriveInfo.YardsFromGoalStart + ",");
                        drivesFile.Write(curLog.AwayTeam.TeamIndex + ",");
                        drivesFile.Write(awayDriveInfo.YardsGained + ",");
                        drivesFile.WriteLine(awayDriveInfo.Result);

                        driveID += 1;
                    }

                    foreach (LeagueData.PlayerGameStatsRecord rec in curLog.HomeTeam.PlayerStats)
                    {
                        if (rec.PlayerID < 0)
                        {
                            continue;
                        }
                        statsFile.Write(rec.PlayerID + ",");
                        statsFile.Write((rec.Week - 5) + ",");
                        statsFile.Write(rec.Team + ",");
                        statsFile.Write(rec.DoubleCoveragesThrownInto + ",");
                        statsFile.Write(rec.DoubleCoveragesAvoided + ",");
                        statsFile.Write(rec.BadPasses + ",");
                        statsFile.Write(rec.RunsForLoss + ",");
                        statsFile.Write(rec.RunsOf20YardsPlus + ",");
                        statsFile.Write(rec.FumblesLost + ",");
                        statsFile.Write(rec.HasKeyCoverage + ",");
                        statsFile.Write(rec.ThrownAt + ",");
                        statsFile.Write(rec.TacklesForLoss + ",");
                        statsFile.Write(rec.AssistedTacklesForLoss + ",");
                        statsFile.Write(rec.ReceptionsOf20YardsPlusGivenUp + ",");
                        statsFile.Write(rec.Kickoffs + ",");
                        statsFile.Write(rec.KickoffYards + ",");
                        statsFile.Write(rec.KickoffTouchbacks + ",");
                        statsFile.Write(rec.TotalFieldPositionAfterKickoff + ",");
                        statsFile.Write(rec.OffensivePassPlays + ",");
                        statsFile.Write(rec.OffensiveRunPlays + ",");
                        statsFile.Write(rec.DefensivePassPlays + ",");
                        statsFile.Write(rec.DefensiveRunPlays + ",");
                        statsFile.Write(rec.SuccessfulPasses + ",");
                        statsFile.Write(rec.SuccessfulCatches + ",");
                        statsFile.Write(rec.SuccessfulRuns + ",");
                        statsFile.Write(rec.BadPassesCaught + ",");
                        statsFile.Write(rec.PlusPlays + ",");
                        statsFile.Write(rec.MinusPlays + ",");
                        statsFile.Write(rec.PenaltiesCommitted + ",");
                        statsFile.WriteLine(rec.PenaltiesAccepted);
                    }

                    foreach (LeagueData.PlayerGameStatsRecord rec in curLog.AwayTeam.PlayerStats)
                    {
                        if (rec.PlayerID < 0)
                        {
                            continue;
                        }
                        statsFile.Write(rec.PlayerID + ",");
                        statsFile.Write((rec.Week - 5) + ",");
                        statsFile.Write(rec.Team + ",");
                        statsFile.Write(rec.DoubleCoveragesThrownInto + ",");
                        statsFile.Write(rec.DoubleCoveragesAvoided + ",");
                        statsFile.Write(rec.BadPasses + ",");
                        statsFile.Write(rec.RunsForLoss + ",");
                        statsFile.Write(rec.RunsOf20YardsPlus + ",");
                        statsFile.Write(rec.FumblesLost + ",");
                        statsFile.Write(rec.HasKeyCoverage + ",");
                        statsFile.Write(rec.ThrownAt + ",");
                        statsFile.Write(rec.TacklesForLoss + ",");
                        statsFile.Write(rec.AssistedTacklesForLoss + ",");
                        statsFile.Write(rec.ReceptionsOf20YardsPlusGivenUp + ",");
                        statsFile.Write(rec.Kickoffs + ",");
                        statsFile.Write(rec.KickoffYards + ",");
                        statsFile.Write(rec.KickoffTouchbacks + ",");
                        statsFile.Write(rec.TotalFieldPositionAfterKickoff + ",");
                        statsFile.Write(rec.OffensivePassPlays + ",");
                        statsFile.Write(rec.OffensiveRunPlays + ",");
                        statsFile.Write(rec.DefensivePassPlays + ",");
                        statsFile.Write(rec.DefensiveRunPlays + ",");
                        statsFile.Write(rec.SuccessfulPasses + ",");
                        statsFile.Write(rec.SuccessfulCatches + ",");
                        statsFile.Write(rec.SuccessfulRuns + ",");
                        statsFile.Write(rec.BadPassesCaught + ",");
                        statsFile.Write(rec.PlusPlays + ",");
                        statsFile.Write(rec.MinusPlays + ",");
                        statsFile.Write(rec.PenaltiesCommitted + ",");
                        statsFile.WriteLine(rec.PenaltiesAccepted);
                    }

                    gameID += 1;
                }
            }

            if (statsFile != null)
            {
                statsFile.Close();
            }
            if (drivesFile != null)
            {
                drivesFile.Close();
            }
            if (resultsFile != null)
            {
                resultsFile.Close();
            }
        }

        private void CreateDataTables()
        {
            CreateGameResultsTable();
        }

        private int mStartingSeason;
        private string mLeagueID;
        private void buttonGenerateCSV_Click(object sender, EventArgs e)
        {
            if (mLeagueID != null)
            {
                mStartingSeason = 0;
                if (checkBoxProcessFromSeason.Checked)
                {
                    if (!Int32.TryParse(textBoxStartingSeason.Text, out mStartingSeason))
                    {
                        mStartingSeason = 0;
                    }
                }
                mLeagueID = mUniverseData.SavedGames[mSavedGameIndex].GameID;

                mSettings.WriteXMLValue(mLeagueID, kUseProcessSeason, checkBoxProcessFromSeason.Checked);
                mSettings.WriteXMLString(mLeagueID, kSeasonToProcessFrom, textBoxStartingSeason.Text);
                mSettings.WriteXMLString(mLeagueID, kFileToOpenWhenDone, textBoxFileToOpen.Text);
                mSettings.WriteXMLString(mLeagueID, kArguments, textBoxArguments.Text);

                mOldCursor = Cursor;
                Cursor = Cursors.WaitCursor;
                buttonGenerateCSV.Enabled = false;
                System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateThreadTask));
                updateThread.IsBackground = true;
                updateThread.Start();
            }
        }

        private void comboBoxSavedGame_SelectedIndexChanged(object sender, EventArgs e)
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
                mLeagueID = mUniverseData.SavedGames[mSavedGameIndex].GameID;

                checkBoxProcessFromSeason.Checked = mSettings.ReadXMLbool(mLeagueID, kUseProcessSeason, false);
                textBoxStartingSeason.Text = mSettings.ReadXMLString(mLeagueID, kSeasonToProcessFrom, "");
                textBoxFileToOpen.Text = mSettings.ReadXMLString(mLeagueID, kFileToOpenWhenDone, "");
                textBoxArguments.Text = mSettings.ReadXMLString(mLeagueID, kArguments, "");
            }
            else
            {
                mLeagueID = null;
                checkBoxProcessFromSeason.Checked = false;
                textBoxFileToOpen.Text = "";
                textBoxStartingSeason.Text = "";
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxFileToOpen.Text = dlg.FileName;
            }
        }

        private void buttonReleaseNotes_Click(object sender, EventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo("in_release_notes.txt");
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }
    }
}
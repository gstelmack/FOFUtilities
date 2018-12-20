import os
import struct
import binaryHelper

class solevision(object):
    """Reads Solevision data"""

    def __init__(self, league_id, starting_season):
        self.save_dir = os.path.expandvars(r'%LOCALAPPDATA%\Solecismic Software\Front Office Football Eight\leagues')
        self.export_dir = os.path.expandvars(r'%LOCALAPPDATA%\Solecismic Software\Front Office Football Eight\leaguedata')
        self.league_id = league_id
        self.starting_season = starting_season

    def load_game_play(self, binary_helper):
        binary_helper.start_block('GamePlay')
        quarter = binary_helper.read_int16('Quarter', self.game_plays_csv)
        minutes = binary_helper.read_int16('Minutes', self.game_plays_csv)
        seconds = binary_helper.read_int16('Seconds', self.game_plays_csv)
        possession = binary_helper.read_int16('Possession', self.game_plays_csv)
        down = binary_helper.read_int16('Down', self.game_plays_csv)
        yardsToGo = binary_helper.read_int16('YardsToGo', self.game_plays_csv)
        yardLine = binary_helper.read_int16('YardLine', self.game_plays_csv)
        homeTimeouts = binary_helper.read_int16('Home Timeouts', self.game_plays_csv)
        awayTimeouts = binary_helper.read_int16('Away Timeouts', self.game_plays_csv)
        playType = binary_helper.read_int16('Play Type', self.game_plays_csv)

        # Formation Data, mostly valid for playtype 5 or 6
        offensiveFormation = binary_helper.read_int16('Offensive Formation', self.game_plays_csv)
        offensivePersonnel = binary_helper.read_int16('Offensive Personnel', self.game_plays_csv)
        defensiveAlignment = binary_helper.read_int16('Defensive Alignment', self.game_plays_csv)
        defensivePersonnel = binary_helper.read_int16('Defensive Personnel', self.game_plays_csv)
        defensiveCoverage = binary_helper.read_int16('Defensive Coverage', self.game_plays_csv)
        defensiveFront = binary_helper.read_int16('Defensive Front', self.game_plays_csv)
        defensivePlaycallSpecialty = binary_helper.read_int16('Defensive Playcall Specialty', self.game_plays_csv)
        possessionChange = binary_helper.read_int16('Possession Change', self.game_plays_csv)
        defensiveBlitzCount = binary_helper.read_int16('Defensive Blitz Count', self.game_plays_csv)    # 0-3, how many blitzing
        defensiveBlitzers = list()
        for blitzer in range (0,10):
            defensiveBlitzers.append(binary_helper.read_int16('Defensive Blitzer ' + str(blitzer), self.game_plays_csv))

        # Penalty Data
        isDefensivePenalty = binary_helper.read_int16('IsDefensivePenalty', self.game_plays_csv)
        isOffensivePenalty = binary_helper.read_int16('IsOffensivePenalty', self.game_plays_csv)
        penaltyYardage = binary_helper.read_int16('PenaltyYardage', self.game_plays_csv)
        happenedOnPuntOrKick = binary_helper.read_int16('HappenedOnPuntOrKick', self.game_plays_csv)
        resultsInFirstDown = binary_helper.read_int16('ResultsInFirstDown', self.game_plays_csv)
        resultsInLossOfDown = binary_helper.read_int16('ResultsInLossOfDown', self.game_plays_csv)
        penaltyLocation = binary_helper.read_int16('PenaltyLocation', self.game_plays_csv)
        unknown_1 = binary_helper.read_int16('Unknown1', self.game_info_csv)
        inEndZone = binary_helper.read_int16('InEndZone', self.game_plays_csv)
        yardLineIfAccepted = binary_helper.read_int16('YardLineIfAccepted', self.game_plays_csv)
        downIfAccepted = binary_helper.read_int16('DownIfAccepted', self.game_plays_csv)
        yardsToGoIfAccepted = binary_helper.read_int16('YardsToGoIfAccepted', self.game_plays_csv)
        yardLineIfDeclined = binary_helper.read_int16('YardLineIfDeclined', self.game_plays_csv)
        downIfDeclined = binary_helper.read_int16('DownIfDeclined', self.game_plays_csv)
        yardsToGoIfDeclined = binary_helper.read_int16('YardsToGoIfDeclined', self.game_plays_csv)
        unknown_2 = binary_helper.read_int16('Unknown2', self.game_info_csv)
        unknown_3 = binary_helper.read_int16('Unknown3', self.game_info_csv)
        unknown_4 = binary_helper.read_int16('Unknown4', self.game_info_csv)
        unknown_5 = binary_helper.read_int16('Unknown5', self.game_info_csv)
        penaltyType = binary_helper.read_int16('Penalty Type', self.game_plays_csv)
        isDefensiveEndOfHalfPenalty = binary_helper.read_int16('IsDefensiveEndOfHalfPenalty', self.game_plays_csv)
        unknown_6 = binary_helper.read_int16('Unknown6', self.game_info_csv)
        unknown_7 = binary_helper.read_int16('Unknown7', self.game_info_csv)
        unknown_8 = binary_helper.read_int16('Unknown8', self.game_info_csv)
        unknown_9 = binary_helper.read_int16('Unknown9', self.game_info_csv)
        unknown_10 = binary_helper.read_int16('Unknown10', self.game_info_csv)
        effectOnPlay = binary_helper.read_int16('EffectOnPlay', self.game_plays_csv)

        # Injury?
        injuryType = binary_helper.read_int16('InjuryType', self.game_plays_csv)
        injuredPlayer = binary_helper.read_int16('InjuredPlayer', self.game_plays_csv)

        # Play-specific data goes here
        playSpecific = list()
        for playDataIndex in range (0,63):
            playSpecific.append(binary_helper.read_int16('PlayData ' + str(playDataIndex), self.game_plays_csv))

        # Playcall bits
        binary_helper.read_int16('Failed4thDown?', self.game_plays_csv)
        binary_helper.read_int16('OffensivePersonnelRepeat', self.game_plays_csv)
        binary_helper.read_int16('FormationRepeat', self.game_plays_csv)
        binary_helper.read_int16('Unknown', self.game_plays_csv)
        qBDepth = binary_helper.read_int16('QBDepth', self.game_plays_csv)
        offensivePlayType = binary_helper.read_int16('PlayType', self.game_plays_csv)
        runDirection = binary_helper.read_int16('RunDirection', self.game_plays_csv)
        ballCarrier = binary_helper.read_int16('BallCarrier', self.game_plays_csv)
        for receiver in range(0, 5):
            role = binary_helper.read_int16('Role ' + str(receiver), self.game_plays_csv)
            route = binary_helper.read_int16('Route ' + str(receiver), self.game_plays_csv)

        # Offensive Grades
        for offensivePlayerIndex in range(0,11):
            offensiveGrade = binary_helper.read_int16('Offensive Grade ' + str(offensivePlayerIndex), self.game_plays_csv)

        # Defensive bits
        binary_helper.read_int16('Possession Change?', self.game_plays_csv)
        binary_helper.read_int16('DefensivePersonnel', self.game_plays_csv)
        for defensiveAssignmentIndex in range(0,11):
            defensiveAssignment = binary_helper.read_int16('DefensiveAssignment ' + str(defensiveAssignmentIndex), self.game_plays_csv)

        # Defensive Grades
        for defensivePlayerIndex in range(0, 11):
            defensiveGrade = binary_helper.read_int16('Defensive Grade ' + str(defensivePlayerIndex), self.game_plays_csv)

        # Unknown section
        # parts of this include defensive formation info, but no idea what the other parts are
        for i in range(0,28):
            nextVal = binary_helper.read_int16('Section Unk ' + str(i), self.game_plays_csv)

        # Offensive Lineup
        for offensivePlayerIndex in range(0,11):
            # Note: these numbers may be 47+, in which case we need to subtract 47 from the result to index into the active player list
            activeRosterIndex = binary_helper.read_int16('Offensive Player ' + str(offensivePlayerIndex), self.game_plays_csv)

        # Defensive Lineup
        for defensivePlayerIndex in range(0, 11):
            # Note: these numbers may be 47+, in which case we need to subtract 47 from the result to index into the active player list
            activeRosterIndex = binary_helper.read_int16('DefensivePlayers ' + str(defensivePlayerIndex), self.game_plays_csv)

        self.game_plays_csv.write('\n')
        binary_helper.end_block()

    def load_game_drive(self, binary_helper):
        binary_helper.start_block('GameDrive')
        driveStartQuarter = binary_helper.read_int16('StartQuarter', self.game_drives_csv)
        driveStartMinutes = binary_helper.read_int16('StartMinutes', self.game_drives_csv)
        driveStartSeconds = binary_helper.read_int16('StartSeconds', self.game_drives_csv)
        driveEndQuarter = binary_helper.read_int16('EndQuarter', self.game_drives_csv)
        driveEndMinutes = binary_helper.read_int16('EndMinutes', self.game_drives_csv)
        driveEndSeconds = binary_helper.read_int16('EndSeconds', self.game_drives_csv)
        yardsFromGoalStart = binary_helper.read_int16('YardsFromGoal', self.game_drives_csv)
        plays = binary_helper.read_int16('Plays', self.game_drives_csv)
        yardsGained = binary_helper.read_int16('YardsGained', self.game_drives_csv)
        result = binary_helper.read_int16('Result', self.game_drives_csv)
        self.game_drives_csv.write('\n')
        binary_helper.end_block()

    def load_game_passing_info(self, binary_helper, prefix):
        screenAttempts = binary_helper.read_int16(prefix + ' ScreenAttempts', self.game_box_csv)
        screenCompletions = binary_helper.read_int16(prefix + ' ScreenCompletions', self.game_box_csv)
        screenYards = binary_helper.read_int16(prefix + ' ScreenYards', self.game_box_csv)
        shortAttempts = binary_helper.read_int16(prefix + ' ShortAttempts', self.game_box_csv)
        shortCompletions = binary_helper.read_int16(prefix + ' ShortCompletions', self.game_box_csv)
        shortYards = binary_helper.read_int16(prefix + ' ShortYards', self.game_box_csv)
        mediumAttempts = binary_helper.read_int16(prefix + ' MediumAttempts', self.game_box_csv)
        mediumCompletions = binary_helper.read_int16(prefix + ' MediumCompletions', self.game_box_csv)
        mediumYards = binary_helper.read_int16(prefix + ' MediumYards', self.game_box_csv)
        longAttempts = binary_helper.read_int16(prefix + ' LongAttempts', self.game_box_csv)
        longCompletions = binary_helper.read_int16(prefix + ' LongCompletions', self.game_box_csv)
        longYards = binary_helper.read_int16(prefix + ' LongYards', self.game_box_csv)
        otherAttempts = binary_helper.read_int16(prefix + ' OtherAttempts', self.game_box_csv)
        otherCompletions = binary_helper.read_int16(prefix + ' OtherCompletions', self.game_box_csv)
        otherYards = binary_helper.read_int16(prefix + ' OtherYards', self.game_box_csv)

    def load_game_rushing_info(self, binary_helper, prefix):
        leftAttempts = binary_helper.read_int16(prefix + ' LeftAttempts', self.game_box_csv)
        leftYards = binary_helper.read_int16(prefix + ' LeftYards', self.game_box_csv)
        middleAttempts = binary_helper.read_int16(prefix + ' MiddleAttempts', self.game_box_csv)
        middleYards = binary_helper.read_int16(prefix + ' MiddleYards', self.game_box_csv)
        rightAttempts = binary_helper.read_int16(prefix + ' RightAttempts', self.game_box_csv)
        rightYards = binary_helper.read_int16(prefix + ' RightYards', self.game_box_csv)
        otherAttempts = binary_helper.read_int16(prefix + ' OtherAttempts', self.game_box_csv) % 100
        otherYards = binary_helper.read_int16(prefix + ' OtherYards', self.game_box_csv)

    def load_game_possession_info(self, binary_helper, prefix):
        timeOfPossession = binary_helper.read_int16(prefix + ' TimeOfPossession', self.game_box_csv)
        redZoneAttempts = binary_helper.read_int16(prefix + ' RedZoneAttempts', self.game_box_csv)
        redZoneTouchdowns = binary_helper.read_int16(prefix + ' RedZoneTDs', self.game_box_csv)
        redZoneFieldGoals = binary_helper.read_int16(prefix + ' RedZoneFGs', self.game_box_csv)

    def load_box_score(self, binary_helper):
        binary_helper.start_block('GameBox')
        pogActivePlayerIndex = binary_helper.read_int32('PlayerOfTheGameActivePlayerIndex', self.game_box_csv)
        homeDriveCount = binary_helper.read_int16('HomeDriveCount', self.game_box_csv)
        awayDriveCount = binary_helper.read_int16('AwayDriveCount', self.game_box_csv)
        for i in range(0, homeDriveCount):
            self.load_game_drive(binary_helper)
        for i in range(0, awayDriveCount):
            self.load_game_drive(binary_helper)
        self.load_game_passing_info(binary_helper, 'Home')
        self.load_game_passing_info(binary_helper, 'Away')
        self.load_game_rushing_info(binary_helper, 'Home')
        self.load_game_rushing_info(binary_helper, 'Away')
        self.load_game_possession_info(binary_helper, 'Home')
        self.load_game_possession_info(binary_helper, 'Away')
        self.game_box_csv.write('\n')
        binary_helper.end_block()

    def load_team_entry(self, binary_helper, prefix):
        teamIndex = binary_helper.read_int16(prefix + ' Team Index', self.game_info_csv)
        cityName = binary_helper.read_string(prefix + ' City Name', self.game_info_csv)
        nickName = binary_helper.read_string(prefix + ' Nickname', self.game_info_csv)
        abbreviation = binary_helper.read_string(prefix + ' Abbreviation', self.game_info_csv)
        for i in range(0, 46):
            activePlayerIndex = binary_helper.read_int32(prefix + ' Player Index ' + str(i), self.game_info_csv)
            activePlayerId = binary_helper.read_coded_int32(prefix + ' Player ID ' + str(i), self.game_info_csv)
        for i in range(0, 129):
            depthChartEntry = binary_helper.read_int16(prefix + ' Depth Chart Entry ' + str(i), self.game_info_csv)

    def load_game_log(self, binary_helper):
        binary_helper.start_block('GameInfo')
        year = binary_helper.read_int16('Year', self.game_info_csv)
        week = binary_helper.read_int16('Week', self.game_info_csv)
        location = binary_helper.read_string('Location', self.game_info_csv)
        description = binary_helper.read_string('Description', self.game_info_csv)
        totalAttendance = binary_helper.read_int16('Total Attendance', self.game_info_csv) * 100
        unknown_1 = binary_helper.read_int16('Unknown1', self.game_info_csv)
        upperDeckAttendance = binary_helper.read_int16('Upper Deck Attendance', self.game_info_csv) * 100
        upperDeckCapacity = binary_helper.read_int16('Upper Deck Capacity', self.game_info_csv) * 100
        endZoneAttendance = binary_helper.read_int16('End Zone Attendance', self.game_info_csv) * 100
        endZoneCapacity = binary_helper.read_int16('End Zone Capacity', self.game_info_csv) * 100
        mezzanineAttendance = binary_helper.read_int16('Mezzanine Attendance', self.game_info_csv) * 100
        mezzanineCapacity = binary_helper.read_int16('Mezzanine Capacity', self.game_info_csv) * 100
        sidelineAttendance = binary_helper.read_int16('Sideline Attendance', self.game_info_csv) * 100
        sidelineCapacity = binary_helper.read_int16('Sideline Capacity', self.game_info_csv) * 100
        clubAttendance = binary_helper.read_int16('Club Attendance', self.game_info_csv) * 100
        clubCapacity = binary_helper.read_int16('Club Capacity', self.game_info_csv) * 100
        boxAttendance = binary_helper.read_int16('Box Attendance', self.game_info_csv) * 100
        boxCapacity = binary_helper.read_int16('Box Capacity', self.game_info_csv) * 100
        unknown_2 = binary_helper.read_int16('Unknown2', self.game_info_csv)
        unknown_3 = binary_helper.read_int16('Unknown3', self.game_info_csv)
        temperature = binary_helper.read_int16('Temperature', self.game_info_csv)
        weather = binary_helper.read_int16('Weather', self.game_info_csv)
        totalCapacity = binary_helper.read_int16('Total Capacity', self.game_info_csv) * 100;
        windStrength = binary_helper.read_int16('Wind Strength', self.game_info_csv)
        self.load_team_entry(binary_helper, 'Home')
        self.load_team_entry(binary_helper, 'Away')
        unknown_4 = binary_helper.read_int16('Unknown4', self.game_info_csv)
        self.game_info_csv.write('\n')

        play_index = 0
        while not binary_helper.at_eof():
            block_header = binary_helper.read_block_header()
            if block_header == 'PD08':
                self.load_game_play(binary_helper)
            elif block_header == 'ND08':
                self.load_box_score(binary_helper)
            elif block_header == 'WD08':
                binary_helper.unwind_block_header()
                break
            else:
                break
        binary_helper.end_block()


    def load_game_week(self, year, week, file_name):
        self.game_id = 0
        print('loading game file ' + file_name + ' for year ' + str(year) + ' wk ' + str(week))
        with open(file_name, 'rb') as in_file:
            file_content = in_file.read()
        file_index = 0
        binary_helper = binaryHelper.binaryHelper(os.path.join(self.export_dir, self.league_id), file_content, file_index)
        with open(os.path.join(self.export_dir, self.league_id, 'GameInfo' + str(year) + str(week) + '.csv'), 'w') as self.game_info_csv:
            with open(os.path.join(self.export_dir, self.league_id, 'GamePlays' + str(year) + str(week) + '.csv'), 'w') as self.game_plays_csv:
                with open(os.path.join(self.export_dir, self.league_id, 'GameBoxScores' + str(year) + str(week) + '.csv'), 'w') as self.game_box_csv:
                    with open(os.path.join(self.export_dir, self.league_id, 'GameDrives' + str(year) + str(week) + '.csv'), 'w') as self.game_drives_csv:
                        while not binary_helper.at_eof():
                            block_header = binary_helper.read_block_header()
                            if block_header != 'WD08':
                                print('Found ' + block_header + ' instead of WD08')
                                break
                            self.load_game_log(binary_helper)

    def load_games(self):
        import glob
        league_dir = os.path.join(self.save_dir, self.league_id)
        for game_file_path in glob.iglob(os.path.join(league_dir, self.league_id + '*.*')):
                # game ID is 8, week is 1 or 2, 1 for the extension, 4 for the
                # year.  No other files follow this pattern/length,
                # unless other people put them there, so we'll do some extra
                # checking.
                game_file_name = os.path.basename(game_file_path)
                if len(game_file_name) >= (8 + 1 + 1 + 4):
                    # strip the period
                    base_name, year_string = os.path.splitext(game_file_name)
                    year_string = year_string[1:]
                    week_string = base_name[8:]
                    try:
                        year = int(year_string)
                        if year < starting_season:
                            continue
                        week = int(week_string)
                    except:
                        continue
                    self.load_game_week(year, week, game_file_path)

if __name__ == "__main__":
    import sys
    starting_season = 0
    if len(sys.argv) == 3:
        starting_season = int(sys.argv[2])
    sole_reader = solevision(sys.argv[1], starting_season)
    sole_reader.load_games()

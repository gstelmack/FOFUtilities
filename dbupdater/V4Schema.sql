ALTER TABLE `fof_mappings` ADD COLUMN `DriveResult` VARCHAR(15) NOT NULL AFTER `ConstructionType`;

ALTER TABLE `fof_gameresults` ADD COLUMN `PlayerOfTheGame` INTEGER UNSIGNED NOT NULL AFTER `HomeReceivingYards`,
 ADD COLUMN `HomeScreenAttempts` TINYINT UNSIGNED NOT NULL AFTER `PlayerOfTheGame`,
 ADD COLUMN `HomeScreenCompletions` TINYINT UNSIGNED NOT NULL AFTER `HomeScreenAttempts`,
 ADD COLUMN `HomeScreenYards` SMALLINT NOT NULL AFTER `HomeScreenCompletions`,
 ADD COLUMN `HomeShortAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeScreenYards`,
 ADD COLUMN `HomeShortCompletions` TINYINT UNSIGNED NOT NULL AFTER `HomeShortAttempts`,
 ADD COLUMN `HomeShortYards` SMALLINT NOT NULL AFTER `HomeShortCompletions`,
 ADD COLUMN `HomeMediumAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeShortYards`,
 ADD COLUMN `HomeMediumCompletions` TINYINT UNSIGNED NOT NULL AFTER `HomeMediumAttempts`,
 ADD COLUMN `HomeMediumYards` SMALLINT NOT NULL AFTER `HomeMediumCompletions`,
 ADD COLUMN `HomeLongAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeMediumYards`,
 ADD COLUMN `HomeLongCompletions` TINYINT UNSIGNED NOT NULL AFTER `HomeLongAttempts`,
 ADD COLUMN `HomeLongYards` SMALLINT NOT NULL AFTER `HomeLongCompletions`,
 ADD COLUMN `HomeOtherAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeLongYards`,
 ADD COLUMN `HomeOtherCompletions` TINYINT UNSIGNED NOT NULL AFTER `HomeOtherAttempts`,
 ADD COLUMN `HomeOtherYards` SMALLINT NOT NULL AFTER `HomeOtherCompletions`,
 ADD COLUMN `AwayScreenAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeOtherYards`,
 ADD COLUMN `AwayScreenCompletions` TINYINT UNSIGNED NOT NULL AFTER `AwayScreenAttempts`,
 ADD COLUMN `AwayScreenYards` SMALLINT NOT NULL AFTER `AwayScreenCompletions`,
 ADD COLUMN `AwayShortAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayScreenYards`,
 ADD COLUMN `AwayShortCompletions` TINYINT UNSIGNED NOT NULL AFTER `AwayShortAttempts`,
 ADD COLUMN `AwayShortYards` SMALLINT NOT NULL AFTER `AwayShortCompletions`,
 ADD COLUMN `AwayMediumAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayShortYards`,
 ADD COLUMN `AwayMediumCompletions` TINYINT UNSIGNED NOT NULL AFTER `AwayMediumAttempts`,
 ADD COLUMN `AwayMediumYards` SMALLINT NOT NULL AFTER `AwayMediumCompletions`,
 ADD COLUMN `AwayLongAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayMediumYards`,
 ADD COLUMN `AwayLongCompletions` TINYINT UNSIGNED NOT NULL AFTER `AwayLongAttempts`,
 ADD COLUMN `AwayLongYards` SMALLINT NOT NULL AFTER `AwayLongCompletions`,
 ADD COLUMN `AwayOtherAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayLongYards`,
 ADD COLUMN `AwayOtherCompletions` TINYINT UNSIGNED NOT NULL AFTER `AwayOtherAttempts`,
 ADD COLUMN `AwayOtherYards` SMALLINT NOT NULL AFTER `AwayOtherCompletions`,
 ADD COLUMN `HomeLeftRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayOtherYards`,
 ADD COLUMN `HomeLeftRushYards` SMALLINT NOT NULL AFTER `HomeLeftRushAttempts`,
 ADD COLUMN `HomeMiddleRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeLeftRushYards`,
 ADD COLUMN `HomeMiddleRushYards` SMALLINT NOT NULL AFTER `HomeMiddleRushAttempts`,
 ADD COLUMN `HomeRightRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeMiddleRushYards`,
 ADD COLUMN `HomeRightRushYards` SMALLINT NOT NULL AFTER `HomeRightRushAttempts`,
 ADD COLUMN `HomeOtherRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeRightRushYards`,
 ADD COLUMN `HomeOtherRushYards` SMALLINT NOT NULL AFTER `HomeOtherRushAttempts`,
 ADD COLUMN `AwayLeftRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeOtherRushYards`,
 ADD COLUMN `AwayLeftRushYards` SMALLINT NOT NULL AFTER `AwayLeftRushAttempts`,
 ADD COLUMN `AwayMiddleRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayLeftRushYards`,
 ADD COLUMN `AwayMiddleRushYards` SMALLINT NOT NULL AFTER `AwayMiddleRushAttempts`,
 ADD COLUMN `AwayRightRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayMiddleRushYards`,
 ADD COLUMN `AwayRightRushYards` SMALLINT NOT NULL AFTER `AwayRightRushAttempts`,
 ADD COLUMN `AwayOtherRushAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayRightRushYards`,
 ADD COLUMN `AwayOtherRushYards` SMALLINT NOT NULL AFTER `AwayOtherRushAttempts`,
 ADD COLUMN `HomeTimeOfPossession` SMALLINT UNSIGNED NOT NULL AFTER `AwayOtherRushYards`,
 ADD COLUMN `HomeRedZoneAttempts` TINYINT UNSIGNED NOT NULL AFTER `HomeTimeOfPossession`,
 ADD COLUMN `HomeRedZoneTDs` TINYINT UNSIGNED NOT NULL AFTER `HomeRedZoneAttempts`,
 ADD COLUMN `HomeRedZoneFGs` TINYINT UNSIGNED NOT NULL AFTER `HomeRedZoneTDs`,
 ADD COLUMN `AwayTimeOfPossession` SMALLINT UNSIGNED NOT NULL AFTER `HomeRedZoneFGs`,
 ADD COLUMN `AwayRedZoneAttempts` TINYINT UNSIGNED NOT NULL AFTER `AwayTimeOfPossession`,
 ADD COLUMN `AwayRedZoneTDs` TINYINT UNSIGNED NOT NULL AFTER `AwayRedZoneAttempts`,
 ADD COLUMN `AwayRedZoneFGs` TINYINT UNSIGNED NOT NULL AFTER `AwayRedZoneTDs`,
 ADD COLUMN `TotalCapacity` INTEGER UNSIGNED NOT NULL AFTER `AwayRedZoneFGs`,
 ADD COLUMN `UpperDeckAttendance` INTEGER UNSIGNED NOT NULL AFTER `TotalCapacity`,
 ADD COLUMN `UpperDeckCapacity` INTEGER UNSIGNED NOT NULL AFTER `UpperDeckAttendance`,
 ADD COLUMN `EndZoneAttendance` INTEGER UNSIGNED NOT NULL AFTER `UpperDeckCapacity`,
 ADD COLUMN `EndZoneCapacity` INTEGER UNSIGNED NOT NULL AFTER `EndZoneAttendance`,
 ADD COLUMN `MezzanineAttendance` INTEGER UNSIGNED NOT NULL AFTER `EndZoneCapacity`,
 ADD COLUMN `MezzanineCapacity` INTEGER UNSIGNED NOT NULL AFTER `MezzanineAttendance`,
 ADD COLUMN `SidelineAttendance` INTEGER UNSIGNED NOT NULL AFTER `MezzanineCapacity`,
 ADD COLUMN `SidelineCapacity` INTEGER UNSIGNED NOT NULL AFTER `SidelineAttendance`,
 ADD COLUMN `ClubAttendance` INTEGER UNSIGNED NOT NULL AFTER `SidelineCapacity`,
 ADD COLUMN `ClubCapacity` INTEGER UNSIGNED NOT NULL AFTER `ClubAttendance`,
 ADD COLUMN `BoxAttendance` INTEGER UNSIGNED NOT NULL AFTER `ClubCapacity`,
 ADD COLUMN `BoxCapacity` INTEGER UNSIGNED NOT NULL AFTER `BoxAttendance`,
 MODIFY COLUMN `HomePassYards` SMALLINT(5) NOT NULL,
 MODIFY COLUMN `AwayRushYards` SMALLINT(5) NOT NULL,
 MODIFY COLUMN `HomeRushYards` SMALLINT(5) NOT NULL,
 MODIFY COLUMN `AwayReceivingYards` SMALLINT(5) NOT NULL,
 MODIFY COLUMN `HomeReceivingYards` SMALLINT(5) NOT NULL;

DROP TABLE IF EXISTS `fof_gamedrive`;
CREATE TABLE  `fof_gamedrive` 
	(
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,

	  `GameID` int(10) unsigned NOT NULL,

 	 `Team` tinyint(3) unsigned NOT NULL,

	 `StartQuarter` tinyint(3) unsigned NOT NULL,

	 `StartMinutes` tinyint(3) unsigned NOT NULL,

	 `StartSeconds` tinyint(3) unsigned NOT NULL,

	 `EndQuarter` tinyint(3) unsigned NOT NULL,

	 `EndMinutes` tinyint(3) unsigned NOT NULL,

	 `EndSeconds` tinyint(3) unsigned NOT NULL,

	 `StartYardsFromGoal` tinyint(3) unsigned NOT NULL,

	 `PlayCount` tinyint(3) unsigned NOT NULL,

	 `YardsGained` smallint(6) NOT NULL,

	 `Result` tinyint(3) unsigned NOT NULL,

	 PRIMARY KEY (`ID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

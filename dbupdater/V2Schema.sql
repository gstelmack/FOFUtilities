ALTER TABLE `fof_mappings` ADD COLUMN `StadiumType` VARCHAR(25) NOT NULL AFTER `Ability`,
 ADD COLUMN `ConstructionType` VARCHAR(15) NOT NULL AFTER `StadiumType`;

CREATE TABLE `fof_teamstadium` (
  `TeamID` smallint(5) NOT NULL default '0',
  `StadiumType` tinyint(3) unsigned NOT NULL,
  `YearStadiumBuilt` smallint(5) unsigned NOT NULL,
  `TotalCapacity` int(10) unsigned NOT NULL,
  `LuxuryBoxes` smallint(5) unsigned NOT NULL,
  `ClubSeats` int(10) unsigned NOT NULL,
  `FanLoyalty` tinyint(3) unsigned NOT NULL,
  `PublicSupport` tinyint(3) unsigned NOT NULL,
  `UpperDeckPrice` smallint(5) unsigned NOT NULL,
  `EndZonePrice` smallint(5) unsigned NOT NULL,
  `MezzaninePrice` smallint(5) unsigned NOT NULL,
  `SidelinesPrice` smallint(5) unsigned NOT NULL,
  `ClubSeatsPrice` smallint(5) unsigned NOT NULL,
  `LuxuryBoxPrice` int(10) unsigned NOT NULL,
  `ConstructionCompleteYear` smallint(5) unsigned NOT NULL,
  `ConstructionType` tinyint(3) unsigned NOT NULL,
  `ConstructionCapacity` int(10) unsigned NOT NULL,
  `ConstructionLuxuryBoxes` smallint(5) unsigned NOT NULL,
  `ConstructionClubSeats` int(10) unsigned NOT NULL,
  `ConstructionStadiumType` tinyint(3) unsigned NOT NULL,
  `PriorYearAttendance` int(10) unsigned NOT NULL,
  PRIMARY KEY (`TeamID`)
) ENGINE = MyISAM DEFAULT CHARSET=latin1;

ALTER TABLE `fof_gameresults` MODIFY COLUMN `AwayPassingLeaderPlayerID` INT(10) UNSIGNED NOT NULL,
 MODIFY COLUMN `HomePassingLeaderPlayerID` INT(10) UNSIGNED NOT NULL,
 MODIFY COLUMN `AwayRushingLeaderPlayerID` INT(10) UNSIGNED NOT NULL,
 MODIFY COLUMN `HomeRushingLeaderPlayerID` INT(10) UNSIGNED NOT NULL,
 MODIFY COLUMN `AwayReceivingLeaderPlayerID` INT(10) UNSIGNED NOT NULL,
 MODIFY COLUMN `HomeReceivingLeaderPlayerID` INT(10) UNSIGNED NOT NULL;
 
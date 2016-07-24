DROP TABLE IF EXISTS `fof_futuredrafts`;
CREATE TABLE  `fof_futuredrafts` (
  `ID` int(10) unsigned NOT NULL auto_increment,
  `Year` smallint(5) unsigned NOT NULL,
  `Round` smallint(5) unsigned NOT NULL,
  `Pick` smallint(5) unsigned NOT NULL,
  `TeamID` smallint(5) unsigned NOT NULL,
  PRIMARY KEY  (`ID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

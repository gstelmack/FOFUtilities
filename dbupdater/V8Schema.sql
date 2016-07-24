﻿ALTER TABLE `fof_playergamestats` DROP COLUMN  `AverageFieldPositionAfterKickoff`,
ADD COLUMN `TotalFieldPositionAfterKickoff` smallint(5) unsigned NOT NULL AFTER `KickoffTouchbacks`,
ADD COLUMN `OffensivePassPlays` tinyint(3) unsigned NOT NULL AFTER `TotalFieldPositionAfterKickoff`,
ADD COLUMN `OffensiveRunPlays` tinyint(3) unsigned NOT NULL AFTER `OffensivePassPlays`,
ADD COLUMN `DefensivePassPlays` tinyint(3) unsigned NOT NULL AFTER `OffensiveRunPlays`,
ADD COLUMN `DefensiveRunPlays` tinyint(3) unsigned NOT NULL AFTER `DefensivePassPlays`,
ADD COLUMN `SuccessfulPasses` tinyint(3) unsigned NOT NULL AFTER `DefensiveRunPlays`,
ADD COLUMN `SuccessfulCatches` tinyint(3) unsigned NOT NULL AFTER `SuccessfulPasses`,
ADD COLUMN `SuccessfulRuns` tinyint(3) unsigned NOT NULL AFTER `SuccessfulCatches`,
ADD COLUMN `BadPassesCaught` tinyint(3) unsigned NOT NULL AFTER `SuccessfulRuns`;
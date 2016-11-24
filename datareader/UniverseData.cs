using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataReader
{
	public class UniverseData
	{
		public UniverseData()
		{
			mSaveDirectory = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Solecismic Software\\Front Office Football Eight");
			mExportDirectory = Path.Combine(mSaveDirectory, "leaguedata");

			if (mSaveDirectory != null && Directory.Exists(mSaveDirectory))
			{
				string universeDirectory = Path.Combine(mSaveDirectory, "universe");
				ProcessSaveDirectory(universeDirectory,"SP - ");
				string leagueDirectory = Path.Combine(mSaveDirectory, "leagues");
				ProcessSaveDirectory(leagueDirectory, "MP - ");
			}
		}
		private void ProcessSaveDirectory(string leagueDirectory, string prefix)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(leagueDirectory);
			DirectoryInfo[] dirInfoArray;

			try
			{
				dirInfoArray = dirInfo.GetDirectories();
			}
			catch
			{
				return;
			}

			for (int i = 0; i < dirInfoArray.Length; i++)
			{
				SavedGameEntry newEntry = new SavedGameEntry();
				newEntry.GameID = dirInfoArray[i].Name;
				newEntry.PathPrefix = Path.Combine(leagueDirectory, newEntry.GameID);
				newEntry.GameName = prefix + newEntry.GameID;
				mSavedGames.Add(newEntry);
			}
		}

		public class SavedGameEntry
		{
			public string GameID;
			public string GameName;
			public string PathPrefix;
		}
		private List<SavedGameEntry> mSavedGames = new List<SavedGameEntry>();
		public List<SavedGameEntry> SavedGames { get { return mSavedGames; } }

		private string mSaveDirectory;
		public string SaveDirectory { get { return mSaveDirectory; } }

		private string mExportDirectory;
		public string ExportDirectory { get { return mExportDirectory;  } }

		static private string[] kPlayoffsMap =
			{
				"None"
				,"Wildcard Round"
				,"Divisional Round"
				,"Conference Round"
				,"Conference Champion"
				,"League Champion"
			};
		public string[] PlayoffsMap { get { return kPlayoffsMap; } }

		static private string[] kPlayerStatusMap =
			{
				"Active"
				,"Injured Reserve"
				,"Inactive"
				,"Suspended"
			};
		public string[] PlayerStatusMap { get { return kPlayerStatusMap; } }

		static private string[] kPositionGroupMap =
			{
				"NA"
				,"QB"
				,"RB"
				,"FB"
				,"TE"
				,"WR"
				,"C"
				,"G"
				,"T"
				,"P"
				,"K"
				,"DE"
				,"DT"
				,"ILB"
				,"OLB"
				,"CB"
				,"S"
				,"LS"
			};
		public string[] PositionGroupMap { get { return kPositionGroupMap; } }

		static private string[] kPositionMap =
			{
				"NA"
				,"QB"
				,"RB"
				,"FB"
				,"TE"
				,"FL"
				,"SE"
				,"LT"
				,"LG"
				,"C"
				,"RG"
				,"RT"
				,"P"
				,"K"
				,"LDE"
				,"LDT"
				,"NT"
				,"RDT"
				,"RDE"
				,"SLB"
				,"SILB"
				,"MLB"
				,"WILB"
				,"WLB"
				,"LCB"
				,"RCB"
				,"SS"
				,"FS"
				,"LS"
			};
		public string[] PositionMap { get { return kPositionMap; } }

		static private string[] kPrecipMap =
			{
				"Fair"
				,"Rain"
				,"Stormy"
				,"Snow"
			};
		public string[] PrecipMap { get { return kPrecipMap; } }

		static private string[] kTransactionTypeMap =
			{
				"None"
				,"Signed as free agent"
				,"Resigned as an unrestricted free agent"
				,"Released"
				,"Received player in trade from"
				,"Received draft pick in trade from"
				,"Retired"
				,"Designated franchise player"
				,"Signed to a new contract"
				,"Placed on injured reserve"
				,"Suspended for off-field conduct"
				,"Hired as head coach"
				,"Hired as lead scout"
				,"Signed to franchise player contract"
				,"Franchise player released under cap rule"
				,"Signed as an unrestricted free agent from"
				,"Signed a renegotiated contract"
				,"Lost draft pick for cap violation"
				,"Hired as offensive coordinator"
				,"Hired as defensive coordinator"
				,"Has begun a contract holdout"
				,"Has ended his contract holdout"
				,"Will re-enter the draft"
				,"Turned down a contract offer"
				,"Offer withdrawn due to lack of cap room"
				,"Signed his rookie contract"
				,"Changed primary position to"
				,"Sent to play in summer league"
				,"Injury"
				,"Renovation Vote"
				,"Construction Vote"
				,"Franchise Move"
			};
		public string[] TransactionTypeMap { get { return kTransactionTypeMap; } }

		static private string[] kAbilityMap =
			{
				"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Poor"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Fair"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Average"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Very Good"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
				,"Excellent"
			};
		public string[] AbilityMap { get { return kAbilityMap; } }

		static private string[] kStadiumTypeMap = 
			{
				"Outdoor/Grass"
				,"Outdoor/Turf"
				,"Dome/Turf"
				,"Retractable Roof/Grass"
			};
		public string[] StadiumTypeMap { get { return kStadiumTypeMap; } }

		static private string[] kConstructionTypeMap = 
			{
				"None"
				,"Renovation"
				,"New Stadium"
			};
		public string[] ConstructionTypeMap { get { return kConstructionTypeMap; } }

		static private string[] kDriveResultMap = 
			{
				"None",
				"Punt",
				"Downs",
				"Half",
				"Game",
				"Fumble",
				"Interception",
				"Safety",
				"FieldGoal",
				"Touchdown",
				"MissedFG"
			};
		public string[] DriveResultMap { get { return kDriveResultMap; } }

		public string GetGameWeekDescription(int week)
		{
			string result = "";
			if (week <= 5)
			{
				result = "Ex" + week;
			}
			else if (week == 23)
			{
				result = "Wildcard";
			}
			else if (week == 24)
			{
				result = "Divisional";
			}
			else if (week == 25)
			{
				result = "Conference";
			}
			else if (week == 26)
			{
				result = "Championship";
			}
			else
			{
				result = "Wk" + (week - 5);
			}
			return result;
		}
	}
}

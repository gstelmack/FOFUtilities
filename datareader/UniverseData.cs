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
			mFOFDirectory = null;
			string directoryINIPath = Path.Combine(WindowsUtilities.OutputLocation.Get(),"FOFPath.txt");
			if (File.Exists(directoryINIPath))
			{
				using (StreamReader inFile = new StreamReader(directoryINIPath))
				{
					mFOFDirectory = inFile.ReadLine();
					mSaveDirectory = inFile.ReadLine();
					inFile.Close();
				}
			}
			if (mFOFDirectory == null || !Directory.Exists(mFOFDirectory))
			{
				mFOFDirectory = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Solecismic Software\\Front Office Football 2007",
					"Full Path", null);
			}
            if (mFOFDirectory == null)
            {
                mFOFDirectory = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Wow6432Node\\Solecismic Software\\Front Office Football 2007",
                    "Full Path", null);
            }

			if (mSaveDirectory == null)
			{
				mSaveDirectory = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Solecismic Software\\Front Office Football 2007");
			}

			if (mFOFDirectory != null && Directory.Exists(mFOFDirectory) && mSaveDirectory != null && Directory.Exists(mSaveDirectory))
			{
				string universeDirectory = Path.Combine(mSaveDirectory, "universe");
				ProcessUniverseDirectory(universeDirectory);
				string leagueDirectory = Path.Combine(mSaveDirectory, "leagues");
				ProcessLeaguesDirectory(leagueDirectory);

				mCityNamesFileName = Path.Combine(universeDirectory, "frfoot.fcy");
				if (!File.Exists(mCityNamesFileName))
				{
					mCityNamesFileName = Path.Combine(mFOFDirectory, "cities.fdt");
				}
				ReadCityNamesData(mCityNamesFileName);

				mCityGameDataFileName = Path.Combine(mFOFDirectory, "citydata.fdt");
				ReadCityGameData(mCityGameDataFileName);

				string injuriesDataPath = Path.Combine(mFOFDirectory, "injuries.fdt");
				ReadInjuriesData(injuriesDataPath);

				string nicknamesDataPath = Path.Combine(universeDirectory, "frfoot.fni");
				if (!File.Exists(nicknamesDataPath))
				{
					nicknamesDataPath = Path.Combine(mFOFDirectory, "nicks.fdt");
				}
				ReadNickNamesData(nicknamesDataPath);

				string collegeNamesDLLPath = Path.Combine(mFOFDirectory, "cnames.dll");
				ReadCollegeNames(collegeNamesDLLPath);

				string hometownNamesTxtPath = Path.Combine(mFOFDirectory, "city.txt");
				ReadHometownNames(hometownNamesTxtPath);
			}
		}

		private void ProcessUniverseDirectory(string universeDirectory)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(universeDirectory);
			FileInfo[] fileInfoArray;

			try
			{
				fileInfoArray = dirInfo.GetFiles("*.fju");
			}
			catch
			{
				return;
			}

			for (int i = 0; i < fileInfoArray.Length; i++)
			{
				SavedGameEntry newEntry = new SavedGameEntry();
				string gameName = Path.GetFileNameWithoutExtension(fileInfoArray[i].Name);
				newEntry.PathPrefix = Path.Combine(universeDirectory, gameName);
				newEntry.GameName = "SP - " + gameName;
				mSavedGames.Add(newEntry);
			}
		}

		private void ProcessLeaguesDirectory(string leagueDirectory)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(leagueDirectory);
			FileInfo[] fileInfoArray;

			try
			{
				fileInfoArray = dirInfo.GetFiles("*.fju");
			}
			catch
			{
				return;
			}

			for (int i = 0; i < fileInfoArray.Length; i++)
			{
				SavedGameEntry newEntry = new SavedGameEntry();
				string gameName = Path.GetFileNameWithoutExtension(fileInfoArray[i].Name);
				newEntry.PathPrefix = Path.Combine(leagueDirectory, gameName);
				newEntry.GameName = "MP - " + gameName;
				mSavedGames.Add(newEntry);
			}
		}

		private void ReadHometownNames(string txtPath)
		{
			StreamReader inFile = new StreamReader(txtPath);

			mHometownNames = new List<string>();
			mHometownNames.Add("NONE");

			string curLine = inFile.ReadLine();
			while (!inFile.EndOfStream)
			{
				if (curLine.Length > 20 && curLine.Length < 60)
				{
					string cityName = curLine.Substring(8, 24).Trim();
					string stateName = curLine.Substring(32).Trim();
					mHometownNames.Add(cityName + ", " + stateName);
				}
				curLine = inFile.ReadLine();
			}

			inFile.Close();
		}

		private void ReadCollegeNames(string dllPath)
		{
			IntPtr dllHandle = WindowsUtilities.Resources.LoadLibrary(dllPath);

			mCollegeNames = new string[688];
			mCollegeNames[0] = "NONE";
			for (uint i = 1; i < mCollegeNames.Length; ++i)
			{
				mCollegeNames[i] = WindowsUtilities.Resources.LoadResourceString(dllHandle, i);
			}

			WindowsUtilities.Resources.FreeLibrary(dllHandle);
		}

		private void ReadCityNamesData(string filePath)
		{
			BinaryHelper.SetupTracer(filePath);
			BinaryHelper.TracerWriteLine("Reading City Names Data File '" + filePath + "'");

			FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			BinaryReader inFile = new BinaryReader(inStream, windows1252Encoding);

			string header = BinaryHelper.ExtractString(inFile, 16, "Header");
			if (header != "CitynaDataFile06")
			{
				System.Windows.Forms.MessageBox.Show("'" + filePath + "' has a header of '" + header + "' which is not a valid City Name Data header.",
					"Bad City Name Data File", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			}
			else
			{
				short cityCount = BinaryHelper.ReadInt16(inFile, "CityCount");
				BinaryHelper.TracerIndent();
				CityRecords = new CityRecord[cityCount];
				for (int i = 0; i < cityCount; i++)
				{
					BinaryHelper.TracerWriteLine("City = " + i);
					BinaryHelper.TracerIndent();
					short strLen = BinaryHelper.ReadInt16(inFile,"NameLength");
					CityRecords[i] = new CityRecord();
					CityRecords[i].Name = BinaryHelper.ExtractString(inFile, strLen,"Name");
					CityRecords[i].Abbrev = BinaryHelper.ExtractString(inFile, 3,"Abbrev");
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();
			}

			inFile.Close();

			BinaryHelper.TracerWriteLine("Done");
			BinaryHelper.ClearTracer();
		}

		private void ReadCityGameData(string filePath)
		{
			BinaryHelper.SetupTracer(filePath);
			BinaryHelper.TracerWriteLine("Reading City Game Data File '" + filePath + "'");

			FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			BinaryReader inFile = new BinaryReader(inStream, windows1252Encoding);

			string header = BinaryHelper.ExtractString(inFile, 16, "Header");
			if (header != "CitiesDataFile06")
			{
				System.Windows.Forms.MessageBox.Show("'" + filePath + "' has a header of '" + header + "' which is not a valid City Game Data header.",
					"Bad City Game Data File", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			}
			else
			{
				short cityCount = BinaryHelper.ReadInt16(inFile, "CityCount");
				System.Diagnostics.Debug.Assert(cityCount == CityRecords.Length);

				BinaryHelper.TracerIndent();
				for (int i = 0; i < cityCount; i++)
				{
					BinaryHelper.TracerWriteLine("City = " + i);
					BinaryHelper.TracerIndent();
					CityRecords[i].Population = BinaryHelper.ReadInt16(inFile, "Population");
					CityRecords[i].GrowthRate = BinaryHelper.ReadInt16(inFile, "Growth Rate");
					CityRecords[i].AverageIncome = BinaryHelper.ReadInt16(inFile, "Avg Income");
					CityRecords[i].PovertyLevel = BinaryHelper.ReadInt16(inFile, "Poverty Level");
					CityRecords[i].EntertainmentCompetiton = BinaryHelper.ReadInt16(inFile, "Entertainment Competition");
					CityRecords[i].SeptemberHigh = BinaryHelper.ReadInt16(inFile,"September High");
					CityRecords[i].SeptemberLow = BinaryHelper.ReadInt16(inFile,"September Low");
					CityRecords[i].SeptemberHumidity = BinaryHelper.ReadInt16(inFile,"September Humidity");
					CityRecords[i].DecemberHigh = BinaryHelper.ReadInt16(inFile,"December High");
					CityRecords[i].DecemberLow = BinaryHelper.ReadInt16(inFile,"December Low");
					CityRecords[i].DecemberHumidity = BinaryHelper.ReadInt16(inFile,"December Humidity");
					CityRecords[i].NinetyDegreeDays = BinaryHelper.ReadInt16(inFile,"90 Degree Days");
					CityRecords[i].SnowDays = BinaryHelper.ReadInt16(inFile,"Snow Days");
					CityRecords[i].StormyDays = BinaryHelper.ReadInt16(inFile,"Stormy Days");
					CityRecords[i].Elevation = BinaryHelper.ReadInt16(inFile, "Elevation");
					CityRecords[i].Longitude = BinaryHelper.ReadInt16(inFile, "Longitude");
					CityRecords[i].Latitude = BinaryHelper.ReadInt16(inFile, "Latitude");
					CityRecords[i].HasTeam = BinaryHelper.ReadInt16(inFile, "Has Team");
					CityRecords[i].Region = BinaryHelper.ReadInt16(inFile, "Region");
					CityRecords[i].State = BinaryHelper.ReadInt16(inFile, "State");
					CityRecords[i].WantsNewTeam = 0;
					CityRecords[i].TrendSetting = 0;
					BinaryHelper.ProbeBytes(inFile, 2);
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();
			}

			inFile.Close();

			BinaryHelper.TracerWriteLine("Done");
			BinaryHelper.ClearTracer();
		}

		public void MakeCityOrgFiles()
		{
			string backupPath = mCityNamesFileName + ".org";
			File.Copy(mCityNamesFileName, backupPath, true);

			backupPath = mCityGameDataFileName + ".org";
			File.Copy(mCityGameDataFileName, backupPath, true);
		}

		private class CityRecordComparer : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				return String.Compare(((CityRecord)x).Name, ((CityRecord)y).Name);
			}
		}

		public void SaveCityData(bool sortCities)
		{
			mCityNamesFileName = Path.Combine(Path.Combine(mFOFDirectory, "universe"),"frfoot.fcy");

			string backupPath;
			if (File.Exists(mCityNamesFileName))
			{
				backupPath = mCityNamesFileName + ".bak";
				File.Copy(mCityNamesFileName, backupPath, true);
			}

			backupPath = mCityGameDataFileName + ".bak";
			File.Copy(mCityGameDataFileName, backupPath, true);

			// Put them in alpha order
			if (sortCities)
			{
				Array.Sort(CityRecords, new CityRecordComparer());
			}

			Encoding windows1252Encoding = Encoding.GetEncoding(1252);

			FileStream nameStream = new FileStream(mCityNamesFileName, FileMode.Create);
			BinaryWriter nameFile = new BinaryWriter(nameStream, windows1252Encoding);
			FileStream dataStream = new FileStream(mCityGameDataFileName, FileMode.Create);
			BinaryWriter dataFile = new BinaryWriter(dataStream, windows1252Encoding);

			BinaryHelper.WriteString(nameFile,"CitynaDataFile06");
			nameFile.Write((short)CityRecords.Length);
			BinaryHelper.WriteString(dataFile,"CitiesDataFile06");
			dataFile.Write((short)CityRecords.Length);
			foreach (CityRecord curRecord in CityRecords)
			{
				nameFile.Write((short)curRecord.Name.Length);
				BinaryHelper.WriteString(nameFile, curRecord.Name);
				BinaryHelper.WriteString(nameFile, curRecord.Abbrev);

				dataFile.Write(curRecord.Population);
				dataFile.Write(curRecord.GrowthRate);
				dataFile.Write(curRecord.AverageIncome);
				dataFile.Write(curRecord.PovertyLevel);
				dataFile.Write(curRecord.EntertainmentCompetiton);
				dataFile.Write(curRecord.SeptemberHigh);
				dataFile.Write(curRecord.SeptemberLow);
				dataFile.Write(curRecord.SeptemberHumidity);
				dataFile.Write(curRecord.DecemberHigh);
				dataFile.Write(curRecord.DecemberLow);
				dataFile.Write(curRecord.DecemberHumidity);
				dataFile.Write(curRecord.NinetyDegreeDays);
				dataFile.Write(curRecord.SnowDays);
				dataFile.Write(curRecord.StormyDays);
				dataFile.Write(curRecord.Elevation);
				dataFile.Write(curRecord.Longitude);
				dataFile.Write(curRecord.Latitude);
				dataFile.Write(curRecord.HasTeam);
				dataFile.Write(curRecord.Region);
				dataFile.Write(curRecord.State);
				dataFile.Write((short)0);
			}

			dataFile.Close();
			nameFile.Close();
		}

		private void ReadNickNamesData(string filePath)
		{
			BinaryHelper.SetupTracer(filePath);
			BinaryHelper.TracerWriteLine("Reading Nicknames Data File '" + filePath + "'");

			FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			BinaryReader inFile = new BinaryReader(inStream, windows1252Encoding);

			string header = BinaryHelper.ExtractString(inFile, 16,"Header");
			if (header != "NicknaDataFile06")
			{
				System.Windows.Forms.MessageBox.Show("'" + filePath + "' has a header of '" + header + "' which is not a valid Nicknames Data header.",
					"Bad Nicknames Data File", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			}
			else
			{
				BinaryHelper.TracerIndent();
				for (int i = 0; i < kTeamCount; i++)
				{
					BinaryHelper.TracerWriteLine("Team " + i);
					BinaryHelper.TracerIndent();
					short strLen = inFile.ReadInt16();
					mTeamRecords[i] = new TeamRecord();
					mTeamRecords[i].Name = BinaryHelper.ExtractString(inFile, strLen,"Name");
					mTeamRecords[i].ConferenceID = BinaryHelper.ReadInt16(inFile, "Conference");
					mTeamRecords[i].DivisionID = BinaryHelper.ReadInt16(inFile, "Division");
					mTeamRecords[i].Year = BinaryHelper.ReadInt16(inFile, "Year");
					mTeamRecords[i].CityIndex = BinaryHelper.ReadInt16(inFile,"CityIndex");
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();
			}
			inFile.Close();

			BinaryHelper.TracerWriteLine("Done");
			BinaryHelper.ClearTracer();
		}

		private void ReadInjuriesData(string filePath)
		{
			BinaryHelper.SetupTracer(filePath);
			BinaryHelper.TracerWriteLine("Reading Injuries Data File '" + filePath + "'");

			FileStream inStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			BinaryReader inFile = new BinaryReader(inStream, windows1252Encoding);

			string header = BinaryHelper.ExtractString(inFile, 16,"Header");
			if (header != "InjureDataFile06")
			{
				System.Windows.Forms.MessageBox.Show("'" + filePath + "' has a header of '" + header + "' which is not a valid Injuries Data header.",
					"Bad Injruies Data File", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			}
			else
			{
				short injuryCount = (short)(BinaryHelper.ReadInt16(inFile,"Injury Count")+1);
				mInjuryRecords = new InjuryRecord[injuryCount];
				mInjuryRecords[0] = new InjuryRecord();
				mInjuryRecords[0].Name = "Healthy";
				BinaryHelper.TracerIndent();
				for (int i = 1; i < injuryCount; i++)
				{
					BinaryHelper.TracerWriteLine("Injury " + i);
					BinaryHelper.TracerIndent();
					mInjuryRecords[i] = new InjuryRecord();
					short strLen = BinaryHelper.ReadInt16(inFile,"Name Length");
					mInjuryRecords[i].Name = BinaryHelper.ExtractString(inFile, strLen,"Name");
					BinaryHelper.ProbeBytes(inFile, 18);
					BinaryHelper.TracerOutdent();
				}
				BinaryHelper.TracerOutdent();
			}

			inFile.Close();

			BinaryHelper.TracerWriteLine("Done");
			BinaryHelper.ClearTracer();
		}

		public class InjuryRecord
		{
			public string Name;
		}
		private InjuryRecord[] mInjuryRecords;
		public InjuryRecord[] InjuryRecords { get { return mInjuryRecords; } }

		public class SavedGameEntry
		{
			public string GameName;
			public string PathPrefix;
		}
		private List<SavedGameEntry> mSavedGames = new List<SavedGameEntry>();
		public List<SavedGameEntry> SavedGames { get { return mSavedGames; } }

		public class CityRecord
		{
			public string Name;
			public string Abbrev;
			public short Population;
			public short GrowthRate;
			public short AverageIncome;
			public short PovertyLevel;
			public short EntertainmentCompetiton;
			public short SeptemberHigh;
			public short SeptemberLow;
			public short SeptemberHumidity;
			public short DecemberHigh;
			public short DecemberLow;
			public short DecemberHumidity;
			public short NinetyDegreeDays;
			public short SnowDays;
			public short StormyDays;
			public short Elevation;
			public short Longitude;
			public short Latitude;
			public short HasTeam;
			public short Region;
			public short State;
			public short TrendSetting;
			public short WantsNewTeam;
		}
		public CityRecord[] CityRecords;

		private const int kTeamCount = 32;
		public class TeamRecord
		{
			public string Name;
			public short CityIndex;
			public short ConferenceID;
			public short DivisionID;
			public short Year;
		}
		private TeamRecord[] mTeamRecords = new TeamRecord[kTeamCount];
		public TeamRecord[] TeamRecords { get { return mTeamRecords; } }

		public string TeamCityName(int teamIndex) { return CityRecords[mTeamRecords[teamIndex].CityIndex].Name; }
		public string TeamCityAbbrev(int teamIndex) { return CityRecords[mTeamRecords[teamIndex].CityIndex].Abbrev; }
		public int TeamIndexByCityName(string cityName)
		{
			for (int i = 0; i < kTeamCount; i++)
			{
				if (TeamCityName(i) == cityName)
				{
					return i;
				}
			}
			return -1;
		}
		public int TeamIndexByCityAbbrev(string cityAbbrev)
		{
			for (int i = 0; i < kTeamCount; i++)
			{
				if (TeamCityAbbrev(i) == cityAbbrev)
				{
					return i;
				}
			}
			return -1;
		}

		private string mFOFDirectory;
		public string FOFDirectory { get { return mFOFDirectory; } }

		private string mSaveDirectory;
		public string SaveDirectory { get { return mSaveDirectory; } }

		private string[] mCollegeNames;
		public string[] CollegeNames { get { return mCollegeNames; } }

		private string mCityNamesFileName;
		private string mCityGameDataFileName;

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

		static private string[] kStaffRoleMap =
			{
				"Head Coach"
				,"Off Coord"
				,"Def Coord"
				,"None"
			};
		public string[] StaffRoleMap { get { return kStaffRoleMap; } }

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

		private List<string> mHometownNames;
		public List<string> HometownNames { get { return mHometownNames; } }

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

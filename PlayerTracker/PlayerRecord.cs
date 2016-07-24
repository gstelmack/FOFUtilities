using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerTracker
{
	[Serializable]
	public class PlayerEntryRecord
	{
		public ushort StageIndex = UInt16.MaxValue;
		public string Position = "";
		public string Position_Group = "";
		public ushort Weight = UInt16.MaxValue;
		public byte Experience = Byte.MaxValue;
		public byte Team = Byte.MaxValue;
		public byte Loyalty = Byte.MaxValue;
		public byte Play_for_Winner = Byte.MaxValue;
		public byte Personality_Strength = Byte.MaxValue;
		public byte Leadership = Byte.MaxValue;
		public byte Intelligence = Byte.MaxValue;
		public byte Mentor = Byte.MaxValue;
		public byte Volatility = Byte.MaxValue;
		public byte Popularity = Byte.MaxValue;
		public byte[] CurBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
		public byte[] FutBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
		public byte CurOverall = Byte.MaxValue;
		public byte FutOverall = Byte.MaxValue;

		public void Write(System.IO.BinaryWriter outFile)
		{
			outFile.Write(StageIndex);
			outFile.Write(Position);
			outFile.Write(Position_Group);
			outFile.Write(Weight);
			outFile.Write(Experience);
			outFile.Write(Team);
			outFile.Write(Loyalty);
			outFile.Write(Play_for_Winner);
			outFile.Write(Personality_Strength);
			outFile.Write(Leadership);
			outFile.Write(Intelligence);
			outFile.Write(Mentor);
			outFile.Write(Volatility);
			outFile.Write(Popularity);
			outFile.Write(CurBars);
			outFile.Write(FutBars);
			outFile.Write(CurOverall);
			outFile.Write(FutOverall);
		}

		public void Read(System.IO.BinaryReader inFile, ushort version)
		{
			StageIndex = inFile.ReadUInt16();
			Position = inFile.ReadString();
			Position_Group = inFile.ReadString();
			Weight = inFile.ReadUInt16();
			Experience = inFile.ReadByte();
			Team = inFile.ReadByte();
			Loyalty = inFile.ReadByte();
			Play_for_Winner = inFile.ReadByte();
			Personality_Strength = inFile.ReadByte();
			Leadership = inFile.ReadByte();
			Intelligence = inFile.ReadByte();
			Mentor = inFile.ReadByte();
			Volatility = inFile.ReadByte();
			Popularity = inFile.ReadByte();
			CurBars = inFile.ReadBytes((int)DataReader.FOFData.ScoutBars.Count);
			FutBars = inFile.ReadBytes((int)DataReader.FOFData.ScoutBars.Count);
			CurOverall = inFile.ReadByte();
			FutOverall = inFile.ReadByte();
		}
	}

	[Serializable]
	public class PlayerRecord
	{
		public uint Player_ID = UInt32.MaxValue;
		public string Last_Name = "";
		public string First_Name = "";
		public byte Junior_Flag = Byte.MaxValue;
		public ushort Player_of_Game_Count = UInt16.MaxValue;
		public byte Championship_Rings = Byte.MaxValue;
		public ushort Player_of_the_Week_Count = UInt16.MaxValue;
		public ushort Player_of_the_Week_Win = UInt16.MaxValue;
		public byte Height = Byte.MaxValue;
		public ushort Weight = UInt16.MaxValue;	// Used only to copy it into the current entry
		public byte Hall_of_Fame_Flag = Byte.MaxValue;
		public ushort Year_Inducted = UInt16.MaxValue;
		public byte Percentage_of_Vote = Byte.MaxValue;
		public byte Chosen_Team = Byte.MaxValue;
		public ushort Year_Born = UInt16.MaxValue;
		public byte Month_Born = Byte.MaxValue;
		public byte Day_Born = Byte.MaxValue;
		public byte Draft_Round = Byte.MaxValue;
		public byte Drafted_Position = Byte.MaxValue;
		public byte Drafted_By = Byte.MaxValue;
		public ushort Draft_Year = UInt16.MaxValue;
		public ushort Draft_Class = 0;
		public ushort Fourth_Quarter_Comebacks = UInt16.MaxValue;
		public ushort Quarterback_Wins = UInt16.MaxValue;
		public ushort Quarterback_Losses = UInt16.MaxValue;
		public ushort Quarterback_Ties = UInt16.MaxValue;
		public ushort Career_Games_Played = UInt16.MaxValue;
		public byte Red_Flag = Byte.MaxValue;
		public byte Number_of_Seasons = Byte.MaxValue;
		public ushort Dash = UInt16.MaxValue;
		public byte Solecismic = Byte.MaxValue;
		public byte Strength = Byte.MaxValue;
		public ushort Agility = UInt16.MaxValue;
		public byte Jump = Byte.MaxValue;
		public byte Position_Specific = Byte.MaxValue;
		public byte Developed = Byte.MaxValue;
		public byte Grade = Byte.MaxValue;
		public byte Interviewed = Byte.MaxValue;
		public byte[] DraftLowBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
		public byte[] DraftHighBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
		public List<PlayerEntryRecord> Entries = new List<PlayerEntryRecord>();

		public void Write(System.IO.BinaryWriter outFile)
		{
			outFile.Write(Player_ID);
			outFile.Write(Last_Name);
			outFile.Write(First_Name);
			outFile.Write(Junior_Flag);
			outFile.Write(Player_of_Game_Count);
			outFile.Write(Championship_Rings);
			outFile.Write(Player_of_the_Week_Count);
			outFile.Write(Player_of_the_Week_Win);
			outFile.Write(Height);
			outFile.Write(Hall_of_Fame_Flag);
			outFile.Write(Year_Inducted);
			outFile.Write(Percentage_of_Vote);
			outFile.Write(Chosen_Team);
			outFile.Write(Year_Born);
			outFile.Write(Month_Born);
			outFile.Write(Day_Born);
			outFile.Write(Draft_Round);
			outFile.Write(Drafted_Position);
			outFile.Write(Drafted_By);
			outFile.Write(Draft_Year);
			outFile.Write(Draft_Class);
			outFile.Write(Fourth_Quarter_Comebacks);
			outFile.Write(Quarterback_Wins);
			outFile.Write(Quarterback_Losses);
			outFile.Write(Quarterback_Ties);
			outFile.Write(Career_Games_Played);
			outFile.Write(Red_Flag);
			outFile.Write(Number_of_Seasons);
			outFile.Write(Dash);
			outFile.Write(Solecismic);
			outFile.Write(Strength);
			outFile.Write(Agility);
			outFile.Write(Jump);
			outFile.Write(Position_Specific);
			outFile.Write(Developed);
			outFile.Write(Grade);
			outFile.Write(Interviewed);
			outFile.Write(DraftLowBars);
			outFile.Write(DraftHighBars);
			outFile.Write(Entries.Count);
			foreach(PlayerEntryRecord rec in Entries)
			{
				rec.Write(outFile);
			}
		}

		public void Read(System.IO.BinaryReader inFile, ushort version)
		{
			Entries.Clear();

			Player_ID = inFile.ReadUInt32();
			Last_Name = inFile.ReadString();
			First_Name = inFile.ReadString();
			Junior_Flag = inFile.ReadByte();
			Player_of_Game_Count = inFile.ReadUInt16();
			Championship_Rings = inFile.ReadByte();
			Player_of_the_Week_Count = inFile.ReadUInt16();
			Player_of_the_Week_Win = inFile.ReadUInt16();
			Height = inFile.ReadByte();
			Hall_of_Fame_Flag = inFile.ReadByte();
			Year_Inducted = inFile.ReadUInt16();
			Percentage_of_Vote = inFile.ReadByte();
			Chosen_Team = inFile.ReadByte();
			Year_Born = inFile.ReadUInt16();
			Month_Born = inFile.ReadByte();
			Day_Born = inFile.ReadByte();
			Draft_Round = inFile.ReadByte();
			Drafted_Position = inFile.ReadByte();
			Drafted_By = inFile.ReadByte();
			Draft_Year = inFile.ReadUInt16();
			if (version > 1)
			{
				Draft_Class = inFile.ReadUInt16();
			}
			else
			{
				Draft_Class = Draft_Year;
			}
			Fourth_Quarter_Comebacks = inFile.ReadUInt16();
			Quarterback_Wins = inFile.ReadUInt16();
			Quarterback_Losses = inFile.ReadUInt16();
			Quarterback_Ties = inFile.ReadUInt16();
			Career_Games_Played = inFile.ReadUInt16();
			Red_Flag = inFile.ReadByte();
			Number_of_Seasons = inFile.ReadByte();
			Dash = inFile.ReadUInt16();
			Solecismic = inFile.ReadByte();
			Strength = inFile.ReadByte();
			Agility = inFile.ReadUInt16();
			Jump = inFile.ReadByte();
			Position_Specific = inFile.ReadByte();
			Developed = inFile.ReadByte();
			Grade = inFile.ReadByte();
			Interviewed = inFile.ReadByte();
			DraftLowBars = inFile.ReadBytes((int)DataReader.FOFData.ScoutBars.Count);
			DraftHighBars = inFile.ReadBytes((int)DataReader.FOFData.ScoutBars.Count);
			int entryCount = inFile.ReadInt32();
			for (int entryIndex = 0; entryIndex < entryCount; ++entryIndex)
			{
				PlayerEntryRecord rec = new PlayerEntryRecord();
				rec.Read(inFile, version);
				if (rec.CurOverall != byte.MaxValue)
				{
					Entries.Add(rec);
				}
			}
		}
	}
}

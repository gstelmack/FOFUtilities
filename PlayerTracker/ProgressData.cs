using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerTracker
{
	[Serializable]
	public class StageData
	{
		public ushort Season;
		public string Stage;

		public void Write(System.IO.BinaryWriter outFile)
		{
			outFile.Write(Season);
			outFile.Write(Stage);
		}

		public void Read(System.IO.BinaryReader inFile, ushort version)
		{
			Season = inFile.ReadUInt16();
			Stage = inFile.ReadString();
		}
	}

	[Serializable]
	public class ProgressData
	{
		const ushort kCurrentVersion = 2;
		public Dictionary<uint,PlayerRecord> PlayerRecords = new Dictionary<uint,PlayerRecord>();
		public List<StageData> StageRecords = new List<StageData>();

		public void Write(System.IO.BinaryWriter outFile)
		{
			outFile.Write(kCurrentVersion);
			outFile.Write(PlayerRecords.Count);
			foreach(PlayerRecord playerRec in PlayerRecords.Values)
			{
				playerRec.Write(outFile);
			}
			outFile.Write(StageRecords.Count);
			foreach(StageData stageData in StageRecords)
			{
				stageData.Write(outFile);
			}
		}

		public void Read(System.IO.BinaryReader inFile)
		{
			PlayerRecords.Clear();
			StageRecords.Clear();

			ushort version = inFile.ReadUInt16();
			int playerCount = inFile.ReadInt32();
			for (int playerIndex=0;playerIndex<playerCount;++playerIndex)
			{
				PlayerRecord playerRec = new PlayerRecord();
				playerRec.Read(inFile, version);
				PlayerRecords.Add(playerRec.Player_ID, playerRec);
			}

			int stageCount = inFile.ReadInt32();
			for (int stageIndex=0;stageIndex<stageCount;++stageIndex)
			{
				StageData stageData = new StageData();
				stageData.Read(inFile, version);
				StageRecords.Add(stageData);
			}

			if (version == 1)
			{
				foreach(PlayerRecord rec in PlayerRecords.Values)
				{
					if (rec.Draft_Class == 0 && rec.Entries.Count > 0)
					{
						rec.Draft_Class = StageRecords[rec.Entries[0].StageIndex].Season;
					}
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace ChangeTracker
{
	public partial class ChangeTrackerForm : Form
	{
		private const string kSettingsRoot = "ChangeTracker";
		private const string kDatabasePath = "DatabasePath";
		private const string kImportPath = "ImportPath";
		private WindowsUtilities.XMLSettings mSettings;
		private DataTable mPlayerListData = new DataTable();
		private BindingSource mPlayerListBindingSource = new BindingSource();
		private DataTable mPlayerDetailsData = new DataTable();
		private BindingSource mPlayerDetailsBindingSource = new BindingSource();

		public ChangeTrackerForm()
		{
			InitializeComponent();

			Assembly a = typeof(ChangeTrackerForm).Assembly;
			Text += " v" + a.GetName().Version;

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "ChangeTracker.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);
			buttonDatabase.Text = mSettings.ReadXMLString(kSettingsRoot, kDatabasePath, "");
			buttonImportPath.Text = mSettings.ReadXMLString(kSettingsRoot, kImportPath, "");

			InitializeAttributeMaps();

			DataColumn column;
			// Player List Columns
			column = new DataColumn();
			column.DataType = typeof(String);
			column.ColumnName = "Name";
			mPlayerListData.Columns.Add(column);

			column = new DataColumn();
			column.DataType = typeof(String);
			column.ColumnName = "Pos";
			mPlayerListData.Columns.Add(column);

			column = new DataColumn();
			column.DataType = typeof(Image);
			column.ColumnName = "Growth";
			mPlayerListData.Columns.Add(column);

			// Player detail columns
			column = new DataColumn();
			column.DataType = typeof(String);
			column.ColumnName = "Attribute";
			mPlayerDetailsData.Columns.Add(column);

			column = new DataColumn();
			column.DataType = typeof(Image);
			column.ColumnName = "Growth";
			mPlayerDetailsData.Columns.Add(column);

			UpdateButtonStates();
			UpdateView();
		}

		private void UpdateButtonStates()
		{
			if (buttonDatabase.Text.Length != 0)
			{
				buttonImportPath.Enabled = true;
				ReadDatabase();
			}
			else
			{
				buttonImportPath.Enabled = false;
			}
		}

		private void buttonDatabase_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			if (buttonDatabase.Text.Length > 0)
			{
				dlg.InitialDirectory = Path.GetDirectoryName(buttonDatabase.Text);
			}
			dlg.FileName = buttonDatabase.Text;
			dlg.Filter = "tracker databases (*.trk)|*.trk";
			dlg.CheckPathExists = true;
			dlg.DefaultExt = "trk";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonDatabase.Text = dlg.FileName;
				mSettings.WriteXMLString(kSettingsRoot, kDatabasePath, buttonDatabase.Text);
				UpdateButtonStates();
			}
		}

		private void buttonImportPath_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (buttonImportPath.Text.Length > 0)
			{
				dlg.InitialDirectory = Path.GetDirectoryName(buttonImportPath.Text);
			}
			dlg.FileName = buttonImportPath.Text;
			dlg.Filter = "csv Extractor Files (*.csv)|*.csv";
			dlg.Multiselect = false;
			dlg.CheckFileExists = true;
			dlg.CheckPathExists = true;
			dlg.DefaultExt = "csv";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				Cursor oldCursor = Cursor;
				Cursor = Cursors.WaitCursor;

				buttonImportPath.Text = dlg.FileName;
				mSettings.WriteXMLString(kSettingsRoot, kImportPath, buttonImportPath.Text);

				// Assume everyone is inactive, we'll correct below
				foreach (PlayerHistory history in mPlayerHistories.Values)
				{
					history.mActive = false;
				}

				using (System.IO.StreamReader inFile = new System.IO.StreamReader(buttonImportPath.Text))
				{
					System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

					string headerLine = inFile.ReadLine();
					while (!inFile.EndOfStream)
					{
						PlayerHistory tempHistory = new PlayerHistory();
						PlayerData newData = new PlayerData();

						string curLine = inFile.ReadLine();
						string[] fields = DataReader.CSVHelper.ParseLine(curLine);

						tempHistory.mActive = true;
						tempHistory.mName = fields[0];
						newData.mPosition = fields[1];
						newData.mPositionGroup = fields[2];
						tempHistory.mCollege = fields[3];
						newData.mTeam = fields[4];
						tempHistory.mBirthDate = fields[5];
						tempHistory.mHomeTown = fields[6];
						newData.mHeight = Byte.Parse(fields[9], nfi);
						newData.mWeight = Int16.Parse(fields[10], nfi);
						newData.mExperience = Byte.Parse(fields[11], nfi);
						newData.mVolatility = Byte.Parse(fields[12], nfi);
						newData.mSolecismic = Byte.Parse(fields[21], nfi);
						newData.m40Time = (short)(Double.Parse(fields[22], nfi)*100);
						newData.mBench = Byte.Parse(fields[23], nfi);
						newData.mAgility = (short)(Double.Parse(fields[24], nfi)*100);
						newData.mBroadJump = Byte.Parse(fields[25], nfi);
						newData.mPositionDrill = Byte.Parse(fields[26], nfi);
						newData.mPercentDeveloped = Byte.Parse(fields[27], nfi);
						// Fields 28 & 29 are interviewed fields
						newData.mCurrent = Byte.Parse(fields[30], nfi);
						newData.mFuture = Byte.Parse(fields[31], nfi);
						int attributeStartField = 35;
						newData.mFormations = 0;
						if (newData.mPositionGroup == "QB")
						{
							newData.mFormations = Byte.Parse(fields[attributeStartField]);
							attributeStartField += 1;
						}
						int attributeIndex = 0;
						for (int i = 0; i < newData.mAttributes.Length; ++i)
						{
							newData.mAttributes[i] = 0;
						}
						while (attributeStartField < fields.Length)
						{
							newData.mAttributes[attributeIndex++] = Byte.Parse(fields[attributeStartField++], nfi);
						}
						newData.mAttribCount = (byte)(attributeIndex / 2);

						string curKey = tempHistory.GetKey();
						PlayerHistory playerHistory;
						if (mPlayerHistories.TryGetValue(curKey, out playerHistory))
						{
							playerHistory.mActive = true;
							playerHistory.mGrowthEntries.Add(newData);
							mPlayerHistories[curKey] = playerHistory;
						}
						else
						{
							tempHistory.mGrowthEntries.Add(newData);
							mPlayerHistories.Add(curKey, tempHistory);
						}
					}
					inFile.Close();
					WriteDatabase();
					UpdateView();
				}

				Cursor = oldCursor;
			}
		}

		private void UpdateView()
		{
			mPlayerListData.Clear();
			mPlayersInTable.Clear();

			foreach (PlayerHistory curPlayer in mPlayerHistories.Values)
			{
				if (curPlayer.mActive)
				{
					DataRow newRow = mPlayerListData.NewRow();
					newRow["Name"] = curPlayer.mName;
					newRow["Pos"] = curPlayer.mGrowthEntries.Last<PlayerData>().mPosition;
					int[] ratings = new int[curPlayer.mGrowthEntries.Count * 2];
					for (int i = 0; i < curPlayer.mGrowthEntries.Count; ++i)
					{
						ratings[(i * 2)] = curPlayer.mGrowthEntries[i].mCurrent;
						ratings[(i * 2)+1] = curPlayer.mGrowthEntries[i].mFuture;
					}
					newRow["Growth"] = CreateGrowthImage(ratings);
					mPlayerListData.Rows.Add(newRow);
					mPlayersInTable.Add(curPlayer);
				}
			}

			dataGridViewPlayers.DataSource = mPlayerListBindingSource;
			mPlayerListBindingSource.DataSource = mPlayerListData;
		}

		private Image CreateGrowthImage(int[] ratings)
		{
			int ratingCounts = ratings.Length / 2;
			int barWidth = 20;
			Bitmap growthBitmap = new Bitmap(1 + (ratingCounts * (barWidth+1)), 101);

			Graphics g = Graphics.FromImage(growthBitmap);
			Pen outlinePen = new Pen(Brushes.Black,1);

			for (int i = 0; i < ratingCounts; ++i)
			{
				int leftX = i*(barWidth+1);
				int cur = ratings[i * 2];
				int fut = ratings[(i * 2) + 1];
				if (cur != fut)
				{
					g.FillRectangle(Brushes.LimeGreen, leftX+1, 100-fut, barWidth, 100);
				}
				g.FillRectangle(Brushes.Red, leftX+1, 100-cur, barWidth, 100);
				for (int y = 0; y < 10; ++y)
				{
					g.DrawRectangle(outlinePen, leftX, y * 10, barWidth + 1, 10);
				}
			}

			return growthBitmap;
		}

		private void ReadDatabase()
		{
			if (File.Exists(buttonDatabase.Text))
			{
				IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				using (Stream stream = new FileStream(buttonDatabase.Text, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					mPlayerHistories = (Dictionary<string, PlayerHistory>)formatter.Deserialize(stream);
					stream.Close();
				}
			}
		}

		private void WriteDatabase()
		{
			IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			using (Stream stream = new FileStream(buttonDatabase.Text, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				formatter.Serialize(stream, mPlayerHistories);
				stream.Close();
			}
		}

		private const int kMaxAttributeCounts = 15;

		[Serializable]
		private class PlayerData
		{
			public string mPosition;
			public string mPositionGroup;
			public string mTeam;
			public byte mHeight;
			public short mWeight;
			public byte mExperience;
			public byte mVolatility;
			public byte mSolecismic;
			public short m40Time;
			public byte mBench;
			public short mAgility;
			public byte mBroadJump;
			public byte mPositionDrill;
			public byte mPercentDeveloped;
			public byte mFormations;
			public byte[] mAttributes = new byte[kMaxAttributeCounts * 2];
			public byte mCurrent;
			public byte mFuture;
			public byte mAttribCount;
		}

		[Serializable]
		private class PlayerHistory
		{
			public string mName;
			public string mCollege;
			public string mBirthDate;
			public string mHomeTown;
			public bool mActive;

			public List<PlayerData> mGrowthEntries = new List<PlayerData>();

			public string GetKey()
			{
				return mCollege + mBirthDate + mHomeTown;
			}
		}

		private Dictionary<string, PlayerHistory> mPlayerHistories = new Dictionary<string, PlayerHistory>();
		private List<PlayerHistory> mPlayersInTable = new List<PlayerHistory>();
		private Dictionary<string, string[]> mPositionGroupAttributeNames;

		private void dataGridViewPlayers_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewPlayers.CurrentCell != null)
			{
				int selectedIndex = dataGridViewPlayers.CurrentCell.OwningRow.Index;
				mPlayerDetailsData.Clear();

				PlayerHistory curPlayer = mPlayersInTable[selectedIndex];
				PlayerData lastGrowthEntry = curPlayer.mGrowthEntries.Last<PlayerData>();
				string curPosition = lastGrowthEntry.mPositionGroup;
				List<byte>[] attributes = new List<byte>[lastGrowthEntry.mAttribCount];
				for (int i = 0; i < lastGrowthEntry.mAttribCount; ++i)
				{
					attributes[i] = new List<byte>();
				}

				foreach (PlayerData growthUpdate in curPlayer.mGrowthEntries)
				{
					if (growthUpdate.mPositionGroup == curPosition)
					{
						for (int i = 0; i < growthUpdate.mAttribCount; ++i)
						{
							attributes[i].Add(growthUpdate.mAttributes[i * 2]);
							attributes[i].Add(growthUpdate.mAttributes[(i * 2) + 1]);
						}
					}
				}
				string[] attributeNames = mPositionGroupAttributeNames[curPosition];
				for (int i = 0; i < lastGrowthEntry.mAttribCount; ++i)
				{
					int[] ratings = new int[attributes[i].Count];
					int ratingIndex = 0;
					foreach (byte attrib in attributes[i])
					{
						ratings[ratingIndex++] = attrib;
					}
					DataRow newRow = mPlayerDetailsData.NewRow();
					newRow["Attribute"] = attributeNames[i];
					newRow["Growth"] = CreateGrowthImage(ratings);
					mPlayerDetailsData.Rows.Add(newRow);
				}

				dataGridViewPlayerDetails.DataSource = mPlayerDetailsBindingSource;
				mPlayerDetailsBindingSource.DataSource = mPlayerDetailsData;
			}
		}

		private void InitializeAttributeMaps()
		{
			mPositionGroupAttributeNames = new Dictionary<string, string[]>();

			// QB
			string[] attributeNames = new string[]
			{
				"Screen Passes",
				"Short Passes",
				"Medium Passes",
				"Long Passes",
				"Deep Passes",
				"Third Down Passing",
				"Accuracy",
				"Timing",
				"Sense Rush",
				"Read Defense",
				"Two Minute Offense",
				"Scramble Frequency",
				"Kick Holding"
			};
			mPositionGroupAttributeNames.Add("QB", attributeNames);

			// RB
			attributeNames = new string[]
			{
				"Breakaway Speed",
				"Power Inside",
				"Third Down Running",
				"Hole Recognition",
				"Elusiveness",
				"Speed to Outside",
				"Blitz Pickup",
				"Avoid Drops",
				"Getting Downfield",
				"Route Running",
				"Third Down Catching",
				"Punt Returns",
				"Kick Returns",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("RB", attributeNames);

			// FB
			attributeNames = new string[]
			{
				"Run Blocking",
				"Pass Blocking",
				"Blocking Strength",
				"Power Inside",
				"Third Down Running",
				"Hole Recognition",
				"Blitz Pickup",
				"Avoid Drops",
				"Route Running",
				"Third Down Catching",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("FB", attributeNames);

			// TE
			attributeNames = new string[]
			{
				"Run Blocking",
				"Pass Blocking",
				"Blocking Strength",
				"Avoid Drops",
				"Getting Downfield",
				"Route Running",
				"Third Down Catching",
				"Big Play Receiving",
				"Courage",
				"Adjust to Ball",
				"Endurance",
				"Special Teams",
				"Long Snapping"
			};
			mPositionGroupAttributeNames.Add("TE", attributeNames);

			// WR
			attributeNames = new string[]
			{
				"Avoid Drops",
				"Getting Downfield",
				"Route Running",
				"Third Down Catching",
				"Big Play Receiving",
				"Courage",
				"Adjust to Ball",
				"Punt Returns",
				"Kick Returns",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("WR", attributeNames);

			// C
			attributeNames = new string[]
			{
				"Run Blocking",
				"Pass Blocking",
				"Blocking Strength",
				"Endurance",
				"Long Snapping"
			};
			mPositionGroupAttributeNames.Add("C", attributeNames);

			// G
			attributeNames = new string[]
			{
				"Run Blocking",
				"Pass Blocking",
				"Blocking Strength",
				"Endurance"
			};
			mPositionGroupAttributeNames.Add("G", attributeNames);

			// T
			attributeNames = new string[]
			{
				"Run Blocking",
				"Pass Blocking",
				"Blocking Strength",
				"Endurance"
			};
			mPositionGroupAttributeNames.Add("T", attributeNames);

			// P
			attributeNames = new string[]
			{
				"Kicking Power",
				"Punt Hang Time",
				"Directional Punting",
				"Kick Holding"
			};
			mPositionGroupAttributeNames.Add("P", attributeNames);

			// K
			attributeNames = new string[]
			{
				"Kicking Accuracy",
				"Kicking Power",
				"Kickoff Distance",
				"Kickoff Hang Time"
			};
			mPositionGroupAttributeNames.Add("K", attributeNames);

			// DE
			attributeNames = new string[]
			{
				"Run Defense",
				"Pass Rush Technique",
				"Pass Rush Strength",
				"Play Diagnosis",
				"Punishing Hitter",
				"Endurance"
			};
			mPositionGroupAttributeNames.Add("DE", attributeNames);

			// DT
			attributeNames = new string[]
			{
				"Run Defense",
				"Pass Rush Technique",
				"Pass Rush Strength",
				"Play Diagnosis",
				"Punishing Hitter",
				"Endurance"
			};
			mPositionGroupAttributeNames.Add("DT", attributeNames);

			// ILB
			attributeNames = new string[]
			{
				"Run Defense",
				"Pass Rush Technique",
				"Man-to-Man Defense",
				"Zone Defense",
				"Bump and Run Defense",
				"Pass Rush Strength",
				"Play Diagnosis",
				"Punishing Hitter",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("ILB", attributeNames);

			// OLB
			attributeNames = new string[]
			{
				"Run Defense",
				"Pass Rush Technique",
				"Man-to-Man Defense",
				"Zone Defense",
				"Bump and Run Defense",
				"Pass Rush Strength",
				"Play Diagnosis",
				"Punishing Hitter",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("OLB", attributeNames);

			// CB
			attributeNames = new string[]
			{
				"Run Defense",
				"Man-to-Man Defense",
				"Zone Defense",
				"Bump and Run Defense",
				"Play Diagnosis",
				"Punishing Hitter",
				"Interceptions",
				"Punt Returns",
				"Kick Returns",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("CB", attributeNames);

			// S
			attributeNames = new string[]
			{
				"Run Defense",
				"Man-to-Man Defense",
				"Zone Defense",
				"Bump and Run Defense",
				"Play Diagnosis",
				"Punishing Hitter",
				"Interceptions",
				"Punt Returns",
				"Kick Returns",
				"Endurance",
				"Special Teams"
			};
			mPositionGroupAttributeNames.Add("S", attributeNames);
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DraftAnalyzer
{
	public partial class ChemistryForm : Form
	{
		private string mAppFolderName;

		public ChemistryForm()
		{
			InitializeComponent();

			mAppFolderName = WindowsUtilities.OutputLocation.Get();
			InitializeData();
			LoadData();
			FillScreen();
		}

		private void InitializeData()
		{
			BackfieldLeaderBirthday.Day = 1;
			BackfieldLeaderBirthday.Month = 1;
			mBackfieldWeight = 1.0;

			DefensiveFrontLeaderBirthday.Day = 1;
			DefensiveFrontLeaderBirthday.Month = 1;
			mDefensiveFrontWeight = 1.0;

			OffensiveLineLeaderBirthday.Day = 1;
			OffensiveLineLeaderBirthday.Month = 1;
			mOffensiveLineWeight = 1.0;

			ReceiversLeaderBirthday.Day = 1;
			ReceiversLeaderBirthday.Month = 1;
			mReceiversWeight = 1.0;

			SecondaryLeaderBirthday.Day = 1;
			SecondaryLeaderBirthday.Month = 1;
			mSecondaryWeight = 1.0;
		}

		private void FillScreen()
		{
			numericUpDownBackfieldWeight.Value = (decimal)mBackfieldWeight;
			numericUpDownDefensiveFrontWeight.Value = (decimal)mDefensiveFrontWeight;
			numericUpDownOffensiveLineWeight.Value = (decimal)mOffensiveLineWeight;
			numericUpDownReceiversWeight.Value = (decimal)mReceiversWeight;
			numericUpDownSecondaryWeight.Value = (decimal)mSecondaryWeight;

			numericUpDownBackFieldDay.Value = BackfieldLeaderBirthday.Day;
			numericUpDownBackfieldMonth.Value = BackfieldLeaderBirthday.Month;
			numericUpDownDefensiveFrontDay.Value = DefensiveFrontLeaderBirthday.Day;
			numericUpDownDefensiveFrontMonth.Value = DefensiveFrontLeaderBirthday.Month;
			numericUpDownOffensiveLineDay.Value = OffensiveLineLeaderBirthday.Day;
			numericUpDownOffensiveLineMonth.Value = OffensiveLineLeaderBirthday.Month;
			numericUpDownReceiversDay.Value = ReceiversLeaderBirthday.Day;
			numericUpDownReceiversMonth.Value = ReceiversLeaderBirthday.Month;
			numericUpDownSecondaryDay.Value = SecondaryLeaderBirthday.Day;
			numericUpDownSecondaryMonth.Value = SecondaryLeaderBirthday.Month;
		}

		private void GrabScreen()
		{
			BackfieldLeaderBirthday.Month = (int)numericUpDownBackfieldMonth.Value;
			BackfieldLeaderBirthday.Day = (int)numericUpDownBackFieldDay.Value;
			ReceiversLeaderBirthday.Month = (int)numericUpDownReceiversMonth.Value;
			ReceiversLeaderBirthday.Day = (int)numericUpDownReceiversDay.Value;
			OffensiveLineLeaderBirthday.Month = (int)numericUpDownOffensiveLineMonth.Value;
			OffensiveLineLeaderBirthday.Day = (int)numericUpDownOffensiveLineDay.Value;
			DefensiveFrontLeaderBirthday.Month = (int)numericUpDownDefensiveFrontMonth.Value;
			DefensiveFrontLeaderBirthday.Day = (int)numericUpDownDefensiveFrontDay.Value;
			SecondaryLeaderBirthday.Month = (int)numericUpDownSecondaryMonth.Value;
			SecondaryLeaderBirthday.Day = (int)numericUpDownSecondaryDay.Value;

			mBackfieldWeight = (double)numericUpDownBackfieldWeight.Value;
			mReceiversWeight = (double)numericUpDownReceiversWeight.Value;
			mOffensiveLineWeight = (double)numericUpDownOffensiveLineWeight.Value;
			mDefensiveFrontWeight = (double)numericUpDownDefensiveFrontWeight.Value;
			mSecondaryWeight = (double)numericUpDownSecondaryWeight.Value;
		}

		private const string kChemistryDataFileName = "DraftAnalyzer.chemistry";
		private const int kChemistyVersion = 1;

		private void LoadData()
		{
			string dataFileName = System.IO.Path.Combine(mAppFolderName, kChemistryDataFileName);
			LoadData(dataFileName);
		}

		private void LoadData(string filePath)
		{
			if (System.IO.File.Exists(filePath))
			{
				try
				{
					Encoding windows1252Encoding = Encoding.GetEncoding(1252);
					System.IO.FileStream inStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
					System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

					int version = inFile.ReadInt32();

					BackfieldLeaderBirthday.Day = inFile.ReadInt32();
					BackfieldLeaderBirthday.Month = inFile.ReadInt32();
					mBackfieldWeight = inFile.ReadDouble();

					DefensiveFrontLeaderBirthday.Day = inFile.ReadInt32();
					DefensiveFrontLeaderBirthday.Month = inFile.ReadInt32();
					mDefensiveFrontWeight = inFile.ReadDouble();

					OffensiveLineLeaderBirthday.Day = inFile.ReadInt32();
					OffensiveLineLeaderBirthday.Month = inFile.ReadInt32();
					mOffensiveLineWeight = inFile.ReadDouble();

					ReceiversLeaderBirthday.Day = inFile.ReadInt32();
					ReceiversLeaderBirthday.Month = inFile.ReadInt32();
					mReceiversWeight = inFile.ReadDouble();

					SecondaryLeaderBirthday.Day = inFile.ReadInt32();
					SecondaryLeaderBirthday.Month = inFile.ReadInt32();
					mSecondaryWeight = inFile.ReadDouble();

					inFile.Close();
				}
				catch (System.IO.IOException e)
				{
					MessageBox.Show("Error reading '" + filePath + "': " + e.ToString(), "Error Loading Chemistry File",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void SaveData()
		{
			string dataFileName = System.IO.Path.Combine(mAppFolderName, kChemistryDataFileName);
			SaveData(dataFileName);
		}

		private void SaveData(string filePath)
		{
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);

			System.IO.FileStream outStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
			System.IO.BinaryWriter outFile = new System.IO.BinaryWriter(outStream, windows1252Encoding);

			outFile.Write(kChemistyVersion);

			outFile.Write(BackfieldLeaderBirthday.Day);
			outFile.Write(BackfieldLeaderBirthday.Month);
			outFile.Write(mBackfieldWeight);

			outFile.Write(DefensiveFrontLeaderBirthday.Day);
			outFile.Write(DefensiveFrontLeaderBirthday.Month);
			outFile.Write(mDefensiveFrontWeight);

			outFile.Write(OffensiveLineLeaderBirthday.Day);
			outFile.Write(OffensiveLineLeaderBirthday.Month);
			outFile.Write(mOffensiveLineWeight);

			outFile.Write(ReceiversLeaderBirthday.Day);
			outFile.Write(ReceiversLeaderBirthday.Month);
			outFile.Write(mReceiversWeight);

			outFile.Write(SecondaryLeaderBirthday.Day);
			outFile.Write(SecondaryLeaderBirthday.Month);
			outFile.Write(mSecondaryWeight);

			outFile.Close();
		}

		public DataReader.ChemistryUtilities.Birthday BackfieldLeaderBirthday;
		public DataReader.ChemistryUtilities.Birthday ReceiversLeaderBirthday;
		public DataReader.ChemistryUtilities.Birthday OffensiveLineLeaderBirthday;
		public DataReader.ChemistryUtilities.Birthday DefensiveFrontLeaderBirthday;
		public DataReader.ChemistryUtilities.Birthday SecondaryLeaderBirthday;

		private double mBackfieldWeight = 1.0;
		public double BackfieldWeight
		{
			get
			{
				return mBackfieldWeight;
			}
			set
			{
				mBackfieldWeight = value;
				numericUpDownBackfieldWeight.Value = (decimal)value;
			}
		}
		private double mReceiversWeight = 1.0;
		public double ReceiversWeight
		{
			get
			{
				return mReceiversWeight;
			}
			set
			{
				mReceiversWeight = value;
				numericUpDownReceiversWeight.Value = (decimal)value;
			}
		}
		private double mOffensiveLineWeight = 1.0;
		public double OffensiveLineWeight
		{
			get
			{
				return mOffensiveLineWeight;
			}
			set
			{
				mOffensiveLineWeight = value;
				numericUpDownOffensiveLineWeight.Value = (decimal)value;
			}
		}
		private double mDefensiveFrontWeight = 1.0;
		public double DefensiveFrontWeight
		{
			get
			{
				return mDefensiveFrontWeight;
			}
			set
			{
				mDefensiveFrontWeight = value;
				numericUpDownDefensiveFrontWeight.Value = (decimal)value;
			}
		}
		private double mSecondaryWeight = 1.0;
		public double SecondaryWeight
		{
			get
			{
				return mSecondaryWeight;
			}
			set
			{
				mSecondaryWeight = value;
				numericUpDownSecondaryWeight.Value = (decimal)value;
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			GrabScreen();
			SaveData();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			InitializeData();
			LoadData();
			FillScreen();
		}

		public double GetPositionLeaderWeight(string position)
		{
			if (position == "RB" || position == "FB")
			{
				return mBackfieldWeight;
			}
			else if (position == "FL" || position == "SE" || position == "TE")
			{
				return mReceiversWeight;
			}
			else if (position == "LT" || position == "LG" || position == "C" || position == "RG" || position == "RT")
			{
				return mOffensiveLineWeight;
			}
			else if (position == "LDE" || position == "RDE" || position == "LDT" || position == "RDT" || position == "NT" ||
		   position == "WILB" || position == "MLB" || position == "SILB" || position == "WLB" || position == "SLB")
			{
				return mDefensiveFrontWeight;
			}
			else if (position == "RCB" || position == "LCB" || position == "FS" || position == "SS")
			{
				return mSecondaryWeight;
			}
			else
			{
				return 1.0;
			}
		}

		public DataReader.ChemistryUtilities.Birthday GetLeaderBirthday(string position)
		{
			if (position == "RB" || position == "FB")
			{
				return BackfieldLeaderBirthday;
			}
			else if (position == "FL" || position == "SE" || position == "TE")
			{
				return ReceiversLeaderBirthday;
			}
			else if (position == "LT" || position == "LG" || position == "C" || position == "RG" || position == "RT")
			{
				return OffensiveLineLeaderBirthday;
			}
			else if (position == "LDE" || position == "RDE" || position == "LDT" || position == "RDT" || position == "NT" ||
		   position == "WILB" || position == "MLB" || position == "SILB" || position == "WLB" || position == "SLB")
			{
				return DefensiveFrontLeaderBirthday;
			}
			else if (position == "RCB" || position == "LCB" || position == "FS" || position == "SS")
			{
				return SecondaryLeaderBirthday;
			}
			else
			{
				return BackfieldLeaderBirthday;
			}
		}

		private void buttonSaveAs_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Chemistry Files (*.chemistry)|*.chemistry|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			dlg.InitialDirectory = WindowsUtilities.OutputLocation.Get();
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				GrabScreen();
				SaveData(dlg.FileName);
			}
		}

		private void buttonLoad_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Chemistry Files (*.chemistry)|*.chemistry|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			dlg.InitialDirectory = WindowsUtilities.OutputLocation.Get();
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				LoadData(dlg.FileName);
				FillScreen();
			}
		}
	}
}
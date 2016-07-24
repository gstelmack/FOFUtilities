using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MLTools
{
	public partial class MLForm : Form
	{
		private const string kSettingsRoot = "MLTools";
		private const string kRosterPath = "RosterPath";
		private const string kDraftTemplatePath = "DraftTemplatePath";
		private const string kGrowthDataPath = "GrowthDataPath";

		private WindowsUtilities.XMLSettings mSettings;
		private string mOutputPath;

		public MLForm()
		{
			Assembly a = typeof(MLForm).Assembly;
			Text += " v" + a.GetName().Version;

			InitializeComponent();

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "MLForm.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);
			buttonRosterPath.Text = mSettings.ReadXMLString(kSettingsRoot, kRosterPath, "");
			buttonDraftTemplatePath.Text = mSettings.ReadXMLString(kSettingsRoot, kDraftTemplatePath, "");
			buttonGrowthDataPath.Text = mSettings.ReadXMLString(kSettingsRoot, kGrowthDataPath, "");
		}

		private void buttonRosterPath_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (buttonRosterPath.Text.Length > 0)
			{
				dlg.InitialDirectory = Path.GetDirectoryName(buttonRosterPath.Text);
			}
			dlg.FileName = buttonRosterPath.Text;
			dlg.Filter = "csv files (*.csv)|*.csv";
			dlg.Multiselect = false;
			dlg.CheckFileExists = true;
			dlg.CheckPathExists = true;
			dlg.DefaultExt = "exe";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonRosterPath.Text = dlg.FileName;
				mSettings.WriteXMLString(kSettingsRoot, kRosterPath, buttonRosterPath.Text);
			}
		}

		private void AddOutputFile(ref System.Collections.Generic.Dictionary<string, StreamWriter> outFiles, string posGroup)
		{
			outFiles.Add(posGroup, new StreamWriter(Path.Combine(mOutputPath, "ML_" + posGroup + ".csv"), true));
		}

		private void buttonSplitRoster_Click(object sender, EventArgs e)
		{
			string filename = buttonRosterPath.Text;
			mOutputPath = Path.GetDirectoryName(filename);
			System.Collections.Generic.Dictionary<string, StreamWriter> posGroupOutFiles = new System.Collections.Generic.Dictionary<string, StreamWriter>();
			AddOutputFile(ref posGroupOutFiles, "QB");
			AddOutputFile(ref posGroupOutFiles, "RB");
			AddOutputFile(ref posGroupOutFiles, "FB");
			AddOutputFile(ref posGroupOutFiles, "WR");
			AddOutputFile(ref posGroupOutFiles, "TE");
			AddOutputFile(ref posGroupOutFiles, "T");
			AddOutputFile(ref posGroupOutFiles, "G");
			AddOutputFile(ref posGroupOutFiles, "C");
			AddOutputFile(ref posGroupOutFiles, "DE");
			AddOutputFile(ref posGroupOutFiles, "DT");
			AddOutputFile(ref posGroupOutFiles, "ILB");
			AddOutputFile(ref posGroupOutFiles, "OLB");
			AddOutputFile(ref posGroupOutFiles, "CB");
			AddOutputFile(ref posGroupOutFiles, "S");
			AddOutputFile(ref posGroupOutFiles, "P");
			AddOutputFile(ref posGroupOutFiles, "K");

			try
			{
				using (StreamReader inFile = new StreamReader(filename))
				{
					System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

					string headerLine = inFile.ReadLine();
					bool hasCurrentAndFuture = headerLine.Contains(",Cur,Fut");
					if (hasCurrentAndFuture)
					{
						bool hasChemistry = headerLine.Contains(",Conflicts,Affinities,Character");
						while (!inFile.EndOfStream)
						{
							string curLine = inFile.ReadLine();
							string[] fields = DataReader.CSVHelper.ParseLine(curLine);
#if !DEBUG
							try
							{
#endif
								string posGroup = fields[2];
								StreamWriter outFile = posGroupOutFiles[posGroup];

								string curScoutLine = fields[30];
								string futScoutLine = fields[31];

								// Skip experience 0 players. These are drafted rookies that did not sign with a team.
								// Many (nut not all) still have blue bars, which will not fit this red/green bar algorithm.
								// I can't currently tell the blue guys from red/green guys, so throw them all out, we
								// should have enough other examples for this to work.
								if (fields[11] == "0")
								{
									continue;
								}

								int attributeStartField = 32;

								if (hasChemistry)
								{
									attributeStartField += 3;
								}

								if (posGroup == "QB")
								{
									// Skip formations
									attributeStartField += 1;
								}
								while (attributeStartField < fields.Length)
								{
									curScoutLine += "," + fields[attributeStartField++];
									futScoutLine += "," + fields[attributeStartField++];
								}
								outFile.WriteLine(curScoutLine);
								// When cur = fut, both lines are the same, so don't write fut
								if (fields[30] != fields[31])
								{
									outFile.WriteLine(futScoutLine);
								}
#if !DEBUG
							}
							catch (FormatException)
							{
								DialogResult result = MessageBox.Show("One of the fields on the line was bad:" + Environment.NewLine + curLine,
									"Parse Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
								if (result == DialogResult.Cancel)
								{
									return;
								}
							}
#endif
						}
					}
				}
			}
			catch (System.IO.IOException ioExcept)
			{
				MessageBox.Show("Could not open file '" + filename + "': " + ioExcept.ToString(), "Error Loading Extractor File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			foreach (StreamWriter outFile in posGroupOutFiles.Values)
			{
				outFile.Close();
			}

			MessageBox.Show("Finished writing files to '" + mOutputPath + "'", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void buttonGenerateDrafts_Click(object sender, EventArgs e)
		{
			string filename = buttonDraftTemplatePath.Text;
			mOutputPath = Path.GetDirectoryName(filename);
			int outFileCount = (int)numericUpDownDraftCount.Value;
			StreamWriter[] draftOutputFiles = new StreamWriter[outFileCount];
			for (int i = 0; i < outFileCount; ++i)
			{
				string outputFilename = Path.Combine(mOutputPath,"DraftFile_" + i.ToString("D3") + ".csv");
				// overwrite existing file if it is there
				draftOutputFiles[i] = new StreamWriter(outputFilename, false);
			}

			try
			{
				Random myRand = new Random();
				using (StreamReader inFile = new StreamReader(filename))
				{
					System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

					while (!inFile.EndOfStream)
					{
						string curLine = inFile.ReadLine();
						string[] fields = DataReader.CSVHelper.ParseLine(curLine);
#if !DEBUG
						try
						{
#endif
						int lastFieldIndex = fields.Length - 1;
						for (int i = 0; i < fields.Length; ++i)
						{
							for (int fileIndex = 0; fileIndex < outFileCount; ++fileIndex)
							{
								// Just copy the first 6 fields and the final ones, unimportant.
								if (i == lastFieldIndex)
								{
									draftOutputFiles[fileIndex].WriteLine(fields[i]);
								}
								else if (i <= 6 || i > 47)
								{
									draftOutputFiles[fileIndex].Write(fields[i] + ",");
								}
								// Random bar attributes
								else
								{
									if (fields[i] == "380")
									{
										draftOutputFiles[fileIndex].Write(fields[i] + ",");
									}
									else
									{
										int newBar = myRand.Next(380, 626);
										draftOutputFiles[fileIndex].Write(newBar.ToString() + ",");
									}
								}
							}
						}
#if !DEBUG
						}
						catch (FormatException)
						{
							DialogResult result = MessageBox.Show("One of the fields on the line was bad:" + Environment.NewLine + curLine,
								"Parse Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
							if (result == DialogResult.Cancel)
							{
								return;
							}
						}
#endif
					}
				}
			}
			catch (System.IO.IOException ioExcept)
			{
				MessageBox.Show("Could not open file '" + filename + "': " + ioExcept.ToString(), "Error Loading Draft CSV Template File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			for (int i = 0; i < outFileCount; ++i)
			{
				draftOutputFiles[i].Close();
			}

			MessageBox.Show("Finished writing files to '" + mOutputPath + "'", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void buttonDraftTemplatePath_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (buttonDraftTemplatePath.Text.Length > 0)
			{
				dlg.InitialDirectory = Path.GetDirectoryName(buttonDraftTemplatePath.Text);
			}
			dlg.FileName = buttonDraftTemplatePath.Text;
			dlg.Filter = "csv files (*.csv)|*.csv";
			dlg.Multiselect = false;
			dlg.CheckFileExists = true;
			dlg.CheckPathExists = true;
			dlg.DefaultExt = "exe";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonDraftTemplatePath.Text = dlg.FileName;
				mSettings.WriteXMLString(kSettingsRoot, kDraftTemplatePath, buttonDraftTemplatePath.Text);
			}
		}

		private void buttonGrowthDataPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			if (buttonGrowthDataPath.Text.Length > 0)
			{
				dlg.SelectedPath = buttonGrowthDataPath.Text;
			}
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonGrowthDataPath.Text = dlg.SelectedPath;
				mSettings.WriteXMLString(kSettingsRoot, kGrowthDataPath, buttonGrowthDataPath.Text);
			}
		}

		private void buttonProcessGrowth_Click(object sender, EventArgs e)
		{
			GrowthData growthData = new GrowthData(buttonGrowthDataPath.Text);
			try
			{
				growthData.Process();

				MessageBox.Show("Finished processing Growth files from '" + buttonGrowthDataPath.Text + "'", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception except)
			{
				MessageBox.Show(except.Message, "Growth Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}

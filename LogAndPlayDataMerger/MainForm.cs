using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LogAndPlayDataMerger
{
	public partial class MainForm : Form
	{
		delegate void AddStatusTextCallback(string text);
		delegate void WorkCompletedCallback();

		private const string kSettingsRoot = "LogAndPlayDataMerger";
		private const string kHTMLLogDirectory = "HTMLLogDirectory";
		private const string kOutputDirectory = "OutputDirectory";

		private WindowsUtilities.XMLSettings mSettings;

		private Cursor mOldCursor = null;

		public MainForm()
		{
			Assembly a = typeof(MainForm).Assembly;
			Text += " v" + a.GetName().Version;

			InitializeComponent();

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "LogAndPlayDataMerger.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);
			buttonLogFileDirectory.Text = mSettings.ReadXMLString(kSettingsRoot, kHTMLLogDirectory, "");
			buttonOutputDirectory.Text = mSettings.ReadXMLString(kSettingsRoot, kOutputDirectory, "");
		}

		private void UpdateThreadTask()
		{
			AddStatusString("Starting...");

			if (checkBoxMergeInfoPlays.Checked)
			{
				MergePlays("InfoPlays.csv");
			}

			if (checkBoxMergeFieldGoalPlays.Checked)
			{
				MergePlays("FGPlays.csv");
			}

			if (checkBoxMergeOnsideKickPlays.Checked)
			{
				MergePlays("OnsidePlays.csv");
			}

			if (checkBoxMergePuntPlays.Checked)
			{
				MergePlays("PuntPlays.csv");
			}

			if (checkBoxMergePassPlays.Checked)
			{
				MergePlays("PassPlays.csv");
			}

			if (checkBoxMergeRunPlays.Checked)
			{
				MergePlays("RunPlays.csv");
			}

			AddStatusString("Finished!");

			WorkCompleted();
		}

		private void AddStatusString(string newText)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.labelOutputStatus.InvokeRequired)
			{
				AddStatusTextCallback d = new AddStatusTextCallback(AddStatusString);
				this.Invoke(d, new object[] { newText });
			}
			else
			{
				labelOutputStatus.Text = newText;
				Refresh();
			}
		}

		private void WorkCompleted()
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.InvokeRequired)
			{
				WorkCompletedCallback d = new WorkCompletedCallback(WorkCompleted);
				this.Invoke(d);
			}
			else
			{
				Cursor = mOldCursor;
				buttonMergeFiles.Enabled = true;
			}
		}

		private void MergePlays(string csvFilename)
		{
			string inputCSVPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), csvFilename);
			if (!System.IO.File.Exists(inputCSVPath))
			{
				AddStatusString(csvFilename + " does not exist, skipping");
				return;
			}
			string outputCSVPath = System.IO.Path.Combine(buttonOutputDirectory.Text, csvFilename);
			using (System.IO.StreamWriter outFile = new System.IO.StreamWriter(outputCSVPath, false))
			{
				using (System.IO.StreamReader csvFile = new System.IO.StreamReader(inputCSVPath))
				{
					AddStatusString("Working on " + csvFilename);
					string headerLine = csvFile.ReadLine();
					outFile.WriteLine(headerLine);
					string oldGameId = "";
					while (!csvFile.EndOfStream)
					{
						string nextLine = csvFile.ReadLine();
						if (nextLine.Length > 0)
						{
							string fofText = "";
							string gameID = nextLine.Substring(1, 10);
							if (gameID != oldGameId)
							{
								ParseGameLog(gameID);
								oldGameId = gameID;
							}

							int playIDStart = 12;
							int playIDEnd = nextLine.IndexOf(",", playIDStart);
							int playID = 0;
							if (playIDEnd > 0)
							{
								string playIDString = nextLine.Substring(playIDStart, playIDEnd - playIDStart);
								if (Int32.TryParse(playIDString, out playID))
								{
									if (playID < mLogPlays.Count)
									{
										fofText = (string)mLogPlays[playID];
									}
								}
							}

							outFile.WriteLine("\"" + fofText + "\"" + nextLine);
						}
					}
					csvFile.Close();
				}
				outFile.Close();
			}
		}

		System.Collections.ArrayList mLogPlays = new System.Collections.ArrayList();

		private void ParseGameLog(string gameID)
		{
			mLogPlays = new System.Collections.ArrayList();
			string logPath = System.IO.Path.Combine(buttonLogFileDirectory.Text, "log" + gameID + ".html");
			if (System.IO.File.Exists(logPath))
			{
				using (System.IO.StreamReader logFile = new System.IO.StreamReader(logPath))
				{
					AddStatusString("Merging log " + gameID);
					int lineNumber = 0;
					while (!logFile.EndOfStream)
					{
						string curLine = logFile.ReadLine();
						++lineNumber;
						if (lineNumber <= 6)
						{
							continue;
						}

						int fontStart = curLine.IndexOf("<FONT");
						if (fontStart < 0)
						{
							continue;
						}

						int lineStart = curLine.IndexOf(">", fontStart);
						if (lineStart < 0)
						{
							continue;
						}
						++lineStart;

						int breakInMiddle = curLine.IndexOf("<BR>", lineStart);
						if (breakInMiddle > lineStart)
						{
							lineStart = breakInMiddle + 4;
						}

						int lineEnd = curLine.IndexOf("</FONT>", lineStart);
						if (lineEnd < 0)
						{
							continue;
						}

						string playLine = curLine.Substring(lineStart, lineEnd - lineStart);
						playLine = playLine.Replace("<B>", "");
						playLine = playLine.Replace("</B>", "");

						mLogPlays.Add(playLine);
					}
					logFile.Close();
				}
			}
		}

		private void buttonMergeFiles_Click(object sender, EventArgs e)
		{
			mOldCursor = Cursor;
			Cursor = Cursors.WaitCursor;
			buttonMergeFiles.Enabled = false;
			System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateThreadTask));
			updateThread.IsBackground = true;
			updateThread.Start();
		}

		private void buttonLogFileDirectory_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = buttonOutputDirectory.Text;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonLogFileDirectory.Text = dlg.SelectedPath;
				mSettings.WriteXMLString(kSettingsRoot, kHTMLLogDirectory, dlg.SelectedPath);
			}
		}

		private void buttonOutputDirectory_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath = buttonOutputDirectory.Text;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonOutputDirectory.Text = dlg.SelectedPath;
				mSettings.WriteXMLString(kSettingsRoot, kOutputDirectory, dlg.SelectedPath);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Maddenator
{
	public partial class MaddenatorForm : Form
	{
		private const string kSettingsRoot = "Maddenator";
		private const string kExtractorCSVPath = "ExtractorCSVPath";
		private const string kMaddenRosterPath = "MaddenRosterPath";

		private WindowsUtilities.XMLSettings mSettings;

		public MaddenatorForm()
		{
			InitializeComponent();

			Assembly a = typeof(MaddenatorForm).Assembly;
			Text += " v" + a.GetName().Version;

			BuildConversionTables();

			mLogFileName = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "Maddenator.log");

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "Maddenator.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);
			buttonExtractorCSVPath.Text = mSettings.ReadXMLString(kSettingsRoot, kExtractorCSVPath, "Extractor CSV Path");
			buttonMaddenRosterPath.Text = mSettings.ReadXMLString(kSettingsRoot, kMaddenRosterPath, "Madden Roster Path");
		}

		private string mLogFileName;
		private System.IO.StreamWriter mLogFile;

		private void buttonRunConversion_Click(object sender, EventArgs e)
		{
			using (mLogFile = new System.IO.StreamWriter(mLogFileName))
			{
				int dbIndex = TDBAccess.TDBOpen(buttonMaddenRosterPath.Text);
				LoadMaddenTables(dbIndex);
				LoadExtractorData();
				DoConversion();
				SaveMaddenTables(dbIndex);
				labelStatus.Text = "Writing Madden Roster...";
				labelStatus.Refresh();
				TDBAccess.TDBSave(dbIndex);
				TDBAccess.TDBClose(dbIndex);
				labelStatus.Text = "Done!";
				labelStatus.Refresh();
			}
		}

		private void buttonMaddenRosterPath_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Madden Roster files (*.ros)|*.ros|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonMaddenRosterPath.Text = dlg.FileName;
				mSettings.WriteXMLString(kSettingsRoot, kMaddenRosterPath, dlg.FileName);
			}
		}

		private void buttonExtractorCSVPath_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Extractor CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonExtractorCSVPath.Text = dlg.FileName;
				mSettings.WriteXMLString(kSettingsRoot, kExtractorCSVPath, dlg.FileName);
			}
		}

		private int FindBestMaddenPlayer(PlayerData playerData)
		{
			// Find highest overall at same position and replace him
			int foundIndex = -1;
			int foundOverall = -1;
			int teamIndex = -1;
			try
			{
				teamIndex = mMaddenTeamIndicesFromFOFName[playerData.Team];

				for (int teamPlayerIndex = 0; teamPlayerIndex < mMaddenTeams[teamIndex].PlayerIndices.Count; ++teamPlayerIndex)
				{
					int testIndex = mMaddenTeams[teamIndex].PlayerIndices[teamPlayerIndex];

					if (mMaddenPlayers[testIndex].Updated == false)
					{
						int weight = 0;
						if (mMaddenPlayers[testIndex].PositionID == mPositionMap[playerData.Position])
						{
							weight = mMaddenPlayers[testIndex].Overall + 101;
						}
						else
						{
							weight = 100 - mMaddenPlayers[testIndex].Overall;
						}
						if (weight > foundOverall)
						{
							foundIndex = testIndex;
							foundOverall = weight;
						}
					}
				}
			}
			catch (KeyNotFoundException)
			{
				mLogFile.WriteLine("Could not find team '" + playerData.Team + "' in Madden teams");
			}

			return foundIndex;
		}

		private void DoConversion()
		{
			int tmp = 0;
			foreach (PlayerData playerData in mPlayerData)
			{
				if (tmp++ % 20 == 0)
				{
					labelStatus.Text = playerData.Position + " " + playerData.Name;
					labelStatus.Refresh();
				}

				int maddenIndex = FindBestMaddenPlayer(playerData);
				if (maddenIndex >= 0)
				{
					mLogFile.WriteLine("Replacing Madden player " + PositionNames[mMaddenPlayers[maddenIndex].PositionID] + " "
						+ mMaddenPlayers[maddenIndex].LastName + ", " + mMaddenPlayers[maddenIndex].FirstName
						+ " (OVR " + mMaddenPlayers[maddenIndex].Overall + ") with FOF player " + playerData.PositionGroup + " " + playerData.Name 
						+ " (Rat " + playerData.Current + "/" + playerData.Future + ")" + " on " + playerData.Team);
					mMaddenPlayers[maddenIndex].Updated = true;
					int nameBreak = playerData.Name.IndexOf(' ');
					mMaddenPlayers[maddenIndex].FirstName = playerData.Name.Substring(0, nameBreak);
					mMaddenPlayers[maddenIndex].LastName = playerData.Name.Substring(nameBreak + 1);
					int nicknameEnd = mMaddenPlayers[maddenIndex].LastName.LastIndexOf('\'');
					if (nicknameEnd >= 0)
					{
						// +1 gets rid of the space after the nickname
						mMaddenPlayers[maddenIndex].LastName = mMaddenPlayers[maddenIndex].LastName.Substring(nicknameEnd + 1);
					}
					mMaddenPlayers[maddenIndex].PositionID = mPositionMap[playerData.Position];
					mMaddenPlayers[maddenIndex].Height = playerData.Height;
					mMaddenPlayers[maddenIndex].Weight = playerData.Weight - 160;
					mMaddenPlayers[maddenIndex].Age = playerData.Experience + 22;
					mMaddenPlayers[maddenIndex].NumberJersey = playerData.Jersey;
					mMaddenPlayers[maddenIndex].Experience = playerData.Experience;
					CalculateSkills(maddenIndex, playerData);
					CalculateAppearance(maddenIndex, playerData);
				}
				else
				{
					mLogFile.WriteLine("Could not find a Madden player to replace with " + playerData.Position + " " + playerData.Name
						+ " (Rat " + playerData.Current + "/" + playerData.Future + ")"
						+ " of " + playerData.Team);
				}
			}
		}

		private int SkillFromOne(PlayerData playerData, int fofIndex, int min, int max)
		{
			int range = max - min;
			int skill = playerData.AdjustedAttributes[fofIndex];
			skill *= range;
			skill /= 100;
			skill += min;
			return skill;
		}

		private int SkillFromTwo(PlayerData playerData, int fof1, double weight1, int fof2, double weight2, int min, int max)
		{
			int range = max - min;
			double value1 = (double)playerData.AdjustedAttributes[fof1] * weight1;
			double value2 = (double)playerData.AdjustedAttributes[fof2] * weight2;
			double normalizedSkill = (value1 + value2) / (weight1 + weight2);
			int skill = (int)(Math.Round(normalizedSkill));
			skill *= range;
			skill /= 100;
			skill += min;
			return skill;
		}

		private int SkillFromMax(PlayerData playerData, int fof1, int fof2, int min, int max)
		{
			int range = max - min;
			int skill = Math.Max(playerData.AdjustedAttributes[fof1],playerData.AdjustedAttributes[fof2]);
			skill *= range;
			skill /= 100;
			skill += min;
			return skill;
		}

		private void CalculateSkills(int maddenIndex, PlayerData playerData)
		{
			int BD = playerData.DayOfBirth;
			int BM = playerData.MonthOfBirth;
			double tempOverall = 0.0;
			int OverallRating = 80;
			switch (playerData.PositionGroup)
			{
				case "QB":
					// 0 = Screen Passes		7 = Timing
					// 1 = Short Passes			8 = Sense Rush
					// 2 = Medium Passes		9 = Read Defense
					// 3 = Long Passes			10 = Two-Minute Offense
					// 4 = Deep Passes			11 = Scramble Frequency
					// 5 = Third Down Passes	12 = Kick Holding
					// 6 = Accuracy
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 11, 40, 95);
					mMaddenPlayers[maddenIndex].Strength = (((playerData.Weight - 180) * 50) / 100) + 40;
					mMaddenPlayers[maddenIndex].Awareness = SkillFromTwo(playerData, 9, 12.0, 5, 8.0, 60, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromTwo(playerData, 8, 15.0, 11, 5.0, 40, 95);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromTwo(playerData, 8, 8.0, 11, 12.0, 40, 95);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 12, 40, 60);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromOne(playerData, 11, 40, 80);
					mMaddenPlayers[maddenIndex].Jumping = 40;
					mMaddenPlayers[maddenIndex].BreakTackle = (int)(mMaddenPlayers[maddenIndex].Strength * 0.8 + mMaddenPlayers[maddenIndex].Agility * 0.2);
					mMaddenPlayers[maddenIndex].Tackle = 40;
					mMaddenPlayers[maddenIndex].ThrowPower = SkillFromMax(playerData, 3, 4, 60, 99);
					mMaddenPlayers[maddenIndex].ThrowAccuracy = SkillFromTwo(playerData, 6, 15.0, 7, 5.0, 60, 99);
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = 85;
					mMaddenPlayers[maddenIndex].Injury = 90;
					mMaddenPlayers[maddenIndex].Toughness = 90;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].ThrowPower - 50) / 10) * 4.9;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].ThrowAccuracy - 50) / 10) * 5.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].BreakTackle - 50) / 10) * 0.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 0.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 4.0;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 2.0;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 28, 1);
					break;
				case "RB":
					// 0 = Breakaway Speed		8 = Getting Downfield
					// 1 = Power Inside			9 = Route Running
					// 2 = Third Down Running	10 = Third Down Catching
					// 3 = Hole Recognition		11 = Punt Returns
					// 4 = Elusiveness			12 = Kick Returns
					// 5 = Speed to Outside		13 = Endurance
					// 6 = Blitz Pickup			14 = Special Teams
					// 7 = Avoid Drops
					mMaddenPlayers[maddenIndex].Speed = SkillFromTwo(playerData, 0, 15.0, 5, 5.0, 45, 99);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 1, 40, 90);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromTwo(playerData, 3, 15.0, 6, 5.0, 40, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 4, 50, 99);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromTwo(playerData, 3, 5.0, 6, 15.0, 50, 99);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 7, 50, 90);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromTwo(playerData, 1, 8.0, 2, 12.0, 60, 99);
					mMaddenPlayers[maddenIndex].Jumping = SkillFromOne(playerData, 10, 40, 85);
					mMaddenPlayers[maddenIndex].BreakTackle = SkillFromTwo(playerData,2,6.0,4,14.0,50,99);
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 14, 40, 65);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = SkillFromOne(playerData, 6, 40, 80);
					mMaddenPlayers[maddenIndex].RunBlocking = (int)((((1.4 * mMaddenPlayers[maddenIndex].Awareness + 0.6 * mMaddenPlayers[maddenIndex].Strength) / 2) / 100) * 30) + 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = SkillFromMax(playerData, 11, 12, 40, 99);
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 13, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 13, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 13, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].PassBlocking - 50) / 10) * 0.33;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].BreakTackle - 50) / 10) * 3.3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Carrying - 50) / 10) * 2.0;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 2.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 2.0;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 0.6;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 3.3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 1.4;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 27, 1);
					break;
				case "FB":
					// 0 = Run Blocking			6 = Blitz Pickup
					// 1 = Pass Blocking		7 = Avoid Drops
					// 2 = Blocking Strength	8 = Route Running
					// 3 = Power Inside			9 = Third Down Catching
					// 4 = Third Down Running	10 = Endurance
					// 5 = Hole Recognition		11 = Special Teams
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 8, 40, 80);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 2, 50, 85);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromTwo(playerData, 5, 10.0, 6, 10.0, 40, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 8, 40, 85);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 9, 40, 85);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 7, 40, 95);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromOne(playerData, 4, 50, 99);
					mMaddenPlayers[maddenIndex].Jumping = SkillFromOne(playerData, 9, 40, 70);
					mMaddenPlayers[maddenIndex].BreakTackle = SkillFromTwo(playerData, 3, 12.0, 4, 8.0, 40, 90);
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 11, 40, 70);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = SkillFromOne(playerData, 1, 50, 90);
					mMaddenPlayers[maddenIndex].RunBlocking = SkillFromOne(playerData, 0, 50, 90);
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 10, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 10, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 10, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].PassBlocking - 50) / 10) * 1.0;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].RunBlocking - 50) / 10) * 7.2;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].BreakTackle - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Carrying - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.0;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 2.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 5.2;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 39, 1);
					break;
				case "TE":
					// 0 = Run Blocking			7 = Big Play Receiving
					// 1 = Pass Blocking		8 = Courage
					// 2 = Blocking Strength	9 = Adjust to Ball
					// 3 = Avoid Drops			10 = Endurance
					// 4 = Getting Downfield	11 = Special Teams
					// 5 = Route Running		12 = Long Snapping
					// 6 = Third Down Catching
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 7, 40, 90);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 2, 40, 95);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 9, 40, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 5, 40, 90);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 4, 40, 90);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 3, 40, 95);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromOne(playerData, 6, 40, 90);
					mMaddenPlayers[maddenIndex].Jumping = SkillFromOne(playerData, 6, 40, 85);
					mMaddenPlayers[maddenIndex].BreakTackle = SkillFromOne(playerData, 4, 40, 85);
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 11, 40, 85);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = SkillFromOne(playerData, 1, 50, 90);
					mMaddenPlayers[maddenIndex].RunBlocking = SkillFromOne(playerData, 0, 50, 90);
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 10, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 10, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 10, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 2.65;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 2.65;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 2.65;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.25;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.25;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 5.4;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].BreakTackle - 50) / 10) * 1.2;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].PassBlocking - 50) / 10) * 1.2;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].RunBlocking - 50) / 10) * 5.4;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 35, 1);
					break;
				case "WR":
					// 0 = Avoid Drops			6 = Adjust To Ball
					// 1 = Getting Downfield	7 = Punt Returns
					// 2 = Route Running		8 = Kick Returns
					// 3 = Third Down Catching	9 = Endurance
					// 4 = Big-Play Receiving	10 = Special Teams
					// 5 = Courage
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 4, 60, 99);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 5, 40, 80);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 6, 50, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 2, 50, 99);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 1, 50, 99);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 0, 60, 99);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromOne(playerData, 3, 40, 90);
					mMaddenPlayers[maddenIndex].Jumping = SkillFromTwo(playerData, 3, 8.0, 5, 12.0, 50, 99);
					mMaddenPlayers[maddenIndex].BreakTackle = SkillFromOne(playerData, 1, 40, 80);
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 10, 40, 65);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = SkillFromOne(playerData, 5, 40, 70);
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = SkillFromMax(playerData, 7, 8, 40, 99);
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 9, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 9, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 9, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].BreakTackle - 50) / 10) * 0.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 2.3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 2.3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 2.3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 0.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 2.3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 4.75;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Jumping - 50) / 10) * 1.4;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 26, 1);
					break;
				case "T":
				case "G":
				case "C":
					// 0 = Run Blocking			3 = Endurance
					// 1 = Pass Blocking		4 = Long Snapping (C only)
					// 2 = Blocking Strength
					mMaddenPlayers[maddenIndex].Speed = SkillFromTwo(playerData, 0, 4.0, 1, 16.0, 40, 70);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 2, 50, 99);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromTwo(playerData, 0, 10.0, 1, 10.0, 50, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromTwo(playerData, 0, 8.0, 1, 12.0, 40, 70);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromTwo(playerData, 0, 6.0, 1, 14.0, 40, 95);
					mMaddenPlayers[maddenIndex].Catching = 40;
					mMaddenPlayers[maddenIndex].Carrying = 40;
					mMaddenPlayers[maddenIndex].Jumping = SkillFromTwo(playerData, 0, 10.0, 1, 10.0, 40, 70);
					mMaddenPlayers[maddenIndex].BreakTackle = SkillFromTwo(playerData, 2, 16.0, 3, 4.0, 40, 70); ;
					mMaddenPlayers[maddenIndex].Tackle = SkillFromTwo(playerData, 0, 10.0, 2, 10.0, 40, 70);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = SkillFromOne(playerData, 1, 60, 99);
					mMaddenPlayers[maddenIndex].RunBlocking = SkillFromOne(playerData, 0, 60, 99);
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 3, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 3, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 3, 65, 99);
					if (playerData.PositionGroup == "T")
					{
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 0.8;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 3.3;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 3.3;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 0.8;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 0.8;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].PassBlocking - 50) / 10) * 4.75;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].RunBlocking - 50) / 10) * 3.75;
						OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 26, 1);
					}
					else
					{
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 1.7;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 3.25;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 3.25;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 0.8;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.7;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].PassBlocking - 50) / 10) * 3.25;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].RunBlocking - 50) / 10) * 4.8;
						OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 28, 1);
					}
					break;
				case "DE":
					// 0 = Run Defense             3 = Play Diagnosis
					// 1 = Pass Rush Technique     4 = Punishing Hitter
					// 2 = Pass Rush Strength      5 = Endurance
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 1, 50, 90);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 2, 50, 95);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 3, 60, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 1, 50, 90);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 1, 50, 95);
					mMaddenPlayers[maddenIndex].Catching = 40;
					mMaddenPlayers[maddenIndex].Carrying = 40;
					mMaddenPlayers[maddenIndex].Jumping = 40;
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = SkillFromTwo(playerData, 0, 14.0, 4, 6.0, 60, 99);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 5, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 5, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 5, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 3.75;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 3.75;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 1.75;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.75;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 3.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 5.5;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 30, 1);
					break;
				case "DT":
					// 0 = Run Defense             3 = Play Diagnosis
					// 1 = Pass Rush Technique     4 = Punishing Hitter
					// 2 = Pass Rush Strength      5 = Endurance
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 1, 40, 75);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 2, 60, 99);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 3, 40, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 1, 40, 80);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 1, 40, 85);
					mMaddenPlayers[maddenIndex].Catching = 40;
					mMaddenPlayers[maddenIndex].Carrying = 40;
					mMaddenPlayers[maddenIndex].Jumping = 40;
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = SkillFromTwo(playerData, 0, 14.0, 4, 6.0, 60, 99);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 5, 50, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 5, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 5, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 1.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 5.5;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 3.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 2.8;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 4.55;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 29, 1);
					break;
				case "OLB":
				case "ILB":
					// 0 = Run Defense             5 = Pass Rush Strength
					// 1 = Pass Rush Technique     6 = Play Diagnosis
					// 2 = Man-to-Man Coverage     7 = Punishing Hitter
					// 3 = Zone Coverage           8 = Endurance
					// 4 = Bump-and-Run Coverage   9 = Special Teams
					mMaddenPlayers[maddenIndex].Speed = SkillFromTwo(playerData, 2, 12.0, 4, 8.0, 50, 95);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 5, 50, 95);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 6, 40, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 1, 40, 99);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 2, 40, 95);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 3, 40, 80);
					mMaddenPlayers[maddenIndex].Carrying = 40;
					mMaddenPlayers[maddenIndex].Jumping = SkillFromOne(playerData, 3, 40, 60);
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 0, 50, 99);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 8, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 8, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 8, 65, 99);
					if (playerData.PositionGroup == "OLB")
					{
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 3.75;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 2.4;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 3.6;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 2.4;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.3;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 1.3;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 4.8;
						OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 29, 1);
					}
					else
					{
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 0.75;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 3.4;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 5.2;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.65;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.75;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 5.2;
						OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 27, 1);
					}
					break;
				case "CB":
					// 0 = Run Defense            6 = Interceptions
					// 1 = Man-to-Man Coverage    7 = Punt Returns
					// 2 = Zone Coverage          8 = Kick Returns
					// 3 = Bump-and-Run Coverage  9 = Endurance
					// 4 = Play Diagnosis        10 = Special Teams
					// 5 = Punishing Hitter
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 1, 50, 99);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 5, 40, 70);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 4, 50, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 3, 50, 99);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 2, 50, 99);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 6, 40, 90);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromOne(playerData, 10, 40, 60);
					mMaddenPlayers[maddenIndex].Jumping = SkillFromTwo(playerData, 1, 10.0, 6, 10.0, 50, 99);
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 0, 40, 85);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = SkillFromMax(playerData, 6, 7, 40, 99);
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 9, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 9, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 9, 65, 99);
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 3.85;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 0.9;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 3.85;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.55;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 2.35;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 3;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Jumping - 50) / 10) * 1.55;
					tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 1.55;
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 28, 1);
					break;
				case "S":
					// 0 = Run Defense            6 = Interceptions
					// 1 = Man-to-Man Coverage    7 = Punt Returns
					// 2 = Zone Coverage          8 = Kick Returns
					// 3 = Bump-and-Run Coverage  9 = Endurance
					// 4 = Play Diagnosis        10 = Special Teams
					// 5 = Punishing Hitter
					mMaddenPlayers[maddenIndex].Speed = SkillFromOne(playerData, 1, 40, 90);
					mMaddenPlayers[maddenIndex].Strength = SkillFromOne(playerData, 5, 50, 85);
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 4, 50, 99);
					mMaddenPlayers[maddenIndex].Agility = SkillFromOne(playerData, 3, 40, 90);
					mMaddenPlayers[maddenIndex].Acceleration = SkillFromOne(playerData, 2, 40, 90);
					mMaddenPlayers[maddenIndex].Catching = SkillFromOne(playerData, 6, 40, 85);
					mMaddenPlayers[maddenIndex].Carrying = SkillFromOne(playerData, 10, 40, 60);
					mMaddenPlayers[maddenIndex].Jumping = SkillFromTwo(playerData, 1, 10.0, 6, 10.0, 40, 90);
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = SkillFromOne(playerData, 0, 50, 95);
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = 40;
					mMaddenPlayers[maddenIndex].KickAccuracy = 40;
					mMaddenPlayers[maddenIndex].KickReturn = SkillFromMax(playerData, 6, 7, 40, 99);
					mMaddenPlayers[maddenIndex].Stamina = SkillFromOne(playerData, 9, 40, 99);
					mMaddenPlayers[maddenIndex].Injury = SkillFromOne(playerData, 9, 65, 99);
					mMaddenPlayers[maddenIndex].Toughness = SkillFromOne(playerData, 9, 65, 99);
					if (playerData.Position == "FS")
					{
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 3.0;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 0.9;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 4.85;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.5;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 2.5;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 3.0;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Jumping - 50) / 10) * 1.5;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 2.5;
						OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 30, 1);
					}
					else
					{
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Speed - 50) / 10) * 3.2;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Strength - 50) / 10) * 1.7;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Awareness - 50) / 10) * 4.75;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Agility - 50) / 10) * 1.7;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Acceleration - 50) / 10) * 1.7;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Catching - 50) / 10) * 3.2;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Jumping - 50) / 10) * 0.9;
						tempOverall += (((double)mMaddenPlayers[maddenIndex].Tackle - 50) / 10) * 3.2;
						OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall) + 30, 1);
					}
					break;
				case "P":
					// 0 = Kick Power		2 = Directional Punting
					// 1 = Punt Hang Time	3 = Kick Holding
					mMaddenPlayers[maddenIndex].Speed = ((BD * 3) / 2) + 40;
					mMaddenPlayers[maddenIndex].Strength = ((BM * 3) / 2) + 40;
					mMaddenPlayers[maddenIndex].Awareness = SkillFromOne(playerData, 1, 50, 99);
					mMaddenPlayers[maddenIndex].Agility = ((BD * 14) / 10) + 45;
					mMaddenPlayers[maddenIndex].Acceleration = ((BD * 13) / 10) + 50;
					mMaddenPlayers[maddenIndex].Catching = 40;
					mMaddenPlayers[maddenIndex].Carrying = 40;
					mMaddenPlayers[maddenIndex].Jumping = 40;
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = (BM * 2) + 40;
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = SkillFromOne(playerData, 0, 60, 99);
					mMaddenPlayers[maddenIndex].KickAccuracy = SkillFromOne(playerData, 2, 60, 99);
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = (BM * 2) + 50;
					mMaddenPlayers[maddenIndex].Injury = (BM * 2) + 65;
					mMaddenPlayers[maddenIndex].Toughness = (BM * 2) + 65;
					tempOverall = (double)(-183 + 0.218 * mMaddenPlayers[maddenIndex].Awareness + 1.5 * mMaddenPlayers[maddenIndex].KickPower + 1.33 * mMaddenPlayers[maddenIndex].KickAccuracy);
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall));
					break;
				case "K":
					// 0 = Kicking Accuracy		2 = Kickoff Distance
					// 1 = Kicking Power		3 = Kickoff Hang Time
					mMaddenPlayers[maddenIndex].Speed = ((BD * 3) / 2) + 40;
					mMaddenPlayers[maddenIndex].Strength = ((BM * 3) / 2) + 40;
					mMaddenPlayers[maddenIndex].Awareness = SkillFromTwo(playerData, 0, 4.0, 3, 16.0, 50, 99);
					mMaddenPlayers[maddenIndex].Agility = ((BD * 14) / 10) + 45;
					mMaddenPlayers[maddenIndex].Acceleration = ((BD * 13) / 10) + 50;
					mMaddenPlayers[maddenIndex].Catching = 40;
					mMaddenPlayers[maddenIndex].Carrying = 40;
					mMaddenPlayers[maddenIndex].Jumping = 40;
					mMaddenPlayers[maddenIndex].BreakTackle = 40;
					mMaddenPlayers[maddenIndex].Tackle = (BM * 2) + 40;
					mMaddenPlayers[maddenIndex].ThrowPower = 40;
					mMaddenPlayers[maddenIndex].ThrowAccuracy = 40;
					mMaddenPlayers[maddenIndex].PassBlocking = 40;
					mMaddenPlayers[maddenIndex].RunBlocking = 40;
					mMaddenPlayers[maddenIndex].KickPower = SkillFromOne(playerData, 1, 60, 99);
					mMaddenPlayers[maddenIndex].KickAccuracy = SkillFromOne(playerData, 0, 60, 99);
					mMaddenPlayers[maddenIndex].KickReturn = 40;
					mMaddenPlayers[maddenIndex].Stamina = (BM * 2) + 50;
					mMaddenPlayers[maddenIndex].Injury = (BM * 2) + 65;
					mMaddenPlayers[maddenIndex].Toughness = (BM * 2) + 65;
					tempOverall = (double)(-177 + 0.218 * mMaddenPlayers[maddenIndex].Awareness + 1.28 * mMaddenPlayers[maddenIndex].KickPower + 1.47 * mMaddenPlayers[maddenIndex].KickAccuracy);
					OverallRating = (int)Math.Round((decimal)Convert.ToInt32(tempOverall));
					break;
			}

			if (OverallRating > 99)
			{
				OverallRating = 99;
			}
			else if (OverallRating < 12)
			{
				OverallRating = 12;
			}
			mMaddenPlayers[maddenIndex].Overall = OverallRating;
		}

		private void CalculateAppearance(int maddenIndex, PlayerData playerData)
		{
			int t; 
			int BV;

			int BD = playerData.DayOfBirth;
			int BM = playerData.MonthOfBirth;

			BV = ((BD * 100) + BM) / 6; 
			if (BV > 518) 
				BV = 500; 
			if (BV < 1) 
				BV = 1; 

			int lightMax = 10;
			int medMax = 20;

			switch (playerData.PositionGroup) 
			{ 
				case "QB": 
					lightMax = 25;
					medMax = 26;
					break; 
				case "RB": 
					lightMax = 4;
					medMax = 13;
					break; 
				case "FB": 
					lightMax = 5;
					medMax = 14;
					break; 
				case "TE": 
					lightMax = 20;
					medMax = 24;
					break; 
				case "WR": 
					lightMax = 5;
					medMax = 14;
					break; 
				case "T":
				case "G":
				case "C":
					lightMax = 14;
					medMax = 19;
					break; 
				case "DE": 
				case "DT": 
					lightMax = 8;
					medMax = 16;
					break; 
				case "OLB":
				case "ILB":
					lightMax = 8;
					medMax = 18;
					break; 
				case "CB": 
					lightMax = 1;
					medMax = 11;
					break; 
				case "S": 
					lightMax = 4;
					medMax = 14;
					break; 
				case "P": 
					lightMax = 29;
					medMax = 30;
					break; 
				case "K": 
					lightMax = 28;
					medMax = 30;
					break; 
			} 
		    
			if (BD <= lightMax)
			{
				if (SkinTone[BV] != 'L') 
				{ 
					do 
					{ 
						BV = BV + 1; 
					} 
					while (SkinTone[BV] != 'L'); 
				} 
			}
			else if (BD <= medMax)
			{
				if (SkinTone[BV] != 'M') 
				{ 
					do 
					{ 
						BV = BV + 1; 
					} 
					while (SkinTone[BV] != 'M'); 
				} 
			}
			else
			{
				if (SkinTone[BV] != 'D') 
				{ 
					do 
					{ 
						BV = BV + 1; 
					} 
					while (SkinTone[BV] != 'D'); 
				} 
			} 

			mMaddenPlayers[maddenIndex].Face = BV;
		    
			switch (SkinTone[BV]) { 
				case 'L': 
					if (BM <= 1)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 0; 
					}
					else if (BM <= 4)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 4; 
					}
					else if (BM <= 7)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 5; 
					}
					else
					{
						mMaddenPlayers[maddenIndex].HairStyle = 7; 
					} 
					break; 
				case 'M': 
					if (BM <= 2)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 1; 
					}
					else if (BM <= 5)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 4; 
					}
					else if (BM <= 7)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 5; 
					}
					else
					{
						mMaddenPlayers[maddenIndex].HairStyle = 7; 
					} 
					break; 
				case 'D': 
					if (BM <= 2)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 1; 
					}
					else if (BM <= 5)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 2; 
					}
					else if (BM <= 7)
					{
						mMaddenPlayers[maddenIndex].HairStyle = 4; 
					}
					else
					{
						mMaddenPlayers[maddenIndex].HairStyle = 5; 
					} 
					break; 
			} 
		    
			int PosAdj; 
			switch (playerData.PositionGroup)
			{ 
				case "QB": 
				case "WR": 
				case "CB": 
				case "P": 
				case "K": 
					PosAdj = 20; 
					break; 
				case "RB": 
				case "S": 
					PosAdj = 10; 
					break; 
				default: 
					PosAdj = 0; 
					break; 
			} 
			int AdjustedWeight = playerData.Weight - 175; 
			int AdjustedHeight = playerData.Height - 60; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 11) * (95 - (PosAdj * 2)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].BodyOverall = t; 
		    
			t = ((mMaddenPlayers[maddenIndex].Strength - 30) / 70) * 25; 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].BodyMuscle = t; 
		    
			t = (AdjustedWeight / 210) * 25; 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].BodyWeight = t; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 10) * 25; 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].BodyFat = t; 
		    
			t = 25; 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].ShoeLength = t; 
		    
			t = 25; 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].ThighPads = t; 
		    
			switch (playerData.PositionGroup) 
			{ 
				case "ILB": 
				case "OLB": 
				case "T": 
				case "G": 
					t = mMaddenPlayers[maddenIndex].NumberJersey / 2; 
					break; 
				default: 
					t = 5; 
					break; 
			} 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].PadHeight = t; 
		    
			switch (playerData.PositionGroup)
			{ 
				case "ILB": 
				case "OLB": 
				case "T": 
				case "G": 
					t = mMaddenPlayers[maddenIndex].NumberJersey / 2; 
					break; 
				default: 
					t = 5; 
					break; 
			} 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].PadWidth = t;
		    
			switch (playerData.PositionGroup) 
			{ 
				case "QB": 
					t = mMaddenPlayers[maddenIndex].NumberJersey + 5; 
					break; 
				default: 
					t = 5; 
					break; 
			} 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].FlakJacket = t; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 11) * (40 - (PosAdj * 1)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].ArmFat = t; 
		    
			t = ((mMaddenPlayers[maddenIndex].Strength - 30) / 70) * (40 - (PosAdj * 1)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].ArmMuscle = t; 
		    
			t = ((mMaddenPlayers[maddenIndex].Acceleration - 30) / 70) * (40 - (PosAdj * 1)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].ThighMuscle = t; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 12) * (40 - (PosAdj * 1)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].ThighFat = t; 
		    
			t = ((mMaddenPlayers[maddenIndex].Speed - 30) / 70) * (25 - (PosAdj / 2)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].CalfMuscle = t; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 12) * (25 - (PosAdj / 2)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].CalfFat = t; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 12) * (80 - (PosAdj * 2)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].RearShape = t; 
		    
			t = ((AdjustedWeight / AdjustedHeight) / 12) * (25 - (PosAdj / 2)); 
			if (t < 0) 
				t = 0; 
			if (t > 100) 
				t = 100; 
			mMaddenPlayers[maddenIndex].RearFat = t; 
		}

		private const int kMaxAttributeCounts = 15;
		private class PlayerData : IComparable
		{
			public string Name;
			public string Position;
			public string PositionGroup;
			public string College;
			public string BirthDate;
			public string HomeTown;
			public string Agent;
			public string Team;
			public int Height;
			public int Weight;
			public int Experience;
			public int Jersey;
			public int Current;
			public int Future;
			public int Formations;
			public int[] Attributes = new int[kMaxAttributeCounts * 2];

			public int MaddenPositionID;
			public int DayOfBirth;
			public int MonthOfBirth;
			public int[] AdjustedAttributes = new int[kMaxAttributeCounts];

			public int CompareTo(object obj)
			{
				// Implements a descending sort
				if (obj is PlayerData)
				{
					PlayerData temp = (PlayerData)obj;
					return temp.Future.CompareTo(Future);
				}
				throw new ArgumentException("object is not a PlayerData");
			}
		}
		private System.Collections.ArrayList mPlayerData;

		private void LoadExtractorData()
		{
			labelStatus.Text = "Loading Extractor Data...";
			labelStatus.Refresh();

			string filename = buttonExtractorCSVPath.Text;
			try
			{
				using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
				{
					mPlayerData = new System.Collections.ArrayList();

					string headerLine = inFile.ReadLine();
					bool hasChemistry = headerLine.Contains(",Conflicts,Affinities,Character");
					while (!inFile.EndOfStream)
					{
						string curLine = inFile.ReadLine();
						string[] fields = DataReader.CSVHelper.ParseLine(curLine);
#if !DEBUG
						try
						{
#endif
							PlayerData newData = new PlayerData();
							newData.Name = fields[0];
							newData.Position = fields[1];
							newData.PositionGroup = fields[2];
							newData.College = fields[3];
							newData.Team = fields[4];
							newData.BirthDate = fields[5];
							newData.HomeTown = fields[6];
							newData.Agent = fields[7];
							newData.Height = Int32.Parse(fields[9]);
							newData.Weight = Int32.Parse(fields[10]);
							newData.Experience = Int32.Parse(fields[11]);
							newData.Jersey = Int32.Parse(fields[13]);
							newData.Current = Int32.Parse(fields[30]);
							newData.Future = Int32.Parse(fields[31]);
							int attributeStartField = 32;
							if (hasChemistry)
							{
								attributeStartField += 3;
							}
							newData.Formations = 0;
							if (newData.PositionGroup == "QB")
							{
								newData.Formations = Int32.Parse(fields[attributeStartField]);
								attributeStartField += 1;
							}
							int attributeIndex = 0;
							while (attributeStartField < fields.Length)
							{
								Int32.TryParse(fields[attributeStartField++], out newData.Attributes[attributeIndex++]);
								//newData.Attributes[attributeIndex++] = Int32.Parse(fields[attributeStartField++]);
								if ((attributeIndex % 2) == 0)
								{
									int start = newData.Attributes[attributeIndex - 2];
									int diff = newData.Attributes[attributeIndex - 1] - start;
									newData.AdjustedAttributes[(attributeIndex - 1) / 2] = start + ((diff * 2) / 3);
								}
							}

							char sepChar = '-';
							if (newData.BirthDate.IndexOf('/') >= 0)
							{
								sepChar = '/';
							}

							int dayStart = newData.BirthDate.IndexOf(sepChar);
							int monthEnd = dayStart;
							dayStart += 1;
							int dayEnd = newData.BirthDate.IndexOf(sepChar, dayStart);
							string BDx = newData.BirthDate.Substring(dayStart, dayEnd - dayStart);
							newData.DayOfBirth = Int32.Parse(BDx);
							string BMx = newData.BirthDate.Substring(0, monthEnd);
							newData.MonthOfBirth = Int32.Parse(BMx);

							mPlayerData.Add(newData);
#if !DEBUG
						}
						catch
						{
							DialogResult result = MessageBox.Show("One of the fields on the line was bad:" + Environment.NewLine + curLine,
								"Parse Error",MessageBoxButtons.RetryCancel,MessageBoxIcon.Error);
							if (result == DialogResult.Cancel)
							{
								return;
							}
						}
#endif
					}

					// Make sure we process players in descending future
					mPlayerData.Sort();
				}
			}
			catch (System.IO.IOException e)
			{
				MessageBox.Show("Could not open file '" + filename + "': " + e.ToString(), "Error Loading Extractor File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private TDBAccess.TableProperties[] mMaddenTableProperties;
		private int mTeamTableIndex;
		private int mPlayerTableIndex;

		private void LoadMaddenTables(int dbIndex)
		{
			mTeamTableIndex = -1;
			mPlayerTableIndex = -1;

			int tableCount = TDBAccess.TDBDatabaseGetTableCount(dbIndex);
			mMaddenTableProperties = new TDBAccess.TableProperties[tableCount];
			for (int tableIndex = 0; tableIndex < tableCount; ++tableIndex)
			{
				mMaddenTableProperties[tableIndex].Name = "";
				bool gotProperties = TDBAccess.TDBTableGetProperties(dbIndex, tableIndex, ref mMaddenTableProperties[tableIndex]);
				if (gotProperties)
				{
					if (mMaddenTableProperties[tableIndex].Name == "TEAM")
					{
						labelStatus.Text = "Reading Madden Teams...";
						labelStatus.Refresh();
						mTeamTableIndex = tableIndex;
						LoadMaddenTeams(dbIndex);
					}
					else if (mMaddenTableProperties[tableIndex].Name == "PLAY")
					{
						labelStatus.Text = "Reading Madden Players...";
						labelStatus.Refresh();
						mPlayerTableIndex = tableIndex;
						LoadMaddenPlayers(dbIndex);
					}
				}
				else
				{
					MessageBox.Show("Could not load properties for table " + tableIndex + " from Madden Roster file", 
						"Error Loading Roster File",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			SetMaddenPlayerTeamNames();
		}

		private void SetMaddenPlayerTeamNames()
		{
			for (int playerIndex = 0; playerIndex < mMaddenPlayers.Length; ++playerIndex)
			{
				if (mMaddenPlayers[playerIndex].TeamID == 1023)
				{
					mMaddenPlayers[playerIndex].TeamName = "Free Agent";
				}
				else
				{
					mMaddenPlayers[playerIndex].TeamName = mMaddenTeamNamesFromID[mMaddenPlayers[playerIndex].TeamID];
					int teamIndex = mMaddenTeamIndicesFromID[mMaddenPlayers[playerIndex].TeamID];
					mMaddenTeams[teamIndex].PlayerIndices.Add(playerIndex);
				}
			}
		}

		private class MaddenTeam
		{
			public MaddenTeam()
			{
				CityName = "";
				NickName = "";
				PlayerIndices = new List<int>();
			}

			public int TeamGameID;
			public string CityName;
			public string NickName;

			public System.Collections.Generic.List<int> PlayerIndices;
		}
		private MaddenTeam[] mMaddenTeams;

		private class MaddenPlayer
		{
			public MaddenPlayer()
			{
				FirstName = "REALLY LONG FIRST NAME RESERVED";
				LastName = "REALL LONG LAST NAME RESERVED";
				TeamName = "";
				Updated = false;
				IndexInTable = -1;
			}

			public bool Updated;
			public string TeamName;
			public int IndexInTable;

			public string FirstName;
			public string LastName;
			public int Height;
			public int Weight;
			public int Age;
			public int Experience;
			public int TeamID;
			public int NumberJersey;
			public int PositionID;
			public int PlayerID;
			public int CollegeID;

			public int Face;
			public int Overall;

			public int Speed;
			public int Strength;
			public int Awareness;
			public int Agility;
			public int Acceleration;
			public int Catching;
			public int Carrying;
			public int Jumping;
			public int BreakTackle;
			public int Tackle;
			public int ThrowPower;
			public int ThrowAccuracy;
			public int PassBlocking;
			public int RunBlocking;
			public int KickPower;
			public int KickAccuracy;
			public int KickReturn;
			public int Stamina;
			public int Injury;
			public int Toughness;

			public int BodyOverall;
			public int BodyMuscle;
			public int BodyWeight;
			public int BodyFat;
			public int ShoeLength;
			public int ThighPads;
			public int PadHeight;
			public int PadWidth;
			public int PadShelf;
			public int FlakJacket;
			public int ArmFat;
			public int ArmMuscle;
			public int ThighMuscle;
			public int ThighFat;
			public int CalfMuscle;
			public int CalfFat;
			public int RearShape;
			public int RearFat;
			public int FaceShape;
			public int HairStyle;
		}
		private MaddenPlayer[] mMaddenPlayers;

		private System.Collections.Generic.Dictionary<int, string> mMaddenTeamNamesFromID;
		private System.Collections.Generic.Dictionary<int, int> mMaddenTeamIndicesFromID;
		private System.Collections.Generic.Dictionary<string, int> mMaddenTeamIndicesFromFOFName;

		private void LoadMaddenTeams(int dbIndex)
		{
			int recordCount = mMaddenTableProperties[mTeamTableIndex].RecordCount;
			mMaddenTeams = new MaddenTeam[recordCount];
			mMaddenTeamNamesFromID = new Dictionary<int, string>();
			mMaddenTeamIndicesFromID = new Dictionary<int, int>();
			mMaddenTeamIndicesFromFOFName = new Dictionary<string, int>();

			for (int teamIndex = 0; teamIndex < recordCount; ++teamIndex)
			{
				mMaddenTeams[teamIndex] = new MaddenTeam();
				mMaddenTeams[teamIndex].CityName = "RESERVE A LOT OF SPACE FOR NAME";
				mMaddenTeams[teamIndex].NickName = "RESERVE A LOT OF SPACE FOR NAME";
				mMaddenTeams[teamIndex].TeamGameID = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "TEAM", "TGID", teamIndex);
				TDBAccess.TDBFieldGetValueAsString(dbIndex, "TEAM", "TDNA", teamIndex, ref mMaddenTeams[teamIndex].NickName);
				TDBAccess.TDBFieldGetValueAsString(dbIndex, "TEAM", "TLNA", teamIndex, ref mMaddenTeams[teamIndex].CityName);

				string fofTeamName = mMaddenTeams[teamIndex].CityName + " " + mMaddenTeams[teamIndex].NickName;
				mMaddenTeamNamesFromID.Add(mMaddenTeams[teamIndex].TeamGameID, fofTeamName);
				mMaddenTeamIndicesFromID.Add(mMaddenTeams[teamIndex].TeamGameID, teamIndex);
				mMaddenTeamIndicesFromFOFName.Add(fofTeamName, teamIndex);
			}
		}

		private void LoadMaddenPlayers(int dbIndex)
		{
			int recordCount = mMaddenTableProperties[mPlayerTableIndex].RecordCount;
			mMaddenPlayers = new MaddenPlayer[recordCount];
			for (int playerIndex = 0; playerIndex < recordCount; ++playerIndex)
			{
				mMaddenPlayers[playerIndex] = new MaddenPlayer();
				mMaddenPlayers[playerIndex].Updated = false;
				mMaddenPlayers[playerIndex].IndexInTable = playerIndex;
				TDBAccess.TDBFieldGetValueAsString(dbIndex, "PLAY", MaddenPlayerFieldConstants.FirstName, playerIndex, ref mMaddenPlayers[playerIndex].FirstName);
				TDBAccess.TDBFieldGetValueAsString(dbIndex, "PLAY", MaddenPlayerFieldConstants.LastName, playerIndex, ref mMaddenPlayers[playerIndex].LastName);
				mMaddenPlayers[playerIndex].Height = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Height, playerIndex);
				mMaddenPlayers[playerIndex].Weight = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Weight, playerIndex);
				mMaddenPlayers[playerIndex].Age = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Age, playerIndex);
				mMaddenPlayers[playerIndex].Experience = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.YearsPro, playerIndex);
				mMaddenPlayers[playerIndex].TeamID = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.TeamID, playerIndex);
				mMaddenPlayers[playerIndex].NumberJersey = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.JerseyNumber, playerIndex);
				mMaddenPlayers[playerIndex].PositionID = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.PositionID, playerIndex);
				mMaddenPlayers[playerIndex].PlayerID = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.PlayerID, playerIndex);
				mMaddenPlayers[playerIndex].CollegeID = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.CollegeID, playerIndex);

				mMaddenPlayers[playerIndex].Face = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.FaceID, playerIndex);
				mMaddenPlayers[playerIndex].Overall = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Overall, playerIndex);

				mMaddenPlayers[playerIndex].Speed = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Speed, playerIndex);
				mMaddenPlayers[playerIndex].Strength = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Strength, playerIndex);
				mMaddenPlayers[playerIndex].Awareness = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Awareness, playerIndex);
				mMaddenPlayers[playerIndex].Agility = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Agility, playerIndex);
				mMaddenPlayers[playerIndex].Acceleration = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Acceleration, playerIndex);
				mMaddenPlayers[playerIndex].Catching = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Catching, playerIndex);
				mMaddenPlayers[playerIndex].Carrying = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Carrying, playerIndex);
				mMaddenPlayers[playerIndex].Jumping = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Jumping, playerIndex);
				mMaddenPlayers[playerIndex].BreakTackle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BreakTackle, playerIndex);
				mMaddenPlayers[playerIndex].Tackle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Tackle, playerIndex);
				mMaddenPlayers[playerIndex].ThrowPower = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThrowPower, playerIndex);
				mMaddenPlayers[playerIndex].ThrowAccuracy = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThrowAccuracy, playerIndex);
				mMaddenPlayers[playerIndex].PassBlocking = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.PassBlocking, playerIndex);
				mMaddenPlayers[playerIndex].RunBlocking = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.RunBlocking, playerIndex);
				mMaddenPlayers[playerIndex].KickPower = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.KickPower, playerIndex);
				mMaddenPlayers[playerIndex].KickAccuracy = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.KickAccuracy, playerIndex);
				mMaddenPlayers[playerIndex].KickReturn = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.KickReturn, playerIndex);
				mMaddenPlayers[playerIndex].Stamina = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Stamina, playerIndex);
				mMaddenPlayers[playerIndex].Injury = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Injury, playerIndex);
				mMaddenPlayers[playerIndex].Toughness = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Toughness, playerIndex);

				mMaddenPlayers[playerIndex].BodyOverall = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyOverall, playerIndex);
				mMaddenPlayers[playerIndex].BodyMuscle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyMuscle, playerIndex);
				mMaddenPlayers[playerIndex].BodyWeight = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyWeight, playerIndex);
				mMaddenPlayers[playerIndex].BodyFat = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyFat, playerIndex);
				mMaddenPlayers[playerIndex].ShoeLength = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpShoes, playerIndex);
				mMaddenPlayers[playerIndex].ThighPads = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.LegsThighPads, playerIndex);
				mMaddenPlayers[playerIndex].PadHeight = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpPadHeight, playerIndex);
				mMaddenPlayers[playerIndex].PadWidth = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpPadWidth, playerIndex);
				mMaddenPlayers[playerIndex].PadShelf = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpPadShelf, playerIndex);
				mMaddenPlayers[playerIndex].FlakJacket = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpFlakJacket, playerIndex);
				mMaddenPlayers[playerIndex].ArmFat = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ArmFat, playerIndex);
				mMaddenPlayers[playerIndex].ArmMuscle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ArmMuscle, playerIndex);
				mMaddenPlayers[playerIndex].ThighMuscle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThighMuscle, playerIndex);
				mMaddenPlayers[playerIndex].ThighFat = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThighFat, playerIndex);
				mMaddenPlayers[playerIndex].CalfMuscle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.CalfMuscle, playerIndex);
				mMaddenPlayers[playerIndex].CalfFat = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.CalfFat, playerIndex);
				mMaddenPlayers[playerIndex].RearShape = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.RearShape, playerIndex);
				mMaddenPlayers[playerIndex].RearFat = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.RearFat, playerIndex);
				mMaddenPlayers[playerIndex].FaceShape = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.FaceID, playerIndex);
				mMaddenPlayers[playerIndex].HairStyle = TDBAccess.TDBFieldGetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.HairStyle, playerIndex);
			}
		}

		private void SaveMaddenTables(int dbIndex)
		{
			labelStatus.Text = "Saving Madden Players...";
			labelStatus.Refresh();

			int recordCount = mMaddenPlayers.Length;
			for (int recIndex = 0; recIndex < recordCount; ++recIndex)
			{
				int playerIndex = mMaddenPlayers[recIndex].IndexInTable;

				if (mMaddenPlayers[playerIndex].Updated)
				{
					TDBAccess.TDBFieldSetValueAsString(dbIndex, "PLAY", MaddenPlayerFieldConstants.FirstName, playerIndex, mMaddenPlayers[playerIndex].FirstName);
					TDBAccess.TDBFieldSetValueAsString(dbIndex, "PLAY", MaddenPlayerFieldConstants.LastName, playerIndex, mMaddenPlayers[playerIndex].LastName);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Height, playerIndex, mMaddenPlayers[playerIndex].Height);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Weight, playerIndex, mMaddenPlayers[playerIndex].Weight);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Age, playerIndex, mMaddenPlayers[playerIndex].Age);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.YearsPro, playerIndex, mMaddenPlayers[playerIndex].Experience);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.TeamID, playerIndex, mMaddenPlayers[playerIndex].TeamID);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.JerseyNumber, playerIndex, mMaddenPlayers[playerIndex].NumberJersey);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.PositionID, playerIndex, mMaddenPlayers[playerIndex].PositionID);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.PlayerID, playerIndex, mMaddenPlayers[playerIndex].PlayerID);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.CollegeID, playerIndex, mMaddenPlayers[playerIndex].CollegeID);

					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.FaceID, playerIndex, mMaddenPlayers[playerIndex].Face);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Overall, playerIndex, mMaddenPlayers[playerIndex].Overall);

					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Speed, playerIndex, mMaddenPlayers[playerIndex].Speed);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Strength, playerIndex, mMaddenPlayers[playerIndex].Strength);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Awareness, playerIndex, mMaddenPlayers[playerIndex].Awareness);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Agility, playerIndex, mMaddenPlayers[playerIndex].Agility);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Acceleration, playerIndex, mMaddenPlayers[playerIndex].Acceleration);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Catching, playerIndex, mMaddenPlayers[playerIndex].Catching);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Carrying, playerIndex, mMaddenPlayers[playerIndex].Carrying);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Jumping, playerIndex, mMaddenPlayers[playerIndex].Jumping);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BreakTackle, playerIndex, mMaddenPlayers[playerIndex].BreakTackle);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Tackle, playerIndex, mMaddenPlayers[playerIndex].Tackle);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThrowPower, playerIndex, mMaddenPlayers[playerIndex].ThrowPower);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThrowAccuracy, playerIndex, mMaddenPlayers[playerIndex].ThrowAccuracy);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.PassBlocking, playerIndex, mMaddenPlayers[playerIndex].PassBlocking);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.RunBlocking, playerIndex, mMaddenPlayers[playerIndex].RunBlocking);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.KickPower, playerIndex, mMaddenPlayers[playerIndex].KickPower);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.KickAccuracy, playerIndex, mMaddenPlayers[playerIndex].KickAccuracy);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.KickReturn, playerIndex, mMaddenPlayers[playerIndex].KickReturn);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Stamina, playerIndex, mMaddenPlayers[playerIndex].Stamina);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Injury, playerIndex, mMaddenPlayers[playerIndex].Injury);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.Toughness, playerIndex, mMaddenPlayers[playerIndex].Toughness);

					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyOverall, playerIndex, mMaddenPlayers[playerIndex].BodyOverall);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyMuscle, playerIndex, mMaddenPlayers[playerIndex].BodyMuscle);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyWeight, playerIndex, mMaddenPlayers[playerIndex].BodyWeight);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.BodyFat, playerIndex, mMaddenPlayers[playerIndex].BodyFat);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpShoes, playerIndex, mMaddenPlayers[playerIndex].ShoeLength);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.LegsThighPads, playerIndex, mMaddenPlayers[playerIndex].ThighPads);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpPadHeight, playerIndex, mMaddenPlayers[playerIndex].PadHeight);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpPadWidth, playerIndex, mMaddenPlayers[playerIndex].PadWidth);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpPadShelf, playerIndex, mMaddenPlayers[playerIndex].PadShelf);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.EqpFlakJacket, playerIndex, mMaddenPlayers[playerIndex].FlakJacket);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ArmFat, playerIndex, mMaddenPlayers[playerIndex].ArmFat);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ArmMuscle, playerIndex, mMaddenPlayers[playerIndex].ArmMuscle);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThighMuscle, playerIndex, mMaddenPlayers[playerIndex].ThighMuscle);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.ThighFat, playerIndex, mMaddenPlayers[playerIndex].ThighFat);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.CalfMuscle, playerIndex, mMaddenPlayers[playerIndex].CalfMuscle);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.CalfFat, playerIndex, mMaddenPlayers[playerIndex].CalfFat);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.RearShape, playerIndex, mMaddenPlayers[playerIndex].RearShape);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.RearFat, playerIndex, mMaddenPlayers[playerIndex].RearFat);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.FaceID, playerIndex, mMaddenPlayers[playerIndex].FaceShape);
					TDBAccess.TDBFieldSetValueAsInteger(dbIndex, "PLAY", MaddenPlayerFieldConstants.HairStyle, playerIndex, mMaddenPlayers[playerIndex].HairStyle);
				}
			}
		}

		private System.Collections.Generic.Dictionary<string, int> mPositionMap;

		private char[] SkinTone = 
			{
				'M'
				,'D'
				,'M'
				,'L'
				,'M'
				,'M'
				,'D'
				,'M'
				,'L'
				,'D'
				,'D'
				,'D'
				,'L'
				,'D'
				,'D'
				,'M'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'D'
				,'D'
				,'L'
				,'M'
				,'L'
				,'M'
				,'D'
				,'M'
				,'L'
				,'M'
				,'M'
				,'D'
				,'L'
				,'M'
				,'D'
				,'L'
				,'M'
				,'M'
				,'M'
				,'D'
				,'L'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'M'
				,'L'
				,'M'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'M'
				,'D'
				,'L'
				,'L'
				,'D'
				,'D'
				,'M'
				,'D'
				,'L'
				,'D'
				,'D'
				,'L'
				,'L'
				,'M'
				,'M'
				,'D'
				,'D'
				,'D'
				,'D'
				,'M'
				,'L'
				,'L'
				,'D'
				,'L'
				,'D'
				,'M'
				,'M'
				,'D'
				,'M'
				,'D'
				,'M'
				,'L'
				,'D'
				,'L'
				,'D'
				,'L'
				,'D'
				,'M'
				,'D'
				,'M'
				,'M'
				,'D'
				,'M'
				,'D'
				,'D'
				,'D'
				,'D'
				,'L'
				,'M'
				,'L'
				,'M'
				,'D'
				,'M'
				,'D'
				,'D'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'L'
				,'L'
				,'L'
				,'D'
				,'D'
				,'L'
				,'L'
				,'M'
				,'D'
				,'L'
				,'L'
				,'M'
				,'L'
				,'L'
				,'M'
				,'D'
				,'L'
				,'D'
				,'D'
				,'M'
				,'M'
				,'M'
				,'L'
				,'D'
				,'L'
				,'L'
				,'D'
				,'D'
				,'M'
				,'D'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'L'
				,'M'
				,'D'
				,'M'
				,'D'
				,'D'
				,'M'
				,'D'
				,'D'
				,'M'
				,'L'
				,'D'
				,'L'
				,'L'
				,'D'
				,'M'
				,'M'
				,'L'
				,'M'
				,'L'
				,'M'
				,'L'
				,'L'
				,'M'
				,'M'
				,'D'
				,'L'
				,'D'
				,'M'
				,'D'
				,'L'
				,'D'
				,'L'
				,'D'
				,'D'
				,'M'
				,'L'
				,'M'
				,'L'
				,'D'
				,'L'
				,'D'
				,'D'
				,'D'
				,'L'
				,'L'
				,'M'
				,'L'
				,'D'
				,'D'
				,'M'
				,'M'
				,'L'
				,'M'
				,'L'
				,'D'
				,'M'
				,'M'
				,'M'
				,'D'
				,'M'
				,'L'
				,'M'
				,'D'
				,'L'
				,'M'
				,'L'
				,'D'
				,'M'
				,'D'
				,'M'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'D'
				,'L'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'M'
				,'M'
				,'L'
				,'M'
				,'L'
				,'L'
				,'M'
				,'L'
				,'M'
				,'D'
				,'M'
				,'L'
				,'L'
				,'M'
				,'M'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'D'
				,'D'
				,'L'
				,'D'
				,'M'
				,'M'
				,'L'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'D'
				,'M'
				,'M'
				,'M'
				,'L'
				,'D'
				,'D'
				,'L'
				,'L'
				,'D'
				,'L'
				,'L'
				,'D'
				,'M'
				,'D'
				,'D'
				,'L'
				,'L'
				,'L'
				,'L'
				,'D'
				,'L'
				,'M'
				,'D'
				,'M'
				,'M'
				,'M'
				,'L'
				,'D'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'M'
				,'L'
				,'L'
				,'M'
				,'L'
				,'L'
				,'M'
				,'M'
				,'D'
				,'M'
				,'M'
				,'M'
				,'M'
				,'D'
				,'L'
				,'M'
				,'L'
				,'L'
				,'D'
				,'L'
				,'M'
				,'M'
				,'M'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'D'
				,'M'
				,'D'
				,'L'
				,'D'
				,'M'
				,'M'
				,'L'
				,'M'
				,'L'
				,'M'
				,'D'
				,'L'
				,'L'
				,'D'
				,'L'
				,'L'
				,'M'
				,'M'
				,'M'
				,'D'
				,'M'
				,'D'
				,'L'
				,'M'
				,'D'
				,'L'
				,'M'
				,'L'
				,'D'
				,'D'
				,'M'
				,'L'
				,'D'
				,'D'
				,'M'
				,'M'
				,'M'
				,'D'
				,'D'
				,'M'
				,'M'
				,'M'
				,'D'
				,'L'
				,'L'
				,'M'
				,'M'
				,'D'
				,'D'
				,'D'
				,'M'
				,'M'
				,'L'
				,'D'
				,'M'
				,'D'
				,'L'
				,'M'
				,'D'
				,'L'
				,'L'
				,'D'
				,'D'
				,'D'
				,'D'
				,'D'
				,'L'
				,'L'
				,'L'
				,'D'
				,'M'
				,'L'
				,'D'
				,'M'
				,'D'
				,'D'
				,'L'
				,'M'
				,'L'
				,'D'
				,'D'
				,'D'
				,'D'
				,'L'
				,'M'
				,'D'
				,'M'
				,'M'
				,'D'
				,'L'
				,'M'
				,'M'
				,'M'
				,'D'
				,'D'
				,'D'
				,'D'
				,'M'
				,'D'
				,'D'
				,'D'
				,'M'
				,'M'
				,'L'
				,'D'
				,'L'
				,'M'
				,'M'
				,'M'
				,'L'
				,'L'
				,'D'
				,'L'
				,'M'
				,'D'
			};

		private string[] PositionNames =
		{
            "QB"
            ,"RB"
            ,"FB"
            ,"WR"
            ,"TE"
            ,"LT"
            ,"LG"
            ,"C"
            ,"RG"
            ,"RT"
            ,"LDE"
            ,"RDE"
            ,"DT"
            ,"WLB"
            ,"MLB"
            ,"SLB"
            ,"CB"
            ,"FS"
            ,"SS"
            ,"K"
            ,"P"
		};

		private void BuildConversionTables()
		{
			mPositionMap = new Dictionary<string, int>();
            mPositionMap["QB"] = 0;
            mPositionMap["RB"] = 1;
            mPositionMap["FB"] = 2;
            mPositionMap["SE"] = 3;
            mPositionMap["FL"] = 3;
            mPositionMap["TE"] = 4;
            mPositionMap["LT"] = 5;
            mPositionMap["LG"] = 6;
            mPositionMap["C"] = 7;
            mPositionMap["RG"] = 8;
            mPositionMap["RT"] = 9;
            mPositionMap["LDE"] = 10;
            mPositionMap["RDE"] = 11;
            mPositionMap["LDT"] = 12;
            mPositionMap["RDT"] = 12;
            mPositionMap["NT"] = 12;
            mPositionMap["WLB"] = 13;
            mPositionMap["MLB"] = 14;
            mPositionMap["WILB"] = 14;
            mPositionMap["SILB"] = 14;
            mPositionMap["SLB"] = 15;
            mPositionMap["LCB"] = 16;
            mPositionMap["RCB"] = 16;
            mPositionMap["FS"] = 17;
            mPositionMap["SS"] = 18;
            mPositionMap["K"] = 19;
            mPositionMap["P"] = 20;
		}
	}
}
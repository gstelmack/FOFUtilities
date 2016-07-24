using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extender
{
	public partial class GameSelector : Form
	{
		private DataReader.UniverseData mUniverseData;
		private DataReader.UniverseData.SavedGameEntry mSelectedEntry;
		private bool mRunCareerReports = false;

		public DataReader.UniverseData.SavedGameEntry SelectedEntry { get { return mSelectedEntry; } }
		public bool RunCareerReports { get { return mRunCareerReports; } }

		public GameSelector(DataReader.UniverseData universeData)
		{
			InitializeComponent();
			mUniverseData = universeData;

			foreach (DataReader.UniverseData.SavedGameEntry entry in mUniverseData.SavedGames)
			{
				listBoxGames.Items.Add(entry.GameName);
			}
			checkBoxRunCareerReports.Checked = false;
			mRunCareerReports = false;
		}

		private void listBoxGames_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listBoxGames.SelectedIndex >= 0)
			{
				mSelectedEntry = mUniverseData.SavedGames[listBoxGames.SelectedIndex];
			}
		}

		private void checkBoxRunCareerReports_CheckedChanged(object sender, EventArgs e)
		{
			mRunCareerReports = checkBoxRunCareerReports.Checked;
		}
	}
}
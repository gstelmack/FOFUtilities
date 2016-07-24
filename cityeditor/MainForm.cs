using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DataReader;

namespace CityEditor
{
	public partial class MainForm : Form
	{
		private UniverseData mUniverseData;
		private bool mCityNameChanged;

		public MainForm()
		{
			InitializeComponent();

			mCityNameChanged = false;

			Assembly a = typeof(MainForm).Assembly;
			Text += " v" + a.GetName().Version;

			mUniverseData = new UniverseData();

			// Load Regions
			comboBoxRegion.Items.Add("Northeast");
			comboBoxRegion.Items.Add("Atlantic");
			comboBoxRegion.Items.Add("South");
			comboBoxRegion.Items.Add("Great Lakes");
			comboBoxRegion.Items.Add("Plains");
			comboBoxRegion.Items.Add("Mountain");
			comboBoxRegion.Items.Add("Southwest");
			comboBoxRegion.Items.Add("Northwest");
			comboBoxRegion.Items.Add("West Coast");

			// Load States
			comboBoxState.Items.Add("Alabama");
			comboBoxState.Items.Add("Alaska");
			comboBoxState.Items.Add("Arizona");
			comboBoxState.Items.Add("Arkansas");
			comboBoxState.Items.Add("California");
			comboBoxState.Items.Add("Colorado");
			comboBoxState.Items.Add("Connecticut");
			comboBoxState.Items.Add("Delaware");
			comboBoxState.Items.Add("Florida");
			comboBoxState.Items.Add("Georgia");
			comboBoxState.Items.Add("Hawaii");
			comboBoxState.Items.Add("Idaho");
			comboBoxState.Items.Add("Illinois");
			comboBoxState.Items.Add("Indiana");
			comboBoxState.Items.Add("Iowa");
			comboBoxState.Items.Add("Kansas");
			comboBoxState.Items.Add("Kentucky");
			comboBoxState.Items.Add("Louisiana");
			comboBoxState.Items.Add("Maine");
			comboBoxState.Items.Add("Maryland");
			comboBoxState.Items.Add("Massachusetts");
			comboBoxState.Items.Add("Michigan");
			comboBoxState.Items.Add("Minnesota");
			comboBoxState.Items.Add("Mississippi");
			comboBoxState.Items.Add("Missouri");
			comboBoxState.Items.Add("Montana");
			comboBoxState.Items.Add("Nebraska");
			comboBoxState.Items.Add("Nevada");
			comboBoxState.Items.Add("New Hampshire");
			comboBoxState.Items.Add("New Jersey");
			comboBoxState.Items.Add("New Mexico");
			comboBoxState.Items.Add("New York");
			comboBoxState.Items.Add("North Carolina");
			comboBoxState.Items.Add("North Dakota");
			comboBoxState.Items.Add("Ohio");
			comboBoxState.Items.Add("Oklahoma");
			comboBoxState.Items.Add("Oregon");
			comboBoxState.Items.Add("Pennsylvania");
			comboBoxState.Items.Add("Rhode Island");
			comboBoxState.Items.Add("South Carolina");
			comboBoxState.Items.Add("South Dakota");
			comboBoxState.Items.Add("Tennessee");
			comboBoxState.Items.Add("Texas");
			comboBoxState.Items.Add("Utah");
			comboBoxState.Items.Add("Vermont");
			comboBoxState.Items.Add("Virginia");
			comboBoxState.Items.Add("Washington");
			comboBoxState.Items.Add("West Virginia");
			comboBoxState.Items.Add("Wisconsin");
			comboBoxState.Items.Add("Wyoming");
			comboBoxState.Items.Add("District of Columbia");

			LoadCityList();
		}

		private void LoadCityList()
		{
			mCityNameChanged = false;

			comboBoxCity.Items.Clear();
			foreach (UniverseData.CityRecord curCity in mUniverseData.CityRecords)
			{
				comboBoxCity.Items.Add(curCity.Name);
			}

			comboBoxCity.SelectedIndex = 0;
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			labelStatus.Text = "Saving City Data Files...";
			labelStatus.Refresh();
			mUniverseData.SaveCityData(mCityNameChanged);
			labelStatus.Text = "Refreshing City List...";
			// Since the cities just got reordered...
			LoadCityList();
			labelStatus.Text = "Done!";
		}

		private void buttonMakeOrgBackups_Click(object sender, EventArgs e)
		{
			labelStatus.Text = "Backing up City Data Files...";
			labelStatus.Refresh();

			mUniverseData.MakeCityOrgFiles();

			labelStatus.Text = "Done!";
		}

		private void comboBoxCity_SelectedIndexChanged(object sender, EventArgs e)
		{
			int cityIndex = comboBoxCity.SelectedIndex;
			if (cityIndex >= 0 && cityIndex < mUniverseData.CityRecords.Length)
			{
				UniverseData.CityRecord rec = mUniverseData.CityRecords[cityIndex];
				textBoxName.Text = rec.Name;
				textBoxAbbreviation.Text = rec.Abbrev;
				textBoxPopulation.Text = (rec.Population * 1000).ToString();
				textBoxGrowthRate.Text = (((double)rec.GrowthRate - 100.0) / 10.0).ToString();
				textBoxAvgIncome.Text = (rec.AverageIncome * 100).ToString();
				textBoxPovertyLevel.Text = ((double)rec.PovertyLevel / 10.0).ToString();
				textBoxEntComp.Text = rec.EntertainmentCompetiton.ToString();
				textBoxSeptemberHigh.Text = rec.SeptemberHigh.ToString();
				textBoxSeptemberLow.Text = rec.SeptemberLow.ToString();
				textBoxSeptemberHumidity.Text = rec.SeptemberHumidity.ToString();
				textBoxDecemberHigh.Text = rec.DecemberHigh.ToString();
				textBoxDecemberLow.Text = rec.DecemberLow.ToString();
				textBoxDecemberHumidity.Text = rec.DecemberHumidity.ToString();
				textBox90DegreeDays.Text = rec.NinetyDegreeDays.ToString();
				textBoxSnowDays.Text = rec.SnowDays.ToString();
				textBoxStormyDays.Text = rec.StormyDays.ToString();
				textBoxElevation.Text = rec.Elevation.ToString();
				textBoxLongitude.Text = ((double)rec.Longitude / 100.0).ToString();
				textBoxLatitude.Text = ((double)rec.Latitude / 100.0).ToString();

				comboBoxRegion.SelectedIndex = rec.Region;
				comboBoxState.SelectedIndex = rec.State - 1;
			}
		}

		private void comboBoxRegion_SelectedIndexChanged(object sender, EventArgs e)
		{
			int regionIndex = comboBoxRegion.SelectedIndex;
			if (regionIndex >= 0)
			{
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Region = (short)regionIndex;
			}
		}

		private void comboBoxState_SelectedIndexChanged(object sender, EventArgs e)
		{
			int stateIndex = comboBoxState.SelectedIndex;
			if (stateIndex >= 0)
			{
				// 1 - 51
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].State = (short)(stateIndex + 1);
			}
		}

		private void textBoxName_Validating(object sender, CancelEventArgs e)
		{
			if (mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Name != textBoxName.Text)
			{
				mCityNameChanged = true;
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Name = textBoxName.Text;
			}
		}

		private void textBoxAbbreviation_Validating(object sender, CancelEventArgs e)
		{
			string abbrev = textBoxAbbreviation.Text;
			if (abbrev.Length == 3)
			{
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Abbrev = abbrev;
			}
			textBoxAbbreviation.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Abbrev;
		}

		private void textBoxPopulation_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxPopulation.Text);
				if (value >= 0)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Population = (short)(value / 1000);
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxPopulation.Text = (mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Population * 1000).ToString();
		}

		private void textBoxGrowthRate_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				Double value = Double.Parse(textBoxGrowthRate.Text);
				int intValue = (int)((value * 10.0) + 100.0);
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].GrowthRate = (short)intValue;
			}
			catch
			{
				// Do nothing
			}
			textBoxGrowthRate.Text = (((double)mUniverseData.CityRecords[comboBoxCity.SelectedIndex].GrowthRate - 100.0) / 10.0).ToString();
		}

		private void textBoxAvgIncome_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxAvgIncome.Text);
				if (value >= 0)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].AverageIncome = (short)(value / 100);
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxAvgIncome.Text = (mUniverseData.CityRecords[comboBoxCity.SelectedIndex].AverageIncome * 100).ToString();
		}

		private void textBoxPovertyLevel_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				Double value = Double.Parse(textBoxPovertyLevel.Text);
				if (value >= 0.0 && value <= 100.0)
				{
					int intValue = (int)(value * 10.0);
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].PovertyLevel = (short)intValue;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxPovertyLevel.Text = ((double)mUniverseData.CityRecords[comboBoxCity.SelectedIndex].PovertyLevel / 10.0).ToString();
		}

		private void textBoxEntComp_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxEntComp.Text);
				if (value >= 0 && value <= 100)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].EntertainmentCompetiton = (short)value;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxEntComp.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].EntertainmentCompetiton.ToString();
		}

		private void textBoxLatitude_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				Double value = Double.Parse(textBoxLatitude.Text);
				if (value >= 0.0 && value <= 180.0)
				{
					int intValue = (int)(value * 100.0);
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Latitude = (short)intValue;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxLatitude.Text = ((double)mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Latitude / 100.0).ToString();
		}

		private void textBoxLongitude_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				Double value = Double.Parse(textBoxLongitude.Text);
				if (value >= 0.0 && value <= 90.0)
				{
					int intValue = (int)(value * 100.0);
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Longitude = (short)intValue;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxLongitude.Text = ((double)mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Longitude / 100.0).ToString();
		}

		private void textBoxElevation_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxElevation.Text);
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Elevation = (short)value;
			}
			catch
			{
				// Do nothing
			}
			textBoxElevation.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].Elevation.ToString();
		}

		private void textBoxSeptemberHigh_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxSeptemberHigh.Text);
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SeptemberHigh = (short)value;
			}
			catch
			{
				// Do nothing
			}
			textBoxSeptemberHigh.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SeptemberHigh.ToString();
		}

		private void textBoxSeptemberLow_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxSeptemberLow.Text);
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SeptemberLow = (short)value;
			}
			catch
			{
				// Do nothing
			}
			textBoxSeptemberLow.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SeptemberLow.ToString();
		}

		private void textBoxSeptemberHumidity_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxSeptemberHumidity.Text);
				if (value >= 0 && value <= 100)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberHumidity = (short)value;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxSeptemberHumidity.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SeptemberHumidity.ToString();
		}

		private void textBoxDecemberHigh_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxDecemberHigh.Text);
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberHigh = (short)value;
			}
			catch
			{
				// Do nothing
			}
			textBoxDecemberHigh.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberHigh.ToString();
		}

		private void textBoxDecemberLow_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxDecemberLow.Text);
				mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberLow = (short)value;
			}
			catch
			{
				// Do nothing
			}
			textBoxDecemberLow.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberLow.ToString();
		}

		private void textBoxDecemberHumidity_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxDecemberHumidity.Text);
				if (value >= 0 && value <= 100)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberHumidity = (short)value;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxDecemberHumidity.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].DecemberHumidity.ToString();
		}

		private void textBox90DegreeDays_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBox90DegreeDays.Text);
				if (value >= 0 && value <= 365)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].NinetyDegreeDays = (short)value;
				}
			}
			catch
			{
				// Do nothing
			}
			textBox90DegreeDays.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].NinetyDegreeDays.ToString();
		}

		private void textBoxSnowDays_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxSnowDays.Text);
				if (value >= 0 && value <= 365)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SnowDays = (short)value;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxSnowDays.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].SnowDays.ToString();
		}

		private void textBoxStormyDays_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				int value = Int32.Parse(textBoxStormyDays.Text);
				if (value >= 0 && value <= 365)
				{
					mUniverseData.CityRecords[comboBoxCity.SelectedIndex].StormyDays = (short)value;
				}
			}
			catch
			{
				// Do nothing
			}
			textBoxStormyDays.Text = mUniverseData.CityRecords[comboBoxCity.SelectedIndex].StormyDays.ToString();
		}

		private void buttonCopyEconomics_Click(object sender, EventArgs e)
		{
			int cityIndex = comboBoxCity.SelectedIndex;
			if (cityIndex >= 0 && cityIndex < mUniverseData.CityRecords.Length)
			{
				UniverseData.CityRecord rec = mUniverseData.CityRecords[cityIndex];
				for (int i = 0; i < mUniverseData.CityRecords.Length; ++i)
				{
					if (i != cityIndex)
					{
						mUniverseData.CityRecords[i].Population = rec.Population;
						mUniverseData.CityRecords[i].GrowthRate = rec.GrowthRate;
						mUniverseData.CityRecords[i].AverageIncome = rec.AverageIncome;
						mUniverseData.CityRecords[i].PovertyLevel = rec.PovertyLevel;
						mUniverseData.CityRecords[i].EntertainmentCompetiton = rec.EntertainmentCompetiton;
					}
				}
			}
		}
	}
}
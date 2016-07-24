using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DraftAnalyzer
{
	public partial class WeightsForm : Form
	{
		private string mCurrentPosition;

		private string mAppFolderName;

		private TrackBar[] mAttributeTrackBars;
		private Label[] mAttributeLabels;

		private DataReader.DraftWeights mDraftWeights;
		private Dictionary<string, string[]> mPositionGroupAttributeNames;

		public DataReader.DraftWeights.PositionWeights GetPositionWeight(string position)
		{
			return mDraftWeights.GetPositionWeight(position);
		}
		public DataReader.DraftWeights.PositionWeights GetNoCombinePositionWeight(string position)
		{
			return mDraftWeights.GetNoCombinePositionWeight(position);
		}
		public DataReader.DraftWeights.GlobalWeightData GlobalWeights { get { return mDraftWeights.GlobalWeights; } }

		public WeightsForm(Dictionary<string, string[]> positionGroupAttributeNames)
		{
			InitializeComponent();

			mPositionGroupAttributeNames = positionGroupAttributeNames;

			mAttributeTrackBars = new TrackBar[DataReader.DraftWeights.MaxAttributeCounts];
			mAttributeTrackBars[0] = trackBarAttribute1;
			mAttributeTrackBars[1] = trackBarAttribute2;
			mAttributeTrackBars[2] = trackBarAttribute3;
			mAttributeTrackBars[3] = trackBarAttribute4;
			mAttributeTrackBars[4] = trackBarAttribute5;
			mAttributeTrackBars[5] = trackBarAttribute6;
			mAttributeTrackBars[6] = trackBarAttribute7;
			mAttributeTrackBars[7] = trackBarAttribute8;
			mAttributeTrackBars[8] = trackBarAttribute9;
			mAttributeTrackBars[9] = trackBarAttribute10;
			mAttributeTrackBars[10] = trackBarAttribute11;
			mAttributeTrackBars[11] = trackBarAttribute12;
			mAttributeTrackBars[12] = trackBarAttribute13;
			mAttributeTrackBars[13] = trackBarAttribute14;
			mAttributeTrackBars[14] = trackBarAttribute15;
			mAttributeLabels = new Label[DataReader.DraftWeights.MaxAttributeCounts];
			mAttributeLabels[0] = labelAttribute1;
			mAttributeLabels[1] = labelAttribute2;
			mAttributeLabels[2] = labelAttribute3;
			mAttributeLabels[3] = labelAttribute4;
			mAttributeLabels[4] = labelAttribute5;
			mAttributeLabels[5] = labelAttribute6;
			mAttributeLabels[6] = labelAttribute7;
			mAttributeLabels[7] = labelAttribute8;
			mAttributeLabels[8] = labelAttribute9;
			mAttributeLabels[9] = labelAttribute10;
			mAttributeLabels[10] = labelAttribute11;
			mAttributeLabels[11] = labelAttribute12;
			mAttributeLabels[12] = labelAttribute13;
			mAttributeLabels[13] = labelAttribute14;
			mAttributeLabels[14] = labelAttribute15;

			mDraftWeights = new DataReader.DraftWeights();

			foreach (string positionName in mDraftWeights.FOFData.PositionToPositionGroupMap.Keys)
			{
				comboBoxPosition.Items.Add(positionName);
			}
			mCurrentPosition = "QB";
			comboBoxPosition.Text = "QB";

			mAppFolderName = WindowsUtilities.OutputLocation.Get();

			LoadData();
			FillScreenFromPosition(mCurrentPosition);

			DisplayGlobalData();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			SaveScreenToPosition(mCurrentPosition);
			GrabGlobalData();

			mDraftWeights.UpdatePositionWeights();

			SaveData();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			mDraftWeights.Reset();
			LoadData();
		}

		private void buttonDefaults_Click(object sender, EventArgs e)
		{
			mDraftWeights.Reset();
			FillScreenFromPosition(mCurrentPosition);
			DisplayGlobalData();
		}

		private const string kWeightsDataFileName = "DraftAnalyzer.weights";

		private void LoadData()
		{
			string dataFileName = System.IO.Path.Combine(mAppFolderName, kWeightsDataFileName);
			try
			{
				mDraftWeights.LoadData(dataFileName);
				DisplayGlobalData();
			}
			catch (System.IO.IOException e)
			{
				MessageBox.Show("Error reading '" + dataFileName + "': " + e.ToString(), "Error Loading Weights File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SaveData()
		{
			string dataFileName = System.IO.Path.Combine(mAppFolderName, kWeightsDataFileName);
			mDraftWeights.SaveData(dataFileName);
		}

		private void GrabGlobalData()
		{
			if (radioButtonAttributesUseMin.Checked)
			{
				mDraftWeights.GlobalWeights.WhichAttributesToUse = DataReader.DraftWeights.AttributeUsage.UseMin;
			}
			else if (radioButtonAttributesUseAverage.Checked)
			{
				mDraftWeights.GlobalWeights.WhichAttributesToUse = DataReader.DraftWeights.AttributeUsage.UseAverage;
			}
			else if (radioButtonAttributesUseMax.Checked)
			{
				mDraftWeights.GlobalWeights.WhichAttributesToUse = DataReader.DraftWeights.AttributeUsage.UseMax;
			}

			mDraftWeights.GlobalWeights.Affinity = (int)numericUpDownAffinity.Value;
			mDraftWeights.GlobalWeights.Attributes = (int)numericUpDownAttributes.Value;
			mDraftWeights.GlobalWeights.Combines = (int)numericUpDownCombine.Value;
			mDraftWeights.GlobalWeights.Conflict = (int)numericUpDownConflict.Value;
			mDraftWeights.GlobalWeights.Height = (int)numericUpDownHeight.Value;
			mDraftWeights.GlobalWeights.RedFlag = (int)numericUpDownRedFlag.Value;
			mDraftWeights.GlobalWeights.ScoutImpression = (int)numericUpDownScoutImpression.Value;
			mDraftWeights.GlobalWeights.Weight = (int)numericUpDownWeight.Value;
			mDraftWeights.GlobalWeights.NoCombineAttributes = (int)numericUpDownNoCombineAttributes.Value;
			mDraftWeights.GlobalWeights.NoCombineCombines = (int)numericUpDownNoCombineCombine.Value;
			mDraftWeights.GlobalWeights.AvgDev = (int)numericUpDownAvgDev.Value;
			mDraftWeights.GlobalWeights.DevWt = (int)numericUpDownDevWt.Value;
			mDraftWeights.GlobalWeights.CombineThresholdPenalty = (int)numericUpDownCombineThreshold.Value;
		}

		private void DisplayGlobalData()
		{
			numericUpDownAttributes.Value = mDraftWeights.GlobalWeights.Attributes;
			numericUpDownCombine.Value = mDraftWeights.GlobalWeights.Combines;
			numericUpDownHeight.Value = mDraftWeights.GlobalWeights.Height;
			numericUpDownScoutImpression.Value = mDraftWeights.GlobalWeights.ScoutImpression;
			numericUpDownWeight.Value = mDraftWeights.GlobalWeights.Weight;
			numericUpDownAffinity.Value = mDraftWeights.GlobalWeights.Affinity;
			numericUpDownConflict.Value = mDraftWeights.GlobalWeights.Conflict;
			numericUpDownRedFlag.Value = mDraftWeights.GlobalWeights.RedFlag;
			numericUpDownNoCombineAttributes.Value = mDraftWeights.GlobalWeights.NoCombineAttributes;
			numericUpDownNoCombineCombine.Value = mDraftWeights.GlobalWeights.NoCombineCombines;
			numericUpDownAvgDev.Value = mDraftWeights.GlobalWeights.AvgDev;
			numericUpDownDevWt.Value = mDraftWeights.GlobalWeights.DevWt;
			numericUpDownCombineThreshold.Value = mDraftWeights.GlobalWeights.CombineThresholdPenalty;

			switch (mDraftWeights.GlobalWeights.WhichAttributesToUse)
			{
				case DataReader.DraftWeights.AttributeUsage.UseMin:
					radioButtonAttributesUseMin.Checked = true;
					break;
				case DataReader.DraftWeights.AttributeUsage.UseAverage:
					radioButtonAttributesUseAverage.Checked = true;
					break;
				case DataReader.DraftWeights.AttributeUsage.UseMax:
					radioButtonAttributesUseMax.Checked = true;
					break;
			}
		}

		private void buttonCopyToGroup_Click(object sender, EventArgs e)
		{
			SaveScreenToPosition(mCurrentPosition);
			DataReader.DraftWeights.PositionWeightInputs curInput = mDraftWeights.GetPositionWeightInputs(mCurrentPosition);
			string positionGroup = mDraftWeights.GetPositionGroup(mCurrentPosition);
			foreach (string clonePosition in mDraftWeights.FOFData.PositionToPositionGroupMap.Keys)
			{
				if (clonePosition != mCurrentPosition && mDraftWeights.GetPositionGroup(clonePosition) == positionGroup)
				{
					DataReader.DraftWeights.PositionWeightInputs weightsInput = mDraftWeights.GetPositionWeightInputs(clonePosition);
					weightsInput.Agility = curInput.Agility;
					curInput.Attributes.CopyTo(weightsInput.Attributes, 0);
					weightsInput.Bench = curInput.Bench;
					weightsInput.BroadJump = curInput.BroadJump;
					weightsInput.Dash = curInput.Dash;
					weightsInput.PositionDrill = curInput.PositionDrill;
					weightsInput.Solecismic = curInput.Solecismic;
					weightsInput.Weight = curInput.Weight;
				}
			}
		}

		private void comboBoxPosition_SelectedIndexChanged(object sender, EventArgs e)
		{
			SaveScreenToPosition(mCurrentPosition);
			mCurrentPosition = (string)comboBoxPosition.Items[comboBoxPosition.SelectedIndex];
			FillScreenFromPosition(mCurrentPosition);
		}

		private void SaveScreenToPosition(string position)
		{
			DataReader.DraftWeights.PositionWeightInputs weightInput = mDraftWeights.GetPositionWeightInputs(position);
			weightInput.Agility = trackBarAgility.Value;
			weightInput.Bench = trackBarBench.Value;
			weightInput.BroadJump = trackBarBroadJump.Value;
			weightInput.Dash = trackBarDash.Value;
			weightInput.PositionDrill = trackBarPositionDrill.Value;
			weightInput.Solecismic = trackBarSolecismic.Value;
			for (int i = 0; i < weightInput.Attributes.Length; ++i)
			{
				weightInput.Attributes[i] = mAttributeTrackBars[i].Value;
			}
			Double.TryParse(textBoxPositionWeight.Text, out weightInput.Weight);
		}

		private void FillScreenFromPosition(string position)
		{
			string positionGroup = mDraftWeights.GetPositionGroup(position);
			DataReader.DraftWeights.PositionWeightInputs weightInput = mDraftWeights.GetPositionWeightInputs(position);
			trackBarAgility.Value = weightInput.Agility;
			trackBarBench.Value = weightInput.Bench;
			trackBarBroadJump.Value = weightInput.BroadJump;
			trackBarDash.Value = weightInput.Dash;
			if (positionGroup == "T" || positionGroup == "G" || positionGroup == "C" || positionGroup == "DE" || positionGroup == "DT")
			{
				trackBarPositionDrill.Value = 0;
				trackBarPositionDrill.Enabled = false;
			}
			else
			{
				trackBarPositionDrill.Value = weightInput.PositionDrill;
				trackBarPositionDrill.Enabled = true;
			}
			trackBarSolecismic.Value = weightInput.Solecismic;
			textBoxPositionWeight.Text = weightInput.Weight.ToString("F3");
			string[] attributeNames = mPositionGroupAttributeNames[positionGroup];
			for (int i = 0; i < weightInput.Attributes.Length; ++i)
			{
				if (i >= attributeNames.Length)
				{
					mAttributeLabels[i].Text = "Unused";
					mAttributeTrackBars[i].Value = 0;
					mAttributeTrackBars[i].Enabled = false;
					mAttributeLabels[i].Enabled = false;
				}
				else
				{
					mAttributeTrackBars[i].Enabled = true;
					mAttributeLabels[i].Enabled = true;
					mAttributeLabels[i].Text = attributeNames[i];
					mAttributeTrackBars[i].Value = weightInput.Attributes[i];
				}
			}
		}

		private void buttonSaveAs_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Weight Files (*.weights)|*.weights|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			dlg.InitialDirectory = WindowsUtilities.OutputLocation.Get();
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				SaveScreenToPosition(mCurrentPosition);
				GrabGlobalData();
				mDraftWeights.SaveData(dlg.FileName);
			}
		}

		private void buttonLoad_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Weight Files (*.weights)|*.weights|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			dlg.InitialDirectory = WindowsUtilities.OutputLocation.Get();
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				mDraftWeights.LoadData(dlg.FileName);
				FillScreenFromPosition(mCurrentPosition);
				DisplayGlobalData();
			}
		}

		private void buttonFOFDefaults_Click(object sender, EventArgs e)
		{
			mDraftWeights.SetFOFDefaultWeights();
			FillScreenFromPosition(mCurrentPosition);
		}

		private void buttonAllOnes_Click(object sender, EventArgs e)
		{
			mDraftWeights.SetAllOnesWeights();
			FillScreenFromPosition(mCurrentPosition);
		}
	}
}
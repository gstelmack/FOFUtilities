using DataReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DraftAnalyzer
{
	public partial class DraftAnalyzerForm : Form
	{
		private const int kMaxAttributeCounts = 15;
		private System.Windows.Forms.Label[] mAttributeLabels = new System.Windows.Forms.Label[kMaxAttributeCounts];
		private System.Windows.Forms.PictureBox[] mAttributePictureBoxes = new System.Windows.Forms.PictureBox[kMaxAttributeCounts];
		private WeightsForm mWeightsForm;
		private ChemistryForm mChemistryForm;
		private int mAttributeSortColumn = 0;

		private enum CombineDisplayType
		{
			DisplayValues,
			DisplayStdDevs,
			DisplayRatings
		}

		CombineDisplayType mCombineDisplayType = CombineDisplayType.DisplayValues;

		private enum DraftPosition
		{
			FirstRound = 0,
			SecondRound,
			ThirdRound,
			FourthRound,
			FifthRound,
			SixthRound,
			SeventhRound,
			UndraftedFA,
			DontDraft,
			NotSet,

			DraftPositionCount
		}
		private string[] mDraftRoundNameMap = new string[]
			{
				"1",
				"2",
				"3",
				"4",
				"5",
				"6",
				"7",
				"FA",
				"X",
				""
			};

		private class PositionRating
		{
			public string Position;
			public double CombineScore;
			public double AttributeScore;
			public double SizeScore;
			public double ScoutImpressionScore;
			public double ChemistryScore;
			public double OverallScore;
			public double DevelopmentScore;
			public double SolecismicRating;
			public double FortyRating;
			public double AgilityRating;
			public double BenchRating;
			public double BroadJumpRating;
			public double PositionDrillRating;
		}

		private class PlayerData
		{
			public string mName;
			public string mPosition;
			public string mPositionGroup;
			public string mCollege;
			public string mBirthDate;
			public int mHeight;
			public int mWeight;
			public int mSolecismic;
			public double m40Time;
			public int mBench;
			public double mAgility;
			public int mBroadJump;
			public int mPositionDrill;
			public int mPercentDeveloped;
			public double mGrade;
			public string mInterviewed;
			public string mConflicts;
			public string mAffinities;
			public int mFormations;
			public int[] mAttributes = new int[kMaxAttributeCounts*2];
			public int[] mCombinePrediction = new int[kMaxAttributeCounts];
			public int mOriginalOrder;
			public int mDesiredOrder;
			public bool mDrafted;
			public bool mMarked;
			public DraftPosition mDraftPosition;
			public int mOrderDrafted;
			public ListViewItem mItem;

			public string mRatedPosition;
			public double mCombineSum;
			public double mRating;
			public double mDevelopmentRating;
			public double mChemistryRating;
			public double mScoutImpressionRating;

			public double mSolecismicRating;
			public double mFortyRating;
			public double mBenchRating;
			public double mAgilityRating;
			public double mBroadJumpRating;
			public double mPositionDrillRating;

			public double mAffinitiesFactor = 1.0;
			public double mConflictsFactor = 1.0;

			public ChemistryUtilities.AstrologicalSign mAstrologicalSign;

			public List<PositionRating> mPositionRatings = new List<PositionRating>();
		}

		private class PositionGroupCombineData
		{
			public double mSolecismicAverage;
			public double mSolecismicStdDev;
			public double mSolecismicThreshold;
			public double m40YardAverage;
			public double m40YardStdDev;
			public double m40YardThreshold;
			public double mBenchAverage;
			public double mBenchStdDev;
			public double mBenchThreshold;
			public double mAgilityAverage;
			public double mAgilityStdDev;
			public double mAgilityThreshold;
			public double mBroadJumpAverage;
			public double mBroadJumpStdDev;
			public double mBroadJumpThreshold;
			public double mPositionDrillAverage;
			public double mPositionDrillStdDev;
			public double mPositionDrillThreshold;
		}

		private class PositionSizeRanges
		{
			public int MinWeight;
			public int WellBelowAverageWeightCap;
			public int BelowAverageWeightCap;
			public int AverageWeightCap;
			public int AboveAverageWeightCap;
			public int WellAboveAverageWeightCap;

			public int MinHeight;
			// 2 inches to either side is Above/Below Average.
			// Everything else is Significantly Above/Below.
			public int AverageHeight;
		}

		private System.Collections.Hashtable mPositionGroupCombineMap = null;
		private System.Collections.Generic.Dictionary<string, int> mPositionGroupOrderMap = null;
		private System.Collections.Generic.SortedList<int, int> mDraftOrderList = null;	// Draft order is key, player data index is value.
		private System.Collections.Generic.Dictionary<string, string> mPositionToPositionGroupMap = null;
		private System.Collections.Generic.Dictionary<string, PositionSizeRanges> mPositionSizeRangesMap = null;
		private Dictionary<string, double> m_PositionWeightsDefaultMap = new Dictionary<string, double>();

		private System.Collections.ArrayList mPlayerData = null;

		private WindowsUtilities.XMLSettings mSettings;
		private const string kSettingsRoot = "DraftAnalyzer";
		private const string kSortDraftedToBottom = "SortDraftedToBottom";
		private const string kColorChemistryGroups = "ColorChemistryGroups";

		public DraftAnalyzerForm()
		{
			InitializeComponent();
			listViewDraftees.RaiseKeyDataEvent += listViewDraftees_KeyDataEvent;
			listViewDraftees.MakeDoubleBuffered();

			toolStripStatusLabelAction.Text = "";
			mCombineDisplayType = CombineDisplayType.DisplayValues;
			showStdDevsToolStripMenuItem.Checked = false;
			showCombineScoresToolStripMenuItem.Checked = false;
			showCombineValuesToolStripMenuItem.Checked = true;

            Assembly a = typeof(DraftAnalyzerForm).Assembly;
            Text += " v" + a.GetName().Version;

            InitializeMaps();
			InitializeSorters();

			mAttributeSortColumn = listViewDraftees.Columns.Count;

			foreach (string newItem in mAllAttributeNames)
			{
				ToolStripMenuItem newMenuItem = new ToolStripMenuItem(newItem);
				newMenuItem.ToolTipText = "Sort by " + newItem + " bar";
				newMenuItem.Click += new System.EventHandler(this.sortyByAttribute_Click);
				sortByToolStripMenuItem.DropDownItems.Add(newMenuItem);
			}

			mWeightsForm = new WeightsForm(mPositionGroupAttributeNames);
			mChemistryForm = new ChemistryForm();

			string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftAnalyzer.ini");
			mSettings = new WindowsUtilities.XMLSettings(settingsPath);
			sortDraftedToBottomToolStripMenuItem.Checked = mSettings.ReadXMLbool(kSettingsRoot, kSortDraftedToBottom, false);
			colorChemistryGroupsToolStripMenuItem.Checked = mSettings.ReadXMLbool(kSettingsRoot, kColorChemistryGroups, true);

			DisplayPlayerDetails(null);
		}

		private void sortyByAttribute_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem senderItem = sender as ToolStripMenuItem;
			string attribute = senderItem.Text;
			foreach (PlayerData data in mPlayerData)
			{
				string[] posAttributes = mPositionGroupAttributeNames[data.mPositionGroup];
				int attributeValue = 0;
				for (int i = 0; i < posAttributes.Length;++i)
				{
					if (posAttributes[i].StartsWith(attribute))
					{
						attributeValue = (data.mAttributes[i * 2] + data.mAttributes[(i * 2) + 1]) / 2;
						break;
					}
				}
				data.mItem.SubItems[mAttributeSortColumn].Tag = attributeValue;
			}
			bool descending = true;
			WindowsUtilities.SortTypeListViewItemSorter.UpdateSortSpecific(listViewDraftees, mAttributeSortColumn, sortDraftedToBottomToolStripMenuItem.Checked,
				WindowsUtilities.SortType.SortByRating, descending);
		}

		private void loadExtractorOutputToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.ShowNewFolderButton = false;
			dlg.Description = "Select the League";
			string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			dlg.RootFolder = Environment.SpecialFolder.LocalApplicationData;
			dlg.SelectedPath = System.IO.Path.Combine(appData, "Solecismic Software", "Front Office Football Eight", "leaguedata");
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				UnselectPlayer();
				LoadExportedDraftees(dlg.SelectedPath);
				DisplayPlayerData();
				saveDraftListToolStripMenuItem.Enabled = true;
			}
		}

		private void loadDraftListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Draft Lists (*.draft)|*.draft|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				UnselectPlayer();
				LoadDraftListFile(dlg.FileName);
				DisplayPlayerData();
				saveDraftListToolStripMenuItem.Enabled = true;
			}
		}

		private void saveDraftListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Draft Lists (*.draft)|*.draft|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				SaveDraftListFile(dlg.FileName);
			}
		}

		private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "CSV Exports (*.csv)|*.csv|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				using (System.IO.StreamWriter outFile = new System.IO.StreamWriter(dlg.FileName))
				{
					outFile.WriteLine("Name,Position,PositionGroup,College,Birthdate,Height,Weight," +
						"HeightBand,WeightBand,"+
						"Solecismic,40Time,Bench,Agility,BroadJump,PosDrill,Developed,Grade,Interviewed,Conflicts," +
						"Affinities,Formations,OrgOrder,DesiredOrder,Drafted,DraftedOrder,Marked,DraftPosition," +
						"RatedPosition,CombineSum,Rating");
					foreach(PlayerData curData in mPlayerData)
					{
						SizeBands heightBand = GetHeightBandIndex(curData.mPosition, curData.mHeight);
						SizeBands weightBand = GetWeightBandIndex(curData.mPosition, curData.mWeight);
						outFile.Write("\"" + curData.mName + "\",");
						outFile.Write("\"" + curData.mPosition + "\",");
						outFile.Write("\"" + curData.mPositionGroup + "\",");
						outFile.Write("\"" + curData.mCollege + "\",");
						outFile.Write("\"" + curData.mBirthDate + "\",");
						outFile.Write(curData.mHeight + ",");
						outFile.Write(curData.mWeight + ",");
						outFile.Write("\"" + kHeightBands[(int)heightBand] + "\",");
						outFile.Write("\"" + kWeightBands[(int)weightBand] + "\",");
						outFile.Write(curData.mSolecismic + ",");
						outFile.Write(curData.m40Time.ToString("F2") + ",");
						outFile.Write(curData.mBench + ",");
						outFile.Write(curData.mAgility.ToString("F2") + ",");
						outFile.Write(curData.mBroadJump + ",");
						outFile.Write(curData.mPositionDrill + ",");
						outFile.Write(curData.mPercentDeveloped + ",");
						outFile.Write(curData.mGrade.ToString("F1") + ",");
						outFile.Write("\"" + curData.mInterviewed + "\",");
						outFile.Write("\"" + curData.mConflicts + "\",");
						outFile.Write("\"" + curData.mAffinities + "\",");
						outFile.Write(curData.mFormations + ",");
						outFile.Write(curData.mOriginalOrder + ",");
						outFile.Write(curData.mDesiredOrder + ",");
						outFile.Write(curData.mDrafted + ",");
						outFile.Write(curData.mOrderDrafted + ",");
						outFile.Write(curData.mMarked + ",");
						outFile.Write("\"" + mDraftRoundNameMap[(int)curData.mDraftPosition] + "\",");

						outFile.Write("\"" + curData.mRatedPosition + "\",");
						outFile.Write(curData.mCombineSum.ToString("F2") + ",");
						outFile.WriteLine(curData.mRating.ToString("F2"));
					}
				}
			}
		}

		private void ColorCombineSum(ListViewItem.ListViewSubItem subItem, double score)
		{
			double bigDiff = ((double)mWeightsForm.GlobalWeights.Combines) * 0.75;
			double smallDiff = ((double)mWeightsForm.GlobalWeights.Combines) * 0.375;

			if (score == 0.0)
			{
				subItem.BackColor = Color.Orange;
			}
			else
			{
				if (score < -bigDiff)
				{
					subItem.BackColor = Color.Blue;
					subItem.ForeColor = Color.White;
				}
				else if (score < -smallDiff)
				{
					subItem.BackColor = Color.DarkBlue;
					subItem.ForeColor = Color.White;
				}
				else if (score < 0.00)
				{
					subItem.BackColor = Color.LightGray;
					subItem.ForeColor = Color.Black;
				}
				else if (score > bigDiff)
				{
					subItem.BackColor = Color.LimeGreen;
					subItem.ForeColor = Color.White;
				}
				else if (score > smallDiff)
				{
					subItem.BackColor = Color.ForestGreen;
					subItem.ForeColor = Color.White;
				}
			}
		}

		private void ColorCombineRating(ListViewItem.ListViewSubItem subItem, double score, double avg, double stdDev, double threshold)
		{
			if (score == 0.0)
			{
				subItem.BackColor = Color.Orange;
				subItem.ForeColor = Color.Black;
			}
			else
			{
				double diff = score - avg;
				double stdDevs = diff / stdDev;
				if (stdDevs < -1.50)
				{
					subItem.BackColor = Color.Blue;
					subItem.ForeColor = Color.White;
				}
				else if (stdDevs < -1.00)
				{
					subItem.BackColor = Color.DarkBlue;
					subItem.ForeColor = Color.White;
				}
				else if (stdDevs < 0.00)
				{
					subItem.BackColor = Color.LightGray;
					subItem.ForeColor = Color.Black;
				}
				else if (stdDevs > 1.50)
				{
					subItem.BackColor = Color.LimeGreen;
					subItem.ForeColor = Color.White;
				}
				else if (stdDevs > 1.00)
				{
					subItem.BackColor = Color.ForestGreen;
					subItem.ForeColor = Color.White;
				}
				else
				{
					subItem.BackColor = Color.White;
					subItem.ForeColor = Color.Black;
				}
				if (mCombineDisplayType == CombineDisplayType.DisplayStdDevs)
				{
					subItem.Text = stdDevs.ToString("F2");
				}
				if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && threshold != 0)
				{
					if ((stdDev < 0 && score > threshold) || (stdDev > 0 && score < threshold))
					{
						subItem.BackColor = Color.Red;
						subItem.ForeColor = Color.Black;
					}
				}
			}
		}

		private void DisplayPlayerData(int i)
		{
			string stage = "";
			PlayerData data = (PlayerData)mPlayerData[i];

			try
			{
				stage = "CalculatePlayerData";
				CalculatePlayerData(data);
				ListViewItem item = data.mItem;
				Color foreColor = SystemColors.ControlText;
				Color backColor = listViewDraftees.BackColor;
				item.Checked = false;
				if (data.mDrafted)
				{
					foreColor = Color.Black;
					backColor = Color.Red;
					item.Checked = true;
				}
				else if (data.mMarked)
				{
					foreColor = Color.Black;
					backColor = Color.Yellow;
				}
				else if (item.Selected)
				{
					foreColor = SystemColors.HighlightText;
					backColor = SystemColors.Highlight;
				}
				else if (data.mConflicts.Length > 0)
				{
					foreColor = Color.Red;
				}
				else if (data.mAffinities.Length > 0)
				{
					foreColor = Color.Green;
				}
				else if (colorChemistryGroupsToolStripMenuItem.Checked)
				{
					foreColor = Color.Black;
					switch (data.mAstrologicalSign)
					{
						case ChemistryUtilities.AstrologicalSign.Aquarius:
						case ChemistryUtilities.AstrologicalSign.Libra:
						case ChemistryUtilities.AstrologicalSign.Capricorn:
							backColor = Color.FromArgb(255, 170, 170);
							break;

						case ChemistryUtilities.AstrologicalSign.Pisces:
						case ChemistryUtilities.AstrologicalSign.Taurus:
						case ChemistryUtilities.AstrologicalSign.Cancer:
							backColor = Color.FromArgb(255, 255, 170);
							break;

						case ChemistryUtilities.AstrologicalSign.Aries:
						case ChemistryUtilities.AstrologicalSign.Gemini:
						case ChemistryUtilities.AstrologicalSign.Scorpio:
							backColor = Color.FromArgb(170, 170, 255);
							break;

						case ChemistryUtilities.AstrologicalSign.Leo:
						case ChemistryUtilities.AstrologicalSign.Virgo:
						case ChemistryUtilities.AstrologicalSign.Sagittarius:
							backColor = Color.FromArgb(170, 255, 170);
							break;
					}
				}
				item.SubItems.Clear();
				item.Text = data.mName;
				stage = "CombineData Retrieval";
				PositionGroupCombineData combineData = (PositionGroupCombineData)mPositionGroupCombineMap[data.mPositionGroup];
				item.BackColor = backColor;
				item.ForeColor = foreColor;
				item.UseItemStyleForSubItems = false;
				stage = "Add Position";
				ListViewItem.ListViewSubItem subItem = item.SubItems.Add(data.mPosition);
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Add PositionGroup";
				subItem.Tag = mPositionGroupOrderMap[data.mPositionGroup];
				subItem = item.SubItems.Add(data.mGrade.ToString("F1"));
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Solecismic Rating";
				if (mCombineDisplayType == CombineDisplayType.DisplayValues)
				{
					subItem = item.SubItems.Add(data.mSolecismic.ToString());
				}
				else
				{
					subItem = item.SubItems.Add(data.mSolecismicRating.ToString("F2"));
				}
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color Solecismic Rating";
				ColorCombineRating(subItem, (double)data.mSolecismic, combineData.mSolecismicAverage, combineData.mSolecismicStdDev, combineData.mSolecismicThreshold);
				stage = "40 Time";
				if (mCombineDisplayType == CombineDisplayType.DisplayValues)
				{
					subItem = item.SubItems.Add(data.m40Time.ToString("F2"));
				}
				else
				{
					subItem = item.SubItems.Add(data.mFortyRating.ToString("F2"));
				}
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color 40 Time";
				ColorCombineRating(subItem, (double)data.m40Time, combineData.m40YardAverage, combineData.m40YardStdDev, combineData.m40YardThreshold);
				stage = "Bench";
				if (mCombineDisplayType == CombineDisplayType.DisplayValues)
				{
					subItem = item.SubItems.Add(data.mBench.ToString());
				}
				else
				{
					subItem = item.SubItems.Add(data.mBenchRating.ToString("F2"));
				}
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color Bench";
				ColorCombineRating(subItem, (double)data.mBench, combineData.mBenchAverage, combineData.mBenchStdDev, combineData.mBenchThreshold);
				stage = "Agility";
				if (mCombineDisplayType == CombineDisplayType.DisplayValues)
				{
					subItem = item.SubItems.Add(data.mAgility.ToString("F2"));
				}
				else
				{
					subItem = item.SubItems.Add(data.mAgilityRating.ToString("F2"));
				}
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color Agility";
				ColorCombineRating(subItem, (double)data.mAgility, combineData.mAgilityAverage, combineData.mAgilityStdDev, combineData.mAgilityThreshold);
				stage = "Broad Jump";
				if (mCombineDisplayType == CombineDisplayType.DisplayValues)
				{
					subItem = item.SubItems.Add(data.mBroadJump.ToString());
				}
				else
				{
					subItem = item.SubItems.Add(data.mBroadJumpRating.ToString("F2"));
				}
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color Broad Jump";
				ColorCombineRating(subItem, (double)data.mBroadJump, combineData.mBroadJumpAverage, combineData.mBroadJumpStdDev, combineData.mBroadJumpThreshold);
				stage = "Position Drill";
				if (mCombineDisplayType == CombineDisplayType.DisplayValues)
				{
					subItem = item.SubItems.Add(data.mPositionDrill.ToString());
				}
				else
				{
					subItem = item.SubItems.Add(data.mPositionDrillRating.ToString("F2"));
				}
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color Position Drill";
				ColorCombineRating(subItem, (double)data.mPositionDrill, combineData.mPositionDrillAverage, combineData.mPositionDrillStdDev, combineData.mPositionDrillThreshold);
				stage = "Percent Developed";
				subItem = item.SubItems.Add(data.mPercentDeveloped.ToString());
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Attribute Score";
				subItem = item.SubItems.Add(data.mPositionRatings[0].AttributeScore.ToString("F1"));
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Combine Sum";
				subItem = item.SubItems.Add(data.mCombineSum.ToString("F2"));
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Color Combine Sum";
				ColorCombineSum(subItem, data.mCombineSum);
				stage = "Rating";
				subItem = item.SubItems.Add(data.mRating.ToString("F1"));
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "Desired Order";
				subItem = item.SubItems.Add(data.mDesiredOrder.ToString());
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;
				stage = "DraftRoundMap";
				subItem = item.SubItems.Add(mDraftRoundNameMap[(int)data.mDraftPosition]);
				subItem.BackColor = backColor;
				subItem.ForeColor = foreColor;

				// Add an extra column for attribute sorting
				subItem = item.SubItems.Add("0");

				item.Tag = data;
			}
			catch (Exception e)
			{
				MessageBox.Show("Error displaying player '" + data.mName + "' during stage " + stage + ": " + e.ToString(), "Error Displaying Player",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void CopyPosRatingToPlayerData(PositionRating posRating, PlayerData data)
		{
			data.mCombineSum = posRating.CombineScore;
			data.mRating = posRating.OverallScore;
			data.mRatedPosition = posRating.Position;
			data.mSolecismicRating = posRating.SolecismicRating;
			data.mFortyRating = posRating.FortyRating;
			data.mBenchRating = posRating.BenchRating;
			data.mBroadJumpRating = posRating.BroadJumpRating;
			data.mPositionDrillRating = posRating.PositionDrillRating;
			data.mAgilityRating = posRating.AgilityRating;
			data.mDevelopmentRating = posRating.DevelopmentScore;
			data.mChemistryRating = posRating.ChemistryScore;
			data.mScoutImpressionRating = posRating.ScoutImpressionScore;
		}

		private void CalculatePlayerData(PlayerData data)
		{
			data.mPositionRatings = new List<PositionRating>();
			PositionRating posRating = CalculatePositionRating(data, data.mPosition);
			data.mPositionRatings.Add(posRating);
			CopyPosRatingToPlayerData(posRating, data);
			string positionGroup = mPositionToPositionGroupMap[data.mPosition];
			foreach (string clonePosition in mPositionToPositionGroupMap.Keys)
			{
				if (clonePosition != data.mPosition && mPositionToPositionGroupMap[clonePosition] == positionGroup)
				{
					posRating = CalculatePositionRating(data, clonePosition);
					data.mPositionRatings.Add(posRating);
					if (posRating.OverallScore > data.mRating)
					{
						CopyPosRatingToPlayerData(posRating, data);
					}
				}
			}

			PredictAttributesFromCombines(data);

			ChemistryUtilities.Birthday bDay = ChemistryUtilities.Birthday.Parse(data.mBirthDate);
			data.mAstrologicalSign = ChemistryUtilities.GetSign(bDay);
		}

		#region PredictAttributes
		private void PredictAttributesFromCombines(PlayerData data)
		{
			if (data.mPositionGroup == "QB")
			{
				PredictQBAttributes(data);
			}
			else if (data.mPositionGroup == "RB")
			{
				PredictRBAttributes(data);
			}
			else if (data.mPositionGroup == "FB")
			{
				PredictFBAttributes(data);
			}
			else if (data.mPositionGroup == "WR")
			{
				PredictWRAttributes(data);
			}
			else if (data.mPositionGroup == "TE")
			{
				PredictTEAttributes(data);
			}
			else if (data.mPositionGroup == "T")
			{
				PredictOLAttributes(data);
			}
			else if (data.mPositionGroup == "G")
			{
				PredictOLAttributes(data);
			}
			else if (data.mPositionGroup == "C")
			{
				PredictOLAttributes(data);
			}
			else if (data.mPositionGroup == "DE")
			{
				PredictDLAttributes(data);
			}
			else if (data.mPositionGroup == "DT")
			{
				PredictDLAttributes(data);
			}
			else if (data.mPositionGroup == "OLB")
			{
				PredictLBAttributes(data);
			}
			else if (data.mPositionGroup == "ILB")
			{
				PredictLBAttributes(data);
			}
			else if (data.mPositionGroup == "CB")
			{
				PredictDBAttributes(data);
			}
			else if (data.mPositionGroup == "S")
			{
				PredictDBAttributes(data);
			}
			else if (data.mPositionGroup == "P")
			{
				PredictPAttributes(data);
			}
			else if (data.mPositionGroup == "K")
			{
				PredictKAttributes(data);
			}
		}

		void CalculateCombineStdDevs(PlayerData data, out double sole, out double forty, out double bench, out double agility, out double bj, out double posDrill)
		{
			sole = 0.0;
			forty = 0.0;
			bench = 0.0;
			agility = 0.0;
			bj = 0.0;
			posDrill = 0.0;

			PositionGroupCombineData combineData = (PositionGroupCombineData)mPositionGroupCombineMap[data.mPositionGroup];
			if (data.mSolecismic != 0 && data.m40Time != 0.0 && data.mBench != 0 && data.mAgility != 0.0 && data.mBroadJump != 0)
			{
				sole = ((((double)data.mSolecismic) - combineData.mSolecismicAverage) / combineData.mSolecismicStdDev);
				forty = ((((double)data.m40Time) - combineData.m40YardAverage) / combineData.m40YardStdDev);
				bench = ((((double)data.mBench) - combineData.mBenchAverage) / combineData.mBenchStdDev);
				agility = ((((double)data.mAgility) - combineData.mAgilityAverage) / combineData.mAgilityStdDev);
				bj = ((((double)data.mBroadJump) - combineData.mBroadJumpAverage) / combineData.mBroadJumpStdDev);
				if (combineData.mPositionDrillStdDev != 0.0)
				{
					posDrill += ((((double)data.mPositionDrill) - combineData.mPositionDrillAverage) / combineData.mPositionDrillStdDev);
				}
			}
		}

		int GeneratePredictedScore(double combine)
		{
			int score = 0;
			if (combine >= 0.0)
			{
				score = 35 + (int)Math.Floor((combine / 6.0) * 65.0);
				if (score > 97)
				{
					score = 97;
				}
			}
			else
			{
				score = 35 + (int)Math.Floor((combine / 6.0) * 35.0);
				if (score < 3)
				{
					score = 3;
				}
			}
			return score;
		}

		void PredictQBAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data,out sole,out forty,out bench,out agility,out broadjump,out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(agility);   // "Screen Passes"
			data.mCombinePrediction[1] = -1;                                // "Short Passes",
			data.mCombinePrediction[2] = GeneratePredictedScore(broadjump); // "Medium Passes",
			data.mCombinePrediction[3] = GeneratePredictedScore(bench);	    // "Long Passes",
			data.mCombinePrediction[4] = GeneratePredictedScore(bench); 	// "Deep Passes",
			data.mCombinePrediction[5] = GeneratePredictedScore(broadjump);	// "Third Down Passing",
			data.mCombinePrediction[6] = GeneratePredictedScore(posDrill);  // "Accuracy",
			data.mCombinePrediction[7] = GeneratePredictedScore(posDrill);	// "Timing",
			data.mCombinePrediction[8] = GeneratePredictedScore(agility);	// "Sense Rush",
			data.mCombinePrediction[9] = GeneratePredictedScore(sole);  	// "Read Defense",
			data.mCombinePrediction[10] = -1;                           	// "Two Minute Offense",
			data.mCombinePrediction[11] = GeneratePredictedScore(forty);	// "Scramble Frequency",
			data.mCombinePrediction[12] = -1;           	                // "Kick Holding"
		}

		void PredictRBAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data,out sole,out forty,out bench,out agility,out broadjump,out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(forty);     // 	"Breakaway Speed (Ft80)",
			data.mCombinePrediction[1] = GeneratePredictedScore(bench);     // 	"Power Inside (Bp100)",
			data.mCombinePrediction[2] = GeneratePredictedScore(agility);   // 	"Third Down Running (Ag33)",
			data.mCombinePrediction[3] = GeneratePredictedScore(sole);	    // 	"Hole Recognition (So90)",
			data.mCombinePrediction[4] = GeneratePredictedScore(agility); 	// 	"Elusiveness (Ag33)",
			data.mCombinePrediction[5] = GeneratePredictedScore(((broadjump*5.0)+(forty*2.0))/7.0);	// 	"Speed to Outside (Bj50/Ft20)",
			data.mCombinePrediction[6] = GeneratePredictedScore(posDrill);  // 	"Blitz Pickup (PD90)",
			data.mCombinePrediction[7] = -1;	                            // 	"Avoid Drops",
			data.mCombinePrediction[8] = GeneratePredictedScore(agility);	// 	"Getting Downfield (Ag33)",
			data.mCombinePrediction[9] = -1;                                // 	"Route Running",
			data.mCombinePrediction[10] = GeneratePredictedScore(posDrill); // 	"Third Down Catching (PD05)",
			data.mCombinePrediction[11] = -1;                           	// 	"Punt Returns",
			data.mCombinePrediction[12] = -1;           	                // 	"Kick Returns",
			data.mCombinePrediction[13] = GeneratePredictedScore(broadjump);	// 	"Endurance (Bj50)",
			data.mCombinePrediction[14] = -1;           	                // 	"Special Teams"
		}

		void PredictFBAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(broadjump); // 	"Run Blocking",
			data.mCombinePrediction[1] = -1;                                // 	"Pass Blocking",
			data.mCombinePrediction[2] = -1;                                // 	"Blocking Strength",
			data.mCombinePrediction[3] = GeneratePredictedScore(bench);	    // 	"Power Inside",
			data.mCombinePrediction[4] = GeneratePredictedScore(((broadjump*3.0)+(agility*2.0))/5.0); // 	"Third Down Running",
			data.mCombinePrediction[5] = GeneratePredictedScore(sole);  	//  "Hole Recognition",
			data.mCombinePrediction[6] = GeneratePredictedScore(posDrill);  // 	"Blitz Pickup",
			data.mCombinePrediction[7] = -1;	                            // 	"Avoid Drops",
			data.mCombinePrediction[8] = GeneratePredictedScore(posDrill);	// 	"Route Running",
			data.mCombinePrediction[9] = -1;                                // 	"Third Down Catching",
			data.mCombinePrediction[10] = -1;                               // 	"Endurance",
			data.mCombinePrediction[11] = -1;                           	// 	"Special Teams"
		} 

		void PredictWRAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(posDrill);  // 	"Avoid Drops (PD65)",
			data.mCombinePrediction[1] = GeneratePredictedScore(agility);   // 	"Getting Downfield (Ag100)",
			data.mCombinePrediction[2] = GeneratePredictedScore(sole);      // 	"Route Running (So50)",
			data.mCombinePrediction[3] = -1;	                            // 	"Third Down Catching",
			data.mCombinePrediction[4] = GeneratePredictedScore(forty); 	// 	"Big Play Receiving (Ft70)",
			data.mCombinePrediction[5] = GeneratePredictedScore(bench); 	// 	"Courage (Bp100)",
			data.mCombinePrediction[6] = GeneratePredictedScore(posDrill);  // 	"Adjust to Ball (PD35)",
			data.mCombinePrediction[7] = GeneratePredictedScore(broadjump);	// 	"Punt Returns (Bj50)",
			data.mCombinePrediction[8] = GeneratePredictedScore(broadjump);	// 	"Kick Returns (Bj50)",
			data.mCombinePrediction[9] = -1;                             	// 	"Endurance",
			data.mCombinePrediction[10] = -1;                           	// 	"Special Teams"
		}

		void PredictTEAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(broadjump); //	"Run Blocking",
			data.mCombinePrediction[1] = -1;                                //  "Pass Blocking",
			data.mCombinePrediction[2] = GeneratePredictedScore(bench);     //	"Blocking Strength",
			data.mCombinePrediction[3] = GeneratePredictedScore(posDrill);  //	"Avoid Drops",
			data.mCombinePrediction[4] = GeneratePredictedScore((forty+agility)*0.5f);     //	"Getting Downfield",
			data.mCombinePrediction[5] = GeneratePredictedScore(sole);      //	"Route Running",
			data.mCombinePrediction[6] = GeneratePredictedScore(broadjump); //	"Third Down Catching",
			data.mCombinePrediction[7] = GeneratePredictedScore(forty);     //	"Big Play Receiving",
			data.mCombinePrediction[8] = -1;                                //	"Courage",
			data.mCombinePrediction[9] = GeneratePredictedScore(posDrill);  //	"Adjust to Ball",
			data.mCombinePrediction[10] = -1;                               //	"Endurance",
			data.mCombinePrediction[11] = -1;                               //	"Special Teams",
			data.mCombinePrediction[12] = -1;                               //	"Long Snapping"
		}

		void PredictOLAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(forty);     //	"Run Blocking (Ft100)",
			data.mCombinePrediction[1] = GeneratePredictedScore(agility);   //	"Pass Blocking (Ag100)",
			data.mCombinePrediction[2] = GeneratePredictedScore(bench);     //	"Blocking Strength (Bp100)",
			data.mCombinePrediction[3] = GeneratePredictedScore(broadjump); //	"Endurance (Bj100)",
			data.mCombinePrediction[4] = -1;                                //	"Long Snapping"
		}

		void PredictDLAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(agility);   //	"Run Defense (Ag100)",
			data.mCombinePrediction[1] = GeneratePredictedScore(forty);     //	"Pass Rush Technique (Ft100)",
			data.mCombinePrediction[2] = GeneratePredictedScore(bench);     //	"Pass Rush Strength (Bp50)",
			data.mCombinePrediction[3] = GeneratePredictedScore(sole);      //	"Play Diagnosis (So50)",
			data.mCombinePrediction[4] = GeneratePredictedScore(bench);     //	"Punishing Hitter (Bp50)",
			data.mCombinePrediction[5] = GeneratePredictedScore(broadjump); //	"Endurance (Bj100)"
		}

		void PredictLBAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(agility);   //	"Run Defense (Ag100)",
			data.mCombinePrediction[1] = GeneratePredictedScore(forty);     //	"Pass Rush Technique (Ft100)",
			data.mCombinePrediction[2] = GeneratePredictedScore(broadjump); //	"Man-to-Man Defense (Bj100)",
			data.mCombinePrediction[3] = GeneratePredictedScore(posDrill);  //	"Zone Defense (PD50)",
			data.mCombinePrediction[4] = GeneratePredictedScore(bench);     //	"Bump and Run Defense (Bp33)",
			data.mCombinePrediction[5] = GeneratePredictedScore(bench);     //	"Pass Rush Strength (Bp33)",
			data.mCombinePrediction[6] = GeneratePredictedScore(sole);      //	"Play Diagnosis (So50)",
			data.mCombinePrediction[7] = GeneratePredictedScore(bench);     //	"Punishing Hitter (Bp33)",
			data.mCombinePrediction[8] = -1;                                //	"Endurance",
			data.mCombinePrediction[9] = -1;                                //	"Special Teams"
		}

		void PredictDBAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(agility);   //	"Run Defense (Ag100)",
			data.mCombinePrediction[1] = GeneratePredictedScore(forty);     //	"Man-to-Man Defense (Ft50)",
			data.mCombinePrediction[2] = GeneratePredictedScore((forty+posDrill)*0.5f);     //	"Zone Defense (Ft50PD50)",
			data.mCombinePrediction[3] = GeneratePredictedScore(bench);     //	"Bump and Run Defense (Bp50)",
			data.mCombinePrediction[4] = GeneratePredictedScore(sole);      //	"Play Diagnosis (So50)",
			data.mCombinePrediction[5] = GeneratePredictedScore(bench);     //	"Punishing Hitter (Bp50)",
			data.mCombinePrediction[6] = GeneratePredictedScore(posDrill);  //	"Interceptions (PD50)",
			data.mCombinePrediction[7] = GeneratePredictedScore(broadjump); //	"Punt Returns (Bj50)",
			data.mCombinePrediction[8] = GeneratePredictedScore(broadjump); //	"Kick Returns (Bj50)",
			data.mCombinePrediction[9] = -1;                                //	"Endurance",
			data.mCombinePrediction[10] = -1;                               //	"Special Teams"
		}

		void PredictPAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(forty);     //	"Kicking Power (Ft100)",
			data.mCombinePrediction[1] = GeneratePredictedScore(bench);     //	"Punt Hang Time (Bp100)",
			data.mCombinePrediction[2] = GeneratePredictedScore(sole);      //	"Directional Punting (So50)",
			data.mCombinePrediction[3] = GeneratePredictedScore(agility);   //	"Kick Holding"
		}

		void PredictKAttributes(PlayerData data)
		{
			double sole, forty, bench, agility, broadjump, posDrill;
			CalculateCombineStdDevs(data, out sole, out forty, out bench, out agility, out broadjump, out posDrill);

			data.mCombinePrediction[0] = GeneratePredictedScore(sole);      //	"Kicking Accuracy (So50)",
			data.mCombinePrediction[1] = GeneratePredictedScore(((bench*2.0)+broadjump)/3.0);   //	"Kicking Power (Bp100Bj50)",
			data.mCombinePrediction[2] = GeneratePredictedScore(forty);     //	"Kickoff Distance (Ft100)",
			data.mCombinePrediction[3] = GeneratePredictedScore(broadjump); //	"Kickoff Hang Time (Bj50)"
		}

		#endregion

		private void CalculateCombineRating(PlayerData data, string position, PositionRating posRating)
		{
			posRating.SolecismicRating = 0.0;
			posRating.FortyRating = 0.0;
			posRating.AgilityRating = 0.0;
			posRating.BenchRating = 0.0;
			posRating.BroadJumpRating = 0.0;
			posRating.PositionDrillRating = 0.0;
			PositionGroupCombineData combineData = (PositionGroupCombineData)mPositionGroupCombineMap[mPositionToPositionGroupMap[position]];
			if (data.mSolecismic != 0 && data.m40Time != 0.0 && data.mBench != 0 && data.mAgility != 0.0 && data.mBroadJump != 0)
			{
				DataReader.DraftWeights.PositionWeights posWeights = mWeightsForm.GetPositionWeight(position);
				if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mSolecismicThreshold > 0 && data.mSolecismic < combineData.mSolecismicThreshold)
				{
					posRating.SolecismicRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					posRating.SolecismicRating = ((((double)data.mSolecismic) - combineData.mSolecismicAverage) / combineData.mSolecismicStdDev) * posWeights.Solecismic;
				}
				if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.m40YardThreshold > 0 && data.m40Time > combineData.m40YardThreshold)
				{
					posRating.FortyRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					posRating.FortyRating = ((((double)data.m40Time) - combineData.m40YardAverage) / combineData.m40YardStdDev) * posWeights.Dash;
				}
				if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mBenchThreshold > 0 && data.mBench < combineData.mBenchThreshold)
				{
					posRating.BenchRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					posRating.BenchRating = ((((double)data.mBench) - combineData.mBenchAverage) / combineData.mBenchStdDev) * posWeights.Bench;
				}
				if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mAgilityThreshold > 0 && data.mAgility > combineData.mAgilityThreshold)
				{
					posRating.AgilityRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					posRating.AgilityRating = ((((double)data.mAgility) - combineData.mAgilityAverage) / combineData.mAgilityStdDev) * posWeights.Agility;
				}
				if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mBroadJumpThreshold > 0 && data.mBroadJump < combineData.mBroadJumpThreshold)
				{
					posRating.BenchRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
				}
				else
				{
					posRating.BroadJumpRating = ((((double)data.mBroadJump) - combineData.mBroadJumpAverage) / combineData.mBroadJumpStdDev) * posWeights.BroadJump;
				}
				if (combineData.mPositionDrillStdDev != 0.0)
				{
					if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mPositionDrillThreshold > 0 && data.mPositionDrill < combineData.mPositionDrillThreshold)
					{
						posRating.PositionDrillRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
					}
					else
					{
						posRating.PositionDrillRating = ((((double)data.mPositionDrill) - combineData.mPositionDrillAverage) / combineData.mPositionDrillStdDev) * posWeights.PositionDrill;
					}
				}
			}
			else
			{
				DataReader.DraftWeights.PositionWeights posWeights = mWeightsForm.GetNoCombinePositionWeight(position);
				if (combineData.mSolecismicStdDev != 0.0 && data.mSolecismic != 0)
				{
					if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mSolecismicThreshold > 0 && data.mSolecismic < combineData.mSolecismicThreshold)
					{
						posRating.SolecismicRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
					}
					else
					{
						posRating.SolecismicRating = ((((double)data.mSolecismic) - combineData.mSolecismicAverage) / combineData.mSolecismicStdDev) * posWeights.Solecismic;
					}
				}
				if (combineData.mPositionDrillStdDev != 0.0 && data.mPositionDrill != 0)
				{
					if (mWeightsForm.GlobalWeights.CombineThresholdPenalty != 0 && combineData.mPositionDrillThreshold > 0 && data.mPositionDrill < combineData.mPositionDrillThreshold)
					{
						posRating.PositionDrillRating = mWeightsForm.GlobalWeights.CombineThresholdPenalty;
					}
					else
					{
						posRating.PositionDrillRating = ((((double)data.mPositionDrill) - combineData.mPositionDrillAverage) / combineData.mPositionDrillStdDev) * posWeights.PositionDrill;
					}
				}
			}
			posRating.CombineScore = posRating.SolecismicRating + posRating.FortyRating + posRating.AgilityRating + posRating.BenchRating + posRating.BroadJumpRating + posRating.PositionDrillRating;
		}

		private PositionRating CalculatePositionRating(PlayerData data, string position)
		{
			PositionRating posRating = new PositionRating();
			posRating.Position = position;
			CalculateCombineRating(data, position, posRating);

			DataReader.DraftWeights.GlobalWeightData globalData = mWeightsForm.GlobalWeights;
			DataReader.DraftWeights.PositionWeights posWeights = null;
			if (data.mSolecismic != 0 && data.m40Time != 0.0 && data.mBench != 0 && data.mAgility != 0.0 && data.mBroadJump != 0)
			{
				posWeights = mWeightsForm.GetPositionWeight(position);
			}
			else
			{
				posWeights = mWeightsForm.GetNoCombinePositionWeight(position);
			}

			SizeBands heightBand = GetHeightBandIndex(position, data.mHeight);
			SizeBands weightBand = GetWeightBandIndex(position, data.mWeight);
			if (data.mPosition != position
				&& (heightBand == SizeBands.TooSmall || weightBand == SizeBands.TooSmall || weightBand == SizeBands.TooBig)
				)
			{
				posRating.SizeScore = -500.0;
			}
			else
			{
				int weightDiff = Math.Abs((int)weightBand - (int)SizeBands.Average);
				int heightDiff = Math.Abs((int)heightBand - (int)SizeBands.Average);
				posRating.SizeScore = -1.0 * (((double)weightDiff * ((double)globalData.Weight * 0.5)) + ((double)heightDiff * ((double)globalData.Height * 0.5)));
			}

			double attributesFactor = 0.0;
			switch(globalData.WhichAttributesToUse)
			{
				case DataReader.DraftWeights.AttributeUsage.UseMin:
					{
						for (int attIndex = 0; attIndex < posWeights.Attributes.Length; ++attIndex)
						{
							attributesFactor += ((double)(data.mAttributes[attIndex * 2] * posWeights.Attributes[attIndex])) * 0.01;
						}
					}
					break;
				case DataReader.DraftWeights.AttributeUsage.UseAverage:
					{
						for (int attIndex = 0; attIndex < posWeights.Attributes.Length; ++attIndex)
						{
							attributesFactor += ((double)((data.mAttributes[attIndex * 2] + data.mAttributes[(attIndex * 2) + 1]) *
								posWeights.Attributes[attIndex])) * 0.005;
						}
					}
					break;
				case DataReader.DraftWeights.AttributeUsage.UseMax:
					{
						for (int attIndex = 0; attIndex < posWeights.Attributes.Length; ++attIndex)
						{
							attributesFactor += ((double)(data.mAttributes[(attIndex * 2) + 1] * posWeights.Attributes[attIndex])) * 0.01;
						}
					}
					break;
			}
			posRating.AttributeScore = attributesFactor;

			double scoutFactor = 0.0;
			//if (data.mImpression == "Very Overrated")
			//{
			//	scoutFactor -= (double)globalData.ScoutImpression;
			//}
			//else if (data.mImpression == "Very Underrated")
			//{
			//	scoutFactor += (double)globalData.ScoutImpression;
			//}
			//else if (data.mImpression == "Underrated")
			//{
			//	scoutFactor += ((double)globalData.ScoutImpression)*0.5;
			//}
			//else if (data.mImpression == "Overrated")
			//{
			//	scoutFactor -= ((double)globalData.ScoutImpression) * 0.5;
			//}
			//else if (data.mImpression == "Hard to Read")
			//{
			//	scoutFactor -= ((double)globalData.ScoutImpression) * 0.1;
			//}
			posRating.ScoutImpressionScore = scoutFactor;

			double chemistryFactor = 0.0;
			if (data.mConflicts.Length > 1)
			{
				chemistryFactor -= (double)globalData.Conflict * data.mConflictsFactor;
			}
			if (data.mAffinities.Length > 1)
			{
				chemistryFactor += (double)globalData.Affinity * data.mAffinitiesFactor;
			}
			posRating.ChemistryScore = chemistryFactor;

			posRating.DevelopmentScore = (data.mPercentDeveloped - globalData.AvgDev) * ((double)globalData.DevWt / (double)globalData.AvgDev);

			posRating.OverallScore = posRating.CombineScore + posRating.SizeScore + posRating.ChemistryScore +
				posRating.AttributeScore + posRating.ScoutImpressionScore + posRating.DevelopmentScore;
			posRating.OverallScore *= posWeights.Weight;

			return posRating;
		}

		private void DisplayPlayerData()
		{
			toolStripStatusLabelAction.Text = "Filling table...";
			statusStripMain.Refresh();
			listViewDraftees.SuspendLayout();
			listViewDraftees.Items.Clear();
			int oldSortColumn = 2;
			WindowsUtilities.SortType oldSortType = WindowsUtilities.SortType.SortByColoredString;
			bool oldDescending = true;
			if (listViewDraftees.ListViewItemSorter != null)
			{
				oldSortColumn = ((WindowsUtilities.SortTypeListViewItemSorter)listViewDraftees.ListViewItemSorter).SortColumn;
				oldSortType = ((WindowsUtilities.SortTypeListViewItemSorter)listViewDraftees.ListViewItemSorter).SortMethod;
				oldDescending = ((WindowsUtilities.SortTypeListViewItemSorter)listViewDraftees.ListViewItemSorter).Descending;
			}
			listViewDraftees.ListViewItemSorter = null;
			if (mPlayerData != null)
			{
				toolStripProgressBarAction.Maximum = mPlayerData.Count;
				toolStripProgressBarAction.Value = 0;
				if (mPlayerData != null)
				{
					for (int i = 0; i < mPlayerData.Count; i++)
					{
						ListViewItem newItem = new ListViewItem();
						PlayerData data = (PlayerData)mPlayerData[i];
						data.mItem = newItem;
						DisplayPlayerData(i);
						listViewDraftees.Items.Add(newItem);
						toolStripProgressBarAction.Value = i;
					}
				}
			}
			listViewDraftees.ListViewItemSorter = new WindowsUtilities.SortTypeListViewItemSorter(oldSortColumn, oldSortType, oldDescending);
			listViewDraftees.Sort();
			listViewDraftees.ResumeLayout();
			toolStripStatusLabelAction.Text = "Finished!";
			toolStripProgressBarAction.Value = 0;
		}

		private void BuildAttributeImage(PictureBox picBox, int attributeMin, int attributeMax, int combineSpot, bool interviewed, bool noMasking)
		{
			Bitmap newImage = new Bitmap(picBox.Width, picBox.Height);
			Graphics g = Graphics.FromImage(newImage);
			Brush attributeBrush = Brushes.LightGray;
			if (noMasking)
			{
				attributeBrush = Brushes.DarkOliveGreen;
			}
			g.FillRectangle(attributeBrush, 0, 0, picBox.Width, picBox.Height);
			if (interviewed)
			{
				attributeBrush = Brushes.Orange;
			}
			else
			{
				attributeBrush = Brushes.DodgerBlue;
			}
			g.FillRectangle(attributeBrush, attributeMin, 0, attributeMax - attributeMin + 1, picBox.Height);
			g.DrawLine(Pens.Black, 25, 0, 25, picBox.Height);
			g.DrawLine(Pens.Black, 50, 0, 50, picBox.Height);
			g.DrawLine(Pens.Black, 75, 0, 75, picBox.Height);
			if (combineSpot >= 0)
			{
				g.FillRectangle(Brushes.White, combineSpot - 1, 0, 3, picBox.Height);
			}
			g.DrawRectangle(Pens.Black, 0, 0, picBox.Width - 1, picBox.Height - 1);
			g.Flush();

			picBox.Image = newImage;
		}

		private void ConnectMaskedPair(Graphics g, Color color, int attr1, int attr2, int depth)
		{
			int rowHeight = pictureBoxMaskedPairs.Height / kMaxAttributeCounts;
			int attr1y = (rowHeight * attr1) + (rowHeight / 2);
			int attr2y = (rowHeight * attr2) + (rowHeight / 2);
			Pen blackPen = new Pen(color, 2.0f);
			g.DrawLine(blackPen, 0, attr1y, depth, attr1y);
			g.DrawLine(blackPen, 0, attr2y, depth, attr2y);
			g.DrawLine(blackPen, depth, attr1y, depth, attr2y);
		}

		private void BuildMaskedPairsImage(string positionGroup)
		{
			Bitmap newImage = new Bitmap(pictureBoxMaskedPairs.Width, pictureBoxMaskedPairs.Height);
			Graphics g = Graphics.FromImage(newImage);
			g.FillRectangle(Brushes.White, 0, 0, pictureBoxMaskedPairs.Width, pictureBoxMaskedPairs.Height);

			if (positionGroup == "QB")
			{
				ConnectMaskedPair(g, Color.Black, 1, 6, 5);
				ConnectMaskedPair(g, Color.Red, 2, 7, 10);
				ConnectMaskedPair(g, Color.Blue, 3, 8, 15);
				ConnectMaskedPair(g, Color.Green, 4, 9, 20);
				ConnectMaskedPair(g, Color.Brown, 5, 10, 25);
			}
			else if (positionGroup == "RB")
			{
				ConnectMaskedPair(g, Color.Black, 8, 13, 8);
				ConnectMaskedPair(g, Color.Green, 9, 14, 20);
			}
			else if (positionGroup == "FB")
			{
				ConnectMaskedPair(g, Color.Black, 8, 11, 15);
			}
			else if (positionGroup == "TE")
			{
				ConnectMaskedPair(g, Color.Black, 4, 10, 8);
				ConnectMaskedPair(g, Color.Green, 5, 11, 20);
			}
			else if (positionGroup == "WR")
			{
				ConnectMaskedPair(g, Color.Black, 1, 9, 8);
				ConnectMaskedPair(g, Color.Green, 2, 10, 20);
			}
			else if (positionGroup == "ILB" || positionGroup == "OLB")
			{
				ConnectMaskedPair(g, Color.Black, 2, 8, 8);
				ConnectMaskedPair(g, Color.Green, 3, 9, 20);
			}
			else if (positionGroup == "CB" || positionGroup == "S")
			{
				ConnectMaskedPair(g, Color.Black, 1, 9, 8);
				ConnectMaskedPair(g, Color.Green, 2, 10, 20);
			}

			g.Flush();

			pictureBoxMaskedPairs.Image = newImage;
		}

		private void DisplayPlayerDetails(PlayerData data)
		{
			if (data == null)
			{
				for (int i = 0; i < kMaxAttributeCounts; i++)
				{
					mAttributeLabels[i].Text = "";
					mAttributePictureBoxes[i].Image = null;
				}
				pictureBoxMaskedPairs.Image = null;
				textBoxDetails.Text = "";
			}
			else
			{
				string[] attributeNames = mPositionGroupAttributeNames[data.mPositionGroup];
				bool[] attributeNotMasked = mPositionGroupAttributeNotMasked[data.mPositionGroup];
				for (int i = 0; i < attributeNames.Length; i++)
				{
					mAttributeLabels[i].Text = attributeNames[i];
					int attributeMin = data.mAttributes[i * 2];
					int attributeMax = data.mAttributes[(i * 2) + 1];
					int combineSpot = data.mCombinePrediction[i];
					BuildAttributeImage(mAttributePictureBoxes[i], attributeMin, attributeMax, combineSpot, data.mInterviewed == "Yes", attributeNotMasked[i]);
				}
				for (int j = attributeNames.Length; j < kMaxAttributeCounts; j++)
				{
					mAttributeLabels[j].Text = "";
					mAttributePictureBoxes[j].Image = null;
				}
				BuildMaskedPairsImage(data.mPositionGroup);
				string detailsText = "";

				detailsText += "Ht: ";
				detailsText += (data.mHeight / 12).ToString();
				detailsText += "' ";
				detailsText += (data.mHeight % 12).ToString();
				detailsText += "\"";
				detailsText += " (" + kHeightBands[(int)GetHeightBandIndex(data.mPosition, data.mHeight)] + ")";
				detailsText += Environment.NewLine;
				detailsText += "Wt: ";
				detailsText += data.mWeight.ToString();
				detailsText += " lbs.";
				detailsText += " (" + kWeightBands[(int)GetWeightBandIndex(data.mPosition, data.mWeight)] + ")";
				detailsText += Environment.NewLine;
				detailsText += "College: ";
				detailsText += data.mCollege;
				detailsText += Environment.NewLine;
				detailsText += "DOB: ";
				detailsText += data.mBirthDate;
				detailsText += Environment.NewLine;
				detailsText += "Bureau: ";
				detailsText += data.mGrade.ToString("F1");
				detailsText += Environment.NewLine;
				if (data.mConflicts.Length > 0)
				{
					detailsText += "Conflicts: ";
					detailsText += data.mConflicts;
					detailsText += Environment.NewLine;
				}
				if (data.mAffinities.Length > 0)
				{
					detailsText += "Affinities: ";
					detailsText += data.mAffinities;
					detailsText += Environment.NewLine;
				}
				if (data.mPositionGroup == "QB")
				{
					detailsText += "Formations: ";
					detailsText += data.mFormations;
					detailsText += Environment.NewLine;
				}
				detailsText += Environment.NewLine;
				detailsText += "Comb: ";
				detailsText += data.mCombineSum.ToString("F2");
				detailsText += Environment.NewLine;
				detailsText += "Sole: ";
				detailsText += data.mSolecismicRating.ToString("F2");
				detailsText += " Dash: ";
				detailsText += data.mFortyRating.ToString("F2");
				detailsText += " Agil: ";
				detailsText += data.mAgilityRating.ToString("F2");
				detailsText += Environment.NewLine;
				detailsText += "Bnch: ";
				detailsText += data.mBenchRating.ToString("F2");
				detailsText += " BrdJ: ";
				detailsText += data.mBroadJumpRating.ToString("F2");
				detailsText += " PDrl: ";
				detailsText += data.mPositionDrillRating.ToString("F2");
				detailsText += Environment.NewLine;
				detailsText += Environment.NewLine;
				detailsText += "Chem: ";
				detailsText += data.mChemistryRating.ToString("F2");
				detailsText += " Impr: ";
				detailsText += data.mScoutImpressionRating.ToString("F2");
				detailsText += " Dev: ";
				detailsText += data.mDevelopmentRating.ToString("F2");
				detailsText += Environment.NewLine;
				detailsText += Environment.NewLine;
				foreach (PositionRating posRating in data.mPositionRatings)
				{
					detailsText += posRating.Position;
					if (posRating.Position.Length < 4)
					{
						detailsText += " ";
					}
					if (posRating.Position.Length < 3)
					{
						detailsText += " ";
					}
					if (posRating.Position.Length < 2)
					{
						detailsText += " ";
					}
					
					detailsText += " Rat: ";
					detailsText += posRating.OverallScore.ToString("F2");
					detailsText += " Size: ";
					detailsText += posRating.SizeScore.ToString("F2");
					detailsText += " Att: ";
					detailsText += posRating.AttributeScore.ToString("F2");
					detailsText += Environment.NewLine;
				}

				textBoxDetails.Text = detailsText;
			}
		}

		private enum SizeBands
		{
			TooSmall,
			WellBelowAverage,
			BelowAverage,
			Average,
			AboveAverage,
			WellAboveAverage,
			TooBig
		}

		private string[] kHeightBands =
			{
				"too short"
				,"well below average"
				,"below average"
				,"about average"
				,"above average"
				,"well above average"
				,"too tall"
			};
		private string[] kWeightBands =
			{
				"too light"
				,"well below average"
				,"below average"
				,"about average"
				,"above average"
				,"well above average"
				,"too heavy"
			};

		private SizeBands GetHeightBandIndex(string playerPosition, int height)
		{
			PositionSizeRanges posRange = mPositionSizeRangesMap[playerPosition];
			if (height < posRange.MinHeight)
			{
				return SizeBands.TooSmall;
			}
			else if (height < (posRange.AverageHeight - 2))
			{
				return SizeBands.WellBelowAverage;
			}
			else if (height < posRange.AverageHeight)
			{
				return SizeBands.BelowAverage;
			}
			else if (height == posRange.AverageHeight)
			{
				return SizeBands.Average;
			}
			else if (height > (posRange.AverageHeight + 2))
			{
				return SizeBands.WellAboveAverage;
			}
			else if (height > posRange.AverageHeight)
			{
				return SizeBands.AboveAverage;
			}
			else
			{
				return SizeBands.TooBig;
			}
		}

		private SizeBands GetWeightBandIndex(string playerPosition, int weight)
		{
			PositionSizeRanges posRange = mPositionSizeRangesMap[playerPosition];
			if (weight <= posRange.MinWeight)
			{
				return SizeBands.TooSmall;
			}
			else if (weight <= posRange.WellBelowAverageWeightCap)
			{
				return SizeBands.WellBelowAverage;
			}
			else if (weight <= posRange.BelowAverageWeightCap)
			{
				return SizeBands.BelowAverage;
			}
			else if (weight <= posRange.AverageWeightCap)
			{
				return SizeBands.Average;
			}
			else if (weight <= posRange.AboveAverageWeightCap)
			{
				return SizeBands.AboveAverage;
			}
			else if (weight <= posRange.WellAboveAverageWeightCap)
			{
				return SizeBands.WellAboveAverage;
			}
			else
			{
				return SizeBands.TooBig;
			}
		}

		private void predictChemistryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (mChemistryForm.ShowDialog() == DialogResult.OK)
			{
				PredictChemistry();
				DisplayPlayerData();
			}
		}

		private void PredictChemistry()
		{
			if (mPlayerData == null)
			{
				return;
			}

			string[] qbTestPositions = new string[] { "RB", "FL", "LT" };

			for (int i = 0; i < mPlayerData.Count; ++i)
			{
				PlayerData data = (PlayerData)mPlayerData[i];
				if (data.mPosition == "QB")
				{
					int leaderConflicts = 0;
					int leaderAffinities = 0;
					double conflictFactor = 0.0;
					double affinityFactor = 0.0;
					foreach (string position in qbTestPositions)
					{
						ChemistryUtilities.Birthday leaderBirthday = mChemistryForm.GetLeaderBirthday(position);
						ChemistryUtilities.Chemistry leaderChemistry = ChemistryUtilities.PredictChemistry(leaderBirthday, data.mBirthDate);
						if (leaderChemistry == ChemistryUtilities.Chemistry.Affinity)
						{
							leaderAffinities += 1;
							affinityFactor += mChemistryForm.GetPositionLeaderWeight(position);
						}
						else if (leaderChemistry == ChemistryUtilities.Chemistry.Conflict)
						{
							leaderConflicts += 1;
							conflictFactor += mChemistryForm.GetPositionLeaderWeight(position);
						}
					}
					if (leaderConflicts > 0)
					{
						data.mConflicts = leaderConflicts.ToString() + " Leader";
						if (leaderConflicts > 1)
						{
							data.mConflicts += "s";
						}
						data.mConflictsFactor = conflictFactor;
					}
					else
					{
						data.mConflicts = "";
						data.mConflictsFactor = 0.0;
					}
					if (leaderAffinities > 0)
					{
						data.mAffinities = leaderAffinities.ToString() + " Leader";
						if (leaderAffinities > 1)
						{
							data.mAffinities += "s";
						}
						data.mAffinitiesFactor = affinityFactor;
					}
					else
					{
						data.mAffinities = "";
						data.mAffinitiesFactor = 0.0;
					}
				}
				else
				{
					ChemistryUtilities.Birthday leaderBirthday = mChemistryForm.GetLeaderBirthday(data.mPosition);
					ChemistryUtilities.Chemistry leaderChemistry = ChemistryUtilities.PredictChemistry(leaderBirthday, data.mBirthDate);
					data.mAffinities = "";
					data.mConflicts = "";
					data.mAffinitiesFactor = 0.0;
					data.mConflictsFactor = 0.0;
					if (leaderChemistry == ChemistryUtilities.Chemistry.Affinity)
					{
						data.mAffinities = "Team Leader";
						data.mAffinitiesFactor = mChemistryForm.GetPositionLeaderWeight(data.mPosition);
					}
					else if (leaderChemistry == ChemistryUtilities.Chemistry.Conflict)
					{
						data.mConflicts = "Team Leader";
						data.mConflictsFactor = mChemistryForm.GetPositionLeaderWeight(data.mPosition);
					}
				}
			}
		}

		private PlayerData mSelectedPlayerData;
		private ListViewItem mSelectedItem;
		private int mSelectedPlayerListIndex;

		private void UnselectPlayer()
		{
			// This removes the highlighting
			if (mSelectedPlayerData != null)
			{
				DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
			}

			mSelectedPlayerData = null;
			mSelectedItem = null;
			mSelectedPlayerListIndex = -1;

			DisplayPlayerDetails(null);
		}

		private void SelectPlayer(int listIndex)
		{
			// This removes the highlighting
			if (mSelectedPlayerData != null)
			{
				DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
			}

			mSelectedPlayerListIndex = listIndex;
			mSelectedItem = listViewDraftees.Items[listIndex];
			mSelectedPlayerData = (PlayerData)mSelectedItem.Tag;

			// This adds the highlighting to the selected player's row
			DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
			DisplayPlayerDetails(mSelectedPlayerData);
		}

		private void listViewDraftees_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewDraftees.SelectedIndices.Count > 0)
			{
				SelectPlayer(listViewDraftees.SelectedIndices[0]);
			}
			else
			{
				UnselectPlayer();
			}
		}

		private void contextMenuStripDraftees_Opening(object sender, CancelEventArgs e)
		{
			if (mSelectedPlayerData != null)
			{
				if (mSelectedPlayerData.mDrafted)
				{
					draftedToolStripMenuItem.Checked = true;
				}
				else
				{
					draftedToolStripMenuItem.Checked = false;
				}

				if (mSelectedPlayerData.mMarked)
				{
					markToolStripMenuItem.Checked = true;
				}
				else
				{
					markToolStripMenuItem.Checked = false;
				}
			}
			else
			{
				draftedToolStripMenuItem.Enabled = false;
				markToolStripMenuItem.Checked = false;
			}
		}

		private void DraftSelectedPlayer()
		{
			if (mSelectedPlayerData != null)
			{
				mSelectedPlayerData.mDrafted = !mSelectedPlayerData.mDrafted;
				DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
			}
		}

		private void draftedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DraftSelectedPlayer();
		}

		private void MarkSelectedPlayer()
		{
			if (mSelectedPlayerData != null)
			{
				mSelectedPlayerData.mMarked = !mSelectedPlayerData.mMarked;
				DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
			}
		}

		private void markToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MarkSelectedPlayer();
		}

		private const int kDraftListVersion = 4;
		private void SaveDraftListFile(string filename)
		{
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);

			System.IO.FileStream outStream = new System.IO.FileStream(filename, System.IO.FileMode.Create);
			System.IO.BinaryWriter outFile = new System.IO.BinaryWriter(outStream, windows1252Encoding);

			outFile.Write(kDraftListVersion);
			outFile.Write(mPlayerData.Count);

			outFile.Write(mChemistryForm.BackfieldWeight);
			outFile.Write(mChemistryForm.ReceiversWeight);
			outFile.Write(mChemistryForm.OffensiveLineWeight);
			outFile.Write(mChemistryForm.DefensiveFrontWeight);
			outFile.Write(mChemistryForm.SecondaryWeight);

			foreach (object obj in mPlayerData)
			{
				PlayerData data = (PlayerData)obj;

				outFile.Write(data.mName);
				outFile.Write(data.mPosition);
				outFile.Write(data.mPositionGroup);
				outFile.Write(data.mCollege);
				outFile.Write(data.mBirthDate);
				outFile.Write(data.mHeight);
				outFile.Write(data.mWeight);
				outFile.Write(data.mSolecismic);
				outFile.Write(data.m40Time);
				outFile.Write(data.mBench);
				outFile.Write(data.mAgility);
				outFile.Write(data.mBroadJump);
				outFile.Write(data.mPositionDrill);
				outFile.Write(data.mPercentDeveloped);
				outFile.Write(data.mGrade);
				outFile.Write(data.mInterviewed);
				outFile.Write(data.mConflicts);
				outFile.Write(data.mAffinities);
				outFile.Write(data.mFormations);
				outFile.Write(data.mOriginalOrder);
				outFile.Write(data.mDesiredOrder);
				outFile.Write(data.mDrafted);
				outFile.Write(data.mMarked);
				outFile.Write(data.mDraftPosition.ToString());
				for (int i = 0; i < data.mAttributes.Length; i++)
				{
					outFile.Write(data.mAttributes[i]);
				}
				outFile.Write(data.mConflictsFactor);
				outFile.Write(data.mAffinitiesFactor);
				outFile.Write(data.mSolecismicRating);
				outFile.Write(data.mFortyRating);
				outFile.Write(data.mBenchRating);
				outFile.Write(data.mAgilityRating);
				outFile.Write(data.mBroadJumpRating);
				outFile.Write(data.mPositionDrillRating);
			}

			outFile.Close();
		}

		private void LoadDraftListFile(string filename)
		{
			try
			{
				System.IO.FileStream inStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				Encoding windows1252Encoding = Encoding.GetEncoding(1252);
				System.IO.BinaryReader inFile = new System.IO.BinaryReader(inStream, windows1252Encoding);

				int version = inFile.ReadInt32();
				int playerCount = inFile.ReadInt32();

				if (version >= 2)
				{
					mChemistryForm.BackfieldWeight = inFile.ReadDouble();
					mChemistryForm.ReceiversWeight = inFile.ReadDouble();
					mChemistryForm.OffensiveLineWeight = inFile.ReadDouble();
					mChemistryForm.DefensiveFrontWeight = inFile.ReadDouble();
					mChemistryForm.SecondaryWeight = inFile.ReadDouble();
				}
				mPlayerData = new System.Collections.ArrayList();
				mDraftOrderList = new System.Collections.Generic.SortedList<int, int>();

				for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
				{
					PlayerData data = new PlayerData();

					data.mName = inFile.ReadString();
					data.mPosition = inFile.ReadString();
					data.mPositionGroup = inFile.ReadString();
					data.mCollege = inFile.ReadString();
					data.mBirthDate = inFile.ReadString();
					data.mHeight = inFile.ReadInt32();
					data.mWeight = inFile.ReadInt32();
					data.mSolecismic = inFile.ReadInt32();
					data.m40Time = inFile.ReadDouble();
					data.mBench = inFile.ReadInt32();
					data.mAgility = inFile.ReadDouble();
					data.mBroadJump = inFile.ReadInt32();
					data.mPositionDrill = inFile.ReadInt32();
					data.mPercentDeveloped = inFile.ReadInt32();
					data.mGrade = inFile.ReadDouble();
					data.mInterviewed = inFile.ReadString();
					if (version >= 2)
					{
						data.mConflicts = inFile.ReadString();
						data.mAffinities = inFile.ReadString();
					}
					data.mFormations = inFile.ReadInt32();
					data.mOriginalOrder = inFile.ReadInt32();
					data.mDesiredOrder = inFile.ReadInt32();
					data.mDrafted = inFile.ReadBoolean();
					data.mMarked = inFile.ReadBoolean();
					string draftPosition = inFile.ReadString();
					data.mDraftPosition = (DraftPosition)Enum.Parse(typeof(DraftPosition), draftPosition);
					data.mAttributes = new int[kMaxAttributeCounts * 2];
					for (int i = 0; i < data.mAttributes.Length; i++)
					{
						data.mAttributes[i] = inFile.ReadInt32();
					}

					if (version >= 2)
					{
						data.mConflictsFactor = inFile.ReadDouble();
						data.mAffinitiesFactor = inFile.ReadDouble();
					}
					else
					{
						data.mAffinitiesFactor = 1.0;
						data.mConflictsFactor = 1.0;
					}
					data.mSolecismicRating = inFile.ReadDouble();
					data.mFortyRating = inFile.ReadDouble();
					data.mBenchRating = inFile.ReadDouble();
					data.mAgilityRating = inFile.ReadDouble();
					data.mBroadJumpRating = inFile.ReadDouble();
					data.mPositionDrillRating = inFile.ReadDouble();

					if (version == 3)
					{
						/*data.mDraftable = */inFile.ReadInt32();
						/*data.mVeryGood = */inFile.ReadInt32();
						/*data.mGood = */inFile.ReadInt32();
						/*data.mAverage = */inFile.ReadInt32();
						/*data.mFairPoor = */inFile.ReadInt32();
						/*data.mSlotScore = */inFile.ReadInt32();
					}

					mPlayerData.Add(data);
					mDraftOrderList.Add(data.mDesiredOrder, playerIndex);
				}

				inFile.Close();
			}
			catch (System.IO.IOException e)
			{
				MessageBox.Show("Error reading '" + filename + "': " + e.ToString(), "Error Loading Draft List File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadExportedDraftees(string directoryName)
		{
			string rookiesPath = System.IO.Path.Combine(directoryName, "rookies.csv");
			string draftPersonalPath = System.IO.Path.Combine(directoryName, "draft_personal.csv");
			string infoPath = System.IO.Path.Combine(directoryName, "player_information.csv");
			try
			{
				// read position and birthdate from player_information.csv
				using (System.IO.StreamReader rookieFile = new System.IO.StreamReader(rookiesPath),
					draftFile = new System.IO.StreamReader(draftPersonalPath),
					infoFile = new System.IO.StreamReader(infoPath))
				{
					mPlayerData = new System.Collections.ArrayList();
					mDraftOrderList = new SortedList<int, int>();

					System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

					string headerLine = rookieFile.ReadLine();
					headerLine = draftFile.ReadLine();
					headerLine = infoFile.ReadLine();
					while (!rookieFile.EndOfStream)
					{
						string rookieLine = rookieFile.ReadLine();
						string draftLine = draftFile.ReadLine();
						string infoLine = infoFile.ReadLine();
						string[] rookieFields = DataReader.CSVHelper.ParseLine(rookieLine);
						string[] draftFields = DataReader.CSVHelper.ParseLine(draftLine);
						string[] infoFields = DataReader.CSVHelper.ParseLine(infoLine);
						while (infoFields[0] != rookieFields[0] && !infoFile.EndOfStream)
						{
							infoLine = infoFile.ReadLine();
							infoFields = DataReader.CSVHelper.ParseLine(infoLine);
						}
						try
						{
							if (rookieFields[0] != draftFields[0])
							{
								DialogResult result = MessageBox.Show("Player IDs do not match up:" + Environment.NewLine + rookieLine + Environment.NewLine + draftLine,
									"Parse Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
								if (result == DialogResult.Cancel)
								{
									return;
								}
							}
							else
							{
								PlayerData newData = new PlayerData();
								newData.mName = rookieFields[2] + " " + rookieFields[1];
								newData.mPosition = infoFields[5];
								newData.mPositionGroup = rookieFields[3];
								newData.mCollege = rookieFields[4];
								newData.mBirthDate = infoFields[17].ToString() + "-" + infoFields[18].ToString() + "-" + infoFields[16].ToString();
								newData.mHeight = Int32.Parse(rookieFields[5], nfi);
								newData.mWeight = Int32.Parse(rookieFields[6], nfi);
								newData.m40Time = ((Double)Int32.Parse(rookieFields[7], nfi)) / 100.0;
								newData.mSolecismic = Int32.Parse(rookieFields[8], nfi);
								newData.mBench = Int32.Parse(rookieFields[9], nfi);
								newData.mAgility = ((Double)Int32.Parse(rookieFields[10], nfi)) / 100.0;
								newData.mBroadJump = Int32.Parse(rookieFields[11], nfi);
								newData.mPositionDrill = Int32.Parse(rookieFields[12], nfi);
								newData.mPercentDeveloped = Int32.Parse(rookieFields[13], nfi);
								newData.mGrade = ((Double)Int32.Parse(rookieFields[14], nfi)) / 10.0;
								newData.mAffinities = "";
								newData.mConflicts = "";

								if (draftFields[1] == "0")
								{
									newData.mInterviewed = "No";
								}
								else
								{
									newData.mInterviewed = "Yes";
								}

								string[] posAttributes = mPositionGroupAttributeNames[newData.mPositionGroup];
								for (int i = 0; i < posAttributes.Length; ++i)
								{
									for (int j = 0; j < mAllAttributeNames.Length; ++j)
									{
										if (posAttributes[i].StartsWith(mAllAttributeNames[j]))
										{
											newData.mAttributes[(i * 2)] = Int32.Parse(draftFields[mAttributeColumns[j]], nfi);
											newData.mAttributes[(i * 2) + 1] = Int32.Parse(draftFields[mAttributeColumns[j] + kHighAttributeOffset], nfi);
										}
									}
								}

								newData.mOriginalOrder = mPlayerData.Count;
								newData.mDesiredOrder = mPlayerData.Count;
								newData.mDrafted = false;
								newData.mMarked = false;
								newData.mOrderDrafted = -1;
								newData.mDraftPosition = DraftPosition.NotSet;
								mDraftOrderList.Add(newData.mDesiredOrder, mPlayerData.Count);
								mPlayerData.Add(newData);
							}
						}
						catch (FormatException)
						{
							DialogResult result = MessageBox.Show("One of the fields on the line was bad:" + Environment.NewLine + rookieLine + Environment.NewLine + draftLine,
								"Parse Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
							if (result == DialogResult.Cancel)
							{
								return;
							}
						}
					}
				}
			}
			catch (System.IO.IOException e)
			{
				string errorString = "IO Error: " + e.ToString();
				if (e.InnerException != null)
				{
					errorString += System.Environment.NewLine + e.InnerException.ToString();
				}
				MessageBox.Show(errorString, "Error Loading draft data", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void textBoxPasteArea_Click(object sender, EventArgs e)
		{
			textBoxPasteArea.SelectAll();
		}

		private void showStdDevsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mCombineDisplayType = CombineDisplayType.DisplayStdDevs;
			showStdDevsToolStripMenuItem.Checked = true;
			showCombineScoresToolStripMenuItem.Checked = false;
			showCombineValuesToolStripMenuItem.Checked = false;
			DisplayPlayerData();
		}

		private void showCombineValuesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mCombineDisplayType = CombineDisplayType.DisplayValues;
			showStdDevsToolStripMenuItem.Checked = false;
			showCombineScoresToolStripMenuItem.Checked = false;
			showCombineValuesToolStripMenuItem.Checked = true;
			DisplayPlayerData();
		}

		private void showCombineScoresToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mCombineDisplayType = CombineDisplayType.DisplayRatings;
			showStdDevsToolStripMenuItem.Checked = false;
			showCombineScoresToolStripMenuItem.Checked = true;
			showCombineValuesToolStripMenuItem.Checked = false;
			DisplayPlayerData();
		}

		private void buttonMarkDrafted_Click(object sender, EventArgs e)
		{
			string[] delims = new string[] { Environment.NewLine };
			string[] lines = textBoxPasteArea.Text.Split(delims,StringSplitOptions.RemoveEmptyEntries);
			toolStripStatusLabelAction.Text = "";
			toolStripProgressBarAction.Maximum = lines.Length;
			toolStripProgressBarAction.Value = 0;
			foreach (string line in lines)
			{
				toolStripStatusLabelAction.Text = line;
				MarkDrafted(line);
				toolStripProgressBarAction.Value += 1;
				statusStripMain.Refresh();
			}
			toolStripStatusLabelAction.Text = "Finished!";
			toolStripProgressBarAction.Value = 0;
			if (sortDraftedToBottomToolStripMenuItem.Checked)
			{
				WindowsUtilities.SortTypeListViewItemSorter.UpdateSortColumn(listViewDraftees, -1, sortDraftedToBottomToolStripMenuItem.Checked);
			}
		}

		private void MarkDrafted(string line)
		{
			string name = "";
			string posGroup = "";
			string college = "";
			int draftedOrder = 0;
			if (line.IndexOf(',') < 0)
			{
				int positionStart = line.IndexOf(" - ");
				if (positionStart >= 0)
				{
					positionStart += 3;
					int positionEnd = line.IndexOf(' ', positionStart);
					if (positionEnd >= 0)
					{
						string position = line.Substring(positionStart,positionEnd - positionStart);
						posGroup = mPositionToPositionGroupMap[position];
						int positionEndIndex = line.IndexOf("  ",positionEnd+1);
						if (positionEndIndex < 0)
						{
							name = line.Substring(positionEnd + 1);
							name = name.Trim();
						}
						else
						{
							int nameLength = positionEndIndex - positionEnd - 1;
							name = line.Substring(positionEnd + 1, nameLength);
							name = name.Trim();
						}
					}
				}
			}
			else
			{
                if (line.StartsWith("Pick #"))
                {
                    line = line.Substring(6);
                }
				int draftedOrderEnd = line.IndexOf(" - ");
				int playerNameStart = line.IndexOf(" - ", draftedOrderEnd+3);
				if (playerNameStart < 0)
				{
					MessageBox.Show("Could not find player name on drafted line" + Environment.NewLine + line, "Error Marking Drafted Players",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				if (draftedOrderEnd < playerNameStart)
				{
					Int32.TryParse(line.Substring(0, draftedOrderEnd), out draftedOrder);
				}

				playerNameStart += 3;
				string[] fields = line.Substring(playerNameStart).Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
				if (fields.Length < 4 || fields.Length > 5)
				{
					MessageBox.Show("Could not split drafted line into correct fields" + Environment.NewLine + line,
						"Error Marking Drafted Players",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				name = fields[1] + " " + fields[0];
				posGroup = fields[2];
				college = fields[3];
				if (fields.Length == 5)
				{
					college += ", ";
					college += fields[4];
				}
			}

			if (name.Length > 0)
			{
				FindAndMarkDraftee(name, posGroup, college, draftedOrder);
			}
		}

		private void FindAndMarkDraftee(string name, string posGroup, string college, int draftedOrder)
		{
			System.Collections.ArrayList candidatePlayers = new System.Collections.ArrayList();
			foreach (ListViewItem curItem in listViewDraftees.Items)
			{
				PlayerData data = (PlayerData)curItem.Tag;
				if (data.mName == name)
				{
					candidatePlayers.Add(curItem);
				}
			}

			int candidateIndex;
			ListViewItem playerItem;
			if (candidatePlayers.Count > 1)
			{
				candidateIndex = 0;
				while (candidateIndex < candidatePlayers.Count)
				{
					playerItem = (ListViewItem)candidatePlayers[candidateIndex];
					PlayerData data = (PlayerData)playerItem.Tag;
					if (data.mPositionGroup != posGroup)
					{
						candidatePlayers.RemoveAt(candidateIndex);
					}
					else
					{
						candidateIndex++;
					}
				}
			}

			if (candidatePlayers.Count > 1)
			{
				candidateIndex = 0;
				while (candidateIndex < candidatePlayers.Count)
				{
					playerItem = (ListViewItem)candidatePlayers[candidateIndex];
					PlayerData data = (PlayerData)playerItem.Tag;
					if (data.mCollege != college)
					{
						candidatePlayers.RemoveAt(candidateIndex);
					}
					else
					{
						candidateIndex++;
					}
				}
			}

			if (candidatePlayers.Count == 1)
			{
				playerItem = (ListViewItem)candidatePlayers[0];
				PlayerData data = (PlayerData)playerItem.Tag;
				data.mDrafted = true;
				data.mOrderDrafted = draftedOrder;
				DisplayPlayerData(data.mOriginalOrder);
			}
			else if (candidatePlayers.Count > 1)
			{
				MessageBox.Show("Found multiple possibilities for draftee" + Environment.NewLine + name + Environment.NewLine + posGroup +
					Environment.NewLine + college,
					"Error Marking Drafted Players",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else
			{
				MessageBox.Show("Did not find any players that matched draftee" + Environment.NewLine + name + Environment.NewLine + posGroup +
					Environment.NewLine + college,
					"Error Marking Drafted Players",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}

		private void exportDrafteesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Draftee Exports (*.draftees)|*.draftees|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				using (System.IO.StreamWriter outFile = new System.IO.StreamWriter(dlg.FileName))
				{
					for (int i = 0; i < mPlayerData.Count; ++i)
					{
						PlayerData data = (PlayerData)mPlayerData[i];
						if (data.mDrafted)
						{
							outFile.WriteLine("\"" + data.mName + "\",\"" + data.mPositionGroup + "\",\"" + data.mCollege + "\"");
						}
					}
					outFile.Close();
				}
			}
		}

		private void importDrafteesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Draftee Exports (*.draftees)|*.draftees|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				using (System.IO.StreamReader inFile = new System.IO.StreamReader(dlg.FileName))
				{
					while (!inFile.EndOfStream)
					{
						string curLine = inFile.ReadLine();
						string[] fields = DataReader.CSVHelper.ParseLine(curLine);
						if (fields.Length != 3)
						{
							MessageBox.Show("Line in file does not have 3 fields" + Environment.NewLine + curLine,
								"Error Importing Drafted Players",
								MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}
						else
						{
							FindAndMarkDraftee(fields[0], fields[1], fields[2], 0);
						}
					}
					inFile.Close();
				}
			}
		}

		private void useCurrentSortAsDraftOrderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			int curDraftOrder = 0;
			foreach (ListViewItem curItem in listViewDraftees.Items)
			{
				PlayerData curData = (PlayerData)(curItem.Tag);
				curData.mDesiredOrder = curDraftOrder;
				curData.mDraftPosition = DraftPosition.NotSet;
				mDraftOrderList[curDraftOrder] = curData.mOriginalOrder;
				DisplayPlayerData(curData.mOriginalOrder);
				++curDraftOrder;
			}
		}

		private void exportDraftOrderConscriptorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Draft Orders (*.csv)|*.csv|All files (*.*)|*.*";
			dlg.FileName = "DraftOrder.csv";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				using (System.IO.StreamWriter outFile = new System.IO.StreamWriter(dlg.FileName))
				{
					outFile.WriteLine("Name,Position,BirthDate");
					int numberToWrite = System.Math.Min(100, mPlayerData.Count);
					int numberWritten = 0;
					int draftOrder = 0;
					while (numberWritten < numberToWrite && draftOrder < mPlayerData.Count)
					{
						PlayerData curData = (PlayerData)mPlayerData[mDraftOrderList[draftOrder]];
						if (!curData.mDrafted)
						{
							outFile.Write("\"" + curData.mName + "\",");
							outFile.Write("\"" + curData.mPosition + "\",");
							outFile.Write("\"" + curData.mBirthDate + "\"");
							outFile.WriteLine();
							numberWritten += 1;
						}
						draftOrder += 1;
					}
				}
			}
		}

		private void UpdateSelectedPlayerDraftPosition(DraftPosition newPosition, bool goToTop, bool updateUI)
		{
			if (mSelectedPlayerData != null)
			{
				mSelectedPlayerData.mDraftPosition = newPosition;
				int oldDraftOrder = mSelectedPlayerData.mDesiredOrder;
				int newDraftOrder = mPlayerData.Count-1;
				for (int i = 0; i < mDraftOrderList.Values.Count; ++i)
				{
					int playerIndex = mDraftOrderList.Values[i];
					PlayerData data = (PlayerData)mPlayerData[playerIndex];
					if (data.mDraftPosition == newPosition)
					{
						newDraftOrder = i;
						if (goToTop)
						{
							break;
						}
					}
					else if (data.mDraftPosition > newPosition)
					{
						// If we're going to be moving up in the world, this is where we want to insert.
						// If we're going to be moving down in the world, we want to go to the one before.
						if (i < oldDraftOrder)
						{
							newDraftOrder = i;
						}
						else if (i > 0)
						{
							newDraftOrder = i - 1;
						}
						break;
					}
				}
				UpdateSelectedPlayerDraftOrder(newPosition, oldDraftOrder, newDraftOrder, updateUI);
			}
		}

		private void UpdateSelectedPlayerDraftOrder(DraftPosition newPosition, int oldDraftOrder, int newDraftOrder, bool updateUI)
		{
			if (oldDraftOrder != newDraftOrder)
			{
				Cursor oldCursor = Cursor;
				if (updateUI)
				{
					Cursor = Cursors.WaitCursor;
					toolStripStatusLabelAction.Text = "Fixing order...";
					toolStripProgressBarAction.Value = 0;
					statusStripMain.Refresh();
					toolStripProgressBarAction.Maximum = Math.Abs(newDraftOrder - oldDraftOrder);
				}

				int draftOrder;
				if (oldDraftOrder > newDraftOrder)
				{
					for (draftOrder = oldDraftOrder; draftOrder > newDraftOrder; --draftOrder)
					{
						mDraftOrderList[draftOrder] = mDraftOrderList[draftOrder - 1];
						PlayerData data = (PlayerData)mPlayerData[mDraftOrderList[draftOrder]];
						data.mDesiredOrder = draftOrder;
						if (updateUI)
						{
							DisplayPlayerData(mDraftOrderList[draftOrder]);
							toolStripProgressBarAction.Value += 1;
						}
					}
				}
				else
				{
					for (draftOrder = oldDraftOrder; draftOrder < newDraftOrder; ++draftOrder)
					{
						mDraftOrderList[draftOrder] = mDraftOrderList[draftOrder + 1];
						PlayerData data = (PlayerData)mPlayerData[mDraftOrderList[draftOrder]];
						data.mDesiredOrder = draftOrder;
						if (updateUI)
						{
							DisplayPlayerData(mDraftOrderList[draftOrder]);
							toolStripProgressBarAction.Value += 1;
						}
					}
				}

				mDraftOrderList[newDraftOrder] = mSelectedPlayerData.mOriginalOrder;

				if (updateUI)
				{
					Cursor = oldCursor;

					toolStripStatusLabelAction.Text = "Finished!";
					toolStripProgressBarAction.Value = 0;
				}
			}
			mSelectedPlayerData.mDesiredOrder = newDraftOrder;
			mSelectedPlayerData.mDraftPosition = newPosition;
			if (updateUI)
			{
				DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
			}
		}

		private void MoveSelectedPlayerUp()
		{
			if (mSelectedPlayerData != null)
			{
				int oldDraftOrder = mSelectedPlayerData.mDesiredOrder;
				int newDraftOrder = oldDraftOrder - 1;
				if (newDraftOrder >= 0)
				{
					int playerIndex = mDraftOrderList[newDraftOrder];
					if (((PlayerData)mPlayerData[playerIndex]).mDraftPosition == mSelectedPlayerData.mDraftPosition)
					{
						UpdateSelectedPlayerDraftOrder(mSelectedPlayerData.mDraftPosition, oldDraftOrder, newDraftOrder, true);
					}
				}
			}
		}

		private void MoveSelectedPlayerDown()
		{
			if (mSelectedPlayerData != null)
			{
				int oldDraftOrder = mSelectedPlayerData.mDesiredOrder;
				int newDraftOrder = oldDraftOrder + 1;
				if (newDraftOrder < mDraftOrderList.Count)
				{
					int playerIndex = mDraftOrderList[newDraftOrder];
					if (((PlayerData)mPlayerData[playerIndex]).mDraftPosition == mSelectedPlayerData.mDraftPosition)
					{
						UpdateSelectedPlayerDraftOrder(mSelectedPlayerData.mDraftPosition, oldDraftOrder, newDraftOrder, true);
					}
				}
			}
		}

		private void firstRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FirstRound, true, true);
		}

		private void firstRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FirstRound, false, true);
		}

		private void secondRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SecondRound, true, true);
		}

		private void secondRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SecondRound, false, true);
		}

		private void thirdRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.ThirdRound, true, true);
		}

		private void thirdRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.ThirdRound, false, true);
		}

		private void fourthRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FourthRound, true, true);
		}

		private void fourthRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FourthRound, false, true);
		}

		private void fifthRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FifthRound, true, true);
		}

		private void fifthRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FifthRound, false, true);
		}

		private void sixthRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SixthRound, true, true);
		}

		private void sixthRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SixthRound, false, true);
		}

		private void seventhRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SeventhRound, true, true);
		}

		private void seventhRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SeventhRound, false, true);
		}

		private void undraftedFATop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.UndraftedFA, true, true);
		}

		private void undraftedFABottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.UndraftedFA, false, true);
		}

		private void dontDraftToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.DontDraft, false, true);
		}

		private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MoveSelectedPlayerUp();
		}

		private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void listViewDraftees_KeyDataEvent(object sender, DraftPoolListView.KeyDataEventArgs e)
		{
			if (listViewDraftees.Focused)
			{
				if (e.KeyData == Keys.D1 || e.KeyData == Keys.NumPad1)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.FirstRound, false, true);
				}
				else if (e.KeyData == Keys.D2 || e.KeyData == Keys.NumPad2)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.SecondRound, false, true);
				}
				else if (e.KeyData == Keys.D3 || e.KeyData == Keys.NumPad3)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.ThirdRound, false, true);
				}
				else if (e.KeyData == Keys.D4 || e.KeyData == Keys.NumPad4)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.FourthRound, false, true);
				}
				else if (e.KeyData == Keys.D5 || e.KeyData == Keys.NumPad5)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.FifthRound, false, true);
				}
				else if (e.KeyData == Keys.D6 || e.KeyData == Keys.NumPad6)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.SixthRound, false, true);
				}
				else if (e.KeyData == Keys.D7 || e.KeyData == Keys.NumPad7)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.SeventhRound, false, true);
				}
				else if (e.KeyData == Keys.F)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.UndraftedFA, false, true);
				}
				else if (e.KeyData == Keys.X)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.DontDraft, false, true);
				}
				else if (e.KeyData == Keys.D)
				{
					DraftSelectedPlayer();
				}
				else if (e.KeyData == Keys.M)
				{
					MarkSelectedPlayer();
				}
				else if (e.KeyData == Keys.OemMinus || e.KeyData == Keys.Subtract)
				{
					MoveSelectedPlayerDown();
				}
				else if (e.KeyData == Keys.Oemplus || e.KeyData == Keys.Add)
				{
					MoveSelectedPlayerUp();
				}
			}
		}

		// Selects and focuses an item when it is clicked anywhere along 
		// its width. The click must normally be on the parent item text.
		private void listViewDraftees_MouseUp(object sender, MouseEventArgs e)
		{
			ListViewItem clickedItem = listViewDraftees.GetItemAt(5, e.Y);
			if (clickedItem != null)
			{
				clickedItem.Selected = true;
				clickedItem.Focused = true;
			}
		}

		private void editWeightsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (mWeightsForm.ShowDialog() == DialogResult.OK)
			{
				DisplayPlayerData();
			}
		}

		private Dictionary<string,string[]> mPositionGroupAttributeNames;
		private Dictionary<string, bool[]> mPositionGroupAttributeNotMasked;
		private string[] mAllAttributeNames;
		private int[] mAttributeColumns;
		private const int kHighAttributeOffset = 58;

		private void InitializeSorters()
		{
			// Initially sort on draft order
			listViewDraftees.ListViewItemSorter = null;
			columnHeader40.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderAgility.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderBench.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderBroadJump.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderCombineSum.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderDraftOrder.Tag = WindowsUtilities.SortType.SortByInteger;
			columnHeaderDraftRound.Tag = WindowsUtilities.SortType.SortByString;
			columnHeaderGrade.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderName.Tag = WindowsUtilities.SortType.SortByString;
			columnHeaderPercentDeveloped.Tag = WindowsUtilities.SortType.SortByInteger;
			columnHeaderBars.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderPosition.Tag = WindowsUtilities.SortType.SortByIntegerTag;
			columnHeaderPositionDrill.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderRating.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderSolecismic.Tag = WindowsUtilities.SortType.SortByDouble;
		}

		private void listViewDraftees_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			WindowsUtilities.SortTypeListViewItemSorter.UpdateSortColumn(listViewDraftees, e.Column, sortDraftedToBottomToolStripMenuItem.Checked);
		}

		private void sortDraftedToBottomToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			WindowsUtilities.SortTypeListViewItemSorter.UpdateSortColumn(listViewDraftees, -1, sortDraftedToBottomToolStripMenuItem.Checked);
			mSettings.WriteXMLValue(kSettingsRoot, kSortDraftedToBottom, sortDraftedToBottomToolStripMenuItem.Checked);
		}

		private void colorChemistryGroupsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			mSettings.WriteXMLValue(kSettingsRoot, kColorChemistryGroups, colorChemistryGroupsToolStripMenuItem.Checked);
			DisplayPlayerData();
		}

		#region Maps
		private void InitializeMaps()
		{
			InitializePositionGroupOrderMap();
			InitializeCombineMap();
			InitializeAttributeNames();
			InitializePositionToPositionGroupMap();
			InitializePositionSizeRangesMap();
		}

		private void InitializePositionSizeRangesMap()
		{
			mPositionSizeRangesMap = new Dictionary<string, PositionSizeRanges>();
			PositionSizeRanges newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 210;
			newRanges.BelowAverageWeightCap = 216;
			newRanges.AverageWeightCap = 223;
			newRanges.AboveAverageWeightCap = 232;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 74;
			mPositionSizeRangesMap["QB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 208;
			newRanges.BelowAverageWeightCap = 214;
			newRanges.AverageWeightCap = 221;
			newRanges.AboveAverageWeightCap = 230;
			newRanges.WellAboveAverageWeightCap = 265;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 71;
			mPositionSizeRangesMap["RB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 210;
			newRanges.WellBelowAverageWeightCap = 232;
			newRanges.BelowAverageWeightCap = 239;
			newRanges.AverageWeightCap = 246;
			newRanges.AboveAverageWeightCap = 256;
			newRanges.WellAboveAverageWeightCap = 189;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 72;
			mPositionSizeRangesMap["FB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 210;
			newRanges.WellBelowAverageWeightCap = 245;
			newRanges.BelowAverageWeightCap = 252;
			newRanges.AverageWeightCap = 260;
			newRanges.AboveAverageWeightCap = 270;
			newRanges.WellAboveAverageWeightCap = 289;
			newRanges.MinHeight = 74;
			newRanges.AverageHeight = 76;
			mPositionSizeRangesMap["TE"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 188;
			newRanges.BelowAverageWeightCap = 195;
			newRanges.AverageWeightCap = 200;
			newRanges.AboveAverageWeightCap = 208;
			newRanges.WellAboveAverageWeightCap = 235;
			newRanges.MinHeight = 70;
			newRanges.AverageHeight = 72;
			mPositionSizeRangesMap["FL"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 187;
			newRanges.BelowAverageWeightCap = 193;
			newRanges.AverageWeightCap = 198;
			newRanges.AboveAverageWeightCap = 207;
			newRanges.WellAboveAverageWeightCap = 235;
			newRanges.MinHeight = 70;
			newRanges.AverageHeight = 72;
			mPositionSizeRangesMap["SE"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 276;
			newRanges.WellBelowAverageWeightCap = 298;
			newRanges.BelowAverageWeightCap = 307;
			newRanges.AverageWeightCap = 317;
			newRanges.AboveAverageWeightCap = 329;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 77;
			mPositionSizeRangesMap["LT"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 270;
			newRanges.WellBelowAverageWeightCap = 298;
			newRanges.BelowAverageWeightCap = 305;
			newRanges.AverageWeightCap = 315;
			newRanges.AboveAverageWeightCap = 327;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 76;
			mPositionSizeRangesMap["LG"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 265;
			newRanges.WellBelowAverageWeightCap = 279;
			newRanges.BelowAverageWeightCap = 288;
			newRanges.AverageWeightCap = 296;
			newRanges.AboveAverageWeightCap = 308;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 75;
			mPositionSizeRangesMap["C"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 270;
			newRanges.WellBelowAverageWeightCap = 301;
			newRanges.BelowAverageWeightCap = 313;
			newRanges.AverageWeightCap = 320;
			newRanges.AboveAverageWeightCap = 332;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 75;
			mPositionSizeRangesMap["RG"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 276;
			newRanges.WellBelowAverageWeightCap = 306;
			newRanges.BelowAverageWeightCap = 315;
			newRanges.AverageWeightCap = 325;
			newRanges.AboveAverageWeightCap = 336;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 77;
			mPositionSizeRangesMap["RT"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 201;
			newRanges.BelowAverageWeightCap = 207;
			newRanges.AverageWeightCap = 214;
			newRanges.AboveAverageWeightCap = 222;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 74;
			mPositionSizeRangesMap["P"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 191;
			newRanges.BelowAverageWeightCap = 197;
			newRanges.AverageWeightCap = 203;
			newRanges.AboveAverageWeightCap = 211;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 72;
			mPositionSizeRangesMap["K"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 255;
			newRanges.WellBelowAverageWeightCap = 266;
			newRanges.BelowAverageWeightCap = 273;
			newRanges.AverageWeightCap = 281;
			newRanges.AboveAverageWeightCap = 292;
			newRanges.WellAboveAverageWeightCap = 314;
			newRanges.MinHeight = 72;
			newRanges.AverageHeight = 76;
			mPositionSizeRangesMap["LDE"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 280;
			newRanges.WellBelowAverageWeightCap = 287;
			newRanges.BelowAverageWeightCap = 296;
			newRanges.AverageWeightCap = 305;
			newRanges.AboveAverageWeightCap = 317;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 72;
			newRanges.AverageHeight = 75;
			mPositionSizeRangesMap["LDT"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 280;
			newRanges.WellBelowAverageWeightCap = 289;
			newRanges.BelowAverageWeightCap = 298;
			newRanges.AverageWeightCap = 309;
			newRanges.AboveAverageWeightCap = 320;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 72;
			newRanges.AverageHeight = 75;
			mPositionSizeRangesMap["NT"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 280;
			newRanges.WellBelowAverageWeightCap = 287;
			newRanges.BelowAverageWeightCap = 296;
			newRanges.AverageWeightCap = 304;
			newRanges.AboveAverageWeightCap = 316;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 72;
			newRanges.AverageHeight = 75;
			mPositionSizeRangesMap["RDT"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 255;
			newRanges.WellBelowAverageWeightCap = 261;
			newRanges.BelowAverageWeightCap = 269;
			newRanges.AverageWeightCap = 277;
			newRanges.AboveAverageWeightCap = 288;
			newRanges.WellAboveAverageWeightCap = 314;
			newRanges.MinHeight = 73;
			newRanges.AverageHeight = 76;
			mPositionSizeRangesMap["RDE"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 220;
			newRanges.WellBelowAverageWeightCap = 230;
			newRanges.BelowAverageWeightCap = 237;
			newRanges.AverageWeightCap = 244;
			newRanges.AboveAverageWeightCap = 254;
			newRanges.WellAboveAverageWeightCap = 275;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 74;
			mPositionSizeRangesMap["SLB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 225;
			newRanges.WellBelowAverageWeightCap = 233;
			newRanges.BelowAverageWeightCap = 240;
			newRanges.AverageWeightCap = 247;
			newRanges.AboveAverageWeightCap = 257;
			newRanges.WellAboveAverageWeightCap = 280;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 73;
			mPositionSizeRangesMap["SILB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 225;
			newRanges.WellBelowAverageWeightCap = 238;
			newRanges.BelowAverageWeightCap = 242;
			newRanges.AverageWeightCap = 249;
			newRanges.AboveAverageWeightCap = 259;
			newRanges.WellAboveAverageWeightCap = 280;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 73;
			mPositionSizeRangesMap["MLB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 225;
			newRanges.WellBelowAverageWeightCap = 239;
			newRanges.BelowAverageWeightCap = 243;
			newRanges.AverageWeightCap = 250;
			newRanges.AboveAverageWeightCap = 260;
			newRanges.WellAboveAverageWeightCap = 280;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 73;
			mPositionSizeRangesMap["WILB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 220;
			newRanges.WellBelowAverageWeightCap = 227;
			newRanges.BelowAverageWeightCap = 234;
			newRanges.AverageWeightCap = 241;
			newRanges.AboveAverageWeightCap = 251;
			newRanges.WellAboveAverageWeightCap = 270;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 73;
			mPositionSizeRangesMap["WLB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 182;
			newRanges.BelowAverageWeightCap = 188;
			newRanges.AverageWeightCap = 193;
			newRanges.AboveAverageWeightCap = 201;
			newRanges.WellAboveAverageWeightCap = 225;
			newRanges.MinHeight = 69;
			newRanges.AverageHeight = 71;
			mPositionSizeRangesMap["LCB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 183;
			newRanges.BelowAverageWeightCap = 189;
			newRanges.AverageWeightCap = 194;
			newRanges.AboveAverageWeightCap = 202;
			newRanges.WellAboveAverageWeightCap = 225;
			newRanges.MinHeight = 69;
			newRanges.AverageHeight = 71;
			mPositionSizeRangesMap["RCB"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 197;
			newRanges.BelowAverageWeightCap = 203;
			newRanges.AverageWeightCap = 210;
			newRanges.AboveAverageWeightCap = 218;
			newRanges.WellAboveAverageWeightCap = 235;
			newRanges.MinHeight = 70;
			newRanges.AverageHeight = 72;
			mPositionSizeRangesMap["SS"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 0;
			newRanges.WellBelowAverageWeightCap = 195;
			newRanges.BelowAverageWeightCap = 201;
			newRanges.AverageWeightCap = 210;
			newRanges.AboveAverageWeightCap = 218;
			newRanges.WellAboveAverageWeightCap = 235;
			newRanges.MinHeight = 70;
			newRanges.AverageHeight = 72;
			mPositionSizeRangesMap["FS"] = newRanges;

			newRanges = new PositionSizeRanges();
			newRanges.MinWeight = 210;
			newRanges.WellBelowAverageWeightCap = 245;
			newRanges.BelowAverageWeightCap = 252;
			newRanges.AverageWeightCap = 278;
			newRanges.AboveAverageWeightCap = 308;
			newRanges.WellAboveAverageWeightCap = 999;
			newRanges.MinHeight = 0;
			newRanges.AverageHeight = 75;
			mPositionSizeRangesMap["LS"] = newRanges;
		}

		private void InitializePositionToPositionGroupMap()
		{
			mPositionToPositionGroupMap = new Dictionary<string, string>();
			mPositionToPositionGroupMap["QB"] = "QB";
			mPositionToPositionGroupMap["RB"] = "RB";
			mPositionToPositionGroupMap["FB"] = "FB";
			mPositionToPositionGroupMap["FL"] = "WR";
			mPositionToPositionGroupMap["SE"] = "WR";
			mPositionToPositionGroupMap["TE"] = "TE";
			mPositionToPositionGroupMap["LT"] = "T";
			mPositionToPositionGroupMap["RT"] = "T";
			mPositionToPositionGroupMap["LG"] = "G";
			mPositionToPositionGroupMap["RG"] = "G";
			mPositionToPositionGroupMap["C"] = "C";
			mPositionToPositionGroupMap["LDE"] = "DE";
			mPositionToPositionGroupMap["RDE"] = "DE";
			mPositionToPositionGroupMap["LDT"] = "DT";
			mPositionToPositionGroupMap["RDT"] = "DT";
			mPositionToPositionGroupMap["NT"] = "DT";
			mPositionToPositionGroupMap["WLB"] = "OLB";
			mPositionToPositionGroupMap["SLB"] = "OLB";
			mPositionToPositionGroupMap["WILB"] = "ILB";
			mPositionToPositionGroupMap["SILB"] = "ILB";
			mPositionToPositionGroupMap["MLB"] = "ILB";
			mPositionToPositionGroupMap["LCB"] = "CB";
			mPositionToPositionGroupMap["RCB"] = "CB";
			mPositionToPositionGroupMap["SS"] = "S";
			mPositionToPositionGroupMap["FS"] = "S";
			mPositionToPositionGroupMap["P"] = "P";
			mPositionToPositionGroupMap["K"] = "K";
			mPositionToPositionGroupMap["LS"] = "LS";
		}

		private void InitializePositionGroupOrderMap()
		{
			mPositionGroupOrderMap = new System.Collections.Generic.Dictionary<string, int>();
			int positionOrder = 0;
			mPositionGroupOrderMap.Add("QB", positionOrder++);
			mPositionGroupOrderMap.Add("RB", positionOrder++);
			mPositionGroupOrderMap.Add("FB", positionOrder++);
			mPositionGroupOrderMap.Add("WR", positionOrder++);
			mPositionGroupOrderMap.Add("TE", positionOrder++);
			mPositionGroupOrderMap.Add("T", positionOrder++);
			mPositionGroupOrderMap.Add("G", positionOrder++);
			mPositionGroupOrderMap.Add("C", positionOrder++);
			mPositionGroupOrderMap.Add("DE", positionOrder++);
			mPositionGroupOrderMap.Add("DT", positionOrder++);
			mPositionGroupOrderMap.Add("ILB", positionOrder++);
			mPositionGroupOrderMap.Add("OLB", positionOrder++);
			mPositionGroupOrderMap.Add("CB", positionOrder++);
			mPositionGroupOrderMap.Add("S", positionOrder++);
			mPositionGroupOrderMap.Add("P", positionOrder++);
			mPositionGroupOrderMap.Add("K", positionOrder++);
			mPositionGroupOrderMap.Add("LS", positionOrder++);

			m_PositionWeightsDefaultMap["QB"] = 1.137;
			m_PositionWeightsDefaultMap["RB"] = 1.058;
			m_PositionWeightsDefaultMap["FB"] = 0.805;
			m_PositionWeightsDefaultMap["TE"] = 0.867;
			m_PositionWeightsDefaultMap["WR"] = 1.036;
			m_PositionWeightsDefaultMap["C"] = 0.856;
			m_PositionWeightsDefaultMap["G"] = 0.945;
			m_PositionWeightsDefaultMap["T"] = 1.095;
			m_PositionWeightsDefaultMap["P"] = 0.529;
			m_PositionWeightsDefaultMap["K"] = 0.591;
			m_PositionWeightsDefaultMap["DE"] = 1.095;
			m_PositionWeightsDefaultMap["DT"] = 1.076;
			m_PositionWeightsDefaultMap["ILB"] = 0.971;
			m_PositionWeightsDefaultMap["OLB"] = 0.955;
			m_PositionWeightsDefaultMap["CB"] = 1.027;
			m_PositionWeightsDefaultMap["S"] = 0.938;
			m_PositionWeightsDefaultMap["LS"] = 0.2;
		}

		private void InitializeAttributeNames()
		{
			mAttributeLabels[0] = labelAttributes1;
			mAttributeLabels[1] = labelAttributes2;
			mAttributeLabels[2] = labelAttributes3;
			mAttributeLabels[3] = labelAttributes4;
			mAttributeLabels[4] = labelAttributes5;
			mAttributeLabels[5] = labelAttributes6;
			mAttributeLabels[6] = labelAttributes7;
			mAttributeLabels[7] = labelAttributes8;
			mAttributeLabels[8] = labelAttributes9;
			mAttributeLabels[9] = labelAttributes10;
			mAttributeLabels[10] = labelAttributes11;
			mAttributeLabels[11] = labelAttributes12;
			mAttributeLabels[12] = labelAttributes13;
			mAttributeLabels[13] = labelAttributes14;
			mAttributeLabels[14] = labelAttributes15;

			mAttributePictureBoxes[0] = pictureBoxAttributes1;
			mAttributePictureBoxes[1] = pictureBoxAttributes2;
			mAttributePictureBoxes[2] = pictureBoxAttributes3;
			mAttributePictureBoxes[3] = pictureBoxAttributes4;
			mAttributePictureBoxes[4] = pictureBoxAttributes5;
			mAttributePictureBoxes[5] = pictureBoxAttributes6;
			mAttributePictureBoxes[6] = pictureBoxAttributes7;
			mAttributePictureBoxes[7] = pictureBoxAttributes8;
			mAttributePictureBoxes[8] = pictureBoxAttributes9;
			mAttributePictureBoxes[9] = pictureBoxAttributes10;
			mAttributePictureBoxes[10] = pictureBoxAttributes11;
			mAttributePictureBoxes[11] = pictureBoxAttributes12;
			mAttributePictureBoxes[12] = pictureBoxAttributes13;
			mAttributePictureBoxes[13] = pictureBoxAttributes14;
			mAttributePictureBoxes[14] = pictureBoxAttributes15;

			mPositionGroupAttributeNames = new Dictionary<string,string[]>();
			mPositionGroupAttributeNotMasked = new Dictionary<string, bool[]>();

			// QB
			string[] attributeNames = new string[]
			{
				"Screen Passes (Ag25)",
				"Short Passes",
				"Medium Passes (Bj66)",
				"Long Passes (Bp50)",
				"Deep Passes (Bp50)",
				"Third Down Passing (Bj33)",
				"Accuracy (PD50)",
				"Timing (PD50)",
				"Sense Rush (Ag75)",
				"Read Defense (So10)",
				"Two Minute Offense",
				"Scramble Frequency (Ft85)",
				"Kick Holding"
			};
			bool[] attributesNotMasked = new bool[]
			{
				false, //"Screen Passes (Ag25)",
				false, //"Short Passes",
				false, //"Medium Passes (Bj66)",
				false, //"Long Passes (Bp50)",
				false, //"Deep Passes (Bp50)",
				false, //"Third Down Passing (Bj33)",
				false, //"Accuracy (PD50)",
				false, //"Timing (PD50)",
				true,  //"Sense Rush (Ag75)",
				false, //"Read Defense (So10)",
				false, //"Two Minute Offense",
				false, //"Scramble Frequency (Ft85)",
				false  //"Kick Holding"
			};
			mPositionGroupAttributeNames.Add("QB", attributeNames);
			mPositionGroupAttributeNotMasked.Add("QB", attributesNotMasked);

			// RB
			attributeNames = new string[]
			{
				"Breakaway Speed (Ft80)",
				"Power Inside (Bp100)",
				"Third Down Running (Ag33)",
				"Hole Recognition (So90)",
				"Elusiveness (Ag33)",
				"Speed to Outside (Bj50/Ft20)",
				"Blitz Pickup (PD90)",
				"Avoid Drops",
				"Getting Downfield (Ag33)",
				"Route Running",
				"Third Down Catching (PD05)",
				"Punt Returns",
				"Kick Returns",
				"Endurance (Bj50)",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				true, //"Breakaway Speed (Ft80)",
				true, //"Power Inside (Bp100)",
				false, //"Third Down Running (Ag33)",
				false, //"Hole Recognition (So90)",
				false, //"Elusiveness (Ag33)",
				true, //"Speed to Outside (Bj50/Ft20)",
				false, //"Blitz Pickup (PD90)",
				false, //"Avoid Drops",
				false, //"Getting Downfield (Ag33)",
				false, //"Route Running",
				false, //"Third Down Catching (PD05)",
				false, //"Punt Returns",
				false, //"Kick Returns",
				false, //"Endurance (Bj50)",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("RB", attributeNames);
			mPositionGroupAttributeNotMasked.Add("RB", attributesNotMasked);

			// FB
			attributeNames = new string[]
			{
				"Run Blocking (Bj50)",
				"Pass Blocking",
				"Blocking Strength",
				"Power Inside (Bp100)",
				"Third Down Running (Ag33Bj50)",
				"Hole Recognition (So50)",
				"Blitz Pickup (PD50)",
				"Avoid Drops",
				"Route Running (PD50)",
				"Third Down Catching",
				"Endurance",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Blocking (Bj50)",
				false, //"Pass Blocking",
				true, //"Blocking Strength",
				true, //"Power Inside (Bp100)",
				false, //"Third Down Running (Ag33Bj50)",
				false, //"Hole Recognition (So50)",
				false, //"Blitz Pickup (PD50)",
				false, //"Avoid Drops",
				false, //"Route Running (PD50)",
				false, //"Third Down Catching",
				false, //"Endurance",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("FB", attributeNames);
			mPositionGroupAttributeNotMasked.Add("FB", attributesNotMasked);

			// TE
			attributeNames = new string[]
			{
				"Run Blocking (Bj50)",
				"Pass Blocking",
				"Blocking Strength (Bp100)",
				"Avoid Drops (PD50)",
				"Getting Downfield (Ft50Ag100)",
				"Route Running (So50)",
				"Third Down Catching (Bj50)",
				"Big Play Receiving (Ft50)",
				"Courage",
				"Adjust to Ball (PD50)",
				"Endurance",
				"Special Teams",
				"Long Snapping"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Blocking (Bj50)",
				false, //"Pass Blocking",
				true, //"Blocking Strength (Bp100)",
				false, //"Avoid Drops (PD50)",
				false, //"Getting Downfield (Ft50Ag100)",
				false, //"Route Running (So50)",
				false, //"Third Down Catching (Bj50)",
				true, //"Big Play Receiving (Ft50)",
				false, //"Courage",
				false, //"Adjust to Ball (PD50)",
				false, //"Endurance",
				false, //"Special Teams",
				false //"Long Snapping"
			};
			mPositionGroupAttributeNames.Add("TE", attributeNames);
			mPositionGroupAttributeNotMasked.Add("TE", attributesNotMasked);

			// WR
			attributeNames = new string[]
			{
				"Avoid Drops (PD65)",
				"Getting Downfield (Ag100)",
				"Route Running (So50)",
				"Third Down Catching",
				"Big Play Receiving (Ft70)",
				"Courage (Bp100)",
				"Adjust to Ball (PD35)",
				"Punt Returns (Bj50)",
				"Kick Returns (Bj50)",
				"Endurance",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Avoid Drops (PD65)",
				false, //"Getting Downfield (Ag100)",
				false, //"Route Running (So50)",
				false, //"Third Down Catching",
				true, //"Big Play Receiving (Ft70)",
				false, //"Courage (Bp100)",
				false, //"Adjust to Ball (PD35)",
				false, //"Punt Returns (Bj50)",
				false, //"Kick Returns (Bj50)",
				false, //"Endurance",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("WR", attributeNames);
			mPositionGroupAttributeNotMasked.Add("WR", attributesNotMasked);

			// C
			attributeNames = new string[]
			{
				"Run Blocking (Ft100)",
				"Pass Blocking (Ag100)",
				"Blocking Strength (Bp100)",
				"Endurance (Bj100)",
				"Long Snapping"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Blocking (Ft100)",
				false, //"Pass Blocking (Ag100)",
				true, //"Blocking Strength (Bp100)",
				false, //"Endurance (Bj100)",
				false //"Long Snapping"
			};
			mPositionGroupAttributeNames.Add("C", attributeNames);
			mPositionGroupAttributeNotMasked.Add("C", attributesNotMasked);

			// G
			attributeNames = new string[]
			{
				"Run Blocking (Ft100)",
				"Pass Blocking (Ag100)",
				"Blocking Strength (Bp100)",
				"Endurance (Bj100)"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Blocking (Ft100)",
				false, //"Pass Blocking (Ag100)",
				true, //"Blocking Strength (Bp100)",
				false //"Endurance (Bj100)",
			};
			mPositionGroupAttributeNames.Add("G", attributeNames);
			mPositionGroupAttributeNotMasked.Add("G", attributesNotMasked);

			// T
			attributeNames = new string[]
			{
				"Run Blocking (Ft100)",
				"Pass Blocking (Ag100)",
				"Blocking Strength (Bp100)",
				"Endurance (Bj100)"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Blocking (Ft100)",
				false, //"Pass Blocking (Ag100)",
				true, //"Blocking Strength (Bp100)",
				false //"Endurance (Bj100)",
			};
			mPositionGroupAttributeNames.Add("T", attributeNames);
			mPositionGroupAttributeNotMasked.Add("T", attributesNotMasked);

			// P
			attributeNames = new string[]
			{
				"Punt Power (Ft100)",
				"Punt Hang Time (Bp100)",
				"Directional Punting (So50)",
				"Kick Holding"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Punt Power (Ft100)",
				false, //"Punt Hang Time (Bp100)",
				false, //"Directional Punting (So50)",
				false //"Kick Holding"
			};
			mPositionGroupAttributeNames.Add("P", attributeNames);
			mPositionGroupAttributeNotMasked.Add("P", attributesNotMasked);

			// K
			attributeNames = new string[]
			{
				"Kicking Accuracy (So50)",
				"Kicking Power (Bp100Bj50)",
				"Kickoff Distance (Ft100)",
				"Kickoff Hang Time (Bj50)"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Kicking Accuracy (So50)",
				false, //"Kicking Power (Bp100Bj50)",
				false, //"Kickoff Distance (Ft100)",
				false //"Kickoff Hang Time (Bj50)"
			};
			mPositionGroupAttributeNames.Add("K", attributeNames);
			mPositionGroupAttributeNotMasked.Add("K", attributesNotMasked);

			// DE
			attributeNames = new string[]
			{
				"Run Defense (Ag100)",
				"Pass Rush Technique (Ft100)",
				"Pass Rush Strength (Bp50)",
				"Play Diagnosis (So50)",
				"Punishing Hitter (Bp50)",
				"Endurance (Bj100)"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Defense (Ag100)",
				false, //"Pass Rush Technique (Ft100)",
				true, //"Pass Rush Strength (Bp50)",
				false, //"Play Diagnosis (So50)",
				true, //"Punishing Hitter (Bp50)",
				false //"Endurance (Bj100)"
			};
			mPositionGroupAttributeNames.Add("DE", attributeNames);
			mPositionGroupAttributeNotMasked.Add("DE", attributesNotMasked);

			// DT
			attributeNames = new string[]
			{
				"Run Defense (Ag100)",
				"Pass Rush Technique (Ft100)",
				"Pass Rush Strength (Bp50)",
				"Play Diagnosis (So50)",
				"Punishing Hitter (Bp50)",
				"Endurance (Bj100)"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Defense (Ag100)",
				false, //"Pass Rush Technique (Ft100)",
				true, //"Pass Rush Strength (Bp50)",
				false, //"Play Diagnosis (So50)",
				true, //"Punishing Hitter (Bp50)",
				false //"Endurance (Bj100)"
			};
			mPositionGroupAttributeNames.Add("DT", attributeNames);
			mPositionGroupAttributeNotMasked.Add("DT", attributesNotMasked);

			// ILB
			attributeNames = new string[]
			{
				"Run Defense (Ag100)",
				"Pass Rush Technique (Ft100)",
				"Man-to-Man Defense (Bj100)",
				"Zone Defense (PD50)",
				"Bump and Run Defense (Bp33)",
				"Pass Rush Strength (Bp33)",
				"Play Diagnosis (So50)",
				"Punishing Hitter (Bp33)",
				"Endurance",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Defense (Ag100)",
				false, //"Pass Rush Technique (Ft100)",
				false, //"Man-to-Man Defense (Bj100)",
				false, //"Zone Defense (PD50)",
				false, //"Bump and Run Defense (Bp33)",
				true, //"Pass Rush Strength (Bp33)",
				false, //"Play Diagnosis (So50)",
				true, //"Punishing Hitter (Bp33)",
				false, //"Endurance",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("ILB", attributeNames);
			mPositionGroupAttributeNotMasked.Add("ILB", attributesNotMasked);

			// OLB
			attributeNames = new string[]
			{
				"Run Defense (Ag100)",
				"Pass Rush Technique (Ft100)",
				"Man-to-Man Defense (Bj100)",
				"Zone Defense (PD50)",
				"Bump and Run Defense (Bp33)",
				"Pass Rush Strength (Bp33)",
				"Play Diagnosis (So50)",
				"Punishing Hitter (Bp33)",
				"Endurance",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Defense (Ag100)",
				false, //"Pass Rush Technique (Ft100)",
				false, //"Man-to-Man Defense (Bj100)",
				false, //"Zone Defense (PD50)",
				false, //"Bump and Run Defense (Bp33)",
				true, //"Pass Rush Strength (Bp33)",
				false, //"Play Diagnosis (So50)",
				true, //"Punishing Hitter (Bp33)",
				false, //"Endurance",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("OLB", attributeNames);
			mPositionGroupAttributeNotMasked.Add("OLB", attributesNotMasked);

			// CB
			attributeNames = new string[]
			{
				"Run Defense (Ag100)",
				"Man-to-Man Defense (Ft50)",
				"Zone Defense (Ft50PD50)",
				"Bump and Run Defense (Bp50)",
				"Play Diagnosis (So50)",
				"Punishing Hitter (Bp50)",
				"Interceptions (PD50)",
				"Punt Returns (Bj50)",
				"Kick Returns (Bj50)",
				"Endurance",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Defense (Ag100)",
				false, //"Man-to-Man Defense (Ft50)",
				false, //"Zone Defense (Ft50PD50)",
				false, //"Bump and Run Defense (Bp50)",
				false, //"Play Diagnosis (So50)",
				true, //"Punishing Hitter (Bp50)",
				false, //"Interceptions (PD50)",
				false, //"Punt Returns (Bj50)",
				false, //"Kick Returns (Bj50)",
				false, //"Endurance",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("CB", attributeNames);
			mPositionGroupAttributeNotMasked.Add("CB", attributesNotMasked);

			// S
			attributeNames = new string[]
			{
				"Run Defense (Ag100)",
				"Man-to-Man Defense (Ft50)",
				"Zone Defense (Ft50PD50)",
				"Bump and Run Defense (Bp50)",
				"Play Diagnosis (So50)",
				"Punishing Hitter (Bp50)",
				"Interceptions (PD50)",
				"Punt Returns (Bj50)",
				"Kick Returns (Bj50)",
				"Endurance",
				"Special Teams"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Run Defense (Ag100)",
				false, //"Man-to-Man Defense (Ft50)",
				false, //"Zone Defense (Ft50PD50)",
				false, //"Bump and Run Defense (Bp50)",
				false, //"Play Diagnosis (So50)",
				true, //"Punishing Hitter (Bp50)",
				false, //"Interceptions (PD50)",
				false, //"Punt Returns (Bj50)",
				false, //"Kick Returns (Bj50)",
				false, //"Endurance",
				false //"Special Teams"
			};
			mPositionGroupAttributeNames.Add("S", attributeNames);
			mPositionGroupAttributeNotMasked.Add("S", attributesNotMasked);

			// LS
			attributeNames = new string[]
			{
				"Long Snapping"
			};
			attributesNotMasked = new bool[]
			{
				false //"Snapping",
			};
			mPositionGroupAttributeNames.Add("LS", attributeNames);
			mPositionGroupAttributeNotMasked.Add("LS", attributesNotMasked);

			mAllAttributeNames = new string[]
			{
				"Accuracy",
				"Adjust to Ball",
				"Avoid Drops",
				"Big Play Receiving",
				"Blitz Pickup",
				"Blocking Strength",
				"Breakaway Speed",
				"Bump and Run Defense",
				"Courage",
				"Deep Passes",
				"Directional Punting",
				"Elusiveness",
				"Endurance",
				"Getting Downfield",
				"Hole Recognition",
				"Interceptions",
				"Kick Holding",
				"Kick Returns",
				"Kicking Accuracy",
				"Kicking Power",
				"Kickoff Distance",
				"Kickoff Hang Time",
				"Long Passes",
				"Long Snapping",
				"Man-to-Man Defense",
				"Medium Passes",
				"Pass Blocking",
				"Pass Rush Strength",
				"Pass Rush Technique",
				"Power Inside",
				"Punt Hang Time",
				"Punt Power",
				"Punt Returns",
				"Play Diagnosis",
				"Punishing Hitter",
				"Read Defense",
				"Route Running",
				"Run Blocking",
				"Run Defense",
				"Scramble Frequency",
				"Screen Passes",
				"Sense Rush",
				"Short Passes",
				"Special Teams",
				"Speed to Outside",
				"Third Down Passing",
				"Third Down Running",
				"Timing",
				"Two Minute Offense",
				"Third Down Catching",
				"Zone Defense"
			};

			mAttributeColumns = new int[]
			{
				10,	// "Accuracy",
				33,	// "Adjust to Ball",
				27,	// "Avoid Drops",
				31,	// "Big Play Receiving",
				26,	// "Blitz Pickup",
				39,	// "Blocking Strength",
				20,	// "Breakaway Speed",
				51,	// "Bump and Run Defense",
				32,	// "Courage",
				6,	// "Deep Passes",
				42,	// "Directional Punting",
				24,	// "Elusiveness",
				56,	// "Endurance",
				28,	// "Getting Downfield",
				23,	// "Hole Recognition",
				55,	// "Interceptions",
				59, // "Kick Holding",
				35,	// "Kick Returns",
				45,	// "Kicking Accuracy",
				46,	// "Kicking Power",
				43,	// "Kickoff Distance",
				44,	// "Kickoff Hang Time",
				5,	// "Long Passes",
				58, // "Long Snapping",
				49,	// "Man-to-Man Defense",
				4,	// "Medium Passes",
				38,	// "Pass Blocking",
				52,	// "Pass Rush Strength",
				48,	// "Pass Rush Technique",
				21,	// "Power Inside",
				41,	// "Punt Hang Time",
				40,	// "Punt Power",
				34,	// "Punt Returns",
				53,	// "Play Diagnosis",
				54,	// "Punishing Hitter",
				13,	// "Read Defense",
				29,	// "Route Running",
				37,	// "Run Blocking",
				47,	// "Run Defense",
				8,	// "Scramble Frequency",
				2,	// "Screen Passes",
				12,	// "Sense Rush",
				3,	// "Short Passes",
				57,	// "Special Teams",
				25,	// "Speed to Outside",
				7,	// "Third Down Passing",
				22,	// "Third Down Running",
				11,	// "Timing",
				14,	// "Two Minute Offense",
				30,	// "Third Down Catching",
				50	// "Zone Defense"
			};
		}

		private enum CombineOrder
		{
			Dash,
			Solecismic,
			Bench,
			Agility,
			BroadJump,
			PositionDrill
		}

		private ushort[,] CombineThresholds =
		{
			{ 0,28,10,780,0,0 },
			{ 465,0,0,735,114,17 },
			{ 478,0,20,0,104,22 },
			{ 451,0,0,720,0,42 },
			{ 478,0,22,775,102,0 },
			{ 527,0,28,780,84,0 },
			{ 527,0,27,790,0,0 },
			{ 531,0,25,800,0,0 },
			{ 485,0,27,760,0,0 },
			{ 0,0,28,780,0,0 },
			{ 0,0,21,760,107,0 },
			{ 0,0,18,740,108,22 },
			{ 452,0,12,720,0,37 },
			{ 459,0,15,735,0,37 },
			{ 497,0,10,0,0,0 },
			{ 0,23,9,0,104,0 },
			{ 531,0,25,800,0,0 },
		};

		private double[,] CombineAverages =
		{
			{4.79,28.0,9.5,7.87,102.6,71.2},
			{4.67,20.7,14.1,7.35,115.3,17.4},
			{4.81,21.9,19.8,7.58,103.9,23.4},
			{4.56,21.0,9.9,7.26,108.1,40.3},
			{4.84,23.3,20.5,7.95,103.0,28.0},
			{5.32,27.3,26.8,7.91,92.3,0.0},
			{5.29,28.0,26.7,8.02,90.0,0.0},
			{5.35,29.6,22.6,8.12,88.4,0.0},
			{4.90,23.6,25.7,7.68,107.8,0.0},
			{5.14,22.4,27.4,7.92,97.3,0.0},
			{4.88,27.1,20.3,7.68,104.4,23.4},
			{4.75,25.1,16.8,7.46,108.6,23.6},
			{4.55,22.2,10.9,7.26,107.2,35.0},
			{4.62,28.8,14.2,7.42,99.1,35.0},
			{5.09,25.6,10.1,7.69,106.8,0.0},
			{5.15,27.0,9.1,7.67,103.7,0.0},
			{5.35,29.6,22.6,8.12,88.4,0.0},
		};

		private double[,] CombineStandardDeviations =
		{
			{0.151,7.21,2.19,0.253,6.01,9.55},
			{0.093,6.45,3.44,0.151,5.14,6.30},
			{0.078,5.75,3.90,0.199,5.07,7.51},
			{0.104,6.99,3.09,0.194,4.92,9.94},
			{0.097,6.81,4.22,0.358,6.69,10.54},
			{0.166,10.24,5.07,0.268,6.66,0.00},
			{0.135,9.76,4.27,0.268,6.32,0.00},
			{0.133,9.87,4.82,0.220,6.69,0.00},
			{0.140,6.82,3.93,0.300,7.34,0.00},
			{0.143,6.54,3.71,0.331,9.43,0.00},
			{0.111,7.09,3.53,0.240,5.93,7.67},
			{0.119,7.13,3.49,0.199,5.86,7.72},
			{0.079,6.90,2.56,0.201,4.35,9.42},
			{0.091,7.59,2.97,0.224,4.31,9.41},
			{0.179,7.44,3.62,0.338,7.04,0.00},
			{0.136,7.47,2.87,0.337,6.40,0.00},
			{0.133,9.87,4.82,0.220,6.69,0.00},
		};

		private void InitializeCombineMap()
		{
			mPositionGroupCombineMap = new System.Collections.Hashtable();
			foreach (string posGroup in mPositionGroupOrderMap.Keys)
			{
				int posIndex = mPositionGroupOrderMap[posGroup];
				PositionGroupCombineData data = new PositionGroupCombineData();
				data.m40YardAverage = CombineAverages[posIndex,(int)CombineOrder.Dash];
				data.m40YardThreshold = CombineThresholds[posIndex,(int)CombineOrder.Dash] / 100.0;
				data.m40YardStdDev = CombineStandardDeviations[posIndex,(int)CombineOrder.Dash];
				data.mSolecismicAverage = CombineAverages[posIndex,(int)CombineOrder.Solecismic];
				data.mSolecismicThreshold = CombineThresholds[posIndex, (int)CombineOrder.Solecismic];
				data.mSolecismicStdDev = CombineStandardDeviations[posIndex, (int)CombineOrder.Solecismic];
				data.mBenchAverage = CombineAverages[posIndex,(int)CombineOrder.Bench];
				data.mBenchThreshold = CombineThresholds[posIndex, (int)CombineOrder.Bench];
				data.mBenchStdDev = CombineStandardDeviations[posIndex, (int)CombineOrder.Bench];
				data.mAgilityAverage = CombineAverages[posIndex,(int)CombineOrder.Agility];
				data.mAgilityThreshold = CombineThresholds[posIndex, (int)CombineOrder.Agility] / 100.0;
				data.mAgilityStdDev = CombineStandardDeviations[posIndex, (int)CombineOrder.Agility];
				data.mBroadJumpAverage = CombineAverages[posIndex,(int)CombineOrder.BroadJump];
				data.mBroadJumpThreshold = CombineThresholds[posIndex, (int)CombineOrder.BroadJump];
				data.mBroadJumpStdDev = CombineStandardDeviations[posIndex, (int)CombineOrder.BroadJump];
				data.mPositionDrillAverage = CombineAverages[posIndex,(int)CombineOrder.PositionDrill];
				data.mPositionDrillThreshold = CombineThresholds[posIndex, (int)CombineOrder.PositionDrill];
				data.mPositionDrillStdDev = CombineStandardDeviations[posIndex, (int)CombineOrder.PositionDrill];

				data.m40YardStdDev *= -1.0;
				data.mAgilityStdDev *= -1.0;

				mPositionGroupCombineMap.Add(posGroup, data);
			}
		}
        #endregion

        private void buttonReleaseNotes_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("da_release_notes.txt");
        }
    }
}
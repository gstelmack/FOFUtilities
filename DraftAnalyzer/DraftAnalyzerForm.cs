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
		}

		private class PlayerData
		{
			public string mName;
			public string mPosition;
			public string mPositionGroup;
			public string mCollege;
			public string mBirthDate;
			public string mHomeTown;
			public string mAgent;
			public int mHeight;
			public int mWeight;
			public int mVolatility;
			public int mLoyalty;
			public int mPlayForWinner;
			public int mLeadership;
			public int mIntelligence;
			public int mPersonality;
			public int mPopularity;
			public int mSolecismic;
			public double m40Time;
			public int mBench;
			public double mAgility;
			public int mBroadJump;
			public int mPositionDrill;
			public int mPercentDeveloped;
			public string mInterviewed;
			public string mImpression;
			public string mConflicts;
			public string mAffinities;
			public string mCharacter;
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

			public double mSolecismicRating;
			public double mFortyRating;
			public double mBenchRating;
			public double mAgilityRating;
			public double mBroadJumpRating;
			public double mPositionDrillRating;

			public double mAffinitiesFactor = 1.0;
			public double mConflictsFactor = 1.0;

			public List<PositionRating> mPositionRatings = new List<PositionRating>();
		}

		private class PositionGroupCombineData
		{
			public double mSolecismicAverage;
			public double mSolecismicStdDev;
			public double mSolecismicMin;
			public double mSolecismicMax;
			public double m40YardAverage;
			public double m40YardStdDev;
			public double m40YardMin;
			public double m40YardMax;
			public double mBenchAverage;
			public double mBenchStdDev;
			public double mBenchMin;
			public double mBenchMax;
			public double mAgilityAverage;
			public double mAgilityStdDev;
			public double mAgilityMin;
			public double mAgilityMax;
			public double mBroadJumpAverage;
			public double mBroadJumpStdDev;
			public double mBroadJumpMin;
			public double mBroadJumpMax;
			public double mPositionDrillAverage;
			public double mPositionDrillStdDev;
			public double mPositionDrillMin;
			public double mPositionDrillMax;
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

		private System.Collections.ArrayList mPlayerData = null;

        private WindowsUtilities.XMLSettings mSettings;
        private const string kSettingsRoot = "DraftAnalyzer";
        private const string kSortDraftedToBottom = "SortDraftedToBottom";

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

			mWeightsForm = new WeightsForm(mPositionGroupAttributeNames,mPositionToPositionGroupMap);
			mChemistryForm = new ChemistryForm();

            string settingsPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftAnalyzer.ini");
            mSettings = new WindowsUtilities.XMLSettings(settingsPath);
            sortDraftedToBottomToolStripMenuItem.Checked = mSettings.ReadXMLbool(kSettingsRoot, kSortDraftedToBottom, false);

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
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Extractor files (*.csv)|*.csv|All files (*.*)|*.*";
			dlg.FilterIndex = 0;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				UnselectPlayer();
				LoadExtractorFile(dlg.FileName);
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
					outFile.WriteLine("Name,Position,PositionGroup,College,BirthDate,HomeTown,Agent,Height,Weight," +
						"HeightBand,WeightBand,"+
						"Volatility,Loyalty,PlayForWinner,Leadership,Intelligence,Personality,Solecismic," +
							"40Time,Bench,Agility,BroadJump,PosDrill,Developed,Interviewed,Impression,Conflicts," +
								"Affinities,Character,Formations,OrgOrder,DesiredOrder,Drafted,DraftedOrder,Marked,DraftPosition," +
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
						outFile.Write("\"" + curData.mHomeTown + "\",");
						outFile.Write("\"" + curData.mAgent + "\",");
						outFile.Write(curData.mHeight + ",");
						outFile.Write(curData.mWeight + ",");
						outFile.Write("\"" + kHeightBands[(int)heightBand] + "\",");
						outFile.Write("\"" + kWeightBands[(int)weightBand] + "\",");
						outFile.Write(curData.mVolatility + ",");
						outFile.Write(curData.mLoyalty + ",");
						outFile.Write(curData.mPlayForWinner + ",");
						outFile.Write(curData.mLeadership + ",");
						outFile.Write(curData.mIntelligence + ",");
						outFile.Write(curData.mPersonality + ",");
						outFile.Write(curData.mSolecismic + ",");
						outFile.Write(curData.m40Time.ToString("F2") + ",");
						outFile.Write(curData.mBench + ",");
						outFile.Write(curData.mAgility.ToString("F2") + ",");
						outFile.Write(curData.mBroadJump + ",");
						outFile.Write(curData.mPositionDrill + ",");
						outFile.Write(curData.mPercentDeveloped + ",");
						outFile.Write("\"" + curData.mInterviewed + "\",");
						outFile.Write("\"" + curData.mImpression + "\",");
						outFile.Write("\"" + curData.mConflicts + "\",");
						outFile.Write("\"" + curData.mAffinities + "\",");
						outFile.Write("\"" + curData.mCharacter + "\",");
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

		private void ColorCombineRating(ListViewItem.ListViewSubItem subItem, double score, double avg, double stdDev)
		{
			if (score == 0.0)
			{
				subItem.BackColor = Color.Orange;
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
				if (mCombineDisplayType == CombineDisplayType.DisplayStdDevs)
				{
					subItem.Text = stdDevs.ToString("F2");
				}
			}
		}

		private void DisplayPlayerData(int i)
		{
			PlayerData data = (PlayerData)mPlayerData[i];
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
			else if (data.mConflicts.Length > 0 || data.mCharacter.Length > 0)
			{
				foreColor = Color.Red;
			}
			else if (data.mAffinities.Length > 0)
			{
				foreColor = Color.Green;
			}

			item.SubItems.Clear();
			item.Text = data.mName;
			PositionGroupCombineData combineData = (PositionGroupCombineData)mPositionGroupCombineMap[data.mPositionGroup];
			item.BackColor = backColor;
			item.ForeColor = foreColor;
			item.UseItemStyleForSubItems = false;
			ListViewItem.ListViewSubItem subItem = item.SubItems.Add(data.mPosition);
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem.Tag = mPositionGroupOrderMap[data.mPositionGroup];
			subItem = item.SubItems.Add(data.mRatedPosition);
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem.Tag = mPositionGroupOrderMap[mPositionToPositionGroupMap[data.mRatedPosition]];
			subItem = item.SubItems.Add(data.mVolatility.ToString());
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
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
			ColorCombineRating(subItem, (double)data.mSolecismic, combineData.mSolecismicAverage, combineData.mSolecismicStdDev);
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
			ColorCombineRating(subItem, (double)data.m40Time, combineData.m40YardAverage, combineData.m40YardStdDev);
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
			ColorCombineRating(subItem, (double)data.mBench, combineData.mBenchAverage, combineData.mBenchStdDev);
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
			ColorCombineRating(subItem, (double)data.mAgility, combineData.mAgilityAverage, combineData.mAgilityStdDev);
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
			ColorCombineRating(subItem, (double)data.mBroadJump, combineData.mBroadJumpAverage, combineData.mBroadJumpStdDev);
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
			ColorCombineRating(subItem, (double)data.mPositionDrill, combineData.mPositionDrillAverage, combineData.mPositionDrillStdDev);
			subItem = item.SubItems.Add(data.mPercentDeveloped.ToString());
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(data.mPositionRatings[0].AttributeScore.ToString("F1"));
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(data.mInterviewed);
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(data.mImpression);
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(data.mCombineSum.ToString("F2"));
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			ColorCombineSum(subItem, data.mCombineSum);
			subItem = item.SubItems.Add(data.mRating.ToString("F1"));
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(data.mOriginalOrder.ToString());
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(data.mDesiredOrder.ToString());
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;
			subItem = item.SubItems.Add(mDraftRoundNameMap[(int)data.mDraftPosition]);
			subItem.BackColor = backColor;
			subItem.ForeColor = foreColor;

			subItem = item.SubItems.Add("0");

			item.Tag = data;
		}

		private void CalculatePlayerData(PlayerData data)
		{
			data.mPositionRatings = new List<PositionRating>();
			data.mPositionRatings.Add(CalculatePositionRating(data, data.mPosition));
			string positionGroup = mPositionToPositionGroupMap[data.mPosition];
			foreach (string clonePosition in mPositionToPositionGroupMap.Keys)
			{
				if (clonePosition != data.mPosition && mPositionToPositionGroupMap[clonePosition] == positionGroup)
				{
					data.mPositionRatings.Add(CalculatePositionRating(data, clonePosition));
				}
			}

			data.mCombineSum = data.mPositionRatings[0].CombineScore;
			data.mRating = data.mPositionRatings[0].OverallScore;
			data.mRatedPosition = data.mPositionRatings[0].Position;

			foreach (PositionRating posRating in data.mPositionRatings)
			{
				if (posRating.OverallScore > data.mRating)
				{
					data.mCombineSum = posRating.CombineScore;
					data.mRating = posRating.OverallScore;
					data.mRatedPosition = posRating.Position;
				}
			}

            PredictAttributesFromCombines(data);
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

        private double CalculateCombineRating(PlayerData data, string position)
		{
			data.mSolecismicRating = 0.0;
			data.mFortyRating = 0.0;
			data.mAgilityRating = 0.0;
			data.mBenchRating = 0.0;
			data.mBroadJumpRating = 0.0;
			data.mPositionDrillRating = 0.0;
			PositionGroupCombineData combineData = (PositionGroupCombineData)mPositionGroupCombineMap[mPositionToPositionGroupMap[position]];
			if (data.mSolecismic != 0 && data.m40Time != 0.0 && data.mBench != 0 && data.mAgility != 0.0 && data.mBroadJump != 0)
			{
				WeightsForm.PositionWeights posWeights = mWeightsForm.GetPositionWeight(position);
				data.mSolecismicRating = ((((double)data.mSolecismic) - combineData.mSolecismicAverage) / combineData.mSolecismicStdDev) * posWeights.Solecismic;
				data.mFortyRating =  ((((double)data.m40Time) - combineData.m40YardAverage) / combineData.m40YardStdDev) * posWeights.Dash;
				data.mBenchRating = ((((double)data.mBench) - combineData.mBenchAverage) / combineData.mBenchStdDev) * posWeights.Bench;
				data.mAgilityRating = ((((double)data.mAgility) - combineData.mAgilityAverage) / combineData.mAgilityStdDev) * posWeights.Agility;
				data.mBroadJumpRating = ((((double)data.mBroadJump) - combineData.mBroadJumpAverage) / combineData.mBroadJumpStdDev) * posWeights.BroadJump;
				if (combineData.mPositionDrillStdDev != 0.0)
				{
					data.mPositionDrillRating = ((((double)data.mPositionDrill) - combineData.mPositionDrillAverage) / combineData.mPositionDrillStdDev) * posWeights.PositionDrill;
				}
			}
			else
			{
				WeightsForm.PositionWeights posWeights = mWeightsForm.GetNoCombinePositionWeight(position);
				if (combineData.mSolecismicStdDev != 0.0)
				{
					data.mSolecismicRating = ((((double)data.mSolecismic) - combineData.mSolecismicAverage) / combineData.mSolecismicStdDev) * posWeights.Solecismic;
				}
				if (combineData.mPositionDrillStdDev != 0.0)
				{
					data.mPositionDrillRating = ((((double)data.mPositionDrill) - combineData.mPositionDrillAverage) / combineData.mPositionDrillStdDev) * posWeights.PositionDrill;
				}
			}
			double combineRating = data.mSolecismicRating + data.mFortyRating + data.mAgilityRating + data.mBenchRating + data.mBroadJumpRating + data.mPositionDrillRating;
			return combineRating;
		}

		private PositionRating CalculatePositionRating(PlayerData data, string position)
		{
			PositionRating posRating = new PositionRating();
			posRating.Position = position;
			posRating.CombineScore = CalculateCombineRating(data, position);

			WeightsForm.GlobalWeightData globalData = mWeightsForm.GlobalWeights;
			WeightsForm.PositionWeights posWeights = null;
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
				case WeightsForm.AttributeUsage.UseMin:
					{
						for (int attIndex = 0; attIndex < posWeights.Attributes.Length; ++attIndex)
						{
							attributesFactor += ((double)(data.mAttributes[attIndex * 2] * posWeights.Attributes[attIndex])) * 0.01;
						}
					}
					break;
				case WeightsForm.AttributeUsage.UseAverage:
					{
						for (int attIndex = 0; attIndex < posWeights.Attributes.Length; ++attIndex)
						{
							attributesFactor += ((double)((data.mAttributes[attIndex * 2] + data.mAttributes[(attIndex * 2) + 1]) *
								posWeights.Attributes[attIndex])) * 0.005;
						}
					}
					break;
				case WeightsForm.AttributeUsage.UseMax:
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
			if (data.mImpression == "Very Overrated")
			{
				scoutFactor -= (double)globalData.ScoutImpression;
			}
			else if (data.mImpression == "Very Underrated")
			{
				scoutFactor += (double)globalData.ScoutImpression;
			}
			else if (data.mImpression == "Underrated")
			{
				scoutFactor += ((double)globalData.ScoutImpression)*0.5;
			}
			else if (data.mImpression == "Overrated")
			{
				scoutFactor -= ((double)globalData.ScoutImpression) * 0.5;
			}
			else if (data.mImpression == "Hard to Read")
			{
				scoutFactor -= ((double)globalData.ScoutImpression) * 0.1;
			}
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
			if (data.mCharacter.Length > 1)
			{
				chemistryFactor -= (double)globalData.RedFlag;
			}
			posRating.ChemistryScore = chemistryFactor;

			posRating.OverallScore = posRating.CombineScore + posRating.SizeScore + posRating.ChemistryScore +
				posRating.AttributeScore + posRating.ScoutImpressionScore;
			posRating.OverallScore *= posWeights.Weight;

			return posRating;
		}

		private void DisplayPlayerData()
		{
			toolStripStatusLabelAction.Text = "Filling table...";
			statusStripMain.Refresh();
			listViewDraftees.SuspendLayout();
			listViewDraftees.Items.Clear();
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
				detailsText += "HomeTown: ";
				detailsText += data.mHomeTown;
				detailsText += Environment.NewLine;
				detailsText += "Agent: ";
				detailsText += data.mAgent;
				detailsText += Environment.NewLine;
				if (data.mCharacter.Length > 0)
				{
					detailsText += "Character: ";
					detailsText += data.mCharacter;
					detailsText += Environment.NewLine;
				}
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
				if (data.mInterviewed == "Yes")
				{
					detailsText += "Loyalty: ";
					detailsText += data.mLoyalty;
					detailsText += Environment.NewLine;
					detailsText += "Play For Winner: ";
					detailsText += data.mPlayForWinner;
					detailsText += Environment.NewLine;
					detailsText += "Leadership: ";
					detailsText += data.mLeadership;
					detailsText += Environment.NewLine;
					detailsText += "Intelligence: ";
					detailsText += data.mIntelligence;
					detailsText += Environment.NewLine;
					detailsText += "Personality: ";
					detailsText += data.mPersonality;
					detailsText += Environment.NewLine;
				}
				PositionRating pRat = data.mPositionRatings[0];
				detailsText += "Comb: ";
				detailsText += pRat.CombineScore.ToString("F2");
				detailsText += " Chem: ";
				detailsText += pRat.ChemistryScore.ToString("F2");
				detailsText += " Impr: ";
				detailsText += pRat.ScoutImpressionScore.ToString("F2");
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
				//listViewDraftees.RedrawItems(mSelectedPlayerListIndex, mSelectedPlayerListIndex, true);
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
				//listViewDraftees.RedrawItems(mSelectedPlayerListIndex, mSelectedPlayerListIndex, true);
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

		private const int kDraftListVersion = 7;
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
				outFile.Write(data.mHomeTown);
				outFile.Write(data.mAgent);
				outFile.Write(data.mHeight);
				outFile.Write(data.mWeight);
				outFile.Write(data.mVolatility);
				outFile.Write(data.mLoyalty);
				outFile.Write(data.mPlayForWinner);
				outFile.Write(data.mLeadership);
				outFile.Write(data.mIntelligence);
				outFile.Write(data.mPersonality);
				outFile.Write(data.mPopularity);
				outFile.Write(data.mSolecismic);
				outFile.Write(data.m40Time);
				outFile.Write(data.mBench);
				outFile.Write(data.mAgility);
				outFile.Write(data.mBroadJump);
				outFile.Write(data.mPositionDrill);
				outFile.Write(data.mPercentDeveloped);
				outFile.Write(data.mInterviewed);
				outFile.Write(data.mImpression);
				outFile.Write(data.mConflicts);
				outFile.Write(data.mAffinities);
				outFile.Write(data.mCharacter);
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

				if (version >= 4)
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
					data.mHomeTown = inFile.ReadString();
					data.mAgent = inFile.ReadString();
					data.mHeight = inFile.ReadInt32();
					data.mWeight = inFile.ReadInt32();
					data.mVolatility = inFile.ReadInt32();
					data.mLoyalty = inFile.ReadInt32();
					data.mPlayForWinner = inFile.ReadInt32();
					data.mLeadership = inFile.ReadInt32();
					data.mIntelligence = inFile.ReadInt32();
					data.mPersonality = inFile.ReadInt32();
					if (version >= 6)
					{
						data.mPopularity = inFile.ReadInt32();
					}
					data.mSolecismic = inFile.ReadInt32();
					data.m40Time = inFile.ReadDouble();
					data.mBench = inFile.ReadInt32();
					data.mAgility = inFile.ReadDouble();
					data.mBroadJump = inFile.ReadInt32();
					data.mPositionDrill = inFile.ReadInt32();
					data.mPercentDeveloped = inFile.ReadInt32();
					data.mInterviewed = inFile.ReadString();
					data.mImpression = inFile.ReadString();
					if (version >= 3)
					{
						data.mConflicts = inFile.ReadString();
						data.mAffinities = inFile.ReadString();
						data.mCharacter = inFile.ReadString();
					}
					data.mFormations = inFile.ReadInt32();
					data.mOriginalOrder = inFile.ReadInt32();
					data.mDesiredOrder = inFile.ReadInt32();
					data.mDrafted = inFile.ReadBoolean();
					data.mMarked = inFile.ReadBoolean();
					if (version > 1)
					{
						string draftPosition = inFile.ReadString();
						data.mDraftPosition = (DraftPosition)Enum.Parse(typeof(DraftPosition), draftPosition);
					}
					else
					{
						data.mDraftPosition = DraftPosition.NotSet;
					}
					data.mAttributes = new int[kMaxAttributeCounts * 2];
					for (int i = 0; i < data.mAttributes.Length; i++)
					{
						data.mAttributes[i] = inFile.ReadInt32();
					}

					if (version >= 5)
					{
						data.mConflictsFactor = inFile.ReadDouble();
						data.mAffinitiesFactor = inFile.ReadDouble();
					}
					else
					{
						data.mAffinitiesFactor = 1.0;
						data.mConflictsFactor = 1.0;
					}

					if (version >= 7)
					{
						data.mSolecismicRating = inFile.ReadDouble();
						data.mFortyRating = inFile.ReadDouble();
						data.mBenchRating = inFile.ReadDouble();
						data.mAgilityRating = inFile.ReadDouble();
						data.mBroadJumpRating = inFile.ReadDouble();
						data.mPositionDrillRating = inFile.ReadDouble();
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

		private void LoadExtractorFile(string filename)
		{
			try
			{
				using (System.IO.StreamReader inFile = new System.IO.StreamReader(filename))
				{
					mPlayerData = new System.Collections.ArrayList();
					mDraftOrderList = new SortedList<int, int>();

					System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;

					string headerLine = inFile.ReadLine();
					bool hasCurrentAndFuture = headerLine.Contains(",Cur,Fut");
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
							newData.mName = fields[0];
							newData.mPosition = fields[1];
							newData.mPositionGroup = fields[2];
							newData.mCollege = fields[3];
							newData.mBirthDate = fields[5];
							newData.mHomeTown = fields[6];
							newData.mAgent = fields[7];
							newData.mHeight = Int32.Parse(fields[9], nfi);
							newData.mWeight = Int32.Parse(fields[10], nfi);
							newData.mVolatility = Int32.Parse(fields[12], nfi);
							newData.mLoyalty = Int32.Parse(fields[14], nfi);
							newData.mPlayForWinner = Int32.Parse(fields[15], nfi);
							newData.mLeadership = Int32.Parse(fields[16], nfi);
							newData.mIntelligence = Int32.Parse(fields[17], nfi);
							newData.mPersonality = Int32.Parse(fields[18], nfi);
							newData.mPopularity = Int32.Parse(fields[19], nfi);
							newData.mSolecismic = Int32.Parse(fields[21], nfi);
							newData.m40Time = Double.Parse(fields[22],nfi);
							newData.mBench = Int32.Parse(fields[23], nfi);
							newData.mAgility = Double.Parse(fields[24], nfi);
							newData.mBroadJump = Int32.Parse(fields[25], nfi);
							newData.mPositionDrill = Int32.Parse(fields[26], nfi);
							newData.mPercentDeveloped = Int32.Parse(fields[27], nfi);
							newData.mInterviewed = fields[28];
							newData.mImpression = fields[29];
							int attributeStartField = 30;
							if (hasCurrentAndFuture)
							{
								attributeStartField += 2;
							}
							if (hasChemistry)
							{
								newData.mConflicts = fields[attributeStartField++];
								newData.mAffinities = fields[attributeStartField++];
								newData.mCharacter = fields[attributeStartField++];
							}
							else
							{
								newData.mConflicts = "";
								newData.mAffinities = "";
								newData.mCharacter = "";
							}
							newData.mFormations = 0;
							if (newData.mPositionGroup == "QB")
							{
								newData.mFormations = Int32.Parse(fields[attributeStartField]);
								attributeStartField += 1;
							}
							int attributeIndex = 0;
							while (attributeStartField < fields.Length)
							{
								newData.mAttributes[attributeIndex++] = Int32.Parse(fields[attributeStartField++], nfi);
							}
							
							newData.mOriginalOrder = mPlayerData.Count;
							newData.mDesiredOrder = mPlayerData.Count;
							newData.mDrafted = false;
							newData.mMarked = false;
							newData.mOrderDrafted = -1;
							newData.mDraftPosition = DraftPosition.NotSet;
							mDraftOrderList.Add(newData.mDesiredOrder, mPlayerData.Count);
							mPlayerData.Add(newData);
#if !DEBUG
						}
						catch (FormatException)
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
				}
			}
			catch (System.IO.IOException e)
			{
				MessageBox.Show("Could not open file '" + filename + "': " + e.ToString(), "Error Loading Extractor File",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				int draftedOrderEnd = line.IndexOf(".");
				int playerNameStart = line.IndexOf(" - ");
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
					for (int draftOrder = 0; draftOrder < numberToWrite; ++draftOrder)
					{
						PlayerData curData = (PlayerData)mPlayerData[mDraftOrderList[draftOrder]];
						outFile.Write("\"" + curData.mName + "\",");
						outFile.Write("\"" + curData.mPosition + "\",");
						outFile.Write("\"" + curData.mBirthDate + "\"");
						outFile.WriteLine();
					}
				}
			}
		}

		private void UpdateSelectedPlayerDraftPosition(DraftPosition newPosition, bool goToTop)
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
				UpdateSelectedPlayerDraftOrder(newPosition, oldDraftOrder, newDraftOrder);
			}
		}

		private void UpdateSelectedPlayerDraftOrder(DraftPosition newPosition, int oldDraftOrder, int newDraftOrder)
		{
			if (oldDraftOrder != newDraftOrder)
			{
				Cursor oldCursor = Cursor;
				Cursor = Cursors.WaitCursor;

				toolStripStatusLabelAction.Text = "Fixing order...";
				toolStripProgressBarAction.Value = 0;
				statusStripMain.Refresh();
				toolStripProgressBarAction.Maximum = Math.Abs(newDraftOrder - oldDraftOrder);
				int draftOrder;
				if (oldDraftOrder > newDraftOrder)
				{
					for (draftOrder = oldDraftOrder; draftOrder > newDraftOrder; --draftOrder)
					{
						mDraftOrderList[draftOrder] = mDraftOrderList[draftOrder - 1];
						PlayerData data = (PlayerData)mPlayerData[mDraftOrderList[draftOrder]];
						data.mDesiredOrder = draftOrder;
						DisplayPlayerData(mDraftOrderList[draftOrder]);
						toolStripProgressBarAction.Value += 1;
					}
				}
				else
				{
					for (draftOrder = oldDraftOrder; draftOrder < newDraftOrder; ++draftOrder)
					{
						mDraftOrderList[draftOrder] = mDraftOrderList[draftOrder + 1];
						PlayerData data = (PlayerData)mPlayerData[mDraftOrderList[draftOrder]];
						data.mDesiredOrder = draftOrder;
						DisplayPlayerData(mDraftOrderList[draftOrder]);
						toolStripProgressBarAction.Value += 1;
					}
				}

				mDraftOrderList[newDraftOrder] = mSelectedPlayerData.mOriginalOrder;
				Cursor = oldCursor;

				toolStripStatusLabelAction.Text = "Finished!";
				toolStripProgressBarAction.Value = 0;
			}
			mSelectedPlayerData.mDesiredOrder = newDraftOrder;
			mSelectedPlayerData.mDraftPosition = newPosition;
			DisplayPlayerData(mSelectedPlayerData.mOriginalOrder);
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
						UpdateSelectedPlayerDraftOrder(mSelectedPlayerData.mDraftPosition, oldDraftOrder, newDraftOrder);
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
						UpdateSelectedPlayerDraftOrder(mSelectedPlayerData.mDraftPosition, oldDraftOrder, newDraftOrder);
					}
				}
			}
		}

		private void firstRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FirstRound, true);
		}

		private void firstRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FirstRound, false);
		}

		private void secondRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SecondRound, true);
		}

		private void secondRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SecondRound, false);
		}

		private void thirdRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.ThirdRound, true);
		}

		private void thirdRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.ThirdRound, false);
		}

		private void fourthRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FourthRound, true);
		}

		private void fourthRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FourthRound, false);
		}

		private void fifthRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FifthRound, true);
		}

		private void fifthRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.FifthRound, false);
		}

		private void sixthRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SixthRound, true);
		}

		private void sixthRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SixthRound, false);
		}

		private void seventhRoundTop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SeventhRound, true);
		}

		private void seventhRoundBottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.SeventhRound, false);
		}

		private void undraftedFATop_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.UndraftedFA, true);
		}

		private void undraftedFABottom_click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.UndraftedFA, false);
		}

		private void dontDraftToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UpdateSelectedPlayerDraftPosition(DraftPosition.DontDraft, false);
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
					UpdateSelectedPlayerDraftPosition(DraftPosition.FirstRound, false);
				}
				else if (e.KeyData == Keys.D2 || e.KeyData == Keys.NumPad2)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.SecondRound, false);
				}
				else if (e.KeyData == Keys.D3 || e.KeyData == Keys.NumPad3)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.ThirdRound, false);
				}
				else if (e.KeyData == Keys.D4 || e.KeyData == Keys.NumPad4)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.FourthRound, false);
				}
				else if (e.KeyData == Keys.D5 || e.KeyData == Keys.NumPad5)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.FifthRound, false);
				}
				else if (e.KeyData == Keys.D6 || e.KeyData == Keys.NumPad6)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.SixthRound, false);
				}
				else if (e.KeyData == Keys.D7 || e.KeyData == Keys.NumPad7)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.SeventhRound, false);
				}
				else if (e.KeyData == Keys.F)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.UndraftedFA, false);
				}
				else if (e.KeyData == Keys.X)
				{
					UpdateSelectedPlayerDraftPosition(DraftPosition.DontDraft, false);
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

		private void InitializeSorters()
		{
			// Initially sort on draft order
			listViewDraftees.ListViewItemSorter = new WindowsUtilities.SortTypeListViewItemSorter(16, WindowsUtilities.SortType.SortByInteger,false);
			columnHeader40.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderAgility.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderBench.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderBroadJump.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderCombineSum.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderDraftOrder.Tag = WindowsUtilities.SortType.SortByInteger;
			columnHeaderDraftRound.Tag = WindowsUtilities.SortType.SortByString;
			columnHeaderImpression.Tag = WindowsUtilities.SortType.SortByString;
			columnHeaderInterviewed.Tag = WindowsUtilities.SortType.SortByString;
			columnHeaderName.Tag = WindowsUtilities.SortType.SortByString;
			columnHeaderOriginalOrder.Tag = WindowsUtilities.SortType.SortByInteger;
			columnHeaderPercentDeveloped.Tag = WindowsUtilities.SortType.SortByInteger;
			columnHeaderBars.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderPosition.Tag = WindowsUtilities.SortType.SortByIntegerTag;
			columnHeaderPositionDrill.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderRatedPosition.Tag = WindowsUtilities.SortType.SortByIntegerTag;
			columnHeaderRating.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderSolecismic.Tag = WindowsUtilities.SortType.SortByDouble;
			columnHeaderVolatility.Tag = WindowsUtilities.SortType.SortByInteger;
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

		#region Maps
		private void InitializeMaps()
		{
			InitializeCombineMap();
			InitializeAttributeNames();
			InitializePositionGroupOrderMap();
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
				"Kicking Power (Ft100)",
				"Punt Hang Time (Bp100)",
				"Directional Punting (So50)",
				"Kick Holding"
			};
			attributesNotMasked = new bool[]
			{
				false, //"Kicking Power (Ft100)",
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
		}

		private void InitializeCombineMap()
		{
			System.Globalization.NumberFormatInfo nfi = System.Globalization.NumberFormatInfo.InvariantInfo;
			System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.Float;

			mPositionGroupCombineMap = new System.Collections.Hashtable();
			using (System.IO.StreamReader inFile = new System.IO.StreamReader("CombineData.csv"))
			{
				string headerLine = inFile.ReadLine();
				while (!inFile.EndOfStream)
				{
					string curLine = inFile.ReadLine();
					string[] fields = DataReader.CSVHelper.ParseLine(curLine);

					if (fields.Length == 25)
					{
						PositionGroupCombineData data = new PositionGroupCombineData();
						Double.TryParse(fields[1], ns, nfi, out data.m40YardAverage);
						Double.TryParse(fields[2], ns, nfi, out data.m40YardMin);
						Double.TryParse(fields[3], ns, nfi, out data.m40YardMax);
						Double.TryParse(fields[4], ns, nfi, out data.m40YardStdDev);
						Double.TryParse(fields[5], ns, nfi, out data.mSolecismicAverage);
						Double.TryParse(fields[6], ns, nfi, out data.mSolecismicMin);
						Double.TryParse(fields[7], ns, nfi, out data.mSolecismicMax);
						Double.TryParse(fields[8], ns, nfi, out data.mSolecismicStdDev);
						Double.TryParse(fields[9], ns, nfi, out data.mBenchAverage);
						Double.TryParse(fields[10], ns, nfi, out data.mBenchMin);
						Double.TryParse(fields[11], ns, nfi, out data.mBenchMax);
						Double.TryParse(fields[12], ns, nfi, out data.mBenchStdDev);
						Double.TryParse(fields[13], ns, nfi, out data.mAgilityAverage);
						Double.TryParse(fields[14], ns, nfi, out data.mAgilityMin);
						Double.TryParse(fields[15], ns, nfi, out data.mAgilityMax);
						Double.TryParse(fields[16], ns, nfi, out data.mAgilityStdDev);
						Double.TryParse(fields[17], ns, nfi, out data.mBroadJumpAverage);
						Double.TryParse(fields[18], ns, nfi, out data.mBroadJumpMin);
						Double.TryParse(fields[19], ns, nfi, out data.mBroadJumpMax);
						Double.TryParse(fields[20], ns, nfi, out data.mBroadJumpStdDev);
						Double.TryParse(fields[21], ns, nfi, out data.mPositionDrillAverage);
						Double.TryParse(fields[22], ns, nfi, out data.mPositionDrillMin);
						Double.TryParse(fields[23], ns, nfi, out data.mPositionDrillMax);
						Double.TryParse(fields[24], ns, nfi, out data.mPositionDrillStdDev);

						data.m40YardStdDev *= -1.0;
						data.mAgilityStdDev *= -1.0;

						mPositionGroupCombineMap.Add(fields[0], data);
					}
					else
					{
						MessageBox.Show("Bad line '" + curLine + "' in CombineData.csv", "Combine Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}
		#endregion
    }
}
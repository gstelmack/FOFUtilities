using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace PlayerTracker
{
    public class PlayerListData
    {
        public uint ID;

        private string m_Name;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private string m_PosGrp;
        public string PosGrp
        {
            get { return m_PosGrp; }
            set { m_PosGrp = value; }
        }
        public string Start
        {
            get
            {
                if (StartingCur != Byte.MaxValue)
                {
                    return StartingCur.ToString("D2") + "/" + StartingFut.ToString("D2");
                }
                else
                {
                    return "-/-";
                }

            }
        }
        public string Peak => PeakCur.ToString("D2") + "/" + PeakFut.ToString("D2");
        public string Current => PresentCur.ToString("D2") + "/" + PresentFut.ToString("D2");

        public byte ExpYears;
        public string Exp => ExpYears.ToString("D2");

        public string Recent
        {
            get
            {
                int recentCurChange = PresentCur - PreviousCur;
                return recentCurChange.ToString("+#;-#;0") + "/" + RecentChange.ToString("+#;-#;0");

            }
        }
        public string Overall
        {
            get
            {
                int overallCurChange = PresentCur - StartingCur;
                return overallCurChange.ToString("+#;-#;0") + "/" + OverallChange.ToString("+#;-#;0");
            }
        }
        public string Combine => CombineScore.ToString("F2");
        public string Bars => AttributeScore.ToString("F2");
        public string Score => OverallScore.ToString("F2");
        public string Dash => (CombineForty / 100.0).ToString("F2");
        public string Sol => CombineSolecismic.ToString();
        public string Bench => CombineBench.ToString();
        public string Agil => (CombineAgility / 100.0).ToString("F2");
        public string Jump => CombineBroadJump.ToString();
        public string Drill => CombinePositionDrill.ToString();
        public string Height
        {
            get
            {
                int feet = PlayerRecord.Height / 12;
                int inches = PlayerRecord.Height % 12;
                return feet.ToString() + "'" + inches.ToString("\"");
            }
        }
        public string Weight => PlayerRecord.Weight.ToString();

        public byte PresentCur;
		public byte PresentFut;
		public byte PreviousCur;
		public byte PreviousFut;
		public byte StartingCur;
		public byte StartingFut;
		public byte PeakCur;
		public byte PeakFut;
		public int RecentChange;
		public int OverallChange;
		public int PosOrder;
		public double PositionWeight;
		public double CombineScore;
		public double AttributeScore;
		public double OverallScore;
		public double SolecismicRating;
		public double FortyRating;
		public double AgilityRating;
		public double BenchRating;
		public double BroadJumpRating;
		public double PositionDrillRating;
        public byte CombineSolecismic;
        public ushort CombineForty;
        public ushort CombineAgility;
        public byte CombineBench;
        public byte CombineBroadJump;
        public byte CombinePositionDrill;
        private SolidColorBrush m_SolecismicForeground = Brushes.Black;
        private SolidColorBrush m_FortyForeground = Brushes.Black;
        private SolidColorBrush m_AgilityForeground = Brushes.Black;
        private SolidColorBrush m_BenchForeground = Brushes.Black;
        private SolidColorBrush m_BroadJumpForeground = Brushes.Black;
        private SolidColorBrush m_PositionDrillForeground = Brushes.Black;
        public SolidColorBrush SolecismicForeground { get { return m_SolecismicForeground; } set { m_SolecismicForeground = value; } }
        public SolidColorBrush FortyForeground { get { return m_FortyForeground; } set { m_FortyForeground = value; } }
        public SolidColorBrush AgilityForeground { get { return m_AgilityForeground; } set { m_AgilityForeground = value; } }
        public SolidColorBrush BenchForeground { get { return m_BenchForeground; } set { m_BenchForeground = value; } }
        public SolidColorBrush BroadJumpForeground { get { return m_BroadJumpForeground; } set { m_BroadJumpForeground = value; } }
        public SolidColorBrush PositionDrillForeground { get { return m_PositionDrillForeground; } set { m_PositionDrillForeground = value; } }
        public SolidColorBrush HeightForeground;
        public SolidColorBrush WeightForeground;
		public PlayerRecord PlayerRecord;
		public byte TeamIndex;
		public byte[] CurrentBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
		public byte[] FutureBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
		public byte[] PeakBars = new byte[(int)DataReader.FOFData.ScoutBars.Count];
        public ushort StartWeight;
        public ushort PeakWeight;
        public string Pos;
	}

	public class DescendingOverallSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			var lhs = (PlayerListData)x;
			var rhs = (PlayerListData)y;

			return rhs.OverallChange.CompareTo(lhs.OverallChange);
		}
	}

	public class DescendingRecentSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			var lhs = (PlayerListData)x;
			var rhs = (PlayerListData)y;

			return rhs.RecentChange.CompareTo(lhs.RecentChange);
		}
	}

	public class DescendingCombineScoreSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			var lhs = (PlayerListData)x;
			var rhs = (PlayerListData)y;

			return rhs.CombineScore.CompareTo(lhs.CombineScore);
		}
	}

	public class DescendingAttributeScoreSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			var lhs = (PlayerListData)x;
			var rhs = (PlayerListData)y;

			return rhs.AttributeScore.CompareTo(lhs.AttributeScore);
		}
	}

	public class DescendingOverallScoreSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			var lhs = (PlayerListData)x;
			var rhs = (PlayerListData)y;

			return rhs.OverallScore.CompareTo(lhs.OverallScore);
		}
	}

	public class AscendingPositionSorter : System.Collections.IComparer
	{
		public int Compare(object x, object y)
		{
			var lhs = (PlayerListData)x;
			var rhs = (PlayerListData)y;

			return lhs.PosOrder.CompareTo(rhs.PosOrder);
		}
	}

    public class AscendingFortySorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var lhs = (PlayerListData)x;
            var rhs = (PlayerListData)y;

            return lhs.CombineForty.CompareTo(rhs.CombineForty);
        }
    }

    public class AscendingAgilitySorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var lhs = (PlayerListData)x;
            var rhs = (PlayerListData)y;

            return lhs.CombineAgility.CompareTo(rhs.CombineAgility);
        }
    }

    public class DescendingSolecismicSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var lhs = (PlayerListData)x;
            var rhs = (PlayerListData)y;

            return rhs.CombineSolecismic.CompareTo(lhs.CombineSolecismic);
        }
    }

    public class DescendingBenchSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var lhs = (PlayerListData)x;
            var rhs = (PlayerListData)y;

            return rhs.CombineBench.CompareTo(lhs.CombineBench);
        }
    }

    public class DescendingBroadJumpSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var lhs = (PlayerListData)x;
            var rhs = (PlayerListData)y;

            return rhs.CombineBroadJump.CompareTo(lhs.CombineBroadJump);
        }
    }

    public class DescendingPositionDrillSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var lhs = (PlayerListData)x;
            var rhs = (PlayerListData)y;

            return rhs.CombinePositionDrill.CompareTo(lhs.CombinePositionDrill);
        }
    }

    public class DescendingPeakSorter : IComparer<PlayerListData>
	{
		public int Compare(PlayerListData lhs, PlayerListData rhs)
		{
			return rhs.PeakCur.CompareTo(lhs.PeakCur);
		}
	}

    public class TeamPositionSorter : IComparer<PlayerListData>
	{
		public int Compare(PlayerListData lhs, PlayerListData rhs)
		{
			if (lhs.TeamIndex == rhs.TeamIndex)
			{
				return lhs.PosOrder.CompareTo(rhs.PosOrder);
			}
			else
			{
				return lhs.TeamIndex.CompareTo(rhs.TeamIndex);
			}
		}
	}

}

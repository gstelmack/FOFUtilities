using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Peak
        {
            get
            {
                return PeakCur.ToString("D2") + "/" + PeakFut.ToString("D2");
            }
        }
        public string Current
        {
            get
            {
                return PresentCur.ToString("D2") + "/" + PresentFut.ToString("D2");
            }
        }

        public byte ExpYears;
        public string Exp
        {
            get { return ExpYears.ToString("D2"); }
        }

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
        public string Combine
        {
            get { return CombineScore.ToString("F2"); }
        }
        public string Bars
        {
            get { return AttributeScore.ToString("F2"); }
        }
        public string Score
        {
            get { return OverallScore.ToString("F2"); }
        }
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

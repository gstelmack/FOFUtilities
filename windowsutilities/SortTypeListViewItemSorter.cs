using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WindowsUtilities
{
	public enum SortType
	{
		SortByIntegerTag,
		SortByShortTag,
		SortByString,
		SortByInteger,
		SortByDouble,
		SortByRating,
		SortByColoredString
	}


	public class SortTypeListViewItemSorter : System.Collections.IComparer
	{
		private int mSortColumn;
		private bool mDescending;
		private bool mUseChecked;
		private SortType mSortType;

		public SortTypeListViewItemSorter(int col, SortType sortType, bool descending)
		{
			mSortColumn = col;
			mSortType = sortType;
			mDescending = descending;
		}

		public int SortColumn
		{
			get { return mSortColumn; }
			set { mSortColumn = value; }
		}

		public SortType SortMethod
		{
			get { return mSortType; }
			set	{ mSortType = value; }
		}

		public bool Descending
		{
			get { return mDescending; }
			set { mDescending = value; }
		}

		public bool UseChecked
		{
			get { return mUseChecked; }
			set { mUseChecked = value; }
		}

		public int Compare(object x, object y)
		{
			ListViewItem i1 = (ListViewItem)x;
			ListViewItem i2 = (ListViewItem)y;


			int sortVal = 0;
			if (mSortColumn < i1.SubItems.Count
				&& mSortColumn < i2.SubItems.Count
				)
			{
				if (mUseChecked && i1.Checked != i2.Checked)
				{
					if (i1.Checked)
					{
						sortVal = 1;
					}
					else
					{
						sortVal = 0;
					}
				}
				else
				{
					switch (mSortType)
					{
						case SortType.SortByString:
						case SortType.SortByColoredString:
							{
								sortVal = String.Compare(i1.SubItems[mSortColumn].Text, i2.SubItems[mSortColumn].Text);
							}
							break;

						case SortType.SortByShortTag:
							{
								int item1 = (short)(i1.SubItems[mSortColumn].Tag);
								int item2 = (short)(i2.SubItems[mSortColumn].Tag);
								sortVal = item1 - item2;
							}
							break;

						case SortType.SortByIntegerTag:
						case SortType.SortByRating:
							{
								int item1 = (int)(i1.SubItems[mSortColumn].Tag);
								int item2 = (int)(i2.SubItems[mSortColumn].Tag);
								sortVal = item1 - item2;
							}
							break;

						case SortType.SortByInteger:
							{
								int item1 = Int32.Parse(i1.SubItems[mSortColumn].Text);
								int item2 = Int32.Parse(i2.SubItems[mSortColumn].Text);
								sortVal = item1 - item2;
							}
							break;

						case SortType.SortByDouble:
							{
								double item1 = Double.Parse(i1.SubItems[mSortColumn].Text) * 100.0;
								double item2 = Double.Parse(i2.SubItems[mSortColumn].Text) * 100.0;
								sortVal = (int)(Math.Floor(item1 - item2));
							}
							break;
					}

					if (mDescending)
					{
						sortVal *= -1;
					}
				}
			}

			return sortVal;
		}

		public static void UpdateSortColumn(ListView lv, int newColumn, bool useChecked)
		{
			SortTypeListViewItemSorter comp = (SortTypeListViewItemSorter)lv.ListViewItemSorter;
			if (comp != null)
			{
				comp.UseChecked = useChecked;
				if (newColumn != -1)
				{
					if (comp.SortColumn == newColumn)
					{
						if (comp.Descending)
						{
							comp.Descending = false;
						}
						else
						{
							comp.Descending = true;
						}
					}
					else
					{
						comp.SortColumn = newColumn;
						SortType newSort = (SortType)(lv.Columns[newColumn].Tag);
						comp.SortMethod = newSort;
					}
				}
				lv.BeginUpdate();
				lv.Sort();
				lv.EndUpdate();
			}
		}

		public static void UpdateSortSpecific(ListView lv, int newColumn, bool useChecked, SortType newSort, bool descending)
		{
			SortTypeListViewItemSorter comp = (SortTypeListViewItemSorter)lv.ListViewItemSorter;
			if (comp != null)
			{
				comp.UseChecked = useChecked;
				comp.SortColumn = newColumn;
				comp.SortMethod = newSort;
				comp.Descending = descending;
				lv.BeginUpdate();
				lv.Sort();
				lv.EndUpdate();
			}
		}
	}
}

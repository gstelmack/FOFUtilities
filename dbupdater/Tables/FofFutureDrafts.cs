﻿/*

insert license info here

*/

using System;
using System.Collections;
using System.Collections.Generic;


namespace DBUpdater.Tables
{
	/// <summary>
	/// Generated by MyGeneration using the NHibernate Object Mapping 1.3.1 by Grimaldi Giuseppe (giuseppe.grimaldi@infracom.it)
	/// </summary>
	[Serializable]
	public class FofFutureDrafts
	{
		#region Private Members

		// Variabili di stato
		private bool misChanged;
		private bool misDeleted;

		// Primary Key(s) 
		private short mid;

		// Properties 
		private short myear;
		private short mround;
		private short mpick;
		private short mteamid;

		#endregion

		#region Default ( Empty ) Class Constructor

		/// <summary>
		/// default constructor
		/// </summary>
		public FofFutureDrafts()
		{
			mid = 0;
			myear = 0;
			mround = 0;
			mpick = 0;
			mteamid = 0;
		}

		#endregion // End of Default ( Empty ) Class Constructor

		#region Full Constructor

		/// <summary>
		/// full constructor
		/// </summary>
		public FofFutureDrafts(short id, short year, short round, short pick, short teamid)
		{
			mid = id;
			myear = year;
			mround = round;
			mpick = pick;
			mteamid = teamid;
		}

		#endregion // End Full Constructor

		#region Internal Accessors for NHibernate

		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Id
		{
			get { return mid; }
			set { mid = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Year
		{
			get { return myear; }
			set { myear = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Round
		{
			get { return mround; }
			set { mround = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Pick
		{
			get { return mpick; }
			set { mpick = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		internal virtual short _TeamID
		{
			get { return mteamid; }
			set { mteamid = value; }
		}

		#endregion // Internal Accessors for NHibernate

		#region Public Properties

		/// <summary>
		/// 
		/// </summary>		
		public virtual short Id
		{
			get { return mid; }
			set { misChanged |= (mid != value); mid = value; }
		}

		/// <summary>
		/// 
		/// </summary>		
		public virtual short Year
		{
			get { return myear; }
			set { misChanged |= (myear != value); myear = value; }
		}

		/// <summary>
		/// 
		/// </summary>		
		public virtual short Round
		{
			get { return mround; }
			set { misChanged |= (mround != value); mround = value; }
		}

		/// <summary>
		/// 
		/// </summary>		
		public virtual short Pick
		{
			get { return mpick; }
			set { misChanged |= (mpick != value); mpick = value; }
		}

		/// <summary>
		/// 
		/// </summary>		
		public virtual short TeamID
		{
			get { return mteamid; }
			set { misChanged |= (mteamid != value); mteamid = value; }
		}

		/// <summary>
		/// Returns whether or not the object has changed it's values.
		/// </summary>
		public virtual bool IsChanged
		{
			get { return misChanged; }
		}

		/// <summary>
		/// Returns whether or not the object has changed it's values.
		/// </summary>
		public virtual bool IsDeleted
		{
			get { return misDeleted; }
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// mark the item as deleted
		/// </summary>
		public virtual void MarkAsDeleted()
		{
			misDeleted = true;
			misChanged = true;
		}

		#endregion

		#region Equals And HashCode Overrides

		/// <summary>
		/// local implementation of Equals based on unique value members
		/// </summary>
		public override bool Equals(object obj)
		{
			if (this == obj) return true;
			if ((obj == null) || (obj.GetType() != this.GetType())) return false;
			FofFutureDrafts castObj = (FofFutureDrafts)obj;
			return (castObj != null) &&
				(this.mid == castObj.Id);
		}

		/// <summary>
		/// local implementation of GetHashCode based on unique value members
		/// </summary>
		public override int GetHashCode()
		{
			int hash = 57;
			hash = 27 * hash * this.mid.GetHashCode();

			return hash;

		}

		#endregion

	}
}
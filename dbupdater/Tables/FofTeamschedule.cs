/*

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
	public class FofTeamschedule 
	{
		#region Private Members
		
		// Variabili di stato
		private bool misChanged;
		private bool misDeleted;

		// Primary Key(s) 
		private int mid; 
		
		// Properties 
		private byte mteamid; 
		private byte mweek; 
		private byte maway; 
		private short mconferencegame; 
		private short mdivisiongame; 
		private byte mopponentid; 
		private byte mscore; 
		private byte moppscore; 
		private int mattendance; 
		private short mtemperature; 
		private byte mprecip; 
		private byte mwind; 
		private short myear; 		

		#endregion
		
		#region Default ( Empty ) Class Constructor
		
		/// <summary>
		/// default constructor
		/// </summary>
		public FofTeamschedule()
		{
			mid = 0; 
			mteamid = 0; 
			mweek = 0; 
			maway = 0; 
			mconferencegame = 0; 
			mdivisiongame = 0; 
			mopponentid = 0; 
			mscore = 0; 
			moppscore = 0; 
			mattendance = 0; 
			mtemperature = 0; 
			mprecip = 0; 
			mwind = 0; 
			myear = 0; 
		}
		
		#endregion // End of Default ( Empty ) Class Constructor
		
		#region Full Constructor
		
		/// <summary>
		/// full constructor
		/// </summary>
		public FofTeamschedule(int id, byte teamid, byte week, byte away, short conferencegame, short divisiongame, byte opponentid, byte score, byte oppscore, int attendance, short temperature, byte precip, byte wind, short year)
		{
			mid = id; 
			mteamid = teamid; 
			mweek = week; 
			maway = away; 
			mconferencegame = conferencegame; 
			mdivisiongame = divisiongame; 
			mopponentid = opponentid; 
			mscore = score; 
			moppscore = oppscore; 
			mattendance = attendance; 
			mtemperature = temperature; 
			mprecip = precip; 
			mwind = wind; 
			myear = year; 
		}
		
		#endregion // End Full Constructor
		
		#region Internal Accessors for NHibernate
		
		/// <summary>
		/// 
		/// </summary>
		internal virtual int _Id
		{
			get { return mid; }
			set { mid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Teamid
		{
			get { return mteamid; }
			set { mteamid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Week
		{
			get { return mweek; }
			set { mweek = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Away
		{
			get { return maway; }
			set { maway = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _ConferenceGame
		{
			get { return mconferencegame; }
			set { mconferencegame = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _DivisionGame
		{
			get { return mdivisiongame; }
			set { mdivisiongame = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Opponentid
		{
			get { return mopponentid; }
			set { mopponentid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Score
		{
			get { return mscore; }
			set { mscore = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _OppScore
		{
			get { return moppscore; }
			set { moppscore = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual int _Attendance
		{
			get { return mattendance; }
			set { mattendance = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Temperature
		{
			get { return mtemperature; }
			set { mtemperature = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Precip
		{
			get { return mprecip; }
			set { mprecip = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Wind
		{
			get { return mwind; }
			set { mwind = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Year
		{
			get { return myear; }
			set { myear = value; }
		} 
	  
		#endregion // Internal Accessors for NHibernate 

		#region Public Properties
			
		/// <summary>
		/// 
		/// </summary>		
		public virtual int Id
		{
			get { return mid; }
			set { misChanged |= (mid != value); mid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Teamid
		{
			get { return mteamid; }
			set { misChanged |= (mteamid != value); mteamid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Week
		{
			get { return mweek; }
			set { misChanged |= (mweek != value); mweek = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Away
		{
			get { return maway; }
			set { misChanged |= (maway != value); maway = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short ConferenceGame
		{
			get { return mconferencegame; }
			set { misChanged |= (mconferencegame != value); mconferencegame = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short DivisionGame
		{
			get { return mdivisiongame; }
			set { misChanged |= (mdivisiongame != value); mdivisiongame = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Opponentid
		{
			get { return mopponentid; }
			set { misChanged |= (mopponentid != value); mopponentid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Score
		{
			get { return mscore; }
			set { misChanged |= (mscore != value); mscore = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte OppScore
		{
			get { return moppscore; }
			set { misChanged |= (moppscore != value); moppscore = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual int Attendance
		{
			get { return mattendance; }
			set { misChanged |= (mattendance != value); mattendance = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short Temperature
		{
			get { return mtemperature; }
			set { misChanged |= (mtemperature != value); mtemperature = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Precip
		{
			get { return mprecip; }
			set { misChanged |= (mprecip != value); mprecip = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Wind
		{
			get { return mwind; }
			set { misChanged |= (mwind != value); mwind = value; }
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
		public override bool Equals( object obj )
		{
			if( this == obj ) return true;
			if( ( obj == null ) || ( obj.GetType() != this.GetType() ) ) return false;
			FofTeamschedule castObj = (FofTeamschedule)obj; 
			return ( castObj != null ) &&
				( this.mid == castObj.Id );
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
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
	public class FofTeams 
	{
		#region Private Members
		
		// Variabili di stato
		private bool misChanged;
		private bool misDeleted;

		// Primary Key(s) 
		private short mid; 
		
		// Properties 
		private string mnickname; 
		private string mcityname; 
		private string mabbrev; 
		private short mcityid; 
		private byte mconference; 
		private byte mdivision; 
		private long mcaplossthisyear; 
		private long mcaplossnextyear; 		

		#endregion
		
		#region Default ( Empty ) Class Constructor
		
		/// <summary>
		/// default constructor
		/// </summary>
		public FofTeams()
		{
			mid = 0; 
			mnickname = null; 
			mcityname = null; 
			mabbrev = null; 
			mcityid = 0; 
			mconference = 0; 
			mdivision = 0; 
			mcaplossthisyear = 0; 
			mcaplossnextyear = 0; 
		}
		
		#endregion // End of Default ( Empty ) Class Constructor
		
		#region Full Constructor
		
		/// <summary>
		/// full constructor
		/// </summary>
		public FofTeams(short id, string nickname, string cityname, string abbrev, short cityid, byte conference, byte division, long caplossthisyear, long caplossnextyear)
		{
			mid = id; 
			mnickname = nickname; 
			mcityname = cityname; 
			mabbrev = abbrev; 
			mcityid = cityid; 
			mconference = conference; 
			mdivision = division; 
			mcaplossthisyear = caplossthisyear; 
			mcaplossnextyear = caplossnextyear; 
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
		internal virtual string _Nickname
		{
			get { return mnickname; }
			set { mnickname = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual string _CityName
		{
			get { return mcityname; }
			set { mcityname = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual string _Abbrev
		{
			get { return mabbrev; }
			set { mabbrev = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Cityid
		{
			get { return mcityid; }
			set { mcityid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Conference
		{
			get { return mconference; }
			set { mconference = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Division
		{
			get { return mdivision; }
			set { mdivision = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual long _CapLossThisYear
		{
			get { return mcaplossthisyear; }
			set { mcaplossthisyear = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual long _CapLossNextYear
		{
			get { return mcaplossnextyear; }
			set { mcaplossnextyear = value; }
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
		public virtual string Nickname
		{
			get { return mnickname; }
			set	
			{
				if ( value != null )
					if( value.Length > 20)
						throw new ArgumentOutOfRangeException("Invalid value for Nickname", value, value.ToString());
				
				misChanged |= (mnickname != value); mnickname = value;
			}
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual string CityName
		{
			get { return mcityname; }
			set	
			{
				if ( value != null )
					if( value.Length > 45)
						throw new ArgumentOutOfRangeException("Invalid value for CityName", value, value.ToString());
				
				misChanged |= (mcityname != value); mcityname = value;
			}
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual string Abbrev
		{
			get { return mabbrev; }
			set	
			{
				if ( value != null )
					if( value.Length > 3)
						throw new ArgumentOutOfRangeException("Invalid value for Abbrev", value, value.ToString());
				
				misChanged |= (mabbrev != value); mabbrev = value;
			}
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short Cityid
		{
			get { return mcityid; }
			set { misChanged |= (mcityid != value); mcityid = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Conference
		{
			get { return mconference; }
			set { misChanged |= (mconference != value); mconference = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Division
		{
			get { return mdivision; }
			set { misChanged |= (mdivision != value); mdivision = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual long CapLossThisYear
		{
			get { return mcaplossthisyear; }
			set { misChanged |= (mcaplossthisyear != value); mcaplossthisyear = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual long CapLossNextYear
		{
			get { return mcaplossnextyear; }
			set { misChanged |= (mcaplossnextyear != value); mcaplossnextyear = value; }
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
			FofTeams castObj = (FofTeams)obj; 
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
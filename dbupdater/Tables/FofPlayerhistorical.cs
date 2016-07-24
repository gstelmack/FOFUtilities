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
	public class FofPlayerhistorical 
	{
		#region Private Members
		
		// Variabili di stato
		private bool misChanged;
		private bool misDeleted;

		// Primary Key(s) 
		private int mid; 
		
		// Properties 
		private byte mposition; 
		private string mlastname; 
		private string mfirstname; 
		private string mnickname; 
		private byte mexperience; 
		private byte mheight; 
		private short mweight; 
		private byte minhalloffame; 
		private short mhalloffameyear; 
		private byte mhalloffamevote; 
		private short mbirthyear; 
		private byte mbirthmonth; 
		private byte mbirthday; 
		private short mcollege; 
		private byte mdraftround; 
		private byte mdraftpick; 
		private short mhometown; 
		private byte mdraftedby; 
		private short myeardraft; 
		private short myearretired; 
		private short mpog; 
		private short mrings; 
		private short mpowmentions; 
		private short mpowwins; 
		private short mfourqhero; 
		private short mqbwins; 
		private short mqblosses; 
		private short mqbties; 		

		#endregion
		
		#region Default ( Empty ) Class Constructor
		
		/// <summary>
		/// default constructor
		/// </summary>
		public FofPlayerhistorical()
		{
			mid = 0; 
			mposition = 0; 
			mlastname = null; 
			mfirstname = null; 
			mnickname = null; 
			mexperience = 0; 
			mheight = 0; 
			mweight = 0; 
			minhalloffame = 0; 
			mhalloffameyear = 0; 
			mhalloffamevote = 0; 
			mbirthyear = 0; 
			mbirthmonth = 0; 
			mbirthday = 0; 
			mcollege = 0; 
			mdraftround = 0; 
			mdraftpick = 0; 
			mhometown = 0; 
			mdraftedby = 0; 
			myeardraft = 0; 
			myearretired = 0; 
			mpog = 0; 
			mrings = 0; 
			mpowmentions = 0; 
			mpowwins = 0; 
			mfourqhero = 0; 
			mqbwins = 0; 
			mqblosses = 0; 
			mqbties = 0; 
		}
		
		#endregion // End of Default ( Empty ) Class Constructor
		
		#region Full Constructor
		
		/// <summary>
		/// full constructor
		/// </summary>
		public FofPlayerhistorical(int id, byte position, string lastname, string firstname, string nickname, byte experience, byte height, short weight, byte inhalloffame, short halloffameyear, byte halloffamevote, short birthyear, byte birthmonth, byte birthday, short college, byte draftround, byte draftpick, short hometown, byte draftedby, short yeardraft, short yearretired, short pog, short rings, short powmentions, short powwins, short fourqhero, short qbwins, short qblosses, short qbties)
		{
			mid = id; 
			mposition = position; 
			mlastname = lastname; 
			mfirstname = firstname; 
			mnickname = nickname; 
			mexperience = experience; 
			mheight = height; 
			mweight = weight; 
			minhalloffame = inhalloffame; 
			mhalloffameyear = halloffameyear; 
			mhalloffamevote = halloffamevote; 
			mbirthyear = birthyear; 
			mbirthmonth = birthmonth; 
			mbirthday = birthday; 
			mcollege = college; 
			mdraftround = draftround; 
			mdraftpick = draftpick; 
			mhometown = hometown; 
			mdraftedby = draftedby; 
			myeardraft = yeardraft; 
			myearretired = yearretired; 
			mpog = pog; 
			mrings = rings; 
			mpowmentions = powmentions; 
			mpowwins = powwins; 
			mfourqhero = fourqhero; 
			mqbwins = qbwins; 
			mqblosses = qblosses; 
			mqbties = qbties; 
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
		internal virtual byte _Position
		{
			get { return mposition; }
			set { mposition = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual string _LastName
		{
			get { return mlastname; }
			set { mlastname = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual string _FirstName
		{
			get { return mfirstname; }
			set { mfirstname = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual string _NickName
		{
			get { return mnickname; }
			set { mnickname = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Experience
		{
			get { return mexperience; }
			set { mexperience = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _Height
		{
			get { return mheight; }
			set { mheight = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Weight
		{
			get { return mweight; }
			set { mweight = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _InHallOfFame
		{
			get { return minhalloffame; }
			set { minhalloffame = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _HallOfFameYear
		{
			get { return mhalloffameyear; }
			set { mhalloffameyear = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _HallOfFameVote
		{
			get { return mhalloffamevote; }
			set { mhalloffamevote = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _BirthYear
		{
			get { return mbirthyear; }
			set { mbirthyear = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _BirthMonth
		{
			get { return mbirthmonth; }
			set { mbirthmonth = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _BirthDay
		{
			get { return mbirthday; }
			set { mbirthday = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _College
		{
			get { return mcollege; }
			set { mcollege = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _DraftRound
		{
			get { return mdraftround; }
			set { mdraftround = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _DraftPick
		{
			get { return mdraftpick; }
			set { mdraftpick = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _HomeTown
		{
			get { return mhometown; }
			set { mhometown = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual byte _DraftedBy
		{
			get { return mdraftedby; }
			set { mdraftedby = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _YearDraft
		{
			get { return myeardraft; }
			set { myeardraft = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _YearRetired
		{
			get { return myearretired; }
			set { myearretired = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Pog
		{
			get { return mpog; }
			set { mpog = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _Rings
		{
			get { return mrings; }
			set { mrings = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _PowMentions
		{
			get { return mpowmentions; }
			set { mpowmentions = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _PowWins
		{
			get { return mpowwins; }
			set { mpowwins = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _FourqHero
		{
			get { return mfourqhero; }
			set { mfourqhero = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _QbWins
		{
			get { return mqbwins; }
			set { mqbwins = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _QbLosses
		{
			get { return mqblosses; }
			set { mqblosses = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>
		internal virtual short _QbTies
		{
			get { return mqbties; }
			set { mqbties = value; }
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
		public virtual byte Position
		{
			get { return mposition; }
			set { misChanged |= (mposition != value); mposition = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual string LastName
		{
			get { return mlastname; }
			set	
			{
				if ( value != null )
					if( value.Length > 128)
						throw new ArgumentOutOfRangeException("Invalid value for LastName", value, value.ToString());
				
				misChanged |= (mlastname != value); mlastname = value;
			}
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual string FirstName
		{
			get { return mfirstname; }
			set	
			{
				if ( value != null )
					if( value.Length > 128)
						throw new ArgumentOutOfRangeException("Invalid value for FirstName", value, value.ToString());
				
				misChanged |= (mfirstname != value); mfirstname = value;
			}
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual string NickName
		{
			get { return mnickname; }
			set	
			{
				if ( value != null )
					if( value.Length > 128)
						throw new ArgumentOutOfRangeException("Invalid value for NickName", value, value.ToString());
				
				misChanged |= (mnickname != value); mnickname = value;
			}
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Experience
		{
			get { return mexperience; }
			set { misChanged |= (mexperience != value); mexperience = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte Height
		{
			get { return mheight; }
			set { misChanged |= (mheight != value); mheight = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short Weight
		{
			get { return mweight; }
			set { misChanged |= (mweight != value); mweight = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte InHallOfFame
		{
			get { return minhalloffame; }
			set { misChanged |= (minhalloffame != value); minhalloffame = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short HallOfFameYear
		{
			get { return mhalloffameyear; }
			set { misChanged |= (mhalloffameyear != value); mhalloffameyear = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte HallOfFameVote
		{
			get { return mhalloffamevote; }
			set { misChanged |= (mhalloffamevote != value); mhalloffamevote = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short BirthYear
		{
			get { return mbirthyear; }
			set { misChanged |= (mbirthyear != value); mbirthyear = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte BirthMonth
		{
			get { return mbirthmonth; }
			set { misChanged |= (mbirthmonth != value); mbirthmonth = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte BirthDay
		{
			get { return mbirthday; }
			set { misChanged |= (mbirthday != value); mbirthday = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short College
		{
			get { return mcollege; }
			set { misChanged |= (mcollege != value); mcollege = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte DraftRound
		{
			get { return mdraftround; }
			set { misChanged |= (mdraftround != value); mdraftround = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte DraftPick
		{
			get { return mdraftpick; }
			set { misChanged |= (mdraftpick != value); mdraftpick = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short HomeTown
		{
			get { return mhometown; }
			set { misChanged |= (mhometown != value); mhometown = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual byte DraftedBy
		{
			get { return mdraftedby; }
			set { misChanged |= (mdraftedby != value); mdraftedby = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short YearDraft
		{
			get { return myeardraft; }
			set { misChanged |= (myeardraft != value); myeardraft = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short YearRetired
		{
			get { return myearretired; }
			set { misChanged |= (myearretired != value); myearretired = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short Pog
		{
			get { return mpog; }
			set { misChanged |= (mpog != value); mpog = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short Rings
		{
			get { return mrings; }
			set { misChanged |= (mrings != value); mrings = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short PowMentions
		{
			get { return mpowmentions; }
			set { misChanged |= (mpowmentions != value); mpowmentions = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short PowWins
		{
			get { return mpowwins; }
			set { misChanged |= (mpowwins != value); mpowwins = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short FourqHero
		{
			get { return mfourqhero; }
			set { misChanged |= (mfourqhero != value); mfourqhero = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short QbWins
		{
			get { return mqbwins; }
			set { misChanged |= (mqbwins != value); mqbwins = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short QbLosses
		{
			get { return mqblosses; }
			set { misChanged |= (mqblosses != value); mqblosses = value; }
		} 
	  
		/// <summary>
		/// 
		/// </summary>		
		public virtual short QbTies
		{
			get { return mqbties; }
			set { misChanged |= (mqbties != value); mqbties = value; }
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
			FofPlayerhistorical castObj = (FofPlayerhistorical)obj; 
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
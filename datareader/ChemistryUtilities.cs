using System;
using System.Collections.Generic;
using System.Text;

namespace DataReader
{
	public class ChemistryUtilities
	{
		public struct Birthday
		{
			public int Month;
			public int Day;

			public static Birthday Parse(string birthdayString)
			{
				Birthday result = new Birthday();
				result.Month = 1;
				result.Day = 1;
				string[] tokens = birthdayString.Split(new char[] { '-','/' }, StringSplitOptions.RemoveEmptyEntries);
				if (tokens.Length >= 2)
				{
					Int32.TryParse(tokens[0], out result.Month);
					Int32.TryParse(tokens[1], out result.Day);
				}

				return result;
			}
		}

		public enum Chemistry
		{
			Conflict,
			Neutral,
			Affinity
		}

		private enum AstrologicalSign
		{
			Aries,
			Taurus,
			Gemini,
			Cancer,
			Leo,
			Virgo,
			Libra,
			Scorpio,
			Sagittarius,
			Capricorn,
			Aquarius,
			Pisces,
			Error
		}

		private static AstrologicalSign GetSign(Birthday testDay)
		{
			switch (testDay.Month)
			{
				case 1:
					if (testDay.Day <= 20)
					{
						return AstrologicalSign.Capricorn;
					}
					else
					{
						return AstrologicalSign.Aquarius;
					}
				case 2:
					if (testDay.Day <= 18)
					{
						return AstrologicalSign.Aquarius;
					}
					else
					{
						return AstrologicalSign.Pisces;
					}
				case 3:
					if (testDay.Day <= 20)
					{
						return AstrologicalSign.Pisces;
					}
					else
					{
						return AstrologicalSign.Aries;
					}
				case 4:
					if (testDay.Day <= 20)
					{
						return AstrologicalSign.Aries;
					}
					else
					{
						return AstrologicalSign.Taurus;
					}
				case 5:
					if (testDay.Day <= 21)
					{
						return AstrologicalSign.Taurus;
					}
					else
					{
						return AstrologicalSign.Gemini;
					}
				case 6:
					if (testDay.Day <= 21)
					{
						return AstrologicalSign.Gemini;
					}
					else
					{
						return AstrologicalSign.Cancer;
					}
				case 7:
					if (testDay.Day <= 23)
					{
						return AstrologicalSign.Cancer;
					}
					else
					{
						return AstrologicalSign.Leo;
					}
				case 8:
					if (testDay.Day <= 23)
					{
						return AstrologicalSign.Leo;
					}
					else
					{
						return AstrologicalSign.Virgo;
					}
				case 9:
					if (testDay.Day <= 23)
					{
						return AstrologicalSign.Virgo;
					}
					else
					{
						return AstrologicalSign.Libra;
					}
				case 10:
					if (testDay.Day <= 23)
					{
						return AstrologicalSign.Libra;
					}
					else
					{
						return AstrologicalSign.Scorpio;
					}
				case 11:
					if (testDay.Day <= 22)
					{
						return AstrologicalSign.Scorpio;
					}
					else
					{
						return AstrologicalSign.Sagittarius;
					}
				case 12:
					if (testDay.Day <= 22)
					{
						return AstrologicalSign.Sagittarius;
					}
					else
					{
						return AstrologicalSign.Capricorn;
					}
				default:
					return AstrologicalSign.Error;
			}
		}

		public static Chemistry PredictChemistry(Birthday leaderBirthday, Birthday playerBirthday)
		{
			AstrologicalSign leaderSign = GetSign(leaderBirthday);
			AstrologicalSign playerSign = GetSign(playerBirthday);

			switch (leaderSign)
			{
				case AstrologicalSign.Aquarius:
					if (playerSign == AstrologicalSign.Libra || playerSign == AstrologicalSign.Capricorn)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Aries)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Aries:
					if (playerSign == AstrologicalSign.Scorpio || playerSign == AstrologicalSign.Gemini)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Aquarius)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Cancer:
					if (playerSign == AstrologicalSign.Pisces || playerSign == AstrologicalSign.Taurus)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Sagittarius)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Capricorn:
					if (playerSign == AstrologicalSign.Libra || playerSign == AstrologicalSign.Aquarius)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Leo)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Error:
					return Chemistry.Neutral;
				case AstrologicalSign.Gemini:
					if (playerSign == AstrologicalSign.Scorpio || playerSign == AstrologicalSign.Aries)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Virgo)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Leo:
					if (playerSign == AstrologicalSign.Virgo || playerSign == AstrologicalSign.Sagittarius)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Capricorn)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Libra:
					if (playerSign == AstrologicalSign.Aquarius || playerSign == AstrologicalSign.Capricorn)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Taurus)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Pisces:
					if (playerSign == AstrologicalSign.Taurus || playerSign == AstrologicalSign.Cancer)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Scorpio)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Sagittarius:
					if (playerSign == AstrologicalSign.Leo || playerSign == AstrologicalSign.Virgo)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Cancer)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Scorpio:
					if (playerSign == AstrologicalSign.Aries || playerSign == AstrologicalSign.Gemini)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Pisces)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Taurus:
					if (playerSign == AstrologicalSign.Pisces || playerSign == AstrologicalSign.Cancer)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Libra)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
				case AstrologicalSign.Virgo:
					if (playerSign == AstrologicalSign.Leo || playerSign == AstrologicalSign.Sagittarius)
					{
						return Chemistry.Affinity;
					}
					else if (playerSign == AstrologicalSign.Gemini)
					{
						return Chemistry.Conflict;
					}
					else
					{
						return Chemistry.Neutral;
					}
			}

			return Chemistry.Neutral;
		}

		public static Chemistry PredictChemistry(Birthday leaderBirthday, string playerBirthday)
		{
			Birthday playerConverted = Birthday.Parse(playerBirthday);
			return PredictChemistry(leaderBirthday, playerConverted);
		}

		public static Chemistry PredictChemistry(string leaderBirthday, string playerBirthday)
		{
			Birthday leaderConverted = Birthday.Parse(leaderBirthday);
			Birthday playerConverted = Birthday.Parse(playerBirthday);
			return PredictChemistry(leaderConverted, playerConverted);
		}
	}
}

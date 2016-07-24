using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsUtilities
{
	public class OutputLocation
	{
		public static string Get()
		{
			const string kCompanyDirectory = "StelmackSoft";
			const string kAppDirectory = "UtilitySuite";

			string saveFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string companyFolder = System.IO.Path.Combine(saveFolder, kCompanyDirectory);

			if (!System.IO.Directory.Exists(companyFolder))
			{
				System.IO.Directory.CreateDirectory(companyFolder);
			}
			string appFolderName = System.IO.Path.Combine(companyFolder, kAppDirectory);
			if (!System.IO.Directory.Exists(appFolderName))
			{
				System.IO.Directory.CreateDirectory(appFolderName);
			}
			return appFolderName;
		}
	}
}

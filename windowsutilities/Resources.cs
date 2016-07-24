using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WindowsUtilities
{
	public class Resources
	{
		public static IntPtr RT_BITMAP_PTR = (IntPtr)2;
		public static int RT_BITMAP_INT = 2;

		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("user32.dll")]
		static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

		[DllImport("kernel32.dll")]
		public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);
		[DllImport("kernel32.dll")]
		public static extern IntPtr FindResource(IntPtr hModule, int lpName, int lpType);
		[DllImport("kernel32.dll")]
		public static extern IntPtr FindResource(IntPtr hModule, int lpName, string lpType);
		[DllImport("kernel32.dll")]
		public static extern IntPtr FindResource(IntPtr hModule, string lpName, int lpType);
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LockResource(IntPtr hResData);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr BeginUpdateResource(string pFileName,
		   [MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);
		
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool UpdateResource(IntPtr hUpdate, string lpType, string lpName, ushort wLanguage, IntPtr lpData, uint cbData);
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool UpdateResource(IntPtr hUpdate, int lpType, int lpName, ushort wLanguage, IntPtr lpData, uint cbData);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

		public static string LoadResourceString(IntPtr hInstance, UInt32 id)
		{
			StringBuilder sb = new StringBuilder(255);
			LoadString(hInstance, id, sb, sb.Capacity + 1);
			return sb.ToString();
		}
	}
}

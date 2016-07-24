using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Automater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//Import the FindWindow API to find our window
		[DllImportAttribute("User32.dll")]
		private static extern int FindWindow(String ClassName, String WindowName);

		//Import the SetForeground API to activate it
		[DllImportAttribute("User32.dll")]
		private static extern IntPtr SetForegroundWindow(int hWnd);

		public MainWindow()
		{
			try
			{
				this.Title += " v" + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
			}
			catch
			{
				this.Title += " DEBUG";
			}

			InitializeComponent();

			textBoxSeasonCount.Text = "1";
		}

		private void textBoxSeasonCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			uint seasonCount;
			e.Handled = !UInt32.TryParse(e.Text, out seasonCount);
		}

		private int m_PlayerTrackerWindow;
		private int m_FOFWindow;
		private void buttonRunSeasons_Click(object sender, RoutedEventArgs e)
		{
			m_PlayerTrackerWindow = FindWindow(null, "PlayerTracker");
			if (m_PlayerTrackerWindow == 0)
			{
				MessageBox.Show("Can't find PlayerTracker window!");
				return;
			}
			m_FOFWindow = FindWindow(null, "Front Office Football Seven");
			if (m_FOFWindow == 0)
			{
				MessageBox.Show("Can't find FOF window!");
			}

			uint seasonCount = UInt32.Parse(textBoxSeasonCount.Text);
			for (uint seasonIndex = 0; seasonIndex < seasonCount;++seasonIndex )
			{
				RunSeason();
			}
		}

		private void RunSeason()
		{
			// Retain staff button
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Finished with renegotiations
			DoMouseClick(1136, 618);
			System.Threading.Thread.Sleep(250);

			// Yes, just in case we have a reneg
			DoMouseClick(907, 589);
			System.Threading.Thread.Sleep(250);

			// Staff Draft
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Select "Fast" in speed drop down
			DoMouseClick(1125, 823);
			System.Threading.Thread.Sleep(250);
			DoMouseClick(1059, 855);
			System.Threading.Thread.Sleep(250);

			// Have Staff finish draft - takes 10 seconds, give some slack
			DoMouseClick(1093, 678);
			System.Threading.Thread.Sleep(15 * 1000);

			// Exit
			DoMouseClick(807, 817);
			System.Threading.Thread.Sleep(250);

			// Begin Free Agency
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Yes - takes a bit
			DoMouseClick(907, 587);
			System.Threading.Thread.Sleep(10 * 1000);

			// Dump Scouting data so we have draft data
			ExportScoutingData();

			// Continue Free Agency
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Skip remaining stages
			DoMouseClick(1190, 752);
			System.Threading.Thread.Sleep(250);

			// Yes - takes a bit
			DoMouseClick(907, 587);
			System.Threading.Thread.Sleep(30 * 1000);

			// Exit
			DoMouseClick(1026, 787);
			System.Threading.Thread.Sleep(250);

			// Begin Amateur Draft
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Draft Speed
			DoMouseClick(1362, 866);
			System.Threading.Thread.Sleep(250);

			// Fastest
			DoMouseClick(1314, 950);
			System.Threading.Thread.Sleep(250);

			// Have Staff Finish Draft - takes a bit
			DoMouseClick(1309, 664);
			System.Threading.Thread.Sleep(45 * 1000);

			// Exit
			DoMouseClick(941, 862);
			System.Threading.Thread.Sleep(250);

			// Begin Late Free Agency
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(5 * 1000);

			// Continue Free Agency
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Skip remaining stages
			DoMouseClick(1190, 752);
			System.Threading.Thread.Sleep(250);

			// Yes - takes a bit
			DoMouseClick(907, 587);
			System.Threading.Thread.Sleep(15 * 1000);

			// Exit
			DoMouseClick(1026, 787);
			System.Threading.Thread.Sleep(250);

			// Dump Beginning of season data and import into PlayerTracker
			ExportScoutingData();
			ImportScoutingData();

			// Begin Training Camp
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Run Training Camp - takes a bit
			DoMouseClick(936, 736);
			System.Threading.Thread.Sleep(10 * 1000);

			// Simulate Games
			DoMouseClick(342, 445);
			System.Threading.Thread.Sleep(250);

			// Simulate Entire Season
			DoMouseClick(1019, 696);
			System.Threading.Thread.Sleep(60 * 1000);

			// Exit
			DoMouseClick(940, 726);
			System.Threading.Thread.Sleep(250);

			// Dump End of season data and import into PlayerTracker
			ExportScoutingData();
			ImportScoutingData();

			// End Season
			DoMouseClick(150, 215);
			System.Threading.Thread.Sleep(250);

			// Yes - takes a bit
			DoMouseClick(907, 589);
			System.Threading.Thread.Sleep(10 * 1000);

			// Continue
			DoMouseClick(938, 731);
			System.Threading.Thread.Sleep(5 * 1000);
		}

		private void ExportScoutingData()
		{
			// Export Data - takes a bit
			DoMouseClick(1073, 662);
			System.Threading.Thread.Sleep(10 * 1000);

			// OK
			DoMouseClick(962, 588);
			System.Threading.Thread.Sleep(250);

			// Export Personal Scouting Data
			DoMouseClick(1073, 678);
			System.Threading.Thread.Sleep(250);

			// Yes - takes a bit
			DoMouseClick(909, 590);
			System.Threading.Thread.Sleep(5 * 1000);

			// OK
			DoMouseClick(962, 588);
			System.Threading.Thread.Sleep(250);
		}

		private void ImportScoutingData()
		{
			// Switch to PlayerTracker
			SetForegroundWindow(m_PlayerTrackerWindow);
			System.Threading.Thread.Sleep(500);

			// Import - takes a bit
			DoMouseClick(263, 47);
			System.Threading.Thread.Sleep(5 * 1000);

			// Switch to FOF
			SetForegroundWindow(m_FOFWindow);
			System.Threading.Thread.Sleep(1000);
		}

		private void DoMouseClick(int clickX, int clickY)
		{
			double sizeX = System.Windows.SystemParameters.PrimaryScreenWidth;
			double sizeY = System.Windows.SystemParameters.PrimaryScreenHeight;

			int inputX = (int)((clickX * 65535) / sizeX);
			int inputY = (int)((clickY * 65535) / sizeY);

			Win32Input.MouseLeftButtonDownAndUp(inputX, inputY);
		}
	}
}

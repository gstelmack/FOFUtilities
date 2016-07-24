using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DataReader;

namespace CheckPaths
{
	public partial class CheckPathsForm : Form
	{
		private UniverseData mUniverseData;

		public CheckPathsForm()
		{
			InitializeComponent();

			Assembly a = typeof(CheckPathsForm).Assembly;
			Text += " v" + a.GetName().Version;

			mUniverseData = new UniverseData();

			buttonFOFInstallPath.Text = mUniverseData.FOFDirectory;
			buttonFOFSavePath.Text = mUniverseData.SaveDirectory;
			buttonUtilitSuiteSavePath.Text = WindowsUtilities.OutputLocation.Get();
		}

		private void buttonFOFInstallPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			if (buttonFOFInstallPath.Text.Length > 0)
			{
				dlg.SelectedPath = buttonFOFInstallPath.Text;
			}
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonFOFInstallPath.Text = dlg.SelectedPath;
				SavePaths();
			}
		}

		private void buttonFOFSavePath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			if (buttonFOFSavePath.Text.Length > 0)
			{
				dlg.SelectedPath = buttonFOFSavePath.Text;
			}
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				buttonFOFSavePath.Text = dlg.SelectedPath;
				SavePaths();
			}
		}

		private void SavePaths()
		{
			string directoryINIPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "FOFPath.txt");
			using (System.IO.StreamWriter pathFile = new System.IO.StreamWriter(directoryINIPath))
			{
				pathFile.WriteLine(buttonFOFInstallPath.Text);
				pathFile.WriteLine(buttonFOFSavePath.Text);
				pathFile.Close();
			}
		}

		private void buttonUtilitSuiteSavePath_Click(object sender, EventArgs e)
		{
			if (buttonUtilitSuiteSavePath.Text != null)
			{
				System.Diagnostics.Process.Start(buttonUtilitSuiteSavePath.Text);
			}
		}

		private void buttonFOFInstallOpen_Click(object sender, EventArgs e)
		{
			if (buttonFOFInstallPath.Text != null)
			{
				System.Diagnostics.Process.Start(buttonFOFInstallPath.Text);
			}
		}

		private void buttonFOFSaveOpen_Click(object sender, EventArgs e)
		{
			if (buttonFOFSavePath.Text != null)
			{
				System.Diagnostics.Process.Start(buttonFOFSavePath.Text);
			}
		}
	}
}

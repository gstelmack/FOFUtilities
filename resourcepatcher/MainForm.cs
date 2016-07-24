using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ResourcePatcher
{
	public partial class MainForm : Form
	{
		private class Patch
		{
			public Patch()
			{
				fofResource = 0;
				packageResource = 0;
			}

			public int fofResource;
			public int packageResource;
		}

		private class PatcherEntry
		{
			public PatcherEntry()
			{
				patches = new List<Patch>();
				description = "";
			}

			public string description;
			public List<Patch> patches;
		}

		private class PatcherPackage
		{
			public PatcherPackage()
			{
				entries = new List<PatcherEntry>();
				name = "";
				dllPath = "";
			}

			public string name;
			public string dllPath;
			public List<PatcherEntry> entries;
		}

		private List<PatcherPackage> packages;

		public MainForm()
		{
			InitializeComponent();

			Assembly a = typeof(MainForm).Assembly;
			Text += " v" + a.GetName().Version;

			string fofDirectory = null;
			string directoryINIPath = Path.Combine(WindowsUtilities.OutputLocation.Get(),"FOFPath.txt");
			if (File.Exists(directoryINIPath))
			{
				using (StreamReader inFile = new StreamReader(directoryINIPath))
				{
					fofDirectory = inFile.ReadLine();
					inFile.Close();
				}
			}
			if (fofDirectory == null || !Directory.Exists(fofDirectory))
			{
				fofDirectory = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Solecismic Software\\Front Office Football 2007",
					"Full Path", null);
			}
            if (fofDirectory == null)
            {
                fofDirectory = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Wow6432Node\\Solecismic Software\\Front Office Football 2007",
                    "Full Path", null);
            }
            if (fofDirectory != null && Directory.Exists(fofDirectory))
			{
				labelFOFExePath.Text = Path.Combine(fofDirectory, "FrFoot2007.exe");
			}

			LoadPackages();
		}

		private void LoadPackages()
		{
			string[] packageFiles = Directory.GetFiles("ResourcePatcherPackages", "*.xml");
			packages = new List<PatcherPackage>();
			PatcherEntry curEntry = null;
			TreeNode parentNode = null;
			treeViewPatches.BeginUpdate();
			foreach (string packageFile in packageFiles)
			{
				PatcherPackage package = new PatcherPackage();
				packages.Add(package);
				XmlTextReader xmlFile = new XmlTextReader(packageFile);
				while (xmlFile.Read())
				{
					if (xmlFile.NodeType == XmlNodeType.Element)
					{
						if (xmlFile.Name == "resources")
						{
							package.name = xmlFile.GetAttribute("name");
							parentNode = treeViewPatches.Nodes.Add(package.name);
							package.dllPath = Path.ChangeExtension(packageFile, ".dll");
						}
						else if (xmlFile.Name == "entry")
						{
							curEntry = new PatcherEntry();
							curEntry.description = xmlFile.GetAttribute("name");
							parentNode.Nodes.Add(curEntry.description);
							package.entries.Add(curEntry);
						}
						else if (xmlFile.Name == "substitution")
						{
							Patch newPatch = new Patch();
							newPatch.packageResource = Int32.Parse(xmlFile.GetAttribute("pkgid"));
							newPatch.fofResource = Int32.Parse(xmlFile.GetAttribute("fofid"));
							curEntry.patches.Add(newPatch);
						}
					}
				}
				xmlFile.Close();
			}
			treeViewPatches.EndUpdate();
		}

		private void PatchResource(IntPtr dllHandle, IntPtr batchHandle, int srcResource, int destResource)
		{
			IntPtr hResLoad;     // handle to loaded resource 
			IntPtr hRes;         // handle/ptr. to res. info. in hExe 
			IntPtr lpResLock;    // pointer to resource data 
			bool result;

			// Locate the resource locally 
			textBoxStatus.Text += "Finding...";
			textBoxStatus.Refresh();
			hRes = WindowsUtilities.Resources.FindResource(dllHandle,
				srcResource, WindowsUtilities.Resources.RT_BITMAP_INT);
			if (hRes == IntPtr.Zero)
			{
				textBoxStatus.Text += "Could not find...";
				textBoxStatus.Refresh();
				return;
			}

			// Load the dialog box into global memory. 
			textBoxStatus.Text += "Loading...";
			textBoxStatus.Refresh();
			hResLoad = WindowsUtilities.Resources.LoadResource(dllHandle, hRes);
			if (hResLoad == IntPtr.Zero)
			{
				textBoxStatus.Text += "Could not load...";
				textBoxStatus.Refresh();
				return;
			}

			// Lock the dialog box into global memory. 
			textBoxStatus.Text += "Locking...";
			textBoxStatus.Refresh();
			lpResLock = WindowsUtilities.Resources.LockResource(hResLoad);
			if (lpResLock == IntPtr.Zero)
			{
				textBoxStatus.Text += "Could not lock...";
				textBoxStatus.Refresh();
				return;
			}

			// Add the dialog box resource to the update list. 
			textBoxStatus.Text += "Updating...";
			textBoxStatus.Refresh();
			result = WindowsUtilities.Resources.UpdateResource(
				batchHandle,				// update resource handle 
				WindowsUtilities.Resources.RT_BITMAP_INT,	// bitmap type 
				destResource,				// resource to replace
				1033,						// neutral language
				lpResLock,                  // ptr to resource info 
				WindowsUtilities.Resources.SizeofResource(dllHandle, hRes) // size of resource info. 
				);

			if (result == false)
			{
				textBoxStatus.Text += "Could not update...";
				textBoxStatus.Refresh();
			}
		}

		private void buttonPatch_Click(object sender, EventArgs e)
		{
			textBoxStatus.Clear();
			textBoxStatus.Refresh();

			textBoxStatus.Text += "Backing up FOF exe...";
			textBoxStatus.Refresh();
			string fofExePath = labelFOFExePath.Text;
			string backupPath = Path.ChangeExtension(fofExePath, ".bak");
			File.Copy(fofExePath, backupPath, true);
			textBoxStatus.Text += "Done!" + Environment.NewLine;
			textBoxStatus.Text += "Beginning resource batch" + Environment.NewLine;
			textBoxStatus.Refresh();

			IntPtr batchHandle = WindowsUtilities.Resources.BeginUpdateResource(fofExePath, false);

			TreeNode dllNode;
			int dllIndex = 0;
			foreach(PatcherPackage curPackage in packages)
			{
				dllNode = treeViewPatches.Nodes[dllIndex];
				textBoxStatus.Text += "  Opening " + Path.GetFileName(curPackage.dllPath) + Environment.NewLine;
				textBoxStatus.Refresh();
				IntPtr dllHandle = WindowsUtilities.Resources.LoadLibrary(curPackage.dllPath);

				try
				{
					int patchIndex = 0;
					TreeNode patchNode;
					foreach (PatcherEntry curEntry in curPackage.entries)
					{
						patchNode = dllNode.Nodes[patchIndex];
						if (patchNode.Checked)
						{
							textBoxStatus.Text += "    Patching " + curEntry.description + "..." + Environment.NewLine;
							textBoxStatus.Refresh();

							foreach (Patch curPatch in curEntry.patches)
							{
								textBoxStatus.Text += "      Applying...";
								textBoxStatus.Refresh();

								PatchResource(dllHandle, batchHandle, curPatch.packageResource, curPatch.fofResource);

								textBoxStatus.Text += "Done!" + Environment.NewLine;
								textBoxStatus.Refresh();
							}

							textBoxStatus.Text += "    Done!" + Environment.NewLine;
							textBoxStatus.Refresh();
						}

						++patchIndex;
					}
				}
				catch
				{
					MessageBox.Show("Failed attempting to apply patch " + curPackage.name,
						"Patch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					textBoxStatus.Text += Environment.NewLine;
					textBoxStatus.Refresh();
				}

				textBoxStatus.Text += "  Closing " + Path.GetFileName(curPackage.dllPath) + Environment.NewLine;
				textBoxStatus.Refresh();
				WindowsUtilities.Resources.FreeLibrary(dllHandle);
				++dllIndex;
			}

			textBoxStatus.Text += "Ending resource batch...";
			textBoxStatus.Refresh();
			WindowsUtilities.Resources.EndUpdateResource(batchHandle, false);
			textBoxStatus.Text += "Done!" + Environment.NewLine;
			textBoxStatus.Text += "Finished!";
			textBoxStatus.Refresh();
		}

		private void buttonFOFExePath_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.InitialDirectory = Path.GetDirectoryName(labelFOFExePath.Text);
			dlg.FileName = labelFOFExePath.Text;
			dlg.Filter = "exe files (*.exe)|*.exe";
			dlg.Multiselect = false;
			dlg.CheckFileExists = true;
			dlg.CheckPathExists = true;
			dlg.DefaultExt = "exe";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				labelFOFExePath.Text = dlg.FileName;
			}
		}

		private bool inACheck = false;

		private void treeViewPatches_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (inACheck)
			{
				return;
			}

			inACheck = true;

			TreeNode curNode = e.Node;
			TreeNode parentNode = e.Node.Parent;
			if (parentNode == null)
			{
				foreach (TreeNode childNode in curNode.Nodes)
				{
					childNode.Checked = curNode.Checked;
				}
			}
			else
			{
				bool anyChecked = false;
				foreach (TreeNode childNode in parentNode.Nodes)
				{
					if (childNode.Checked)
					{
						anyChecked = true;
						break;
					}
				}
				if (anyChecked)
				{
					parentNode.Checked = true;
				}
				else
				{
					parentNode.Checked = false;
				}
			}

			inACheck = false;
		}

		private void buttonMakeORGBackup_Click(object sender, EventArgs e)
		{
			textBoxStatus.Text = "Making org backup of FOF exe...";
			textBoxStatus.Refresh();
			string fofExePath = labelFOFExePath.Text;
			string backupPath = Path.ChangeExtension(fofExePath, ".org");
			File.Copy(fofExePath, backupPath, true);
			textBoxStatus.Text += "Done!" + Environment.NewLine;
			textBoxStatus.Text += "Finished!" + Environment.NewLine;
		}
	}
}
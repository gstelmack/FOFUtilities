namespace ResourcePatcher
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.treeViewPatches = new System.Windows.Forms.TreeView();
			this.buttonFOFExePath = new System.Windows.Forms.Button();
			this.labelFOFExePath = new System.Windows.Forms.Label();
			this.buttonPatch = new System.Windows.Forms.Button();
			this.textBoxStatus = new System.Windows.Forms.TextBox();
			this.buttonMakeORGBackup = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// treeViewPatches
			// 
			this.treeViewPatches.CheckBoxes = true;
			this.treeViewPatches.Location = new System.Drawing.Point(12, 67);
			this.treeViewPatches.Name = "treeViewPatches";
			this.treeViewPatches.Size = new System.Drawing.Size(258, 343);
			this.treeViewPatches.TabIndex = 0;
			this.treeViewPatches.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewPatches_AfterCheck);
			// 
			// buttonFOFExePath
			// 
			this.buttonFOFExePath.Location = new System.Drawing.Point(12, 38);
			this.buttonFOFExePath.Name = "buttonFOFExePath";
			this.buttonFOFExePath.Size = new System.Drawing.Size(121, 23);
			this.buttonFOFExePath.TabIndex = 1;
			this.buttonFOFExePath.Text = "FOF Exe Path";
			this.buttonFOFExePath.UseVisualStyleBackColor = true;
			this.buttonFOFExePath.Click += new System.EventHandler(this.buttonFOFExePath_Click);
			// 
			// labelFOFExePath
			// 
			this.labelFOFExePath.AutoEllipsis = true;
			this.labelFOFExePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelFOFExePath.Location = new System.Drawing.Point(139, 38);
			this.labelFOFExePath.Name = "labelFOFExePath";
			this.labelFOFExePath.Size = new System.Drawing.Size(501, 23);
			this.labelFOFExePath.TabIndex = 2;
			this.labelFOFExePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonPatch
			// 
			this.buttonPatch.Location = new System.Drawing.Point(276, 64);
			this.buttonPatch.Name = "buttonPatch";
			this.buttonPatch.Size = new System.Drawing.Size(364, 23);
			this.buttonPatch.TabIndex = 3;
			this.buttonPatch.Text = "Patch";
			this.buttonPatch.UseVisualStyleBackColor = true;
			this.buttonPatch.Click += new System.EventHandler(this.buttonPatch_Click);
			// 
			// textBoxStatus
			// 
			this.textBoxStatus.Location = new System.Drawing.Point(276, 93);
			this.textBoxStatus.Multiline = true;
			this.textBoxStatus.Name = "textBoxStatus";
			this.textBoxStatus.ReadOnly = true;
			this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxStatus.Size = new System.Drawing.Size(364, 317);
			this.textBoxStatus.TabIndex = 4;
			// 
			// buttonMakeORGBackup
			// 
			this.buttonMakeORGBackup.Location = new System.Drawing.Point(12, 12);
			this.buttonMakeORGBackup.Name = "buttonMakeORGBackup";
			this.buttonMakeORGBackup.Size = new System.Drawing.Size(628, 23);
			this.buttonMakeORGBackup.TabIndex = 5;
			this.buttonMakeORGBackup.Text = "Make ORG backup of FOF Exe";
			this.buttonMakeORGBackup.UseVisualStyleBackColor = true;
			this.buttonMakeORGBackup.Click += new System.EventHandler(this.buttonMakeORGBackup_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(652, 422);
			this.Controls.Add(this.buttonMakeORGBackup);
			this.Controls.Add(this.textBoxStatus);
			this.Controls.Add(this.buttonPatch);
			this.Controls.Add(this.labelFOFExePath);
			this.Controls.Add(this.buttonFOFExePath);
			this.Controls.Add(this.treeViewPatches);
			this.Name = "MainForm";
			this.Text = "FOF Resource Patcher";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TreeView treeViewPatches;
		private System.Windows.Forms.Button buttonFOFExePath;
		private System.Windows.Forms.Label labelFOFExePath;
		private System.Windows.Forms.Button buttonPatch;
		private System.Windows.Forms.TextBox textBoxStatus;
		private System.Windows.Forms.Button buttonMakeORGBackup;
	}
}


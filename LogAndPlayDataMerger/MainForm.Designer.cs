namespace LogAndPlayDataMerger
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
			this.buttonLogFileDirectory = new System.Windows.Forms.Button();
			this.labelOutputDirectory = new System.Windows.Forms.Label();
			this.checkBoxMergeFieldGoalPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxMergePuntPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxMergeOnsideKickPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxMergeInfoPlays = new System.Windows.Forms.CheckBox();
			this.buttonMergeFiles = new System.Windows.Forms.Button();
			this.labelOutputStatus = new System.Windows.Forms.Label();
			this.buttonOutputDirectory = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxMergePassPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxMergeRunPlays = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonLogFileDirectory
			// 
			this.buttonLogFileDirectory.Location = new System.Drawing.Point(162, 12);
			this.buttonLogFileDirectory.Name = "buttonLogFileDirectory";
			this.buttonLogFileDirectory.Size = new System.Drawing.Size(453, 23);
			this.buttonLogFileDirectory.TabIndex = 16;
			this.buttonLogFileDirectory.UseVisualStyleBackColor = true;
			this.buttonLogFileDirectory.Click += new System.EventHandler(this.buttonLogFileDirectory_Click);
			// 
			// labelOutputDirectory
			// 
			this.labelOutputDirectory.AutoSize = true;
			this.labelOutputDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOutputDirectory.Location = new System.Drawing.Point(11, 17);
			this.labelOutputDirectory.Name = "labelOutputDirectory";
			this.labelOutputDirectory.Size = new System.Drawing.Size(145, 13);
			this.labelOutputDirectory.TabIndex = 15;
			this.labelOutputDirectory.Text = "HTML Log File Directory";
			this.labelOutputDirectory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxMergeFieldGoalPlays
			// 
			this.checkBoxMergeFieldGoalPlays.AutoSize = true;
			this.checkBoxMergeFieldGoalPlays.Location = new System.Drawing.Point(148, 93);
			this.checkBoxMergeFieldGoalPlays.Name = "checkBoxMergeFieldGoalPlays";
			this.checkBoxMergeFieldGoalPlays.Size = new System.Drawing.Size(101, 17);
			this.checkBoxMergeFieldGoalPlays.TabIndex = 25;
			this.checkBoxMergeFieldGoalPlays.Text = "Merge FG Plays";
			this.checkBoxMergeFieldGoalPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxMergePuntPlays
			// 
			this.checkBoxMergePuntPlays.AutoSize = true;
			this.checkBoxMergePuntPlays.Location = new System.Drawing.Point(387, 70);
			this.checkBoxMergePuntPlays.Name = "checkBoxMergePuntPlays";
			this.checkBoxMergePuntPlays.Size = new System.Drawing.Size(109, 17);
			this.checkBoxMergePuntPlays.TabIndex = 24;
			this.checkBoxMergePuntPlays.Text = "Merge Punt Plays";
			this.checkBoxMergePuntPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxMergeOnsideKickPlays
			// 
			this.checkBoxMergeOnsideKickPlays.AutoSize = true;
			this.checkBoxMergeOnsideKickPlays.Location = new System.Drawing.Point(239, 70);
			this.checkBoxMergeOnsideKickPlays.Name = "checkBoxMergeOnsideKickPlays";
			this.checkBoxMergeOnsideKickPlays.Size = new System.Drawing.Size(144, 17);
			this.checkBoxMergeOnsideKickPlays.TabIndex = 23;
			this.checkBoxMergeOnsideKickPlays.Text = "Merge Onside Kick Plays";
			this.checkBoxMergeOnsideKickPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxMergeInfoPlays
			// 
			this.checkBoxMergeInfoPlays.AutoSize = true;
			this.checkBoxMergeInfoPlays.Location = new System.Drawing.Point(130, 70);
			this.checkBoxMergeInfoPlays.Name = "checkBoxMergeInfoPlays";
			this.checkBoxMergeInfoPlays.Size = new System.Drawing.Size(105, 17);
			this.checkBoxMergeInfoPlays.TabIndex = 22;
			this.checkBoxMergeInfoPlays.Text = "Merge Info Plays";
			this.checkBoxMergeInfoPlays.UseVisualStyleBackColor = true;
			// 
			// buttonMergeFiles
			// 
			this.buttonMergeFiles.Location = new System.Drawing.Point(276, 116);
			this.buttonMergeFiles.Name = "buttonMergeFiles";
			this.buttonMergeFiles.Size = new System.Drawing.Size(75, 23);
			this.buttonMergeFiles.TabIndex = 26;
			this.buttonMergeFiles.Text = "Merge Files";
			this.buttonMergeFiles.UseVisualStyleBackColor = true;
			this.buttonMergeFiles.Click += new System.EventHandler(this.buttonMergeFiles_Click);
			// 
			// labelOutputStatus
			// 
			this.labelOutputStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOutputStatus.Location = new System.Drawing.Point(12, 142);
			this.labelOutputStatus.Name = "labelOutputStatus";
			this.labelOutputStatus.Size = new System.Drawing.Size(591, 21);
			this.labelOutputStatus.TabIndex = 27;
			this.labelOutputStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonOutputDirectory
			// 
			this.buttonOutputDirectory.Location = new System.Drawing.Point(162, 41);
			this.buttonOutputDirectory.Name = "buttonOutputDirectory";
			this.buttonOutputDirectory.Size = new System.Drawing.Size(453, 23);
			this.buttonOutputDirectory.TabIndex = 29;
			this.buttonOutputDirectory.UseVisualStyleBackColor = true;
			this.buttonOutputDirectory.Click += new System.EventHandler(this.buttonOutputDirectory_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(30, 46);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 13);
			this.label1.TabIndex = 28;
			this.label1.Text = "Output Directory";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxMergePassPlays
			// 
			this.checkBoxMergePassPlays.AutoSize = true;
			this.checkBoxMergePassPlays.Location = new System.Drawing.Point(255, 93);
			this.checkBoxMergePassPlays.Name = "checkBoxMergePassPlays";
			this.checkBoxMergePassPlays.Size = new System.Drawing.Size(110, 17);
			this.checkBoxMergePassPlays.TabIndex = 30;
			this.checkBoxMergePassPlays.Text = "Merge Pass Plays";
			this.checkBoxMergePassPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxMergeRunPlays
			// 
			this.checkBoxMergeRunPlays.AutoSize = true;
			this.checkBoxMergeRunPlays.Location = new System.Drawing.Point(371, 93);
			this.checkBoxMergeRunPlays.Name = "checkBoxMergeRunPlays";
			this.checkBoxMergeRunPlays.Size = new System.Drawing.Size(107, 17);
			this.checkBoxMergeRunPlays.TabIndex = 31;
			this.checkBoxMergeRunPlays.Text = "Merge Run Plays";
			this.checkBoxMergeRunPlays.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(626, 174);
			this.Controls.Add(this.checkBoxMergeRunPlays);
			this.Controls.Add(this.checkBoxMergePassPlays);
			this.Controls.Add(this.buttonOutputDirectory);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.labelOutputStatus);
			this.Controls.Add(this.buttonMergeFiles);
			this.Controls.Add(this.checkBoxMergeFieldGoalPlays);
			this.Controls.Add(this.checkBoxMergePuntPlays);
			this.Controls.Add(this.checkBoxMergeOnsideKickPlays);
			this.Controls.Add(this.checkBoxMergeInfoPlays);
			this.Controls.Add(this.buttonLogFileDirectory);
			this.Controls.Add(this.labelOutputDirectory);
			this.Name = "MainForm";
			this.Text = "LogAndPlayDataMerger";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonLogFileDirectory;
		private System.Windows.Forms.Label labelOutputDirectory;
		private System.Windows.Forms.CheckBox checkBoxMergeFieldGoalPlays;
		private System.Windows.Forms.CheckBox checkBoxMergePuntPlays;
		private System.Windows.Forms.CheckBox checkBoxMergeOnsideKickPlays;
		private System.Windows.Forms.CheckBox checkBoxMergeInfoPlays;
		private System.Windows.Forms.Button buttonMergeFiles;
		private System.Windows.Forms.Label labelOutputStatus;
		private System.Windows.Forms.Button buttonOutputDirectory;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxMergePassPlays;
		private System.Windows.Forms.CheckBox checkBoxMergeRunPlays;
	}
}


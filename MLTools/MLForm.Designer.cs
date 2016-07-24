namespace MLTools
{
	partial class MLForm
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
			this.buttonRosterPath = new System.Windows.Forms.Button();
			this.buttonSplitRoster = new System.Windows.Forms.Button();
			this.buttonGenerateDrafts = new System.Windows.Forms.Button();
			this.buttonDraftTemplatePath = new System.Windows.Forms.Button();
			this.numericUpDownDraftCount = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonProcessGrowth = new System.Windows.Forms.Button();
			this.buttonGrowthDataPath = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDraftCount)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonRosterPath
			// 
			this.buttonRosterPath.Location = new System.Drawing.Point(165, 12);
			this.buttonRosterPath.Name = "buttonRosterPath";
			this.buttonRosterPath.Size = new System.Drawing.Size(412, 23);
			this.buttonRosterPath.TabIndex = 0;
			this.buttonRosterPath.Text = "Roster Path";
			this.buttonRosterPath.UseVisualStyleBackColor = true;
			this.buttonRosterPath.Click += new System.EventHandler(this.buttonRosterPath_Click);
			// 
			// buttonSplitRoster
			// 
			this.buttonSplitRoster.Location = new System.Drawing.Point(12, 12);
			this.buttonSplitRoster.Name = "buttonSplitRoster";
			this.buttonSplitRoster.Size = new System.Drawing.Size(147, 23);
			this.buttonSplitRoster.TabIndex = 1;
			this.buttonSplitRoster.Text = "Split Roster";
			this.buttonSplitRoster.UseVisualStyleBackColor = true;
			this.buttonSplitRoster.Click += new System.EventHandler(this.buttonSplitRoster_Click);
			// 
			// buttonGenerateDrafts
			// 
			this.buttonGenerateDrafts.Location = new System.Drawing.Point(12, 41);
			this.buttonGenerateDrafts.Name = "buttonGenerateDrafts";
			this.buttonGenerateDrafts.Size = new System.Drawing.Size(147, 23);
			this.buttonGenerateDrafts.TabIndex = 3;
			this.buttonGenerateDrafts.Text = "Generate Drafts";
			this.buttonGenerateDrafts.UseVisualStyleBackColor = true;
			this.buttonGenerateDrafts.Click += new System.EventHandler(this.buttonGenerateDrafts_Click);
			// 
			// buttonDraftTemplatePath
			// 
			this.buttonDraftTemplatePath.Location = new System.Drawing.Point(165, 41);
			this.buttonDraftTemplatePath.Name = "buttonDraftTemplatePath";
			this.buttonDraftTemplatePath.Size = new System.Drawing.Size(412, 23);
			this.buttonDraftTemplatePath.TabIndex = 2;
			this.buttonDraftTemplatePath.Text = "Draft Template Path";
			this.buttonDraftTemplatePath.UseVisualStyleBackColor = true;
			this.buttonDraftTemplatePath.Click += new System.EventHandler(this.buttonDraftTemplatePath_Click);
			// 
			// numericUpDownDraftCount
			// 
			this.numericUpDownDraftCount.Location = new System.Drawing.Point(165, 70);
			this.numericUpDownDraftCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownDraftCount.Name = "numericUpDownDraftCount";
			this.numericUpDownDraftCount.Size = new System.Drawing.Size(56, 20);
			this.numericUpDownDraftCount.TabIndex = 4;
			this.numericUpDownDraftCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(65, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Drafts to Generate";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonProcessGrowth
			// 
			this.buttonProcessGrowth.Location = new System.Drawing.Point(12, 97);
			this.buttonProcessGrowth.Name = "buttonProcessGrowth";
			this.buttonProcessGrowth.Size = new System.Drawing.Size(147, 23);
			this.buttonProcessGrowth.TabIndex = 7;
			this.buttonProcessGrowth.Text = "Process Growth";
			this.buttonProcessGrowth.UseVisualStyleBackColor = true;
			this.buttonProcessGrowth.Click += new System.EventHandler(this.buttonProcessGrowth_Click);
			// 
			// buttonGrowthDataPath
			// 
			this.buttonGrowthDataPath.Location = new System.Drawing.Point(165, 97);
			this.buttonGrowthDataPath.Name = "buttonGrowthDataPath";
			this.buttonGrowthDataPath.Size = new System.Drawing.Size(412, 23);
			this.buttonGrowthDataPath.TabIndex = 6;
			this.buttonGrowthDataPath.Text = "Growth Data Path";
			this.buttonGrowthDataPath.UseVisualStyleBackColor = true;
			this.buttonGrowthDataPath.Click += new System.EventHandler(this.buttonGrowthDataPath_Click);
			// 
			// MLForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(589, 217);
			this.Controls.Add(this.buttonProcessGrowth);
			this.Controls.Add(this.buttonGrowthDataPath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numericUpDownDraftCount);
			this.Controls.Add(this.buttonGenerateDrafts);
			this.Controls.Add(this.buttonDraftTemplatePath);
			this.Controls.Add(this.buttonSplitRoster);
			this.Controls.Add(this.buttonRosterPath);
			this.Name = "MLForm";
			this.Text = "MachineLearning Tools";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownDraftCount)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonRosterPath;
		private System.Windows.Forms.Button buttonSplitRoster;
		private System.Windows.Forms.Button buttonGenerateDrafts;
		private System.Windows.Forms.Button buttonDraftTemplatePath;
		private System.Windows.Forms.NumericUpDown numericUpDownDraftCount;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonProcessGrowth;
		private System.Windows.Forms.Button buttonGrowthDataPath;
	}
}


namespace Interrogator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.labelSavedGame = new System.Windows.Forms.Label();
            this.labelOutputStatus = new System.Windows.Forms.Label();
            this.comboBoxSavedGame = new System.Windows.Forms.ComboBox();
            this.checkBoxProcessFromSeason = new System.Windows.Forms.CheckBox();
            this.textBoxStartingSeason = new System.Windows.Forms.TextBox();
            this.buttonGenerateCSV = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFileToOpen = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonReleaseNotes = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelSavedGame
            // 
            this.labelSavedGame.AutoSize = true;
            this.labelSavedGame.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSavedGame.Location = new System.Drawing.Point(33, 35);
            this.labelSavedGame.Name = "labelSavedGame";
            this.labelSavedGame.Size = new System.Drawing.Size(79, 13);
            this.labelSavedGame.TabIndex = 9;
            this.labelSavedGame.Text = "Saved Game";
            this.labelSavedGame.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelOutputStatus
            // 
            this.labelOutputStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOutputStatus.Location = new System.Drawing.Point(12, 138);
            this.labelOutputStatus.Name = "labelOutputStatus";
            this.labelOutputStatus.Size = new System.Drawing.Size(392, 21);
            this.labelOutputStatus.TabIndex = 11;
            this.labelOutputStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBoxSavedGame
            // 
            this.comboBoxSavedGame.FormattingEnabled = true;
            this.comboBoxSavedGame.Location = new System.Drawing.Point(144, 32);
            this.comboBoxSavedGame.Name = "comboBoxSavedGame";
            this.comboBoxSavedGame.Size = new System.Drawing.Size(162, 21);
            this.comboBoxSavedGame.TabIndex = 12;
            this.comboBoxSavedGame.SelectedIndexChanged += new System.EventHandler(this.comboBoxSavedGame_SelectedIndexChanged);
            // 
            // checkBoxProcessFromSeason
            // 
            this.checkBoxProcessFromSeason.AutoSize = true;
            this.checkBoxProcessFromSeason.Location = new System.Drawing.Point(6, 61);
            this.checkBoxProcessFromSeason.Name = "checkBoxProcessFromSeason";
            this.checkBoxProcessFromSeason.Size = new System.Drawing.Size(132, 17);
            this.checkBoxProcessFromSeason.TabIndex = 15;
            this.checkBoxProcessFromSeason.Text = "Process From Season:";
            this.checkBoxProcessFromSeason.UseVisualStyleBackColor = true;
            // 
            // textBoxStartingSeason
            // 
            this.textBoxStartingSeason.Location = new System.Drawing.Point(144, 59);
            this.textBoxStartingSeason.MaxLength = 4;
            this.textBoxStartingSeason.Name = "textBoxStartingSeason";
            this.textBoxStartingSeason.Size = new System.Drawing.Size(162, 20);
            this.textBoxStartingSeason.TabIndex = 16;
            // 
            // buttonGenerateCSV
            // 
            this.buttonGenerateCSV.Location = new System.Drawing.Point(112, 112);
            this.buttonGenerateCSV.Name = "buttonGenerateCSV";
            this.buttonGenerateCSV.Size = new System.Drawing.Size(171, 23);
            this.buttonGenerateCSV.TabIndex = 17;
            this.buttonGenerateCSV.Text = "Load and Generate CSVs";
            this.buttonGenerateCSV.UseVisualStyleBackColor = true;
            this.buttonGenerateCSV.Click += new System.EventHandler(this.buttonGenerateCSV_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "File to Open when Done";
            // 
            // textBoxFileToOpen
            // 
            this.textBoxFileToOpen.Location = new System.Drawing.Point(144, 86);
            this.textBoxFileToOpen.Name = "textBoxFileToOpen";
            this.textBoxFileToOpen.Size = new System.Drawing.Size(162, 20);
            this.textBoxFileToOpen.TabIndex = 19;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(312, 85);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 20;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonReleaseNotes
            // 
            this.buttonReleaseNotes.Location = new System.Drawing.Point(312, 13);
            this.buttonReleaseNotes.Name = "buttonReleaseNotes";
            this.buttonReleaseNotes.Size = new System.Drawing.Size(92, 23);
            this.buttonReleaseNotes.TabIndex = 21;
            this.buttonReleaseNotes.Text = "Release Notes";
            this.buttonReleaseNotes.UseVisualStyleBackColor = true;
            this.buttonReleaseNotes.Click += new System.EventHandler(this.buttonReleaseNotes_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 173);
            this.Controls.Add(this.buttonReleaseNotes);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxFileToOpen);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonGenerateCSV);
            this.Controls.Add(this.textBoxStartingSeason);
            this.Controls.Add(this.checkBoxProcessFromSeason);
            this.Controls.Add(this.comboBoxSavedGame);
            this.Controls.Add(this.labelOutputStatus);
            this.Controls.Add(this.labelSavedGame);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Interrogator";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelSavedGame;
		private System.Windows.Forms.Label labelOutputStatus;
		private System.Windows.Forms.ComboBox comboBoxSavedGame;
		private System.Windows.Forms.CheckBox checkBoxProcessFromSeason;
		private System.Windows.Forms.TextBox textBoxStartingSeason;
		private System.Windows.Forms.Button buttonGenerateCSV;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFileToOpen;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonReleaseNotes;
    }
}
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
			this.checkBoxDumpInfoPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxDumpOnsideKickPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxDumpPuntPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxDumpFieldGoalPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxDumpRunPlays = new System.Windows.Forms.CheckBox();
			this.checkBoxDumpPassPlays = new System.Windows.Forms.CheckBox();
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
			this.labelOutputStatus.Location = new System.Drawing.Point(6, 133);
			this.labelOutputStatus.Name = "labelOutputStatus";
			this.labelOutputStatus.Size = new System.Drawing.Size(591, 21);
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
			this.buttonGenerateCSV.Location = new System.Drawing.Point(217, 107);
			this.buttonGenerateCSV.Name = "buttonGenerateCSV";
			this.buttonGenerateCSV.Size = new System.Drawing.Size(171, 23);
			this.buttonGenerateCSV.TabIndex = 17;
			this.buttonGenerateCSV.Text = "Load and Generate CSVs";
			this.buttonGenerateCSV.UseVisualStyleBackColor = true;
			this.buttonGenerateCSV.Click += new System.EventHandler(this.buttonGenerateCSV_Click);
			// 
			// checkBoxDumpInfoPlays
			// 
			this.checkBoxDumpInfoPlays.AutoSize = true;
			this.checkBoxDumpInfoPlays.Location = new System.Drawing.Point(322, 34);
			this.checkBoxDumpInfoPlays.Name = "checkBoxDumpInfoPlays";
			this.checkBoxDumpInfoPlays.Size = new System.Drawing.Size(103, 17);
			this.checkBoxDumpInfoPlays.TabIndex = 18;
			this.checkBoxDumpInfoPlays.Text = "Dump Info Plays";
			this.checkBoxDumpInfoPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxDumpOnsideKickPlays
			// 
			this.checkBoxDumpOnsideKickPlays.AutoSize = true;
			this.checkBoxDumpOnsideKickPlays.Location = new System.Drawing.Point(322, 61);
			this.checkBoxDumpOnsideKickPlays.Name = "checkBoxDumpOnsideKickPlays";
			this.checkBoxDumpOnsideKickPlays.Size = new System.Drawing.Size(142, 17);
			this.checkBoxDumpOnsideKickPlays.TabIndex = 19;
			this.checkBoxDumpOnsideKickPlays.Text = "Dump Onside Kick Plays";
			this.checkBoxDumpOnsideKickPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxDumpPuntPlays
			// 
			this.checkBoxDumpPuntPlays.AutoSize = true;
			this.checkBoxDumpPuntPlays.Location = new System.Drawing.Point(322, 85);
			this.checkBoxDumpPuntPlays.Name = "checkBoxDumpPuntPlays";
			this.checkBoxDumpPuntPlays.Size = new System.Drawing.Size(107, 17);
			this.checkBoxDumpPuntPlays.TabIndex = 20;
			this.checkBoxDumpPuntPlays.Text = "Dump Punt Plays";
			this.checkBoxDumpPuntPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxDumpFieldGoalPlays
			// 
			this.checkBoxDumpFieldGoalPlays.AutoSize = true;
			this.checkBoxDumpFieldGoalPlays.Location = new System.Drawing.Point(470, 85);
			this.checkBoxDumpFieldGoalPlays.Name = "checkBoxDumpFieldGoalPlays";
			this.checkBoxDumpFieldGoalPlays.Size = new System.Drawing.Size(99, 17);
			this.checkBoxDumpFieldGoalPlays.TabIndex = 21;
			this.checkBoxDumpFieldGoalPlays.Text = "Dump FG Plays";
			this.checkBoxDumpFieldGoalPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxDumpRunPlays
			// 
			this.checkBoxDumpRunPlays.AutoSize = true;
			this.checkBoxDumpRunPlays.Location = new System.Drawing.Point(470, 34);
			this.checkBoxDumpRunPlays.Name = "checkBoxDumpRunPlays";
			this.checkBoxDumpRunPlays.Size = new System.Drawing.Size(105, 17);
			this.checkBoxDumpRunPlays.TabIndex = 22;
			this.checkBoxDumpRunPlays.Text = "Dump Run Plays";
			this.checkBoxDumpRunPlays.UseVisualStyleBackColor = true;
			// 
			// checkBoxDumpPassPlays
			// 
			this.checkBoxDumpPassPlays.AutoSize = true;
			this.checkBoxDumpPassPlays.Location = new System.Drawing.Point(470, 61);
			this.checkBoxDumpPassPlays.Name = "checkBoxDumpPassPlays";
			this.checkBoxDumpPassPlays.Size = new System.Drawing.Size(108, 17);
			this.checkBoxDumpPassPlays.TabIndex = 23;
			this.checkBoxDumpPassPlays.Text = "Dump Pass Plays";
			this.checkBoxDumpPassPlays.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(604, 165);
			this.Controls.Add(this.checkBoxDumpPassPlays);
			this.Controls.Add(this.checkBoxDumpRunPlays);
			this.Controls.Add(this.checkBoxDumpFieldGoalPlays);
			this.Controls.Add(this.checkBoxDumpPuntPlays);
			this.Controls.Add(this.checkBoxDumpOnsideKickPlays);
			this.Controls.Add(this.checkBoxDumpInfoPlays);
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
		private System.Windows.Forms.CheckBox checkBoxDumpInfoPlays;
		private System.Windows.Forms.CheckBox checkBoxDumpOnsideKickPlays;
		private System.Windows.Forms.CheckBox checkBoxDumpPuntPlays;
		private System.Windows.Forms.CheckBox checkBoxDumpFieldGoalPlays;
		private System.Windows.Forms.CheckBox checkBoxDumpRunPlays;
		private System.Windows.Forms.CheckBox checkBoxDumpPassPlays;
	}
}
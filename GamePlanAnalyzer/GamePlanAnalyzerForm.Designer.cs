namespace GamePlanAnalyzer
{
	partial class GamePlanAnalyzerForm
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tabControlResults = new System.Windows.Forms.TabControl();
			this.tabPageLeaguePassingOffense = new System.Windows.Forms.TabPage();
			this.webBrowserLeaguePassingOffense = new System.Windows.Forms.WebBrowser();
			this.tabPageLeagueRushingOffense = new System.Windows.Forms.TabPage();
			this.webBrowserLeagueRushingOffense = new System.Windows.Forms.WebBrowser();
			this.tabPageTeamOffense = new System.Windows.Forms.TabPage();
			this.webBrowserTeamOffense = new System.Windows.Forms.WebBrowser();
			this.tabPageTeamDefense = new System.Windows.Forms.TabPage();
			this.webBrowserTeamDefense = new System.Windows.Forms.WebBrowser();
			this.tabPageOffensivePlaycalling = new System.Windows.Forms.TabPage();
			this.webBrowserOffensivePlaycalling = new System.Windows.Forms.WebBrowser();
			this.panel1 = new System.Windows.Forms.Panel();
			this.checkBoxAnalyzePostseason = new System.Windows.Forms.CheckBox();
			this.checkBoxAnalyzePreseason = new System.Windows.Forms.CheckBox();
			this.textBoxStartingSeason = new System.Windows.Forms.TextBox();
			this.labelStatus = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonAnalyze = new System.Windows.Forms.Button();
			this.comboBoxTeams = new System.Windows.Forms.ComboBox();
			this.comboBoxLeagues = new System.Windows.Forms.ComboBox();
			this.tabPageMiscellaneous = new System.Windows.Forms.TabPage();
			this.webBrowserMiscellaneous = new System.Windows.Forms.WebBrowser();
			this.tableLayoutPanel1.SuspendLayout();
			this.tabControlResults.SuspendLayout();
			this.tabPageLeaguePassingOffense.SuspendLayout();
			this.tabPageLeagueRushingOffense.SuspendLayout();
			this.tabPageTeamOffense.SuspendLayout();
			this.tabPageTeamDefense.SuspendLayout();
			this.tabPageOffensivePlaycalling.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPageMiscellaneous.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tabControlResults, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(984, 662);
			this.tableLayoutPanel1.TabIndex = 12;
			// 
			// tabControlResults
			// 
			this.tabControlResults.Controls.Add(this.tabPageLeaguePassingOffense);
			this.tabControlResults.Controls.Add(this.tabPageLeagueRushingOffense);
			this.tabControlResults.Controls.Add(this.tabPageTeamOffense);
			this.tabControlResults.Controls.Add(this.tabPageTeamDefense);
			this.tabControlResults.Controls.Add(this.tabPageOffensivePlaycalling);
			this.tabControlResults.Controls.Add(this.tabPageMiscellaneous);
			this.tabControlResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlResults.Location = new System.Drawing.Point(3, 103);
			this.tabControlResults.Name = "tabControlResults";
			this.tabControlResults.SelectedIndex = 0;
			this.tabControlResults.Size = new System.Drawing.Size(978, 556);
			this.tabControlResults.TabIndex = 8;
			// 
			// tabPageLeaguePassingOffense
			// 
			this.tabPageLeaguePassingOffense.Controls.Add(this.webBrowserLeaguePassingOffense);
			this.tabPageLeaguePassingOffense.Location = new System.Drawing.Point(4, 22);
			this.tabPageLeaguePassingOffense.Name = "tabPageLeaguePassingOffense";
			this.tabPageLeaguePassingOffense.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageLeaguePassingOffense.Size = new System.Drawing.Size(970, 530);
			this.tabPageLeaguePassingOffense.TabIndex = 0;
			this.tabPageLeaguePassingOffense.Text = "League Passing";
			this.tabPageLeaguePassingOffense.UseVisualStyleBackColor = true;
			// 
			// webBrowserLeaguePassingOffense
			// 
			this.webBrowserLeaguePassingOffense.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserLeaguePassingOffense.Location = new System.Drawing.Point(3, 3);
			this.webBrowserLeaguePassingOffense.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowserLeaguePassingOffense.Name = "webBrowserLeaguePassingOffense";
			this.webBrowserLeaguePassingOffense.Size = new System.Drawing.Size(964, 524);
			this.webBrowserLeaguePassingOffense.TabIndex = 0;
			// 
			// tabPageLeagueRushingOffense
			// 
			this.tabPageLeagueRushingOffense.Controls.Add(this.webBrowserLeagueRushingOffense);
			this.tabPageLeagueRushingOffense.Location = new System.Drawing.Point(4, 22);
			this.tabPageLeagueRushingOffense.Name = "tabPageLeagueRushingOffense";
			this.tabPageLeagueRushingOffense.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageLeagueRushingOffense.Size = new System.Drawing.Size(970, 530);
			this.tabPageLeagueRushingOffense.TabIndex = 1;
			this.tabPageLeagueRushingOffense.Text = "League Rushing";
			this.tabPageLeagueRushingOffense.UseVisualStyleBackColor = true;
			// 
			// webBrowserLeagueRushingOffense
			// 
			this.webBrowserLeagueRushingOffense.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserLeagueRushingOffense.Location = new System.Drawing.Point(3, 3);
			this.webBrowserLeagueRushingOffense.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowserLeagueRushingOffense.Name = "webBrowserLeagueRushingOffense";
			this.webBrowserLeagueRushingOffense.Size = new System.Drawing.Size(964, 524);
			this.webBrowserLeagueRushingOffense.TabIndex = 0;
			// 
			// tabPageTeamOffense
			// 
			this.tabPageTeamOffense.Controls.Add(this.webBrowserTeamOffense);
			this.tabPageTeamOffense.Location = new System.Drawing.Point(4, 22);
			this.tabPageTeamOffense.Name = "tabPageTeamOffense";
			this.tabPageTeamOffense.Size = new System.Drawing.Size(970, 530);
			this.tabPageTeamOffense.TabIndex = 2;
			this.tabPageTeamOffense.Text = "Team Offense";
			this.tabPageTeamOffense.UseVisualStyleBackColor = true;
			// 
			// webBrowserTeamOffense
			// 
			this.webBrowserTeamOffense.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserTeamOffense.Location = new System.Drawing.Point(0, 0);
			this.webBrowserTeamOffense.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowserTeamOffense.Name = "webBrowserTeamOffense";
			this.webBrowserTeamOffense.Size = new System.Drawing.Size(970, 530);
			this.webBrowserTeamOffense.TabIndex = 0;
			// 
			// tabPageTeamDefense
			// 
			this.tabPageTeamDefense.Controls.Add(this.webBrowserTeamDefense);
			this.tabPageTeamDefense.Location = new System.Drawing.Point(4, 22);
			this.tabPageTeamDefense.Name = "tabPageTeamDefense";
			this.tabPageTeamDefense.Size = new System.Drawing.Size(970, 530);
			this.tabPageTeamDefense.TabIndex = 3;
			this.tabPageTeamDefense.Text = "Team Defense";
			this.tabPageTeamDefense.UseVisualStyleBackColor = true;
			// 
			// webBrowserTeamDefense
			// 
			this.webBrowserTeamDefense.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserTeamDefense.Location = new System.Drawing.Point(0, 0);
			this.webBrowserTeamDefense.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowserTeamDefense.Name = "webBrowserTeamDefense";
			this.webBrowserTeamDefense.Size = new System.Drawing.Size(970, 530);
			this.webBrowserTeamDefense.TabIndex = 0;
			// 
			// tabPageOffensivePlaycalling
			// 
			this.tabPageOffensivePlaycalling.Controls.Add(this.webBrowserOffensivePlaycalling);
			this.tabPageOffensivePlaycalling.Location = new System.Drawing.Point(4, 22);
			this.tabPageOffensivePlaycalling.Name = "tabPageOffensivePlaycalling";
			this.tabPageOffensivePlaycalling.Size = new System.Drawing.Size(970, 530);
			this.tabPageOffensivePlaycalling.TabIndex = 4;
			this.tabPageOffensivePlaycalling.Text = "Offensive Playcalling";
			this.tabPageOffensivePlaycalling.UseVisualStyleBackColor = true;
			// 
			// webBrowserOffensivePlaycalling
			// 
			this.webBrowserOffensivePlaycalling.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserOffensivePlaycalling.Location = new System.Drawing.Point(0, 0);
			this.webBrowserOffensivePlaycalling.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowserOffensivePlaycalling.Name = "webBrowserOffensivePlaycalling";
			this.webBrowserOffensivePlaycalling.Size = new System.Drawing.Size(970, 530);
			this.webBrowserOffensivePlaycalling.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.checkBoxAnalyzePostseason);
			this.panel1.Controls.Add(this.checkBoxAnalyzePreseason);
			this.panel1.Controls.Add(this.textBoxStartingSeason);
			this.panel1.Controls.Add(this.labelStatus);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.buttonAnalyze);
			this.panel1.Controls.Add(this.comboBoxTeams);
			this.panel1.Controls.Add(this.comboBoxLeagues);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(978, 94);
			this.panel1.TabIndex = 0;
			// 
			// checkBoxAnalyzePostseason
			// 
			this.checkBoxAnalyzePostseason.AutoSize = true;
			this.checkBoxAnalyzePostseason.Location = new System.Drawing.Point(779, 37);
			this.checkBoxAnalyzePostseason.Name = "checkBoxAnalyzePostseason";
			this.checkBoxAnalyzePostseason.Size = new System.Drawing.Size(121, 17);
			this.checkBoxAnalyzePostseason.TabIndex = 21;
			this.checkBoxAnalyzePostseason.Text = "Analyze Postseason";
			this.checkBoxAnalyzePostseason.UseVisualStyleBackColor = true;
			// 
			// checkBoxAnalyzePreseason
			// 
			this.checkBoxAnalyzePreseason.AutoSize = true;
			this.checkBoxAnalyzePreseason.Location = new System.Drawing.Point(657, 37);
			this.checkBoxAnalyzePreseason.Name = "checkBoxAnalyzePreseason";
			this.checkBoxAnalyzePreseason.Size = new System.Drawing.Size(116, 17);
			this.checkBoxAnalyzePreseason.TabIndex = 20;
			this.checkBoxAnalyzePreseason.Text = "Analyze Preseason";
			this.checkBoxAnalyzePreseason.UseVisualStyleBackColor = true;
			// 
			// textBoxStartingSeason
			// 
			this.textBoxStartingSeason.Location = new System.Drawing.Point(301, 37);
			this.textBoxStartingSeason.Name = "textBoxStartingSeason";
			this.textBoxStartingSeason.Size = new System.Drawing.Size(96, 20);
			this.textBoxStartingSeason.TabIndex = 19;
			this.textBoxStartingSeason.Text = "0";
			// 
			// labelStatus
			// 
			this.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelStatus.Location = new System.Drawing.Point(9, 61);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(956, 23);
			this.labelStatus.TabIndex = 18;
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(300, 11);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 21);
			this.label3.TabIndex = 17;
			this.label3.Text = "Starting Season";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(181, 11);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(113, 21);
			this.label2.TabIndex = 16;
			this.label2.Text = "Pick Team";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(9, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(166, 21);
			this.label1.TabIndex = 15;
			this.label1.Text = "Pick League";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonAnalyze
			// 
			this.buttonAnalyze.Location = new System.Drawing.Point(906, 33);
			this.buttonAnalyze.Name = "buttonAnalyze";
			this.buttonAnalyze.Size = new System.Drawing.Size(63, 23);
			this.buttonAnalyze.TabIndex = 14;
			this.buttonAnalyze.Text = "Analyze";
			this.buttonAnalyze.UseVisualStyleBackColor = true;
			this.buttonAnalyze.Click += new System.EventHandler(this.buttonAnalyze_Click);
			// 
			// comboBoxTeams
			// 
			this.comboBoxTeams.FormattingEnabled = true;
			this.comboBoxTeams.Location = new System.Drawing.Point(181, 37);
			this.comboBoxTeams.Name = "comboBoxTeams";
			this.comboBoxTeams.Size = new System.Drawing.Size(113, 21);
			this.comboBoxTeams.TabIndex = 13;
			// 
			// comboBoxLeagues
			// 
			this.comboBoxLeagues.FormattingEnabled = true;
			this.comboBoxLeagues.Location = new System.Drawing.Point(9, 37);
			this.comboBoxLeagues.Name = "comboBoxLeagues";
			this.comboBoxLeagues.Size = new System.Drawing.Size(166, 21);
			this.comboBoxLeagues.TabIndex = 12;
			this.comboBoxLeagues.SelectedIndexChanged += new System.EventHandler(this.comboBoxLeagues_SelectedIndexChanged);
			// 
			// tabPageMiscellaneous
			// 
			this.tabPageMiscellaneous.Controls.Add(this.webBrowserMiscellaneous);
			this.tabPageMiscellaneous.Location = new System.Drawing.Point(4, 22);
			this.tabPageMiscellaneous.Name = "tabPageMiscellaneous";
			this.tabPageMiscellaneous.Size = new System.Drawing.Size(970, 530);
			this.tabPageMiscellaneous.TabIndex = 5;
			this.tabPageMiscellaneous.Text = "Miscellaneous";
			this.tabPageMiscellaneous.UseVisualStyleBackColor = true;
			// 
			// webBrowserMiscellaneous
			// 
			this.webBrowserMiscellaneous.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserMiscellaneous.Location = new System.Drawing.Point(0, 0);
			this.webBrowserMiscellaneous.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowserMiscellaneous.Name = "webBrowserMiscellaneous";
			this.webBrowserMiscellaneous.Size = new System.Drawing.Size(970, 530);
			this.webBrowserMiscellaneous.TabIndex = 1;
			// 
			// GamePlanAnalyzerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 662);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "GamePlanAnalyzerForm";
			this.Text = "Gameplan Analyzer";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tabControlResults.ResumeLayout(false);
			this.tabPageLeaguePassingOffense.ResumeLayout(false);
			this.tabPageLeagueRushingOffense.ResumeLayout(false);
			this.tabPageTeamOffense.ResumeLayout(false);
			this.tabPageTeamDefense.ResumeLayout(false);
			this.tabPageOffensivePlaycalling.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.tabPageMiscellaneous.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TabControl tabControlResults;
		private System.Windows.Forms.TabPage tabPageLeaguePassingOffense;
		private System.Windows.Forms.WebBrowser webBrowserLeaguePassingOffense;
		private System.Windows.Forms.TabPage tabPageLeagueRushingOffense;
		private System.Windows.Forms.WebBrowser webBrowserLeagueRushingOffense;
		private System.Windows.Forms.TabPage tabPageTeamOffense;
		private System.Windows.Forms.WebBrowser webBrowserTeamOffense;
		private System.Windows.Forms.TabPage tabPageTeamDefense;
		private System.Windows.Forms.WebBrowser webBrowserTeamDefense;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox checkBoxAnalyzePostseason;
		private System.Windows.Forms.CheckBox checkBoxAnalyzePreseason;
		private System.Windows.Forms.TextBox textBoxStartingSeason;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonAnalyze;
		private System.Windows.Forms.ComboBox comboBoxTeams;
		private System.Windows.Forms.ComboBox comboBoxLeagues;
		private System.Windows.Forms.TabPage tabPageOffensivePlaycalling;
		private System.Windows.Forms.WebBrowser webBrowserOffensivePlaycalling;
		private System.Windows.Forms.TabPage tabPageMiscellaneous;
		private System.Windows.Forms.WebBrowser webBrowserMiscellaneous;

	}
}


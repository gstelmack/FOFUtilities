namespace Extender
{
	partial class GameSelector
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
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.listBoxGames = new System.Windows.Forms.ListBox();
			this.checkBoxRunCareerReports = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(116, 162);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(197, 162);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// listBoxGames
			// 
			this.listBoxGames.FormattingEnabled = true;
			this.listBoxGames.Location = new System.Drawing.Point(13, 35);
			this.listBoxGames.Name = "listBoxGames";
			this.listBoxGames.ScrollAlwaysVisible = true;
			this.listBoxGames.Size = new System.Drawing.Size(259, 121);
			this.listBoxGames.TabIndex = 2;
			this.listBoxGames.SelectedIndexChanged += new System.EventHandler(this.listBoxGames_SelectedIndexChanged);
			// 
			// checkBoxRunCareerReports
			// 
			this.checkBoxRunCareerReports.AutoSize = true;
			this.checkBoxRunCareerReports.Location = new System.Drawing.Point(13, 13);
			this.checkBoxRunCareerReports.Name = "checkBoxRunCareerReports";
			this.checkBoxRunCareerReports.Size = new System.Drawing.Size(126, 17);
			this.checkBoxRunCareerReports.TabIndex = 3;
			this.checkBoxRunCareerReports.Text = "Run Career Reports?";
			this.checkBoxRunCareerReports.UseVisualStyleBackColor = true;
			this.checkBoxRunCareerReports.CheckedChanged += new System.EventHandler(this.checkBoxRunCareerReports_CheckedChanged);
			// 
			// GameSelector
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(284, 195);
			this.ControlBox = false;
			this.Controls.Add(this.checkBoxRunCareerReports);
			this.Controls.Add(this.listBoxGames);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GameSelector";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Choose FOF Game";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.ListBox listBoxGames;
		private System.Windows.Forms.CheckBox checkBoxRunCareerReports;
	}
}
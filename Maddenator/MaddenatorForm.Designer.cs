namespace Maddenator
{
	partial class MaddenatorForm
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
			this.buttonMaddenRosterPath = new System.Windows.Forms.Button();
			this.buttonExtractorCSVPath = new System.Windows.Forms.Button();
			this.buttonRunConversion = new System.Windows.Forms.Button();
			this.labelMaddenRosterPath = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.labelStatus = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonMaddenRosterPath
			// 
			this.buttonMaddenRosterPath.Location = new System.Drawing.Point(149, 12);
			this.buttonMaddenRosterPath.Name = "buttonMaddenRosterPath";
			this.buttonMaddenRosterPath.Size = new System.Drawing.Size(440, 23);
			this.buttonMaddenRosterPath.TabIndex = 0;
			this.buttonMaddenRosterPath.Text = "MaddenRosterPath";
			this.buttonMaddenRosterPath.UseVisualStyleBackColor = true;
			this.buttonMaddenRosterPath.Click += new System.EventHandler(this.buttonMaddenRosterPath_Click);
			// 
			// buttonExtractorCSVPath
			// 
			this.buttonExtractorCSVPath.Location = new System.Drawing.Point(149, 41);
			this.buttonExtractorCSVPath.Name = "buttonExtractorCSVPath";
			this.buttonExtractorCSVPath.Size = new System.Drawing.Size(440, 23);
			this.buttonExtractorCSVPath.TabIndex = 1;
			this.buttonExtractorCSVPath.Text = "ExtractorCSVPath";
			this.buttonExtractorCSVPath.UseVisualStyleBackColor = true;
			this.buttonExtractorCSVPath.Click += new System.EventHandler(this.buttonExtractorCSVPath_Click);
			// 
			// buttonRunConversion
			// 
			this.buttonRunConversion.Location = new System.Drawing.Point(424, 93);
			this.buttonRunConversion.Name = "buttonRunConversion";
			this.buttonRunConversion.Size = new System.Drawing.Size(165, 23);
			this.buttonRunConversion.TabIndex = 3;
			this.buttonRunConversion.Text = "Run Conversion";
			this.buttonRunConversion.UseVisualStyleBackColor = true;
			this.buttonRunConversion.Click += new System.EventHandler(this.buttonRunConversion_Click);
			// 
			// labelMaddenRosterPath
			// 
			this.labelMaddenRosterPath.Location = new System.Drawing.Point(12, 12);
			this.labelMaddenRosterPath.Name = "labelMaddenRosterPath";
			this.labelMaddenRosterPath.Size = new System.Drawing.Size(130, 23);
			this.labelMaddenRosterPath.TabIndex = 5;
			this.labelMaddenRosterPath.Text = "Madden Roster Path";
			this.labelMaddenRosterPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(130, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Extractor CSV Path";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelStatus
			// 
			this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelStatus.Location = new System.Drawing.Point(15, 67);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(574, 23);
			this.labelStatus.TabIndex = 8;
			this.labelStatus.Text = "Status";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MaddenatorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(601, 128);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.labelMaddenRosterPath);
			this.Controls.Add(this.buttonRunConversion);
			this.Controls.Add(this.buttonExtractorCSVPath);
			this.Controls.Add(this.buttonMaddenRosterPath);
			this.Name = "MaddenatorForm";
			this.Text = "Maddenator";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonMaddenRosterPath;
		private System.Windows.Forms.Button buttonExtractorCSVPath;
		private System.Windows.Forms.Button buttonRunConversion;
		private System.Windows.Forms.Label labelMaddenRosterPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelStatus;
	}
}


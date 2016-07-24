namespace DraftTrainerML
{
	partial class Form1
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
			this.decisionTreeViewCenters = new Accord.Controls.DecisionTreeView();
			this.textBoxTestError = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// decisionTreeViewCenters
			// 
			this.decisionTreeViewCenters.Codebook = null;
			this.decisionTreeViewCenters.Dock = System.Windows.Forms.DockStyle.Left;
			this.decisionTreeViewCenters.Location = new System.Drawing.Point(0, 0);
			this.decisionTreeViewCenters.Name = "decisionTreeViewCenters";
			this.decisionTreeViewCenters.Size = new System.Drawing.Size(429, 861);
			this.decisionTreeViewCenters.TabIndex = 0;
			this.decisionTreeViewCenters.TreeSource = null;
			// 
			// textBoxTestError
			// 
			this.textBoxTestError.Dock = System.Windows.Forms.DockStyle.Right;
			this.textBoxTestError.Location = new System.Drawing.Point(462, 0);
			this.textBoxTestError.Multiline = true;
			this.textBoxTestError.Name = "textBoxTestError";
			this.textBoxTestError.ReadOnly = true;
			this.textBoxTestError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxTestError.Size = new System.Drawing.Size(260, 861);
			this.textBoxTestError.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(722, 861);
			this.Controls.Add(this.textBoxTestError);
			this.Controls.Add(this.decisionTreeViewCenters);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Accord.Controls.DecisionTreeView decisionTreeViewCenters;
		private System.Windows.Forms.TextBox textBoxTestError;
	}
}


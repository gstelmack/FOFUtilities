namespace ChangeTracker
{
	partial class ChangeTrackerForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.buttonDatabase = new System.Windows.Forms.Button();
			this.buttonImportPath = new System.Windows.Forms.Button();
			this.dataGridViewPlayers = new System.Windows.Forms.DataGridView();
			this.dataGridViewPlayerDetails = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlayers)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlayerDetails)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Player Database";
			// 
			// buttonDatabase
			// 
			this.buttonDatabase.Location = new System.Drawing.Point(104, 8);
			this.buttonDatabase.Name = "buttonDatabase";
			this.buttonDatabase.Size = new System.Drawing.Size(379, 23);
			this.buttonDatabase.TabIndex = 1;
			this.buttonDatabase.Text = "Database";
			this.buttonDatabase.UseVisualStyleBackColor = true;
			this.buttonDatabase.Click += new System.EventHandler(this.buttonDatabase_Click);
			// 
			// buttonImportPath
			// 
			this.buttonImportPath.Location = new System.Drawing.Point(502, 8);
			this.buttonImportPath.Name = "buttonImportPath";
			this.buttonImportPath.Size = new System.Drawing.Size(470, 23);
			this.buttonImportPath.TabIndex = 3;
			this.buttonImportPath.Text = "Import Path";
			this.buttonImportPath.UseVisualStyleBackColor = true;
			this.buttonImportPath.Click += new System.EventHandler(this.buttonImportPath_Click);
			// 
			// dataGridViewPlayers
			// 
			this.dataGridViewPlayers.AllowUserToAddRows = false;
			this.dataGridViewPlayers.AllowUserToDeleteRows = false;
			this.dataGridViewPlayers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
			this.dataGridViewPlayers.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
			this.dataGridViewPlayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewPlayers.Location = new System.Drawing.Point(13, 37);
			this.dataGridViewPlayers.MultiSelect = false;
			this.dataGridViewPlayers.Name = "dataGridViewPlayers";
			this.dataGridViewPlayers.ReadOnly = true;
			this.dataGridViewPlayers.Size = new System.Drawing.Size(470, 613);
			this.dataGridViewPlayers.TabIndex = 4;
			this.dataGridViewPlayers.SelectionChanged += new System.EventHandler(this.dataGridViewPlayers_SelectionChanged);
			// 
			// dataGridViewPlayerDetails
			// 
			this.dataGridViewPlayerDetails.AllowUserToAddRows = false;
			this.dataGridViewPlayerDetails.AllowUserToDeleteRows = false;
			this.dataGridViewPlayerDetails.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
			this.dataGridViewPlayerDetails.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
			this.dataGridViewPlayerDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewPlayerDetails.Location = new System.Drawing.Point(502, 37);
			this.dataGridViewPlayerDetails.Name = "dataGridViewPlayerDetails";
			this.dataGridViewPlayerDetails.ReadOnly = true;
			this.dataGridViewPlayerDetails.Size = new System.Drawing.Size(470, 613);
			this.dataGridViewPlayerDetails.TabIndex = 5;
			// 
			// ChangeTrackerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 662);
			this.Controls.Add(this.dataGridViewPlayerDetails);
			this.Controls.Add(this.dataGridViewPlayers);
			this.Controls.Add(this.buttonImportPath);
			this.Controls.Add(this.buttonDatabase);
			this.Controls.Add(this.label1);
			this.Name = "ChangeTrackerForm";
			this.Text = "ChangeTrackerForm";
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlayers)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPlayerDetails)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonDatabase;
		private System.Windows.Forms.Button buttonImportPath;
		private System.Windows.Forms.DataGridView dataGridViewPlayers;
		private System.Windows.Forms.DataGridView dataGridViewPlayerDetails;
	}
}
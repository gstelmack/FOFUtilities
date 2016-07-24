namespace DBUpdater
{
	partial class DBUpdaterForm
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
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.textBoxServer = new System.Windows.Forms.TextBox();
			this.textBoxDatabase = new System.Windows.Forms.TextBox();
			this.textBoxUser = new System.Windows.Forms.TextBox();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.buttonUpdateDatabase = new System.Windows.Forms.Button();
			this.textBoxProgress = new System.Windows.Forms.TextBox();
			this.comboBoxLeague = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxPort = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(23, 36);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(151, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "MySQL Server";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(23, 90);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(151, 23);
			this.label3.TabIndex = 3;
			this.label3.Text = "MySQL Database";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(23, 117);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(151, 23);
			this.label4.TabIndex = 4;
			this.label4.Text = "Database User";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(23, 144);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(151, 23);
			this.label5.TabIndex = 5;
			this.label5.Text = "Database Password";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxServer
			// 
			this.textBoxServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxServer.Location = new System.Drawing.Point(180, 38);
			this.textBoxServer.Name = "textBoxServer";
			this.textBoxServer.Size = new System.Drawing.Size(248, 21);
			this.textBoxServer.TabIndex = 6;
			// 
			// textBoxDatabase
			// 
			this.textBoxDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxDatabase.Location = new System.Drawing.Point(180, 91);
			this.textBoxDatabase.Name = "textBoxDatabase";
			this.textBoxDatabase.Size = new System.Drawing.Size(248, 21);
			this.textBoxDatabase.TabIndex = 7;
			// 
			// textBoxUser
			// 
			this.textBoxUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxUser.Location = new System.Drawing.Point(180, 117);
			this.textBoxUser.Name = "textBoxUser";
			this.textBoxUser.Size = new System.Drawing.Size(248, 21);
			this.textBoxUser.TabIndex = 8;
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxPassword.Location = new System.Drawing.Point(180, 143);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.Size = new System.Drawing.Size(248, 21);
			this.textBoxPassword.TabIndex = 9;
			this.textBoxPassword.UseSystemPasswordChar = true;
			// 
			// buttonUpdateDatabase
			// 
			this.buttonUpdateDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonUpdateDatabase.Location = new System.Drawing.Point(190, 174);
			this.buttonUpdateDatabase.Name = "buttonUpdateDatabase";
			this.buttonUpdateDatabase.Size = new System.Drawing.Size(116, 23);
			this.buttonUpdateDatabase.TabIndex = 10;
			this.buttonUpdateDatabase.Text = "Update Database";
			this.buttonUpdateDatabase.UseVisualStyleBackColor = true;
			this.buttonUpdateDatabase.Click += new System.EventHandler(this.buttonUpdateDatabase_Click);
			// 
			// textBoxProgress
			// 
			this.textBoxProgress.Location = new System.Drawing.Point(13, 203);
			this.textBoxProgress.Multiline = true;
			this.textBoxProgress.Name = "textBoxProgress";
			this.textBoxProgress.ReadOnly = true;
			this.textBoxProgress.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxProgress.Size = new System.Drawing.Size(457, 453);
			this.textBoxProgress.TabIndex = 11;
			// 
			// comboBoxLeague
			// 
			this.comboBoxLeague.FormattingEnabled = true;
			this.comboBoxLeague.Location = new System.Drawing.Point(180, 12);
			this.comboBoxLeague.Name = "comboBoxLeague";
			this.comboBoxLeague.Size = new System.Drawing.Size(248, 21);
			this.comboBoxLeague.TabIndex = 12;
			this.comboBoxLeague.SelectedIndexChanged += new System.EventHandler(this.comboBoxLeague_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(23, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(151, 23);
			this.label1.TabIndex = 13;
			this.label1.Text = "League";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxPort
			// 
			this.textBoxPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxPort.Location = new System.Drawing.Point(180, 65);
			this.textBoxPort.Name = "textBoxPort";
			this.textBoxPort.Size = new System.Drawing.Size(248, 21);
			this.textBoxPort.TabIndex = 15;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(23, 64);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(151, 23);
			this.label6.TabIndex = 14;
			this.label6.Text = "Port";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DBUpdaterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(482, 668);
			this.Controls.Add(this.textBoxPort);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBoxLeague);
			this.Controls.Add(this.textBoxProgress);
			this.Controls.Add(this.buttonUpdateDatabase);
			this.Controls.Add(this.textBoxPassword);
			this.Controls.Add(this.textBoxUser);
			this.Controls.Add(this.textBoxDatabase);
			this.Controls.Add(this.textBoxServer);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Name = "DBUpdaterForm";
			this.Text = "DB Updater";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBoxServer;
		private System.Windows.Forms.TextBox textBoxDatabase;
		private System.Windows.Forms.TextBox textBoxUser;
		private System.Windows.Forms.TextBox textBoxPassword;
		private System.Windows.Forms.Button buttonUpdateDatabase;
		private System.Windows.Forms.TextBox textBoxProgress;
		private System.Windows.Forms.ComboBox comboBoxLeague;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxPort;
		private System.Windows.Forms.Label label6;
	}
}


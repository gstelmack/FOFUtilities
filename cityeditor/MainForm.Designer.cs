namespace CityEditor
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
			this.comboBoxCity = new System.Windows.Forms.ComboBox();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.labelName = new System.Windows.Forms.Label();
			this.labelAbbrev = new System.Windows.Forms.Label();
			this.labelPopulation = new System.Windows.Forms.Label();
			this.labelGrowthRate = new System.Windows.Forms.Label();
			this.labelAvgIncome = new System.Windows.Forms.Label();
			this.labelPovertyLevel = new System.Windows.Forms.Label();
			this.labelEntComp = new System.Windows.Forms.Label();
			this.labelRegion = new System.Windows.Forms.Label();
			this.labelState = new System.Windows.Forms.Label();
			this.labelLatitude = new System.Windows.Forms.Label();
			this.labelLongitude = new System.Windows.Forms.Label();
			this.labelElevation = new System.Windows.Forms.Label();
			this.textBoxAbbreviation = new System.Windows.Forms.TextBox();
			this.textBoxPopulation = new System.Windows.Forms.TextBox();
			this.textBoxGrowthRate = new System.Windows.Forms.TextBox();
			this.textBoxAvgIncome = new System.Windows.Forms.TextBox();
			this.textBoxPovertyLevel = new System.Windows.Forms.TextBox();
			this.textBoxEntComp = new System.Windows.Forms.TextBox();
			this.comboBoxRegion = new System.Windows.Forms.ComboBox();
			this.comboBoxState = new System.Windows.Forms.ComboBox();
			this.textBoxLatitude = new System.Windows.Forms.TextBox();
			this.textBoxLongitude = new System.Windows.Forms.TextBox();
			this.textBoxElevation = new System.Windows.Forms.TextBox();
			this.labelSeptemberHigh = new System.Windows.Forms.Label();
			this.labelSeptemberLow = new System.Windows.Forms.Label();
			this.labelSeptemberHumidity = new System.Windows.Forms.Label();
			this.labelDecemberHigh = new System.Windows.Forms.Label();
			this.labelDecemberLow = new System.Windows.Forms.Label();
			this.labelDecemberHumidity = new System.Windows.Forms.Label();
			this.label90DegreeDays = new System.Windows.Forms.Label();
			this.labelSnowDays = new System.Windows.Forms.Label();
			this.labelStormyDays = new System.Windows.Forms.Label();
			this.textBoxSeptemberHigh = new System.Windows.Forms.TextBox();
			this.textBoxSeptemberLow = new System.Windows.Forms.TextBox();
			this.textBoxSeptemberHumidity = new System.Windows.Forms.TextBox();
			this.textBoxDecemberHigh = new System.Windows.Forms.TextBox();
			this.textBoxDecemberLow = new System.Windows.Forms.TextBox();
			this.textBoxDecemberHumidity = new System.Windows.Forms.TextBox();
			this.textBox90DegreeDays = new System.Windows.Forms.TextBox();
			this.textBoxSnowDays = new System.Windows.Forms.TextBox();
			this.textBoxStormyDays = new System.Windows.Forms.TextBox();
			this.buttonSave = new System.Windows.Forms.Button();
			this.buttonMakeOrgBackups = new System.Windows.Forms.Button();
			this.labelStatus = new System.Windows.Forms.Label();
			this.buttonCopyEconomics = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// comboBoxCity
			// 
			this.comboBoxCity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboBoxCity.FormattingEnabled = true;
			this.comboBoxCity.Location = new System.Drawing.Point(13, 13);
			this.comboBoxCity.Name = "comboBoxCity";
			this.comboBoxCity.Size = new System.Drawing.Size(248, 21);
			this.comboBoxCity.TabIndex = 0;
			this.comboBoxCity.SelectedIndexChanged += new System.EventHandler(this.comboBoxCity_SelectedIndexChanged);
			// 
			// textBoxName
			// 
			this.textBoxName.Location = new System.Drawing.Point(167, 54);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(182, 20);
			this.textBoxName.TabIndex = 1;
			this.textBoxName.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxName_Validating);
			// 
			// labelName
			// 
			this.labelName.Location = new System.Drawing.Point(13, 52);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(148, 23);
			this.labelName.TabIndex = 2;
			this.labelName.Text = "Name";
			this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAbbrev
			// 
			this.labelAbbrev.Location = new System.Drawing.Point(13, 75);
			this.labelAbbrev.Name = "labelAbbrev";
			this.labelAbbrev.Size = new System.Drawing.Size(148, 23);
			this.labelAbbrev.TabIndex = 3;
			this.labelAbbrev.Text = "Abbreviation";
			this.labelAbbrev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPopulation
			// 
			this.labelPopulation.Location = new System.Drawing.Point(13, 98);
			this.labelPopulation.Name = "labelPopulation";
			this.labelPopulation.Size = new System.Drawing.Size(148, 23);
			this.labelPopulation.TabIndex = 4;
			this.labelPopulation.Text = "Population";
			this.labelPopulation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelGrowthRate
			// 
			this.labelGrowthRate.Location = new System.Drawing.Point(13, 121);
			this.labelGrowthRate.Name = "labelGrowthRate";
			this.labelGrowthRate.Size = new System.Drawing.Size(148, 23);
			this.labelGrowthRate.TabIndex = 5;
			this.labelGrowthRate.Text = "Growth Rate";
			this.labelGrowthRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAvgIncome
			// 
			this.labelAvgIncome.Location = new System.Drawing.Point(13, 144);
			this.labelAvgIncome.Name = "labelAvgIncome";
			this.labelAvgIncome.Size = new System.Drawing.Size(148, 23);
			this.labelAvgIncome.TabIndex = 6;
			this.labelAvgIncome.Text = "Average Income";
			this.labelAvgIncome.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPovertyLevel
			// 
			this.labelPovertyLevel.Location = new System.Drawing.Point(13, 167);
			this.labelPovertyLevel.Name = "labelPovertyLevel";
			this.labelPovertyLevel.Size = new System.Drawing.Size(148, 23);
			this.labelPovertyLevel.TabIndex = 7;
			this.labelPovertyLevel.Text = "Poverty Level";
			this.labelPovertyLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelEntComp
			// 
			this.labelEntComp.Location = new System.Drawing.Point(13, 190);
			this.labelEntComp.Name = "labelEntComp";
			this.labelEntComp.Size = new System.Drawing.Size(148, 23);
			this.labelEntComp.TabIndex = 8;
			this.labelEntComp.Text = "Entertainment Competition";
			this.labelEntComp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelRegion
			// 
			this.labelRegion.Location = new System.Drawing.Point(13, 213);
			this.labelRegion.Name = "labelRegion";
			this.labelRegion.Size = new System.Drawing.Size(148, 23);
			this.labelRegion.TabIndex = 9;
			this.labelRegion.Text = "Region";
			this.labelRegion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelState
			// 
			this.labelState.Location = new System.Drawing.Point(13, 236);
			this.labelState.Name = "labelState";
			this.labelState.Size = new System.Drawing.Size(148, 23);
			this.labelState.TabIndex = 10;
			this.labelState.Text = "State";
			this.labelState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLatitude
			// 
			this.labelLatitude.Location = new System.Drawing.Point(13, 286);
			this.labelLatitude.Name = "labelLatitude";
			this.labelLatitude.Size = new System.Drawing.Size(148, 23);
			this.labelLatitude.TabIndex = 11;
			this.labelLatitude.Text = "Latitude";
			this.labelLatitude.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelLongitude
			// 
			this.labelLongitude.Location = new System.Drawing.Point(13, 309);
			this.labelLongitude.Name = "labelLongitude";
			this.labelLongitude.Size = new System.Drawing.Size(148, 23);
			this.labelLongitude.TabIndex = 12;
			this.labelLongitude.Text = "Longitude";
			this.labelLongitude.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelElevation
			// 
			this.labelElevation.Location = new System.Drawing.Point(13, 332);
			this.labelElevation.Name = "labelElevation";
			this.labelElevation.Size = new System.Drawing.Size(148, 23);
			this.labelElevation.TabIndex = 13;
			this.labelElevation.Text = "Elevation";
			this.labelElevation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxAbbreviation
			// 
			this.textBoxAbbreviation.Location = new System.Drawing.Point(167, 77);
			this.textBoxAbbreviation.Name = "textBoxAbbreviation";
			this.textBoxAbbreviation.Size = new System.Drawing.Size(61, 20);
			this.textBoxAbbreviation.TabIndex = 14;
			this.textBoxAbbreviation.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxAbbreviation_Validating);
			// 
			// textBoxPopulation
			// 
			this.textBoxPopulation.Location = new System.Drawing.Point(167, 100);
			this.textBoxPopulation.Name = "textBoxPopulation";
			this.textBoxPopulation.Size = new System.Drawing.Size(94, 20);
			this.textBoxPopulation.TabIndex = 15;
			this.textBoxPopulation.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxPopulation_Validating);
			// 
			// textBoxGrowthRate
			// 
			this.textBoxGrowthRate.Location = new System.Drawing.Point(167, 123);
			this.textBoxGrowthRate.Name = "textBoxGrowthRate";
			this.textBoxGrowthRate.Size = new System.Drawing.Size(94, 20);
			this.textBoxGrowthRate.TabIndex = 16;
			this.textBoxGrowthRate.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxGrowthRate_Validating);
			// 
			// textBoxAvgIncome
			// 
			this.textBoxAvgIncome.Location = new System.Drawing.Point(167, 146);
			this.textBoxAvgIncome.Name = "textBoxAvgIncome";
			this.textBoxAvgIncome.Size = new System.Drawing.Size(94, 20);
			this.textBoxAvgIncome.TabIndex = 17;
			this.textBoxAvgIncome.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxAvgIncome_Validating);
			// 
			// textBoxPovertyLevel
			// 
			this.textBoxPovertyLevel.Location = new System.Drawing.Point(167, 169);
			this.textBoxPovertyLevel.Name = "textBoxPovertyLevel";
			this.textBoxPovertyLevel.Size = new System.Drawing.Size(61, 20);
			this.textBoxPovertyLevel.TabIndex = 18;
			this.textBoxPovertyLevel.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxPovertyLevel_Validating);
			// 
			// textBoxEntComp
			// 
			this.textBoxEntComp.Location = new System.Drawing.Point(167, 192);
			this.textBoxEntComp.Name = "textBoxEntComp";
			this.textBoxEntComp.Size = new System.Drawing.Size(61, 20);
			this.textBoxEntComp.TabIndex = 19;
			this.textBoxEntComp.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxEntComp_Validating);
			// 
			// comboBoxRegion
			// 
			this.comboBoxRegion.FormattingEnabled = true;
			this.comboBoxRegion.Location = new System.Drawing.Point(167, 215);
			this.comboBoxRegion.Name = "comboBoxRegion";
			this.comboBoxRegion.Size = new System.Drawing.Size(182, 21);
			this.comboBoxRegion.TabIndex = 20;
			this.comboBoxRegion.SelectedIndexChanged += new System.EventHandler(this.comboBoxRegion_SelectedIndexChanged);
			// 
			// comboBoxState
			// 
			this.comboBoxState.FormattingEnabled = true;
			this.comboBoxState.Location = new System.Drawing.Point(167, 238);
			this.comboBoxState.Name = "comboBoxState";
			this.comboBoxState.Size = new System.Drawing.Size(182, 21);
			this.comboBoxState.TabIndex = 21;
			this.comboBoxState.SelectedIndexChanged += new System.EventHandler(this.comboBoxState_SelectedIndexChanged);
			// 
			// textBoxLatitude
			// 
			this.textBoxLatitude.Location = new System.Drawing.Point(167, 288);
			this.textBoxLatitude.Name = "textBoxLatitude";
			this.textBoxLatitude.Size = new System.Drawing.Size(94, 20);
			this.textBoxLatitude.TabIndex = 22;
			this.textBoxLatitude.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxLatitude_Validating);
			// 
			// textBoxLongitude
			// 
			this.textBoxLongitude.Location = new System.Drawing.Point(167, 311);
			this.textBoxLongitude.Name = "textBoxLongitude";
			this.textBoxLongitude.Size = new System.Drawing.Size(94, 20);
			this.textBoxLongitude.TabIndex = 23;
			this.textBoxLongitude.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxLongitude_Validating);
			// 
			// textBoxElevation
			// 
			this.textBoxElevation.Location = new System.Drawing.Point(167, 334);
			this.textBoxElevation.Name = "textBoxElevation";
			this.textBoxElevation.Size = new System.Drawing.Size(94, 20);
			this.textBoxElevation.TabIndex = 24;
			this.textBoxElevation.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxElevation_Validating);
			// 
			// labelSeptemberHigh
			// 
			this.labelSeptemberHigh.Location = new System.Drawing.Point(376, 52);
			this.labelSeptemberHigh.Name = "labelSeptemberHigh";
			this.labelSeptemberHigh.Size = new System.Drawing.Size(127, 23);
			this.labelSeptemberHigh.TabIndex = 25;
			this.labelSeptemberHigh.Text = "September High";
			this.labelSeptemberHigh.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSeptemberLow
			// 
			this.labelSeptemberLow.Location = new System.Drawing.Point(376, 75);
			this.labelSeptemberLow.Name = "labelSeptemberLow";
			this.labelSeptemberLow.Size = new System.Drawing.Size(127, 23);
			this.labelSeptemberLow.TabIndex = 26;
			this.labelSeptemberLow.Text = "September Low";
			this.labelSeptemberLow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSeptemberHumidity
			// 
			this.labelSeptemberHumidity.Location = new System.Drawing.Point(376, 98);
			this.labelSeptemberHumidity.Name = "labelSeptemberHumidity";
			this.labelSeptemberHumidity.Size = new System.Drawing.Size(127, 23);
			this.labelSeptemberHumidity.TabIndex = 27;
			this.labelSeptemberHumidity.Text = "September Humidity";
			this.labelSeptemberHumidity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDecemberHigh
			// 
			this.labelDecemberHigh.Location = new System.Drawing.Point(376, 121);
			this.labelDecemberHigh.Name = "labelDecemberHigh";
			this.labelDecemberHigh.Size = new System.Drawing.Size(127, 23);
			this.labelDecemberHigh.TabIndex = 28;
			this.labelDecemberHigh.Text = "December High";
			this.labelDecemberHigh.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDecemberLow
			// 
			this.labelDecemberLow.Location = new System.Drawing.Point(376, 144);
			this.labelDecemberLow.Name = "labelDecemberLow";
			this.labelDecemberLow.Size = new System.Drawing.Size(127, 23);
			this.labelDecemberLow.TabIndex = 29;
			this.labelDecemberLow.Text = "DecemberLow";
			this.labelDecemberLow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDecemberHumidity
			// 
			this.labelDecemberHumidity.Location = new System.Drawing.Point(376, 167);
			this.labelDecemberHumidity.Name = "labelDecemberHumidity";
			this.labelDecemberHumidity.Size = new System.Drawing.Size(127, 23);
			this.labelDecemberHumidity.TabIndex = 30;
			this.labelDecemberHumidity.Text = "December Humidity";
			this.labelDecemberHumidity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label90DegreeDays
			// 
			this.label90DegreeDays.Location = new System.Drawing.Point(376, 190);
			this.label90DegreeDays.Name = "label90DegreeDays";
			this.label90DegreeDays.Size = new System.Drawing.Size(127, 23);
			this.label90DegreeDays.TabIndex = 31;
			this.label90DegreeDays.Text = "90 Degree Days";
			this.label90DegreeDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSnowDays
			// 
			this.labelSnowDays.Location = new System.Drawing.Point(376, 213);
			this.labelSnowDays.Name = "labelSnowDays";
			this.labelSnowDays.Size = new System.Drawing.Size(127, 23);
			this.labelSnowDays.TabIndex = 32;
			this.labelSnowDays.Text = "Snow Days";
			this.labelSnowDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelStormyDays
			// 
			this.labelStormyDays.Location = new System.Drawing.Point(376, 236);
			this.labelStormyDays.Name = "labelStormyDays";
			this.labelStormyDays.Size = new System.Drawing.Size(127, 23);
			this.labelStormyDays.TabIndex = 33;
			this.labelStormyDays.Text = "Stormy Days";
			this.labelStormyDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBoxSeptemberHigh
			// 
			this.textBoxSeptemberHigh.Location = new System.Drawing.Point(509, 54);
			this.textBoxSeptemberHigh.Name = "textBoxSeptemberHigh";
			this.textBoxSeptemberHigh.Size = new System.Drawing.Size(61, 20);
			this.textBoxSeptemberHigh.TabIndex = 34;
			this.textBoxSeptemberHigh.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxSeptemberHigh_Validating);
			// 
			// textBoxSeptemberLow
			// 
			this.textBoxSeptemberLow.Location = new System.Drawing.Point(509, 77);
			this.textBoxSeptemberLow.Name = "textBoxSeptemberLow";
			this.textBoxSeptemberLow.Size = new System.Drawing.Size(61, 20);
			this.textBoxSeptemberLow.TabIndex = 35;
			this.textBoxSeptemberLow.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxSeptemberLow_Validating);
			// 
			// textBoxSeptemberHumidity
			// 
			this.textBoxSeptemberHumidity.Location = new System.Drawing.Point(509, 100);
			this.textBoxSeptemberHumidity.Name = "textBoxSeptemberHumidity";
			this.textBoxSeptemberHumidity.Size = new System.Drawing.Size(61, 20);
			this.textBoxSeptemberHumidity.TabIndex = 36;
			this.textBoxSeptemberHumidity.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxSeptemberHumidity_Validating);
			// 
			// textBoxDecemberHigh
			// 
			this.textBoxDecemberHigh.Location = new System.Drawing.Point(509, 123);
			this.textBoxDecemberHigh.Name = "textBoxDecemberHigh";
			this.textBoxDecemberHigh.Size = new System.Drawing.Size(61, 20);
			this.textBoxDecemberHigh.TabIndex = 37;
			this.textBoxDecemberHigh.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxDecemberHigh_Validating);
			// 
			// textBoxDecemberLow
			// 
			this.textBoxDecemberLow.Location = new System.Drawing.Point(509, 146);
			this.textBoxDecemberLow.Name = "textBoxDecemberLow";
			this.textBoxDecemberLow.Size = new System.Drawing.Size(61, 20);
			this.textBoxDecemberLow.TabIndex = 38;
			this.textBoxDecemberLow.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxDecemberLow_Validating);
			// 
			// textBoxDecemberHumidity
			// 
			this.textBoxDecemberHumidity.Location = new System.Drawing.Point(509, 169);
			this.textBoxDecemberHumidity.Name = "textBoxDecemberHumidity";
			this.textBoxDecemberHumidity.Size = new System.Drawing.Size(61, 20);
			this.textBoxDecemberHumidity.TabIndex = 39;
			this.textBoxDecemberHumidity.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxDecemberHumidity_Validating);
			// 
			// textBox90DegreeDays
			// 
			this.textBox90DegreeDays.Location = new System.Drawing.Point(509, 192);
			this.textBox90DegreeDays.Name = "textBox90DegreeDays";
			this.textBox90DegreeDays.Size = new System.Drawing.Size(61, 20);
			this.textBox90DegreeDays.TabIndex = 40;
			this.textBox90DegreeDays.Validating += new System.ComponentModel.CancelEventHandler(this.textBox90DegreeDays_Validating);
			// 
			// textBoxSnowDays
			// 
			this.textBoxSnowDays.Location = new System.Drawing.Point(509, 215);
			this.textBoxSnowDays.Name = "textBoxSnowDays";
			this.textBoxSnowDays.Size = new System.Drawing.Size(61, 20);
			this.textBoxSnowDays.TabIndex = 41;
			this.textBoxSnowDays.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxSnowDays_Validating);
			// 
			// textBoxStormyDays
			// 
			this.textBoxStormyDays.Location = new System.Drawing.Point(509, 239);
			this.textBoxStormyDays.Name = "textBoxStormyDays";
			this.textBoxStormyDays.Size = new System.Drawing.Size(61, 20);
			this.textBoxStormyDays.TabIndex = 42;
			this.textBoxStormyDays.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxStormyDays_Validating);
			// 
			// buttonSave
			// 
			this.buttonSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonSave.Location = new System.Drawing.Point(542, 11);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(75, 23);
			this.buttonSave.TabIndex = 43;
			this.buttonSave.Text = "Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// buttonMakeOrgBackups
			// 
			this.buttonMakeOrgBackups.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonMakeOrgBackups.Location = new System.Drawing.Point(446, 311);
			this.buttonMakeOrgBackups.Name = "buttonMakeOrgBackups";
			this.buttonMakeOrgBackups.Size = new System.Drawing.Size(171, 54);
			this.buttonMakeOrgBackups.TabIndex = 44;
			this.buttonMakeOrgBackups.Text = "Make ORG Backups Of Data Files";
			this.buttonMakeOrgBackups.UseVisualStyleBackColor = true;
			this.buttonMakeOrgBackups.Click += new System.EventHandler(this.buttonMakeOrgBackups_Click);
			// 
			// labelStatus
			// 
			this.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelStatus.Location = new System.Drawing.Point(267, 11);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(269, 23);
			this.labelStatus.TabIndex = 45;
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonCopyEconomics
			// 
			this.buttonCopyEconomics.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCopyEconomics.Location = new System.Drawing.Point(446, 263);
			this.buttonCopyEconomics.Name = "buttonCopyEconomics";
			this.buttonCopyEconomics.Size = new System.Drawing.Size(171, 42);
			this.buttonCopyEconomics.TabIndex = 46;
			this.buttonCopyEconomics.Text = "Copy Economics";
			this.buttonCopyEconomics.UseVisualStyleBackColor = true;
			this.buttonCopyEconomics.Click += new System.EventHandler(this.buttonCopyEconomics_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(629, 378);
			this.Controls.Add(this.buttonCopyEconomics);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.buttonMakeOrgBackups);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.textBoxStormyDays);
			this.Controls.Add(this.textBoxSnowDays);
			this.Controls.Add(this.textBox90DegreeDays);
			this.Controls.Add(this.textBoxDecemberHumidity);
			this.Controls.Add(this.textBoxDecemberLow);
			this.Controls.Add(this.textBoxDecemberHigh);
			this.Controls.Add(this.textBoxSeptemberHumidity);
			this.Controls.Add(this.textBoxSeptemberLow);
			this.Controls.Add(this.textBoxSeptemberHigh);
			this.Controls.Add(this.labelStormyDays);
			this.Controls.Add(this.labelSnowDays);
			this.Controls.Add(this.label90DegreeDays);
			this.Controls.Add(this.labelDecemberHumidity);
			this.Controls.Add(this.labelDecemberLow);
			this.Controls.Add(this.labelDecemberHigh);
			this.Controls.Add(this.labelSeptemberHumidity);
			this.Controls.Add(this.labelSeptemberLow);
			this.Controls.Add(this.labelSeptemberHigh);
			this.Controls.Add(this.textBoxElevation);
			this.Controls.Add(this.textBoxLongitude);
			this.Controls.Add(this.textBoxLatitude);
			this.Controls.Add(this.comboBoxState);
			this.Controls.Add(this.comboBoxRegion);
			this.Controls.Add(this.textBoxEntComp);
			this.Controls.Add(this.textBoxPovertyLevel);
			this.Controls.Add(this.textBoxAvgIncome);
			this.Controls.Add(this.textBoxGrowthRate);
			this.Controls.Add(this.textBoxPopulation);
			this.Controls.Add(this.textBoxAbbreviation);
			this.Controls.Add(this.labelElevation);
			this.Controls.Add(this.labelLongitude);
			this.Controls.Add(this.labelLatitude);
			this.Controls.Add(this.labelState);
			this.Controls.Add(this.labelRegion);
			this.Controls.Add(this.labelEntComp);
			this.Controls.Add(this.labelPovertyLevel);
			this.Controls.Add(this.labelAvgIncome);
			this.Controls.Add(this.labelGrowthRate);
			this.Controls.Add(this.labelPopulation);
			this.Controls.Add(this.labelAbbrev);
			this.Controls.Add(this.labelName);
			this.Controls.Add(this.textBoxName);
			this.Controls.Add(this.comboBoxCity);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "City Editor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboBoxCity;
		private System.Windows.Forms.TextBox textBoxName;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label labelAbbrev;
		private System.Windows.Forms.Label labelPopulation;
		private System.Windows.Forms.Label labelGrowthRate;
		private System.Windows.Forms.Label labelAvgIncome;
		private System.Windows.Forms.Label labelPovertyLevel;
		private System.Windows.Forms.Label labelEntComp;
		private System.Windows.Forms.Label labelRegion;
		private System.Windows.Forms.Label labelState;
		private System.Windows.Forms.Label labelLatitude;
		private System.Windows.Forms.Label labelLongitude;
		private System.Windows.Forms.Label labelElevation;
		private System.Windows.Forms.TextBox textBoxAbbreviation;
		private System.Windows.Forms.TextBox textBoxPopulation;
		private System.Windows.Forms.TextBox textBoxGrowthRate;
		private System.Windows.Forms.TextBox textBoxAvgIncome;
		private System.Windows.Forms.TextBox textBoxPovertyLevel;
		private System.Windows.Forms.TextBox textBoxEntComp;
		private System.Windows.Forms.ComboBox comboBoxRegion;
		private System.Windows.Forms.ComboBox comboBoxState;
		private System.Windows.Forms.TextBox textBoxLatitude;
		private System.Windows.Forms.TextBox textBoxLongitude;
		private System.Windows.Forms.TextBox textBoxElevation;
		private System.Windows.Forms.Label labelSeptemberHigh;
		private System.Windows.Forms.Label labelSeptemberLow;
		private System.Windows.Forms.Label labelSeptemberHumidity;
		private System.Windows.Forms.Label labelDecemberHigh;
		private System.Windows.Forms.Label labelDecemberLow;
		private System.Windows.Forms.Label labelDecemberHumidity;
		private System.Windows.Forms.Label label90DegreeDays;
		private System.Windows.Forms.Label labelSnowDays;
		private System.Windows.Forms.Label labelStormyDays;
		private System.Windows.Forms.TextBox textBoxSeptemberHigh;
		private System.Windows.Forms.TextBox textBoxSeptemberLow;
		private System.Windows.Forms.TextBox textBoxSeptemberHumidity;
		private System.Windows.Forms.TextBox textBoxDecemberHigh;
		private System.Windows.Forms.TextBox textBoxDecemberLow;
		private System.Windows.Forms.TextBox textBoxDecemberHumidity;
		private System.Windows.Forms.TextBox textBox90DegreeDays;
		private System.Windows.Forms.TextBox textBoxSnowDays;
		private System.Windows.Forms.TextBox textBoxStormyDays;
		private System.Windows.Forms.Button buttonSave;
		private System.Windows.Forms.Button buttonMakeOrgBackups;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.Button buttonCopyEconomics;
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Statistics.Analysis;

namespace DraftTrainerML
{
	class TrainData
	{
		System.Data.DataTable InputData;

		public double[][] InputArrays;
		public double[] PeakCurOutputs;
		public int[] AbilityOutputs;
		public int[] DraftableOutputs;

		//public double[][] AbilityNeuralOutputs;
		//public double[][] DraftableNeuralOutputs;
		//public double[][] PeakCurNeuralOutputs;

		PrincipalComponentAnalysis PCA;

		Dictionary<string, int> ColumnIndexMap = new Dictionary<string, int>();

		public TrainData()
		{
		}

		public void Load(string trainFile)
		{
			using (GenericParsing.GenericParserAdapter parser = new GenericParsing.GenericParserAdapter())
			{
				parser.SetDataSource(System.IO.Path.Combine("TrainData",trainFile));

				parser.ColumnDelimiter = ',';
				parser.FirstRowHasHeader = true;
				parser.TextQualifier = '\"';

				InputData = parser.GetDataTable();

				// source column names
				string[] columnNames;

				// Creates a matrix from the entire source data table
				double[,] table = InputData.ToMatrix(out columnNames);

				for (int i = 0; i < columnNames.Length;++i )
				{
					ColumnIndexMap.Add(columnNames[i], i);
				}

				int posStart = trainFile.IndexOf('_') + 1;
				string posGroup = trainFile.Substring(posStart, trainFile.Length - 4 - posStart);

				// Get only the input vector values (first two columns)
				if (posGroup == "C")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["BenchZ"]
						,ColumnIndexMap["BrdJZ"]
						,ColumnIndexMap["AgilZ"]
						,ColumnIndexMap["DashZ"]
						, ColumnIndexMap["RawGrdZ"]
						, ColumnIndexMap["Run_BlockingZ"]
						, ColumnIndexMap["Pass_BlockingZ"]
						, ColumnIndexMap["Blocking_StrengthZ"]
						, ColumnIndexMap["EnduranceZ"]
						).ToArray();
				}
				else if (posGroup == "T" || posGroup == "G")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["BenchZ"]
						, ColumnIndexMap["BrdJZ"]
						, ColumnIndexMap["AgilZ"]
						, ColumnIndexMap["DashZ"]
						, ColumnIndexMap["RawGrdZ"]
						, ColumnIndexMap["Run_BlockingZ"]
						, ColumnIndexMap["Pass_BlockingZ"]
						, ColumnIndexMap["Blocking_StrengthZ"]
						, ColumnIndexMap["EnduranceZ"]
						).ToArray();
				}
				else if (posGroup == "TE")
				{
					InputArrays = table.GetColumns(
						//ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Run_BlockingZ"],
						ColumnIndexMap["Pass_BlockingZ"],
						ColumnIndexMap["Blocking_StrengthZ"],
						ColumnIndexMap["Avoid_DropsZ"],
						//ColumnIndexMap["Get_DownfieldZ"],
						ColumnIndexMap["Route_RunningZ"],
						ColumnIndexMap["Third_Down_ReceivingZ"],
						ColumnIndexMap["Big_Play_ReceivingZ"],
						//ColumnIndexMap["CourageZ"],
						//ColumnIndexMap["Adjust_to_BallZ"],
						ColumnIndexMap["EnduranceZ"]
						//ColumnIndexMap["Special_TeamsZ"]
						//ColumnIndexMap["Long_SnappingZ"]
						).ToArray();
				}
				else if (posGroup == "S" || posGroup == "CB")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Run_DefenseZ"],
						ColumnIndexMap["Man_to_Man_DefenseZ"],
						ColumnIndexMap["Zone_DefenseZ"],
						ColumnIndexMap["Bump_and_Run_DefenseZ"],
						ColumnIndexMap["Play_DiagnosisZ"],
						ColumnIndexMap["Punishing_HitterZ"],
						ColumnIndexMap["InterceptingZ"],
						ColumnIndexMap["Punt_ReturnsZ"],
						ColumnIndexMap["Kick_ReturnsZ"],
						ColumnIndexMap["EnduranceZ"],
						ColumnIndexMap["Special_TeamsZ"]
						).ToArray();
				}
				else if (posGroup == "WR")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Avoid_DropsZ"],
						ColumnIndexMap["Get_DownfieldZ"],
						ColumnIndexMap["Route_RunningZ"],
						ColumnIndexMap["Third_Down_ReceivingZ"],
						ColumnIndexMap["Big_Play_ReceivingZ"],
						ColumnIndexMap["CourageZ"],
						ColumnIndexMap["Adjust_to_BallZ"],
						ColumnIndexMap["Punt_ReturnsZ"],
						ColumnIndexMap["Kick_ReturnsZ"],
						ColumnIndexMap["EnduranceZ"],
						ColumnIndexMap["Special_TeamsZ"]
						).ToArray();
				}
				else if (posGroup == "DE" || posGroup == "DT")
				{
					InputArrays = table.GetColumns(
						//ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Run_DefenseZ"],
						ColumnIndexMap["Pass_Rush_TechniqueZ"],
						ColumnIndexMap["Pass_Rush_StrengthZ"],
						ColumnIndexMap["Play_DiagnosisZ"],
						ColumnIndexMap["Punishing_HitterZ"],
						ColumnIndexMap["EnduranceZ"]
						).ToArray();
				}
				else if (posGroup == "FB")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Run_BlockingZ"],
						ColumnIndexMap["Pass_BlockingZ"],
						ColumnIndexMap["Blocking_StrengthZ"],
						ColumnIndexMap["Power_InsideZ"],
						ColumnIndexMap["Third_Down_RunsZ"],
						ColumnIndexMap["Hole_RecognitionZ"],
						ColumnIndexMap["Blitz_PickupZ"],
						ColumnIndexMap["Avoid_DropsZ"],
						ColumnIndexMap["Route_RunningZ"],
						ColumnIndexMap["Third_Down_ReceivingZ"],
						ColumnIndexMap["EnduranceZ"],
						ColumnIndexMap["Special_TeamsZ"]
						).ToArray();
				}
				else if (posGroup == "ILB" || posGroup == "OLB")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Run_DefenseZ"],
						ColumnIndexMap["Pass_Rush_TechniqueZ"],
						ColumnIndexMap["Pass_Rush_StrengthZ"],
						ColumnIndexMap["Man_to_Man_DefenseZ"],
						ColumnIndexMap["Zone_DefenseZ"],
						ColumnIndexMap["Bump_and_Run_DefenseZ"],
						ColumnIndexMap["Play_DiagnosisZ"],
						ColumnIndexMap["Punishing_HitterZ"],
						ColumnIndexMap["EnduranceZ"],
						ColumnIndexMap["Special_TeamsZ"]
						).ToArray();
				}
				else if (posGroup == "K")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Kicking_AccuracyZ"],
						ColumnIndexMap["Kicking_PowerZ"],
						ColumnIndexMap["Kickoff_DistanceZ"],
						ColumnIndexMap["Kickoff_Hang_TimeZ"]
						).ToArray();
				}
				else if (posGroup == "P")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Punting_PowerZ"],
						ColumnIndexMap["Hang_TimeZ"],
						ColumnIndexMap["Directional_PuntingZ"],
						ColumnIndexMap["Kick_HoldingZ"]
						).ToArray();
				}
				else if (posGroup == "QB")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Screen_PassesZ"],
						ColumnIndexMap["Short_PassesZ"],
						ColumnIndexMap["Medium_PassesZ"],
						ColumnIndexMap["Long_PassesZ"],
						ColumnIndexMap["Deep_PassesZ"],
						ColumnIndexMap["Third_Down_PassesZ"],
						ColumnIndexMap["AccuracyZ"],
						ColumnIndexMap["TimingZ"],
						ColumnIndexMap["Sense_RushZ"],
						ColumnIndexMap["Read_DefenseZ"],
						ColumnIndexMap["Two_Minute_OffenseZ"],
						ColumnIndexMap["Run_FrequencyZ"],
						ColumnIndexMap["Kick_HoldingZ"]
						).ToArray();
				}
				else if (posGroup == "RB")
				{
					InputArrays = table.GetColumns(
						ColumnIndexMap["SoleZ"],
						ColumnIndexMap["BenchZ"],
						ColumnIndexMap["BrdJZ"],
						ColumnIndexMap["AgilZ"],
						ColumnIndexMap["DashZ"],
						ColumnIndexMap["PosSpecZ"],
						//ColumnIndexMap["DevlZ"],
						ColumnIndexMap["RawGrdZ"],
						ColumnIndexMap["Breakaway_SpeedZ"],
						ColumnIndexMap["Power_InsideZ"],
						ColumnIndexMap["Third_Down_RunsZ"],
						ColumnIndexMap["Hole_RecognitionZ"],
						ColumnIndexMap["ElusivenessZ"],
						ColumnIndexMap["Speed_OutsideZ"],
						ColumnIndexMap["Blitz_PickupZ"],
						ColumnIndexMap["Avoid_DropsZ"],
						ColumnIndexMap["Get_DownfieldZ"],
						ColumnIndexMap["Route_RunningZ"],
						ColumnIndexMap["Third_Down_ReceivingZ"],
						ColumnIndexMap["Punt_ReturnsZ"],
						ColumnIndexMap["Kick_ReturnsZ"],
						ColumnIndexMap["EnduranceZ"],
						ColumnIndexMap["Special_TeamsZ"]
						).ToArray();
				}
				// Get only the output labels (last column)
				PeakCurOutputs = table.GetColumn(ColumnIndexMap["PeakCurZ"]);
				AbilityOutputs = table.GetColumn(ColumnIndexMap["Ability"]).ToInt32();
				DraftableOutputs = table.GetColumn(ColumnIndexMap["Draftable"]).ToInt32();

				// Not enough Excellent, so lump in with Very Good for better training / analysis.
				// Lump Fair/Poor together as well.
				for (int i=0;i<AbilityOutputs.Length;++i)
				{
					if (AbilityOutputs[i] > 0)
					{
						AbilityOutputs[i] -= 1;
					}
					if (AbilityOutputs[i] > 3)
					{
						AbilityOutputs[i] = 3;
					}
				}

				//AbilityNeuralOutputs = new double[InputArrays.Length][];
				//DraftableNeuralOutputs = new double[InputArrays.Length][];
				//PeakCurNeuralOutputs = new double[InputArrays.Length][];

				//for (int i = 0; i < InputArrays.Length; ++i)
				//{
				//	AbilityNeuralOutputs[i] = new double[5] { 0.0, 0.0, 0.0, 0.0, 0.0 };
				//	DraftableNeuralOutputs[i] = new double[2] { 0.0, 0.0 };
				//	PeakCurNeuralOutputs[i] = new double[1] { PeakCurOutputs[i] };
				//	int draftable = (int)DraftableOutputs[i];
				//	int ability = (int)AbilityOutputs[i];
				//	AbilityNeuralOutputs[i][ability] = 1.0;
				//	DraftableNeuralOutputs[i][draftable] = 1.0;
				//}
			}
		}

		public void PrincipalComponentAnalysis(TrainData inputData)
		{
			AbilityOutputs = inputData.AbilityOutputs;
			DraftableOutputs = inputData.DraftableOutputs;
			PeakCurOutputs = inputData.PeakCurOutputs;
			ColumnIndexMap = inputData.ColumnIndexMap;

			// Creates the Principal Component Analysis of the given source
			PCA = new PrincipalComponentAnalysis(inputData.InputArrays, AnalysisMethod.Center);

			// Compute the Principal Component Analysis
			PCA.Compute();

			// Creates a projection considering 80% of the information
			int dimensionCount = PCA.GetNumberOfComponents(0.99f);
			InputArrays = PCA.Transform(inputData.InputArrays, dimensionCount);
		}

		public void ApplyPrincipalComponentAnalysis(TrainData originalData, TrainData pcaData)
		{
			PCA = pcaData.PCA;

			AbilityOutputs = originalData.AbilityOutputs;
			DraftableOutputs = originalData.DraftableOutputs;
			PeakCurOutputs = originalData.PeakCurOutputs;
			ColumnIndexMap = originalData.ColumnIndexMap;

			// Creates a projection considering 80% of the information
			int dimensionCount = PCA.GetNumberOfComponents(0.99f);
			InputArrays = PCA.Transform(originalData.InputArrays, dimensionCount);
		}
	}
}

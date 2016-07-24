using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using Accord.Neuro.Neurons;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Models.Regression.Linear;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace DraftTrainerML
{
	public partial class Form1 : Form
	{
		private TrainData m_TrainData;
		private TrainData m_TestData;

		private System.IO.FileStream mAbilityOutStream;
		private System.IO.FileStream mDraftableOutStream;

		delegate void AddStatusTextCallback(string text);
		delegate void WorkCompletedCallback();

		public Form1()
		{
			InitializeComponent();

			textBoxTestError.Text = "";

			string outputPath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftTrainerML");
			if (!System.IO.Directory.Exists(outputPath))
			{
				System.IO.Directory.CreateDirectory(outputPath);
			}

			System.Threading.Thread updateThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.DoLearning));
			updateThread.IsBackground = true;
			updateThread.Start();
		}

		private void DoLearning()
		{
			string savePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftTrainerML", "Ability.ml");
			mAbilityOutStream = new System.IO.FileStream(savePath, System.IO.FileMode.Create);

			savePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftTrainerML", "Draftable.ml");
			mDraftableOutStream = new System.IO.FileStream(savePath, System.IO.FileMode.Create);

			//LoadTrainingData("QB");
			//RunLogisticRegressionDraftable("QB");
			//RunLogisticRegressionAbility("QB");
			//LoadTrainingData("RB");
			//RunLogisticRegressionDraftable("RB");
			//RunLogisticRegressionAbility("RB");
			//LoadTrainingData("FB");
			//RunLogisticRegressionDraftable("FB");
			//RunLogisticRegressionAbility("FB");
			LoadTrainingData("TE");
			RunLogisticRegressionDraftable("TE");
			RunLogisticRegressionAbility("TE");
			//LoadTrainingData("WR");
			//RunLogisticRegressionDraftable("WR");
			//RunLogisticRegressionAbility("WR");
			//LoadTrainingData("C");
			//RunLogisticRegressionDraftable("C");
			//RunLogisticRegressionAbility("C");
			//LoadTrainingData("G");
			//RunLogisticRegressionDraftable("G");
			//RunLogisticRegressionAbility("G");
			//LoadTrainingData("T");
			//RunLogisticRegressionDraftable("T");
			//RunLogisticRegressionAbility("T");
			LoadTrainingData("DT");
			RunLogisticRegressionDraftable("DT");
			RunLogisticRegressionAbility("DT");
			LoadTrainingData("DE");
			RunLogisticRegressionDraftable("DE");
			RunLogisticRegressionAbility("DE");
			//LoadTrainingData("ILB");
			//RunLogisticRegressionDraftable("ILB");
			//RunLogisticRegressionAbility("ILB");
			//LoadTrainingData("OLB");
			//RunLogisticRegressionDraftable("OLB");
			//RunLogisticRegressionAbility("OLB");
			//LoadTrainingData("CB");
			//RunLogisticRegressionDraftable("CB");
			//RunLogisticRegressionAbility("CB");
			//LoadTrainingData("S");
			//RunLogisticRegressionDraftable("S");
			//RunLogisticRegressionAbility("S");
			//LoadTrainingData("P");
			//RunLogisticRegressionDraftable("P");
			//RunLogisticRegressionAbility("P");
			//LoadTrainingData("K");
			//RunLogisticRegressionDraftable("K");
			//RunLogisticRegressionAbility("K");

			//LoadTrainingData("RB");
			//RunNeuralNetAbility("RB");
			//RunDecisionTreeAbility("RB");

			//LoadTrainingData("C");
			//RunLinearRegressionPeakCur("C");

			mAbilityOutStream.Close();
			mDraftableOutStream.Close();

			WorkCompleted();
		}

		private void AddStatusString(string newText)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.textBoxTestError.InvokeRequired)
			{
				AddStatusTextCallback d = new AddStatusTextCallback(AddStatusString);
				this.Invoke(d, new object[] { newText });
			}
			else
			{
				textBoxTestError.Text += newText;
				Refresh();
			}
		}

		private void WorkCompleted()
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.InvokeRequired)
			{
				WorkCompletedCallback d = new WorkCompletedCallback(WorkCompleted);
				this.Invoke(d);
			}
			else
			{
				AddStatusString(System.Environment.NewLine + System.Environment.NewLine + "Done!");
			}
		}

		private void LoadTrainingData(string posGroup)
		{
			m_TrainData = new TrainData();
			m_TrainData.Load("Train_" + posGroup + ".csv");
			m_TestData = new TrainData();
			m_TestData.Load("Test_" + posGroup + ".csv");
		}

		private void RunLogisticRegressionAbility(string posGroup)
		{
			AddStatusString("Logistic Regression Ability " + posGroup + System.Environment.NewLine + System.Environment.NewLine);

			int categories = 4;
			MultinomialLogisticRegression regression = new MultinomialLogisticRegression(inputs: m_TrainData.InputArrays[0].Length, categories: categories);

			// Next, we are going to estimate this model. For this, we
			// will use the Iteratively Reweighted Least Squares method.
			var teacher = new LowerBoundNewtonRaphson(regression);

			// Now, we will iteratively estimate our model. The Run method returns
			// the maximum relative change in the model parameters and we will use
			// it as the convergence criteria.
			int iterationCount = 0;
			double delta = 1000;
			do
			{
				// Perform an iteration
				delta = teacher.Run(m_TrainData.InputArrays, m_TrainData.AbilityOutputs);
				iterationCount++;

				if (iterationCount % 500 == 0)
				{
					AddStatusString("Iterations: " + iterationCount + " Delta:" + delta + System.Environment.NewLine);
				}
			} while (delta > 0.0001);

			AddStatusString("Iterations: " + iterationCount + " Delta:" + delta + System.Environment.NewLine);

			// Check against the test set
			int totalTrainingSet = m_TestData.InputArrays.Length;
			int totalCorrect = 0;
			int offBy1 = 0;
			int offByMore = 0;
			int[] classCorrect = { 0, 0, 0, 0 };
			int[] classCount = { 0, 0, 0, 0 };
			int[] classOffBy1 = { 0, 0, 0, 0 };

			for (int i = 0; i < totalTrainingSet; ++i)
			{
				double[] results = regression.Compute(m_TestData.InputArrays[i]);
				int decisionClass = 0;
				double highestVal = results[0];
				for (int r = 1; r < results.Length;++r )
				{
					if (results[r] > highestVal)
					{
						decisionClass = r;
						highestVal = results[r];
					}
				}
				int actualClass = m_TestData.AbilityOutputs[i];
				classCount[actualClass] += 1;
				int diff = Math.Abs(decisionClass - actualClass);
				if (diff == 0)
				{
					totalCorrect += 1;
					classCorrect[actualClass] += 1;
				}
				else if (diff == 1)
				{
					offBy1 += 1;
					classOffBy1[actualClass] += 1;
				}
				else
				{
					offByMore += 1;
				}
			}

			double correctPercent = (double)totalCorrect / (double)totalTrainingSet;
			double offBy1Percent = (double)offBy1 / (double)totalTrainingSet;
			double offByMorePercent = (double)offByMore / (double)totalTrainingSet;
			AddStatusString("Test Size = " + totalTrainingSet + System.Environment.NewLine);
			AddStatusString("Total Correct = " + totalCorrect + " (" + correctPercent.ToString("P") + ")" + System.Environment.NewLine);
			AddStatusString("Off By 1 = " + offBy1 + " (" + offBy1Percent.ToString("P") + ")" + System.Environment.NewLine);
			AddStatusString("Off By More = " + offByMore + " (" + offByMorePercent.ToString("P") + ")" + System.Environment.NewLine);
			for (int i = 0; i < classCorrect.Length; ++i)
			{
				AddStatusString("Class " + i + ": size: " + classCount[i] + " correct: " + classCorrect[i] + " off by 1: " + classOffBy1[i] + System.Environment.NewLine);
			}
			AddStatusString(System.Environment.NewLine + System.Environment.NewLine);

			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			formatter.Serialize(mAbilityOutStream, posGroup);
			formatter.Serialize(mAbilityOutStream, regression.Coefficients);
		}

		private void RunLogisticRegressionDraftable(string posGroup)
		{
			AddStatusString("Logistic Regression Draftable " + posGroup + System.Environment.NewLine + System.Environment.NewLine);

			LogisticRegression regression = new LogisticRegression(inputs: m_TrainData.InputArrays[0].Length);

			// Next, we are going to estimate this model. For this, we
			// will use the Iteratively Reweighted Least Squares method.
			var teacher = new IterativeReweightedLeastSquares(regression);

			// Now, we will iteratively estimate our model. The Run method returns
			// the maximum relative change in the model parameters and we will use
			// it as the convergence criteria.
			int iterationCount = 0;
			double delta = 0;
			do
			{
				// Perform an iteration
				delta = teacher.Run(m_TrainData.InputArrays, m_TrainData.DraftableOutputs);
				iterationCount++;
			} while (delta > 0.00001);

			AddStatusString("Iterations: " + iterationCount + System.Environment.NewLine);

			// Check against the test set
			int totalTrainingSet = m_TestData.InputArrays.Length;
			int correctPositives = 0;
			int correctNegatives = 0;
			int falsePositives = 0;
			int falseNegatives = 0;
			int trainingPositives = 0;
			int trainingNegatives = 0;
			int highScoreCorrect = 0;
			int highScoreCount = 0;
			int medScoreCorrect = 0;
			int medScoreCount = 0;
			int lowScoreCorrect = 0;
			int lowScoreCount = 0;
			for (int i = 0; i < totalTrainingSet; ++i)
			{
				double draftable = regression.Compute(m_TestData.InputArrays[i]);
				bool predictDraftable = (draftable >= 0.5);
				bool isDraftable = (m_TestData.DraftableOutputs[i] == 1);
				if (isDraftable)
				{
					trainingPositives += 1;
				}
				else
				{
					trainingNegatives += 1;
				}
				if (predictDraftable && isDraftable)
				{
					correctPositives += 1;
				}
				else if (!predictDraftable && !isDraftable)
				{
					correctNegatives += 1;
				}
				else if (predictDraftable)
				{
					falsePositives += 1;
				}
				else
				{
					falseNegatives += 1;
				}

				if (draftable >= 0.9)
				{
					highScoreCount += 1;
					if (isDraftable)
					{
						highScoreCorrect += 1;
					}
				}
				if (draftable >= 0.65)
				{
					medScoreCount += 1;
					if (isDraftable)
					{
						medScoreCorrect += 1;
					}
				}
				if (draftable <= 0.2)
				{
					lowScoreCount += 1;
					if (!isDraftable)
					{
						lowScoreCorrect += 1;
					}
				}
			}

			int totalCorrect = correctPositives + correctNegatives;
			double correctPercent = (double)totalCorrect / (double)totalTrainingSet;
			double falsePositivePercent = (double)falsePositives / (double)trainingNegatives;
			double falseNegativePercent = (double)falseNegatives / (double)trainingPositives;
			double correctPositivePercent = (double)correctPositives / (double)trainingPositives;
			double correctNegativePercent = (double)correctNegatives / (double)trainingNegatives;
			double highScorePercent = (double)highScoreCorrect / (double)highScoreCount;
			double medScorePercent = (double)medScoreCorrect / (double)medScoreCount;
			double lowScorePercent = (double)lowScoreCorrect / (double)lowScoreCount;
			AddStatusString("Test Size = " + totalTrainingSet + System.Environment.NewLine);
			AddStatusString("Total Correct = " + totalCorrect + " (" + correctPercent.ToString("P") + ")" + System.Environment.NewLine);
			AddStatusString("Correct Positives = " + correctPositives + " (" + correctPositivePercent.ToString("P") + ") of " + trainingPositives + System.Environment.NewLine);
			AddStatusString("Correct Negatives = " + correctNegatives + " (" + correctNegativePercent.ToString("P") + ") of " + trainingNegatives + System.Environment.NewLine);
			AddStatusString("False Positives = " + falsePositives + " (" + falsePositivePercent.ToString("P") + ") of " + trainingNegatives + System.Environment.NewLine);
			AddStatusString("False Negatives = " + falseNegatives + " (" + falseNegativePercent.ToString("P") + ") of " + trainingPositives + System.Environment.NewLine);
			AddStatusString("High Score Correct = " + highScoreCorrect + " (" + highScorePercent.ToString("P") + ") of " + highScoreCount + System.Environment.NewLine);
			AddStatusString("Med Score Correct = " + medScoreCorrect + " (" + medScorePercent.ToString("P") + ") of " + medScoreCount + System.Environment.NewLine);
			AddStatusString("Low Score Correct = " + lowScoreCorrect + " (" + lowScorePercent.ToString("P") + ") of " + lowScoreCount + System.Environment.NewLine);
			AddStatusString(System.Environment.NewLine + System.Environment.NewLine);

			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			formatter.Serialize(mDraftableOutStream, posGroup);
			formatter.Serialize(mDraftableOutStream, regression.Coefficients);
		}

		//private void RunNeuralNetAbility(string posGroup)
		//{
		//	AddStatusString("Neural Net Ability " + posGroup + System.Environment.NewLine + System.Environment.NewLine);

		//	int inputCount = m_TrainData.InputArrays[0].Length;
		//	int outputCount = m_TrainData.AbilityNeuralOutputs[0].Length;

		//	// create neural network
		//	int[] hiddenNeurons = { inputCount * 2, inputCount * 2, outputCount };

		//	DeepBeliefNetwork network = DeepBeliefNetwork.CreateGaussianBernoulli(inputCount, hiddenNeurons);

		//	// create teacher
		//	BackPropagationLearning teacher = new BackPropagationLearning(network)
		//	{
		//		LearningRate = 0.5,
		//		Momentum = 0.9
		//	};
		//	// loop
		//	for (int i = 0; i <= 30000; ++i)
		//	{
		//		double error = teacher.RunEpoch(m_TrainData.InputArrays, m_TrainData.AbilityNeuralOutputs);
		//		if (i > 0 && i % 100 == 0)
		//		{
		//			AddStatusString("Iterations: " + i + " Error: " + error.ToString("F4") + System.Environment.NewLine);
		//		}
		//	}

		//	// Check against the test set
		//	int totalTrainingSet = m_TestData.InputArrays.Length;
		//	int totalCorrect = 0;
		//	int offBy1 = 0;
		//	int offByMore = 0;
		//	int[] classCorrect = { 0, 0, 0, 0, 0 };
		//	int[] classCount = { 0, 0, 0, 0, 0 };
		//	int[] classOffBy1 = { 0, 0, 0, 0, 0 };

		//	for (int i = 0; i < totalTrainingSet; ++i)
		//	{
		//		double[] results = network.Compute(m_TestData.InputArrays[i]);
		//		int decisionClass = 0;
		//		double highestVal = results[0];
		//		for (int r = 1; r < results.Length; ++r)
		//		{
		//			if (results[r] > highestVal)
		//			{
		//				decisionClass = r;
		//				highestVal = results[r];
		//			}
		//		}
		//		int actualClass = m_TestData.AbilityOutputs[i];
		//		classCount[actualClass] += 1;
		//		int diff = Math.Abs(decisionClass - actualClass);
		//		if (diff == 0)
		//		{
		//			totalCorrect += 1;
		//			classCorrect[actualClass] += 1;
		//		}
		//		else if (diff == 1)
		//		{
		//			offBy1 += 1;
		//			classOffBy1[actualClass] += 1;
		//		}
		//		else
		//		{
		//			offByMore += 1;
		//		}
		//	}

		//	double correctPercent = (double)totalCorrect / (double)totalTrainingSet;
		//	double offBy1Percent = (double)offBy1 / (double)totalTrainingSet;
		//	double offByMorePercent = (double)offByMore / (double)totalTrainingSet;
		//	AddStatusString("Test Size = " + totalTrainingSet + System.Environment.NewLine);
		//	AddStatusString("Total Correct = " + totalCorrect + " (" + correctPercent.ToString("P") + ")" + System.Environment.NewLine);
		//	AddStatusString("Off By 1 = " + offBy1 + " (" + offBy1Percent.ToString("P") + ")" + System.Environment.NewLine);
		//	AddStatusString("Off By More = " + offByMore + " (" + offByMorePercent.ToString("P") + ")" + System.Environment.NewLine);
		//	for (int i = 0; i < classCorrect.Length; ++i)
		//	{
		//		AddStatusString("Class " + i + ": size: " + classCount[i] + " correct: " + classCorrect[i] + " off by 1: " + classOffBy1[i] + System.Environment.NewLine);
		//	}
		//	AddStatusString(System.Environment.NewLine + System.Environment.NewLine);

		//	string savePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftTrainerML", posGroup + "_ability.ml");
		//	network.Save(savePath);
		//}

		private void RunLinearRegressionPeakCur(string posGroup)
		{
			AddStatusString("Linear Regression PeakCur " + posGroup + System.Environment.NewLine + System.Environment.NewLine);

			int inputCount = m_TrainData.InputArrays[0].Length;

			// Create a multiple linear regression for two input and an intercept
			MultipleLinearRegression target = new MultipleLinearRegression(inputCount, true);

			double error = target.Regress(m_TrainData.InputArrays, m_TrainData.PeakCurOutputs);
			AddStatusString("Error = " + error + System.Environment.NewLine);

			// Check against the test set
			int totalTrainingSet = m_TestData.InputArrays.Length;
			int totalDraftable = 0;
			double[] diffs = { 0.2, 0.1, 0.05 };
			int[] totals = { 0, 0, 0, 0, 0, 0 };

			for (int trainingIndex = 0; trainingIndex < totalTrainingSet; ++trainingIndex)
			{
				double result = target.Compute(m_TestData.InputArrays[trainingIndex]);
				double diff = Math.Abs(result-m_TestData.PeakCurOutputs[trainingIndex]);
				for (int diffIndex = 0; diffIndex < diffs.Length; ++diffIndex)
				{
					if (m_TestData.DraftableOutputs[trainingIndex] == 1)
					{
						totalDraftable += 1;
					}
					if (diff < diffs[diffIndex])
					{
						totals[diffIndex] += 1;
						if (m_TestData.DraftableOutputs[trainingIndex] == 1)
						{
							totals[diffIndex + 3] += 1;
						}
					}
				}
			}

			AddStatusString("Test Size = " + totalTrainingSet + System.Environment.NewLine);
			for (int diffIndex = 0; diffIndex < diffs.Length; ++diffIndex)
			{
				double percent = (double)totals[diffIndex] / (double)totalTrainingSet;
				AddStatusString("Total Within " + (int)(diffs[diffIndex] * 100.0) + " = " + totals[diffIndex] + " (" + percent.ToString("P") + ")" + System.Environment.NewLine);
			}
			AddStatusString("Draftable = " + totalDraftable + System.Environment.NewLine);
			for (int diffIndex = 0; diffIndex < diffs.Length; ++diffIndex)
			{
				double percent = (double)totals[diffIndex+3] / (double)totalDraftable;
				AddStatusString("Total Within " + (int)(diffs[diffIndex] * 100.0) + " = " + totals[diffIndex+3] + " (" + percent.ToString("P") + ")" + System.Environment.NewLine);
			}

			AddStatusString(System.Environment.NewLine + System.Environment.NewLine);

			string savePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftTrainerML", posGroup + "_peakCur.ml");
			Encoding windows1252Encoding = Encoding.GetEncoding(1252);
			using (System.IO.FileStream outStream = new System.IO.FileStream(savePath, System.IO.FileMode.Create))
			{
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				formatter.Serialize(outStream, target.Coefficients);
				outStream.Close();
			}
		}

		private void RunDecisionTreeAbility(string posGroup)
		{
			AddStatusString("Decision Tree " + posGroup + System.Environment.NewLine + System.Environment.NewLine);

			// Specify the input variables
			DecisionVariable[] variables = new DecisionVariable[m_TrainData.InputArrays[0].Length];
			for (int i = 0; i < variables.Length; ++i)
			{
				variables[i] = new DecisionVariable("input_" + i.ToString(), DecisionVariableKind.Continuous);
			}

			// Create the discrete Decision tree
			DecisionTree tree = new DecisionTree(variables, 4);

			// Create the C4.5 learning algorithm
			C45Learning c45 = new C45Learning(tree);
			c45.MaxHeight = 20;

			// Learn the decision tree using C4.5
			double error = c45.Run(m_TrainData.InputArrays, m_TrainData.AbilityOutputs);
			AddStatusString("Training error: " + error.ToString("P") + System.Environment.NewLine);

			//// Show the learned tree in the view
			//if (variables.Length == 10)
			//{
			//	DisplayDecisionTree(tree);
			//}

			// Check against the test set
			int totalTrainingSet = m_TestData.InputArrays.Length;
			int totalCorrect = 0;
			int offBy1 = 0;
			int offByMore = 0;
			int[] classCorrect = { 0, 0, 0, 0 };
			int[] classCount = { 0, 0, 0, 0 };
			int[] classOffBy1 = { 0, 0, 0, 0 };

			for (int i = 0; i < totalTrainingSet; ++i)
			{
				int decisionClass = tree.Compute(m_TestData.InputArrays[i]);
				int actualClass = m_TestData.AbilityOutputs[i];
				classCount[actualClass] += 1;
				int diff = Math.Abs(decisionClass - actualClass);
				if (diff == 0)
				{
					totalCorrect += 1;
					classCorrect[actualClass] += 1;
				}
				else if (diff == 1)
				{
					offBy1 += 1;
					classOffBy1[actualClass] += 1;
				}
				else
				{
					offByMore += 1;
				}
			}

			double correctPercent = (double)totalCorrect / (double)totalTrainingSet;
			double offBy1Percent = (double)offBy1 / (double)totalTrainingSet;
			double offByMorePercent = (double)offByMore / (double)totalTrainingSet;
			AddStatusString("Test Size = " + totalTrainingSet + System.Environment.NewLine);
			AddStatusString("Total Correct = " + totalCorrect + " (" + correctPercent.ToString("P") + ")" + System.Environment.NewLine);
			AddStatusString("Off By 1 = " + offBy1 + " (" + offBy1Percent.ToString("P") + ")" + System.Environment.NewLine);
			AddStatusString("Off By More = " + offByMore + " (" + offByMorePercent.ToString("P") + ")" + System.Environment.NewLine);
			for (int i = 0; i < classCorrect.Length; ++i)
			{
				AddStatusString("Class " + i + ": size: " + classCount[i] + " correct: " + classCorrect[i] + " off by 1: " + classOffBy1[i] + System.Environment.NewLine);
			}
			AddStatusString(System.Environment.NewLine + System.Environment.NewLine);

			string savePath = System.IO.Path.Combine(WindowsUtilities.OutputLocation.Get(), "DraftTrainerML", posGroup + "_ability.ml");
			tree.Save(savePath);
		}
	}
}

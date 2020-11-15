using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

using SharpLearning.RandomForest.Learners;

namespace PredictionModel
{
	class Program
	{
		public const string Symbol = "BTCUSDT";

		public const int Window = 5;

		private static List<TradeInformation> History = new List<TradeInformation>();

		private static bool LoadHistory()
		{
			if(Binance.GetTradeHistory(Symbol, DateTime.Now.AddDays(-60), DateTime.Now, out var history))
			{
				Binance.WriteTradeHistory("history.txt", history);

				History = history;

				return true;
			}
			else
			{
				Logger.Write("Can not Load Trade History");

				return false;
			}
		}

		private static bool IsLong(List<TradeInformation> history, int index, int delay, decimal percent)
		{
			decimal start = history[index].Average;

			decimal target = percent*start;

			for(int i=index+1; i<index+delay; ++i)
			{
				if(history[i].High >= target)
				{
					return true;
				}
			}

			return false;
		}
		
		private static bool IsShort(List<TradeInformation> history, int index, int delay, decimal percent)
		{
			decimal start = history[index].Average;

			decimal target = percent*start;

			for(int i=index+1; i<index+delay; ++i)
			{
				if(history[i].Low <= target)
				{
					return true;
				}
			}

			return false;
		}

		private static void GetLongPredictionModel(string path1, string path2, int delay, decimal percent)
		{
			if(!Binance.ReadTradeHistory(path1, out History))
			{
				Console.WriteLine("Can not Read Trade History");
				Console.ReadKey();
				return;
			}

			const int start = 1;

			int stop = History.Count - Window - delay;

			int count = stop - start;

			double[][] inputs = new double[count][];

			double[] outputs = new double[count];

			for(int i=0; i<count; ++i)
			{
				inputs[i] = new double[Window];

				int index = start+i;

				for(int j=0; j<Window; ++j)
				{
					inputs[i][j] = (double)(History[index+j].Average-History[index+j-1].Average);
				}

				if(IsLong(History, index, delay, percent))
				{
					outputs[i] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 32);

			var model = learner.Learn(inputs, outputs);

			model.Save(() => new StreamWriter(path2));
		}

		private static void TestLongPredictionModel(string path1, int test, int delay, decimal percent)
		{
			if(!Binance.ReadTradeHistory(path1, out History))
			{
				Console.WriteLine("Can not Read Trade History");
				Console.ReadKey();
				return;
			}

			int start = 1;

			int stop = History.Count - Window - delay;

			int count1 = stop - start - test;

			int count2 = test;

			if(count1 <= 0 || count2 <= 0)
			{
				throw new Exception();
			}

			double[][] inputs1 = new double[count1][];

			double[][] inputs2 = new double[count2][];

			double[] outputs1 = new double[count1];

			double[] outputs2 = new double[count2];

			for(int i=0; i<count1; ++i)
			{
				inputs1[i] = new double[Window];

				int index = start+i;

				for(int j=0; j<Window; ++j)
				{
					inputs1[i][j] = (double)(History[index+j].Average-History[index+j-1].Average);
				}

				if(IsLong(History, index, delay, percent))
				{
					outputs1[i] = 1.0;
				}
			}

			for(int i=count1; i<count1+count2; ++i)
			{
				inputs2[i-count1] = new double[Window];

				int index = start+i;

				for(int j=0; j<Window; ++j)
				{
					inputs2[i-count1][j] = (double)(History[index+j].Average-History[index+j-1].Average);
				}

				if(IsLong(History, index, delay, percent))
				{
					outputs2[i-count1] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 32);

			var model = learner.Learn(inputs1, outputs1);

			StringBuilder stringBuilder = new StringBuilder();

			for(int i=0; i<count2; ++i)
			{
				double prediction = model.Predict(inputs2[i]);
				double probability = model.PredictProbability(inputs2[i]).Prediction;

				stringBuilder.Append(prediction);
				stringBuilder.Append("\t");

				if(probability > 0.99)
				{
					stringBuilder.Append(outputs2[i]);
					stringBuilder.Append("\t");
				}
				else
				{
					stringBuilder.Append(0.0);
					stringBuilder.Append("\t");
				}

				stringBuilder.Append(Format(probability));
				stringBuilder.Append("\n");
			}

			File.WriteAllText("test.txt", stringBuilder.ToString());
		}

		private static void GetShortPredictionModel(string path1, string path2, int delay, decimal percent)
		{
			if(!Binance.ReadTradeHistory(path1, out History))
			{
				Console.WriteLine("Can not Read Trade History");
				Console.ReadKey();
				return;
			}

			const int start = 1;

			int stop = History.Count - Window - delay;

			int count = stop - start;

			double[][] inputs = new double[count][];

			double[] outputs = new double[count];

			for(int i=0; i<count; ++i)
			{
				inputs[i] = new double[Window];

				int index = start+i;

				for(int j=0; j<Window; ++j)
				{
					inputs[i][j] = (double)(History[index+j].Average-History[index+j-1].Average);
				}

				if(IsShort(History, index, delay, percent))
				{
					outputs[i] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 32);

			var model = learner.Learn(inputs, outputs);

			model.Save(() => new StreamWriter(path2));
		}
		
		private static void TestShortPredictionModel(string path1, int test, int delay, decimal percent)
		{
			if(!Binance.ReadTradeHistory(path1, out History))
			{
				Console.WriteLine("Can not Read Trade History");
				Console.ReadKey();
				return;
			}

			int start = 1;

			int stop = History.Count - Window - delay;

			int count1 = stop - start - test;

			int count2 = test;

			if(count1 <= 0 || count2 <= 0)
			{
				throw new Exception();
			}

			double[][] inputs1 = new double[count1][];

			double[][] inputs2 = new double[count2][];

			double[] outputs1 = new double[count1];

			double[] outputs2 = new double[count2];

			for(int i=0; i<count1; ++i)
			{
				inputs1[i] = new double[Window];

				int index = start+i;

				for(int j=0; j<Window; ++j)
				{
					inputs1[i][j] = (double)(History[index+j].Average-History[index+j-1].Average);
				}

				if(IsShort(History, index, delay, percent))
				{
					outputs1[i] = 1.0;
				}
			}

			for(int i=count1; i<count1+count2; ++i)
			{
				inputs2[i-count1] = new double[Window];

				int index = start+i;

				for(int j=0; j<Window; ++j)
				{
					inputs2[i-count1][j] = (double)(History[index+j].Average-History[index+j-1].Average);
				}

				if(IsShort(History, index, delay, percent))
				{
					outputs2[i-count1] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 32);

			var model = learner.Learn(inputs1, outputs1);

			StringBuilder stringBuilder = new StringBuilder();

			for(int i=0; i<count2; ++i)
			{
				double prediction = model.Predict(inputs2[i]);
				double probability = model.PredictProbability(inputs2[i]).Prediction;

				stringBuilder.Append(prediction);
				stringBuilder.Append("\t");

				if(probability > 0.99)
				{
					stringBuilder.Append(outputs2[i]);
					stringBuilder.Append("\t");
				}
				else
				{
					stringBuilder.Append(0.0);
					stringBuilder.Append("\t");
				}

				stringBuilder.Append(Format(probability));
				stringBuilder.Append("\n");
			}

			File.WriteAllText("test.txt", stringBuilder.ToString());
		}

		private static string Format(double value, int sign = 4)
		{
			sign = Math.Max(sign, 0);
			sign = Math.Min(sign, 8);

			return string.Format(CultureInfo.InvariantCulture, "{0:F" + sign + "}", value);
		}

		static void Main()
		{
			string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			
			directory += "\\PredictionModels\\";
			
			if(!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			//LoadHistory();

			//TestShortPredictionModel("history.txt", 2048, 10, 0.999m);
			
			GetShortPredictionModel("history.txt", directory + "short1.xml", 10, 0.999m);
		}
	}
}

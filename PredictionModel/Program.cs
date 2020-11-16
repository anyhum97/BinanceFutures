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

		public const int Count = 5;

		public const int Window = 30;

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
			
			decimal target = (1.0m + percent)*start;
			
			decimal lower = (1.0m - 0.5m*percent)*start;

			for(int i=index+1; i<index+1+delay; ++i)
			{
				if(0.5m*(history[i].Average + history[i].High) >= target)
				{
					return true;
				}

				if(0.5m*(history[i].Average + history[i].Low) <= lower)
				{
					return false;
				}
			}
			
			return false;
		}
		
		private static bool IsShort(List<TradeInformation> history, int index, int delay, decimal percent)
		{
			decimal start = history[index].Average;

			decimal target = (1.0m - percent)*start;
			
			decimal upper = (1.0m + 0.5m*percent)*start;

			for(int i=index+1; i<index+1+delay; ++i)
			{
				if(0.5m*(history[i].Average + history[i].Low) <= target)
				{
					return true;
				}
				
				if(0.5m*(history[i].Average + history[i].High) >= upper)
				{
					return false;
				}
			}

			return false;
		}

		private static decimal GetDelta(List<TradeInformation> history, int index, int window)
		{
			decimal delta = 0.0m;

			for(int i=index-window; i<index; ++i)
			{
				delta += history[i+1].Average - history[i].Average;
			}

			return delta;
		}

		private static void GetLongPredictionModel(string path1, string path2, int delay, decimal percent)
		{
			if(!Binance.ReadTradeHistory(path1, out History))
			{
				Console.WriteLine("Can not Read Trade History");
				Console.ReadKey();
				return;
			}

			if(File.Exists(path2))
			{
				File.Delete(path2);
			}

			const int start = 1;

			int stop = History.Count - Count - delay;

			int count = stop - start;

			double[][] inputs = new double[count][];

			double[] outputs = new double[count];

			for(int i=0; i<count; ++i)
			{
				inputs[i] = new double[Count];

				int index = start+i;

				for(int j=0; j<Count; ++j)
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

			int stop = History.Count - Count - delay;

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
				inputs1[i] = new double[Count];

				int index = start+i;

				for(int j=0; j<Count; ++j)
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
				inputs2[i-count1] = new double[Count];

				int index = start+i;

				for(int j=0; j<Count; ++j)
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

			if(File.Exists(path2))
			{
				File.Delete(path2);
			}

			const int start = 1;

			int stop = History.Count - Count - delay;

			int count = stop - start;

			double[][] inputs = new double[count][];

			double[] outputs = new double[count];

			for(int i=0; i<count; ++i)
			{
				inputs[i] = new double[Count];

				int index = start+i;

				for(int j=0; j<Count; ++j)
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

			int stop = History.Count - Count - delay;

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
				inputs1[i] = new double[Count];

				int index = start+i;

				for(int j=0; j<Count; ++j)
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
				inputs2[i-count1] = new double[Count];

				int index = start+i;

				for(int j=0; j<Count; ++j)
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
			//LoadHistory();

			//TestShortPredictionModel("history.txt", 2048, 10, 0.999m);
			
			decimal percent = 0.001m;
			
			for(int i=1; i<=10; ++i)
			{
				int delay = 5;

				if(i == 2)
				{
					delay = 10;
				}

				if(i == 3 || i == 4 || i == 5 || i == 6)
				{
					delay = 15;
				}

				if(i == 7 || i == 8 || i == 9)
				{
					delay = 20;
				}

				if(i == 10)
				{
					delay = 30;
				}

				//GetLongPredictionModel("history.txt", "long" + i + ".xml", delay, percent);

				GetShortPredictionModel("history.txt", "short" + i + ".xml", delay, percent);
				
				percent += 0.001m;
			}

		}
	}
}

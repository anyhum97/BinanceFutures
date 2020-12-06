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

		public const int Window = 60;

		public const int Count = 5;

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

			if(percent < 0.0m || percent > 0.01m)
			{
				throw new Exception();
			}

			decimal target = (1.0m+percent)*start;

			decimal border = (1.0m-0.30m*percent)*start;

			decimal max = decimal.MinValue;

			for(int i=index+1; i<index+delay+1; ++i)
			{
				decimal value = history[i].Average;

				if(value < border)
				{
					return false;
				}

				if(value > max)
				{
					max = value;
				}
			}

			if(max < target)
			{
				return false;
			}

			return true;
		}
		
		private static bool IsShort(List<TradeInformation> history, int index, int delay, decimal percent)
		{
			decimal start = history[index].Average;

			if(percent <= 0.0m || percent > 0.01m)
			{
				throw new Exception();
			}

			decimal target = (1.0m-percent)*start;

			decimal border = (1.0m+0.25m*percent)*start;

			decimal min = decimal.MaxValue;

			for(int i=index+1; i<index+delay+1; ++i)
			{
				decimal value = history[i].Average;

				if(value > border)
				{
					return false;
				}

				if(value < min)
				{
					min = value;
				}
			}

			if(min > target)
			{
				return false;
			}

			return true;
		}

		private static bool IsLong2(List<TradeInformation> history, int index, int delay, decimal percent)
		{
			decimal start = history[index].Average;

			for(int i=index+1; i<index+delay+1; ++i)
			{
				decimal value = history[i].Average;

				if(value < start)
				{
					return false;
				}
			}

			return true;
		}

		private static double GetDeltaPrice(List<TradeInformation> history, int index, int window)
		{
			decimal delta = 0.0m;

			for(int i=index-window; i<index; ++i)
			{
				delta += history[i+1].Average - history[i].Average;
			}

			return (double)delta;
		}
		
		private static double GetDeltaVolume(List<TradeInformation> history, int index, int window)
		{
			decimal delta = 0.0m;

			for(int i=index-window; i<index; ++i)
			{
				delta += history[i+1].Volume - history[i].Volume;
			}

			return (double)delta;
		}

		private static double GetSecondDerivativeDelta(List<TradeInformation> history, int index, int window)
		{
			decimal delta = 0.0m;

			for(int i=index-window; i<index; ++i)
			{
				delta += history[i-1].Average - 2.0m*history[i].Average + history[i+1].Average;
			}

			return (double)delta;
		}

		private static double GetSecondDerivative(List<TradeInformation> history, int index)
		{
			double secondDerivative = (double)(history[index-1].Average - 2.0m*history[index].Average + history[index+1].Average);
			
			return secondDerivative;
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

			int start = Window;

			int stop = History.Count - delay;

			int count = stop - start;

			double[][] inputs = new double[count][];

			double[] outputs = new double[count];

			for(int i=0; i<count; ++i)
			{
				int index = i+start;

				inputs[i] = new double[9]
				{
					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsLong2(History, index, delay, percent))
				{
					outputs[i] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 128, seed:11);

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

			int start = Window;

			int stop = History.Count - delay;

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
				int index = i+start;

				inputs1[i] = new double[11]
				{
					GetDeltaPrice(History, index, 20),					
					GetDeltaVolume(History, index, 11),

					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsLong(History, index, delay, percent))
				{
					outputs1[i] = 1.0;
				}
			}

			for(int i=count1; i<count1+count2; ++i)
			{
				int index = i;

				inputs2[i-count1] = new double[11]
				{
					GetDeltaPrice(History, index, 20),
					GetDeltaVolume(History, index, 11),

					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsLong(History, index, delay, percent))
				{
					outputs2[i-count1] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 64);

			var model = learner.Learn(inputs1, outputs1);

			StringBuilder stringBuilder = new StringBuilder();

			int score = 0;
			int good = 0;
			int all = 0;

			for(int i=1; i<count2-1; ++i)
			{
				double prediction = model.Predict(inputs2[i]);
				double probability = model.PredictProbability(inputs2[i]).Prediction;

				stringBuilder.Append(outputs2[i]);
				stringBuilder.Append("\t");

				if(probability > 0.99)
				{
					stringBuilder.Append(prediction);
					stringBuilder.Append("\t");

					if(prediction == 1.0)
					{
						if(outputs2[i] == 1.0)
						{
							score += 1;
							good += 1;
						}
						else
						{
							score -= 1;
						}

						++all;
					}
				}
				else
				{
					stringBuilder.Append(0.0);
					stringBuilder.Append("\t");
				}

				stringBuilder.Append(Format(probability));
				stringBuilder.Append("\n");
			}

			double[] importance = new double[inputs1[0].Length];

			foreach(var tree in model.Trees)
			{
				double[] buffer = tree.GetRawVariableImportance();

				for(int i=0; i<inputs1[0].Length; ++i)
				{
					importance[i] += buffer[i];
				}
			}

			File.WriteAllText("test.txt", stringBuilder.ToString());

			Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} ({1:F2} %)", score, (100.0*good)/all));
			Console.WriteLine();

			double sum = 0.0;

			for(int i=0; i<importance.Length; ++i)
			{
				sum += importance[i];
			}

			for(int i=0; i<importance.Length; ++i)
			{
				Console.Write(Format(100.0*importance[i]/sum, 1) + "% ");
			}

			Console.WriteLine();
			Console.ReadKey();
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

			int start = Window;

			int stop = History.Count - delay;

			int count = stop - start;

			double[][] inputs = new double[count][];

			double[] outputs = new double[count];

			for(int i=0; i<count; ++i)
			{
				int index = i+start;

				inputs[i] = new double[9]
				{
					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

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

			int start = Window;

			int stop = History.Count - delay;

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
				int index = i+start;

				inputs1[i] = new double[9]
				{
					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsShort(History, index, delay, percent))
				{
					outputs1[i] = 1.0;
				}
			}

			for(int i=count1; i<count1+count2; ++i)
			{
				int index = i;

				inputs2[i-count1] = new double[9]
				{
					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsShort(History, index, delay, percent))
				{
					outputs2[i-count1] = 1.0;
				}
			}

			var learner = new ClassificationRandomForestLearner(trees: 64);

			var model = learner.Learn(inputs1, outputs1);

			StringBuilder stringBuilder = new StringBuilder();

			int scores = 0;

			for(int i=1; i<count2-1; ++i)
			{
				double prediction = model.Predict(inputs2[i]);
				double probability = model.PredictProbability(inputs2[i]).Prediction;

				stringBuilder.Append(outputs2[i]);
				stringBuilder.Append("\t");

				if(probability > 0.99)
				{
					stringBuilder.Append(prediction);
					stringBuilder.Append("\t");

					if(prediction == outputs2[i])
					{
						++scores;
					}
					else
					{
						if(prediction == 1)
						{
							if(outputs2[i+1] == 1.0 || outputs2[i-1] == 1.0)
							{
								--scores;
							}
							else
							{
								--scores;
								--scores;
							}
						}
					}
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

			Console.WriteLine(scores);
			Console.ReadKey();
		}

		private static void SaveLearningData(string path, int test, int delay, decimal percent)
		{
			if(!Binance.ReadTradeHistory(path, out History))
			{
				Console.WriteLine("Can not Read Trade History");
				Console.ReadKey();
				return;
			}

			int start = Window;

			int stop = History.Count - delay;

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
				int index = i+start;

				inputs1[i] = new double[11]
				{
					GetDeltaPrice(History, index, 20),					
					GetDeltaVolume(History, index, 11),

					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsLong(History, index, delay, percent))
				{
					outputs1[i] = 1.0;
				}
			}

			for(int i=count1; i<count1+count2; ++i)
			{
				int index = i;

				inputs2[i-count1] = new double[11]
				{
					GetDeltaPrice(History, index, 20),
					GetDeltaVolume(History, index, 11),

					(double)(History[index-2].Average-History[index-3].Average),
					(double)(History[index-1].Average-History[index-2].Average),
					(double)(History[index-0].Average-History[index-1].Average),

					(double)(History[index-1].High-History[index-1].Low),
					(double)(History[index-0].High-History[index-0].Low),

					GetDeltaPrice(History, index, 15),
					GetDeltaPrice(History, index, 10),
					GetDeltaPrice(History, index, 8),
					GetDeltaPrice(History, index, 4),
				};

				if(IsLong(History, index, delay, percent))
				{
					outputs2[i-count1] = 1.0;
				}
			}

			StringBuilder learning = new StringBuilder();

			for(int i=0; i<count1; ++i)
			{
				for(int j=0; j<inputs1[i].Length; ++j)
				{
					learning.Append(Format(inputs1[i][j], 2));
					learning.Append("\t");
				}

				learning.Append(Format(outputs1[i], 2));
				learning.Append("\n");
			}

			File.WriteAllText("data.txt", learning.ToString());

			StringBuilder check = new StringBuilder();
			StringBuilder answers = new StringBuilder();

			for(int i=1; i<count2; ++i)
			{
				for(int j=0; j<inputs2[i].Length; ++j)
				{
					check.Append(Format(inputs2[i][j], 2));
					check.Append("\t");

					answers.Append(Format(inputs2[i][j], 2));
					answers.Append("\t");
				}

				answers.Append(Format(outputs2[i], 2));
				answers.Append("\n");
			}

			File.WriteAllText("check.txt", learning.ToString());
			File.WriteAllText("answers.txt", learning.ToString());
		}

		private static string Format(double value, int sign = 4)
		{
			sign = Math.Max(sign, 0);
			sign = Math.Min(sign, 8);

			return string.Format(CultureInfo.InvariantCulture, "{0:F" + sign + "}", value);
		}

		static void Main()
		{
			SaveLearningData("history.txt", 2048, 5, 0.001m);

			//LoadHistory();
			
			//TestLongPredictionModel("history.txt", 10000, 60, 0.004m);

			//GetLongPredictionModel("history.txt", "long2.xml", 10, 0.002m);

			//GetShortPredictionModel1("history.txt", "short.xml", 5, 0.001m);

			//decimal percent = 0.001m;
			//
			//for(int i=1; i<=10; ++i)
			//{
			//	int delay = 5;
			//	
			//	if(i == 2)
			//	{
			//		delay = 10;
			//	}
			//
			//	if(i == 3 || i == 4 || i == 5 || i == 6)
			//	{
			//		delay = 15;
			//	}
			//
			//	if(i == 7 || i == 8 || i == 9)
			//	{
			//		delay = 20;
			//	}
			//
			//	if(i == 10)
			//	{
			//		delay = 30;
			//	}
			//
			//	GetLongPredictionModel("history.txt", "long" + i + ".xml", delay, percent);
			//
			//	GetShortPredictionModel("history.txt", "short" + i + ".xml", delay, percent);
			//	
			//	percent += 0.001m;
			//}
		}
	}
}

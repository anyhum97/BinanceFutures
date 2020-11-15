using System;
using System.Globalization;

namespace PredictionModel
{
	public readonly struct LearningData
	{
		public readonly decimal[] Delta;

		public readonly int Result;

		public const int Count = 5;

		public LearningData(decimal[] delta, int result)
		{
			if(delta == null)
			{
				throw new Exception();
			}

			if(delta.Length != Count)
			{
				throw new Exception();
			}

			if(result < 0 || result > 1)
			{
				throw new Exception();
			}

			Delta = delta;
			Result = result;
		}

		public override string ToString()
		{
			if(Result == 0)
			{
				return string.Format("({0:F0}, {1:F0}, {2:F0}, {3:F0}, {4:F0}): False", Delta[0], Delta[1], Delta[2], Delta[3], Delta[4]);
			}
			else
			{
				return string.Format("({0:F0}, {1:F0}, {2:F0}, {3:F0}, {4:F0}): True", Delta[0], Delta[1], Delta[2], Delta[3], Delta[4]);
			}
		}

		public string Format()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:F8}\t{1:F8}\t{2:F8}\t{3:F8}\t{4:F8}\t{5}", Delta[0], Delta[1], Delta[2], Delta[3], Delta[4], Result);
		}

		public static bool TryParse(string str, out LearningData learningData)
		{
			learningData = new LearningData();

			if(str == null)
			{
				return false;
			}

			string[] parts = str.Split('\t');

			if(parts.Length != Count + 1)
			{
				return false;
			}
			
			decimal[] delta = new decimal[Count];

			for(int i=0; i<Count; ++i)
			{
				if(decimal.TryParse(parts[i], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
				{
					delta[i] = value;
				}
				else
				{
					return false;
				}
			}

			if(int.TryParse(parts[Count], out int result))
			{
				learningData = new LearningData(delta, result);

				return true;
			}

			return false;
		}
	}
}


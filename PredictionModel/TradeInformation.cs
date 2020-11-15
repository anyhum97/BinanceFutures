using System;
using System.Globalization;

namespace PredictionModel
{
	public readonly struct TradeInformation
	{
		public readonly DateTime CloseTime;

		public readonly decimal Low;
		public readonly decimal High;
		public readonly decimal Average;

		public TradeInformation(DateTime closeTime, decimal low, decimal high)
		{
			CloseTime = closeTime;

			Low = low;
			High = high;

			Average = 0.5m*(low + high);
		}

		public static bool TryParse(string str, out TradeInformation tradeInformation)
		{
			tradeInformation = new TradeInformation();

			if(str == null)
			{
				return false;
			}

			if(str.Length > 256)
			{
				str = str.Substring(0, 256);
			}

			string[] parts = str.Split('\t');

			if(parts.Length < 3)
			{
				return false;
			}

			if(!DateTime.TryParse(parts[0], out DateTime time))
			{
				return false;
			}

			decimal[] values = new decimal[2];

			for(int i=0; i<2; ++i)
			{
				if(!decimal.TryParse(parts[i+1], NumberStyles.Number, CultureInfo.InvariantCulture, out values[i]))
				{
					return false;
				}

				if(values[i] < 0.0m)
				{
					return false;
				}
			}

			tradeInformation = new TradeInformation(time, values[0], values[1]);

			return true;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:dd.MM.yyyy HH:mm} Price: {1:F6}", CloseTime, Average);
		}

		public string Format()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:dd.MM.yyyy HH:mm}\t{1:F8}\t{2:F8}\t{3:F8}", CloseTime, Low, High, Average);
		}
	}
}


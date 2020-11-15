using System;
using System.Globalization;

namespace BinanceFutures
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

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:dd.MM.yyyy HH:mm} Price: {1:F6}", CloseTime, Average);
		}
	}
}


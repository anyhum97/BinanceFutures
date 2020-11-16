using System;
using System.Collections.Generic;

using Binance.Net;
using Binance.Net.Enums;

namespace BinanceFutures
{
	public static class Binance
	{
		public const string Symbol = "BTCUSDT";

		public static bool GetCurrentPrice(out decimal price)
		{
			price = 0.0m;

			try
			{
				BinanceClient client = new BinanceClient();
				
				var responce = client.FuturesUsdt.Market.GetPrice(Symbol);

				if(responce.Success)
				{
					price = responce.Data.Price;

					return true;
				}
				else
				{
					Logger.Write("GetCurrentPrice: " + responce.Error.Message);

					return false;
				}
			}
			catch(Exception exception)
			{
				Logger.Write("GetCurrentPrice: " + exception.Message);

				return false;
			}
		}

		public static bool GetTradeHistory(int count, out List<TradeInformation> history)
		{
			history = new List<TradeInformation>();

			try
			{
				count = Math.Min(count, 1000);
				count = Math.Max(count, 1);

				BinanceClient tradeClient = new BinanceClient();

				//DateTime start = DateTime.Now.AddMinutes(-1-count).ToUniversalTime();
				//DateTime stop = DateTime.Now.AddMinutes(-1).ToUniversalTime();

				DateTime start = DateTime.Now.AddMinutes(-count).ToUniversalTime();
				DateTime stop = DateTime.Now.ToUniversalTime();

				var responce = tradeClient.FuturesUsdt.Market.GetKlines(Symbol, KlineInterval.OneMinute, start, stop);
				
				if(responce.Success)
				{
					foreach(var record in responce.Data)
					{
						history.Add(new TradeInformation(record.CloseTime.ToLocalTime(), record.Low, record.High));
					}

					if(history.Count == count)
					{
						return true;
					}

					return false;
				}
				else
				{
					Logger.Write("GetTradeHistory: Bad Request, Error = " + responce.Error.Message);

					return false;
				}
			}
			catch(Exception exception)
			{
				Logger.Write("GetTradeHistory: " + exception.Message);

				return false;
			}
		}
	}
}


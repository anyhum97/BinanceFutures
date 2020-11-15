using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using CryptoExchange.Net.Objects;

using Binance.Net.Objects.Futures.FuturesData;
using Binance.Net.Objects.Futures.MarketData;
using Binance.Net.Objects.Futures.MarketStream;

using Binance.Net;
using Binance.Net.Enums;

namespace PredictionModel
{
	public static class Binance
	{
		public static bool GetTradeHistory(string symbol, DateTime start, DateTime stop, out List<TradeInformation> history)
		{
			history = new List<TradeInformation>();

			try
			{
				BinanceClient tradeClient = new BinanceClient();

				start = start.ToUniversalTime();
				stop = stop.ToUniversalTime();

				TimeSpan timeSpan = stop - start;

				int count = (int)timeSpan.TotalMinutes;

				const int size = 720;
				const int attempts = 3;

				int pages = count / size;
				int remainder = count - pages*size;

				for(int i=0; i<pages; ++i)
				{
					bool success = false;

					string message = string.Empty;

					for(int j=0; j<attempts; ++j)
					{
						var page = tradeClient.FuturesUsdt.Market.GetKlines(symbol, KlineInterval.OneMinute, start, stop, size);
						
						if(page.Success)
						{
							start = start.AddMinutes(size);

							foreach(var record in page.Data)
							{
								history.Add(new TradeInformation(record.CloseTime.ToLocalTime(), record.Low, record.High));
							}

							success = true;

							break;
						}
						else
						{
							message = page.Error.Message;
						}
					}

					if(!success)
					{
						Logger.Write("GetTradeHistory: " + message);

						return false;
					}
				}

				if(remainder > 0)
				{
					bool success = false;

					string message = string.Empty;

					for(int j = 0; j < attempts; ++j)
					{
						var page = tradeClient.FuturesUsdt.Market.GetKlines(symbol, KlineInterval.OneMinute, start, stop, size);

						if(page.Success)
						{
							foreach(var record in page.Data)
							{
								history.Add(new TradeInformation(record.CloseTime.ToLocalTime(), record.Low, record.High));
							}

							success = true;

							break;
						}
						else
						{
							message = page.Error.Message;
						}
					}

					if(!success)
					{
						Logger.Write("GetTradeHistory: " + message);

						return false;
					}
				}

				return true;
			}
			catch(Exception exception)
			{
				Logger.Write("GetTradeHistory: " + exception.Message);

				return false;
			}
		}

		public static bool ReadTradeHistory(string path, out List<TradeInformation> history)
		{
			history = new List<TradeInformation>();

			try
			{
				string str = File.ReadAllText(path);

				string[] lines = str.Split('\n');

				for(int i=0; i<lines.Length; ++i)
				{
					if(TradeInformation.TryParse(lines[i], out var extendedSellInformation))
					{
						history.Add(extendedSellInformation);
					}
					else
					{
						return true;
					}
				}

				return true;
			}
			catch(Exception exception)
			{
				Logger.Write("ReadTradeHistory: " + exception.Message);
				
				return false;
			}
		}

		public static bool WriteTradeHistory(string path, List<TradeInformation> history)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				
				for(int i=0; i<history.Count; ++i)
				{
					stringBuilder.Append(history[i].Format());

					if(i < history.Count - 1)
					{
						stringBuilder.Append("\n");
					}
				}
				
				File.WriteAllText(path, stringBuilder.ToString());

				return true;
			}
			catch(Exception exception)
			{
				Logger.Write("WriteHistory: " + exception.Message);
				
				return false;
			}
		}
	}
}

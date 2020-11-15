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
using TradeBot;

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
	}
}


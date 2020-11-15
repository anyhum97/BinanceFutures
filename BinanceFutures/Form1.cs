using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace BinanceFutures
{
	public partial class Form1 : Form
	{
		private const int States = 3;
		private const int Levels = 10;
		private const int Count = 5;

		private static Bitmap[] ColoredImages;

		private PictureBox[] LongImages;
		private PictureBox[] ShortImages;

		private Thread CurrentPriceThread;
		private Thread TradeHistoryThread;

		private bool[] IsLong;
		private bool[] IsShort;

		public const string Version = "0.12";

		public decimal CurrentPrice { get; private set; }

		public decimal BasePrice { get; private set; }

		public Form1()
		{
			InitializeComponent();
			InitializeObjects();
		}

		private void InitializeObjects()
		{
			PreparePictures();
			PreparePredictionModel();
			UpdateColors();
		}

		private void PreparePictures()
		{
			ColoredImages = new Bitmap[States]
			{
				GetBitmap(32, 2, Color.FromArgb(129, 129, 129)),
				GetBitmap(32, 2, Color.FromArgb(14, 245, 37)),
				GetBitmap(32, 2, Color.FromArgb(245, 14, 37)),
			};

			LongImages = new PictureBox[Levels]
			{
				pictureBox1,
				pictureBox2,
				pictureBox3,
				pictureBox4,
				pictureBox5,
				pictureBox6,
				pictureBox7,
				pictureBox8,
				pictureBox9,
				pictureBox10
			};

			ShortImages = new PictureBox[Levels]
			{
				pictureBox11,
				pictureBox12,
				pictureBox13,
				pictureBox14,
				pictureBox15,
				pictureBox16,
				pictureBox17,
				pictureBox18,
				pictureBox19,
				pictureBox20
			};
		}

		private void PreparePredictionModel()
		{
			IsLong = new bool[Levels];
			IsShort = new bool[Levels];
		}

		private void UpdateColors()
		{
			for(int i=0; i<Levels; ++i)
			{
				if(IsLong[i])
				{
					LongImages[i].Image = ColoredImages[1];
				}
				else
				{
					LongImages[i].Image = ColoredImages[0];
				}

				if(IsShort[i])
				{
					ShortImages[i].Image = ColoredImages[1];
				}
				else
				{
					ShortImages[i].Image = ColoredImages[0];
				}
			}
		}

		private void UpdateCurrentPrice()
		{
			if(Binance.GetCurrentPrice(out decimal price))
			{
				CurrentPrice = price;
			}
			else
			{
				CurrentPrice = 0.0m;
			}
		}

		private void UpdateTradeHistory()
		{
			if(Binance.GetTradeHistory(Count, out var history))
			{
				BasePrice = history[Count-1].Average;
			}
			else
			{
				BasePrice = 0.0m;
			}
		}

		private void UpdateCurrentPriceThread()
		{
			if(CurrentPriceThread != null)
			{
				CurrentPriceThread.Abort();
			}

			CurrentPriceThread = new Thread(UpdateCurrentPrice);
			CurrentPriceThread.IsBackground = false;
			CurrentPriceThread.Start();
		}
		
		private void UpdateTradeHistoryThread()
		{
			if(TradeHistoryThread != null)
			{
				TradeHistoryThread.Abort();
			}

			TradeHistoryThread = new Thread(UpdateTradeHistory);
			TradeHistoryThread.IsBackground = false;
			TradeHistoryThread.Start();
		}

		private void UpdateWindowTitle()
		{
			if(CurrentPrice > 0.0m)
			{
				Text = string.Format("Binance Futures {0} ({1}) - {2}", Version, Binance.Symbol, Format(CurrentPrice, 2));
			}
			else
			{
				Text = string.Format("Binance Futures {0} ({1})", Version, Binance.Symbol);
			}
		}

		private void TimerTick1(object sender, EventArgs e)
		{
			UpdateColors();
			UpdateWindowTitle();
		}

		private void TimerTick2(object sender, EventArgs e)
		{
			UpdateCurrentPriceThread();
			UpdateTradeHistoryThread();
		}

		private void SetClipboardPrice(decimal factor)
		{
			if(BasePrice > 0.0m)
			{
				if(factor >= 0.99m && factor <= 1.01m)
				{
					decimal target = factor * BasePrice;

					Clipboard.SetText(Format(target, 2));
				}
				else
				{
					Clipboard.SetText("");
				}
			}
			else
			{
				Clipboard.SetText("");
			}
		}

		private void PictureClick1(object sender, EventArgs e)
		{
			SetClipboardPrice(1.001m);
		}

		private void PictureClick2(object sender, EventArgs e)
		{
			SetClipboardPrice(1.002m);
		}

		private void PictureClick3(object sender, EventArgs e)
		{
			SetClipboardPrice(1.003m);
		}

		private void PictureClick4(object sender, EventArgs e)
		{
			SetClipboardPrice(1.004m);
		}

		private void PictureClick5(object sender, EventArgs e)
		{
			SetClipboardPrice(1.005m);
		}

		private void PictureClick6(object sender, EventArgs e)
		{
			SetClipboardPrice(1.006m);
		}

		private void PictureClick7(object sender, EventArgs e)
		{
			SetClipboardPrice(1.007m);
		}

		private void PictureClick8(object sender, EventArgs e)
		{
			SetClipboardPrice(1.008m);
		}

		private void PictureClick9(object sender, EventArgs e)
		{
			SetClipboardPrice(1.009m);
		}

		private void PictureClick10(object sender, EventArgs e)
		{
			SetClipboardPrice(1.010m);
		}
		
		private void PictureClick11(object sender, EventArgs e)
		{
			SetClipboardPrice(0.999m);
		}

		private void PictureClick12(object sender, EventArgs e)
		{
			SetClipboardPrice(0.998m);
		}

		private void PictureClick13(object sender, EventArgs e)
		{
			SetClipboardPrice(0.997m);
		}

		private void PictureClick14(object sender, EventArgs e)
		{
			SetClipboardPrice(0.996m);
		}

		private void PictureClick15(object sender, EventArgs e)
		{
			SetClipboardPrice(0.995m);
		}

		private void PictureClick16(object sender, EventArgs e)
		{
			SetClipboardPrice(0.994m);
		}

		private void PictureClick17(object sender, EventArgs e)
		{
			SetClipboardPrice(0.993m);
		}

		private void PictureClick18(object sender, EventArgs e)
		{
			SetClipboardPrice(0.992m);
		}

		private void PictureClick19(object sender, EventArgs e)
		{
			SetClipboardPrice(0.991m);
		}

		private void PictureClick20(object sender, EventArgs e)
		{
			SetClipboardPrice(0.990m);
		}

		private static Bitmap GetBitmap(int side, int border, Color color)
		{
			Bitmap bitmap = new Bitmap(side, side);

			Color borderColor = Color.FromArgb(103, 103, 108);

			for(int x=0; x<border; ++x)
			{
				for(int y=0; y<side; ++y)
				{
					bitmap.SetPixel(x, y, borderColor);
					bitmap.SetPixel(side-border+x, y, borderColor);
				}
			}

			for(int x=0; x<side; ++x)
			{
				for(int y=0; y<2; ++y)
				{
					bitmap.SetPixel(x, y, borderColor);
					bitmap.SetPixel(x, side-border+y, borderColor);
				}
			}

			for(int x=border; x<side-border; ++x)
			{
				for(int y=border; y<side-border; ++y)
				{
					bitmap.SetPixel(x, y, color);
				}
			}

			return bitmap;
		}

		private static string Format(decimal value, int sign = 4)
		{
			sign = Math.Max(sign, 0);
			sign = Math.Min(sign, 8);

			return string.Format(CultureInfo.InvariantCulture, "{0:F" + sign + "}", value);
		}

		private static string Format(double value, int sign = 4)
		{
			sign = Math.Max(sign, 0);
			sign = Math.Min(sign, 8);

			return string.Format(CultureInfo.InvariantCulture, "{0:F" + sign + "}", value);
		}
	}
}

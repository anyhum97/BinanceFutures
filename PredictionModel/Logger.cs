using System;
using System.Threading;
using System.IO;

namespace PredictionModel
{
	public static class Logger
	{
		public static bool IsConsoleEnabled { get; set; } = true;
		
		private const string LogFilePath = "PredictionModel.txt";
		
		private const long MaxFileLength = 67108864;
		
		private static void Print(string str)
		{
			if(IsConsoleEnabled)
			{
				try
				{
					Console.Write(str);
				}
				catch
				{
					
				}
			}
		}

		public static void Write(string str)
		{
			Print(str + "\n\n");
			
			try
			{
				if(File.Exists(LogFilePath))
				{
					FileInfo fileInfo = new FileInfo(LogFilePath);
					
					if(fileInfo.Length > MaxFileLength)
					{
						File.Delete(LogFilePath);

						Thread.Sleep(20);
					}
				}
			}
			catch(Exception exception)
			{
				Print(exception + "\n\n");
			}
			
			try
			{
				string currentMoment = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss ");
				
				File.AppendAllText(LogFilePath, currentMoment + str + "\r\n\r\n");
			}
			catch(Exception exception)
			{
				Print(exception + "\n\n");
			}
		}
	}
}

using System;

namespace EduCATS.Helpers
{
	public class ConverterSize
	{
		// Load all suffixes in an array  
		static readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
		public static string FormatSize(long bytes)
		{
			int counter = 0;
			decimal number = (decimal)bytes;
			while (Math.Round(number / 1024) >= 1)
			{
				number = number / 1024;
				counter++;
			}
			return string.Format("{0:N1}{1}", number, suffixes[counter]);
		}
	}
}


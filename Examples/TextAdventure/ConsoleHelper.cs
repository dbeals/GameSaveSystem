#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Date: Month Day, Year
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/6/2015 2:17:58 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Text;

#endregion

namespace TextAdventure
{
	/// <summary>
	/// 
	/// </summary>
	public static class ConsoleHelper
	{
		public static string FitString(string input)
		{
			StringBuilder output = new StringBuilder(input);
			var bufferWidth = Console.BufferWidth;
			var lastSpaceIndex = 0;
			var textWidth = 0;
			for(var index = 0; index < output.Length; ++index)
			{
				var character = output[index];
				if(character == ' ')
					lastSpaceIndex = index;
				if(character == '\n')
				{
					textWidth = 0;
					lastSpaceIndex = 0;
					continue;
				}

				if(textWidth + 1 > bufferWidth)
				{
					output.Remove(lastSpaceIndex, 1);
					output.Insert(lastSpaceIndex, Environment.NewLine);
					index = lastSpaceIndex + Environment.NewLine.Length;
					textWidth = 0;
					continue;
				}

				++textWidth;
			}

			return output.ToString();
		}

		public static void WriteInColor(ConsoleColor color, string input)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(input);
			Console.ForegroundColor = oldColor;
		}

		public static void WriteLineInColor(ConsoleColor color, string input)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(input);
			Console.ForegroundColor = oldColor;
		}
	}
}

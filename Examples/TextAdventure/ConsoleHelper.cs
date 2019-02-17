// /***********************************************************************
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org/>
// ***********************************************************************/

using System;
using System.Text;

namespace TextAdventure
{
	public static class ConsoleHelper
	{
		#region Methods
		public static string FitString(string input)
		{
			var output = new StringBuilder(input);
			var bufferWidth = Console.BufferWidth;
			var lastSpaceIndex = 0;
			var textWidth = 0;
			for (var index = 0; index < output.Length; ++index)
			{
				var character = output[index];
				if (character == ' ')
					lastSpaceIndex = index;
				if (character == '\n')
				{
					textWidth = 0;
					lastSpaceIndex = 0;
					continue;
				}

				if (textWidth + 1 > bufferWidth)
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
		#endregion
	}
}
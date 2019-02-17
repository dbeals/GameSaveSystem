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
using System.IO;
using GameSaveSystem;

namespace GameSaveSystemTests
{
	public sealed class SwapSaveManager : SwapSaveManagerBase
	{
		#region Variables
		private readonly bool needsToFail;

		// We'll store our variables right here, but normally they'd go in a GameState class or such.
		public string PlayerName;
		public int PlayerAge;
		private int saveCount;
		#endregion

		#region Properties
		public override string FileExtension => ".sav";

		public override string FileKey => "SAVTEST";

		public override Version CurrentVersion => new Version(1, 0);
		#endregion

		#region Constructors
		public SwapSaveManager(string rootPath, bool needsToFail)
			: base(rootPath, "AutoSave", 900.0f) =>
			this.needsToFail = needsToFail;
		#endregion

		#region Methods
		protected override void OnSaveRequested(string fullFilePath)
		{
			++saveCount;
			using (var stream = File.OpenWrite(fullFilePath))
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.WriteLine(FileKey);
					writer.WriteLine(CurrentVersion.ToString());
					writer.WriteLine(PlayerName);

					if (needsToFail == false || saveCount < 3)
					{
						Console.WriteLine("Creating a valid save ({0}).", fullFilePath);
						writer.Write(PlayerAge);
					}
					else
						Console.WriteLine("Creating an invalid save ({0}).", fullFilePath);
				}
			}
		}

		protected override bool OnLoadRequested(string fullFilePath)
		{
			try
			{
				using (var stream = File.OpenRead(fullFilePath))
				{
					using (var reader = new StreamReader(stream))
					{
						var key = reader.ReadLine();
						if (key != FileKey)
							return false;

						var version = Version.Parse(reader.ReadLine());
						if (version != CurrentVersion)
							return false;

						PlayerName = reader.ReadLine();
						PlayerAge = int.Parse(reader.ReadLine());
						return true;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Unfortunately we were unable to load '{0}'. We'll try a previous version if it's available.", fullFilePath);
				Console.WriteLine(e.Message);
				return false;
			}
		}
		#endregion
	}
}
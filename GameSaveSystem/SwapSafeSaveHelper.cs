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
using System.Linq;

namespace GameSaveSystem
{
	public static class SwapSafeSaveHelper
	{
		#region Methods
		/// <summary>
		///     Processes a save request, passing the output file name to <paramref name="saveCallback" />.
		/// </summary>
		/// <param name="rootPath">The root path to the save folder.</param>
		/// <param name="fileName">The file name to use (in FileName.Extension format.)</param>
		/// <param name="saveCallback">The callback that does the actual saving.</param>
		public static void SaveGame(string rootPath, string fileName, Action<string> saveCallback)
		{
			var directoryInfo = new DirectoryInfo(rootPath);
			directoryInfo.Create();

			var target = Path.Combine(rootPath, fileName);
			if (File.Exists(target))
			{
				var backupPath = target + ".bak";
				var newPath = target + ".new";
				saveCallback(newPath);
				if (File.Exists(backupPath))
					File.Delete(backupPath);
				File.Move(target, backupPath);
				File.Move(newPath, target);
			}
			else
				saveCallback(target);
		}

		/// <summary>
		///     Processes a load request, passing the file name to <paramref name="loadCallback" />.
		/// </summary>
		/// <param name="rootPath">The root path to the save folder.</param>
		/// <param name="fileName">The file name to use (in FileName.Extension format.)</param>
		/// <param name="forceRevert">
		///     True to skip the first result (in cases where the system can't tell that the file is invalid,
		///     but the player can), false otherwise.
		/// </param>
		/// <param name="loadCallback">The callback that does the actual loading.</param>
		/// <returns>The actual file name (FileName.Index.Extension format) that was loaded.</returns>
		public static string LoadGame(string rootPath, string fileName, bool forceRevert, Func<string, bool> loadCallback)
		{
			var directoryInfo = new DirectoryInfo(rootPath);
			if (!directoryInfo.Exists)
				return null;

			var fileInfosEnumerable = from fileInfo in directoryInfo.EnumerateFiles(fileName + '*')
				where !fileInfo.Name.EndsWith(".new")
				orderby GetFileSortValue(fileInfo) descending
				select fileInfo;

			var fileInfos = (forceRevert ? fileInfosEnumerable.Skip(1) : fileInfosEnumerable).ToArray();
			if (!fileInfos.Any())
				return null;

			var filePath = from fileInfo in fileInfos
				where loadCallback(Path.Combine(rootPath, fileInfo.Name))
				select fileInfo.Name;

			return filePath.FirstOrDefault();
		}

		/// <summary>
		///     Calculates the sort value for a file, using the LastWriteTimeUtc and putting .bak files last.
		///     Note that this is an assumption, but is our best bet since file times only go to the minute.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns>The file's sort value.</returns>
		private static long GetFileSortValue(FileSystemInfo fileInfo) => fileInfo.LastWriteTimeUtc.Ticks + (fileInfo.Name.EndsWith(".bak") ? 0 : 1);
		#endregion
	}
}
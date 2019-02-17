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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
			Contract.Ensures(saveCallback != null, "You must provide a save callback to use SwapSafeSaveManager.SaveGame().");

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
			Contract.Ensures(loadCallback != null, "You must provide a load callback to use SwapSafeSaveManager.LoadGame().");

			var directoryInfo = new DirectoryInfo(rootPath);
			if (!directoryInfo.Exists)
				return null;

			var fileInfosEnumerable = (from fileInfo in directoryInfo.EnumerateFiles(fileName + '*')
				where !fileInfo.Name.EndsWith(".new")
				orderby GetFileSortValue(fileInfo) descending
				select fileInfo);

			var fileInfos = (forceRevert ? fileInfosEnumerable.Skip(1) : fileInfosEnumerable).ToArray();
			if (fileInfos.Length > 0)
			{
				foreach (var fileInfo in fileInfos)
				{
					if (loadCallback(Path.Combine(rootPath, fileInfo.Name)))
						return fileInfo.Name;
				}
			}

			return null;
		}

		/// <summary>
		///     Enumerates all of the save files, returning the base name (FileName.Extension) as well as the most recent
		///     incremental version (FileName.Index.Extension.)
		/// </summary>
		/// <param name="rootPath">The root path to the save folder.</param>
		/// <param name="fileExtension">The save file extension to use.</param>
		/// <returns>
		///     A list of pairs containing the base name (FileName.Extension) as the Key and the incremental file
		///     (FileName.Index.Extension) as the Value.
		/// </returns>
		public static IEnumerable<KeyValuePair<string, string>> EnumerateSaveFiles(string rootPath, string fileExtension)
		{
			var directoryInfo = new DirectoryInfo(rootPath);
			return (from fileInfo in directoryInfo.EnumerateFiles(SafeSaveHelper.AddFileExtension("*", fileExtension))
				orderby GetFileSortValue(fileInfo) descending
				select new KeyValuePair<string, string>(fileInfo.Name, fileInfo.Name));
		}

		/// <summary>
		///     Deletes all save files in <paramref name="rootPath" /> based on the base name (FileName.Extension) provided.
		/// </summary>
		/// <param name="rootPath">The root path to the save folder.</param>
		/// <param name="baseFileName">The base file name (FileName.Extension) to delete.</param>
		public static void CleanseSaveByBaseName(string rootPath, string baseFileName)
		{
			var directoryInfo = new DirectoryInfo(rootPath);
			var files = (from file in directoryInfo.EnumerateFiles(baseFileName + '*')
				select file).ToArray();

			foreach (var file in files)
				file.Delete();
		}

		/// <summary>
		///     Calculates the sort value for a file, using the LastWriteTimeUtc and putting .bak files last.
		///     Note that this is an assumption, but is our best bet since file times only go to the minute.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns>The file's sort value.</returns>
		private static long GetFileSortValue(FileInfo fileInfo) => fileInfo.LastWriteTimeUtc.Ticks + (fileInfo.Name.EndsWith(".bak") ? 0 : 1);
		#endregion
	}
}
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
using System.IO;
using System.Linq;

namespace GameSaveSystem
{
	public static class SafeSaveHelper
	{
		#region Methods
		/// <summary>
		///     Takes a file name (either FileName.Extension or FileName.Index.Extension) and returns the base form
		///     (FileName.Extension.)
		/// </summary>
		/// <param name="fileName">The file name to convert.</param>
		/// <returns>A base form file name (FileName.Extension.)</returns>
		public static string GetBaseFileName(string fileName)
		{
			var parts = fileName.Split('.');
			switch (parts.Length)
			{
				case 2: return fileName;
				case 3: return parts[0] + '.' + parts[2];
				default: throw new ArgumentException($"{fileName} is not a valid file name. GetBaseFileName() expects file names in the format of FileName.Extension or FileName.Index.Extension.", nameof(fileName));
			}
		}

		/// <summary>
		///     Takes a file name (either FileName.Extension or FileName.Index.Extension) and returns an incremental file name
		///     using the supplied <paramref name="index" /> (FileName.Index.Extension.)
		/// </summary>
		/// <param name="fileName">The file name to convert.</param>
		/// <param name="index">The incremental index to use.</param>
		/// <returns>An incremental file name (FileName.Index.Extension.)</returns>
		public static string GetIncrementalFileName(string fileName, int index)
		{
			var parts = fileName.Split('.');
			if (parts.Length < 2 || parts.Length > 3)
				throw new ArgumentException($"{fileName} is not a valid file name. GetIncrementalFileName() expects file names in the format of FileName.Extension or FileName.Index.Extension.", nameof(fileName));
			return AddFileExtension(parts[0] + '.' + index, parts.Length == 2 ? parts[1] : parts[2]);
		}

		/// <summary>
		///     Takes a file name and increments it's index (wrapping back to 1 if we reach <paramref name="maximumIndex" />.)
		/// </summary>
		/// <param name="fileName">The file name to increment.</param>
		/// <param name="maximumIndex">The maximum index value.</param>
		/// <returns>The file name, with it's index incremented by 1.</returns>
		public static string IncrementFileName(string fileName, int maximumIndex)
		{
			var parts = fileName.Split('.');
			if (parts.Length != 3)
				throw new ArgumentException($"{fileName} is not a valid file name. IncrementFileName() expects file names in the format of FileName.Index.Extension.", nameof(fileName));

			if (!int.TryParse(parts[1], out var index))
				throw new ArgumentException($"{fileName} is not a valid file name. IncrementFileName() expects the Index component to be a number.", nameof(fileName));

			if (index++ >= maximumIndex)
				index = 1;
			return parts[0] + '.' + index + '.' + parts[2];
		}

		/// <summary>
		///     Returns a file search pattern (FileName.*.Extension) to find incremental files.
		/// </summary>
		/// <param name="fileName">
		///     The file name to use for creating the pattern (either FileName.Extension or
		///     FileName.Index.Extension.)
		/// </param>
		/// <returns>A search pattern to find incremental files.</returns>
		public static string GetSearchPatternFromFileName(string fileName)
		{
			var parts = fileName.Split('.');
			switch (parts.Length)
			{
				case 2: return parts[0] + ".*." + parts[1];
				case 3: return parts[0] + ".*." + parts[2];
				default: throw new ArgumentException($"{fileName} is not a valid file name. GetSearchPatternFromFileName() expects file names in the format of FileName.Extension or FileName.Index.Extension.", nameof(fileName));
			}
		}

		/// <summary>
		///     Creates a valid file name, ensuring that a dot is placed between <paramref name="fileName" /> and
		///     <paramref name="extension" />.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="extension"></param>
		/// <returns>The full file name.</returns>
		public static string AddFileExtension(string fileName, string extension) => extension.StartsWith(".") ? fileName + extension : $"{fileName}.{extension}";

		/// <summary>
		///     Processes a save request, passing the output file name to <paramref name="saveCallback" />.
		/// </summary>
		/// <param name="rootPath">The root path to the save folder.</param>
		/// <param name="fileName">The file name to use (in FileName.Extension format.)</param>
		/// <param name="maximumSaveCount">The maximum number of incremental saves allowed.</param>
		/// <param name="saveType">The type of save.</param>
		/// <param name="saveCallback">The callback that does the actual saving.</param>
		public static void SaveGame(string rootPath, string fileName, int maximumSaveCount, SaveType saveType, Action<SaveType, string> saveCallback)
		{
			var directoryInfo = new DirectoryInfo(rootPath);
			directoryInfo.Create();

			var fileInfos = (from fileInfo in directoryInfo.EnumerateFiles(GetSearchPatternFromFileName(fileName))
				orderby fileInfo.LastWriteTimeUtc
				select fileInfo).ToArray();
			if (fileInfos.Length == 0)
			{
				saveCallback(saveType, Path.Combine(rootPath, GetIncrementalFileName(fileName, 1)));
				return;
			}

			saveCallback(saveType, Path.Combine(rootPath, IncrementFileName(fileInfos.Last().Name, maximumSaveCount)));
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

			var fileInfosEnumerable = from fileInfo in directoryInfo.EnumerateFiles(GetSearchPatternFromFileName(fileName))
				orderby GetFileSortValue(fileInfo) descending
				select fileInfo;

			var fileInfos = (forceRevert ? fileInfosEnumerable.Skip(1) : fileInfosEnumerable).ToArray();
			return (from fileInfo in fileInfos where loadCallback(Path.Combine(rootPath, fileInfo.Name)) select fileInfo.Name).FirstOrDefault();
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
			if (!directoryInfo.Exists)
				return Enumerable.Empty<KeyValuePair<string, string>>();

			return from fileInfo in directoryInfo.EnumerateFiles(AddFileExtension("*", fileExtension))
				orderby GetFileSortValue(fileInfo) descending
				group fileInfo by GetBaseFileName(fileInfo.Name)
				into fileGroup
				select new KeyValuePair<string, string>(fileGroup.Key, fileGroup.First().Name);
		}

		/// <summary>
		///     Deletes all save files in <paramref name="rootPath" /> based on the base name (FileName.Extension) provided.
		/// </summary>
		/// <param name="rootPath">The root path to the save folder.</param>
		/// <param name="baseFileName">The base file name (FileName.Extension) to delete.</param>
		public static void CleanseSaveByBaseName(string rootPath, string baseFileName)
		{
			var directoryInfo = new DirectoryInfo(rootPath);
			var files = (from file in directoryInfo.EnumerateFiles(GetSearchPatternFromFileName(baseFileName))
				select file).ToArray();

			foreach (var file in files)
				file.Delete();
		}

		/// <summary>
		///     Calculates the sort value for a file, using the LastWriteTimeUtc as a base and index as an offset (in case two
		///     files have the same exact LastWriteTimeUtc value.)
		///     Note that this is an assumption, but is our best bet since file times only go to the minute.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns>The file's sort value.</returns>
		private static long GetFileSortValue(FileSystemInfo fileInfo)
		{
			var parts = fileInfo.Name.Split('.');
			var index = 1;
			if (parts.Length == 3 && !int.TryParse(parts[1], out index))
				index = 1;
			return fileInfo.LastWriteTimeUtc.Ticks + index;
		}
		#endregion
	}
}
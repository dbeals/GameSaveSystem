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
using System.Diagnostics;
using System.IO;
using GameSaveSystem;

namespace ExamplesCore
{
	public abstract class GameStateBase : SaveManagerBase, IGameState
	{
		#region Variables
		private readonly Dictionary<string, object> stateValues = new Dictionary<string, object>();
		#endregion

		#region Constructors
		protected GameStateBase(string rootPath, string autoSaveFileNamePrefix, float autoSaveIntervalInSeconds, int maximumAutoSaveCount, int maximumSafeSaveCount)
			: base(rootPath, autoSaveFileNamePrefix, autoSaveIntervalInSeconds, maximumAutoSaveCount, maximumSafeSaveCount) { }
		#endregion

		#region Methods
		public void SetStateValue(string key, object value)
		{
			// I know I could simply make 3 overloads here that take the different types
			// However, I find this to be a better solution as we'll more than likely need
			// other types eventually. I simply didn't implement serializing them to and from strings.
			if (!(value is bool || value is int || value is string))
				throw new ArgumentException("The game state system only supports boolean, integer, and string state values.", "value");
			stateValues[key] = value;
		}

		public object GetStateValue(string key) => stateValues[key];

		public bool StateValueExists(string key) => stateValues.ContainsKey(key);

		public void ClearStateValues()
		{
			stateValues.Clear();
		}

		protected abstract void HandleLoadError(string filePath, LoadResult error);
		protected abstract LoadResult LoadGame(StreamReader reader, Version version);

		protected override void OnSaveRequested(string fullFilePath)
		{
			using (var stream = File.OpenWrite(fullFilePath))
			{
				using (var writer = new StreamWriter(stream))
				{
					SaveGame(writer);
				}
			}
		}

		protected override bool OnLoadRequested(string fullFilePath)
		{
			using (var stream = File.OpenRead(fullFilePath))
			{
				using (var reader = new StreamReader(stream))
				{
					var result = LoadGame(reader);
					if (result != LoadResult.Success)
					{
						HandleLoadError(fullFilePath, result);
						return false;
					}

					return true;
				}
			}
		}

		protected virtual void SaveGame(StreamWriter writer)
		{
			Debug.Assert(writer.BaseStream.Position == 0, "You need to call base.SaveGame() at the top of your override as it writes the header information.");
			writer.WriteLine(FileKey);
			writer.WriteLine(CurrentVersion.ToString());
			writer.WriteLine(stateValues.Count);
			foreach (var pair in stateValues)
			{
				writer.Write(pair.Key);
				if (pair.Value is bool)
				{
					writer.Write(";BOOL;");
					writer.WriteLine((bool) pair.Value);
				}
				else if (pair.Value is int)
				{
					writer.Write(";INT;");
					writer.WriteLine((int) pair.Value);
				}
				else if (pair.Value is string)
				{
					writer.Write(";STRING;");
					writer.WriteLine((pair.Value as string) ?? string.Empty);
				}
			}
		}

		private LoadResult LoadGame(StreamReader reader)
		{
			if (reader.BaseStream.Length == 0)
				return LoadResult.EmptyFile;

			var fileKey = reader.ReadLine();
			if (fileKey != FileKey)
				return LoadResult.InvalidKey;

			var versionString = reader.ReadLine();
			Version version;
			if (!Version.TryParse(versionString, out version))
				return LoadResult.InvalidFormat;

			var stateValueCountString = reader.ReadLine();
			int stateValueCount;
			if (!int.TryParse(stateValueCountString, out stateValueCount))
				return LoadResult.InvalidFormat;

			stateValues.Clear();
			for (var index = 0; index < stateValueCount; ++index)
			{
				var stateValueLine = reader.ReadLine();

				var firstSeparator = stateValueLine.IndexOf(';');
				var secondSeparator = firstSeparator + stateValueLine.Substring(firstSeparator + 1).IndexOf(';');

				var valueKey = stateValueLine.Substring(0, firstSeparator);
				var valueType = stateValueLine.Substring(firstSeparator + 1, secondSeparator - firstSeparator);
				var value = stateValueLine.Substring(secondSeparator + 2);

				if (valueType == "BOOL")
					stateValues[valueKey] = bool.Parse(value);
				else if (valueType == "INT")
					stateValues[valueKey] = int.Parse(value);
				else if (valueType == "STRING")
					stateValues[valueKey] = value;
			}

			return LoadGame(reader, version);
		}
		#endregion
	}
}
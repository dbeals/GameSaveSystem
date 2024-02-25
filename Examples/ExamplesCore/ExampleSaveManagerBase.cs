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

using System.Diagnostics;
using GameSaveSystem;

namespace ExamplesCore;

public abstract class ExampleSaveManagerBase<TGameState> : SaveManagerBase
	where TGameState : class, IGameState, new()
{
	#region Properties
	public TGameState CurrentGameState { get; set; }
	#endregion

	#region Constructors
	protected ExampleSaveManagerBase(string rootPath, string autoSaveFileNamePrefix, float autoSaveIntervalInSeconds, int maximumAutoSaveCount, int maximumSafeSaveCount)
		: base(rootPath, autoSaveFileNamePrefix, autoSaveIntervalInSeconds, maximumAutoSaveCount, maximumSafeSaveCount) { }
	#endregion

	#region Methods
	protected abstract void HandleLoadError(string filePath, LoadResult error);

	protected override void OnSaveRequested(SaveType saveType, string fullFilePath)
	{
		using var stream = File.OpenWrite(fullFilePath);
		using var writer = new StreamWriter(stream);
		SaveGame(writer);
	}

	protected override bool OnLoadRequested(string fullFilePath)
	{
		using var stream = File.OpenRead(fullFilePath);
		using var reader = new StreamReader(stream);
		var result = LoadGame(reader);
		if (result == LoadResult.Success)
			return true;

		HandleLoadError(fullFilePath, result);
		return false;
	}

	protected virtual void SaveGame(StreamWriter writer)
	{
		Debug.Assert(writer.BaseStream.Position == 0, "You need to call base.SaveGame() at the top of your override as it writes the header information.");
		writer.WriteLine(FileKey);
		writer.WriteLine(CurrentVersion.ToString());
		CurrentGameState.WriteToStream(writer);
	}

	protected virtual LoadResult LoadGame(StreamReader reader)
	{
		var newGameState = new TGameState();

		if (reader.BaseStream.Length == 0)
			return LoadResult.EmptyFile;

		var fileKey = reader.ReadLine();
		if (fileKey != FileKey)
			return LoadResult.InvalidKey;

		var versionString = reader.ReadLine() ?? string.Empty;
		if (!Version.TryParse(versionString, out var version))
			return LoadResult.InvalidFormat;

		var result = newGameState.ReadFromStream(reader);
		if (result != LoadResult.Success)
			return result;

		CurrentGameState = newGameState;
		return result;
	}
	#endregion
}
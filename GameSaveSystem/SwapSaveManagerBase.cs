﻿// /***********************************************************************
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

namespace GameSaveSystem;

public abstract class SwapSaveManagerBase : ISaveManager
{
	#region Variables
	private float _autoSaveTimeElapsedInSeconds;
	#endregion

	#region Properties
	public abstract string FileExtension { get; }
	public abstract string FileKey { get; }
	public abstract Version CurrentVersion { get; }
	public IEnumerable<SaveFileInfo> SaveFiles => SafeSaveHelper.EnumerateSaveFiles(RootPath, FileExtension);
	public string RootPath { get; set; }
	public string AutoSaveFileNamePrefix { get; set; }
	public float AutoSaveIntervalInSeconds { get; set; }

	public int MaximumAutoSaveCount
	{
		get => 1;
		set => throw new NotSupportedException();
	}

	public int MaximumSafeSaveCount
	{
		get => 1;
		set => throw new NotSupportedException();
	}

	public bool IsAutoSaveEnabled { get; set; }
	public string QuickSaveName { get; set; } = "Quick Save";
	#endregion

	#region Constructors
	protected SwapSaveManagerBase()
		: this("Saves/", "Auto Save", 900f) { }

	protected SwapSaveManagerBase(string rootPath, string autoSaveFileNamePrefix, float autoSaveIntervalInSeconds)
	{
		RootPath = rootPath;
		AutoSaveFileNamePrefix = autoSaveFileNamePrefix;
		AutoSaveIntervalInSeconds = autoSaveIntervalInSeconds;
	}
	#endregion

	#region Methods
	public void SaveGame(string fileNameWithoutExtension)
	{
		SwapSafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(fileNameWithoutExtension, FileExtension), SaveType.Manual, OnSaveRequested);
	}

	public void LoadGame(string fileNameWithoutExtension, bool forceRevert = false)
	{
		SwapSafeSaveHelper.LoadGame(RootPath, SafeSaveHelper.AddFileExtension(fileNameWithoutExtension, FileExtension), forceRevert, OnLoadRequested);
	}

	public void QuickSave()
	{
		SafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(AutoSaveFileNamePrefix, FileExtension), MaximumAutoSaveCount, SaveType.QuickSave, OnSaveRequested);
	}

	public void LoadQuickSave(bool forceRevert = false)
	{
		LoadGame(QuickSaveName, forceRevert);
	}

	public void AutoSave()
	{
		SwapSafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(AutoSaveFileNamePrefix, FileExtension), SaveType.AutoSave, OnSaveRequested);
	}

	public void LoadAutoSave(bool forceRevert = false)
	{
		SafeSaveHelper.LoadGame(RootPath, SafeSaveHelper.AddFileExtension(AutoSaveFileNamePrefix, FileExtension), forceRevert, OnLoadRequested);
	}

	public void Export(string exportFileName, int compressionLevel = 3, string password = null)
	{
		ImportExportHelper.Export(SaveFiles, exportFileName, compressionLevel, password);
	}

	public void Import(string importFileName)
	{
		ImportExportHelper.Import(RootPath, importFileName, entryName => SafeSaveHelper.GetIncrementalFileName(entryName, 1));
	}

	public void Update(float deltaInSeconds)
	{
		if (!IsAutoSaveEnabled)
			return;

		_autoSaveTimeElapsedInSeconds += deltaInSeconds;
		if (!(_autoSaveTimeElapsedInSeconds >= AutoSaveIntervalInSeconds))
			return;

		_autoSaveTimeElapsedInSeconds = 0.0f;
		AutoSave();
	}

	protected abstract void OnSaveRequested(SaveType saveType, string fullFilePath);
	protected abstract bool OnLoadRequested(string fullFilePath);
	#endregion
}
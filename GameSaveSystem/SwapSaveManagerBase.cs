#region File Header
/***********************************************************************
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>
************************************************************************
Author: Donald Beals
Date: April 25th, 2015
Description: This is a simple save-swap save manager with auto-save options.
****************************** Change Log ******************************
4/25/2015 - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

#endregion

namespace GameSaveSystem
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class SwapSaveManagerBase
	{
		#region Variables
		private float autoSaveTimeElapsedInSeconds;
		#endregion

		#region Properties
		public abstract string FileExtension
		{
			get;
		}

		public abstract string FileKey
		{
			get;
		}

		public abstract Version CurrentVersion
		{
			get;
		}

		public string RootPath
		{
			get;
			set;
		}

		public string AutoSaveFileNamePrefix
		{
			get;
			set;
		}

		public float AutoSaveIntervalInSeconds
		{
			get;
			set;
		}

		public bool IsAutoSaveEnabled
		{
			get;
			set;
		}

		public IEnumerable<KeyValuePair<string, string>> SaveFiles
		{
			get
			{
				return SafeSaveHelper.EnumerateSaveFiles(RootPath, FileExtension);
			}
		}
		#endregion

		#region Constructors
		protected SwapSaveManagerBase(string rootPath, string autoSaveFileNamePrefix, float autoSaveIntervalInSeconds)
		{
			RootPath = rootPath;
			AutoSaveFileNamePrefix = autoSaveFileNamePrefix;
			AutoSaveIntervalInSeconds = autoSaveIntervalInSeconds;
		}
		#endregion

		#region Methods
		protected abstract void OnSaveRequested(string fullFilePath);
		protected abstract bool OnLoadRequested(string fullFilePath);

		public void SaveGame(string fileNameWithoutExtension)
		{
			SwapSafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(fileNameWithoutExtension, FileExtension), OnSaveRequested);
		}

		public void LoadGame(string fileNameWithoutExtension, bool forceRevert = false)
		{
			SwapSafeSaveHelper.LoadGame(RootPath, SafeSaveHelper.AddFileExtension(fileNameWithoutExtension, FileExtension), forceRevert, OnLoadRequested);
		}

		public void AutoSave()
		{
			SwapSafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(AutoSaveFileNamePrefix, FileExtension), OnSaveRequested);
		}

		public void Export(string exportFileName, int compressionLevel = 3, string password = null)
		{
			ImportExportHelper.Export(RootPath, SaveFiles, exportFileName, compressionLevel, password);
		}

		public void Import(string importFileName)
		{
			ImportExportHelper.Import(RootPath, importFileName, (entryName) => SafeSaveHelper.GetIncrementalFileName(entryName, 1));
		}

		public void Update(float deltaInSeconds)
		{
			if(!IsAutoSaveEnabled)
				return;

			autoSaveTimeElapsedInSeconds += deltaInSeconds;
			if(autoSaveTimeElapsedInSeconds >= AutoSaveIntervalInSeconds)
			{
				autoSaveTimeElapsedInSeconds = 0.0f;
				AutoSave();
			}
		}
		#endregion
	}
}

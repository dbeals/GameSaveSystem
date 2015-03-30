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
Date: March 29th, 2015
Description: This is a simple auto-save manager.
****************************** Change Log ******************************
3/29/2015 - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
#endregion

namespace GameSaveSystem
{
	public abstract class SaveManagerBase
	{
		#region Variables
		private string rootPath;
		private string autoSaveFileNamePrefix;
		private float autoSaveTimeElapsedInSeconds;
		private float autoSaveIntervalInSeconds;
		private int maximumAutoSaveCount;
		private int maximumSafeSaveCount;
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
			get
			{
				return rootPath;
			}
			set
			{
				rootPath = value;
			}
		}

		public string AutoSaveFileNamePrefix
		{
			get
			{
				return autoSaveFileNamePrefix;
			}
			set
			{
				autoSaveFileNamePrefix = value;
			}
		}

		public float AutoSaveIntervalInSeconds
		{
			get
			{
				return autoSaveIntervalInSeconds;
			}
			set
			{
				autoSaveIntervalInSeconds = value;
			}
		}

		public int MaximumAutoSaveCount
		{
			get
			{
				return maximumAutoSaveCount;
			}
			set
			{
				maximumAutoSaveCount = value;
			}
		}

		public int MaximumSafeSaveCount
		{
			get
			{
				return maximumSafeSaveCount;
			}
			set
			{
				maximumSafeSaveCount = value;
			}
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
		protected SaveManagerBase(string rootPath, string autoSaveFileNamePrefix, float autoSaveIntervalInSeconds, int maximumAutoSaveCount, int maximumSafeSaveCount)
		{
			RootPath = rootPath;
			AutoSaveFileNamePrefix = autoSaveFileNamePrefix;
			AutoSaveIntervalInSeconds = autoSaveIntervalInSeconds;
			MaximumAutoSaveCount = maximumAutoSaveCount;
			MaximumSafeSaveCount = maximumSafeSaveCount;
		}
		#endregion

		#region Methods
		private void CleanseSaveByBaseName(string baseFileName)
		{
			var directoryInfo = new DirectoryInfo(RootPath);
			var files = (from file in directoryInfo.EnumerateFiles(SafeSaveHelper.GetSearchPatternFromFileName(baseFileName))
						 select file).ToArray();

			foreach(var file in files)
				file.Delete();
		}

		protected abstract void OnSaveRequested(string fullFilePath);
		protected abstract bool OnLoadRequested(string fullFilePath);

		public void SaveGame(string fileNameWithoutExtension)
		{
			SafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(fileNameWithoutExtension, FileExtension), MaximumSafeSaveCount, OnSaveRequested);
		}

		public void LoadGame(string fileNameWithoutExtension, bool forceRevert = false)
		{
			SafeSaveHelper.LoadGame(RootPath, SafeSaveHelper.AddFileExtension(fileNameWithoutExtension, FileExtension), forceRevert, OnLoadRequested);
		}

		public void AutoSave()
		{
			SafeSaveHelper.SaveGame(RootPath, SafeSaveHelper.AddFileExtension(AutoSaveFileNamePrefix, FileExtension), MaximumAutoSaveCount, OnSaveRequested);
		}

		public void Export(string exportFileName, int compressionLevel = 3, string password = null)
		{
			using(var fileStream = File.Create(exportFileName))
			{
				using(var outputStream = new ZipOutputStream(fileStream))
				{
					outputStream.SetLevel(compressionLevel);
					outputStream.Password = password;
					foreach(var file in SaveFiles)
					{
						var fileName = file.Key.Replace('\\', '/');
						var fileInfo = new FileInfo(Path.Combine(RootPath, file.Value));
						var entry = new ZipEntry(ZipEntry.CleanName(fileName))
						{
							DateTime = fileInfo.LastWriteTime,
							Size = fileInfo.Length,
						};
						outputStream.PutNextEntry(entry);

						var buffer = new byte[4096];
						using(var streamReader = fileInfo.OpenRead())
						{
							ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(streamReader, outputStream, buffer);
						}
						outputStream.CloseEntry();
					}
				}
			}
		}

		public void Import(string importFileName)
		{
			using(var streamReader = File.OpenRead(importFileName))
			{
				var zipFile = new ZipFile(streamReader);

				foreach(ZipEntry entry in zipFile)
				{
					if(!entry.IsFile)
						continue;

					var entryName = entry.Name;

					var buffer = new byte[4096];
					using(var inputStream = zipFile.GetInputStream(entry))
					{
						var fullZipToPath = Path.Combine(RootPath, SafeSaveHelper.GetIncrementalFileName(entry.Name, 1));
						var directoryName = Path.GetDirectoryName(fullZipToPath);
						if(directoryName.Length > 0)
							Directory.CreateDirectory(directoryName);

						using(var streamWriter = File.Create(fullZipToPath))
						{
							ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(inputStream, streamWriter, buffer);
						}
					}
				}
			}
		}

		public void Update(float deltaInSeconds)
		{
			if(!IsAutoSaveEnabled)
				return;

			autoSaveTimeElapsedInSeconds += deltaInSeconds;
			if(autoSaveTimeElapsedInSeconds >= autoSaveIntervalInSeconds)
			{
				autoSaveTimeElapsedInSeconds = 0.0f;
				AutoSave();
			}
		}
		#endregion
	}
}

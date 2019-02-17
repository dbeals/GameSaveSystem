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
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSaveSystemTests
{
	[TestClass]
	public class ExportImportTest
	{
		#region Methods
		[TestMethod]
		public void ExportTest()
		{
			var saveDirectory = new DirectoryInfo("ExportTest");
			if (saveDirectory.Exists)
			{
				Console.WriteLine("Deleting {0}", saveDirectory.Name);
				saveDirectory.Delete(true);
				Thread.Sleep(10);
			}

			var exportFile = new FileInfo("SaveGameExport.zip");
			if (exportFile.Exists)
				exportFile.Delete();

			saveDirectory.Refresh();
			Assert.IsFalse(saveDirectory.Exists);

			exportFile.Refresh();
			Assert.IsFalse(exportFile.Exists);

			var saveManager = new SaveManager(saveDirectory.Name, false);
			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 27;
			saveManager.SaveGame("ExportTest1");

			saveManager.SaveGame("ExportTest2");
			saveManager.PlayerName = "Donald";
			saveManager.PlayerAge = 28;

			saveManager.SaveGame("ExportTest1");

			var files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 2);
			Assert.AreEqual("ExportTest1.sav", files[0].Key);
			Assert.AreEqual("ExportTest1.2.sav", files[0].Value);
			Assert.AreEqual("ExportTest2.sav", files[1].Key);
			Assert.AreEqual("ExportTest2.1.sav", files[1].Value);

			var exportContents = new List<string>();
			exportContents.Add(files[0].Key);
			exportContents.Add(files[1].Key);

			saveManager.Export("SaveGameExport.zip");
			exportFile.Refresh();
			Assert.IsTrue(File.Exists("SaveGameExport.zip"));

			var zipFile = new ZipFile(exportFile.Name);
			foreach (ZipEntry entry in zipFile)
				Assert.IsTrue(exportContents.Remove(entry.Name));
			Assert.IsTrue(exportContents.Count == 0);
		}
		#endregion
	}
}
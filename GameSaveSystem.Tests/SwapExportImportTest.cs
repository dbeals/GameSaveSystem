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

using ICSharpCode.SharpZipLib.Zip;

namespace GameSaveSystem.Tests;

[TestFixture]
public class SwapExportImportTest
{
	[SetUp]
	public void CreateExport()
	{
		var saveDirectory = new DirectoryInfo("SwapExportTest");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(100);
		}

		saveDirectory.Refresh();
		Assert.IsFalse(saveDirectory.Exists);

		var exportFile = new FileInfo("SwapSaveGameExport.zip");
		if (exportFile.Exists)
			exportFile.Delete();

		exportFile.Refresh();
		Assert.IsFalse(exportFile.Exists);

		_saveManager = new SwapSaveManager(saveDirectory.Name, false)
		{
			PlayerName = "Donny",
			PlayerAge = 27
		};
		_saveManager.SaveGame("ExportTest1");

		_saveManager.SaveGame("ExportTest2");
		_saveManager.PlayerName = "Donald";
		_saveManager.PlayerAge = 28;

		_saveManager.SaveGame("ExportTest1");

		_saveManager.Export(exportFile.Name);
	}

	private SwapSaveManager _saveManager;

	[Test]
	public void ExportTest()
	{
		var files = _saveManager.SaveFiles.ToArray();
		Assert.IsTrue(files.Length == 2);
		Assert.AreEqual("ExportTest1.sav", files[0].Key);
		Assert.AreEqual("ExportTest1.sav", files[0].Value);
		Assert.AreEqual("ExportTest2.sav", files[1].Key);
		Assert.AreEqual("ExportTest2.sav", files[1].Value);

		var exportContents = new List<string>
		{
			files[0].Key,
			files[1].Key
		};

		var exportFile = new FileInfo("SwapSaveGameExport.zip");
		Assert.IsTrue(exportFile.Exists);

		var zipFile = new ZipFile(exportFile.Name);
		foreach (ZipEntry entry in zipFile)
			Assert.IsTrue(exportContents.Remove(entry.Name));
		Assert.IsTrue(exportContents.Count == 0);
		zipFile.Close();
	}

	[Test]
	public void ImportTest()
	{
		var saveDirectory = new DirectoryInfo("SwapExportTest");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(10);
			saveDirectory.Refresh();
		}

		Assert.IsFalse(saveDirectory.Exists);

		var exportFile = new FileInfo("SwapSaveGameExport.zip");
		Assert.IsTrue(exportFile.Exists);
		_saveManager.Import(exportFile.Name);

		saveDirectory.Refresh();
		Assert.AreEqual(2, saveDirectory.EnumerateFiles().Count());
	}
}
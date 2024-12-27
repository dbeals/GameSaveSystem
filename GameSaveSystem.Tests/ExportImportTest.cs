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
public class ExportImportTest
{
	[SetUp]
	public void CreateExport()
	{
		var saveDirectory = new DirectoryInfo("ExportTest");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(100);
		}

		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.False);

		var exportFile = new FileInfo("SaveGameExport.zip");
		if (exportFile.Exists)
			exportFile.Delete();

		exportFile.Refresh();
		Assert.That(exportFile.Exists, Is.False);

		_testSaveManager = new TestSaveManager(saveDirectory.Name, false)
		{
			PlayerName = "Donny",
			PlayerAge = 27
		};
		_testSaveManager.SaveGame("ExportTest1");

		_testSaveManager.SaveGame("ExportTest2");
		_testSaveManager.PlayerName = "Donald";
		_testSaveManager.PlayerAge = 28;

		_testSaveManager.SaveGame("ExportTest1");

		_testSaveManager.Export("SaveGameExport.zip");
	}

	private TestSaveManager _testSaveManager;

	[Test]
	public void ExportTest()
	{
		var files = _testSaveManager.SaveFiles.ToArray();
		Assert.Multiple(() =>
		{
			Assert.That(files, Has.Length.EqualTo(2));
			Assert.That(files[0].SaveGroup, Is.EqualTo("ExportTest1.sav"));
			Assert.That(files[0].FileInfo.Name, Is.EqualTo("ExportTest1.2.sav"));
			Assert.That(files[1].SaveGroup, Is.EqualTo("ExportTest2.sav"));
			Assert.That(files[1].FileInfo.Name, Is.EqualTo("ExportTest2.1.sav"));
		});
		var exportContents = new List<string>
		{
			files[0].SaveGroup,
			files[1].SaveGroup
		};

		var exportFile = new FileInfo("SaveGameExport.zip");
		Assert.That(exportFile.Exists, Is.True);

		var zipFile = new ZipFile(exportFile.Name);
		foreach (ZipEntry entry in zipFile)
			Assert.That(exportContents.Remove(entry.Name), Is.True);
		Assert.That(exportContents, Is.Empty);
		zipFile.Close();
	}

	[Test]
	public void ImportTest()
	{
		var saveDirectory = new DirectoryInfo("ExportTest");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(10);
			saveDirectory.Refresh();
		}

		Assert.That(saveDirectory.Exists, Is.False);

		var exportFile = new FileInfo("SaveGameExport.zip");
		Assert.That(exportFile.Exists, Is.True);
		_testSaveManager.Import(exportFile.Name);

		saveDirectory.Refresh();
		Assert.That(saveDirectory.EnumerateFiles().Count(), Is.EqualTo(2));
	}
}
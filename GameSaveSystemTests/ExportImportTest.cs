using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSaveSystemTests
{
	[TestClass]
	public class ExportImportTest
	{
		[TestMethod]
		public void ExportTest()
		{
			var saveDirectory = new DirectoryInfo("ExportTest");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var exportFile = new FileInfo("SaveGameExport.zip");
			if(exportFile.Exists)
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
			foreach(ZipEntry entry in zipFile)
				Assert.IsTrue(exportContents.Remove(entry.Name));
			Assert.IsTrue(exportContents.Count == 0);
		}
	}
}

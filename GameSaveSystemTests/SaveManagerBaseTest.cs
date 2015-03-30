using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSaveSystemTests
{
	[TestClass]
	public class SaveManagerBaseTest
	{
		[TestMethod]
		public void AutoSaveTest()
		{
			var saveDirectory = new DirectoryInfo("AutoSaveTests");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var saveManager = new SaveManager(saveDirectory.Name, false);
			saveManager.IsAutoSaveEnabled = true;

			saveDirectory.Refresh();
			Assert.IsFalse(saveDirectory.Exists);
			saveManager.Update(450.0f);
			saveDirectory.Refresh();
			Assert.IsFalse(saveDirectory.Exists);
			saveManager.Update(450.0f);
			saveDirectory.Refresh();
			Assert.IsTrue(saveDirectory.Exists);
			Assert.IsTrue(saveManager.SaveFiles.Count() == 1);
		}

		[TestMethod]
		public void SafeSaveTest()
		{
			var saveDirectory = new DirectoryInfo("SafeSaveTests");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var saveManager = new SaveManager(saveDirectory.Name, true);
			saveManager.IsAutoSaveEnabled = false;
			saveDirectory.Refresh();
			Assert.IsFalse(saveDirectory.Exists);

			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 27;
			saveManager.SaveGame("TestSave");
			saveDirectory.Refresh();
			Assert.IsTrue(saveDirectory.Exists);
			var files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.1.sav", files[0].Value);

			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 28;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.2.sav", files[0].Value);

			saveManager.PlayerName = "Donald";
			saveManager.PlayerAge = 29;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.3.sav", files[0].Value);

			Assert.AreEqual("Donald", saveManager.PlayerName);
			Assert.AreEqual(29, saveManager.PlayerAge);
			saveManager.LoadGame("TestSave");
			// We should have reverted here as TestSave.3.sav should be corrupt.
			Assert.AreEqual("Donny", saveManager.PlayerName);
			Assert.AreEqual(28, saveManager.PlayerAge);
		}

		[TestMethod]
		public void ForceRevertTest()
		{
			var saveDirectory = new DirectoryInfo("ForceRevertTests");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var saveManager = new SaveManager(saveDirectory.Name, false);
			saveManager.IsAutoSaveEnabled = false;
			saveDirectory.Refresh();
			Assert.IsFalse(saveDirectory.Exists);

			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 27;
			saveManager.SaveGame("TestSave");
			saveDirectory.Refresh();
			Assert.IsTrue(saveDirectory.Exists);
			var files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.1.sav", files[0].Value);

			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 28;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.2.sav", files[0].Value);

			saveManager.PlayerName = "Donald";
			saveManager.PlayerAge = 29;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.3.sav", files[0].Value);

			Assert.AreEqual("Donald", saveManager.PlayerName);
			Assert.AreEqual(29, saveManager.PlayerAge);
			saveManager.LoadGame("TestSave", true);
			// We should have reverted here we requested it.
			Assert.AreEqual("Donny", saveManager.PlayerName);
			Assert.AreEqual(28, saveManager.PlayerAge);
		}
	}
}

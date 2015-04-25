using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSaveSystemTests
{
	[TestClass]
	public class SwapSaveManagerBaseTest
	{
		[TestMethod]
		public void SwapAutoSaveTest()
		{
			var saveDirectory = new DirectoryInfo("AutoSaveTests");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var saveManager = new SwapSaveManager(saveDirectory.Name, false);
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
		public void SwapSafeSaveTest()
		{
			var saveDirectory = new DirectoryInfo("SwapSaveTests");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var saveManager = new SwapSaveManager(saveDirectory.Name, true);
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
			Assert.AreEqual("TestSave.sav", files[0].Value);
			Assert.IsFalse(File.Exists(Path.Combine(saveDirectory.FullName, "TestSave.sav.bak")));

			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 28;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.sav", files[0].Value);
			Assert.IsTrue(File.Exists(Path.Combine(saveDirectory.FullName, "TestSave.sav.bak")));

			saveManager.PlayerName = "Donald";
			saveManager.PlayerAge = 29;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.sav", files[0].Value);
			Assert.IsTrue(File.Exists(Path.Combine(saveDirectory.FullName, "TestSave.sav.bak")));

			Assert.AreEqual("Donald", saveManager.PlayerName);
			Assert.AreEqual(29, saveManager.PlayerAge);
			saveManager.LoadGame("TestSave");
			// We should have reverted here as TestSave.3.sav should be corrupt.
			Assert.AreEqual("Donny", saveManager.PlayerName);
			Assert.AreEqual(28, saveManager.PlayerAge);
		}

		[TestMethod]
		public void SwapForceRevertTest()
		{
			var saveDirectory = new DirectoryInfo("ForceRevertTests");
			if(saveDirectory.Exists)
			{
				Console.WriteLine(string.Format("Deleting {0}", saveDirectory.Name));
				saveDirectory.Delete(true);
				System.Threading.Thread.Sleep(10);
			}

			var saveManager = new SwapSaveManager(saveDirectory.Name, false);
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
			Assert.AreEqual("TestSave.sav", files[0].Value);
			Assert.IsFalse(File.Exists(Path.Combine(saveDirectory.FullName, "TestSave.sav.bak")));

			saveManager.PlayerName = "Donny";
			saveManager.PlayerAge = 28;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.sav", files[0].Value);
			Assert.IsTrue(File.Exists(Path.Combine(saveDirectory.FullName, "TestSave.sav.bak")));

			saveManager.PlayerName = "Donald";
			saveManager.PlayerAge = 29;
			saveManager.SaveGame("TestSave");
			files = saveManager.SaveFiles.ToArray();
			Assert.IsTrue(files.Length == 1);
			Assert.AreEqual("TestSave.sav", files[0].Key);
			Assert.AreEqual("TestSave.sav", files[0].Value);
			Assert.IsTrue(File.Exists(Path.Combine(saveDirectory.FullName, "TestSave.sav.bak")));

			Assert.AreEqual("Donald", saveManager.PlayerName);
			Assert.AreEqual(29, saveManager.PlayerAge);
			saveManager.LoadGame("TestSave", true);
			// We should have reverted here we requested it.
			Assert.AreEqual("Donny", saveManager.PlayerName);
			Assert.AreEqual(28, saveManager.PlayerAge);
		}
	}
}

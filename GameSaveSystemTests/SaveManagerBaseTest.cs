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

using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSaveSystemTests
{
	[TestClass]
	public class SaveManagerBaseTest
	{
		#region Methods
		[TestMethod]
		public void EmptyConstructorTest()
		{
			var saveManager = new SaveManager();
			Assert.AreEqual("Saves/", saveManager.RootPath);
			Assert.AreEqual("Auto Save", saveManager.AutoSaveFileNamePrefix);
			Assert.AreEqual(900f, saveManager.AutoSaveIntervalInSeconds);
			Assert.AreEqual(3, saveManager.MaximumAutoSaveCount);
			Assert.AreEqual(3, saveManager.MaximumSafeSaveCount);
		}

		[TestMethod]
		public void AutoSaveTest()
		{
			var saveDirectory = new DirectoryInfo("AutoSaveTests");
			if (saveDirectory.Exists)
			{
				saveDirectory.Delete(true);
				Thread.Sleep(10);
			}

			var saveManager = new SaveManager(saveDirectory.Name, false)
			{
				IsAutoSaveEnabled = true
			};

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
			if (saveDirectory.Exists)
			{
				saveDirectory.Delete(true);
				Thread.Sleep(10);
			}

			var saveManager = new SaveManager(saveDirectory.Name, true)
			{
				IsAutoSaveEnabled = false
			};
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
			if (saveDirectory.Exists)
			{
				saveDirectory.Delete(true);
				Thread.Sleep(10);
			}

			var saveManager = new SaveManager(saveDirectory.Name, false)
			{
				IsAutoSaveEnabled = false
			};
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
		#endregion
	}
}
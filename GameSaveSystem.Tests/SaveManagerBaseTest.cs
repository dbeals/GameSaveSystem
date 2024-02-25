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

namespace GameSaveSystem.Tests;

[TestFixture]
public class SaveManagerBaseTest
{
	[Test]
	public void EmptyConstructorTest()
	{
		var saveManager = new TestSaveManager();
		Assert.Multiple(() =>
		{
			Assert.That(saveManager.RootPath, Is.EqualTo("Saves/"));
			Assert.That(saveManager.AutoSaveFileNamePrefix, Is.EqualTo("Auto Save"));
			Assert.That(saveManager.AutoSaveIntervalInSeconds, Is.EqualTo(900f));
			Assert.That(saveManager.MaximumAutoSaveCount, Is.EqualTo(3));
			Assert.That(saveManager.MaximumSafeSaveCount, Is.EqualTo(3));
		});
	}

	[Test]
	public void AutoSaveTest()
	{
		var saveDirectory = new DirectoryInfo("AutoSaveTests");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(10);
		}

		var saveManager = new TestSaveManager(saveDirectory.Name, false)
		{
			IsAutoSaveEnabled = true
		};

		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.False);
		saveManager.Update(450.0f);
		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.False);
		saveManager.Update(450.0f);
		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.True);
		Assert.That(saveManager.SaveFiles.Count(), Is.EqualTo(1));
	}

	[Test]
	public void LoadAutoSaveTest()
	{
		var saveDirectory = new DirectoryInfo("AutoSaveTests");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(10);
		}

		var saveManager = new TestSaveManager(saveDirectory.Name, false)
		{
			IsAutoSaveEnabled = true
		};

		saveDirectory.Refresh();
		saveManager.PlayerName = "Donny";
		saveManager.PlayerAge = 27;
		saveManager.Update(900.0f);
		saveDirectory.Refresh();
		Assert.IsTrue(saveDirectory.Exists);
		Assert.That(saveManager.SaveFiles.Count(), Is.EqualTo(1));

		saveManager.PlayerName = "Donald";
		saveManager.PlayerAge = 30;
		Assert.That(saveManager.PlayerName, Is.EqualTo("Donald"));
		Assert.That(saveManager.PlayerAge, Is.EqualTo(30));
		saveManager.LoadAutoSave();
		Assert.That(saveManager.PlayerName, Is.EqualTo("Donny"));
		Assert.That(saveManager.PlayerAge, Is.EqualTo(27));
	}

	[Test]
	public void SafeSaveTest()
	{
		var saveDirectory = new DirectoryInfo("SafeSaveTests");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(10);
		}

		var saveManager = new TestSaveManager(saveDirectory.Name, true)
		{
			IsAutoSaveEnabled = false
		};
		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.False);

		saveManager.PlayerName = "Donny";
		saveManager.PlayerAge = 27;
		saveManager.SaveGame("TestSave");
		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.True);
		var files = saveManager.SaveFiles.ToArray();
		Assert.That(files, Has.Length.EqualTo(1));
		Assert.That(files[0].Key, Is.EqualTo("TestSave.sav"));
		Assert.That(files[0].Value, Is.EqualTo("TestSave.1.sav"));

		saveManager.PlayerName = "Donny";
		saveManager.PlayerAge = 28;
		saveManager.SaveGame("TestSave");
		files = saveManager.SaveFiles.ToArray();
		Assert.That(files, Has.Length.EqualTo(1));
		Assert.That(files[0].Key, Is.EqualTo("TestSave.sav"));
		Assert.That(files[0].Value, Is.EqualTo("TestSave.2.sav"));

		saveManager.PlayerName = "Donald";
		saveManager.PlayerAge = 29;
		saveManager.SaveGame("TestSave");
		files = saveManager.SaveFiles.ToArray();
		Assert.That(files, Has.Length.EqualTo(1));
		Assert.That(files[0].Key, Is.EqualTo("TestSave.sav"));
		Assert.That(files[0].Value, Is.EqualTo("TestSave.3.sav"));

		Assert.That(saveManager.PlayerName, Is.EqualTo("Donald"));
		Assert.That(saveManager.PlayerAge, Is.EqualTo(29));
		saveManager.LoadGame("TestSave");

		// We should have reverted here as TestSave.3.sav should be corrupt.
		Assert.That(saveManager.PlayerName, Is.EqualTo("Donny"));
		Assert.That(saveManager.PlayerAge, Is.EqualTo(28));
	}

	[Test]
	public void ForceRevertTest()
	{
		var saveDirectory = new DirectoryInfo("ForceRevertTests");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete(true);
			Thread.Sleep(10);
		}

		var saveManager = new TestSaveManager(saveDirectory.Name, false)
		{
			IsAutoSaveEnabled = false
		};
		saveDirectory.Refresh();
		Assert.IsFalse(saveDirectory.Exists);

		saveManager.PlayerName = "Donny";
		saveManager.PlayerAge = 27;
		saveManager.SaveGame("TestSave");
		saveDirectory.Refresh();
		Assert.That(saveDirectory.Exists, Is.True);
		var files = saveManager.SaveFiles.ToArray();
		Assert.That(files, Has.Length.EqualTo(1));
		Assert.That(files[0].Key, Is.EqualTo("TestSave.sav"));
		Assert.That(files[0].Value, Is.EqualTo("TestSave.1.sav"));

		saveManager.PlayerName = "Donny";
		saveManager.PlayerAge = 28;
		saveManager.SaveGame("TestSave");
		files = saveManager.SaveFiles.ToArray();
		Assert.That(files, Has.Length.EqualTo(1));
		Assert.That(files[0].Key, Is.EqualTo("TestSave.sav"));
		Assert.That(files[0].Value, Is.EqualTo("TestSave.2.sav"));

		saveManager.PlayerName = "Donald";
		saveManager.PlayerAge = 29;
		saveManager.SaveGame("TestSave");
		files = saveManager.SaveFiles.ToArray();
		Assert.That(files, Has.Length.EqualTo(1));
		Assert.That(files[0].Key, Is.EqualTo("TestSave.sav"));
		Assert.That(files[0].Value, Is.EqualTo("TestSave.3.sav"));

		Assert.That(saveManager.PlayerName, Is.EqualTo("Donald"));
		Assert.That(saveManager.PlayerAge, Is.EqualTo(29));
		saveManager.LoadGame("TestSave", true);

		// We should have reverted here we requested it.
		Assert.That(saveManager.PlayerName, Is.EqualTo("Donny"));
		Assert.That(saveManager.PlayerAge, Is.EqualTo(28));
	}
}
﻿// /***********************************************************************
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

using static NUnit.Framework.Assert;

namespace GameSaveSystem.Tests;

[TestFixture]
public class SafeSaveHelperTests
{
	[Test]
	public void GetBaseFileNameTest_2Part()
	{
		var result = SafeSaveHelper.GetBaseFileName("FileName.Extension");
		That(result, Is.EqualTo("FileName.Extension"));
	}

	[Test]
	public void GetBaseFileNameTest_3Part()
	{
		var result = SafeSaveHelper.GetBaseFileName("FileName.1.Extension");
		That(result, Is.EqualTo("FileName.Extension"));
	}

	[Test]
	public void GetBaseFileNameTest_Invalid()
	{
		Throws<ArgumentException>(() =>
		{
			_ = SafeSaveHelper.GetBaseFileName("FileName.1.2.Extension");
		});
	}

	[Test]
	public void GetIncrementalFileNameTest_2Part()
	{
		var result = SafeSaveHelper.GetIncrementalFileName("FileName.Extension", 1);
		That(result, Is.EqualTo("FileName.1.Extension"));
	}

	[Test]
	public void GetIncrementalFileNameTest_3Part()
	{
		var result = SafeSaveHelper.GetIncrementalFileName("FileName.1.Extension", 2);
		That(result, Is.EqualTo("FileName.2.Extension"));
	}

	[Test]
	public void GetIncrementalFileNameTest_Invalid()
	{
		Throws<ArgumentException>(() =>
		{
			_ = SafeSaveHelper.GetIncrementalFileName("FileName.1.2.Extension", 3);
		});
	}

	[Test]
	public void IncrementFileNameTest_Normal()
	{
		var result = SafeSaveHelper.IncrementFileName("FileName.1.Extension", 3);
		That(result, Is.EqualTo("FileName.2.Extension"));
	}

	[Test]
	public void IncrementFileNameTest_Wrap()
	{
		var result = SafeSaveHelper.IncrementFileName("FileName.3.Extension", 3);
		That(result, Is.EqualTo("FileName.1.Extension"));
	}

	[Test]
	public void IncrementFileNameTest_TooManyParts()
	{
		Throws<ArgumentException>(() =>
		{
			_ = SafeSaveHelper.IncrementFileName("FileName.1.2.Extension", 3);
		});
	}

	[Test]
	public void IncrementFileNameTest_NonInt()
	{
		Throws<ArgumentException>(() =>
		{
			_ = SafeSaveHelper.IncrementFileName("FileName.One.Extension", 3);
		});
	}

	[Test]
	public void GetSearchPatternFromFileNameTest_2Part()
	{
		var result = SafeSaveHelper.GetSearchPatternFromFileName("FileName.Extension");
		That(result, Is.EqualTo("FileName.*.Extension"));
	}

	[Test]
	public void GetSearchPatternFromFileNameTest_3Part()
	{
		var result = SafeSaveHelper.GetSearchPatternFromFileName("FileName.3.Extension");
		That(result, Is.EqualTo("FileName.*.Extension"));
	}

	[Test]
	public void GetSearchPatternFromFileNameTest_Invalid()
	{
		Throws<ArgumentException>(() =>
		{
			_ = SafeSaveHelper.GetSearchPatternFromFileName("FileName.1.3.Extension");
		});
	}

	[Test]
	public void AddFileExtensionTest_WithDot()
	{
		var result = SafeSaveHelper.AddFileExtension("FileName", ".Extension");
		That(result, Is.EqualTo("FileName.Extension"));
	}

	[Test]
	public void AddFileExtensionTest_WithoutDot()
	{
		var result = SafeSaveHelper.AddFileExtension("FileName", "Extension");
		That(result, Is.EqualTo("FileName.Extension"));
	}

	[Test]
	public void LoadGameTest_DirectoryDoesNotExist()
	{
		var saveDirectory = new DirectoryInfo("NonExistitentSaveFolder");
		if (saveDirectory.Exists)
			saveDirectory.Delete();

		That(SafeSaveHelper.LoadGame(saveDirectory.FullName, "Save.game", false, null), Is.Null);
	}

	[Test]
	public void LoadGameTest_NoSaves()
	{
		var saveDirectory = new DirectoryInfo("EmptySavesFolder");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete();
			Thread.Sleep(10);
			saveDirectory.Create();
		}

		That(SafeSaveHelper.LoadGame(saveDirectory.FullName, "Save.game", false, null), Is.Null);
	}

	[Test]
	public void CleanseSaveByBaseNameTest()
	{
		var saveDirectory = new DirectoryInfo("CleanseFolder");
		if (saveDirectory.Exists)
		{
			saveDirectory.Delete();
			Thread.Sleep(10);
		}

		saveDirectory.Create();

		var file = new FileInfo(Path.Combine(saveDirectory.FullName, "FileName.1.Extension"));
		File.WriteAllText(file.FullName, "FileSave");

		file = new FileInfo(Path.Combine(saveDirectory.FullName, "FileName.2.Extension"));
		File.WriteAllText(file.FullName, "FileSave");

		file = new FileInfo(Path.Combine(saveDirectory.FullName, "FileName.3.Extension"));
		File.WriteAllText(file.FullName, "FileSave");

		That(saveDirectory.EnumerateFiles().Count(), Is.EqualTo(3));
		SafeSaveHelper.CleanseSaveByBaseName(saveDirectory.FullName, "FileName.Extension");
		That(saveDirectory.EnumerateFiles().Count(), Is.EqualTo(0));
	}
}
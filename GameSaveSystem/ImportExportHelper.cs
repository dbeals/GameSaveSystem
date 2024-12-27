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

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace GameSaveSystem;

public static class ImportExportHelper
{
	#region Methods
	public static void Export(IEnumerable<SaveFileInfo> saveFiles, string exportFileName, int compressionLevel = 3, string password = null)
	{
		using var fileStream = File.Create(exportFileName);
		using var outputStream = new ZipOutputStream(fileStream);

		outputStream.SetLevel(compressionLevel);
		outputStream.Password = password;
		foreach (var (saveGroup, fileInfo) in saveFiles)
		{
			var fileName = saveGroup.Replace('\\', '/');
			var entry = new ZipEntry(ZipEntry.CleanName(fileName))
			{
				DateTime = fileInfo.LastWriteTime,
				Size = fileInfo.Length
			};
			outputStream.PutNextEntry(entry);

			var buffer = new byte[4096];
			using (var streamReader = fileInfo.OpenRead())
			{
				StreamUtils.Copy(streamReader, outputStream, buffer);
			}

			outputStream.CloseEntry();
		}
	}

	public static void Import(string rootPath, string importFileName, Func<string, string> getOutputFileName)
	{
		using var streamReader = File.OpenRead(importFileName);
		var zipFile = new ZipFile(streamReader);

		foreach (ZipEntry entry in zipFile)
		{
			if (!entry.IsFile)
				continue;

			var buffer = new byte[4096];
			using var inputStream = zipFile.GetInputStream(entry);
			var fullZipToPath = Path.Combine(rootPath, getOutputFileName(entry.Name));
			var directoryName = Path.GetDirectoryName(fullZipToPath);
			if (directoryName.Length > 0)
				Directory.CreateDirectory(directoryName);

			using var streamWriter = File.Create(fullZipToPath);
			StreamUtils.Copy(inputStream, streamWriter, buffer);
		}

		zipFile.Close();
	}
	#endregion
}
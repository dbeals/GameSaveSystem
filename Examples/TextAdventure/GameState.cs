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
using System.IO;
using ExamplesCore;
using TextAdventure.Data;

namespace TextAdventure;

public sealed class GameState : GameStateBase
{
	#region Properties
	public override string FileExtension => ".gss-ta";
	public override string FileKey => "GSS-EX-TEXTADV";
	public override Version CurrentVersion => new (1, 0);
	public Room CurrentRoom { get; set; }
	public string LastMessage { get; set; }
	public bool LastMessageIsError { get; set; }
	#endregion

	#region Constructors
	public GameState()
		: base("Saves/", "AutoSave-", 0.0f, 3, 3)
	{
		// I set auto save interval to 0 as we won't be calling update anyway in a console game.
	}
	#endregion

	#region Methods
	public void StartGame(string roomKey)
	{
		ClearStateValues();
		ChangeRoom(roomKey);
	}

	public void ChangeRoom(string roomKey)
	{
		CurrentRoom = Rooms.LoadRoom(roomKey);
		SetStateValue(roomKey + "-visited", true);
		CurrentRoom.Entered?.Invoke(this);
	}

	public void Exit()
	{
		CurrentRoom = null;
	}

	protected override void HandleLoadError(string filePath, LoadResult error)
	{
		LastMessageIsError = true;
		LastMessage = "Failed to load save " + filePath + " : " + error;
	}

	protected override void SaveGame(StreamWriter writer)
	{
		base.SaveGame(writer);
		writer.WriteLine(CurrentRoom.Key);
	}

	protected override LoadResult LoadGame(StreamReader reader, Version version)
	{
		var currentRoomKey = reader.ReadLine();
		return (CurrentRoom = Rooms.LoadRoom(currentRoomKey)) == null ? LoadResult.ContentNotFound : LoadResult.Success;
	}
	#endregion
}
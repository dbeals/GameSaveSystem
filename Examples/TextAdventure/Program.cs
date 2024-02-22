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
using System.Linq;

namespace TextAdventure;

internal static class Program
{
	#region Methods
	private static void Main()
	{
		var saveManager = new TextAdventureSaveManager();
		saveManager.StartNewGame("intro");

		while (saveManager.CurrentGameState.CurrentRoom != null)
		{
			Console.Clear();

			TryWriteStateMessage(saveManager);

			Console.WriteLine(ConsoleHelper.FitString(saveManager.CurrentGameState.CurrentRoom.Description ?? "..."));
			Console.WriteLine();

			WriteActions(saveManager);
			WriteMenu(saveManager);

			HandleInput(saveManager);
		}

		Console.WriteLine("Press any key to continue");
		Console.ReadKey(true);
	}

	private static void HandleInput(TextAdventureSaveManager saveManager)
	{
		var state = saveManager.CurrentGameState;
		var key = Console.ReadKey(true);
		if (state.CurrentRoom.CanSave && char.ToUpper(key.KeyChar) == 'S')
		{
			SaveState(saveManager);
			return;
		}

		if (state.CurrentRoom.CanLoad && char.ToUpper(key.KeyChar) == 'L')
		{
			LoadState(saveManager);
			return;
		}

		if (state.CurrentRoom.CanExit && char.ToUpper(key.KeyChar) == 'E')
		{
			saveManager.CurrentGameState.CurrentRoom = null;
			return;
		}

		if (!int.TryParse(key.KeyChar.ToString(), out var keyNumber) || keyNumber < 1 || keyNumber > saveManager.CurrentRoomActions.Length)
		{
			state.LastMessage = $"You must enter a number that is between 1 and {saveManager.CurrentRoomActions.Length}.";
			state.LastMessageIsError = true;
			return;
		}

		saveManager.CurrentRoomActions[keyNumber - 1].Action(saveManager);
	}

	private static void WriteActions(TextAdventureSaveManager saveManager)
	{
		var activeActions = saveManager.CurrentGameState.CurrentRoom.Actions.Where(action => action.StateTest == null || action.StateTest(saveManager)).ToArray();
		saveManager.CurrentRoomActions = activeActions;
		for (var index = 0; index < activeActions.Length; ++index)
			Console.WriteLine($"{index + 1}) {activeActions[index].Description}");
	}

	private static void LoadState(TextAdventureSaveManager saveManager)
	{
		saveManager.LoadGame("TextAdventure");
		saveManager.CurrentGameState.LastMessage = "Game loaded successfully.";
		saveManager.CurrentGameState.LastMessageIsError = false;
	}

	private static void SaveState(TextAdventureSaveManager saveManager)
	{
		saveManager.SaveGame("TextAdventure");
		saveManager.CurrentGameState.LastMessage = "Game saved successfully.";
		saveManager.CurrentGameState.LastMessageIsError = false;
	}

	private static void WriteMenu(TextAdventureSaveManager saveManager)
	{
		Console.WriteLine();
		if (saveManager.CurrentGameState.CurrentRoom.CanSave)
			Console.WriteLine("S) Save");
		if (saveManager.CurrentGameState.CurrentRoom.CanLoad)
			Console.WriteLine("L) Load");
		if (saveManager.CurrentGameState.CurrentRoom.CanExit)
			Console.WriteLine("E) Exit");
	}

	private static void TryWriteStateMessage(TextAdventureSaveManager saveManager)
	{
		if (string.IsNullOrWhiteSpace(saveManager.CurrentGameState.LastMessage))
			return;

		ConsoleHelper.WriteLineInColor(saveManager.CurrentGameState.LastMessageIsError ? ConsoleColor.Red : ConsoleColor.DarkGreen, saveManager.CurrentGameState.LastMessage);
		saveManager.CurrentGameState.LastMessage = string.Empty;
		saveManager.CurrentGameState.LastMessageIsError = false;
	}
	#endregion
}
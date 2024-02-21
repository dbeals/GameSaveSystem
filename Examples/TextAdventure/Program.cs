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
		var state = new GameState();
		state.StartGame("intro");

		while (state.CurrentRoom != null)
		{
			Console.Clear();

			TryWriteStateMessage(state);

			Console.WriteLine(ConsoleHelper.FitString(state.CurrentRoom.Description ?? "..."));
			Console.WriteLine();

			WriteActions(state);
			WriteMenu(state);

			HandleInput(state);
		}

		Console.WriteLine("Press any key to continue");
		Console.ReadKey(true);
	}

	private static void HandleInput(GameState state)
	{
		var key = Console.ReadKey(true);
		if (state.CurrentRoom.CanSave && char.ToUpper(key.KeyChar) == 'S')
		{
			SaveState(state);
			return;
		}

		if (state.CurrentRoom.CanLoad && char.ToUpper(key.KeyChar) == 'L')
		{
			LoadState(state);
			return;
		}

		if (state.CurrentRoom.CanExit && char.ToUpper(key.KeyChar) == 'E')
		{
			state.Exit();
			return;
		}

		if (!int.TryParse(key.KeyChar.ToString(), out var keyNumber) || keyNumber < 1 || keyNumber > state.CurrentRoom.Actions.Length)
		{
			state.LastMessage = "You must enter a number that is between 1 and " + (state.CurrentRoom.Actions.Length + 3) + '.';
			state.LastMessageIsError = true;
			return;
		}

		state.CurrentRoom.Actions[keyNumber - 1].Action(state);
	}

	private static void WriteActions(GameState state)
	{
		var activeActions = state.CurrentRoom.Actions.Where(action => action.StateTest == null || action.StateTest(state)).ToArray();
		for (var index = 0; index < activeActions.Length; ++index)
			Console.WriteLine($"{index + 1}) {activeActions[index].Description}");
	}

	private static void LoadState(GameState state)
	{
		state.LoadGame("TextAdventure");
		state.LastMessage = "Game loaded successfully.";
		state.LastMessageIsError = false;
	}

	private static void SaveState(GameState state)
	{
		state.SaveGame("TextAdventure");
		state.LastMessage = "Game saved successfully.";
		state.LastMessageIsError = false;
	}

	private static void WriteMenu(GameState state)
	{
		Console.WriteLine();
		if (state.CurrentRoom.CanSave)
			Console.WriteLine("S) Save");
		if (state.CurrentRoom.CanLoad)
			Console.WriteLine("L) Load");
		if (state.CurrentRoom.CanExit)
			Console.WriteLine("E) Exit");
	}

	private static void TryWriteStateMessage(GameState state)
	{
		if (string.IsNullOrWhiteSpace(state.LastMessage))
			return;

		ConsoleHelper.WriteLineInColor(state.LastMessageIsError ? ConsoleColor.Red : ConsoleColor.DarkGreen, state.LastMessage);
		state.LastMessage = string.Empty;
		state.LastMessageIsError = false;
	}
	#endregion
}
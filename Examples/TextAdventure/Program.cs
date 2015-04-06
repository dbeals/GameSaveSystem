#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Date: Month Day, Year
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/4/2015 4:16:58 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using System.Linq;

#endregion

namespace TextAdventure
{
	/// <summary>
	/// 
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			var state = new GameState();
			state.StartGame("intro");

			while(state.CurrentRoom != null)
			{
				Console.Clear();
				if(!string.IsNullOrWhiteSpace(state.LastMessage))
				{
					ConsoleHelper.WriteLineInColor(state.LastMessageIsError ? ConsoleColor.Red : ConsoleColor.DarkGreen, state.LastMessage);
					state.LastMessage = string.Empty;
					state.LastMessageIsError = false;
				}

				Console.WriteLine(ConsoleHelper.FitString(state.CurrentRoom.Description ?? "..."));
				Console.WriteLine();

				var index = 0;
				var activeActions = state.CurrentRoom.Actions.Where(action => action.StateTest == null || action.StateTest(state)).ToArray();
				for(; index < activeActions.Length; ++index)
					Console.WriteLine((index + 1).ToString() + ") " + activeActions[index].Description);

				Console.WriteLine();
				if(state.CurrentRoom.CanSave)
					Console.WriteLine("S) Save");
				if(state.CurrentRoom.CanLoad)
					Console.WriteLine("L) Load");
				if(state.CurrentRoom.CanExit)
					Console.WriteLine("E) Exit");

				var key = Console.ReadKey(true);
				if(state.CurrentRoom.CanSave && char.ToUpper(key.KeyChar) == 'S')
				{
					state.SaveGame("TextAdventure");
					state.LastMessage = "Game saved successfully.";
					state.LastMessageIsError = false;
					continue;
				}
				else if(state.CurrentRoom.CanLoad && char.ToUpper(key.KeyChar) == 'L')
				{
					state.LoadGame("TextAdventure");
					state.LastMessage = "Game loaded successfully.";
					state.LastMessageIsError = false;
					continue;
				}
				else if(state.CurrentRoom.CanExit && char.ToUpper(key.KeyChar) == 'E')
				{
					state.Exit();
					continue;
				}

				int keyNumber;
				if(!int.TryParse(key.KeyChar.ToString(), out keyNumber) || keyNumber < 1 || keyNumber > state.CurrentRoom.Actions.Length)
				{
					state.LastMessage = "You must enter a number that is between 1 and " + (state.CurrentRoom.Actions.Length + 3) + '.';
					state.LastMessageIsError = true;
					continue;
				}

				state.CurrentRoom.Actions[keyNumber - 1].Action(state);
			}

			Console.WriteLine("Press any key to continue");
			Console.ReadKey(true);
		}
	}
}

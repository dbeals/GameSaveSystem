#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Date: Month Day, Year
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/4/2015 3:36:02 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;
using ExamplesCore;
#endregion

namespace TextAdventure
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class GameState : GameStateBase
	{
		#region Variables
		#endregion

		#region Properties
		public override string FileExtension
		{
			get
			{
				return ".gss-ta";
			}
		}

		public override string FileKey
		{
			get
			{
				return "GSS-EX-TEXTADV";
			}
		}

		public override Version CurrentVersion
		{
			get
			{
				return new Version(1, 0);
			}
		}

		public Room CurrentRoom
		{
			get;
			set;
		}

		public string LastMessage
		{
			get;
			set;
		}

		public bool LastMessageIsError
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public GameState()
			: base("Saves/", "AutoSave-", 0.0f, 3, 3)
		{
			// I set auto save interval to 0 as we won't be calling update anyway in a console game.
		}
		#endregion

		#region Methods
		#endregion

		protected override void HandleLoadError(string filePath, LoadResult error)
		{
			LastMessageIsError = true;
			LastMessage = "Failed to load save " + filePath + " : " + error.ToString();
		}

		protected override void SaveGame(System.IO.StreamWriter writer)
		{
			base.SaveGame(writer);
			writer.WriteLine(CurrentRoom.Key);
		}

		protected override LoadResult LoadGame(System.IO.StreamReader reader, System.Version version)
		{
			var currentRoomKey = reader.ReadLine();
			if((CurrentRoom = Data.Rooms.LoadRoom(currentRoomKey)) == null)
				return LoadResult.ContentNotFound;

			return LoadResult.Success;
		}

		public void StartGame(string roomKey)
		{
			ClearStateValues();
			ChangeRoom(roomKey);
		}

		public void ChangeRoom(string roomKey)
		{
			CurrentRoom = Data.Rooms.LoadRoom(roomKey);
			SetStateValue(roomKey + "-visited", true);
			if(CurrentRoom.Entered != null)
				CurrentRoom.Entered(this);
		}

		public void Exit()
		{
			CurrentRoom = null;
		}
	}
}

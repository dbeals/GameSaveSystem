#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Date: Month Day, Year
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/4/2015 4:06:07 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System.Collections.ObjectModel;

#endregion

namespace TextAdventure.Data
{
	/// <summary>
	/// 
	/// </summary>
	public static class Rooms
	{
		private sealed class RoomCollection : KeyedCollection<string, Room>
		{
			protected override string GetKeyForItem(Room item)
			{
				return item.Key;
			}
		}

		private static RoomCollection rooms = new RoomCollection();

		static Rooms()
		{
			rooms.Add(new Room("intro", "Outside Cave Entrance", @"You stand at the entrance of a cave. Everyone says it's haunted, but you obviously don't believe them...wait, is that crying?",
				new RoomAction("Enter the cave", gameState => gameState.ChangeRoom("inside-cave1")),
				new RoomAction("Go home", gameState => gameState.ChangeRoom("early-home"))
			));

			rooms.Add(new Room("inside-cave1", "Inside Cave Entrance", @"You stand in a small opening, just inside the cave. It seems to be getting dark outside already, so maybe you should just head home.

There is a path to the north and a path to the east. The exit is behind you.",
				new RoomAction("Go north", gameState => gameState.ChangeRoom("pit1")),
				new RoomAction("Go east", gameState => gameState.ChangeRoom("hallway1")),
				new RoomAction("Go home", gameState => gameState.ChangeRoom("early-home"))
			));

			rooms.Add(new Room("pit1", "Dark Room", @"You stand in a small room which is completely dark. The crying appears to have become quieter here, but you can still hear it. There seems to be a cold draft coming from somewhere in the room.

There is a path to the north and path to the south.",
				new RoomAction("Go north", gameState => gameState.ChangeRoom("pit1-death")),
				new RoomAction("Go back", gameState => gameState.ChangeRoom("inside-cave1")),
				new RoomAction("Use Flashlight", gameState => gameState.ChangeRoom("pit1-lit"), gameState => gameState.StateValueExists("hallway1-visited") ? (bool)gameState.GetStateValue("hallway1-visited") : false)
			));

			rooms.Add(new Room("pit1-death", "Dark Room", @"As you take a step forward, you feel yourself being pushed. You realise where that draft was coming from: there was a large hole in room.

You look up only to see the outline of a small child as you fall into the darkness of the pit.",
				new RoomAction("Start over", gameState => gameState.StartGame("inside-cave1"))
			)
			{
				CanSave = false,
			});

			rooms.Add(new Room("pit1-lit", "Pit Room", @"There is a large pit blocking the way to the north exit. You use the flashlight to peer into the pit, but all you can see is a large red pit at the bottom.",
				new RoomAction("Go back", gameState => gameState.ChangeRoom("pit1-death2"))
			)
			{
				CanSave = false,
			});

			rooms.Add(new Room("pit1-death2", "Dark Room", @"You turn to leave and find yourself impeded by black figure about the size of a child. Startled you take a step back and find yourself falling. The last thing you hear is a child's giggle.",
				new RoomAction("Start over", gameState => gameState.StartGame("inside-cave1"))
			)
			{
				CanSave = false,
			});

			rooms.Add(new Room("hallway1", "Dark Hallway", @"You walk down the dark hallway, tripping over something. You feel around and find a flashlight.

You can continue down the hallway or head back the way you came.",
				new RoomAction("Continue", gameState => gameState.ChangeRoom("bone-room")),
				new RoomAction("Head back", gameState => gameState.ChangeRoom("inside-cave1")),
				new RoomAction("Use Flashlight", gameState => gameState.ChangeRoom("hallway1-lit"))
			));

			rooms.Add(new Room("hallway1-lit", "Hallway", @"You look around with the flashlight you just found. You see nothing as you peer behind you, but as your gaze returns to the hallway ahead, you see what appears to be a lot of bones. Standing on top of them, you see the dark outline of a child.",
				new RoomAction("Escape", gameState => gameState.ChangeRoom("hallway1-death"))
			)
			{
				RoomActionsOnly = true
			});

			rooms.Add(new Room("hallway1-death", "Hallway", @"You turn to run, but it's too late. Whatever that thing was tackles you to the ground. The last thing you hear is the giggle of a small child.",
				new RoomAction("Start over", gameState => gameState.StartGame("inside-cave1"))
			)
			{
				CanSave = false,
			});

			rooms.Add(new Room("bone-room", "Dark Room", @"The crying seems to be getting quieter, but you're sure it's coming from this direction. You hear a small crunch as you walk into the room and then crying stops all together...replaced with the giggling of a small child. It's the last sound you hear as you feel a sharp pain in your back and fall to the floor.",
				new RoomAction("Start over", gameState => gameState.StartGame("inside-cave1"))
			)
			{
				CanSave = false,
			});

			rooms.Add(new Room("early-home", "Home", @"You decide the best choice is to just leave it alone and head home. You may never know what adventure you missed out on, but maybe that's for the best.",
				new RoomAction("Start over", gameState => gameState.ChangeRoom("intro"))
			));
		}

		public static Room LoadRoom(string roomKey)
		{
			if(!rooms.Contains(roomKey))
				return null;
			return rooms[roomKey];
		}
	}
}

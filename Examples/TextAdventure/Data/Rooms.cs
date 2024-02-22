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

using System.Collections.ObjectModel;

namespace TextAdventure.Data;

public static class Rooms
{
	#region Nested types
	private sealed class RoomCollection : KeyedCollection<string, Room>
	{
		#region Methods
		protected override string GetKeyForItem(Room item) => item.Key;
		#endregion
	}
	#endregion

	#region Variables
	private static readonly RoomCollection _rooms = [];
	#endregion

	#region Constructors
	static Rooms()
	{
		_rooms.Add(new Room("intro", "Outside Cave Entrance", "You stand at the entrance of a cave. Everyone says it's haunted, but you obviously don't believe them...wait, is that crying?",
			new RoomAction("Enter the cave", saveManager => saveManager.CurrentGameState.ChangeRoom("inside-cave1")),
			new RoomAction("Go home", saveManager => saveManager.CurrentGameState.ChangeRoom("early-home"))
		));

		_rooms.Add(new Room("inside-cave1", "Inside Cave Entrance", @"You stand in a small opening, just inside the cave. It seems to be getting dark outside already, so maybe you should just head home.

There is a path to the north and a path to the east. The exit is behind you.",
			new RoomAction("Go north", saveManager => saveManager.CurrentGameState.ChangeRoom("pit1")),
			new RoomAction("Go east", saveManager => saveManager.CurrentGameState.ChangeRoom("hallway1")),
			new RoomAction("Go home", saveManager => saveManager.CurrentGameState.ChangeRoom("early-home"), saveManager => !saveManager.CurrentGameState.StateValueExists("hallway1-visited") || (bool)saveManager.CurrentGameState.GetStateValue("hallway1-visited") == false),
			new RoomAction("Go home", saveManager => saveManager.CurrentGameState.ChangeRoom("early-home-flashlight"), saveManager => saveManager.CurrentGameState.StateValueExists("hallway1-visited") && (bool)saveManager.CurrentGameState.GetStateValue("hallway1-visited"))
		));

		_rooms.Add(new Room("pit1", "Dark Room", @"You stand in a small room which is completely dark. The crying appears to have become quieter here, but you can still hear it. There seems to be a cold draft coming from somewhere in the room.

There is a path to the north and path to the south.",
			new RoomAction("Go north", saveManager => saveManager.CurrentGameState.ChangeRoom("pit1-death")),
			new RoomAction("Go back", saveManager => saveManager.CurrentGameState.ChangeRoom("inside-cave1")),
			new RoomAction("Use flashlight", saveManager => saveManager.CurrentGameState.ChangeRoom("pit1-lit"), saveManager => saveManager.CurrentGameState.StateValueExists("hallway1-visited") && (bool)saveManager.CurrentGameState.GetStateValue("hallway1-visited"))
		));

		_rooms.Add(new Room("pit1-death", "Dark Room", @"As you take a step forward, you feel yourself being pushed. You realise where that draft was coming from: there was a large hole in room.

You look up only to see the outline of a small child as you fall into the darkness of the pit.",
			new RoomAction("Start over", saveManager => saveManager.StartNewGame("inside-cave1"))
		)
		{
			CanSave = false
		});

		_rooms.Add(new Room("pit1-lit", "Pit Room", "There is a large pit blocking the way to the north exit. You use the flashlight to peer into the pit, but all you can see is a large red pit at the bottom.",
			new RoomAction("Go back", saveManager => saveManager.CurrentGameState.ChangeRoom("pit1-death2"))
		)
		{
			CanSave = false
		});

		_rooms.Add(new Room("pit1-death2", "Dark Room", "You turn to leave and find yourself impeded by black figure about the size of a child. Startled you take a step back and find yourself falling. The last thing you hear is a child's giggle.",
			new RoomAction("Start over", saveManager => saveManager.StartNewGame("inside-cave1"))
		)
		{
			CanSave = false
		});

		_rooms.Add(new Room("hallway1", "Dark Hallway", @"You walk down the dark hallway, tripping over something. You feel around and find a flashlight.

You can continue down the hallway or head back the way you came.",
			new RoomAction("Continue", saveManager => saveManager.CurrentGameState.ChangeRoom("bone-room")),
			new RoomAction("Head back", saveManager => saveManager.CurrentGameState.ChangeRoom("inside-cave1")),
			new RoomAction("Use flashlight", saveManager => saveManager.CurrentGameState.ChangeRoom("hallway1-lit"))
		));

		_rooms.Add(new Room("hallway1-lit", "Hallway", "You look around with the flashlight you just found. You see nothing as you peer behind you, but as your gaze returns to the hallway ahead, you see what appears to be a lot of bones. Standing on top of them, you see the dark outline of a child.",
			new RoomAction("Escape", saveManager => saveManager.CurrentGameState.ChangeRoom("hallway1-death"))
		)
		{
			RoomActionsOnly = true
		});

		_rooms.Add(new Room("hallway1-death", "Hallway", "You turn to run, but it's too late. Whatever that thing was tackles you to the ground. The last thing you hear is the giggle of a small child.",
			new RoomAction("Start over", saveManager => saveManager.StartNewGame("inside-cave1"))
		)
		{
			CanSave = false
		});

		_rooms.Add(new Room("bone-room", "Dark Room", "The crying seems to be getting quieter, but you're sure it's coming from this direction. You hear a small crunch as you walk into the room and then crying stops all together...replaced with the giggling of a small child. It's the last sound you hear as you feel a sharp pain in your back and fall to the floor.",
			new RoomAction("Start over", saveManager => saveManager.StartNewGame("inside-cave1"))
		)
		{
			CanSave = false
		});

		_rooms.Add(new Room("early-home", "Home", "You decide the best choice is to just leave it alone and head home. You may never know what adventure you missed out on, but maybe that's for the best.",
			new RoomAction("Start over", saveManager => saveManager.StartNewGame("intro"))
		));

		_rooms.Add(new Room("early-home-flashlight", "Home", "You decide the best choice is to just leave it alone and head home. You may never know what adventure you missed out on, but at least you got a flashlight out of it!",
			new RoomAction("Start over", saveManager => saveManager.StartNewGame("intro"))
		));
	}
	#endregion

	#region Methods
	public static Room LoadRoom(string roomKey) => !_rooms.Contains(roomKey) ? null : _rooms[roomKey];
	#endregion
}
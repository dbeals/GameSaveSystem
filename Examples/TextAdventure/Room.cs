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

namespace TextAdventure;

public sealed class Room
{
	#region Properties
	public string Key { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public Action<GameState> Entered { get; set; }
	public RoomAction[] Actions { get; set; }
	public bool CanSave { get; set; }
	public bool CanLoad { get; set; }
	public bool CanExit { get; set; }

	public bool RoomActionsOnly
	{
		get => CanSave == CanLoad && CanSave == CanExit;
		set => CanSave = CanLoad = CanExit = value;
	}
	#endregion

	#region Constructors
	public Room(string key, string name, string description, params RoomAction[] actions)
		: this(key, name, description, null, actions) { }

	public Room(string key, string name, string description, Action<GameState> entered, params RoomAction[] actions)
	{
		Key = key;
		Name = name;
		Description = description;
		Entered = entered;
		Actions = actions;
		CanSave = true;
		CanLoad = true;
		CanExit = true;
	}
	#endregion
}
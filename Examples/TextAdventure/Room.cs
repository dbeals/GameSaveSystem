#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Date: Month Day, Year
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/4/2015 4:00:03 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements
using System;

#endregion

namespace TextAdventure
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class Room
	{
		#region Variables
		#endregion

		#region Properties
		public string Key
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public Action<GameState> Entered
		{
			get;
			set;
		}

		public RoomAction[] Actions
		{
			get;
			set;
		}

		public bool CanSave
		{
			get;
			set;
		}

		public bool CanLoad
		{
			get;
			set;
		}

		public bool CanExit
		{
			get;
			set;
		}

		public bool RoomActionsOnly
		{
			get
			{
				return CanSave == CanLoad && CanSave == CanExit;
			}
			set
			{
				CanSave = CanLoad = CanExit = value;
			}
		}
		#endregion

		#region Constructors
		public Room(string key, string name, string description, params RoomAction[] actions)
			: this(key, name, description, null, actions)
		{
		}

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

		#region Methods
		#endregion
	}
}

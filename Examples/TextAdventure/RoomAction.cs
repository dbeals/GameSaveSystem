#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Date: Month Day, Year
Description: TODO: Write a description of this file here.
****************************** Change Log ******************************
4/4/2015 4:00:29 PM - Created initial file. (dbeals)
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
	public sealed class RoomAction
	{
		#region Variables
		#endregion

		#region Properties
		public string Description
		{
			get;
			set;
		}

		public Action<GameState> Action
		{
			get;
			set;
		}

		public Func<GameState, bool> StateTest
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public RoomAction()
		{
		}

		public RoomAction(string description, Action<GameState> action, Func<GameState, bool> stateTest = null)
		{
			Description = description;
			Action = action;
			StateTest = stateTest;
		}
		#endregion

		#region Methods
		#endregion
	}
}

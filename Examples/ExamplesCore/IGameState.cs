#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Description: The simple game state class used in our example(s).
****************************** Change Log ******************************
4/3/2015 1:50:02 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements

#endregion

namespace ExamplesCore
{
	/// <summary>
	/// 
	/// </summary>
	public interface IGameState
	{
		#region Events
		#endregion

		#region Properties
		#endregion

		#region Methods
		void SetStateValue(string key, object value);
		object GetStateValue(string key);
		bool StateValueExists(string key);
		#endregion
	}
}

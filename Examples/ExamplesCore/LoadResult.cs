#region File Header
/***********************************************************************
 * Copyright © 2015 Beals Software
 * All Rights Reserved
************************************************************************
Author: Donald Beals
Description: An enumeration that defines the result of a load operation.
****************************** Change Log ******************************
4/3/2015 2:07:02 PM - Created initial file. (dbeals)
***********************************************************************/
#endregion

#region Using Statements

#endregion

namespace ExamplesCore
{
	/// <summary>
	/// 
	/// </summary>
	public enum LoadResult
	{
		Success = 0,
		EmptyFile,
		InvalidKey,
		UnsupportedVersion,
		InvalidFormat,
		ContentNotFound,
		UnknownError,
	}
}

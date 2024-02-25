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

namespace ExamplesCore;

public abstract class GameStateBase : IGameState
{
	#region Variables
	private readonly Dictionary<string, object> _stateValues = new ();
	#endregion

	#region Methods
	public void SetStateValue(string key, object value)
	{
		// I know I could simply make 3 overloads here that take the different types
		// However, I find this to be a better solution as we'll more than likely need
		// other types eventually. I simply didn't implement serializing them to and from strings.
		if (value is not (bool or int or string))
			throw new ArgumentException("The game state system only supports boolean, integer, and string state values.", nameof(value));
		_stateValues[key] = value;
	}

	public object GetStateValue(string key) => _stateValues[key];
	public bool StateValueExists(string key) => _stateValues.ContainsKey(key);

	public void ClearStateValues()
	{
		_stateValues.Clear();
	}

	public virtual void WriteToStream(StreamWriter writer)
	{
		writer.WriteLine(_stateValues.Count);
		foreach (var pair in _stateValues)
		{
			writer.Write(pair.Key);
			switch (pair.Value)
			{
				case bool boolValue:
				{
					writer.Write(";BOOL;");
					writer.WriteLine(boolValue);
					break;
				}

				case int intValue:
				{
					writer.Write(";INT;");
					writer.WriteLine(intValue);
					break;
				}

				case string stringValue:
				{
					writer.Write(";STRING;");
					writer.WriteLine(stringValue);
					break;
				}
			}
		}
	}

	public virtual LoadResult ReadFromStream(StreamReader reader)
	{
		var stateValueCountString = reader.ReadLine();
		if (!int.TryParse(stateValueCountString, out var stateValueCount))
			return LoadResult.InvalidFormat;

		_stateValues.Clear();
		for (var index = 0; index < stateValueCount; ++index)
		{
			var stateValueLine = reader.ReadLine() ?? string.Empty;

			var firstSeparator = stateValueLine.IndexOf(';');
			var secondSeparator = firstSeparator + stateValueLine.Substring(firstSeparator + 1).IndexOf(';');

			var valueKey = stateValueLine.Substring(0, firstSeparator);
			var valueType = stateValueLine.Substring(firstSeparator + 1, secondSeparator - firstSeparator);
			var value = stateValueLine.Substring(secondSeparator + 2);

			_stateValues[valueKey] = valueType switch
			{
				"BOOL" => bool.Parse(value),
				"INT" => int.Parse(value),
				"STRING" => value,
				_ => _stateValues[valueKey]
			};
		}

		return LoadResult.Success;
	}
	#endregion
}
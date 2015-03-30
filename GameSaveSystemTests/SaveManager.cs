using System;
using System.IO;
using GameSaveSystem;

namespace GameSaveSystemTests
{
	public sealed class SaveManager : SaveManagerBase
	{
		#region Variables
		private int saveCount = 0;
		private bool needsToFail = false;

		// We'll store our variables right here, but normally they'd go in a GameState class or such.
		public string PlayerName;
		public int PlayerAge;
		#endregion

		#region Properties
		public override string FileExtension
		{
			get
			{
				return ".sav";
			}
		}

		public override string FileKey
		{
			get
			{
				return "SAVTEST";
			}
		}

		public override Version CurrentVersion
		{
			get
			{
				return new Version(1, 0);
			}
		}
		#endregion

		#region Constructors
		public SaveManager(string rootPath, bool needsToFail)
			: base(rootPath, "AutoSave", 900.0f, 10, 3)
		{
			this.needsToFail = needsToFail;
		}
		#endregion

		#region Methods
		protected override void OnSaveRequested(string fullFilePath)
		{
			++saveCount;
			using(var stream = File.OpenWrite(fullFilePath))
			{
				using(var writer = new StreamWriter(stream))
				{
					writer.WriteLine(FileKey);
					writer.WriteLine(CurrentVersion.ToString());
					writer.WriteLine(PlayerName);

					if(needsToFail == false || saveCount < 3)
					{
						Console.WriteLine(string.Format("Creating a valid save ({0}).", fullFilePath));
						writer.Write(PlayerAge);
					}
					else
						Console.WriteLine(string.Format("Creating an invalid save ({0}).", fullFilePath));
				}
			}
		}

		protected override bool OnLoadRequested(string fullFilePath)
		{
			try
			{
				using(var stream = File.OpenRead(fullFilePath))
				{
					using(var reader = new StreamReader(stream))
					{
						var key = reader.ReadLine();
						if(key != FileKey)
							return false;

						var version = Version.Parse(reader.ReadLine());
						if(version != CurrentVersion)
							return false;

						PlayerName = reader.ReadLine();
						PlayerAge = int.Parse(reader.ReadLine());
						return true;
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(string.Format("Unfortunately we were unable to load '{0}'. We'll try a previous version if it's available.", fullFilePath));
				Console.WriteLine(e.Message);
				return false;
			}
		}
		#endregion
	}
}

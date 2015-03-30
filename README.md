# GameSaveSystem
A simple game save system which has automated support for incremental auto-saves, "safe saving", and import/export.

# Features
The main idea behind this system is preventing complete save-game loss. To do this, we implement a simple incremental save system. This way, in any case of a save failure, the system can simply revert to a previous version of the same save.

# Getting Started
Simply derive from the SaveManagerBase class, implement the abstract members and you're off and running. We will get an example solution created, but for now the GameSaveSystemTests/SaveManager.cs is a starting point.

Once you have your derived class, you simply need to make your save/load calls:
```CSharp
// Inside your main game engine class
MySaveManager saveManager = new MySaveManager("Saves/");

// When you want to auto-save, if you're not using the automatic auto-save.
saveManager.AutoSave();

// When the player has asked to save the game and entered a save game name.
saveManager.SaveGame(userFileName); // The system will automatically handling the numeric index of the file.

// When the player has requested to load a save.
saveManager.LoadGame(userFileName); // The system will load the most recent version of the save.

// When the user has requested to export their saves.
// *NOTE: This only backs up the most recent version; not all versions.*
saveManager.Export(userExportZipFileName);

// When the user has requested to import their saves.
// *NOTE: Make sure to note to your users that this will delete any versions if there is a file name collision.*
saveManager.Import(userImportZipFileName);
```

# Important Notes
It is important to note that the export features will only back up the most recent version of the save and does so without an index value. So, if you have 3 versions of MySave.sav (MySave.1.sav, MySave.2.sav, and MySave.3.sav) and MySave.2.sav is the most recent, it will only export MySave.2.sav and it will be exported as MySave.sav.

On the opposite end, the import feature cleanses the directory of matching files. So, in the same situation above where you have 3 versions of MySave.sav in your saves folder, if you import MySave.sav those 3 versions will be deleted and the new one will be imported as MySave.1.sav.
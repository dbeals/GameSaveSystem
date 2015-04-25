# GameSaveSystem
**Please note that this library is still in infancy, so we may have major structure changes as we add features.**

A simple game save system with the following features:

+ Incremental auto-saves
+ "Safe saving" using incremental or "save and swap" implementations
+ Import/export system

If you are not interested in the structure provided (SaveManagerBase and such), the actual implementation can be found in the helper classes (SafeSaveHelper, SwapSafeSaveHelper, etc.) so that you can utilize the system in your own structure.

# Features
The main idea behind this system is preventing complete save-game loss (due to power loss, corruption, etc.) There are currently two implementations supported:

**Incremental Saves**

The incremental implementation works by saving to a new file in the backend. You specify the maximum index and the library works it's way through them. For example, you can call MySaveManager.SaveGame('MyNewSave.sav') and the first time it will create 'MyNewSave.1.sav', followed by 'MyNewSave.2.sav', etc. You would load using the base name, MyNewSave.sav, and it will work through them, trying to load a save (ordered by the last write time.)

**Save and Swap**

The save and swap implementation works by:

+ Saving to a to a .new file (i.e. MyNewSave.sav.new)
+ Renaming the existing file, if one exists, with a .bak extension (i.e. MyNewSave.sav.bak)
+ Renaming the new file to the proper save (i.e. MyNewSave.sav)


# Getting Started
Simply derive from the SaveManagerBase class, implement the abstract members and you're off and running. We've created a simple text-based game that utilizes the incremental safe-save system. You can find it in the Examples directory; the base implementation is in ExamplesCore/GameStateBase.cs and the game's derived version is in TextAdventure/GameState.cs.

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
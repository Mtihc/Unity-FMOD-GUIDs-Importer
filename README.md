# Unity FMOD GUIDs Importer
1. Import these classes into your Unity Project `Assets` folder
1. Export GUIDs from FMOD via `File -> Export GUIDs...`
1. Place the `GUIDs.txt` file in your Unity Project `Assets` folder
1. The [FMODGuidsImporter](./Assets/Mitch/Editor/AssetImporters/FMODGuidsImporter.cs) will automatically generate a class, containing all GUIDs as `public static readonly` fields.

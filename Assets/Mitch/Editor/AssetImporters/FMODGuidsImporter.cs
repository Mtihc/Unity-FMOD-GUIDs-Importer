using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Globalization;
using System.CodeDom;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using Mitch.Editor.ClassGenerators;

namespace Mitch.Editor.AssetImporters 
{
	/// <summary>
	/// This importer will automatically generate a class file when a .txt asset with the name GUIDs*.txt is imported.
	/// </summary>
	class FMODGuidsImporter : AssetPostprocessor {


		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (string assetPath in importedAssets.Where(IsValidAssetPath))
			{
				Debug.LogFormat("{0}: Reimported txt asset \"{1}\"", typeof(FMODGuidsImporter).Name, assetPath);
				var classAssetPath = TxtToClassAssetPath(assetPath);

				Debug.LogFormat("{0}: Reimported C# class \"{1}\"", typeof(FMODGuidsImporter).Name, classAssetPath);
				FMODGuidsClassGenerator.Write(assetPath, classAssetPath);
				AssetDatabase.ImportAsset(classAssetPath);


			}
			foreach (string assetPath in deletedAssets.Where(IsValidAssetPath)) 
			{
				Debug.LogFormat("{0}: Deleted txt asset \"{1}\"", typeof(FMODGuidsImporter).Name, assetPath);

				var classAssetPath = TxtToClassAssetPath(assetPath);

				if (EditorUtility.DisplayDialog(
					string.Format("Delete C# class?"),
					string.Format("Deleted txt asset \"{0}\". Do you also want to delete the C# class \"{1}\"?", assetPath, classAssetPath), "Yes", "No")) {

					if (AssetDatabase.DeleteAsset(classAssetPath)) {
						Debug.LogFormat("{0}: Deleted C# class \"{1}\"", typeof(FMODGuidsImporter).Name, classAssetPath);
					}
				}
			}

			for (int i=0; i<movedAssets.Length; i++)
			{
				var oldPath = movedFromAssetPaths[i];
				var newPath = movedAssets[i];

				if (!IsValidAssetPath(oldPath)) continue;
				Debug.LogFormat("{0}: Moved txt asset \"{1}\" to \"{2}\"", typeof(FMODGuidsImporter).Name, oldPath, newPath);

				var oldClassPath = TxtToClassAssetPath(oldPath);
				var newClassPath = TxtToClassAssetPath(newPath);
				var error = AssetDatabase.MoveAsset(oldClassPath, newClassPath);
				if (error != null) {
					Debug.LogWarningFormat("{0}: Failed to move C# class \"{1}\" to \"{2}\". Error: \"{3}\"", typeof(FMODGuidsImporter).Name, oldClassPath, newClassPath, error);
				}
				else {
					Debug.LogFormat("{0}: Moved C# class \"{1}\" to \"{2}\"", typeof(FMODGuidsImporter).Name, oldClassPath, newClassPath);
				}
			}
		}

		private static bool IsValidAssetPath(string assetPath) {
			const string pattern = @".*GUIDs.*\.txt\b";
			return Regex.IsMatch(assetPath, pattern, RegexOptions.Singleline);
		}

		private static string TxtToClassAssetPath(string assetPath) {
			const string pattern = @"(.*\/)(.+)(.txt)";
			var match = Regex.Match(assetPath, pattern, RegexOptions.Singleline);
			var className = FMODGuidsClassGenerator.StringToValidMemberName("FMOD" + match.Groups[2].Value);
			return match.Groups[1].Value + className + ".cs";
		}
	}


}
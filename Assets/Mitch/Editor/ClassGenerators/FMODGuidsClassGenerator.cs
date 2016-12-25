using System.Text.RegularExpressions;

namespace Mitch.Editor.ClassGenerators
{
	/// <summary>
	/// FMOD guids class generator. Used by class <see cref="Mitch.Editor.AssetImporters.FMODGuidsImporter"/>
	/// </summary>
	public class FMODGuidsClassGenerator : ConstantsClassGenerator<System.Guid> {

		/// <summary>
		/// Initializes a new instance of the <see cref="Mitch.Editor.ClassGenerators.FMODGuidsClassGenerator"/> class.
		/// </summary>
		/// <param name="className">The class name</param>
		public FMODGuidsClassGenerator(string className) : base(className) { }

		/// <summary>
		/// Write the class file.
		/// </summary>
		/// <param name="sourceAssetPath">Source .txt asset path.</param>
		/// <param name="targetAssetPath">Target .cs asset path.</param>
		public static void Write(string sourceAssetPath, string targetAssetPath) {

			// extract class name from .cs file path
			var className = Regex.Match(targetAssetPath, @".*\/(.+).cs", RegexOptions.Singleline).Groups[1].Value;

			// create valid class name
			className = StringToValidMemberName(className);

			// create generator
			var generator = new FMODGuidsClassGenerator(className);

			// read txt file
			var text = System.IO.File.ReadAllText(sourceAssetPath);

			// extract key-value pairs from .txt file
			const string keyValuePattern = @"{(.*)} (event:.*)\r\n";
			foreach (Match match in Regex.Matches(text, keyValuePattern, RegexOptions.IgnoreCase | RegexOptions.Multiline)) {

				// the key
				var eventRef = match.Groups[2].Value;
				// the value
				var guid = new System.Guid(match.Groups[1].Value);

				// add key-value pair to generator
				generator[eventRef] = guid;
			}

			// generate class
			generator.WriteToFile(targetAssetPath);
		}

		public override string KeyToFieldName (string key)
		{
			const string pattern = @"event:.*\/(.*)";
			var value = Regex.Match(key, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
			return StringToValidMemberName(value);
		}
	}
}
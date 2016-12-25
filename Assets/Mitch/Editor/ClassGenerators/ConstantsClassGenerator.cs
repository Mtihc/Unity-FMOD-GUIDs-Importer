using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Mitch.Editor.ClassGenerators
{
	/// <summary>
	/// 
	/// Constants class generator.
	/// 
	/// Provide a class name and a dictionary of key-value pairs.
	/// Then call <code>WriteToFile</code> to generate a .cs file.
	/// 
	/// Override <code>KeyToFieldName</code> and <code>GetCodeSnippet</code> to change the output.
	/// 
	/// </summary>
	public class ConstantsClassGenerator<T> {

		public ConstantsClassGenerator(string className) {
			this.ClassName = className;
		}

		private string _className;
		public string ClassName { 
			get { return _className; } 
			set { _className = StringToValidMemberName(value); } 
		}

		private Dictionary<string, T> _dict = new Dictionary<string, T>();
		public T this[string key] {
			get { return _dict[key]; }
			set { _dict[key] = value; }
		}

		public virtual string KeyToFieldName (string key) {
			return StringToValidMemberName(key);
		}

		public virtual CodeSnippetTypeMember GetCodeSnippet(string key, T value) {
			var text = string.Format(
				"public static readonly {0} {1} = new {0}(\"{2}\");\r\n", 
				value.GetType().Name, KeyToFieldName(key), value);

			var result = new CodeSnippetTypeMember(text);
			result.Comments.Add(new CodeCommentStatement(key));
			return result;
		}

		public void WriteToFile(string file) {

			CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

			// create class
			var targetClass = new CodeTypeDeclaration(ClassName);
			targetClass.IsClass = true;
			targetClass.Attributes = MemberAttributes.Public;


			targetClass.StartDirectives.Add(new CodeRegionDirective(
				CodeRegionMode.Start, System.Environment.NewLine + "\r\nstatic"));

			targetClass.EndDirectives.Add(new CodeRegionDirective(
				CodeRegionMode.End, string.Empty));

			foreach (KeyValuePair<string, T> item in _dict) {

				var snippet = GetCodeSnippet(item.Key, item.Value);
				targetClass.Members.Add(snippet);

			}

			// add the class to Code Compile Unit
			var targetUnit = new CodeCompileUnit();
			CodeNamespace ns = new CodeNamespace();
			ns.Imports.Add(new CodeNamespaceImport(typeof(T).Namespace));
			ns.Types.Add(targetClass);
			targetUnit.Namespaces.Add(ns);
			// write .cs file
			CodeGeneratorOptions options = new CodeGeneratorOptions();
			options.BracingStyle = "C";
			using (StreamWriter sourceWriter = new StreamWriter(file))
			{
				provider.GenerateCodeFromCompileUnit(
					targetUnit, sourceWriter, options);
			}
		}


		public static string StringToValidMemberName(string value)
		{
			bool isValid = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#").IsValidIdentifier(value);

			if (!isValid)
			{ 
				// File name contains invalid chars, remove them
				Regex regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");
				value = regex.Replace(value, "");

				// Class name doesn't begin with a letter, insert an underscore
				if (!char.IsLetter(value, 0))
				{
					value = value.Insert(0, "_");
				}
			}

			return value.Replace(" ", string.Empty);
		}
	}
}
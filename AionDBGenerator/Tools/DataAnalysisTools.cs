// -----------------------------------------------------------------------
// <copyright file="DataAnalysisTools.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AionDBGenerator.Tools {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.IO;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public static class DataAnalysisTools {

		class DataType {
			public string type;
			public HashSet<string> examples;
		}
		public static void WriteItemsCS(AionData data) {

			Dictionary<string, DataType> itemAttrs = new Dictionary<string, DataType>();

			Regex enumtype = new Regex("^[a-z][0-9a-z_]*(, [a-z][0-9a-z_]*)*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			var itemsxml = data.ReadXMLFile(@"Data\Items\Items.pak", "client_items.xml");
			foreach (var node in itemsxml.Root.Children) {

				foreach (var itemattr in node.Children) {
					DataType dt = new DataType();
					dt.examples = new HashSet<string>();
					int a;
					double b;
					if (itemattr.Value != null && int.TryParse(itemattr.Value, out a)) {
						dt.type = "int";
					} else if (itemattr.Value != null && double.TryParse(itemattr.Value, out b)) {
						dt.type = "double";
					} else if (itemattr.Value != null && enumtype.Match(itemattr.Value).Success) {
						dt.type = "enum";
					} else {
						dt.type = "string";
					}
					if (!itemAttrs.ContainsKey(itemattr.Name)) {
						itemAttrs[itemattr.Name] = dt;
					} else {
						// fix bad preconception
						if (dt.type != itemAttrs[itemattr.Name].type) {
							dt.type = "string";
							itemAttrs[itemattr.Name] = dt;
						}
					}

					if (itemAttrs[itemattr.Name].type == "enum") {
						if (enumtype.Match(itemattr.Value).Success) {
							foreach (var str in itemattr.Value.Split(',')) {
								itemAttrs[itemattr.Name].examples.Add(str.Trim().ToLowerInvariant());
							}
						}
					}
				}
			}

			using (StreamWriter sw = new StreamWriter(@"itemsAnalysis.cs")) {
				foreach (var attr in itemAttrs) {
					if (attr.Value.type == "enum"

						&& (
							(attr.Value.examples.Count == 2
							&& attr.Value.examples.Contains("true")
							&& attr.Value.examples.Contains("false"))


						|| (attr.Value.examples.Count == 1 && (
									attr.Value.examples.Contains("true") ||
									attr.Value.examples.Contains("false")
								)
							)
						)

						) {
						attr.Value.type = "bool";
					}

					if (attr.Value.type == "enum" && attr.Value.examples.Count > 100) {
						attr.Value.type = "string";
					}

					if (attr.Value.type == "enum") {
						sw.WriteLine("enum {0} {{", "enum_" + attr.Key);
						foreach (var example in attr.Value.examples) {
							sw.WriteLine("\t@{0},", example);
						}
						sw.WriteLine("}");
					}
					Console.WriteLine("{1} {0}", attr.Key, attr.Value.type);
				}


				sw.WriteLine("struct Item {");
				foreach (var attr in itemAttrs) {
					if (attr.Value.type == "enum") {
						sw.WriteLine("\tpublic {0} {1};", "enum_" + attr.Key, attr.Key);
					} else {
						sw.WriteLine("\tpublic {0} {1};", attr.Value.type, attr.Key);
					}
				}
				sw.WriteLine("}");
			}
		}

	}
}

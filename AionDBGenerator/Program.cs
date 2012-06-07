using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using BlueBlocksLib.Database;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using AionDBGenerator.Tools;
using AionDBGenerator.AionDataTypes;

namespace AionDBGenerator {
	static class Program {

		[StructLayout(LayoutKind.Sequential)]
		struct MyStudent {
			public string name;
			public int age;
			public string moon;
		}


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {

			AionData data = new AionData(@"C:\Downloads\aion");

			//DataAnalysisTools.WriteItemsCS(data,@"Data\Items\Items.pak", "client_items.xml","itemsAnalysis.cs");
			//DataAnalysisTools.MakeFileWithStructForBXML(data, @"L10N\1_enu\data\data.pak", "strings/client_strings_item2.xml", "clientstring.cs");

			using (SQLiteConnection conn = new SQLiteConnection("aion.sqlite")) {
				conn.CreateTable<Item>("items");
				var itemsxml = data.ReadXMLFile(@"Data\Items\Items.pak", "client_items.xml");

				conn.CreateTable<ClientString>("stringtable");
				var clientStringItem = data.ReadXMLFile(@"L10N\1_enu\data\data.pak", "strings/client_strings_item.xml");
				var clientStringItem2 = data.ReadXMLFile(@"L10N\1_enu\data\data.pak", "strings/client_strings_item2.xml");

				using (SQLiteTransaction transaction = new SQLiteTransaction(conn)) {
					InsertBXMLData<Item>(conn, "items", itemsxml);

					InsertBXMLData<ClientString>(conn, "stringtable", clientStringItem);
					InsertBXMLData<ClientString>(conn, "stringtable", clientStringItem2);
				}

			}

			return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Items(data));
		}

		private static void InsertBXMLData<T>(SQLiteConnection conn, string table, BinaryXmlFile bxml) where T : new() {
			int count = 0;
			foreach (var node in bxml.Root.Children) {
				object item = new T();
				foreach (var attr in node.Children) {
					var fi = typeof(T).GetField(attr.Name);
					if (fi.FieldType == typeof(int)) {
						fi.SetValue(item, int.Parse(attr.Value));
					} else if (fi.FieldType == typeof(double)) {
						fi.SetValue(item, double.Parse(attr.Value));
					} else if (fi.FieldType == typeof(bool)) {
						fi.SetValue(item, bool.Parse(attr.Value));
					} else if (fi.FieldType.IsEnum) {
						fi.SetValue(item, Enum.Parse(fi.FieldType, attr.Value.Replace('-', '_'), true));
					} else {
						fi.SetValue(item, attr.Value);
					}
				}
				count++;
				if (count % 100 == 0) {
					Console.WriteLine("Processed {0} items for {1}", count, table);
				}
				conn.Insert(table, (T)item);
			}
		}

	}
}

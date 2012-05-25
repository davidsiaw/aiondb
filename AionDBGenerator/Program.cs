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


			MyStudent mystudent = new MyStudent() {
				name = "haruhi",
				age = 16,
				moon = "getsu"
			};

			//MyStudent mystudent2 = new MyStudent() {
			//    name = "kyon",
			//    age = 16,
			//    moon = "ka"
			//};

			//conn.Insert("students", mystudent);
			//conn.Insert("students", mystudent2);
			using (SQLiteConnection conn = new SQLiteConnection("test.sqlite")) {
				conn.CreateTable<MyStudent>("students");

				var res = conn.Select<MyStudent>("students").WhereLike("name", "KYON");
				var committed = res.ToArray();
			}

			AionData data = new AionData(@"C:\Downloads\aion");

			//DataAnalysisTools.WriteItemsCS(data);

			using (SQLiteConnection conn = new SQLiteConnection("aion.sqlite")) {
				conn.CreateTable<Item>("items");
				var itemsxml = data.ReadXMLFile(@"Data\Items\Items.pak", "client_items.xml");
				int count = 0;
				using (SQLiteTransaction transaction = new SQLiteTransaction(conn)) {
					foreach (var node in itemsxml.Root.Children) {
						object item = new Item();
						foreach (var attr in node.Children) {
							var fi = typeof(Item).GetField(attr.Name);
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
							Console.WriteLine("Processed {0} items", count);
						}
						conn.Insert("items", (Item)item);
					}
				}

			}

			return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Items(data));
		}

	}
}

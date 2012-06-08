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
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

namespace AionDBGenerator {
	static class Program {

		[StructLayout(LayoutKind.Sequential)]
		struct MyStudent {
			public string name;
			public int age;
			public string moon;
		}

		struct Bmp {
			public string name;
			public Bitmap bmp;
			public int area {
				get {
					return bmp.Width * bmp.Height;
				}
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {

			AionData data = new AionData(@"C:\Downloads\aion");

			//DataAnalysisTools.MakeFileWithStructForBXML(data, "itemsAnalysis.cs",
			//    new PakFile(@"Data\Items\Items.pak", "client_items.xml"),
			//    new PakFile(@"Data\Housing\Housing.pak", "client_housing_object.xml"));

			//DataAnalysisTools.MakeFileWithStructForBXML(data, @"L10N\1_enu\data\data.pak", "strings/client_strings_item2.xml", "clientstring.cs");

			using (SQLiteConnection conn = new SQLiteConnection("aion.sqlite")) {
				

				conn.CreateTable<Item>("items");
				var itemsxml = data.ReadXMLFile(@"Data\Items\Items.pak", "client_items.xml");
				var housingobjectxml = data.ReadXMLFile(@"Data\Housing\Housing.pak", "client_housing_object.xml");

				Dictionary<string, string> nameToCoords = CreateTextureMapOfIcons(data, itemsxml, housingobjectxml);

				conn.CreateTable<ClientString>("stringtable");
				var clientStringItem = data.ReadXMLFile(@"L10N\1_enu\data\data.pak", "strings/client_strings_item.xml");
				var clientStringItem2 = data.ReadXMLFile(@"L10N\1_enu\data\data.pak", "strings/client_strings_item2.xml");


				using (SQLiteTransaction transaction = new SQLiteTransaction(conn)) {
					InsertBXMLData<Item>(conn, "items", itemsxml,
						(fi, str) => {
							if (fi.Name == "name") {
								return "str_" + str.ToLowerInvariant();
							}
							if (fi.Name == "icon_name") {
								return nameToCoords[str + ".dds"];
							}
							return str.ToLowerInvariant();
						});

					InsertBXMLData<Item>(conn, "items", housingobjectxml,
						(fi, str) => {
							if (fi.Name == "name") {
								return "str_" + str.ToLowerInvariant();
							}
							if (fi.Name == "icon_name") {
								return nameToCoords[str + ".dds"];
							}
							return str.ToLowerInvariant();
						});

					InsertBXMLData<ClientString>(conn, "stringtable", clientStringItem, (fi, str) => fi.Name == "body" ? str : str.ToLowerInvariant());
					InsertBXMLData<ClientString>(conn, "stringtable", clientStringItem2, (fi, str) => fi.Name == "body" ? str : str.ToLowerInvariant());
				}

				conn.Index("stringtable", "name");
				conn.Index("items", "name");

				conn.Index("stringtable", "body");
			}

			return;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Items(data));
		}

		private static Dictionary<string, string> CreateTextureMapOfIcons(AionData data, params BinaryXmlFile[] xmls) {

			int width = 64;
			int height = 64;

			HashSet<string> filesUsed = new HashSet<string>();
			foreach (var itemsxml in xmls) {
				foreach (var node in itemsxml.Root.Children) {
					if (node.Children.Count(x => x.Name == "icon_name") != 0) {
						string iconname = node.Children.First(x => x.Name == "icon_name").Value;
						filesUsed.Add(iconname + ".dds");
					}
				}
			}

			Bitmap def = new Bitmap(1, 1);
			List<Bmp> bitmaps = new List<Bmp>();
			foreach (var file in filesUsed) {
				Bitmap b = data.ReadDDSFile(@"Data\Items\Items.pak", file);
				if (b == null) {
					b = def;
				}
				var bmp = new Bmp() { name = file, bmp = b };
				bitmaps.Add(bmp);
				Console.WriteLine("Processed Icon {0}", bmp.name);
			}
			bitmaps.Sort((y,x) => x.area.CompareTo(y.area));

		restart:

			

			RBSPNode<Bmp, string> rbsp = new RBSPNode<Bmp, string>(new FRectangle() { x1 = 0, x2 = width, y1 = 0, y2 = height }, 1);

			foreach (var bmp in bitmaps) {
				bool success = rbsp.Insert(
					new FRectangle() { x1 = 0, x2 = 40, y1 = 0, y2 = 40 }, bmp);
				if (!success) {
					width *= 2;
					height *= 2;
					Console.WriteLine("Texture map too small, resizing to {0}x{1} and restarting", width, height);
					goto restart;
				}
			}

			Bitmap superbitmap = new Bitmap(width, height);
			Dictionary<string, string> nameToCoords = new Dictionary<string, string>();
			using (Graphics g = Graphics.FromImage(superbitmap)) {
				rbsp.RetrieveRectangles((rect, bmp, z) => {
					nameToCoords[bmp.name] = string.Join(",", rect.x1, rect.y1, rect.Width(), rect.Height());
					g.DrawImage(bmp.bmp, rect.x1, rect.y1, new RectangleF(0,0,rect.Width(),rect.Height()), GraphicsUnit.Pixel);
				}, "");
			}
			superbitmap.Save("icons.png", ImageFormat.Png);

			return nameToCoords;
		}

		private static void InsertBXMLData<T>(SQLiteConnection conn, string table, BinaryXmlFile bxml, Func<FieldInfo, string, string> stringModifier) where T : new() {
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
						var modified = attr.Value == null ? null : stringModifier(fi, attr.Value);
						fi.SetValue(item, modified);
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

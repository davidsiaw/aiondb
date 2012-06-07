using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BlueBlocksLib.Database;
using AionDBGenerator.AionDataTypes;

namespace AionDBViewer {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {

			AionDB db = new AionDB(@"C:\Experiments\AionDBGenerator\AionDBGenerator\bin\Debug\aion.sqlite");
			var items = db.Search("Sundrenched");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		class AionDB {
			SQLiteConnection conn;
			public AionDB(string dbfile) {
				conn = new SQLiteConnection(dbfile);
			}

			public Item[] Search(string name) {
				var res = conn.Select<ClientString>("stringtable").WhereLike("body", name +"%").ToArray();

				List<Item> items = new List<Item>();
				foreach (var stringentry in res) {
					items.AddRange(conn.Select<Item>("items").WhereEquals("name", stringentry.name.Substring(4).ToLower()));
				}
				return items.ToArray();
			}
		}
	}
}

// -----------------------------------------------------------------------
// <copyright file="AionDB.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AionDBViewer {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using BlueBlocksLib.Database;
	using AionDBGenerator.AionDataTypes;
using System.Drawing;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>

	class AionDB {
		SQLiteConnection conn;
		Bitmap icons;
		public AionDB(string dbfile, string iconfile) {
			conn = new SQLiteConnection(dbfile);
			icons = new Bitmap(iconfile);
		}

		public string GetProperName(string itemName) {
			var res = conn.Select<ClientString>("stringtable").WhereEquals("name", itemName.ToLower()).ToArray();
			if (res.Length == 0) {
				return itemName;
			}
			return res[0].body;
		}

		public Bitmap GetIcon(string iconstring) {
			string[] toks = iconstring.Split(',');
			Bitmap bmp = new Bitmap(int.Parse(toks[2]), int.Parse(toks[3]));
			using (Graphics g = Graphics.FromImage(bmp)) {
				g.DrawImage(icons, 
					new Rectangle(0, 0, bmp.Width, bmp.Height),
					new Rectangle(
						int.Parse(toks[0]), 
						int.Parse(toks[1]), 
						bmp.Width,
						bmp.Height),
					GraphicsUnit.Pixel);
			}
			return bmp;
		}

		public Item[] Search(string name, int limit) {

			var res = conn.
				Select<Item>("items").
				Join("stringtable", "name", "name").
				WhereLike("body", name + "%").
				Limit(limit).ToArray();

			return res.ToArray();
		}
	}
}

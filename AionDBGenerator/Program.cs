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

namespace AionDBGenerator {
	static class Program {

		public static string aionPath = @"C:\Downloads\aion";

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

			using (SQLiteConnection conn = new SQLiteConnection("test.sqlite")) {
				conn.CreateTable<MyStudent>("students");

				//MyStudent mystudent = new MyStudent() {
				//    name = "haruhi",
				//    age = 16,
				//    moon = "getsu"
				//};

				//MyStudent mystudent2 = new MyStudent() {
				//    name = "kyon",
				//    age = 16,
				//    moon = "ka"
				//};

				//conn.Insert("students", mystudent);
				//conn.Insert("students", mystudent2);

				var res = conn.Select<MyStudent>("students").WhereLike("name", "KYON");
				var committed = res.ToArray();
			}


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Items());
		}

	}
}

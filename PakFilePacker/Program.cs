using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AionUtils;

namespace PakFilePacker {
	class Program {
		static void Main(string[] args) {
			if (args.Length == 3) {

				string mode = args[0];
				string pakfile = args[1];
				string dir = args[2];

				switch (mode.ToLower()) {
					case "pack":
						PAKFile.SerializeDirToPak(dir, pakfile);
						return;
					case "unpack":
						PAKFile.DeserializePakToDir(pakfile, dir);
						return;
					default:
						break;
				}
			}

			Console.WriteLine("Usage: pakfilepacker <pack|unpack> <pak file> <dir>");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AionPAKViewer {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {

			if (args.Length != 1) {
				return;
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FileList(args[0]));
		}
	}
}

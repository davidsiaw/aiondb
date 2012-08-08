using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AionUtils;

namespace AionPAKViewer {
	public partial class FileList : Form {
		PAKFile pakfile;
		public FileList(string pak) {
			InitializeComponent();
			pakfile = new PAKFile(pak);

			Text = pak;
			listBox1.Items.AddRange(pakfile.Files.ToArray());
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {

		}

		private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (listBox1.SelectedItem != null) {
				string file = (string)listBox1.SelectedItem;
				if (file.EndsWith(".dds") || file.EndsWith(".bmp")) {
					PictureViewer pv = new PictureViewer(file, pakfile[file]);
					pv.Show();
				}
				if (file.EndsWith(".xml")) {
					XMLViewer xv = new XMLViewer(file, pakfile[file]);
					xv.Show();
				}
			}
		}


	}
}

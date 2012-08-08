using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AionUtils;
using System.IO;

namespace AionPAKViewer {
	public partial class PictureViewer : Form {
		public PictureViewer(string filename, byte[] ddsbytes) {
			InitializeComponent();

			Image bmp;
			if (filename.EndsWith(".dds")) {
				bmp = JitConverter.DDSDataToBMP(ddsbytes);
			} else {
				bmp = Bitmap.FromStream(new MemoryStream(ddsbytes));
			}
			pictureBox1.Image = bmp;
			Size = bmp.Size;
			Text = filename;
		}
	}
}

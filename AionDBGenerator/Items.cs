using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;

namespace AionDBGenerator {
	public partial class Items : Form {
		public Items(AionData data) {
			InitializeComponent();

			Thread t = new Thread(() => {

				XDocument doc = AionData.BXMLToXML(data.ReadXMLFile(@"Data\Items\Items.pak", "client_items.xml"));

				Dictionary<string, int> columns = new Dictionary<string, int>();
				BeginInvoke(new Action(() => {
					DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
					iconColumn.Name = "icon";
					iconColumn.HeaderText = "icon";
					dataGridView1.Columns.Add(iconColumn);

					foreach (var node in doc.Elements().First().Elements().First().Elements()) {
						dataGridView1.Columns.Add(node.Name.LocalName, node.Name.LocalName);
						columns[node.Name.LocalName] = columns.Count;
					}
				}));

				foreach (var node in doc.Elements().First().Elements()) {
					BeginInvoke(new Action<XElement>(x => {
						string[] values = new string[columns.Count];
						foreach (XElement val in x.Elements()) {
							if (!columns.ContainsKey(val.Name.LocalName)) {
								dataGridView1.Columns.Add(val.Name.LocalName, val.Name.LocalName);
								columns[val.Name.LocalName] = columns.Count;
								Array.Resize(ref values, columns.Count);
							}
							values[columns[val.Name.LocalName]] = (val.Value);
						}

						var iconname = values[columns["icon_name"]];

						Bitmap b = data.ReadDDSFile(@"Data\Items\Items.pak", iconname + ".dds");

						object[] rowValues = new object[] { b }.Concat(values).ToArray();
						int rowIndex = dataGridView1.Rows.Add(rowValues);
						dataGridView1.Rows[rowIndex].Height = 64;

					}), node);
					Thread.Sleep(10);


					if (dataGridView1.Rows.Count > 10000) {
						return;
					}
				}


			});

			t.IsBackground = true;
			t.Priority = ThreadPriority.Lowest;
			t.Start();

		}

	}
}

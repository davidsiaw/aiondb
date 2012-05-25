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
using JitOpener;

namespace AionDBGenerator {
	public partial class Items : Form {
		public Items() {
			InitializeComponent();

			Dictionary<string, Bitmap> iconcache = new Dictionary<string, Bitmap>();

			Thread t = new Thread(() => {
				var file = OpenPAKFile(Path.Combine(Program.aionPath, @"Data\Items\Items.pak"));
				using (ZipFile zFile = new ZipFile(file)) {
					var assemblyItems = zFile["client_items.xml"];
					BinaryXmlFile bxml = new BinaryXmlFile();
					using (var theFile = zFile.GetInputStream(assemblyItems)) {
						bxml.Read(theFile);
					}

					XmlDocument doc = new XmlDocument();
					doc.AppendChild(FromBXMLNode(doc, bxml.Root));

					Dictionary<string, int> columns = new Dictionary<string, int>();
					BeginInvoke(new Action(() => {
						DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
						iconColumn.Name = "icon";
						iconColumn.HeaderText = "icon";
						dataGridView1.Columns.Add(iconColumn);

						foreach (XmlNode node in doc.FirstChild.FirstChild.ChildNodes) {
							dataGridView1.Columns.Add(node.Name, node.Name);
							columns[node.Name] = columns.Count;
						}
					}));

					foreach (var node in doc.FirstChild.ChildNodes) {
						BeginInvoke(new Action<XmlElement>(x => {
							string[] values = new string[columns.Count];
							foreach (XmlNode val in x.ChildNodes) {
								if (!columns.ContainsKey(val.Name)) {
									dataGridView1.Columns.Add(val.Name, val.Name);
									columns[val.Name] = columns.Count;
									Array.Resize(ref values, columns.Count);
								}
								values[columns[val.Name]] = (val.InnerText);
							}

							var iconname = values[columns["icon_name"]];

							if (!iconcache.ContainsKey(iconname)) {
								var entry = zFile[iconname + ".dds"];
								var iconFile = zFile.GetInputStream(entry);

								byte[] bytes = new byte[entry.Size];
								iconFile.Read(bytes, 0, bytes.Length);
								Bitmap b = JitConverter.DDSDataToBMP(bytes);
								iconcache[iconname] = b;
							}

							object[] rowValues = new object[] { iconcache[iconname] }.Concat(values).ToArray();
							int rowIndex = dataGridView1.Rows.Add(rowValues);
							dataGridView1.Rows[rowIndex].Height = 64;

						}), node);
						Thread.Sleep(10);


						if (dataGridView1.Rows.Count > 10000) {
							return;
						}
					}
				}

			});

			t.IsBackground = true;
			t.Priority = ThreadPriority.Lowest;
			t.Start();

		}
		// open pak file return path to zip file
		static string OpenPAKFile(string pakFile) {
			string resultFile = Path.GetFullPath(Path.GetFileNameWithoutExtension(pakFile) + ".zip");
			var p = Process.Start("pak2zip.py",
				string.Join(" ", "\"" + pakFile + "\"", "\"" + resultFile + "\""));
			p.WaitForExit();
			return resultFile;
		}

		static XmlNode FromBXMLNode(XmlDocument doc, BinaryXmlNode bxmlnode) {
			var node = doc.CreateElement(bxmlnode.Name);
			foreach (KeyValuePair<string, string> current in bxmlnode.Attributes) {
				node.SetAttribute(current.Key, current.Value);
			}
			foreach (BinaryXmlNode current2 in bxmlnode.Children) {
				node.AppendChild(FromBXMLNode(doc, current2));
			}
			if (bxmlnode.Value != null) {
				node.InnerText = bxmlnode.Value;
			}
			return node;
		}
	}
}

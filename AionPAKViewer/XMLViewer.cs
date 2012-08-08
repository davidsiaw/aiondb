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
using System.Xml;

namespace AionPAKViewer {
	public partial class XMLViewer : Form {
		public XMLViewer(string file, byte[] xml) {
			InitializeComponent();

			try {
				BinaryXmlFile f = new BinaryXmlFile();
				f.Read(new MemoryStream(xml));
				treeView1.Nodes.Add(MakeTree(f.Root));
				Text = "bxml://" + file;

			} catch {
				XmlDocument xdoc = new XmlDocument();
				xdoc.Load(new MemoryStream(xml));
				treeView1.Nodes.Add(MakeTree(xdoc));
				Text = "xml://" + file;
			}

		}

		TreeNode MakeTree(XmlNode node) {
			TreeNode tn = new TreeNode(node.Name);
			if (node.Attributes != null) {
				foreach (XmlAttribute attr in node.Attributes) {
					tn.Nodes.Add(attr.Name + "=" + attr.Value);
				}
			}
			if (node.Value != null) {
				tn.Nodes.Add(node.Value);
			}
			foreach (XmlNode child in node.ChildNodes) {
				tn.Nodes.Add(MakeTree(child));
			}
			return tn;
		}

		TreeNode MakeTree(BinaryXmlNode node) {
			TreeNode tn = new TreeNode(node.Name);
			foreach (var attr in node.Attributes) {
				tn.Nodes.Add(attr.Key + "=" + attr.Value);
			}
			if (node.Value != null) {
				tn.Nodes.Add(node.Value);
			}
			foreach (var child in node.Children) {
				tn.Nodes.Add(MakeTree(child));
			}
			return tn;
		}

		private void XMLViewer_Load(object sender, EventArgs e) {

		}
	}
}

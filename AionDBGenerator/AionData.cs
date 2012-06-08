// -----------------------------------------------------------------------
// <copyright file="AionData.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AionDBGenerator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Drawing;
	using System.IO;
	using System.Diagnostics;
	using System.Xml;
	using ICSharpCode.SharpZipLib.Zip;
	using JitOpener;
	using System.Xml.Linq;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>

	public class AionData {

		Dictionary<string, Bitmap> imageCache = new Dictionary<string, Bitmap>();

		string aionPath;
		public AionData(string aionPath) {
			this.aionPath = aionPath;
		}

		public BinaryXmlFile ReadXMLFile(string pakFile, string internalFile) {
			var itempak = OpenPAKFile(Path.Combine(aionPath, pakFile));
			Debug.Assert(internalFile.EndsWith("xml"));
			using (ZipFile zFile = new ZipFile(itempak)) {
				var assemblyItems = zFile[internalFile];
				BinaryXmlFile bxml = new BinaryXmlFile();
				bxml.Read(zFile.GetInputStream(assemblyItems));
				return bxml;
			}
		}

		public string[] GetFiles(string pakFile) {
			var itempak = OpenPAKFile(Path.Combine(aionPath, pakFile));
			List<string> files = new List<string>();
			using (ZipFile zFile = new ZipFile(itempak)) {
				foreach (ZipEntry entry in zFile) {
					files.Add(entry.Name);
				}
			}
			return files.ToArray();
		}

		public Bitmap ReadDDSFile(string pakFile, string internalFile) {
			string key = pakFile + "!" + internalFile;
			Debug.Assert(internalFile.EndsWith("dds"));
			if (!imageCache.ContainsKey(key)) {
				var itempak = OpenPAKFile(Path.Combine(aionPath, pakFile));
				using (ZipFile zFile = new ZipFile(itempak)) {
					int entrynum = zFile.FindEntry(internalFile, true);
					if (entrynum == -1) { return null; }
					var entry = zFile[entrynum];
					var iconFile = zFile.GetInputStream(entry);
					byte[] bytes = new byte[entry.Size];
					iconFile.Read(bytes, 0, bytes.Length);
					Bitmap b = JitConverter.DDSDataToBMP(bytes);
					imageCache[key] = b;
				}
			}
			return imageCache[key];
		}

		// open pak file return path to zip file
		static string OpenPAKFile(string pakFile) {
			string resultFile = Path.GetFullPath(Path.GetFileNameWithoutExtension(pakFile) + ".zip");
			if (!File.Exists(resultFile)) {
				var p = Process.Start("pak2zip.py",
					string.Join(" ", "\"" + pakFile + "\"", "\"" + resultFile + "\""));
				p.WaitForExit();
			}
			return resultFile;
		}

		public static XDocument BXMLToXML(BinaryXmlFile bxml) {
			XDocument doc = new XDocument();
			doc.Add(FromBXMLNode(doc, bxml.Root));
			return doc;
		}

		static XElement FromBXMLNode(XDocument doc, BinaryXmlNode bxmlnode) {
			var node = new XElement(bxmlnode.Name);
			foreach (KeyValuePair<string, string> current in bxmlnode.Attributes) {
				node.Add(new XAttribute(current.Key, current.Value));
			}
			foreach (BinaryXmlNode current2 in bxmlnode.Children) {
				node.Add(FromBXMLNode(doc, current2));
			}
			if (bxmlnode.Value != null) {
				node.Value = bxmlnode.Value;
			}
			return node;
		}
	}
}

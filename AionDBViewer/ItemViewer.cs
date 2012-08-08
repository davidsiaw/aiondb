using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AionDBViewer {
	public partial class ItemViewer : Form {
		AionDB db;
		public ItemViewer() {
			InitializeComponent();
			db = new AionDB(@"C:\Experiments\AionDBGenerator\AionDBGenerator\bin\Debug\aion.sqlite",
				@"C:\Experiments\AionDBGenerator\AionDBGenerator\bin\Debug\icons.png");

			DataGridViewImageColumn dgvic = new DataGridViewImageColumn();
			dgvic.Name = "Icon";
			dgvic.HeaderText = "Icon";
			dataGridView1.Columns.Add(dgvic);

			dataGridView1.Columns.Add("Name", "Name");
			dataGridView1.Columns.Add("Level", "Level");
			dataGridView1.Columns.Add("Grade", "Grade");
			dataGridView1.Columns.Add("NPC Buy", "NPC Buy");
			dataGridView1.Columns.Add("AP", "AP");
			dataGridView1.Columns.Add("Side", "Side");
			dataGridView1.Columns.Add("Warehouseidx", "Warehouseidx");

		}

		[StructLayout(LayoutKind.Sequential)]
		struct ItemInfo {
			public string icon_name;
			public string name;
			public int level;
			public enum_item_type item_type;
			public int price;
			public int abyss_point;
			public string race_permitted;
			public int in_house_warehouse_idx;
			public enum_quality quality;
		}

		private void textBox1_TextChanged(object sender, EventArgs e) {
			var items = db.Search<ItemInfo>(textBox1.Text, 100);
			dataGridView1.Rows.Clear();

			foreach (var item in items) {
				
				int num = dataGridView1.Rows.Add(
					db.GetIcon(item.icon_name),
					db.GetProperName(item.name), 
					item.level, 
					db.GetProperName("ITEMTYPE_" + item.item_type.ToString()),
					item.price,
					item.abyss_point,
					item.race_permitted,
					item.in_house_warehouse_idx);

				dataGridView1.Rows[num].Height = 44;
				dataGridView1.Rows[num].DefaultCellStyle.BackColor = Color.Black;
				dataGridView1.Rows[num].DefaultCellStyle.ForeColor = Color.White;
				switch (item.quality) {
					case enum_quality.common:
						dataGridView1.Rows[num].Cells["Name"].Style.ForeColor = Color.White;
						break;
					case enum_quality.epic:
						dataGridView1.Rows[num].Cells["Name"].Style.ForeColor = Color.OrangeRed;
						break;
					case enum_quality.junk:
						dataGridView1.Rows[num].Cells["Name"].Style.ForeColor = Color.Gray;
						break;
					case enum_quality.legend:
						dataGridView1.Rows[num].Cells["Name"].Style.ForeColor = Color.SkyBlue;
						break;
					case enum_quality.mythic:
						break;
					case enum_quality.rare:
						dataGridView1.Rows[num].Cells["Name"].Style.ForeColor = Color.LimeGreen;
						break;
					case enum_quality.unique:
						dataGridView1.Rows[num].Cells["Name"].Style.ForeColor = Color.Orange;
						break;
				}
			
			}
		}
	}
}

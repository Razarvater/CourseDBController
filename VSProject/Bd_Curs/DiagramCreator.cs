using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private bool ActiveDragAndDrop = false;
        private Control SelectedControl;
        private void PrintShema()
        {
            Random rnd = new Random();
            tabPage8.Controls.Clear();
            for (int i = 0; i < db.Tables.Count; i++)
            {
                DataGridView temp = new DataGridView();
                temp.Location = new System.Drawing.Point(rnd.Next(0,tabPage8.Width - 200), rnd.Next(0, tabPage8.Height - 200));
                DataTable tempData = new DataTable();
                tempData.Columns.Add(new DataColumn(db.Tables[i].name));
                for (int j = 0; j < db.Tables[i].ColumnsNames.Count; j++)
                {
                    DataRow tempo = tempData.NewRow();
                    tempo[db.Tables[i].name] = db.Tables[i].ColumnsNames[j];
                    tempData.Rows.Add(tempo);
                }

                temp.DataSource = tempData;
                temp.AllowUserToDeleteRows = false;
                temp.AllowUserToResizeColumns = false;
                temp.AllowUserToResizeRows = false;
                temp.MultiSelect = false;
                temp.ReadOnly = true;
                temp.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                temp.Size = new System.Drawing.Size(tempData.Columns[0].ColumnName.Length*15, tempData.Rows.Count * 22 + 47);

                temp.RowHeadersWidth = 4;
                temp.MouseUp += mouseUp;
                temp.MouseDown += mouseDown;
                temp.MouseMove += MoveDrop;
                tabPage8.Controls.Add(temp);
            }
        }
        private void mouseDown(object sender, EventArgs e)
        {
            SelectedControl = (Control)sender;
            ActiveDragAndDrop = true;
        }
        private void mouseUp(object sender, EventArgs e)
        {
            SelectedControl = (Control)sender;
            ActiveDragAndDrop = false;
        }
        private void MoveDrop(object sender,EventArgs e)
        {
            if (ActiveDragAndDrop && SelectedControl.Name == sender.GetType().GetProperty("Name").GetValue(sender).ToString())
                sender.GetType().GetProperty("Location").SetValue(sender, new System.Drawing.Point(Cursor.Position.X - 240, Cursor.Position.Y - 230));
        }
    }
}

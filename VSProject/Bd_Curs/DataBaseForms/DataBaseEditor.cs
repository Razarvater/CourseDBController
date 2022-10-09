using System.Drawing;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private void InitTableForms()//Форма просмотра характеристик таблиц
        {
            int Y,step = 150;//Координаты
            int tempBarrier = tabControl5.TabPages.Count;//Барьер для пропуска печати таблицы Users
            int IndexTabControl = 0;
            for (int i = 0; i < tempBarrier; i++)//Заполнение всех табличных форм
            {
                if (db.Tables[i].name == "Users")//Исключая таблицу Users
                {
                    tempBarrier++;
                    continue;
                }
                Y = 0;
                //Создание названий колонок
                Label ColumnNameZ = new Label();
                ColumnNameZ.Text = "ColumnName";
                ColumnNameZ.Location = new Point(0, 0);
                ColumnNameZ.Width = step;
                ColumnNameZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnNameZ.Font = new Font(ColumnNameZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnNameZ);

                Label ColumnTypeZ = new Label();
                ColumnTypeZ.Text = "TypeOfData";
                ColumnTypeZ.Location = new Point(step, 0);
                ColumnTypeZ.Width = step;
                ColumnTypeZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnTypeZ.Font = new Font(ColumnTypeZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnTypeZ);

                Label ColumnPKZ = new Label();
                ColumnPKZ.Text = "IsPrimaryKey";
                ColumnPKZ.Location = new Point(step*2, 0);
                ColumnPKZ.Width = step;
                ColumnPKZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnPKZ.Font = new Font(ColumnPKZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnPKZ);

                Label ColumnNullZ = new Label();
                ColumnNullZ.Text = "IsNullable";
                ColumnNullZ.Location = new Point(step*3, 0);
                ColumnNullZ.Width = step;
                ColumnNullZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnNullZ.Font = new Font(ColumnNullZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnNullZ);
                Y += 25;
                //Заполнение колонок содержимым
                for (int j = 0; j < db.Tables[i].Columns.Count; j++)
                {
                    Label ColumnName = new Label();
                    ColumnName.Text = db.Tables[i].ColumnsNames[j];
                    ColumnName.Location = new Point(0, Y);
                    ColumnName.Width = step;
                    ColumnName.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnName);

                    Label ColumnType = new Label();
                    ColumnType.Text = db.Tables[i].Columns[j].type.ToString();
                    ColumnType.Location = new Point(step, Y);
                    ColumnType.Width = step;
                    ColumnType.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnType);

                    Label ColumnPK = new Label();
                    ColumnPK.Text = db.Tables[i].Columns[j].IsPrimaryKey.ToString();
                    ColumnPK.Location = new Point(step*2, Y);
                    ColumnPK.Width = step;
                    ColumnPK.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnPK);

                    Label ColumnNull = new Label();
                    ColumnNull.Text = db.Tables[i].Columns[j].IsNullable.ToString();
                    ColumnNull.Location = new Point(step*3, Y);
                    ColumnNull.Width = step;
                    ColumnNull.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnNull);
                    Y += 25;
                }
                IndexTabControl++;//Смещение на следующую таблицу
            }
        }

    }
}

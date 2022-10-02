using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Bd_Curs
{
    public partial class Form1 : Form 
    {
        private bool ISDistinct = false;
        private List<TextBox> WhereSelectBoxes = new List<TextBox>();
        private string SelectQuery;
        private List<CheckBox> CheckBoxes;
        private List<Label> SelectLabels;

        private List<string> OperationsCollection = new List<string> {"=","!=","<",">","=>","<="};
        private List<ComboBox> ColumnNames;
        private List<ComboBox> Operations;
        private List<TextBox> Values;
        private List<ComboBox> AndOR;
        private int x = 5;
        private int y = 5;

        private void DISTINCT_CheckedChanged(object sender, EventArgs e) => ISDistinct = !ISDistinct;//Галочка на убирание повторений в запросе

        private void InitConditions()
        {
            x = 5;
            y = 5;
            tabPage5.Controls.Clear();

            Button button = new Button();
            button.Location = new Point(x, y);
            button.Size = new Size(100, 40);
            button.Text = $"New Condition";
            button.Click += AddNewCondition;
            button.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage5.Controls.Add(button);//Добавление на страницу
            y += 50;

            ColumnNames = new List<ComboBox>();
            Operations = new List<ComboBox>();
            Values = new List<TextBox>();
            AndOR = new List<ComboBox>();
        }
        private void AddNewCondition(object sender, EventArgs e)
        {
            if (ColumnNames.Count != 0 && ColumnNames[ColumnNames.Count - 1].Text == string.Empty)
                return;
            if (Operations.Count != 0 && Operations[Operations.Count - 1].Text == string.Empty)
                return;
            if (Values.Count != 0 && Values[Values.Count - 1].Text == string.Empty)
                return;
            if (AndOR.Count != 0 && ColumnNames.Count > 1 && AndOR[AndOR.Count - 1].Text == string.Empty)
                return;

            AnchorStyles style = (AnchorStyles.Left | AnchorStyles.Top);

            if (ColumnNames.Count>0)
            {
                ComboBox tempy = new ComboBox();
                if(x + 275 - 350 < 0)
                    tempy.Location = new Point(x + 975, y - 30);
                else
                    tempy.Location = new Point(x + 275 - 350, y);
                tempy.Size = new Size(50, 30);
                tempy.Anchor = style;
                tempy.Name = $"ANDOR{ColumnNames.Count + 1}";
                tempy.DropDownStyle = ComboBoxStyle.DropDownList;
                tempy.Items.Add("AND");
                tempy.Items.Add("OR");
                AndOR.Add(tempy);
                tabPage5.Controls.Add(tempy);
            }

            ComboBox temp = new ComboBox();
            temp.Location = new Point(x,y);
            temp.Size = new Size(100, 30);
            temp.Name = $"Condition{ColumnNames.Count + 1}";
            temp.DropDownStyle = ComboBoxStyle.DropDownList;
            temp.Anchor = style;
            foreach (var item in db.Tables[SelectedTableNameINT].ColumnsNames)
            {
                temp.Items.Add(item);
            }
            ColumnNames.Add(temp);
            tabPage5.Controls.Add(temp);

            ComboBox tempo = new ComboBox();
            tempo.Location = new Point(x + 110, y);
            tempo.Size = new Size(35, 30);
            tempo.Name = $"Operation{ColumnNames.Count + 1}";
            tempo.DropDownStyle = ComboBoxStyle.DropDownList;
            tempo.Anchor = style;
            foreach (var item in OperationsCollection)
            {
                tempo.Items.Add(item);
            }
            Operations.Add(tempo);
            tabPage5.Controls.Add(tempo);

            TextBox tempr = new TextBox();
            tempr.Location = new Point(x + 155, y);
            tempr.Size = new Size(100, 30);
            tempr.Name = $"Value{ColumnNames.Count + 1}";
            tempr.Anchor = style;
            Values.Add(tempr);
            tabPage5.Controls.Add(tempr);

            x += 350;
            if (x + 200 > tabPage2.Width)
            {
                x = 5;
                y += 30;
            }
        }
        private void InitSelectForm()
        {
            int Y = 10;
            int X = 100;
            tabPage4.AutoScrollMinSize = new Size(0, (db.Tables[SelectedTableNameINT].Columns.Count * 30) / 3 + 100);
            CheckBoxes = new List<CheckBox>();
            SelectLabels = new List<Label>();
            tabPage4.Controls.Clear();
            for (int i = 0; i < db.Tables[SelectedTableNameINT].Columns.Count; i++)
            {
                CheckBox temp = new CheckBox();//Создание нового бокса с условием
                temp.Location = new Point(X, Y);//Его местоположение и новое имя
                temp.Size = new Size(100, 20);
                temp.Name = $"{db.Tables[SelectedTableNameINT].Columns[i].Name}SELECT";
                temp.Anchor = AnchorStyles.Left;
                temp.CheckedChanged += CheckClick;
                CheckClick(temp, EventArgs.Empty);
                if (db.Tables[SelectedTableNameINT].Columns[i].IsPrimaryKey)
                {
                    temp.Enabled = false;
                    temp.Checked = true;
                }
                CheckBoxes.Add(temp);//Добавление в коллекцию 
                tabPage4.Controls.Add(CheckBoxes[CheckBoxes.Count - 1]);//Добавление на страницу
                
                Label tempo = new Label();
                tempo.Location = new Point(X - 100, Y + 3);
                tempo.Size = new Size(100, 13);
                tempo.Text = $"{db.Tables[SelectedTableNameINT].Columns[i].Name}";
                tempo.Anchor = AnchorStyles.Left;
                SelectLabels.Add(tempo);//Добавление в коллекцию 
                tabPage4.Controls.Add(SelectLabels[SelectLabels.Count - 1]);//Добавление на страницу

                X += 300;
                if (X + 200 > tabPage2.Width)
                {
                    X = 100;
                    Y += 30;
                }
            }
            Button button = new Button();
            button.Location = new Point(10, Y + 30);
            button.Size = new Size(100, 40);
            button.Text = $"Select";
            button.Anchor = AnchorStyles.Left;
            button.Click += button3_Click;
            tabPage4.Controls.Add(button);//Добавление на страницу
            InitConditions();
        }
        
        private void button3_Click(object sender, EventArgs e)//Выполнить запрос из формы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
            Query_IsWorking = true;//Запрос выполняется
            SelectQuery = "SELECT ";//Выборка
            if (ISDistinct) SelectQuery += "DISTINCT ";//Если требуется уборка повторений

            foreach (var item in CheckBoxes)
            {
                if (item.Checked)
                    SelectQuery += $"[{item.Name.Remove(item.Name.Length - 6)}], ";
                
            }
            SelectQuery = SelectQuery.Remove(SelectQuery.Length - 2);

            SelectQuery += $" FROM [{SelectedTableName}] ";//Из таблицы
            SqlCommand command = new SqlCommand(SelectQuery, db.connection);
            if (ColumnNames.Count > 0)
            {
                SelectQuery += "WHERE ";
                for (int i = 0; i < ColumnNames.Count; i++)
                {
                    SelectQuery += $"[{ColumnNames[i].Text}] {Operations[i].Text} @{Values[i].Name}";
                    command.Parameters.Add(new SqlParameter($"@{Values[i].Name}", Values[i].Text));
                    if (i != ColumnNames.Count - 1)
                        SelectQuery += $" {AndOR[i].Text} ";
                }
            }
            command.CommandText = SelectQuery;

            Thread UpdateThread = new Thread(() => db.SetQueryAsync(SelectQuery,command));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}

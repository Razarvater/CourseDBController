using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class Form1 : Form 
    {
        private bool ISDistinct = false;
        private List<TextBox> WhereSelectBoxes = new List<TextBox>();
        private string SelectQuery;
        private List<CheckBox> CheckBoxes;
        private List<Label> SelectLabels;


        private void DISTINCT_CheckedChanged(object sender, EventArgs e) => ISDistinct = !ISDistinct;//Галочка на убирание повторений в запросе
        

        private void InitSelectForm()
        {
            int Y = 10;
            int X = 100;
            tabPage3.AutoScrollMinSize = new Size(0, (db.Tables[SelectedTableNameINT].Columns.Count * 30) / 5 + 100);
            CheckBoxes = new List<CheckBox>();
            SelectLabels = new List<Label>();
            tabPage3.Controls.Clear();
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
                tabPage3.Controls.Add(CheckBoxes[CheckBoxes.Count - 1]);//Добавление на страницу
                
                Label tempo = new Label();
                tempo.Location = new Point(X - 100, Y + 3);
                tempo.Size = new Size(100, 13);
                tempo.Text = $"{db.Tables[SelectedTableNameINT].Columns[i].Name}";
                tempo.Anchor = AnchorStyles.Left;
                SelectLabels.Add(tempo);//Добавление в коллекцию 
                tabPage3.Controls.Add(SelectLabels[SelectLabels.Count - 1]);//Добавление на страницу

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
            tabPage3.Controls.Add(button);//Добавление на страницу
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
                    SelectQuery += $"{item.Name.Remove(item.Name.Length - 6)}, ";
                
            }
            SelectQuery = SelectQuery.Remove(SelectQuery.Length - 2);

            SelectQuery += $" FROM [{SelectedTableName}] ";//Из таблицы

            /*if (WhereSelectBoxes[0].Text != string.Empty)//Добавление условий при их наличии
            {
                SelectQuery += $"WHERE {WhereSelectBoxes[0].Text} ";//Добавление первого условия
                foreach (var item in WhereSelectBoxes)//Добавление всех остальных условий
                {
                    if (item.Text != string.Empty)//Если оно не пустое
                        SelectQuery += $"AND {item.Text} ";//Добавить условие
                }
            }*/


            Thread UpdateThread = new Thread(() => db.SetQueryAsync(SelectQuery,new System.Data.SqlClient.SqlCommand(SelectQuery,db.connection)));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}

using System.Data;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private List<Control> InsertBoxes;//Боксы для значений
        private List<Label> labels;//Названия столбцов
        private bool IsInsert = false;

        private void CreateInsertForm()
        {
            //Создание координат для генерируемых элементов интерфейса
            int Y = 10;
            int X = 100;
            tabPage2.AutoScrollMinSize = new Size(0, (db.Tables[IndexSelectedTable].Columns.Count * 30) / 3 + 100);//Установка размеров страницы
            //Создание новых и очистка старых элементов
            InsertBoxes = new List<Control>();
            labels = new List<Label>();
            tabPage2.Controls.Clear();

            for (int i = 0; i < db.Tables[IndexSelectedTable].Columns.Count; i++)//Создание для всех столбцов
            {
                //Кроме автоинкрементых и картинок
                if (db.Tables[IndexSelectedTable].Columns[i].IsAutoIncrement || db.Tables[IndexSelectedTable].Columns[i].type == SqlDbType.Image) continue;

                if (db.Tables[IndexSelectedTable].Columns[i].type != SqlDbType.Bit)//Если не логическое значение
                {
                    TextBox temp = new TextBox();//Создание нового бокса с условием
                    temp.Location = new Point(X, Y);//Его местоположение и новое имя
                    temp.Size = new Size(200, 20);
                    temp.Name = $"{db.Tables[IndexSelectedTable].Columns[i].Name}";
                    if (!db.Tables[IndexSelectedTable].Columns[i].IsNullable)
                        temp.Text = "NOT NULL please fill in the field";
                    temp.Anchor = AnchorStyles.Left;
                    InsertBoxes.Add(temp);//Добавление в коллекцию 
                    tabPage2.Controls.Add(InsertBoxes[InsertBoxes.Count - 1]);//Добавление на страницу
                }
                else
                {
                    CheckBox temp = new CheckBox();//Создание нового бокса с условием
                    temp.Location = new Point(X, Y);//Его местоположение и новое имя
                    temp.Size = new Size(200, 20);
                    temp.Name = $"{db.Tables[IndexSelectedTable].Columns[i].Name}";
                    temp.Anchor = AnchorStyles.Left;
                    temp.CheckedChanged += CheckClick;
                    CheckClick(temp, EventArgs.Empty);
                    InsertBoxes.Add(temp);//Добавление в коллекцию 
                    tabPage2.Controls.Add(InsertBoxes[InsertBoxes.Count - 1]);//Добавление на страницу
                }

                //Имя поля
                Label tempo = new Label();
                tempo.Location = new Point(X - 100, Y + 3);
                tempo.Size = new Size(100, 13);
                tempo.Text = $"{db.Tables[IndexSelectedTable].Columns[i].Name}";
                tempo.Anchor = AnchorStyles.Left;
                labels.Add(tempo);//Добавление в коллекцию 
                tabPage2.Controls.Add(labels[labels.Count - 1]);//Добавление на страницу

                X += 300;
                if (X + 200 > tabPage2.Width)//Переход на следующую строку
                {
                    X = 100;
                    Y += 30;
                }
            }

            //Кнопка для вноса записи
            Button button = new Button();
            button.Location = new Point(10, Y + 30);
            button.Size = new Size(100, 40);
            button.Text = $"Insert";
            button.Anchor = AnchorStyles.Left;
            button.Click += button1_Click;
            tabPage2.Controls.Add(button);//Добавление на страницу
        }
        private void CheckClick(object sender, EventArgs e)//Отображение статуса Чекбоксов
        {
            if(sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "true")
                sender.GetType().GetProperty("Text").SetValue(sender,"True");
            else
                sender.GetType().GetProperty("Text").SetValue(sender,"False");
        }
        private void button1_Click(object sender, EventArgs e)//Короче вместо всей этой херни автогенерируемую форму сделать и не париться...
        {
            string Query = $"INSERT INTO [{SelectedTableName}] (";//Создание запроса
            for (int i = 0; i < InsertBoxes.Count; i++)//Вставка всех имен полей
            {
                if (InsertBoxes[i].Text != string.Empty)
                {
                    Query += $"[{InsertBoxes[i].Name}], ";
                }
            }
            Query = Query.Remove(Query.Length - 2);
            Query += ") VALUES (";
            SqlCommand sqlCommand = new SqlCommand(Query, db.connection);//Создание параметризированного запроса
            for (int i = 0; i < InsertBoxes.Count; i++)
            {
                if (InsertBoxes[i].Text != string.Empty)
                {
                    Query += $"@{InsertBoxes[i].Name}, ";
                    sqlCommand.Parameters.Add(new SqlParameter($"@{InsertBoxes[i].Name}",InsertBoxes[i].Text));
                }
            }
            Query = Query.Remove(Query.Length - 2);
            Query += ")";
            sqlCommand.CommandText = Query;

            IsUpdate = true;
            IsInsert = true;
            Thread UpdateThread = new Thread(() => db.SetQueryAsync(Query, sqlCommand));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
        private void NewRow()//Новая строка
        {
            ((DataTable)SelectedTable.DataSource).Rows.Add();//Создание новой строки для отображения

            int tempINdex = 0;

            for (int i = 0; i < SelectedTable.Rows[0].Cells.Count; i++)//Установка значений в строку
            {
                foreach (var item2 in InsertBoxes)
                {
                    if (SelectedTable.Columns[i].HeaderText == item2.Name)
                    {
                        SelectedTable.Rows[0].Cells[tempINdex].Value = item2.Text;
                        break;
                    }

                }
                tempINdex++;
            }
            for (int i = 0; i < SelectedTable.Rows[0].Cells.Count; i++)//Установка автоинкрементых значений в строку
            {
                foreach (var item in db.Tables[IndexSelectedTable].Columns)
                {
                    if (SelectedTable.Columns[i].HeaderText == item.Name && item.IsAutoIncrement)
                    {
                        SelectedTable.Rows[0].Cells[i].Value = db.GetAutoIndex(SelectedTableName).ToString();
                        i = SelectedTable.Rows[0].Cells.Count;
                        break;
                    }
                }
            }

        }
    }
}


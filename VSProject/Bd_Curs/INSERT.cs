﻿using System.Data;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private List<Control> InsertBoxes;
        private List<Label> labels;
        
        private void CreateInsertForm()
        {
            int Y = 10;
            int X = 100;
            tabPage2.AutoScrollMinSize = new Size(0, (db.Tables[SelectedTableNameINT].Columns.Count * 30) / 5 + 100);
            InsertBoxes = new List<Control>();
            labels = new List<Label>();
            tabPage2.Controls.Clear();
            if(SelectedTableName == "Employees")
                Console.WriteLine("");
            for (int i = 0; i < db.Tables[SelectedTableNameINT].Columns.Count;i++)
            {
                if (db.Tables[SelectedTableNameINT].Columns[i].IsAutoIncrement || db.Tables[SelectedTableNameINT].Columns[i].type == SqlDbType.Image) continue;

                if (db.Tables[SelectedTableNameINT].Columns[i].type != SqlDbType.Bit)
                {
                    TextBox temp = new TextBox();//Создание нового бокса с условием
                    temp.Location = new Point(X, Y);//Его местоположение и новое имя
                    temp.Size = new Size(200, 20);
                    temp.Name = $"{db.Tables[SelectedTableNameINT].Columns[i].Name}";
                    if (!db.Tables[SelectedTableNameINT].Columns[i].IsNullable)
                        temp.Text = "NOT NULL please fill in the field";
                    temp.Anchor = AnchorStyles.Left;
                    InsertBoxes.Add(temp);//Добавление в коллекцию 
                    tabPage2.Controls.Add(InsertBoxes[InsertBoxes.Count - 1]);//Добавление на страницу
                }
                else
                {
                    CheckBox temp = new CheckBox();//Создание нового бокса с условием
                    temp.Location = new Point(X, Y);//Его местоположение и новое имя
                    temp.Size = new Size(100, 20);
                    temp.Name = $"{db.Tables[SelectedTableNameINT].Columns[i].Name}";
                    temp.Anchor = AnchorStyles.Left;
                    InsertBoxes.Add(temp);//Добавление в коллекцию 
                    tabPage2.Controls.Add(InsertBoxes[InsertBoxes.Count - 1]);//Добавление на страницу
                }
                
                Label tempo = new Label();
                tempo.Location = new Point(X - 100,Y + 3);
                tempo.Size = new Size(100,13);
                tempo.Text = $"{db.Tables[SelectedTableNameINT].Columns[i].Name}";
                tempo.Anchor = AnchorStyles.Left;
                labels.Add(tempo);//Добавление в коллекцию 
                tabPage2.Controls.Add(labels[labels.Count - 1]);//Добавление на страницу

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
            button.Text = $"Insert";
            button.Anchor = AnchorStyles.Left;
            button.Click += button1_Click;
            tabPage2.Controls.Add(button);//Добавление на страницу
        }
        
        private void button1_Click(object sender, EventArgs e)//Короче вместо всей этой херни автогенерируемую форму сделать и не париться...
        {
            ((DataTable) SelectedTable.DataSource).Rows.Add();
            //SelectedTable.Rows[0].Cells[i].Value = InsertBoxes[i].Text;
            int tempINdex = 0;

                for (int i = 0; i < SelectedTable.Rows[0].Cells.Count; i++)
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

            string Query = $"INSERT INTO [{SelectedTableName}] (";
            for (int i = 0; i < InsertBoxes.Count; i++)
            {
                if (InsertBoxes[i].Text != string.Empty)
                {
                    Query += $"[{InsertBoxes[i].Name}], ";
                }
            }
            Query = Query.Remove(Query.Length - 2);
            Query += ") VALUES (";
            SqlCommand sqlCommand = new SqlCommand(Query, db.connection);
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
            ColumnsSELECT.Text = Query;

            IsUpdate = true;
            Thread UpdateThread = new Thread(() => db.SetQueryAsync(Query, sqlCommand));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}


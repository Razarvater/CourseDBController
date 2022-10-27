﻿using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private bool ActiveDragAndDrop = false;//включено ли перетаскивание объектов диаграммы
        private PictureBox Lines;//PictureBox для отрисовки связей
        private Control SelectedControl;//Выбранный элемент
        private Graphics gr;//Объект для отрисовки связей
        private void PrintShema()//Инициализация схемы БД
        {
            Random rnd = new Random();
            tabPage8.Controls.Clear();//Очистка от предыдущей диаграммы

            //Создание Picture box'a 
            Lines = new PictureBox();
            Lines.BorderStyle = BorderStyle.FixedSingle;
            Lines.Dock = DockStyle.Fill;
            Lines.Location = new Point(3, 3);
            Lines.Name = "pictureBox1";
            Lines.Size = new Size(796, 342);
            Lines.TabIndex = 0;
            Lines.TabStop = false;
            Lines.SizeChanged += PrintConstraint;
            tabPage8.Controls.Add(Lines);
            
            for (int i = 0; i < db.Tables.Count; i++)//Определдение таблиц и их отрисовка
            {
                DataGridView temp = new DataGridView();
                //Случайное положение
                temp.Location = new Point(rnd.Next(0,tabPage8.Width - 100), rnd.Next(0, tabPage8.Height - 100));
                //Определение содержимого таблиц
                DataTable tempData = new DataTable();
                tempData.Columns.Add(new DataColumn(db.Tables[i].name));
                for (int j = 0; j < db.Tables[i].ColumnsNames.Count; j++)
                {
                    DataRow tempo = tempData.NewRow();
                    tempo[db.Tables[i].name] = db.Tables[i].ColumnsNames[j];
                    tempData.Rows.Add(tempo);
                }
                temp.Name = db.Tables[i].name;

                temp.DataSource = tempData;
                //Права редактирования(почему то не работают до конца, всё равно если изловчиться можно отредачить)
                temp.AllowUserToDeleteRows = false;
                temp.AllowUserToResizeColumns = false;
                temp.AllowUserToResizeRows = false;
                temp.MultiSelect = false;
                temp.ReadOnly = true;
                temp.TabStop = false;
                temp.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                //АвтоSize
                temp.Size = new Size(tempData.Columns[0].ColumnName.Length*15, tempData.Rows.Count * 22 + 47);
                temp.RowHeadersWidth = 4;
                //Подписка на все события
                temp.MouseUp += MouseUpDrop;
                temp.MouseDown += MouseDownDrop;
                temp.MouseMove += MoveDrop;
                temp.PreviewKeyDown += MainForm_PreviewKeyDown;
                tabPage8.Controls.Add(temp);
                tabPage8.Controls[tabPage8.Controls.Count - 1].BringToFront();//Отображение передним планом
            }
            PrintConstraint(new object(), EventArgs.Empty);//Отображение связей
        }
        private void PrintConstraint(object sender, EventArgs e)
        {
            //Новый Bitmap для PictureBox
            Lines.Image = new Bitmap(Lines.Width, Lines.Height);
            gr = Graphics.FromImage(Lines.Image);
            gr.Clear(Color.White);//Очищаем поле
            Pen blackPen = new Pen(Color.Black,1);//Чёрный цвет отриисовки
            for (int i = 0; i < db.Constrains.Count; i++)//Нахождение связей(можно оптимизировать)
            {
                Point size1 = new Point(0,0);
                Point size2 = new Point(0,0);

                for (int j = 0; j < tabPage8.Controls.Count; j++)
                {
                    //Определение первичного и внешнего ключей(добавить различие в отображении)
                    if (tabPage8.Controls[j].Name == db.Constrains[i].TableNamePK)
                        size1 = new Point(tabPage8.Controls[j].Location.X + tabPage8.Controls[j].Width/2, tabPage8.Controls[j].Location.Y + tabPage8.Controls[j].Height/2);
                    else if (tabPage8.Controls[j].Name == db.Constrains[i].TableNameFK)
                        size2 = new Point(tabPage8.Controls[j].Location.X + tabPage8.Controls[j].Width/2, tabPage8.Controls[j].Location.Y + tabPage8.Controls[j].Height/2);
                }
                if(size1.X != 0 && size1.Y != 0 && size2.X != 0 && size2.Y != 0)
                    gr.DrawLine(blackPen, size1, size2);//Отрисовка связи если она не (таблица N - таблица N) 
            }
            gr.Dispose();
            Lines.Refresh();//Перерисовка
        }
        private void MouseDownDrop(object sender, EventArgs e)//Старт перемещения
        {
            SelectedControl = (Control)sender;
            ActiveDragAndDrop = true;
        }
        private void MouseUpDrop(object sender, EventArgs e)//Остановка перемещения
        {
            SelectedControl = (Control)sender;
            ActiveDragAndDrop = false;
        }
        private void MoveDrop(object sender,EventArgs e)//перемещение объекта если он выбран
        {
            if (ActiveDragAndDrop && SelectedControl.Name == sender.GetType().GetProperty("Name").GetValue(sender).ToString())
            {
                sender.GetType().GetProperty("Location").SetValue(sender, new Point(Cursor.Position.X - 240, Cursor.Position.Y - 230));
                PrintConstraint(new object(), EventArgs.Empty);
            }
        }
        //Увеличение и уменьшение размеров таблиц в диаграмме
        private void MainForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.U)
                foreach (Control item in tabPage8.Controls)
                {
                    item.Size = new Size(item.Width - item.Width/10, item.Height - item.Height/10);
                }
            else if (e.KeyCode == Keys.I)
                foreach (Control item in tabPage8.Controls)
                {
                    item.Size = new Size(item.Width + item.Width/10, item.Height + item.Height/10);
                }

            PrintConstraint(new object(), EventArgs.Empty);//печать изменений
        }
    }
}

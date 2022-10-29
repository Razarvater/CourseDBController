using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SqlClient;
using Bd_Curs.Plogic;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private List<string> OperationsCollectionDel = new List<string> { "=", "!=", "<", ">", ">=", "<=" };//Коллекция возможных операций для WHERE
        private List<ComboBox> ColumnNamesDel;//Боксы для выбора столбцов в условии
        private List<ComboBox> OperationsDel;//Боксы для выбора операции в условии
        private List<TextBox> ValuesDel;//Боксы для выбора значения в условии
        private List<ComboBox> AndORDel;//Боксы для выбора AND OR в условиях
        private int xDel = 5;//x координата генерируемого элемента
        private int yDel = 5;//y координата генерируемого элемента
        private string DeleteQuery;//Текст запроса
        private void DeleteLocalize()
        {
            if(tabPage6.Controls.Count != 0)
            {
                tabPage6.Controls[0].Text = Localize.GetString("NewCondition");
                tabPage6.Controls[1].Text = Localize.GetString("RemoveAllConditions");
                tabPage6.Controls[2].Text = Localize.GetString("Delete");
            }
        }
        private void InitConditionsDel(object sender, EventArgs e)//Инициализация Формы
        {
            //Сброс сгенерированного интерфейса
            xDel = 5;
            yDel = 5;
            tabPage6.Controls.Clear();

            //Создание кнопки для инициализации нового условия
            Button button = new Button();
            button.Location = new Point(xDel, yDel);
            button.Size = new Size(100, 40);
            button.Text = Localize.GetString("NewCondition");
            button.Click += AddNewConditionDel;
            button.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage6.Controls.Add(button);//Добавление на страницу

            Button button2 = new Button();
            button2.Location = new Point(xDel + 110, yDel);
            button2.Size = new Size(100, 40);
            button2.Text = Localize.GetString("RemoveAllConditions");
            button2.Click += InitConditionsDel;
            button2.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage6.Controls.Add(button2);//Добавление на страницу

            //Создание кнопки для удаления
            Button buttonr = new Button();
            buttonr.Location = new Point(xDel + 220, yDel);
            buttonr.Size = new Size(100, 40);
            buttonr.Text = Localize.GetString("Delete");
            buttonr.Click += DeleteFields;
            buttonr.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage6.Controls.Add(buttonr);//Добавление на страницу
            yDel += 50;

            //Сброс сгенерированных элементов
            ColumnNamesDel = new List<ComboBox>();
            OperationsDel = new List<ComboBox>();
            ValuesDel = new List<TextBox>();
            AndORDel = new List<ComboBox>();
        }
        private void AddNewConditionDel(object sender, EventArgs e)//Генерация условия
        {
            //Пока предыдущее условие не заполнено новое создать нельзя
            if (ColumnNamesDel.Count != 0 && ColumnNamesDel[ColumnNamesDel.Count - 1].Text == string.Empty)
                return;
            if (OperationsDel.Count != 0 && OperationsDel[OperationsDel.Count - 1].Text == string.Empty)
                return;
            if (ValuesDel.Count != 0 && ValuesDel[ValuesDel.Count - 1].Text == string.Empty)
                return;
            if (AndORDel.Count != 0 && ColumnNamesDel.Count > 1 && AndOR[AndORDel.Count - 1].Text == string.Empty)
                return;

            if (AndORDel.Count != 0)
                AndORDel[AndORDel.Count - 1].Enabled = false;
            if (ColumnNamesDel.Count != 0)
                ColumnNamesDel[ColumnNamesDel.Count - 1].Enabled = false;
            if (ValuesDel.Count != 0)
                ValuesDel[ValuesDel.Count - 1].Enabled = false;
            if (OperationsDel.Count != 0)
                OperationsDel[OperationsDel.Count - 1].Enabled = false;
            AnchorStyles style = (AnchorStyles.Left | AnchorStyles.Top);//Стиль расположения кнопки

            if (ColumnNamesDel.Count > 0)//Если условие не первое то 
            {
                //Сгенерировать Бокс для AND OR
                ComboBox tempy = new ComboBox();
                if (x + 275 - 350 < 0)
                    tempy.Location = new Point(xDel + 975, yDel - 30);
                else
                    tempy.Location = new Point(xDel + 275 - 350, yDel);
                tempy.Size = new Size(50, 30);
                tempy.Anchor = style;
                tempy.Name = $"ANDOR{ColumnNamesDel.Count + 1}";
                tempy.DropDownStyle = ComboBoxStyle.DropDownList;
                tempy.Items.Add("AND");
                tempy.Items.Add("OR");
                AndORDel.Add(tempy);
                tabPage6.Controls.Add(tempy);
            }

            //Генерация бокса с именем столбца
            ComboBox temp = new ComboBox();
            temp.Location = new Point(xDel, yDel);
            temp.Size = new Size(100, 30);
            temp.Name = $"Condition{ColumnNamesDel.Count + 1}";
            temp.DropDownStyle = ComboBoxStyle.DropDownList;
            temp.Anchor = style;
            foreach (var item in db.Tables[IndexSelectedTable].ColumnsNames)
            {
                temp.Items.Add(item);
            }
            ColumnNamesDel.Add(temp);
            tabPage6.Controls.Add(temp);

            //Генерация бокса с операциям
            ComboBox tempo = new ComboBox();
            tempo.Location = new Point(xDel + 110, yDel);
            tempo.Size = new Size(35, 30);
            tempo.Name = $"Operation{ColumnNamesDel.Count + 1}";
            tempo.DropDownStyle = ComboBoxStyle.DropDownList;
            tempo.Anchor = style;
            foreach (var item in OperationsCollectionDel)
            {
                tempo.Items.Add(item);
            }
            OperationsDel.Add(tempo);
            tabPage6.Controls.Add(tempo);

            //Генерация бокса для значения условия
            TextBox tempr = new TextBox();
            tempr.Location = new Point(xDel + 155, yDel);
            tempr.Size = new Size(100, 30);
            tempr.Name = $"Value{ColumnNamesDel.Count + 1}";
            tempr.Anchor = style;
            ValuesDel.Add(tempr);
            tabPage6.Controls.Add(tempr);

            xDel += 350;
            if (xDel + 200 > tabPage6.Width)//Переход на следующую строку генерации
            {
                xDel = 5;
                yDel += 30;
            }
        }
        private void DeleteFields(object sender, EventArgs e)//Выполнить запрос на удаление
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
            Query_IsWorking = true;//Запрос выполняется

            DeleteQuery = $"DELETE FROM [{SelectedTableName}] ";//Из таблицы
            List<SqlParameterStr> parameters = new List<SqlParameterStr>();
            if (ColumnNamesDel.Count > 0)//Если условий для удаления нет
            {
                DeleteQuery += "WHERE ";
                for (int i = 0; i < ColumnNamesDel.Count; i++)//Добавление условий 
                {
                    DeleteQuery += $"[{ColumnNamesDel[i].Text}] {OperationsDel[i].Text} @{ValuesDel[i].Name}";
                    parameters.Add(new SqlParameterStr($"@{ValuesDel[i].Name}", ValuesDel[i].Text));
                    if (i != ColumnNamesDel.Count - 1)//Если количество не 1 то добавить AND OR
                        DeleteQuery += $" {AndORDel[i].Text} ";
                }
            }
            else
            {
                Query_IsWorking = false;//Закрытие запроса
                return;
            }
            Thread UpdateThread = new Thread(() => db.SetQuery(DeleteQuery, parameters));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}

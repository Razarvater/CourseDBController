using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form 
    {
        private string SelectQuery;//Текст запроса
        private List<CheckBox> CheckBoxes;//Боксы для статуса выбрано поле для отображения или нет
        private List<Label> SelectLabels;//Имена полей
        private List<string> OperationsCollection = new List<string> {"=","!=","<",">",">=","<="};//Коллекция возможных операций для WHERE
        private List<ComboBox> ColumnNames;//Боксы для выбора столбцов в условии
        private List<ComboBox> Operations;//Боксы для выбора операции в условии
        private List<TextBox> Values;//Боксы для выбора значения в условии
        private List<ComboBox> AndOR;//Боксы для выбора AND OR в условиях
        private int x = 5;//x координата генерируемого элемента
        private int y = 5;//y координата генерируемого элемента
        private void InitConditions(object sender, EventArgs e)//Инициализация условий формы
        {
            //Сброс сгенерированного интерфейса
            x = 5;
            y = 5;
            tabPage5.Controls.Clear();

            //Кнопка для добавления нового условия
            Button button = new Button();
            button.Location = new Point(x, y);
            button.Size = new Size(100, 40);
            button.Text = $"New Condition";
            button.Click += AddNewCondition;
            button.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage5.Controls.Add(button);//Добавление на страницу

            Button button2 = new Button();
            button2.Location = new Point(x + 110, y);
            button2.Size = new Size(100, 40);
            button2.Text = $"RemoveAllConditions";
            button2.Click += InitConditions;
            button2.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage5.Controls.Add(button2);//Добавление на страницу
            y += 50;

            //Сброс сгенерированных элементов
            ColumnNames = new List<ComboBox>();
            Operations = new List<ComboBox>();
            Values = new List<TextBox>();
            AndOR = new List<ComboBox>();
        }
        private void AddNewCondition(object sender, EventArgs e)//Добавление нового условия
        {
            //Пока предыдущее условие не заполнено новое создать нельзя
            if (ColumnNames.Count != 0 && ColumnNames[ColumnNames.Count - 1].Text == string.Empty)
                return;
            if (Operations.Count != 0 && Operations[Operations.Count - 1].Text == string.Empty)
                return;
            if (Values.Count != 0 && Values[Values.Count - 1].Text == string.Empty)
                return;
            if (AndOR.Count != 0 && ColumnNames.Count > 1 && AndOR[AndOR.Count - 1].Text == string.Empty)
                return;

            if(AndOR.Count != 0)
                AndOR[AndOR.Count - 1].Enabled = false;
            if(ColumnNames.Count != 0)
                ColumnNames[ColumnNames.Count - 1].Enabled = false;
            if(Values.Count != 0)
                Values[Values.Count - 1].Enabled = false;
            if(Operations.Count != 0)
                Operations[Operations.Count - 1].Enabled = false;

            AnchorStyles style = (AnchorStyles.Left | AnchorStyles.Top);//Стиль расположения кнопки
            if (ColumnNames.Count>0)//Если условие не первое то 
            {
                //Сгенерировать Бокс для AND OR
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
            //Генерация бокса с именем столбца
            ComboBox temp = new ComboBox();
            temp.Location = new Point(x,y);
            temp.Size = new Size(100, 30);
            temp.Name = $"Condition{ColumnNames.Count + 1}";
            temp.DropDownStyle = ComboBoxStyle.DropDownList;
            temp.Anchor = style;
            foreach (var item in db.Tables[IndexSelectedTable].ColumnsNames)
            {
                temp.Items.Add(item);
            }
            ColumnNames.Add(temp);
            tabPage5.Controls.Add(temp);

            //Генерация бокса с операциям
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

            //Генерация бокса для значения условия
            TextBox tempr = new TextBox();
            tempr.Location = new Point(x + 155, y);
            tempr.Size = new Size(100, 30);
            tempr.Name = $"Value{ColumnNames.Count + 1}";
            tempr.Anchor = style;
            Values.Add(tempr);
            tabPage5.Controls.Add(tempr);

            x += 350;
            if (x + 200 > tabPage5.Width)//Переход на следующую строку генерации
            {
                x = 5;
                y += 30;
            }
        }
        private void InitSelectForm()//Инициализация формы
        {
            //Сброс сгенерированного интерфейса
            int Y = 10;
            int X = 100;
            tabPage4.AutoScrollMinSize = new Size(0, (db.Tables[IndexSelectedTable].Columns.Count * 30) / 3 + 100);
            CheckBoxes = new List<CheckBox>();
            SelectLabels = new List<Label>();
            tabPage4.Controls.Clear();

            for (int i = 0; i < db.Tables[IndexSelectedTable].Columns.Count; i++)//Создание для всех колонок
            {
                //Бокс с условием выбора поля
                CheckBox temp = new CheckBox();//Создание нового бокса с условием
                temp.Location = new Point(X, Y);//Его местоположение и новое имя
                temp.Size = new Size(100, 20);
                temp.Name = $"{db.Tables[IndexSelectedTable].Columns[i].Name}SELECT";
                temp.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                temp.CheckedChanged += CheckClick;
                CheckClick(temp, EventArgs.Empty);
                if (db.Tables[IndexSelectedTable].Columns[i].IsPrimaryKey)
                {
                    temp.Enabled = false;
                    temp.Checked = true;
                }
                CheckBoxes.Add(temp);//Добавление в коллекцию 
                tabPage4.Controls.Add(CheckBoxes[CheckBoxes.Count - 1]);//Добавление на страницу
                
                //Имя поля
                Label tempo = new Label();
                tempo.Location = new Point(X - 100, Y + 3);
                tempo.Size = new Size(100, 13);
                tempo.Text = $"{db.Tables[IndexSelectedTable].Columns[i].Name}";
                tempo.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                tempo.TextAlign = ContentAlignment.MiddleRight;
                SelectLabels.Add(tempo);//Добавление в коллекцию 
                tabPage4.Controls.Add(SelectLabels[SelectLabels.Count - 1]);//Добавление на страницу

                X += 300;
                if (X + 200 > tabPage2.Width)//Переход на следующую строку генерации
                {
                    X = 100;
                    Y += 30;
                }
            }
            //Кнопка для выполнения выборки
            Button button = new Button();
            button.Location = new Point(10, Y + 30);
            button.Size = new Size(100, 40);
            button.Text = $"Select";
            button.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            button.Click += button3_Click;
            tabPage4.Controls.Add(button);//Добавление на страницу
            InitConditions(1, EventArgs.Empty);
        }  
        private void button3_Click(object sender, EventArgs e)//Выполнить запрос из формы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
            Query_IsWorking = true;//Запрос выполняется
            SelectQuery = "SELECT ";//Выборка

            foreach (var item in CheckBoxes)
            {
                if (item.Checked)
                    SelectQuery += $"[{item.Name.Remove(item.Name.Length - 6)}], ";
                
            }
            SelectQuery = SelectQuery.Remove(SelectQuery.Length - 2);

            SelectQuery += $" FROM [{SelectedTableName}] ";//Из таблицы
            SqlCommand command = new SqlCommand(SelectQuery, db.connection);
            //Создание параметризированного запроса
            if (ColumnNames.Count > 0)//Добавление условий если они есть
            {
                SelectQuery += "WHERE ";
                for (int i = 0; i < ColumnNames.Count; i++)
                {
                    SelectQuery += $"[{ColumnNames[i].Text}] {Operations[i].Text} @{Values[i].Name}";
                    command.Parameters.Add(new SqlParameter($"@{Values[i].Name}", Values[i].Text));
                    if (i != ColumnNames.Count - 1)//Если условий больше 1
                        SelectQuery += $" {AndOR[i].Text} ";
                }
            }
            command.CommandText = SelectQuery;

            Thread UpdateThread = new Thread(() => db.SetQuery(SelectQuery,command));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}

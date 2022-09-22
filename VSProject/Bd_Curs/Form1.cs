using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private Database db;//Переменная базы данных
        private bool db_Connected = false;//Подключена ли БД
        private int ButtonsMin = 100;//Кнопки таблиц
        private string SelectedTableName;
        private Stopwatch sw = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
            WhereSelectBoxes.Add(ConditionSelect);//Добавления текстбокс с условием в коллекцию
            tabControl1.Enabled = false;
        }
        public void Message(string mess) => MessageBox.Show(mess);
        private void timer1_Tick(object sender, EventArgs e) => IsQueue();
        private void UpdateTimer_Tick(object sender, EventArgs e) => UpdateTable();


        private async void ConnectButton_Click(object sender, EventArgs e)//Подключение к БД
        {

            if (!db_Connected)//Если БД не подключена то
            {
                db = new Database(ServerName.Text, DbName.Text);//Создать новое подключение к БД
            }
            else
            {
                db.CloseConnection();//Закрыть старое подключение
                db = new Database(ServerName.Text, DbName.Text);//Создать новое подключение к БД
            }

            db.Show += Message;
            db.UpdateData += UpdateTable;

            await db.startConnectionAsync(NameBox.Text, PassBox.Text);//Подключение к базе данных
            db_Connected = db.Connected;//Подключена ли БД

            if (db_Connected)//Изменение сообщения о подключении
            {
                ConnectionStatus.Text = $"Was Connected to: Server:{ServerName.Text}          Database:{DbName.Text}";
            }
            else
            {
                ConnectionStatus.Text = "Wasn't Connected";
                return;
            }

            //-------------Создание кнопок таблиц------------\\
            splitContainer5.Panel2.Controls.Clear();
            int tempHeight = splitContainer5.Panel2.Height;
            if (ButtonsMin * db.TableNames.Count > splitContainer5.Panel2.Width) tempHeight -= 20;
            for (int i = 0; i < db.TableNames.Count; i++)
            {
                Button temp = new Button();

                temp.Text = db.TableNames[i].ToString();
                temp.Location = new Point(ButtonsMin * i, 0);
                temp.Size = new Size(ButtonsMin, tempHeight);
                temp.Click += new EventHandler(ChooseNewTable);

                splitContainer5.Panel2.Controls.Add(temp);
            }
            SetSelectedtable();
            button1.Enabled = true;//Включение кнопки для дисконекта от БД

            tabControl1.Enabled = true;
        }
        private void SetSelectedtable(string name = "c")
        {
            if (name == "c") name = db.TableNames[0];

            SelectedTable.DataSource = null;
            QueueTimer.Start();
            RunCounter();
            Thread UpdateThread = new Thread(() => db.GetSelectedTable($"{name}"));
            UpdateThread.Start();

        }
        private void IsQueue()
        {
            if (db.queue.Count != 0)
            {
                db.queue.Clear();
                UpdateTimer.Start();

                QueueTimer.Stop();
            }
        }
        private void UpdateTable()
        {
            UpdateTimer.Stop();
            SelectedTable.DataSource = db.TableData;

            if (SelectedTable.Columns.Count <= 10) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            else SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

            SelectedTable.Sort(SelectedTable.Columns[0], ListSortDirection.Ascending);
            StopCounter();
        }

        private void ChooseNewTable(object sender, EventArgs e)//Выбор другой таблицы
        {
            SelectedTableName = sender.GetType().GetProperty("Text").GetValue(sender).ToString();//Задание выбранной таблицы
            SetSelectedtable(SelectedTableName);//Печать таблицы указанной в названий кнопки
        }

        private void Disconnect_Click(object sender, EventArgs e)//Дисконнект
        {
            db.CloseConnection();

            SelectedTable.Columns.Clear();//Очистка выделенного поля
            splitContainer5.Panel2.Controls.Clear();

            db_Connected = db.Connected;//Подключена ли БД

            ConnectionStatus.Text = "Wasn't Connected";//Изменение строки подключения

            button1.Enabled = false;//Выключение кнопки дисконнекта

            tabControl1.Enabled = false;
            CounterOfConnection.BackColor = SystemColors.AppWorkspace;
            CounterOfConnection.Text = string.Empty;

        }
        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectedTable.DataSource = null;
                Thread UpdateThread = new Thread(() => db.SetQueryAsync(textBox2.Text));
                UpdateThread.Start();
                QueueTimer.Start();
                RunCounter();
            }

        }

        //------------SELECT-FORM------------\\
        private bool ISDistinct = false;
        private List<TextBox> WhereSelectBoxes = new List<TextBox>();
        private string SelectQuery;
        private byte LimitWhere = 6;
        private void DISTINCT_CheckedChanged(object sender, EventArgs e) => ISDistinct = !ISDistinct;

        private void button3_Click(object sender, EventArgs e)//Выполнить запрос из формы
        {
            SelectQuery = "SELECT ";//Выборка
            if (ISDistinct) SelectQuery += "DISTINCT ";//Если требуется уборка повторений

            if (ColumnsSELECT.Text == string.Empty) SelectQuery += "*";//Все столбцы или только выбранные
            else SelectQuery += ColumnsSELECT.Text;

            SelectQuery += $" FROM {TableNameSELECT.Text} ";//Из таблицы

            if (WhereSelectBoxes[0].Text != string.Empty)//Добавление условий при их наличии
            {
                SelectQuery += $"WHERE {WhereSelectBoxes[0].Text} ";//Добавление первого условия
                foreach (var item in WhereSelectBoxes)//Добавление всех остальных условий
                {
                    if (item.Text != string.Empty)//Если оно не пустое
                        SelectQuery += $"AND {item.Text} ";//Добавить условие
                }
            }

                Thread UpdateThread = new Thread(() => db.SetQueryAsync(SelectQuery));
                UpdateThread.Start();
            QueueTimer.Start();

            RemoveNewConditions();
        }
        private void RemoveNewConditions()//Удалить добавленные пользовательские условия
        {
            WhereSelectBoxes.Clear();//удаление боксов с условиями из списка
            WhereSelectBoxes.Add(ConditionSelect);//Добавление первого бокса в список

            string tempName;
            foreach (var item in tabPage3.Controls)//Удаление всех текстбоксов с цифрой в конце(генерируемых)
            {
                if ((item as TextBox) is TextBox)
                {
                    tempName = item.GetType().GetProperty("Name").GetValue(item).ToString();//Получение имени
                    if (char.IsDigit(tempName[tempName.Length - 1]))//Проверка последнего символа
                        tabPage3.Controls.Remove((Control)item);//Удаление
                }
            }
        }

        private void AddNewConditionButton_Click(object sender, EventArgs e)//Добавить новое условие
        {
            if (WhereSelectBoxes.Count >= LimitWhere) return;//Не добавлять новое условие если достигнут лимит

            TextBox temp = new TextBox();//Создание нового бокса с условием

            //Его местоположение и новое имя
            temp.Location = new Point(WhereSelectBoxes[WhereSelectBoxes.Count - 1].Location.X + WhereSelectBoxes[WhereSelectBoxes.Count - 1].Size.Width + 10, 56);
            temp.Name = $"ConditionSelect{WhereSelectBoxes.Count}";
            WhereSelectBoxes.Add(temp);//Добавление в коллекцию 

            tabPage3.Controls.Add(WhereSelectBoxes[WhereSelectBoxes.Count - 1]);//Добавление на страницу
        }

        private void UpdateTimeTimer_Tick(object sender, EventArgs e)
        {
            CounterOfConnection.Text = $"{(decimal)sw.ElapsedMilliseconds/(decimal)1000}";
        }
        private void RunCounter()
        {
            sw.Restart();
            UpdateTimeTimer.Start();
            CounterOfConnection.BackColor = SystemColors.AppWorkspace;
        }
        private void StopCounter()
        {
            sw.Stop();
            UpdateTimeTimer.Stop();
            CounterOfConnection.BackColor = Color.White;
        }
    }
}

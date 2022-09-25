using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private Database db;//Переменная базы данных
        private bool db_Connected = false;//Подключена ли БД
        private bool Query_IsWorking = false;
        private bool IsError = false;
        private int ButtonsMin = 100;//Кнопки таблиц
        private string SelectedTableName;
        private int IndexSelectedTable = 0;
        private Stopwatch sw = new Stopwatch();
        public Form1()
        {
            InitializeComponent();
            WhereSelectBoxes.Add(ConditionSelect);//Добавления текстбокс с условием в коллекцию
            tabControl1.Enabled = false;
        }
        public void Message(string mess) 
        {
            IsError = true;
            MessageBox.Show(mess);//сообщение от класса Database
        }
        private void Timer1_Tick(object sender, EventArgs e) => IsQueue();//Таймер проверки запроса
        private void UpdateTimer_Tick(object sender, EventArgs e) => UpdateTable();//Таймер на обновление таблицы
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

            await db.startConnectionAsync(NameBox.Text, PassBox.Text);//Подключение к базе данных
            db_Connected = db.Connected;//Подключена ли БД

            if (db_Connected)//Изменение сообщения о подключении
            {
                ConnectionStatus.Text = $"Was Connected to: Server:{ServerName.Text}          Database:{DbName.Text}";
                SelectedTableName = db.TableNames[0];
            }
            else
            {
                ConnectionStatus.Text = "Wasn't Connected";
                return;
            }
            //-------------Создание кнопок таблиц------------\\
            splitContainer5.Panel2.Controls.Clear();//Очистка предыдущих кнопок
            int tempHeight = splitContainer5.Panel2.Height - 4;//Установка высоты для кнопок
            //уменьшить высоту кнопок если они не помещаются в контейнер
            if (ButtonsMin * db.TableNames.Count > splitContainer5.Panel2.Width) tempHeight -= 16;
            for (int i = 0; i < db.TableNames.Count; i++)//Создание кнопок для всех таблиц
            {
                Button temp = new Button();

                temp.Text = db.TableNames[i].ToString();//Установка текста кнопки
                temp.Location = new Point(ButtonsMin * i, 0);//Установка следующей позиции кнопки
                temp.Size = new Size(ButtonsMin, tempHeight);//Установка размера кнопки
                temp.Click += new EventHandler(ChooseNewTable);//Установка события на смену отображаемой таблицы

                splitContainer5.Panel2.Controls.Add(temp);//Отображение кнопки
            }
            SetSelectedtable();//Отобразить первую таблицу
            button1.Enabled = true;//Включение кнопки для дисконекта от БД

            tabControl1.Enabled = true;//Включение контроллера таблиц
        }
        private void SetSelectedtable(string name = "DeFaUlT_TaBlE")
        {

            if (name == "DeFaUlT_TaBlE") name = db.TableNames[0];//Если имя таблицы не задано то выбрать первое из доступных

            SelectedTable.DataSource = null;//очистка выделенной таблицы
            Thread UpdateThread = new Thread(() => db.GetSelectedTable($"{name}"));//Создание потока с запросом
            UpdateThread.Start();//Стар потока
            QueueTimer.Start();//Старт таймера проверки запроса
            RunCounter();//Старт счётчиков
        }
        private void IsQueue()
        {
            if (db.IsQueryCompleted)//Если запрос выполнен
            {
                db.IsQueryCompleted = false;
                UpdateTimer.Start();//Запустить таймер обновления
                QueueTimer.Stop();//Остановить таймер проверки
            }
        }
        private void UpdateTable()
        {
            UpdateTimer.Stop();//Остановить таймер
            if (!IsError)
            {
                SelectedTable.DataSource = db.TableData;//Установить новый источник данных
                SelectedTable.RowHeadersWidth = db.TableData.Rows.Count.ToString().Length * 4 + 50;
                //Выбор форматирования таблицы
                if (SelectedTable.Columns.Count <= 10) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                else SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;

                //Сортировка таблицы по первому столбцу
                SelectedTable.Sort(SelectedTable.Columns[0], ListSortDirection.Ascending);
            }
            IsError = false;
            StopCounter();//Остановка счётчика запроса
            Query_IsWorking = false;//запрос не выполняется
        }
        private void ChooseNewTable(object sender, EventArgs e)//Выбор другой таблицы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать

            splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.Transparent;
            Query_IsWorking = true;
            SelectedTableName = sender.GetType().GetProperty("Text").GetValue(sender).ToString();//Задание выбранной таблицы
            IndexSelectedTable = Array.IndexOf(db.TableNames.ToArray(), SelectedTableName);//Индекс выбранной таблицы
            splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.LightGreen;
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

            tabControl1.Enabled = false;//Отключение контроллера форм
            CounterOfConnection.BackColor = SystemColors.AppWorkspace;//Изменение цвета отображения времени последнего запроса
            CounterOfConnection.Text = string.Empty;

        }
        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
                Query_IsWorking = true;//Запрос выполняется
                SelectedTable.DataSource = null;//Очистка выводящейся таблицы
                Thread UpdateThread = new Thread(() => db.SetQueryAsync(textBox2.Text));//Создание нового потока с запросом
                UpdateThread.Start();//Старт потока
                QueueTimer.Start();//Старкт таймера проверяющего закончился ли выполнятся поток
                RunCounter();//Таймер отвечающий за отображение счётчика времени выполнения запроса
            }
        }
        private void UpdateTimeTimer_Tick(object sender, EventArgs e)=>
            CounterOfConnection.Text = $"Time of query: {(decimal)sw.ElapsedMilliseconds/(decimal)1000}";//Обновление счётчика
        private void RunCounter()
        {
            sw.Restart();//Рестарт счётчика времени
            UpdateTimeTimer.Start();//Старт таймера для отображения времени
        }
        private void StopCounter()
        {
            sw.Stop();//Остановка счётчика времени
            UpdateTimeTimer.Stop();//Остановка таймера для отображения времени
        }
        private bool IsQueryWorked()
        {
            //Сообщение если пользователь нажал на кнопку, но предыдущий запрос ещё не выполнился
            if (Query_IsWorking)
            {
                MessageBox.Show("Query already in progress please wait");//Сообщение если пользователь нажал на кнопку, но предыдущий запрос ещё не выполнился
                return true;
            }
            else
                return false;
        }
        private void SelectedTable_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)//Создание индексов элементов слева
        {
            int Index = e.RowIndex;
            string indexStr = (Index + 1).ToString();
            object header = SelectedTable.Rows[Index].HeaderCell.Value;
            if (header == null || !header.Equals(indexStr))
                SelectedTable.Rows[Index].HeaderCell.Value = indexStr;
        }
    }
}

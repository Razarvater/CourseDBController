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
        private string ConnectedServer;//Имя поключенного сервера
        private bool db_Connected = false;//Подключена ли БД
        private bool Query_IsWorking = false;//Статус выполнения запроса
        private bool IsError = false;//Возникла ли ошибка
        private int ButtonsMin = 100;//Кнопки таблиц
        private string SelectedTableName;//Имя выбранной таблицы
        private int IndexSelectedTable = 0;//Индекс выбраной таблицы
        private Stopwatch Query_Time = new Stopwatch();//Время выполнения запроса
        public Form1()
        {
            InitializeComponent();
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
            if (DbName.Text == string.Empty) return;

            tabControl1.SelectedTab = tabPage1;//Выбрана первая форма редактирования

            if (!db_Connected)//Если БД не подключена то
            {
                db = new Database(ConnectedServer, DbName.Text);//Создать новое подключение к БД
            }
            else
            {
                db.CloseConnection();//Закрыть старое подключение
                db = new Database(ConnectedServer, DbName.Text);//Создать новое подключение к БД
            }

            db.Show += Message;//Подключение Message

            await db.startConnectionAsync(NameBox.Text, PassBox.Text);//Подключение к базе данных
            db_Connected = db.Connected;//Подключена ли БД

            if (db_Connected)//Изменение сообщения о подключении
            {
                ConnectionStatus.Text = $"Server:{ConnectedServer}          Database:{DbName.Text}";
                SelectedTableName = db.TableNames[0];//Выбранная таблица
            }
            else return;
            //-------------Создание кнопок таблиц------------\\
            splitContainer5.Panel2.Controls.Clear();//Очистка предыдущих кнопок
            int tempHeight = splitContainer5.Panel2.Height - 5;//Установка высоты для кнопок
            int tempCount = 0;
            //уменьшить высоту кнопок если они не помещаются в контейнер
            if (ButtonsMin * db.TableNames.Count > splitContainer5.Panel2.Width) tempHeight -= 16;
            for (int i = 0; i < db.TableNames.Count; i++)//Создание кнопок для всех таблиц
            {
                if (db.TableNames[i] == "Users")//Если таблица Users
                {
                    Button tempr = new Button();

                    tempr.Text = db.TableNames[i].ToString();//Установка текста кнопки
                    tempr.Location = new Point(ButtonsMin * tempCount, 0);//Установка следующей позиции кнопки
                    tempr.Size = new Size(0, 0);//Установка размера кнопки
                    tempr.Click += new EventHandler(ChooseNewTable);//Установка события на смену отображаемой таблицы

                    tempr.TabStop = false;
                    splitContainer5.Panel2.Controls.Add(tempr);//Отображение кнопки

                    continue;//То напечатать размерами 0 на 0 и скрыть под следующей кнопкой
                }
   
                Button temp = new Button();

                temp.Text = db.TableNames[i].ToString();//Установка текста кнопки
                temp.Location = new Point(ButtonsMin * tempCount, 0);//Установка следующей позиции кнопки
                temp.Size = new Size(ButtonsMin, tempHeight);//Установка размера кнопки
                temp.Click += new EventHandler(ChooseNewTable);//Установка события на смену отображаемой таблицы

                splitContainer5.Panel2.Controls.Add(temp);//Отображение кнопки
                tempCount++;

                
            }
            SetSelectedtable();//Отобразить первую таблицу
            DisconnectButton.Enabled = true;//Включение кнопки для дисконекта от БД
            IndexSelectedTable = 0;//Индекс выбранной таблицы

            tabControl1.Enabled = true;//Включение контроллера таблиц
        }
        private void SetSelectedtable(string name = "DeFaUlT_TaBlE")
        {
            tabControl1.SelectedTab = tabPage1;
            //Создание формы редактирования
            CreateInsertForm();
            //Создание формы отображения
            InitSelectForm();
            //Создание формы Удаления
            InitConditionsDel();
            if (name == "DeFaUlT_TaBlE") name = db.TableNames[0];//Если имя таблицы не задано то выбрать первое из доступных

            //Установка цвета выбранной кнопки таблицы
            splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.LightGreen;
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
            StopCounter();//Остановка счётчика запроса
                if (!IsError && !IsUpdate)
                {
                    SelectedTable.DataSource = db.TableData;//Установить новый источник данных
                    SelectedTable.RowHeadersWidth = db.TableData.Rows.Count.ToString().Length * 4 + 50;
                    //Выбор форматирования таблицы
                    if (SelectedTable.Columns.Count <= 10) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    else
                    {
                        SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        for (int i = 0; i < SelectedTable.Columns.Count; i++)
                        {
                            SelectedTable.Columns[i].Width = 100;//Установка дефолтной ширины столбцов
                        }
                    }
                    if (SelectedTable.Columns[0].Width * SelectedTable.Columns.Count <= splitContainer4.Panel2.Width) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    //Сортировка таблицы по первому столбцу
                    SelectedTable.Sort(SelectedTable.Columns[0], ListSortDirection.Ascending);
                    
                    //Переинициализация форм
                    InitConditions();
                    InitConditionsDel();
                }
                if (IsInsert && !IsError)//Создание новой строки
                {
                    NewRow();
                }
            IsError = false;
            IsUpdate = false;
            Query_IsWorking = false;//запрос не выполняется
        }
        private void ChooseNewTable(object sender, EventArgs e)//Выбор другой таблицы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
            //Сброс цвета предыдущей выбранной кнопки
            splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.Transparent;
            Query_IsWorking = true;//Обновление статуса выполнения запроса
            SelectedTableName = sender.GetType().GetProperty("Text").GetValue(sender).ToString();//Задание выбранной таблицы
            IndexSelectedTable = Array.IndexOf(db.TableNames.ToArray(), SelectedTableName);//Индекс выбранной таблицы
            splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.LightGreen;
            SetSelectedtable(SelectedTableName);//Печать таблицы указанной в названий кнопки
        }
        private void Disconnect_Click(object sender, EventArgs e)//Дисконнект
        {
            db.CloseConnection();//Закрытие подключения

            SelectedTable.Columns.Clear();//Очистка выделенного поля
            splitContainer5.Panel2.Controls.Clear();//Очистка выбранных таблиц

            db_Connected = db.Connected;//Подключена ли БД

            ConnectionStatus.Text = "Wasn't Connected";//Изменение строки подключения

            DisconnectButton.Enabled = false;//Выключение кнопки дисконнекта
            tabControl1.SelectedTab = tabPage1;//Выбор первой формы редактирования
            tabControl1.Enabled = false;//Отключение контроллера форм
            CounterOfConnection.Text = string.Empty;//Обнуление счётчика

        }
        private void UpdateTimeTimer_Tick(object sender, EventArgs e)=>
            CounterOfConnection.Text = $"Time of query: {(decimal)Query_Time.ElapsedMilliseconds/(decimal)1000}";//Обновление счётчика
        private void RunCounter()
        {
            progressBar1.Visible = true;
            Query_Time.Restart();//Рестарт счётчика времени
            UpdateTimeTimer.Start();//Старт таймера для отображения времени
        }
        private void StopCounter()
        {
            progressBar1.Visible = false;
            Query_Time.Stop();//Остановка счётчика времени
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
        //Заглушка для отсутствия ошибки о слишком длинных полях
        private void SelectedTable_DataError(object sender, DataGridViewDataErrorEventArgs e) { }
        private void ConnectServer_Click(object sender, EventArgs e)//Подключение к серверу и получение списка Баз данных
        {
            if (DisconnectButton.Enabled)
                Disconnect_Click(this,EventArgs.Empty);//Отключится от Базы данных если уже подключены к БД

            ConnectedServer = ServerName.Text;//Изменение подключенного сервера
            ServerConnection serv = new ServerConnection(ServerName.Text);//Подключение к серверу
            DbName.Items.Clear();//Очистка списка баз данных
            foreach (var item in serv.GetDatabases())//Получение списка всех Баз данных на данном сервере
            {
                DbName.Items.Add(item);//Отображение их в списке
            }
            if (DbName.Items.Count > 0)//Включение/отключение возможности подключится к БД
            {
                ConnectButton.Enabled = true;
                DbName.Enabled = true;
                NameBox.Enabled = true;
                PassBox.Enabled = true;
                ConnectionStatus.Text = $"Server:{ConnectedServer}";
            }
            else
            {
                ConnectButton.Enabled = false;
                DbName.Enabled = false;
                NameBox.Enabled = false;
                PassBox.Enabled = false;
                ConnectionStatus.Text = "Wasn't connected";
            }
        }
    }
}

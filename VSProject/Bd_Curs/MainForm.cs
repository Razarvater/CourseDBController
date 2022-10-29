using System;
using System.Resources;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using Bd_Curs.Properties;
using Bd_Curs.Plogic;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private Database db;//Переменная базы данных
        private string ConnectedServer = string.Empty;//Имя подключенного сервера
        private string FilePath = string.Empty;

        private int ButtonsMin = 100;//Размер кнопок таблиц в горизонтали

        private string SelectedTableName;//Имя выбранной таблицы
        private int IndexSelectedTable = 0;//Индекс выбраной таблицы

        private bool SettingsOpened = false;//Открыты ли настройки
        private bool db_Connected = false;//Подключена ли БД
        private bool Query_IsWorking = false;//Статус выполнения запроса
        private bool IsError = false;//Возникла ли ошибка

        private Stopwatch Query_Time = new Stopwatch();//Время выполнения запроса
        private ResourceManager Localize;//Ресурс указывающий на файлы с локализацией
        public MainForm() => InitializeComponent();
        private void MainForm_Load(object sender, EventArgs e)
        {
            LittleFormTabControl.Enabled = false;
            BigFormTabControl.Enabled = false;
            LocalizatorResource.INIT(typeof(MainForm).Assembly);//Инициализация локализатора
            Localize = LocalizatorResource.Localize;

            //Смена языка
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
            LocalizeControls();//Локализация элементов

            //Подгрузка дефолтных значений в поля
            ServerName.Text = Settings.Default.ServerName;
            if (Settings.Default.AutoLogin)//Только если включена функция авто запомненного пароля
            {
                NameBox.Text = Settings.Default.DefaultLogin;
                PassBox.Text = Settings.Default.DefaultPassword;
            }
        }
        public void ErrorMessage(string mess)//Сообщение об ошибке
        {
            IsError = true;
            MessageBox.Show(mess, Localize.GetString("DefaultErrorMessageUpper"), MessageBoxButtons.OK, MessageBoxIcon.Error);//сообщение от класса Database
        }
        private void Timer1_Tick(object sender, EventArgs e) => IsQueue();//Таймер проверки запроса
        private void UpdateTimer_Tick(object sender, EventArgs e) => UpdateTable();//Таймер на обновление таблицы
        private async void ConnectButton_Click(object sender, EventArgs e)//Подключение к БД
        {
            if (DbName.Text == string.Empty) return;
            ProgressBarMainQuery.Visible = true;
            ConnectButton.Enabled = false;//Выключение кнопки подключения
            FromFileButton.Enabled = false;
            DisconnectServer.Enabled = false;
            LittleFormTabControl.Enabled = false;//Отключение контроллера таблиц
            CreateDBButton.Enabled = false;
            CreateDBName.Visible = false;
            SelectedTable.DataSource = null;
            ConnectServer.Enabled = false;

            LittleFormTabControl.SelectedTab = tabPage1;//Выбрана первая форма редактирования
            
            if (!db_Connected)//Если БД не подключена то
            {
                db = new DatabaseMSSQL(ConnectedServer, DbName.Text, string.Empty);//Создать новое подключение к БД
            }
            else
            {
                db.CloseConnection();//Закрыть старое подключение
                db = new DatabaseMSSQL(ConnectedServer, DbName.Text, string.Empty);//Создать новое подключение к БД
            }
            db.SetShow(ErrorMessage);//Подключение Message

            await db.startConnectionAsync(NameBox.Text, PassBox.Text);//Подключение к базе данных
            db_Connected = db.Connected;//Подключена ли БД

            if (db_Connected && db.TableNames.Count != 0)
            {
                ConnectionStatus.Text = $"{Localize.GetString("ConnectionStatusServer")}:{ConnectedServer}          {Localize.GetString("ConnectionStatusDB")}:{DbName.Text}";
                SelectedTableName = db.TableNames[0];//Выбранная таблица
                IndexSelectedTable = 0;
            }
            tabPage8.Controls.Clear();//Очистка от предыдущей диаграммы
                //-------------Создание кнопок таблиц------------\\
                splitContainer5.Panel2.Controls.Clear();//Очистка предыдущих кнопок
                TableInfoTabControl.TabPages.Clear();//Очистка информации о таблицах
                if (db_Connected && db.TableNames.Count != 0)
                {
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

                            tempr.TabStop = false;
                            splitContainer5.Panel2.Controls.Add(tempr);//Отображение кнопки

                            continue;//То напечатать размерами 0 на 0 и скрыть под следующей кнопкой
                        }

                        Button temp = new Button();

                        temp.Text = db.TableNames[i].ToString();//Установка текста кнопки
                        temp.Location = new Point(ButtonsMin * tempCount, 0);//Установка следующей позиции кнопки
                        temp.Size = new Size(ButtonsMin, tempHeight);//Установка размера кнопки
                        temp.Click += ChooseNewTable;//Установка события на смену отображаемой таблицы
                        temp.KeyDown += DropTable;
                        temp.MouseEnter += new EventHandler((object sender, EventArgs e) => ((Control)sender).Focus());
                        splitContainer5.Panel2.Controls.Add(temp);//Отображение кнопки
                        TableInfoTabControl.TabPages.Add(new TabPage(db.TableNames[i]));//Отображение информации о таблицах
                        TableInfoTabControl.TabPages[TableInfoTabControl.TabCount - 1].AutoScroll = true;
                        tempCount++;
                    }
                    SetSelectedtable();//Отобразить первую таблицу

                    //Установить цвет выбранных кнопок
                    if (splitContainer5.Panel2.Controls.Count > IndexSelectedTable)
                        splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.Transparent;
                    IndexSelectedTable = 0;//Индекс выбранной таблицы
                    splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.LightGreen;

                    PrintShema();//напечатать схему бд
                    //Инициализация вкладок
                    InitTableForms();
                }
            if (db_Connected)//Изменение сообщения о подключении
            {
                //Инициализация вкладок
                InitCreateTableForm();
                InitTableRelation();

                DisconnectButton.Enabled = true;//Включение кнопки для дисконекта от БД
                if (db.TableNames.Count != 0)
                    LittleFormTabControl.Enabled = true;//Включение контроллера таблиц
                BigFormTabControl.Enabled = true;//Включение контроллера таблиц
            }
            else
            {
                ConnectButton.Enabled = true;//Выключение кнопки подключения
                FromFileButton.Enabled = true;
                LittleFormTabControl.Enabled = false;//Отключение контроллера таблиц
                CreateDBButton.Enabled = true;
                CreateDBName.Visible = false;
                DisconnectServer.Enabled = true;
                SelectedTable.DataSource = null;
                ConnectServer.Enabled = true;
            }
            ProgressBarMainQuery.Visible = false;
        }
        private void SetSelectedtable(string name = "DeFaUlT_TaBlE")
        {
            LittleFormTabControl.SelectedTab = tabPage1;
            //Создание формы редактирования
            CreateInsertForm();
            //Создание формы отображения
            InitSelectForm();
            //Создание формы Удаления
            InitConditionsDel(1,EventArgs.Empty);
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
                    InitConditions(1, EventArgs.Empty);
                    InitConditionsDel(1,EventArgs.Empty);
                    PrintKeys();
                }
                if (IsInsert && !IsError)//Создание новой строки
                {
                    NewRow();
                }
            if (IsError && IsUpdate)
                SetSelectedtable(SelectedTableName);
            IsError = false;
            IsUpdate = false;
            IsInsert = false;
            Query_IsWorking = false;//запрос не выполняется
        }
        private void PrintKeys()//Отображение первичных ключей
        {
            int CountSearchedPK = 0;//Количество найденных первичных ключей
            for (int i = 0; i < SelectedTable.Columns.Count; i++)//Проход по всем колонкам
            {
                if (CountSearchedPK == db.Tables[IndexSelectedTable].PrimaryKeys.Count)//Если найдены все ключи
                    break;
                for (int j = 0; j < db.Tables[IndexSelectedTable].PrimaryKeys.Count; j++)//Проход по всем первичным ключам таблицы
                {
                    if (db.Tables[IndexSelectedTable].PrimaryKeys[j] == SelectedTable.Columns[i].HeaderText)
                    {
                        SelectedTable.Columns[i].HeaderText = $"{SelectedTable.Columns[i].HeaderText}🔑";
                        CountSearchedPK++;
                        break;
                    }
                    
                }
            }
            CountSearchedPK = 0;//Количество найденных внешних ключей
            for (int i = 0; i < SelectedTable.Columns.Count; i++)//Проход по всем колонкам
            {
                if (CountSearchedPK == db.Tables[IndexSelectedTable].ForeignKeys.Count)//Если найдены все ключи
                    break;
                for (int j = 0; j < db.Tables[IndexSelectedTable].ForeignKeys.Count; j++)//Проход по всем внешним ключам таблицы
                {
                    if (db.Tables[IndexSelectedTable].ForeignKeys[j] == SelectedTable.Columns[i].HeaderText)
                    {
                        SelectedTable.Columns[i].HeaderText = $"{SelectedTable.Columns[i].HeaderText}🔗";
                        CountSearchedPK++;
                        break;
                    }

                }
            }
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
            splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.Transparent;
            IndexSelectedTable = 0;
            SelectedTable.Columns.Clear();//Очистка выделенного поля
            splitContainer5.Panel2.Controls.Clear();//Очистка выбранных таблиц

            db_Connected = db.Connected;//Подключена ли БД

            ConnectionStatus.Text = Localize.GetString("ConnectionStatusDis");//Изменение строки подключения

            FromFileButton.Enabled = true;
            DisconnectButton.Enabled = false;//Выключение кнопки дисконнекта
            LittleFormTabControl.SelectedTab = tabPage1;//Выбор первой формы редактирования
            LittleFormTabControl.Enabled = false;//Отключение контроллера форм
            DbConstructorFormTabControl.SelectedIndex = -1;
            BigFormTabControl.SelectedIndex = 0;
            SelectFormTabControl.SelectedIndex = -1;
            LittleFormTabControl.SelectedIndex = -1;
            BigFormTabControl.Enabled = false;
            CounterOfConnection.Text = string.Empty;//Обнуление счётчика
            DisconnectServer.Enabled = true;

            if (ConnectedServer == string.Empty) return;
            CreateDBButton.Enabled = true;
            ConnectButton.Enabled = true;//Включение кнопки подключения
        }
        private void UpdateTimeTimer_Tick(object sender, EventArgs e)=>
            CounterOfConnection.Text = $"{Localize.GetString("TimeOfQuerylabel")}: {Query_Time.ElapsedMilliseconds/1000.0}";//Обновление счётчика
        private void RunCounter()
        {
            ProgressBarMainQuery.Visible = true;
            Query_Time.Restart();//Рестарт счётчика времени
            UpdateTimeTimer.Start();//Старт таймера для отображения времени
        }
        private void StopCounter()
        {
            ProgressBarMainQuery.Visible = false;
            Query_Time.Stop();//Остановка счётчика времени
            UpdateTimeTimer.Stop();//Остановка таймера для отображения времени
        }
        private bool IsQueryWorked()
        {
            //Сообщение если пользователь нажал на кнопку, но предыдущий запрос ещё не выполнился
            if (Query_IsWorking)
            {
                ErrorMessage(Localize.GetString("QueryInProgress"));//Сообщение если пользователь нажал на кнопку, но предыдущий запрос ещё не выполнился
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
        private void SelectedTable_DataError(object sender, DataGridViewDataErrorEventArgs e) { }//Заглушка для отсутствия ошибки о слишком длинных полях
        private async void ConnectServer_Click(object sender, EventArgs e)//Подключение к серверу и получение списка Баз данных
        {
            if (ServerName.Text == string.Empty) return;
            ProgressBarServerConnection.Visible = true;
            if (DisconnectButton.Enabled)
                Disconnect_Click(this,EventArgs.Empty);//Отключится от Базы данных если уже подключены к БД
            ConnectedServer = ServerName.Text;
            ServerConnection serv = new ServerConnection(ServerName.Text);//Подключение к серверу
            serv.Show += ErrorMessage;
            DbName.Items.Clear();//Очистка списка баз данных
            await serv.GetDatabases();
            foreach (var item in serv.Databases)//Получение списка всех Баз данных на данном сервере
            {
                DbName.Items.Add(item);//Отображение их в списке
            }
            if (DbName.Items.Count > 0)//Включение/отключение возможности подключится к БД
            {
                ConnectButton.Enabled = true;
                CreateDBButton.Enabled = true;
                DbName.Enabled = true;
                NameBox.Enabled = true;
                PassBox.Enabled = true;
                DisconnectServer.Enabled = true;
                FromFileButton.Enabled = false;
                ConnectServer.Enabled = false;
                ConnectionStatus.Text = $"{Localize.GetString("ConnectionStatusServer")}:{ConnectedServer}";
            }
            else
            {
                ConnectButton.Enabled = false;
                CreateDBButton.Enabled = false;
                DbName.Enabled = false;
                NameBox.Enabled = false;
                PassBox.Enabled = false;
                FromFileButton.Enabled = true;
                ConnectServer.Enabled = true;
                ConnectionStatus.Text = Localize.GetString("ConnectionStatusDis");
            }
            ProgressBarServerConnection.Visible = false;
        }
        private void DisconnectServer_Click(object sender, EventArgs e)
        {
            ConnectedServer = string.Empty;
            ConnectButton.Enabled = false;
            CreateDBButton.Enabled = false;
            DbName.Enabled = false;
            NameBox.Enabled = false;
            PassBox.Enabled = false;
            DisconnectServer.Enabled = false;
            ConnectServer.Enabled = true;
            FromFileButton.Enabled = true;
            ConnectionStatus.Text = Localize.GetString("ConnectionStatusDis");
        }
        private void CreateDatabase(object sender, EventArgs e)
        {
            //Вопрос пользователю будем ли мы создавать БД
            DialogResult d = MessageBox.Show(Localize.GetString("CreateDbQue"), Localize.GetString("DbcreateUpper"),MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if (d == DialogResult.Yes)
            {
                CreateDBButton.Enabled = false;
                CreateDBName.Visible = true;
                CreateDBName.Text = Localize.GetString("EnterDBname");
            }
        }
        private async void CreateDBName_KeyDown(object sender, KeyEventArgs e)
        {
            //Создание БД после нажатия на Enter
            if (e.KeyCode != Keys.Enter) return;

            if (CreateDBName.Text == Localize.GetString("CreateDBNameText")) return;

            CreateDBName.Visible = false;
            ServerConnection serv = new ServerConnection(ConnectedServer);
            serv.Show += ErrorMessage;
            await serv.CreateDataBase(CreateDBName.Text);

            ConnectServer_Click(new object(), EventArgs.Empty);
        }
        private void SettingButton_Click(object sender, EventArgs e)
        {
            if (SettingsOpened)//Если настройки уже открыты
            {
                Program.sett.Focus();//Установка фокуса на форму с настройками
                return;//Выйти из метода
            }
            SettingsOpened = true;//Настройки открыты
            //Открытие настроек
            Program.sett = new SettingForm();
            //Делегаты для локализации и закрытия
            Program.sett.localizationButton += LocalizeButton_Click;
            Program.sett.disable += () => SettingsOpened = false;
            Program.sett.Show();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (OpenDbfile.ShowDialog() == DialogResult.OK)
            {
                FileConnect(OpenDbfile.FileName);
            }
        }
        private async void FileConnect(string FilePath)//Подключение к БД
        {
            if (FilePath == string.Empty) return;
            ProgressBarMainQuery.Visible = true;
            ConnectButton.Enabled = false;//Выключение кнопки подключения
            FromFileButton.Enabled = false;
            LittleFormTabControl.Enabled = false;//Отключение контроллера таблиц
            CreateDBButton.Enabled = false;
            CreateDBName.Visible = false;
            ConnectServer.Enabled = false;
            SelectedTable.DataSource = null;

            LittleFormTabControl.SelectedTab = tabPage1;//Выбрана первая форма редактирования

            if (!db_Connected)//Если БД не подключена то
            {
                db = new DatabaseMSSQL(ConnectedServer, DbName.Text, FilePath);//Создать новое подключение к БД
            }
            else
            {
                db.CloseConnection();//Закрыть старое подключение
                db = new DatabaseMSSQL(ConnectedServer, DbName.Text, FilePath);//Создать новое подключение к БД
            }
            db.SetShow(ErrorMessage);//Подключение Message

            await db.startConnectionAsync(NameBox.Text, PassBox.Text);//Подключение к базе данных
            db_Connected = db.Connected;//Подключена ли БД

            if (db_Connected && db.TableNames.Count != 0)
            {
                ConnectionStatus.Text = $"{Localize.GetString("ConnectionStatusServer")}:{FilePath}";
                SelectedTableName = db.TableNames[0];//Выбранная таблица
                IndexSelectedTable = 0;
            }
            tabPage8.Controls.Clear();//Очистка от предыдущей диаграммы
                                      //-------------Создание кнопок таблиц------------\\
            splitContainer5.Panel2.Controls.Clear();//Очистка предыдущих кнопок
            TableInfoTabControl.TabPages.Clear();//Очистка информации о таблицах
            if (db_Connected && db.TableNames.Count != 0)
            {
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

                        tempr.TabStop = false;
                        splitContainer5.Panel2.Controls.Add(tempr);//Отображение кнопки

                        continue;//То напечатать размерами 0 на 0 и скрыть под следующей кнопкой
                    }

                    Button temp = new Button();

                    temp.Text = db.TableNames[i].ToString();//Установка текста кнопки
                    temp.Location = new Point(ButtonsMin * tempCount, 0);//Установка следующей позиции кнопки
                    temp.Size = new Size(ButtonsMin, tempHeight);//Установка размера кнопки
                    temp.Click += ChooseNewTable;//Установка события на смену отображаемой таблицы
                    temp.KeyDown += DropTable;
                    temp.MouseEnter += new EventHandler((object sender, EventArgs e) => ((Control)sender).Focus());
                    splitContainer5.Panel2.Controls.Add(temp);//Отображение кнопки
                    TableInfoTabControl.TabPages.Add(new TabPage(db.TableNames[i]));//Отображение информации о таблицах
                    TableInfoTabControl.TabPages[TableInfoTabControl.TabCount - 1].AutoScroll = true;
                    tempCount++;
                }
                SetSelectedtable();//Отобразить первую таблицу

                //Установить цвет выбранных кнопок
                if (splitContainer5.Panel2.Controls.Count > IndexSelectedTable)
                    splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.Transparent;
                IndexSelectedTable = 0;//Индекс выбранной таблицы
                splitContainer5.Panel2.Controls[IndexSelectedTable].BackColor = Color.LightGreen;

                PrintShema();//напечатать схему бд
                             //Инициализация вкладок
                InitTableForms();
            }
            if (db_Connected)//Изменение сообщения о подключении
            {
                //Инициализация вкладок
                InitCreateTableForm();
                InitTableRelation();

                DisconnectButton.Enabled = true;//Включение кнопки для дисконекта от БД
                if (db.TableNames.Count != 0)
                    LittleFormTabControl.Enabled = true;//Включение контроллера таблиц
                BigFormTabControl.Enabled = true;//Включение контроллера таблиц
            }
            else
            {
                FromFileButton.Enabled = true;
                LittleFormTabControl.Enabled = false;//Отключение контроллера таблиц
                CreateDBName.Visible = false;
                SelectedTable.DataSource = null;
                ConnectServer.Enabled = true;
            }
            ProgressBarMainQuery.Visible = false;
        }
    }
}

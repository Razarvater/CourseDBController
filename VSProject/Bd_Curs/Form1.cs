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
        private bool Query_IsWorking = false;
        private bool IsError = false;
        private int ButtonsMin = 100;//Кнопки таблиц
        private string SelectedTableName;
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
        private void timer1_Tick(object sender, EventArgs e) => IsQueue();//Таймер проверки запроса
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
            splitContainer5.Panel2.Controls.Clear();//Очистка предыдущих кнопок
            int tempHeight = splitContainer5.Panel2.Height;//Установка высоты для кнопок
            //уменьшить высоту кнопок если они не помещаются в контейнер
            if (ButtonsMin * db.TableNames.Count > splitContainer5.Panel2.Width) tempHeight -= 23;
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
            if (IsError)//Если запрос с ошибкой
            {
                IsError = false;
                UpdateTimer.Stop();//Остановить таймер
                StopCounter();//Остановка счётчика запроса
                Query_IsWorking = false;//запрос не выполняется
                return;
            }
            UpdateTimer.Stop();//Остановить таймер
            SelectedTable.DataSource = db.TableData;//Установить новый источник данных

            //Выбор форматирования таблицы
            if (SelectedTable.Columns.Count <= 10) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            else SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
      
            //Сортировка таблицы по первому столбцу
            SelectedTable.Sort(SelectedTable.Columns[0], ListSortDirection.Ascending);
            StopCounter();//Остановка счётчика запроса
            Query_IsWorking = false;//запрос не выполняется
        }

        private void ChooseNewTable(object sender, EventArgs e)//Выбор другой таблицы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать

            Query_IsWorking = true;
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
        //------------SELECT-FORM------------\\
        private bool ISDistinct = false;
        private List<TextBox> WhereSelectBoxes = new List<TextBox>();
        private string SelectQuery;
        private byte LimitWhere = 6;
        private void DISTINCT_CheckedChanged(object sender, EventArgs e) => ISDistinct = !ISDistinct;//Галочка на убирание повторений в запросе
        private void button3_Click(object sender, EventArgs e)//Выполнить запрос из формы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
            Query_IsWorking = true;//Запрос выполняется
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

            Thread UpdateThread = new Thread(() => db.SetQueryAsync(SelectQuery));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
            RemoveNewConditions();//Удалить пользовательские условия
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
    }
}

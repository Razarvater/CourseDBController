using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private Database db;//Переменная базы данных
        private bool db_Connected = false;//Подключена ли БД
        private int ButtonsMin = 100;//Кнопки таблиц
        private int ChoosedPage = 1;
        private int ElementsInPage = 100;
        private int PageCountInSelectedTable;
        private string SelectedTableName;
        public Form1() => InitializeComponent();

        private async void ConnectButton_Click(object sender, EventArgs e)//Подключение к БД
        {
            
            if (!db_Connected)//Если БД не подключена то
            {
                db = new Database(ServerName.Text, DbName.Text);//Создать новое подключение к БД
            }
            else
            {
                await db.CloseConnection();//Закрыть старое подключение
                db = new Database(ServerName.Text, DbName.Text);//Создать новое подключение к БД
            }
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

            ////-------------Создание кнопок таблиц------------\\
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

            await UpdateSelectedTable(db.TableNames[1]);//Обновление напечатанной таблицы
            button1.Enabled = true;//Включение кнопки для дисконекта от БД

            NextPage.Enabled = true;//Включение кнопок переключения страниц
            PreviousPage.Enabled = true;
        }

        private async void ChooseNewTable(object sender, EventArgs e)//Выбор другой таблицы
        {
            SelectedTableName = sender.GetType().GetProperty("Text").GetValue(sender).ToString();//Задание выбранной таблицы
            await UpdateSelectedTable(SelectedTableName);//Печать таблицы указанной в названий кнопки
        }

        private async Task UpdateSelectedTable(string table)//Обновление напечатанной таблицы
        {
            await db.GetSelectedTableAsync(table);//Обновить выбранную таблицу для печати
            SelectedTable.Columns.Clear();//Очистить уже напечатанную таблицу
            ChoosedPage = 1;

            //ограничение отрисовки таблицы в горизонтали
            if (db.ColumNames.Count > 10) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            else SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            UpdateCounters();//Обновление счётчиков

            foreach (var item in db.ColumNames)//Задание имён столбцов
                SelectedTable.Columns.Add(item, item);

            //Отображение первой страницы
            for (int i = ChoosedPage * ElementsInPage - ElementsInPage; i < db.Table.Count && ChoosedPage * ElementsInPage >= i; i++)
                SelectedTable.Rows.Add(db.Table[i]);
        }

        private async void button1_Click(object sender, EventArgs e)//Дисконнект
        {
            await db.CloseConnection();

            SelectedTable.Columns.Clear();//Очистка выделенного поля
            splitContainer5.Panel2.Controls.Clear();

            db_Connected = db.Connected;//Подключена ли БД

            ConnectionStatus.Text = "Wasn't Connected";//Изменение строки подключения

            button1.Enabled = false;//Выключение кнопки дисконнекта
            NextPage.Enabled = false;//Выключение кнопок переключения страниц
            PreviousPage.Enabled = false;
        }

        private void NextPage_Click(object sender, EventArgs e)//Переход на следующую страницу
        {
            if (ChoosedPage != PageCountInSelectedTable)//Только если выбранная страница не последняя
            {
                ChoosedPage++;//Переход страницы
                UpdateCounters();//Обновление счётчиков страниц
                UpdateSelectedPage();//Обновить отображаемую страницу
            }
        }

        private void PreviousPage_Click(object sender, EventArgs e)//Переход на предыдущую страницу
        {
            if (ChoosedPage != 1)//Только если выбранная страница не первая
            {
                ChoosedPage--;//Переход страницы
                UpdateCounters();//Обновление счётчиков страниц
                UpdateSelectedPage();//Обновить отображаемую страницу
            }
        }
        private void UpdateSelectedPage()//Обновление отображаемой страницы
        {
            //ограничение отрисовки таблицы в горизонтали
            if (db.ColumNames.Count > 10) SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            else SelectedTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            SelectedTable.Columns.Clear();
            foreach (var item in db.ColumNames)//Задание имён столбцов
            {
                SelectedTable.Columns.Add(item, item);
            }

            for (int i = ChoosedPage * ElementsInPage - ElementsInPage; i < db.Table.Count && ChoosedPage * ElementsInPage >= i; i++)//Отображение первой страницы
            {
                SelectedTable.Rows.Add(db.Table[i]);
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)//Изменение количества записей на странице
        {
            if(NextPage.Enabled && e.KeyCode == Keys.Enter)
            {
                int.TryParse(textBox1.Text, out ElementsInPage);
                ChoosedPage = 1;

                UpdateCounters();
                UpdateSelectedPage();
            }
            else if(e.KeyCode == Keys.Enter)
            {
                int.TryParse(textBox1.Text, out ElementsInPage);
                ChoosedPage = 1;
            }
        }
        private void UpdateCounters()//Обновление счётчиков страниц
        {
            CountEl.Text = $"CountOfElements = {db.Table.Count}";
            PageCountInSelectedTable = (int)Math.Ceiling((decimal)db.Table.Count / (decimal)ElementsInPage);
            PagesCounter.Text = $"{ChoosedPage}/{PageCountInSelectedTable}";
            Elements.Text = $"{(ChoosedPage - 1) * ElementsInPage}-{((ChoosedPage) * ElementsInPage > db.Table.Count ? (ChoosedPage) * ElementsInPage - (ElementsInPage - (db.Table.Count - (ChoosedPage - 1) * ElementsInPage)) : (ChoosedPage) * ElementsInPage)}";
        }

        private async void textBox2_KeyUp(object sender, KeyEventArgs e)//Выполнение SQL запроса введённого от юзера
        {
            if (e.KeyCode == Keys.Enter)
            {
                await db.SetQueryAsync(textBox2.Text);//запрос
                ChoosedPage = 1;
                UpdateCounters();//Обновить счётчики страниц
                UpdateSelectedPage();//Обновить страницу
            }
        }
    }
}

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;

namespace Bd_Curs
{
    public class Database
    {
        private SqlConnection connection;//подклбючение к БД
        private SqlCommand command;//SQL команда
        public List<string> TableNames;//Имена таблиц
        public DataTable TableData = new DataTable();//Выбранная таблица

        public delegate void MessageShow(string message);//Делегат для выдачи сообщений в форму
        public event MessageShow Show;

        public bool IsQueryCompleted = false;//Статус завершённости запроса
        public bool Connected { get; set; }//Подключена ли БД
        public Database(string server,string database) =>
            connection = new SqlConnection($"Server={server};Database={database};Trusted_Connection=True;");//Создание объекта SQl connection
        public async Task startConnectionAsync(string name, string password)//Подключится к базе данных 
        {
            if (Connected) return ;//Не подключаться повторно если БД уже подключена
            try
            {
                await connection.OpenAsync();//Асинхронное открытие подключения
                Connected = true;//Подключено
            }
            catch (SqlException e) 
            {
                Show.Invoke($"An SQL exception occurred, Unable to connect to database:[{e.Message}]");
                return; 
            }

            command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME!='sysdiagrams' AND TABLE_TYPE = 'BASE TABLE'\r\n", connection);
            SqlDataReader temp = await command.ExecuteReaderAsync();
            TableNames = new List<string>();//Имена таблиц
            while(await temp.ReadAsync())//Получение имён таблиц для базы данных
            {
                TableNames.Add(temp[0].ToString());
            }
            temp.Close();

            if(Array.IndexOf(TableNames.ToArray(),"Users")!=-1)//Только если есть таблица с пользователями
                await AuthAsync(name,password);//Авторизация 
        }
        public async Task AuthAsync(string name, string password)//Авторизация в базе данных
        {
            try
            {
                command = new SqlCommand($"SELECT * FROM Users WHERE name = '{name}' AND pass = '{password}'", connection);
                SqlDataReader temp = await command.ExecuteReaderAsync();//запрос к таблице Users
                if (temp.HasRows)//Если пользователя с таким именем и паролем нет то закрыть подключение
                {
                    temp.Close();//Закрыть устройство чтения
                    return;
                }
                CloseConnection();//Закрытие подключения
            }
            catch (SqlException e)
            {
                Show.Invoke($"An SQL exception occurred, Unable to connect to Table Users:[{e.Message}]");
            }
        }
        public void CloseConnection()//Закрытие подключения
        {
            try
            {
                connection.Close();//Закрытие подключения
                Connected = false;//Не подключено
            }
            catch (SqlException e )
            {
                Show.Invoke($"An SQL exception occurred, Unable to disconnect from database:[{e.Message}]");
            }
        }
        public void GetSelectedTable(string table)
        {
            try
            {
                command = new SqlCommand($"SELECT * FROM {table}", connection);//Создание команды
                SqlDataAdapter adapter = new SqlDataAdapter(command);//Создание адаптера
                TableData = new DataTable();//Очистка предыдущей выбранной таблицы
                adapter.Fill(TableData);//Новое заполение
                IsQueryCompleted = true;//Запрос завершён
            }
            catch (SqlException e)
            {
                IsQueryCompleted = true;//Запрос завершён
                Show.Invoke($"An SQL exception occurred, please check the correctness of the entered query: [{e.Message}]");
            }
        }     
        public void SetQueryAsync(string Query)//Выполнение запроса
        {
            try
            { 
                command = new SqlCommand(Query, connection);//Создание новой SQL команды

                string TableName = string.Empty;//Имя таблицы для которой применяется запрос

                bool IsTableName = false;//Является ли следующее слово в запросе именем таблицы
                bool IsSelect = false;
                string tempstr = string.Empty;

                foreach (var item in Query)//Определение таблицы к которой применялся запрос
                {
                    if (item == ' ')//Если слово закончилось
                    {
                        if (IsTableName)//Если это было имя таблицы
                        {
                            TableName = tempstr;//Присвоить
                            break;//Прекратить перебор
                        }
                        tempstr = tempstr.ToUpper();//Приведение SQL команды к "Заглавному" состоянию
                        if (tempstr == "UPDATE" || tempstr == "FROM" || tempstr == "INTO" || tempstr == "CREATE")
                            IsTableName = true;//После этих операторов следует имя таблицы

                        if (tempstr == "DROP")//Запрет на DROP баз данных и таблиц(сделать тоже самое схемами)
                        {
                            IsQueryCompleted = true;//Запрос завершён
                            Show.Invoke("Don't DROP");
                            return;
                        }
                           
                        if (tempstr == "SELECT")//Если запрос на выборку
                            IsSelect = true;

                        tempstr = String.Empty;//Очистка предыдущего слова
                    }
                    else
                        tempstr += item;
                }
                if (IsSelect)//Если запрос на выборку
                {
                    command = new SqlCommand(Query, connection);//Создание SQL команды
                    SqlDataAdapter adapter = new SqlDataAdapter(command);//Создание адаптера
                    TableData = new DataTable();//Очистка предыдущей выбранной таблицы
                    adapter.Fill(TableData);//Заполнение новыми данными
                    IsQueryCompleted = true;//Запрос завершён
                }
                else
                {
                    command.ExecuteNonQuery();//Выполнение запроса не на выборку
                    GetSelectedTable(TableName);//Отображение таблицы к которой применялся запрос
                }
            }
            catch (SqlException e)
            {
                IsQueryCompleted = true;//Запрос завершён
                Show.Invoke($"An SQL exception occurred, please check the correctness of the entered query: [{e.Message}]");
            }
        }
    }     
}

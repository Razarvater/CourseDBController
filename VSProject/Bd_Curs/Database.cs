﻿using System;
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

        public DataTable TableData = new DataTable();

        public delegate void MessageShow(string message);
        public event MessageShow Show;

        public delegate void Updater();
        public event Updater UpdateData;

        public Queue<Updater> queue = new Queue<Updater>();

        public bool Connected { get; set; }//Подключена ли БД
        public Database(string server,string database) =>
            connection = new SqlConnection($"Server={server};Database={database};Trusted_Connection=True;");//Создание объекта SQl connection

        async public Task startConnectionAsync(string name, string password)//Подключится к базе данных 
        {
           
            if (Connected) return ;//Не подключаться повторно если БД уже подключена
            try
            {
                await connection.OpenAsync();//Асинхронное открытие подключения
                                             //(на случай если сервер отсутсвует или доступ затруднён и не возникало задержек в работе программы)
                Connected = true;//Подключено
            }
            catch (SqlException) { return; }

            command = new SqlCommand("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_TYPE != 'VIEW'\r\n", connection);
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
        async public Task AuthAsync(string name, string password)//Авторизация в базе данных
        {
            command = new SqlCommand($"SELECT * FROM Users WHERE name = '{name}' AND pass = '{password}'",connection);

            SqlDataReader temp = await command.ExecuteReaderAsync();//Асинхронный запрос к таблице Users
                                                                    //(на случай если пользователей очень много или возникнет ошибка подключения)
            if (temp.HasRows)//Если пользователя с таким именем и паролем нет то закрыть подключение
            {
                temp.Close();//Закрыть устройство чтения
                return;
            }
            CloseConnection();//Закрытие подключения
        }
        public void CloseConnection()//Закрытие подключения
        {
            try
            {
                connection.Close();//Закрытие подключения
                Connected = false;//Не подключено
            }
            catch (SqlException){}
        }
        public void GetSelectedTable(string table)
        {
            command = new SqlCommand($"SELECT * FROM {table}", connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);

            TableData = new DataTable();
            adapter.Fill(TableData);
            queue.Enqueue(UpdateData);
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
                            return;

                        if (tempstr == "SELECT")//Если запрос на выборку
                            IsSelect = true;

                        tempstr = String.Empty;//Очистка предыдущего слова
                    }
                    else
                        tempstr += item;
                }
                if (IsSelect)//Если запрос на выборку
                {
                    command = new SqlCommand(Query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    TableData = new DataTable();
                    adapter.Fill(TableData);
                    queue.Enqueue(UpdateData);
                }
                else
                {
                    command.ExecuteNonQuery();//Выполнение запроса не на выборку
                    GetSelectedTable(TableName);//Отображение таблицы к которой применялся запрос
                }
            }
            catch (SqlException)
            {
                Show.Invoke("Возникло исключение SQL проверьте правильность введённого запроса");
            }
        }
    }     
}

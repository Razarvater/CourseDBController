﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;

namespace Bd_Curs
{
    public class ServerConnection
    {
        private SqlConnection connection;//Подключение к серверу
        public ServerConnection(string server) =>
            connection = new SqlConnection($"Server={server};Database=master;Trusted_Connection=True;");//Создание строки подключения

        public List<string> GetDatabases()//Получение имён Баз Данных
        {
            List<string> Databases = new List<string>();
            try
            {
                connection.Open();//открытие подключения
                //Создание адаптера и заполнение таблицы
                SqlDataAdapter adapter =new SqlDataAdapter( new SqlCommand("SELECT name FROM sys.databases WHERE name!='master' AND name!='tempdb' AND name!='model' AND name!='msdb'", connection));
                DataTable temp = new DataTable();
                adapter.Fill(temp);
                
                foreach (DataRow item in temp.Rows)//Заполнение итоговой коллекции
                {
                    Databases.Add(item[0].ToString());
                }
            }
            catch (SqlException ex){ MessageBox.Show($"Don't connected|{ex.Number}|{ex.Message}");}

            return Databases;//Вернуть List с именами Баз Данных
            
        }
    }
}

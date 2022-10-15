using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;
using System.Threading.Tasks;

namespace Bd_Curs
{
    public class ServerConnection
    {
        public List<string> Databases;
        private SqlConnection connection;//Подключение к серверу
        public delegate void MessageShow(string message);//Делегат для выдачи сообщений в форму
        public event MessageShow Show;
        public ServerConnection(string server) =>
            connection = new SqlConnection($"Server={server};Database=master;Trusted_Connection=True;");//Создание строки подключения

        public async Task GetDatabases()//Получение имён Баз Данных
        {
            Databases = new List<string>();
            try
            {
                await connection.OpenAsync();//открытие подключения
                //Создание адаптера и заполнение таблицы
                SqlDataAdapter adapter =new SqlDataAdapter( new SqlCommand("SELECT name FROM sys.databases WHERE name!='master' AND name!='tempdb' AND name!='model' AND name!='msdb'", connection));
                DataTable temp = new DataTable();
                adapter.Fill(temp);
                
                foreach (DataRow item in temp.Rows)//Заполнение итоговой коллекции
                {
                    Databases.Add(item[0].ToString());
                }
            }
            catch (SqlException ex){ Show.Invoke($"Don't connected|{ex.Number}|{ex.Message}");} 
        }
        public async Task CreateDataBase(string name)
        {
            try
            {
                await connection.OpenAsync();//открытие подключения
                foreach (var item in name)
                {
                    if (item == ' ')
                    {
                        Show.Invoke("Database name must not contain spaces!");
                        return;
                    }
                }
                SqlCommand command = new SqlCommand($"CREATE DATABASE {name}", connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) { Show.Invoke($"Don't connected|{ex.Number}|{ex.Message}");}
        }
    }
}

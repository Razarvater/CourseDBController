using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bd_Curs
{
    public class TableSql//Таблица
    {
        public string name;
        public List<Column> Columns;
        public List<string> ColumnsNames;
        public List<string> PrimaryKeys;
        public List<string> ForeignKeys;
        public bool isAutoIncremented = false;
    }
    public class Column//Столбец
    {
        public string TableName;
        public string Name;
        public object DefaultValue;
        public bool IsNullable;
        public bool IsPrimaryKey;
        public bool IsAutoIncrement;
        public SqlDbType type;
    }
    public class SqlRef//Связь в БД
    {
        public string TableNameFK;
        public string ColumnFK;
        public string TableNamePK;
        public string ColumnPK;
    }
    public class Database
    {
        public SqlConnection connection;//подключение к БД
        private SqlCommand command;//SQL команда
        public List<TableSql> Tables;//Таблицы
        public List<SqlRef> Constrains;//Связи
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
            catch (SqlException ex) 
            {
                Show.Invoke($"{LocalizatorResource.Localize.GetString("NotConnectToDB")}: {ex.Number}\n\n{ex.Message}]");
                return; 
            }

            //Получение всех имён таблиц
            command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME!='sysdiagrams' AND TABLE_TYPE = 'BASE TABLE'\r\n", connection);
            SqlDataReader temp = await command.ExecuteReaderAsync();
            
            Tables = new List<TableSql>();//Имена таблиц
            TableNames = new List<string>();
            while(await temp.ReadAsync())//Получение имён таблиц для базы данных
            {
                TableNames.Add(temp[0].ToString());
                Tables.Add(new TableSql());
                Tables[Tables.Count - 1].name = temp[0].ToString();
            }
            temp.Close();

            //Получение всех первичных ключей
            for (int i = 0; i < TableNames.Count; i++)
            {
                try
                {
                    Tables[i].PrimaryKeys = new List<string>();
                    command = new SqlCommand($"SELECT INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE\r\nJOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS\r\nON INFORMATION_SCHEMA.KEY_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME\r\nWHERE INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_TYPE = 'PRIMARY KEY' AND INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME = '{TableNames[i]}'", connection);
                    SqlDataReader tempo = await command.ExecuteReaderAsync();
                    while (await tempo.ReadAsync())
                    {
                        Tables[i].PrimaryKeys.Add(tempo[0].ToString());
                    }
                    tempo.Close();
                }
                catch (InvalidOperationException) { }
            }

            //Получение всех Внешних ключей
            for (int i = 0; i < TableNames.Count; i++)
            {
                try
                {
                    Tables[i].ForeignKeys = new List<string>();
                    command = new SqlCommand($"SELECT INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE\r\nJOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS\r\nON INFORMATION_SCHEMA.KEY_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME\r\nWHERE INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_TYPE = 'FOREIGN KEY' AND INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME = '{TableNames[i]}'", connection);
                    SqlDataReader tempo = await command.ExecuteReaderAsync();
                    while (await tempo.ReadAsync())
                    {
                        Tables[i].ForeignKeys.Add(tempo[0].ToString());
                    }
                    tempo.Close();
                }
                catch (InvalidOperationException) { }
            }

            //Получение всех колонок и некоторых их харрактеристик
            for (int i = 0; i < TableNames.Count; i++)
            {
                Tables[i].Columns = new List<Column>();
                Tables[i].ColumnsNames = new List<string>();
                command = new SqlCommand($"SELECT COLUMN_NAME,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TableNames[i]}'", connection);
                temp = await command.ExecuteReaderAsync();
                while (await temp.ReadAsync())
                {
                    Tables[i].Columns.Add(new Column());
                    Tables[i].Columns[Tables[i].Columns.Count - 1].TableName = TableNames[i];
                    Tables[i].Columns[Tables[i].Columns.Count - 1].Name = temp[0].ToString();
                    Tables[i].ColumnsNames.Add(temp[0].ToString());
                    Tables[i].Columns[Tables[i].Columns.Count - 1].DefaultValue = temp[1];
                    Tables[i].Columns[Tables[i].Columns.Count - 1].IsNullable = temp[2].ToString() == "YES";
                    Tables[i].Columns[Tables[i].Columns.Count - 1].IsPrimaryKey = Array.IndexOf(Tables[i].PrimaryKeys.ToArray(), $"{Tables[i].Columns[Tables[i].Columns.Count - 1].Name}") != -1;
                    foreach (var item in Enum.GetValues(typeof(SqlDbType)).Cast<SqlDbType>())//Установка типа данных
                    {
                        if (item.ToString().ToLower() == temp[3].ToString().ToLower())
                        {
                            Tables[i].Columns[Tables[i].Columns.Count - 1].type = item;
                            break;
                        }
                    }
                }
                temp.Close();
            }

            //Установка втоинкрементных столбцов
            for (int i = 0; i < TableNames.Count; i++)
            {
                command = new SqlCommand($"SELECT IDENT_CURRENT('{TableNames[i]}');", connection);
                temp = await command.ExecuteReaderAsync();

                await temp.ReadAsync();
                for (int j = 0; j < Tables[i].Columns.Count && !Tables[i].isAutoIncremented; j++)
                {
                    bool tempbool = Tables[i].Columns[j].type != SqlDbType.TinyInt && Tables[i].Columns[j].type != SqlDbType.SmallInt && Tables[i].Columns[j].type != SqlDbType.Int;
                    tempbool = tempbool && Tables[i].Columns[j].type != SqlDbType.BigInt && Tables[i].Columns[j].type != SqlDbType.Decimal;

                    if (Tables[i].Columns[j].IsPrimaryKey && !tempbool )
                    {
                        Tables[i].Columns[j].IsAutoIncrement = temp[0].ToString() != string.Empty;
                        if (Tables[i].Columns[j].IsAutoIncrement)
                            Tables[i].isAutoIncremented = true;
                    }
                }
                temp.Close();
            }
            //Получение всех связей в таблице
            Constrains = new List<SqlRef>();
            command.CommandText = "SELECT T1.FK, T1.TFK, T2.PK, T2.TPK FROM(\r\n    SELECT INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS.CONSTRAINT_NAME,\r\n    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS.UNIQUE_CONSTRAINT_NAME,\r\n    INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME AS FK, INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME AS TFK\r\n    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS\r\n    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE\r\n    ON INFORMATION_SCHEMA.KEY_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS.CONSTRAINT_NAME\r\n    )T1,\r\n    (\r\n    SELECT INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS.CONSTRAINT_NAME,\r\n    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS.UNIQUE_CONSTRAINT_NAME,\r\n    INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME AS PK, INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME AS TPK\r\n    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS\r\n    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE\r\n    ON INFORMATION_SCHEMA.KEY_COLUMN_USAGE.CONSTRAINT_NAME = INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS.UNIQUE_CONSTRAINT_NAME\r\n    )T2 WHERE T1.CONSTRAINT_NAME = T2.CONSTRAINT_NAME";
            temp = await command.ExecuteReaderAsync();
            while(await temp.ReadAsync())
            {
                Constrains.Add(new SqlRef());
                Constrains[Constrains.Count - 1].ColumnFK = temp[0].ToString();
                Constrains[Constrains.Count - 1].TableNameFK = temp[1].ToString();
                Constrains[Constrains.Count - 1].ColumnPK = temp[2].ToString();
                Constrains[Constrains.Count - 1].TableNamePK = temp[3].ToString();
            }
            temp.Close();

            if (Array.IndexOf(TableNames.ToArray(),"Users")!=-1)//Только если есть таблица с пользователями
                await AuthAsync(name,password);//Авторизация 
        }
        public async Task AuthAsync(string name, string password)//Авторизация в базе данных
        {
            try
            { 
                //Запрос для определения права этого пользователя заходить в БД
                command = new SqlCommand($"SELECT * FROM Users WHERE name = @name AND pass = @password", connection);
                command.Parameters.Add(new SqlParameter("@name", name));
                command.Parameters.Add(new SqlParameter("@password", password));
                SqlDataReader temp = await command.ExecuteReaderAsync();//запрос к таблице Users
                if (temp.HasRows)//Если пользователя с таким именем и паролем нет то закрыть подключение
                {
                    temp.Close();//Закрыть устройство чтения
                    return;
                }
                CloseConnection();//Закрытие подключения
            }
            catch (SqlException)
            {
                Show.Invoke($"{LocalizatorResource.Localize.GetString("AuthorizationError")}");
                CloseConnection();//Закрытие подключения
            }
        }
        public void CloseConnection()//Закрытие подключения
        {
            try
            {
                connection.Close();//Закрытие подключения
                Connected = false;//Не подключено
            }
            catch (SqlException ex)
            {
                Show.Invoke($"{LocalizatorResource.Localize.GetString("NotDisconnectFromDB")}: {ex.Number}\n\n{ex.Message}");
            }
        }
        public void GetSelectedTable(string table)
        {
            try
            {
                command = new SqlCommand($"SELECT * FROM [{table}]", connection);//Создание команды
                SqlDataAdapter adapter = new SqlDataAdapter(command);//Создание адаптера
                TableData = new DataTable();//Очистка предыдущей выбранной таблицы
                adapter.Fill(TableData);//Новое заполение
                IsQueryCompleted = true;//Запрос завершён
            }
            catch (SqlException ex)
            {
                IsQueryCompleted = true;//Запрос завершён
                Show.Invoke($"{LocalizatorResource.Localize.GetString("TableSqlException")}:{ex.Number} \n\n{ex.Message}");
            }
        }
        public void SetQuery(string Query, SqlCommand sqlCommand)//Выполнение запроса
        {
            try
            { 
                command = sqlCommand;//Создание новой SQL команды

                string TableName = string.Empty;//Имя таблицы для которой применяется запрос

                bool IsTableName = false;//Является ли следующее слово в запросе именем таблицы
                bool IsSelect = false;
                bool IsDelete = false;
                bool IsAlter = false;
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

                        if (tempstr == "DROP")//Запрет на DROP баз данных и таблиц
                        {
                            IsQueryCompleted = true;//Запрос завершён
                            Show.Invoke("Don't DROP");
                            return;
                        }

                        if (tempstr == "ALTER")
                        {
                            IsAlter = true;
                            break;
                        }
                        if (tempstr == "SELECT")//Если запрос на выборку
                            IsSelect = true;
                        if (tempstr == "DELETE")//Если запрос на удаление то выбрать повторно
                            IsDelete = true;

                        tempstr = string.Empty;//Очистка предыдущего слова
                    }
                    else
                        tempstr += item;
                }
                if (IsSelect)//Если запрос на выборку
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);//Создание адаптера
                    TableData = new DataTable();//Очистка предыдущей выбранной таблицы
                    adapter.Fill(TableData);//Заполнение новыми данными
                    IsQueryCompleted = true;//Запрос завершён
                }
                else
                {
                    command.CommandTimeout = 180;
                    command.ExecuteNonQuery();//Выполнение запроса не на выборку
                    if(IsDelete && !IsAlter)
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand($"SELECT * FROM {TableName}",connection));//Создание адаптера
                        TableData = new DataTable();//Очистка предыдущей выбранной таблицы
                        adapter.Fill(TableData);//Заполнение новыми данными
                    }
                    IsQueryCompleted = true;//Запрос завершён
                }
            }
            catch (SqlException ex)
            {
                IsQueryCompleted = true;//Запрос завершён
                
                Show.Invoke($"{LocalizatorResource.Localize.GetString("QueryException")}: {ex.Number}\n\n{ex.Message}");
            }
        }
        public void DropTable(string TableName)//Удаление таблицы из БД
        {
            SqlCommand command = new SqlCommand($"DROP TABLE {TableName}", connection);
            command.ExecuteNonQuery();
        }
        public int GetAutoIndex(string NameTable)//Получение следующего автоинкремента у таблицы
        {
            SqlCommand command = new SqlCommand($"SELECT IDENT_CURRENT('{NameTable}');", connection);
            SqlDataReader temp = command.ExecuteReader();

            //Получение значения
            temp.ReadAsync();
                int Result = int.Parse(temp[0].ToString());
            temp.Close();

            return Result;
        }
    }     
}

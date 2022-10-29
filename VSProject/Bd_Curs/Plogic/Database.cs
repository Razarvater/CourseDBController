using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Bd_Curs.Plogic
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
    public class SqlParameterStr//Универсальный параметр
    {
        public string name;
        public object value;
        public SqlParameterStr(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public abstract class Database
    {
        public List<TableSql> Tables;//Таблицы
        public List<SqlRef> Constrains;//Связи
        public List<string> TableNames;//Имена таблиц
        public DataTable TableData = new DataTable();//Выбранная таблица

        public delegate void MessageShow(string message);//Делегат для выдачи сообщений в форму

        public bool IsQueryCompleted = false;//Статус завершённости запроса
        public bool Connected { get; set; }//Подключена ли БД

        public abstract Task startConnectionAsync(string name, string password);
        public abstract Task AuthAsync(string name, string password);
        public abstract void CloseConnection();
        public abstract void GetSelectedTable(string table);
        public abstract void SetQuery(string Query, List<SqlParameterStr> parameters);
        public abstract void DropTable(string TableName);
        public abstract int GetAutoIndex(string NameTable);
        public abstract void SetShow(MessageShow message);
    }
}

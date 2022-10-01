using System.Data;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using System;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private void button1_Click(object sender, EventArgs e)//Короче вместо всей этой херни автогенерируемую форму сделать и не париться...
        {
            ((DataTable) SelectedTable.DataSource).Rows.Add();
            string Query = $"INSERT INTO [{SelectedTableName}] (";
            for (int i = 0; i < db.Tables[SelectedTableNameINT].Columns.Count; i++)
            {
                if(!db.Tables[SelectedTableNameINT].Columns[i].IsAutoIncrement)
                    Query += $"[{db.Tables[SelectedTableNameINT].Columns[i].Name}], ";
            }
            Query = Query.Remove(Query.Length - 2);
            Query += ") VALUES (";
            SqlCommand sqlCommand = new SqlCommand(Query, db.connection);
            for (int i = 0; i < db.Tables[SelectedTableNameINT].Columns.Count; i++)
            {
                if (!db.Tables[SelectedTableNameINT].Columns[i].IsAutoIncrement)
                {
                

                    Query += $" @F{i} , ";
                    if(db.Tables[SelectedTableNameINT].Columns[i].IsNullable)
                        sqlCommand.Parameters.Add(new SqlParameter($"@F{i}", db.Tables[SelectedTableNameINT].Columns[i].DefaultValue));
                    else if(!db.Tables[SelectedTableNameINT].Columns[i].IsPrimaryKey)
                        sqlCommand.Parameters.Add(new SqlParameter($"@F{i}", "Default Value"));
                    else
                    {
                        DateTime centuryBegin = new DateTime(2022, 10, 1);
                        DateTime currentDate = DateTime.Now;
                        long elapsedTicks = currentDate.Ticks - centuryBegin.Ticks;
                        sqlCommand.Parameters.Add(new SqlParameter($"@F{i}", (elapsedTicks % 32768).ToString()));
                    }    
                        

                    sqlCommand.Parameters[sqlCommand.Parameters.Count - 1].SqlDbType = db.Tables[SelectedTableNameINT].Columns[i].type;
                }
            }
            Query = Query.Remove(Query.Length - 2);
            Query += ")";
            sqlCommand.CommandText = Query;
            ColumnsSELECT.Text = Query;

            IsUpdate = true;
            Thread UpdateThread = new Thread(() => db.SetQueryAsync(Query, sqlCommand));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}


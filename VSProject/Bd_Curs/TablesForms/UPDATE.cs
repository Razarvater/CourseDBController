using Bd_Curs.Plogic;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private int SelectedParIndex = 0;
        private int SelectedColumnIndex = 0;
        private bool IsUpdate = false;
        private DataGridViewRow SelectedParLast;//Выбранная строка
        private DataGridViewRow SelectedParSecond;//Изменённая строка
        private void SelectedTable_CellClick(object sender, DataGridViewCellEventArgs e)//Выделение записи для редактирования
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1)//Выбранная запись и её поле
            {
                SelectedParIndex = e.RowIndex;
                SelectedColumnIndex = e.ColumnIndex;

                //Копирование выбранной строки
                SelectedParLast = (DataGridViewRow)SelectedTable.Rows[SelectedParIndex].Clone();   
                for (int i = 0; i < SelectedTable.Rows[SelectedParIndex].Cells.Count; i++)
                {
                    SelectedParLast.Cells[i].Value = SelectedTable.Rows[SelectedParIndex].Cells[i].Value;
                }
            }
        }
        private void SelectedTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)//Заканчивание редактирования записи
        {
            SelectedParSecond = SelectedTable.Rows[SelectedParIndex];//Изменённая строка
            string Query = $"UPDATE [{SelectedTableName}] SET";//Строка запроса
            float temp;

            List<SqlParameterStr> parameters = new List<SqlParameterStr>();
            //Создание параметризированного запроса
            string Parameter = SelectedTable.Columns[SelectedColumnIndex].HeaderText;
            if (Parameter.IndexOf("🔑") != -1 || Parameter.IndexOf("🔗") != -1)
                Parameter = Parameter.Remove(Parameter.Length - 2);
            //Проверка на float(Изменение ',' на '.')
            if (float.TryParse(SelectedParSecond.Cells[SelectedColumnIndex].Value.ToString(), out temp))
            {
                Query += $" {Parameter} = @{Parameter}1 WHERE ";//Создание условий для изменения записи 
                parameters.Add(new SqlParameterStr($"@{Parameter}1", SelectedParSecond.Cells[SelectedColumnIndex].Value.ToString().Replace(',', '.')));
            }
            else
            {
                Query += $" {Parameter} = @{Parameter}1 WHERE ";//Создание условий для изменения записи
                parameters.Add(new SqlParameterStr($"@{Parameter}1", SelectedParSecond.Cells[SelectedColumnIndex].Value));
            }
            
            //Вставка в запрос всех уникальных полей параметризированно
            for (int j = 0;j < db.Tables[IndexSelectedTable].PrimaryKeys.Count;j++)
            {
                if(j>0)Query += $" AND {db.Tables[IndexSelectedTable].PrimaryKeys[j]} = ";
                else Query += $"{db.Tables[IndexSelectedTable].PrimaryKeys[j]} = ";
                Parameter = db.Tables[IndexSelectedTable].PrimaryKeys[j];
                for (int i = 0; i < SelectedTable.Rows[0].Cells.Count; i++)
                {
                    if (SelectedTable.Columns[i].HeaderText == db.Tables[IndexSelectedTable].PrimaryKeys[j]+"🔑")//Вставка поля
                    {
                        //Проверка на float(Изменение ',' на '.')
                        if (float.TryParse(SelectedParSecond.Cells[SelectedColumnIndex].Value.ToString(), out temp))
                        {
                            Query += $"@{Parameter}2";
                            parameters.Add(new SqlParameterStr($"@{Parameter}2", SelectedParLast.Cells[i].Value.ToString().Replace(',', '.')));
                        }
                        else
                        {
                            Query += $"@{Parameter}2";
                            parameters.Add(new SqlParameterStr($"@{Parameter}2", SelectedParLast.Cells[i].Value));
                        }
                        break;
                    }
                }
            }
            IsUpdate = true;

            Thread UpdateThread = new Thread(() => db.SetQuery(Query,parameters));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
            SelectedParLast = null;
            SelectedParSecond = null;
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
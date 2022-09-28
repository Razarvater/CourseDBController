using System;
using System.Threading;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private int SelectedParIndex = 0;
        private int SelectedColumnIndex = 0;
        private DataGridViewRow SelectedParLast;//Выбранная строка
        private DataGridViewRow SelectedParSecond;//Изменённая строка

        private void SelectedTable_CellClick(object sender, DataGridViewCellEventArgs e)//Выделение записи для редактирования
        {
            if (e.ColumnIndex > -1 && e.RowIndex > -1)//Выбранная запись и её поле
            {
                SelectedParIndex = e.RowIndex;
                SelectedColumnIndex = e.ColumnIndex;
                SelectedParLast = (DataGridViewRow)SelectedTable.Rows[SelectedParIndex].Clone();//Копирование выбранной строки
                
                for (int i = 0; i < SelectedTable.Rows[SelectedParIndex].Cells.Count; i++)
                {
                    SelectedParLast.Cells[i].Value = SelectedTable.Rows[SelectedParIndex].Cells[i].Value;
                }

            }
        }
        private void SelectedTable_CellEndEdit(object sender, DataGridViewCellEventArgs e)//Заканчивание редактирования записи
        {
            SelectedParSecond = SelectedTable.Rows[SelectedParIndex];//Изменённая строка
            string Query = $"UPDATE {SelectedTableName} SET";//Строка запроса
            float temp = 0f;
            if(float.TryParse(SelectedParSecond.Cells[SelectedColumnIndex].Value.ToString(),out temp))
                Query += $" {SelectedTable.Columns[SelectedColumnIndex].HeaderText} = '{SelectedParSecond.Cells[SelectedColumnIndex].Value.ToString().Replace(',','.')}' WHERE ";//Создание условий для изменения записи
            else
                Query += $" {SelectedTable.Columns[SelectedColumnIndex].HeaderText} = '{SelectedParSecond.Cells[SelectedColumnIndex].Value}' WHERE ";//Создание условий для изменения записи

            for (int j = 0;j < db.PrimaryKeys[db.TableNames.IndexOf(SelectedTableName)].Count;j++)
            {
                if(j>0)Query += $" AND {db.PrimaryKeys[db.TableNames.IndexOf(SelectedTableName)][j]} = ";
                else Query += $"{db.PrimaryKeys[db.TableNames.IndexOf(SelectedTableName)][j]} = ";
                for (int i = 0; i < SelectedTable.Rows[0].Cells.Count; i++)
                {
                    if (SelectedTable.Columns[i].HeaderText == db.PrimaryKeys[db.TableNames.IndexOf(SelectedTableName)][j])
                    {
                        if (float.TryParse(SelectedParSecond.Cells[SelectedColumnIndex].Value.ToString(), out temp))
                            Query += $"'{SelectedParSecond.Cells[i].Value.ToString().Replace(',', '.')}'";
                        else Query += $"'{SelectedParSecond.Cells[i].Value}'";
                        break;
                    }
                }
            }

            textBox2.Text = Query;  
            Thread UpdateThread = new Thread(() => db.SetQueryAsync(Query));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
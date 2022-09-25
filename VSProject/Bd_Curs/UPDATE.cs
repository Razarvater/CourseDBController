using System.Threading;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class Form1 : Form
    {
        private int SelectedParIndex = 0;
        private DataGridViewRow SelectedParLast;//Выбранная строка
        private DataGridViewRow SelectedParSecond;//Изменённая строка

        private void SelectedTable_CellClick(object sender, DataGridViewCellEventArgs e)//Выделение записи для редактирования
        {
            if (e.ColumnIndex > -1 && e.RowIndex > 0)//Выбранная запись и её поле
            {
                SelectedParIndex = e.RowIndex;
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

            for (int i = 0; i < SelectedParSecond.Cells.Count - 1; i++)//Изменённая строка
            {
                Query += $" {SelectedTable.Columns[i].HeaderText} = '{SelectedParSecond.Cells[i].Value}', ";
            }
            Query += $" {SelectedTable.Columns[SelectedTable.Columns.Count - 1].HeaderText} = '{SelectedParSecond.Cells[SelectedParSecond.Cells.Count - 1].Value}' WHERE";//Создание условий для изменения записи

            for (int i = 0; i < SelectedParLast.Cells.Count - 1; i++)//Условие выборки
            {
                Query += $" {SelectedTable.Columns[i].HeaderText} = '{SelectedParLast.Cells[i].Value}' AND";
            }
            Query += $" {SelectedTable.Columns[SelectedTable.Columns.Count - 1].HeaderText} = '{SelectedParLast.Cells[SelectedParLast.Cells.Count - 1].Value}'";
            
            textBox2.Text = Query;  
            Thread UpdateThread = new Thread(() => db.SetQueryAsync(Query));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
        }
    }
}


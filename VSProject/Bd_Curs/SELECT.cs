using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class Form1 : Form 
    {
        private bool ISDistinct = false;
        private List<TextBox> WhereSelectBoxes = new List<TextBox>();
        private string SelectQuery;
        private byte LimitWhere = 6;
        private void DISTINCT_CheckedChanged(object sender, EventArgs e) => ISDistinct = !ISDistinct;//Галочка на убирание повторений в запросе
        private void button3_Click(object sender, EventArgs e)//Выполнить запрос из формы
        {
            if (IsQueryWorked()) return;//Если запрос уже активен и выполняется, ничего не делать
            Query_IsWorking = true;//Запрос выполняется
            SelectQuery = "SELECT ";//Выборка
            if (ISDistinct) SelectQuery += "DISTINCT ";//Если требуется уборка повторений

            if (ColumnsSELECT.Text == string.Empty) SelectQuery += "*";//Все столбцы или только выбранные
            else SelectQuery += ColumnsSELECT.Text;

            SelectQuery += $" FROM {TableNameSELECT.Text} ";//Из таблицы

            if (WhereSelectBoxes[0].Text != string.Empty)//Добавление условий при их наличии
            {
                SelectQuery += $"WHERE {WhereSelectBoxes[0].Text} ";//Добавление первого условия
                foreach (var item in WhereSelectBoxes)//Добавление всех остальных условий
                {
                    if (item.Text != string.Empty)//Если оно не пустое
                        SelectQuery += $"AND {item.Text} ";//Добавить условие
                }
            }
            Thread UpdateThread = new Thread(() => db.SetQueryAsync(SelectQuery));//Создание потока с запросом
            UpdateThread.Start();//Старт потока
            QueueTimer.Start();//Старт таймера на проверку завершения потока
            RunCounter();//Старт Счётчиков
            RemoveNewConditions();//Удалить пользовательские условия
        }
        private void RemoveNewConditions()//Удалить добавленные пользовательские условия
        {
            WhereSelectBoxes.Clear();//удаление боксов с условиями из списка
            WhereSelectBoxes.Add(ConditionSelect);//Добавление первого бокса в список

            string tempName;
            foreach (var item in tabPage3.Controls)//Удаление всех текстбоксов с цифрой в конце(генерируемых)
            {
                if ((item as TextBox) is TextBox)
                {
                    tempName = item.GetType().GetProperty("Name").GetValue(item).ToString();//Получение имени
                    if (char.IsDigit(tempName[tempName.Length - 1]))//Проверка последнего символа
                        tabPage3.Controls.Remove((Control)item);//Удаление
                }
            }
        }
        private void AddNewConditionButton_Click(object sender, EventArgs e)//Добавить новое условие
        {
            if (WhereSelectBoxes.Count >= LimitWhere) return;//Не добавлять новое условие если достигнут лимит

            TextBox temp = new TextBox();//Создание нового бокса с условием

            //Его местоположение и новое имя
            temp.Location = new Point(WhereSelectBoxes[WhereSelectBoxes.Count - 1].Location.X + WhereSelectBoxes[WhereSelectBoxes.Count - 1].Size.Width + 10, 56);
            temp.Name = $"ConditionSelect{WhereSelectBoxes.Count}";
            WhereSelectBoxes.Add(temp);//Добавление в коллекцию 

            tabPage3.Controls.Add(WhereSelectBoxes[WhereSelectBoxes.Count - 1]);//Добавление на страницу
        }
    }
}

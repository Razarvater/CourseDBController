using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private void InitTableRelation()//Очистка и заполнение формы TableRelation
        {
            //Очистка полей
            comboTable1.Items.Clear();
            comboColumn1.Items.Clear();
            comboTable2.Items.Clear();
            comboColumn2.Items.Clear();
            comboColumn2.Enabled = false;
            comboColumn1.Enabled = false;
            foreach (string item in db.TableNames)//обавление имён таблиц кроме Users
            {
                if (item == "Users") continue;
                comboTable1.Items.Add(item);
                comboTable2.Items.Add(item);
            }
        }
        private void comboTable1_SelectedIndexChanged(object sender, EventArgs e)//Если поставлено имя 1-ой таблицы
        {
            //Включить и очистить поле первичного ключа
            comboColumn1.Enabled = true;
            comboColumn1.Items.Clear();
            //Добавление в него всех первичных ключей данной таблицы
            int IndexSelectedItem = Array.IndexOf(db.TableNames.ToArray(), comboTable1.SelectedItem);
            for (int i = 0; i < db.Tables[IndexSelectedItem].ColumnsNames.Count; i++)
                if (db.Tables[IndexSelectedItem].Columns[i].IsPrimaryKey)
                    comboColumn1.Items.Add(db.Tables[IndexSelectedItem].ColumnsNames[i]);
        }
        private void comboColumn1_SelectedIndexChanged(object sender, EventArgs e)//Если выбран первичный ключ
        {
            if (comboColumn1.SelectedIndex == -1 || comboTable2.SelectedIndex == -1) return;//Если это была инициализация поля выйти
            comboColumn2.Enabled = true;//Включить выбор внешнего ключа во второй таблице
            comboColumn2.Items.Clear();//Очистить поле

            //Заполнить его возможными столбцами для внешнего ключа с тем же типом данных
            int IndexForeignKey = Array.IndexOf(db.TableNames.ToArray(), comboTable2.SelectedItem);
            for (int i = 0; i < db.Tables[IndexForeignKey].ColumnsNames.Count; i++)
            {
                if (db.Tables[IndexForeignKey].Columns[i].type != db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable1.SelectedItem)].Columns[comboColumn1.SelectedIndex].type)
                    continue;
                comboColumn2.Items.Add(db.Tables[IndexForeignKey].ColumnsNames[i]);
            }
        }
        private void comboTable2_SelectedIndexChanged(object sender, EventArgs e) =>//Обновить список внешних ключей
            comboColumn1_SelectedIndexChanged(new object(), EventArgs.Empty);
        private void CreateRelation(object sender, EventArgs e)//Создание связи
        {
            //Если одно из полей не заполнено
            if (comboColumn1.Text == string.Empty || comboColumn2.Text == string.Empty || comboTable1.Text == string.Empty || comboTable2.Text == string.Empty)
            {
                //Выдать сообщени об ошибке
                ErrorMessage("Fill in all the fields");
                IsError = false;
                return;
            }
            //Если выбраны одинаковые таблицы
            if (comboTable1.Text == comboTable2.Text)
            {
                //Выдать сообщени об ошибке
                ErrorMessage("Tables must be different");
                IsError=false;
                return;
            }
            //Сформировать запрос
            string Query = $"ALTER TABLE {comboTable2.Text} ADD CONSTRAINT FK_{comboColumn1.Text}_{comboColumn2.Text}" +
                $" FOREIGN KEY ({comboColumn2.Text}) REFERENCES {comboTable1.Text} ({comboColumn1.Text})";

            //Если выбрано Каскадное обновление
            if (checkCascade.Checked)
                Query += " ON DELETE CASCADE ON UPDATE CASCADE";

            db.SetQuery(Query, new SqlCommand(Query, db.connection));//Выполнить запрос
            ConnectButton_Click(new object(), EventArgs.Empty);//Переподключиться к БД
        }
    }
}

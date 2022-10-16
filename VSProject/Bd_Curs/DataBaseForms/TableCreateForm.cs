using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private List<FieldSQl> FieldTableForm;//Все поля создаваемой таблицы
        private int AutoIncrementCount;//Количество автоинкрементных полей
        private void AutoIncrementClick(object sender, EventArgs e)
        {
            //Если уже есть автоинкрементное поле это поле стало false то убрать автоинкрементное поле
            if (AutoIncrementCount == 1 && sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "false")
                AutoIncrementCount = 0;
            else if (AutoIncrementCount == 0)//Если это поле первое автоикрементное то добавить
                AutoIncrementCount = 1;
            else
            {
                //Если автоинкрементное поле уже есть то не добавлять новое
                sender.GetType().GetProperty("Checked").SetValue(sender, false);
                AutoIncrementCount = 1;
            }
        }
        private void PrimaryClick(object sender,EventArgs e)
        {
            int PrimaryIndex = int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString());
            foreach (Control item in tabPage10.Controls)//Проход по всем элементам страницы
            {
                if(item == FieldTableForm[PrimaryIndex].FieldRedactions[5])//Если это поле AutoIncrement
                {
                    //Если мы добавляем PrimaryKey
                    if(sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "true")
                        item.Enabled = true;//Включить поле
                    else//Если убираем
                    {
                        item.Enabled = false;//Выключить поле
                        item.GetType().GetProperty("Checked").SetValue(item, false);//Поставить ему false
                    }
                    break;
                }
                else if(item == FieldTableForm[PrimaryIndex].FieldRedactions[4])//Если это Nullable
                {
                    //Если мы добавляем PrimaryKey
                    if (sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "true")
                    {
                        item.Enabled = false;//Выключить поле
                        item.GetType().GetProperty("Checked").SetValue(item, false);//Поставить ему false
                    }
                    else
                        item.Enabled = true;//Включить поле
                }
            }
        }
        private void CreateTableClick(object sender, EventArgs e)
        {
            int temp;
            int PrimaryCount = 0;
            foreach (Control item in tabPage10.Controls)//Получение всех данных из полей
            {
                if(int.TryParse(item.Name,out temp))//Только если его название является цифрой(поле)
                {
                    for (int i = 0; i < FieldTableForm[temp].FieldRedactions.Count; i++)//Просматриваем все поля
                    {
                        if(item == FieldTableForm[temp].FieldRedactions[i])//Если поле на форме есть поле коллекции
                        {
                            try
                            {
                                switch (i)//Добавить его значение в коллекцию
                                {
                                    case 0:
                                        FieldTableForm[temp].FieldName = item.Text;
                                        break;
                                    case 1:
                                        FieldTableForm[temp].FieldType = (SqlDbType)item.GetType().GetProperty("SelectedItem").GetValue(item);
                                        break;
                                    case 2:
                                        //Допустимо только для полей ниже
                                        if(FieldTableForm[temp].FieldType is SqlDbType.NChar or SqlDbType.NText or SqlDbType.Text or SqlDbType.VarChar or SqlDbType.NVarChar)
                                            int.TryParse(item.Text, out FieldTableForm[temp].FieldCount);
                                        break;
                                    case 3:
                                        FieldTableForm[temp].IsPrimary = (bool)item.GetType().GetProperty("Checked").GetValue(item);
                                        if (FieldTableForm[temp].IsPrimary)//Определение количества первичных ключей
                                            PrimaryCount++;
                                        break;
                                    case 4:
                                        FieldTableForm[temp].IsNullable = (bool)item.GetType().GetProperty("Checked").GetValue(item);
                                        break;
                                    case 5:
                                        FieldTableForm[temp].IsAutoIncrementField = (bool)item.GetType().GetProperty("Checked").GetValue(item);
                                        break;
                                }
                            }
                            catch (Exception){}//Игнорирование исключений
                        }
                    }
                }
            }

            if (PrimaryCount < 1)//Если количество первичных ключей 0
            {
                //Выдать сообщение об ошибке
                ErrorMessage("U need more PrimaryKeys >=1");
                IsError = false;
                return;
            }
            for (int i = 0; i < FieldTableForm.Count; i++)
            {
                if (FieldTableForm[i].FieldName == string.Empty)//Если поле пустое
                {
                    //Выдать сообщение об ошибке
                    ErrorMessage($"FieldName in {i + 1} field is invalid");
                    IsError = false;
                    return;
                }
                else if (FieldTableForm[i].FieldType.ToString() == string.Empty)//Если поле пустое
                {
                    //Выдать сообщение об ошибке
                    ErrorMessage($"FieldType in {i + 1} field is invalid");
                    IsError = false;
                    return;
                }//Если поле пустое и оно требует размерность(char типы)
                else if (FieldTableForm[i].FieldCount <= 0 && FieldTableForm[i].FieldType is SqlDbType.NChar or SqlDbType.NText or SqlDbType.Text or SqlDbType.VarChar or SqlDbType.NVarChar)
                {
                    //Выдать сообщение об ошибке
                    ErrorMessage($"FieldCount ({FieldTableForm[i].FieldCount}) in {i + 1} field is invalid");
                    IsError = false;
                    return;
                }
            }

            foreach (FieldSQl item in FieldTableForm)
            {
                foreach (char Letter in item.FieldName)
                {
                    if (Letter == ' ' || Letter == '\n' || Letter == '\\' || Letter == '*')
                    {
                        ErrorMessage("field name must not contain extraneous characters");
                        IsError = false;
                        return;
                    }
                }
            }

            SqlCommand command = new SqlCommand("", db.connection);//Создание новой SQl команды
            
            string Query = $"CREATE TABLE {tabPage10.Controls[1].Text} (";
            string After = string.Empty;

            for (int i = 0; i < FieldTableForm.Count; i++)//Составление команды по полям
            {
                Query += $" {FieldTableForm[i].FieldName} {FieldTableForm[i].FieldType} ";//Имя и тип поля
                if (FieldTableForm[i].FieldCount != 0)
                    Query += $"( {FieldTableForm[i].FieldCount} )";//Количество элементов в нём
                if(!FieldTableForm[i].IsNullable)
                    Query += $" NOT NULL ";//Допустим ли NULL
                if(FieldTableForm[i].IsAutoIncrementField)
                    Query += $" IDENTITY(1,1)";//Автоинкрементное ли оно

                if (FieldTableForm[i].IsPrimary)//Если поле первичный ключ то добавить его
                {
                    if (After == string.Empty)
                        After += $" PRIMARY KEY( {FieldTableForm[i].FieldName}";
                    else
                        After += $", {FieldTableForm[i].FieldName} ";
                }
                Query += ",\n";
            }
            After += ")";
            Query += After + " )";
            command.CommandText = Query;
            db.SetQuery(Query, command);//Выполнение запроса
            ConnectButton_Click(new object(), EventArgs.Empty);//Переподключение к БД
        }
        public void DropTable(object sender, KeyEventArgs e)//Удаление таблицы
        {
            if (e.KeyCode != Keys.Back) return;//Только если нажата клавиша BackSpace

            //Точно ли пользователь хочет удалить таблицу
            if(MessageBox.Show("U really wont delete this table?", "Drop Table", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //Удалить таблицу
                db.DropTable(sender.GetType().GetProperty("Text").GetValue(sender).ToString());
                ConnectButton_Click(new object(), EventArgs.Empty);//Переподключится к БД
            }
        }
    }
    public class FieldSQl//поле SQL
    {
        public string FieldName;//имя поля
        public SqlDbType FieldType;//тип поля
        public int FieldCount;//размер поля
        public bool IsPrimary;//первичный ли ключ
        public bool IsNullable;//допустим ли NULL
        public bool IsAutoIncrementField;//автоинкрементно ли оно
        public List<Control> FieldRedactions =  new List<Control>();//Перечень элементов на форме
    }
}

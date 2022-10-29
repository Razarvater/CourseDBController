using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private void CreateTableFormLocalize()//Локализация
        {
            if (tabPage10.Controls.Count != 0)
            {
                tabPage10.Controls[0].Text = Localize.GetString("CreateNewField");
                tabPage10.Controls[1].Text = Localize.GetString("Table_Name_Default");
                tabPage10.Controls[2].Text = Localize.GetString("CreateTable");
                tabPage10.Controls[3].Text = $"{Localize.GetString("CountOfFields")}: {FieldTableForm.Count}";
                tabPage10.Controls[4].Text = Localize.GetString("FieldName");
                tabPage10.Controls[5].Text = Localize.GetString("FieldType");
                tabPage10.Controls[6].Text = Localize.GetString("SymbolsCount");
                tabPage10.Controls[7].Text = Localize.GetString("Primary");
                tabPage10.Controls[8].Text = Localize.GetString("Nullable");
                tabPage10.Controls[9].Text = Localize.GetString("AutoIncrement");

                for (int i = 16; i < tabPage10.Controls.Count; i+=7)
                    tabPage10.Controls[i].Text = Localize.GetString("DeleteField");
            }
        }
        private void TableFormsLocalization()//Локализация
        {
            for (int i = 0; i < TableInfoTabControl.TabPages.Count; i++)
            {
                TableInfoTabControl.TabPages[i].Controls[0].Text = Localize.GetString("FieldName");
                TableInfoTabControl.TabPages[i].Controls[1].Text = Localize.GetString("TypeOfData");
                TableInfoTabControl.TabPages[i].Controls[2].Text = Localize.GetString("Primary");
                TableInfoTabControl.TabPages[i].Controls[3].Text = Localize.GetString("Nullable");
            }
        }
        private void InitTableForms()//Форма просмотра характеристик таблиц
        {
            int Y, step = 150;//Координаты
            int tempBarrier = TableInfoTabControl.TabPages.Count;//Барьер для пропуска печати таблицы Users
            int IndexTabControl = 0;
            for (int i = 0; i < tempBarrier; i++)//Заполнение всех табличных форм
            {
                if (db.Tables[i].name == "Users")//Исключая таблицу Users
                {
                    tempBarrier++;
                    continue;
                }
                Y = 0;
                //Создание названий колонок
                Label ColumnNameZ = new Label();
                ColumnNameZ.Text = Localize.GetString("FieldName");
                ColumnNameZ.Location = new Point(0, 0);
                ColumnNameZ.Width = step;
                ColumnNameZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnNameZ.Font = new Font(ColumnNameZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnNameZ);

                Label ColumnTypeZ = new Label();
                ColumnTypeZ.Text = Localize.GetString("TypeOfData");
                ColumnTypeZ.Location = new Point(step, 0);
                ColumnTypeZ.Width = step;
                ColumnTypeZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnTypeZ.Font = new Font(ColumnTypeZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnTypeZ);

                Label ColumnPKZ = new Label();
                ColumnPKZ.Text = Localize.GetString("Primary");
                ColumnPKZ.Location = new Point(step * 2, 0);
                ColumnPKZ.Width = step;
                ColumnPKZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnPKZ.Font = new Font(ColumnPKZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnPKZ);

                Label ColumnNullZ = new Label();
                ColumnNullZ.Text = Localize.GetString("Nullable");
                ColumnNullZ.Location = new Point(step * 3, 0);
                ColumnNullZ.Width = step;
                ColumnNullZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnNullZ.Font = new Font(ColumnNullZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnNullZ);
                Y += 25;
                //Заполнение колонок содержимым
                for (int j = 0; j < db.Tables[i].Columns.Count; j++)
                {
                    Label ColumnName = new Label();
                    ColumnName.Text = db.Tables[i].ColumnsNames[j];
                    ColumnName.Location = new Point(0, Y);
                    ColumnName.Width = step;
                    ColumnName.TextAlign = ContentAlignment.MiddleCenter;
                    TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnName);

                    Label ColumnType = new Label();
                    ColumnType.Text = db.Tables[i].Columns[j].type.ToString();
                    ColumnType.Location = new Point(step, Y);
                    ColumnType.Width = step;
                    ColumnType.TextAlign = ContentAlignment.MiddleCenter;
                    TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnType);

                    Label ColumnPK = new Label();
                    ColumnPK.Text = db.Tables[i].Columns[j].IsPrimaryKey.ToString();
                    ColumnPK.Location = new Point(step * 2, Y);
                    ColumnPK.Width = step;
                    ColumnPK.TextAlign = ContentAlignment.MiddleCenter;
                    TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnPK);

                    Label ColumnNull = new Label();
                    ColumnNull.Text = db.Tables[i].Columns[j].IsNullable.ToString();
                    ColumnNull.Location = new Point(step * 3, Y);
                    ColumnNull.Width = step;
                    ColumnNull.TextAlign = ContentAlignment.MiddleCenter;
                    TableInfoTabControl.TabPages[IndexTabControl].Controls.Add(ColumnNull);
                    Y += 25;
                }
                IndexTabControl++;//Смещение на следующую таблицу
            }
        } 
        private void InitCreateTableForm()//Форма для создания таблиц
        {
            AutoIncrementCount = 0;//Сброс количества актоинкрементных столбцов
            //Сброс предыдущей формы
            tabPage10.Controls.Clear();
            FieldTableForm = new List<FieldSQl>();

            //Создание всех элементов
            Button CreateNewFieldButton = new Button();
            TextBox TableNameField = new TextBox();
            Button CreateTableButton = new Button();
            Label CountFields = new Label();
            Label FieldName = new Label();
            Label FieldType = new Label();
            Label FieldCount = new Label();
            Label IsPrimary = new Label();
            Label IsNullable = new Label();
            Label IsAutoIncrement = new Label();

            //Кнопка для создания нового поля
            CreateNewFieldButton.Location = new Point(0, 25);
            CreateNewFieldButton.Text = Localize.GetString("CreateNewField");
            CreateNewFieldButton.Size = new Size(100, 50);
            CreateNewFieldButton.Click += CreateNewField;
            CreateNewFieldButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //Имя таблицы
            TableNameField.Location = new Point(0, 75);
            TableNameField.Text = Localize.GetString("Table_Name_Default");
            TableNameField.Size = new Size(100, 25);
            TableNameField.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //Кнопка для создания таблицы
            CreateTableButton.Location = new Point(0, 95);
            CreateTableButton.Text = Localize.GetString("CreateTable");
            CreateTableButton.Size = new Size(100, 50);
            CreateTableButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            CreateTableButton.Click += CreateTableClick;

            //Названия столбцов
                //Информация об общем количестве полей
                CountFields.Location = new Point(0, 0);
                CountFields.Text = $"{Localize.GetString("CountOfFields")}: 0";
                CountFields.Size = new Size(150, 30);
                CountFields.Font = new Font(CountFields.Font.FontFamily, 8.5F, FontStyle.Bold);
                CountFields.TextAlign = ContentAlignment.MiddleCenter;
                CountFields.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //Имя поля таблицы
                FieldName.Location = new Point(150, 0);
                FieldName.Text = Localize.GetString("FieldName");
                FieldName.Size = new Size(100, 30);
                FieldName.Font = new Font(FieldName.Font.FontFamily, 8.5F, FontStyle.Bold);
                FieldName.TextAlign = ContentAlignment.MiddleCenter;
                FieldName.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //Тип поля
                FieldType.Location = new Point(300, 0);
                FieldType.Text = Localize.GetString("FieldType");
                FieldType.Size = new Size(100, 30);
                FieldType.Font = new Font(FieldType.Font.FontFamily, 8.5F, FontStyle.Bold);
                FieldType.TextAlign = ContentAlignment.MiddleCenter;
                FieldType.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //Ограничение размера поля (для char varchar и т.д.)
                FieldCount.Location = new Point(450, 0);
                FieldCount.Text = Localize.GetString("SymbolsCount");
                FieldCount.Size = new Size(100, 30);
                FieldCount.Font = new Font(CountFields.Font.FontFamily, 8.5F, FontStyle.Bold);
                FieldCount.TextAlign = ContentAlignment.MiddleCenter;
                FieldCount.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //Первичный ли ключ это поле
                IsPrimary.Location = new Point(600, 0);
                IsPrimary.Text = Localize.GetString("Primary");
                IsPrimary.Size = new Size(100, 30);
                IsPrimary.Font = new Font(IsPrimary.Font.FontFamily, 8.5F, FontStyle.Bold);
                IsPrimary.TextAlign = ContentAlignment.MiddleCenter;
                IsPrimary.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //Допускает ли NULL значение это поле
                IsNullable.Location = new Point(750, 0);
                IsNullable.Text = Localize.GetString("Nullable");
                IsNullable.Size = new Size(100, 30);
                IsNullable.Font = new Font(IsNullable.Font.FontFamily, 8.5F, FontStyle.Bold);
                IsNullable.TextAlign = ContentAlignment.MiddleCenter;
                IsNullable.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                //Автоинкрементно ли оно
                IsAutoIncrement.Location = new Point(900, 0);
                IsAutoIncrement.Text = Localize.GetString("AutoIncrement");
                IsAutoIncrement.Size = new Size(120, 30);
                IsAutoIncrement.Font = new Font(CountFields.Font.FontFamily, 8.5F, FontStyle.Bold);
                IsAutoIncrement.TextAlign = ContentAlignment.MiddleCenter;
                IsAutoIncrement.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //Добавление всех элементов на форму
            tabPage10.Controls.Add(CreateNewFieldButton);
            tabPage10.Controls.Add(TableNameField);
            tabPage10.Controls.Add(CreateTableButton);
            tabPage10.Controls.Add(CountFields);
            tabPage10.Controls.Add(FieldName);
            tabPage10.Controls.Add(FieldType);
            tabPage10.Controls.Add(FieldCount);
            tabPage10.Controls.Add(IsPrimary);
            tabPage10.Controls.Add(IsNullable);
            tabPage10.Controls.Add(IsAutoIncrement);
        }
        private void CreateNewField(object sender, EventArgs e)//Создание нового поля
        {
            FieldTableForm.Add(new FieldSQl());//Добавление нового поля

            //Создание элементов
            Button DeleteFieldButton = new Button();
            TextBox FieldNameTextBox = new TextBox();
            ComboBox FieldTypeBox = new ComboBox();
            TextBox FieldCountBox = new TextBox();

            CheckBox IsPrimaryField = new CheckBox();
            CheckBox IsNullableBox = new CheckBox();
            CheckBox IsAutoIncrementedField = new CheckBox();

            //Имя поля
            FieldNameTextBox.Location = new Point(150, FieldTableForm.Count * 25 + 15 - tabPage10.VerticalScroll.Value);
            FieldNameTextBox.Size = new Size(100, 20);
            FieldNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //Тип поля
            FieldTypeBox.Location = new Point(300, FieldTableForm.Count * 25 + 15 - tabPage10.VerticalScroll.Value);
            FieldTypeBox.Size = new Size(100, 20);
            FieldTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            FieldTypeBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            foreach (var item in Enum.GetValues(typeof(SqlDbType)))//Установка всех SqlDbType как возможный тип поля
            {
                FieldTypeBox.Items.Add(item);
            }

            //Ограничение размера поля (для char, varchar и т.д.)
            FieldCountBox.Location = new Point(450, FieldTableForm.Count * 25 + 15 - tabPage10.VerticalScroll.Value);
            FieldCountBox.Size = new Size(100, 20);
            FieldCountBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //Первичный ли ключ это поле
            IsPrimaryField.Location = new Point(642, FieldTableForm.Count * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
            IsPrimaryField.Size = new Size(15, 15);
            IsPrimaryField.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            IsPrimaryField.CheckedChanged += PrimaryClick;

            //Допустимо ли значение NULL в этом поле
            IsNullableBox.Location = new Point(788, FieldTableForm.Count * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
            IsNullableBox.Size = new Size(15, 15);
            IsNullableBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            //Автоинкрементно ли это поле
            IsAutoIncrementedField.Location = new Point(942, FieldTableForm.Count * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
            IsAutoIncrementedField.Size = new Size(15, 15);
            IsAutoIncrementedField.Enabled = false;
            IsAutoIncrementedField.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            IsAutoIncrementedField.CheckedChanged += AutoIncrementClick;

            //Кнопка для удаления поля
            DeleteFieldButton.Location = new Point(1000, FieldTableForm.Count * 25 + 15 - tabPage10.VerticalScroll.Value);
            DeleteFieldButton.Text = Localize.GetString("DeleteField");
            DeleteFieldButton.Name = $"{FieldTableForm.Count - 1}";
            DeleteFieldButton.Size = new Size(100, 20);
            DeleteFieldButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            DeleteFieldButton.Click += DeleteSetField;

            //Внесение всех элементов в коллекцию и назначение им одинакого имени значащего номер их поля
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(FieldNameTextBox);
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(FieldTypeBox);
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(FieldCountBox);
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(IsPrimaryField);
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(IsNullableBox);
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(IsAutoIncrementedField);
            FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Add(DeleteFieldButton);
            for (int i = 0; i < FieldTableForm[FieldTableForm.Count - 1].FieldRedactions.Count - 1; i++)
            {
                FieldTableForm[FieldTableForm.Count - 1].FieldRedactions[i].Name = $"{FieldTableForm.Count - 1}";
            }

            //Перенос их на форму
            foreach (Control item in FieldTableForm[FieldTableForm.Count - 1].FieldRedactions)
                tabPage10.Controls.Add(item);

            //Изменения сообщения о количестве полей
            tabPage10.Controls[3].Text = $"CountOfFields: {FieldTableForm.Count}";
        }
        private void DeleteSetField(object sender, EventArgs e)//Удаление поля
        {
            for (int i = 0; i < tabPage10.Controls.Count;)//Удаление всех элементов поля из отображения формы
            {
                if (tabPage10.Controls[i].Name == sender.GetType().GetProperty("Name").GetValue(sender).ToString())
                {
                    tabPage10.Controls.Remove(tabPage10.Controls[i]);
                    continue;
                }
                i++;
            }
            //Удаление из коллекции элементов удаленного поля
            FieldTableForm.Remove(FieldTableForm[int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString())]);

            int Counter = 1;
            foreach (FieldSQl item in FieldTableForm)//Проход по всем полям создаваемой таблицы
            {
                //Все элементы которые были выше удалённых игнорируются
                if (int.Parse(item.FieldRedactions[0].Name) < int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString()))
                {
                    Counter++;
                    continue;
                }
                foreach (Control itemC in tabPage10.Controls)//Проход по всем элементам полей
                {
                    //Сдвинуть поле если оно то которое нужно сдвинуть
                    if (itemC == item.FieldRedactions[0])
                    {
                        item.FieldRedactions[0].Location = new Point(150, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[0].Name = $"{Counter - 1}";
                        itemC.Location = new Point(150, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";
                    }
                    else if (itemC == item.FieldRedactions[1])
                    {
                        item.FieldRedactions[1].Location = new Point(300, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[1].Name = $"{Counter - 1}";
                        itemC.Location = new Point(300, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";
                    }
                    else if (itemC == item.FieldRedactions[2])
                    {
                        item.FieldRedactions[2].Location = new Point(450, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[2].Name = $"{Counter - 1}";
                        itemC.Location = new Point(450, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";

                    }
                    else if (itemC == item.FieldRedactions[3])
                    {
                        item.FieldRedactions[3].Location = new Point(642, Counter * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[3].Name = $"{Counter - 1}";
                        itemC.Location = new Point(642, Counter * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";

                    }
                    else if (itemC == item.FieldRedactions[4])
                    {
                        item.FieldRedactions[4].Location = new Point(788, Counter * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[4].Name = $"{Counter - 1}";
                        itemC.Location = new Point(788, Counter * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";

                    }
                    else if (itemC == item.FieldRedactions[5])
                    {
                        item.FieldRedactions[5].Location = new Point(942, Counter * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[5].Name = $"{Counter - 1}";
                        itemC.Location = new Point(942, Counter * 25 + 3 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";

                    }
                    else if (itemC == item.FieldRedactions[6])
                    {
                        item.FieldRedactions[6].Location = new Point(1000, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[6].Name = $"{Counter - 1}";
                        itemC.Location = new Point(1000, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";
                    }
                }
                Counter++;
            }
            tabPage10.Controls[3].Text = $"CountOfFields: {FieldTableForm.Count}";//Изменение сообщения о количестве полей
        }
    }
}

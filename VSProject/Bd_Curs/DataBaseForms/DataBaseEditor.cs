using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        private List<FieldSQl> FieldTableForm;
        private int AutoIncrementCount;
        private void InitTableForms()//Форма просмотра характеристик таблиц
        {
            int Y,step = 150;//Координаты
            int tempBarrier = tabControl5.TabPages.Count;//Барьер для пропуска печати таблицы Users
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
                ColumnNameZ.Text = "ColumnName";
                ColumnNameZ.Location = new Point(0, 0);
                ColumnNameZ.Width = step;
                ColumnNameZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnNameZ.Font = new Font(ColumnNameZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnNameZ);

                Label ColumnTypeZ = new Label();
                ColumnTypeZ.Text = "TypeOfData";
                ColumnTypeZ.Location = new Point(step, 0);
                ColumnTypeZ.Width = step;
                ColumnTypeZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnTypeZ.Font = new Font(ColumnTypeZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnTypeZ);

                Label ColumnPKZ = new Label();
                ColumnPKZ.Text = "IsPrimaryKey";
                ColumnPKZ.Location = new Point(step*2, 0);
                ColumnPKZ.Width = step;
                ColumnPKZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnPKZ.Font = new Font(ColumnPKZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnPKZ);

                Label ColumnNullZ = new Label();
                ColumnNullZ.Text = "IsNullable";
                ColumnNullZ.Location = new Point(step*3, 0);
                ColumnNullZ.Width = step;
                ColumnNullZ.TextAlign = ContentAlignment.MiddleCenter;
                ColumnNullZ.Font = new Font(ColumnNullZ.Font.FontFamily, 8.5F, FontStyle.Bold);
                tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnNullZ);
                Y += 25;
                //Заполнение колонок содержимым
                for (int j = 0; j < db.Tables[i].Columns.Count; j++)
                {
                    Label ColumnName = new Label();
                    ColumnName.Text = db.Tables[i].ColumnsNames[j];
                    ColumnName.Location = new Point(0, Y);
                    ColumnName.Width = step;
                    ColumnName.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnName);

                    Label ColumnType = new Label();
                    ColumnType.Text = db.Tables[i].Columns[j].type.ToString();
                    ColumnType.Location = new Point(step, Y);
                    ColumnType.Width = step;
                    ColumnType.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnType);

                    Label ColumnPK = new Label();
                    ColumnPK.Text = db.Tables[i].Columns[j].IsPrimaryKey.ToString();
                    ColumnPK.Location = new Point(step*2, Y);
                    ColumnPK.Width = step;
                    ColumnPK.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnPK);

                    Label ColumnNull = new Label();
                    ColumnNull.Text = db.Tables[i].Columns[j].IsNullable.ToString();
                    ColumnNull.Location = new Point(step*3, Y);
                    ColumnNull.Width = step;
                    ColumnNull.TextAlign = ContentAlignment.MiddleCenter;
                    tabControl5.TabPages[IndexTabControl].Controls.Add(ColumnNull);
                    Y += 25;
                }
                IndexTabControl++;//Смещение на следующую таблицу
            }
        }
        private void InitCreateTableForm()
        {
            AutoIncrementCount = 0;
            tabPage10.Controls.Clear();
            FieldTableForm = new List<FieldSQl>();

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

            CreateNewFieldButton.Location = new Point(0,25);
            CreateNewFieldButton.Text = "CreateNewField";
            CreateNewFieldButton.Size = new Size(100,50);
            CreateNewFieldButton.Click += CreateNewField;
            CreateNewFieldButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            TableNameField.Location = new Point(0, 75);
            TableNameField.Text = "Table_Name";
            TableNameField.Size = new Size(100, 25);
            TableNameField.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            CreateTableButton.Location = new Point(0, 95);
            CreateTableButton.Text = "CreateTable";
            CreateTableButton.Size = new Size(100, 50);
            CreateTableButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            CreateTableButton.Click += CreateTableClickAsync;

            CountFields.Location = new Point(0, 0);
            CountFields.Text = "CountOfFields: ";
            CountFields.Size = new Size(150, 30);
            CountFields.Font = new Font(CountFields.Font.FontFamily, 8.5F, FontStyle.Bold);
            CountFields.TextAlign = ContentAlignment.MiddleCenter;
            CountFields.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            FieldName.Location = new Point(150, 0);
            FieldName.Text = "FieldName";
            FieldName.Size = new Size(100, 30);
            FieldName.Font = new Font(FieldName.Font.FontFamily, 8.5F, FontStyle.Bold);
            FieldName.TextAlign = ContentAlignment.MiddleCenter;
            FieldName.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            FieldType.Location = new Point(300, 0);
            FieldType.Text = "FieldType";
            FieldType.Size = new Size(100, 30);
            FieldType.Font = new Font(FieldType.Font.FontFamily, 8.5F, FontStyle.Bold);
            FieldType.TextAlign = ContentAlignment.MiddleCenter;
            FieldType.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            FieldCount.Location = new Point(450, 0);
            FieldCount.Text = "SymbolsCount";
            FieldCount.Size = new Size(100, 30);
            FieldCount.Font = new Font(CountFields.Font.FontFamily, 8.5F, FontStyle.Bold);
            FieldCount.TextAlign = ContentAlignment.MiddleCenter;
            FieldCount.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            IsPrimary.Location = new Point(600, 0);
            IsPrimary.Text = "Primary";
            IsPrimary.Size = new Size(100, 30);
            IsPrimary.Font = new Font(IsPrimary.Font.FontFamily, 8.5F, FontStyle.Bold);
            IsPrimary.TextAlign = ContentAlignment.MiddleCenter;
            IsPrimary.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            IsNullable.Location = new Point(750, 0);
            IsNullable.Text = "Nullable";
            IsNullable.Size = new Size(100, 30);
            IsNullable.Font = new Font(IsNullable.Font.FontFamily, 8.5F, FontStyle.Bold);
            IsNullable.TextAlign = ContentAlignment.MiddleCenter;
            IsNullable.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            IsAutoIncrement.Location = new Point(900, 0);
            IsAutoIncrement.Text = "AutoIncrement";
            IsAutoIncrement.Size = new Size(100, 30);
            IsAutoIncrement.Font = new Font(CountFields.Font.FontFamily, 8.5F, FontStyle.Bold);
            IsAutoIncrement.TextAlign = ContentAlignment.MiddleCenter;
            IsAutoIncrement.Anchor = AnchorStyles.Left | AnchorStyles.Top;

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

        private void AutoIncrementClick(object sender, EventArgs e)
        {
            if (AutoIncrementCount == 1 && sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "false")
                AutoIncrementCount = 0;
            else if (AutoIncrementCount == 0)
                AutoIncrementCount = 1;
            else
            {
                sender.GetType().GetProperty("Checked").SetValue(sender, false);
                AutoIncrementCount = 1;
            }
        }
        private void CreateNewField(object sender, EventArgs e)
        {
            FieldTableForm.Add(new FieldSQl());

            Button DeleteFieldButton = new Button();
            TextBox FieldNameTextBox = new TextBox();
            ComboBox FieldTypeBox = new ComboBox();
            TextBox FieldCountBox = new TextBox();

            CheckBox IsPrimaryField = new CheckBox();
            CheckBox IsNullableBox = new CheckBox();
            CheckBox IsAutoIncrementedField = new CheckBox();

            FieldNameTextBox.Location = new Point(150,FieldTableForm.Count * 25 + 15);
            FieldNameTextBox.Size = new Size(100, 20);
            FieldNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            FieldTypeBox.Location = new Point(300, FieldTableForm.Count * 25 + 15 );
            FieldTypeBox.Size = new Size(100, 20);
            FieldTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            FieldTypeBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            foreach (var item in Enum.GetValues(typeof(SqlDbType)))
            {
                FieldTypeBox.Items.Add(item);
            }
           
            FieldCountBox.Location = new Point(450, FieldTableForm.Count * 25 + 15);
            FieldCountBox.Size = new Size(100, 20);
            FieldCountBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            IsPrimaryField.Location = new Point(642, FieldTableForm.Count * 25 + 3 + 15 );
            IsPrimaryField.Size = new Size(15, 15);
            IsPrimaryField.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            IsPrimaryField.CheckedChanged += PrimaryClick;

            IsNullableBox.Location = new Point(788, FieldTableForm.Count * 25 + 3 + 15);
            IsNullableBox.Size = new Size(15, 15);
            IsNullableBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            
            IsAutoIncrementedField.Location = new Point(942, FieldTableForm.Count * 25 + 3 + 15 );
            IsAutoIncrementedField.Size = new Size(15, 15);
            IsAutoIncrementedField.Enabled = false;
            IsAutoIncrementedField.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            IsAutoIncrementedField.CheckedChanged += AutoIncrementClick;

            DeleteFieldButton.Location = new Point(1000, FieldTableForm.Count * 25 + 15);
            DeleteFieldButton.Text = "DeleteField";
            DeleteFieldButton.Name = $"{FieldTableForm.Count - 1}";
            DeleteFieldButton.Size = new Size(100, 20);
            DeleteFieldButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            DeleteFieldButton.Click += DeleteSetField;
            
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

            foreach (Control item in FieldTableForm[FieldTableForm.Count - 1].FieldRedactions)
                tabPage10.Controls.Add(item);

            tabPage10.Controls[3].Text = $"CountOfFields: {FieldTableForm.Count}";
        }
        private void PrimaryClick(object sender,EventArgs e)
        {
            foreach (Control item in tabPage10.Controls)
            {
                if(item == FieldTableForm[int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString())].FieldRedactions[5])
                {
                    if(sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "true")
                    {
                        item.Enabled = true;
                    }
                    else
                    {
                        item.Enabled = false;
                        item.GetType().GetProperty("Checked").SetValue(item, false);
                    }
                    break;
                }
                else if(item == FieldTableForm[int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString())].FieldRedactions[4])
                {
                    if (sender.GetType().GetProperty("Checked").GetValue(sender).ToString().ToLower() == "true")
                    {
                        item.Enabled = false;
                        item.GetType().GetProperty("Checked").SetValue(item, false);
                    }
                    else
                    {
                        item.Enabled = true;
                    }
                }
            }
        }
        private void DeleteSetField(object sender, EventArgs e)//Оптимизировать(возможно)
        {
            for (int i = 0; i < tabPage10.Controls.Count;)
            {
                if (tabPage10.Controls[i].Name == sender.GetType().GetProperty("Name").GetValue(sender).ToString())
                {
                    tabPage10.Controls.Remove(tabPage10.Controls[i]);
                    continue;
                }
                i++;
            }
            
            FieldTableForm.Remove(FieldTableForm[int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString())]);

            int Counter = 1;
            foreach (FieldSQl item in FieldTableForm)
            {
                if (int.Parse(item.FieldRedactions[0].Name) < int.Parse(sender.GetType().GetProperty("Name").GetValue(sender).ToString()))
                {
                    Counter++;
                    continue;
                }
                foreach (Control itemC in tabPage10.Controls)
                {
                    if (itemC == item.FieldRedactions[0])
                    {
                        item.FieldRedactions[0].Location = new Point(150, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[0].Name = $"{Counter - 1}";
                        itemC.Location = new Point(150, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";
                        
                    }
                    else if(itemC == item.FieldRedactions[1])
                    {
                        item.FieldRedactions[1].Location = new Point(300, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        item.FieldRedactions[1].Name = $"{Counter - 1}";
                        itemC.Location = new Point(300, Counter * 25 + 15 - tabPage10.VerticalScroll.Value);
                        itemC.Name = $"{Counter - 1}";
                    }
                    else if(itemC == item.FieldRedactions[2])
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
            tabPage10.Controls[3].Text = $"CountOfFields: {FieldTableForm.Count}";
        }
        private void CreateTableClickAsync(object sender, EventArgs e)
        {
            int temp;
            int PrimaryCount = 0;
            foreach (Control item in tabPage10.Controls)
            {
                if(int.TryParse(item.Name,out temp))
                {
                    for (int i = 0; i < FieldTableForm[temp].FieldRedactions.Count; i++)
                    {
                        if(item == FieldTableForm[temp].FieldRedactions[i])
                        {
                            try
                            {
                                switch (i)
                                {
                                    case 0:
                                        FieldTableForm[temp].FieldName = item.Text;
                                        break;
                                    case 1:
                                        FieldTableForm[temp].FieldType = (SqlDbType)item.GetType().GetProperty("SelectedItem").GetValue(item);
                                        break;
                                    case 2:
                                        if(FieldTableForm[temp].FieldType is SqlDbType.NChar or SqlDbType.NText or SqlDbType.Text or SqlDbType.VarChar or SqlDbType.NVarChar)
                                            int.TryParse(item.Text, out FieldTableForm[temp].FieldCount);
                                        break;
                                    case 3:
                                        FieldTableForm[temp].IsPrimary = (bool)item.GetType().GetProperty("Checked").GetValue(item);
                                        if (FieldTableForm[temp].IsPrimary)
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
                            catch (Exception){}
                        }
                    }
                }
            }
            if (PrimaryCount < 1)
            {
                MessageBox.Show("U need more PrimaryKeys >=1", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int i = 0; i < FieldTableForm.Count; i++)
            {
                if (FieldTableForm[i].FieldName == string.Empty)
                {
                    MessageBox.Show($"FieldName in {i + 1} field is invalid","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (FieldTableForm[i].FieldType.ToString() == string.Empty)
                {
                    MessageBox.Show($"FieldType in {i + 1} field is invalid","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (FieldTableForm[i].FieldCount <= 0 && FieldTableForm[i].FieldType is SqlDbType.NChar or SqlDbType.NText or SqlDbType.Text or SqlDbType.VarChar or SqlDbType.NVarChar)
                {
                    MessageBox.Show($"FieldCount ({FieldTableForm[i].FieldCount}) in {i + 1} field is invalid","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    return;
                }
            }
            SqlCommand command = new SqlCommand("", db.connection);
            
            string Query = $"CREATE TABLE {tabPage10.Controls[1].Text} (";
            string After = string.Empty;

            for (int i = 0; i < FieldTableForm.Count; i++)
            {
                Query += $" {FieldTableForm[i].FieldName} {FieldTableForm[i].FieldType} ";
                if (FieldTableForm[i].FieldCount != 0)
                    Query += $"( {FieldTableForm[i].FieldCount} )";
                if(!FieldTableForm[i].IsNullable)
                    Query += $" NOT NULL ";
                if(FieldTableForm[i].IsAutoIncrementField)
                    Query += $" IDENTITY(1,1)";

                if (FieldTableForm[i].IsPrimary)
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
            db.SetQuery(Query, command);
            ConnectButton_Click(new object(), EventArgs.Empty);
        }
        public void DropTable(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Back) return;
            DialogResult resultMessage = MessageBox.Show("U really wont delete this table?","Drop Table",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if(resultMessage == DialogResult.Yes)
            {
                db.DropTable(sender.GetType().GetProperty("Text").GetValue(sender).ToString());
                ConnectButton_Click(new object(), EventArgs.Empty);
            }
        }
        
        private void InitTableRelation()
        {
            comboTable1.Items.Clear();
            comboColumn1.Items.Clear();
            comboTable2.Items.Clear();
            comboColumn2.Items.Clear();
            comboColumn2.Enabled = false;
            comboColumn1.Enabled = false;
            foreach (string item in db.TableNames)
            {
                if (item == "Users") continue;
                comboTable1.Items.Add(item);
                comboTable2.Items.Add(item);
            }
        }
        private void comboTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboColumn1.Enabled = true;
            comboColumn1.Items.Clear();
            for (int i = 0; i < db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable1.SelectedItem)].ColumnsNames.Count; i++)
                if (db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable1.SelectedItem)].Columns[i].IsPrimaryKey)
                    comboColumn1.Items.Add(db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable1.SelectedItem)].ColumnsNames[i]);
        }
        private void comboColumn1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboColumn1.SelectedIndex == -1) return;
            comboColumn2.Enabled = true;
            comboColumn2.Items.Clear();

            for (int i = 0; i < db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable2.SelectedItem)].ColumnsNames.Count; i++)
            {
                if (db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable2.SelectedItem)].Columns[i].type != db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable1.SelectedItem)].Columns[comboColumn1.SelectedIndex].type)
                    continue;
                comboColumn2.Items.Add(db.Tables[Array.IndexOf(db.TableNames.ToArray(), comboTable2.SelectedItem)].ColumnsNames[i]);
            }
        }
        private void comboTable2_SelectedIndexChanged(object sender, EventArgs e)=>
            comboColumn1_SelectedIndexChanged(new object(), EventArgs.Empty);
        private void CreateRelation(object sender, EventArgs e)
        {
            if (comboColumn1.Text == string.Empty || comboColumn2.Text == string.Empty || comboTable1.Text == string.Empty || comboTable2.Text == string.Empty)
            {
                MessageBox.Show("Fill in all the fields", "Error",MessageBoxButtons.OK ,MessageBoxIcon.Error);
                return;
            }
            if (comboTable1.Text == comboTable2.Text)
            {
                MessageBox.Show("Tables must be different", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string Query = $"ALTER TABLE {comboTable2.Text} ADD CONSTRAINT FK_{comboColumn1.Text}_{comboColumn2.Text}" +
                $" FOREIGN KEY ({comboColumn2.Text}) REFERENCES {comboTable1.Text} ({comboColumn1.Text})";

            if (checkCascade.Checked)
                Query += " ON DELETE CASCADE ON UPDATE CASCADE";

            db.SetQuery(Query,new SqlCommand(Query,db.connection));
            ConnectButton_Click(new object(), EventArgs.Empty);
        }
    }
    public class FieldSQl
    {
        public string FieldName;
        public SqlDbType FieldType;
        public int FieldCount;
        public bool IsPrimary;
        public bool IsNullable;
        public bool IsAutoIncrementField;
        public List<Control> FieldRedactions =  new List<Control>();
    }
}

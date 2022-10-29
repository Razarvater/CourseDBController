using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Bd_Curs.Properties;

namespace Bd_Curs
{
    public partial class MainForm : Form
    {
        public void LocalizeButton_Click()// смена локализации
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
            LocalizeControls();
        }
        public void LocalizeControls()//Локализация
        {
            //-----------Left Menu-----------//
            label3.Text = Localize.GetString("ServerNamelabel");
            label4.Text = Localize.GetString("DataBaselabel");
            label2.Text = Localize.GetString("Login");
            label1.Text = Localize.GetString("Password");

            CreateDBName.Text = Localize.GetString("CreateDBNameText");

            DisconnectServer.Text = Localize.GetString("DisconServer");
            FromFileButton.Text = Localize.GetString("FileDB");
            ConnectServer.Text = Localize.GetString("ConnectServerButton");
            CreateDBButton.Text = Localize.GetString("CreateDBButton");
            ConnectButton.Text = Localize.GetString("ConnectButtonText");
            DisconnectButton.Text = Localize.GetString("DisConnectButtonText");
            CounterOfConnection.Text = $"{Localize.GetString("TimeOfQuerylabel")}: {Query_Time.ElapsedMilliseconds / 1000}";
            SettingButton.Text = Localize.GetString("Settings");
            //-----------LeftMenu-----------//

            //----------ConnectMessage----------//
            if (db_Connected)
                ConnectionStatus.Text = $"{Localize.GetString("ConnectionStatusServer")}:{ConnectedServer}          {Localize.GetString("ConnectionStatusDB")}:{DbName.Text}";
            else
                ConnectionStatus.Text = Localize.GetString("ConnectionStatusDis");
            //----------ConnectMessage----------//

            //----------TableLabel----------//
            label5.Text = Localize.GetString("Table");
            if (Properties.Settings.Default.Language == "ru")
                label5.Location = new Point(-3, label5.Location.Y);
            else
                label5.Location = new Point(5, label5.Location.Y);
            //----------TableLabel----------//

            //----------LittleForms----------//
            LittleFormTabControl.TabPages[0].Text = Localize.GetString("InsertFormName");
                InsertLocalize();
            LittleFormTabControl.TabPages[1].Text = Localize.GetString("SelectFormName");
                SelectFormTabControl.TabPages[0].Text = Localize.GetString("Select");
                SelectFormTabControl.TabPages[1].Text = Localize.GetString("Condition");
                SelectLocalize();

            LittleFormTabControl.TabPages[2].Text = Localize.GetString("DeleteFormName");
                DeleteLocalize();
            //----------LittleForms----------//

            //----------BigForms----------//
            BigFormTabControl.TabPages[0].Text = Localize.GetString("Selectedtable");
            BigFormTabControl.TabPages[1].Text = Localize.GetString("DbDiagram");
            BigFormTabControl.TabPages[2].Text = Localize.GetString("DbConstructor");
                DbConstructorFormTabControl.TabPages[0].Text = Localize.GetString("TableInfo");
                    TableFormsLocalization();
                DbConstructorFormTabControl.TabPages[1].Text = Localize.GetString("CreateTable");
                    CreateTableFormLocalize();
                DbConstructorFormTabControl.TabPages[2].Text = Localize.GetString("CreateConstraint");
                    label6.Text = Localize.GetString("TableLabel");
                    label8.Text = label6.Text;
                    label7.Text = Localize.GetString("ColumnLabel");
                    label9.Text = label7.Text;
                    label10.Text = Localize.GetString("RelationLabel");
                    checkCascade.Text = Localize.GetString("CascadeCheckBox");
                    CreateRelationButton.Text = Localize.GetString("CreateRelationButton");
            //----------BigForms----------//

            //----------ToolTips----------//
            ServerToolTip.SetToolTip(ServerName, Localize.GetString("TooltipServerName"));
            DbNameTooltip.SetToolTip(DbName, Localize.GetString("TooltipDbName"));
            FileButtonToolTip.SetToolTip(FromFileButton, Localize.GetString("TooltipFromFileButton"));
            //----------ToolTips----------//
        }
    }
}

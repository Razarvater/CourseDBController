using Bd_Curs.Properties;
using System;
using System.Windows.Forms;

namespace Bd_Curs
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }
        //Делегат для смены локализации на главной форме
        public delegate void LocMess();
        public LocMess localizationButton;

        //Делагат для передачи сообщения о том что форма закрыта
        public delegate void Off();
        public Off disable;
        private void SettingForm_Load(object sender, EventArgs e)
        {
            //Добавление возможных локализаций
            for (int i = 0; i < Settings.Default.Languages.Count; i++)
            {
                LanguageBox.Items.Add(Settings.Default.Languages[i]);
            }
            LanguageBox.SelectedItem = Settings.Default.Language;

            ServerName.Text = Settings.Default.ServerName;
            nameAuth.Text = Settings.Default.DefaultLogin;
            passAuth.Text = Settings.Default.DefaultPassword;
            AutoLogin.Checked = Settings.Default.AutoLogin;
            Localize();//Локализация
        }
        private void Apply_Click(object sender, EventArgs e)//Принять изменения
        {
            Settings.Default.AutoLogin = AutoLogin.Checked;
            Settings.Default.DefaultLogin = nameAuth.Text;
            Settings.Default.DefaultPassword = passAuth.Text;
            Settings.Default.ServerName = ServerName.Text;
            Settings.Default.Language = LanguageBox.Text;
            Settings.Default.Save();
            //Сменить локализацию
            localizationButton.Invoke();
            Localize();
        }
        private void Localize()//Локализация всех эелементов
        {
            label1.Text = LocalizatorResource.Localize.GetString("Languagelabel");
            label2.Text = LocalizatorResource.Localize.GetString("DefaultSN");
            label3.Text = LocalizatorResource.Localize.GetString("DefaultLogin");
            label4.Text = LocalizatorResource.Localize.GetString("DefaultPass");
            AutoLogin.Text = LocalizatorResource.Localize.GetString("AutoLogin");
            ApplyButton.Text = LocalizatorResource.Localize.GetString("Apply");
            groupBox1.Text = LocalizatorResource.Localize.GetString("Authorization");

            Text = LocalizatorResource.Localize.GetString("Settings");//Название формы
        }
        //Закрытие формы
        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e) =>
            disable.Invoke();
    }
}

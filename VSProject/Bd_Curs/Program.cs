using System;
using System.Windows.Forms;

namespace Bd_Curs
{
    public static class Program
    {
        public static SettingForm sett;//Форма с настройками

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

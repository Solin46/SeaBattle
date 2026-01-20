using System;
using System.Windows.Forms;

namespace SeaBattle.Client
{
    static class Program
    {
        [STAThread] //Single Threaded Apartment - нужен для корректной работы winfowm
        static void Main() //точка входа программы
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm()); //запуск UI цикла
        }
    }
}

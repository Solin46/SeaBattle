using System;

namespace SeaBattle.Server
{
    class Program
    {
        static void Main()
        {
            var server = new GameServer();
            server.Start(5000);

            Console.WriteLine("Нажмите Enter для остановки сервера...");
            Console.ReadLine();
            server.Stop();
        }
    }
}

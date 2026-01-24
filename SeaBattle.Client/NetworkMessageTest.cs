using System;
using SeaBattle.Common.Networking; // <-- где лежит NetworkMessage

namespace SeaBattle.Client.Tests
{
    public static class NetworkMessageTest
    {
        public static void Run()
        {
            Console.WriteLine("=== NetworkMessage Test ===");

            // Создаем сообщение
            var m1 = new NetworkMessage(NetworkCommand.Shoot, "4,7");
            Console.WriteLine("ToString(): " + m1.ToString()); // должно вывести Shoot|4,7

            // Разбираем сообщение
            var m2 = NetworkMessage.Parse("Shoot|4,7");
            Console.WriteLine("Command: " + m2.Command);      // должно вывести Shoot
            Console.WriteLine("Payload: " + m2.Payload);      // должно вывести 4,7

            Console.WriteLine("=== Test finished ===");
        }
    }
}

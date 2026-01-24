using System;
using System.Net.Sockets;
using System.Text;
using SeaBattle.Common.Networking;

namespace SeaBattle.Server
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public event Action<ClientHandler, NetworkMessage> MessageReceived;


        public ClientHandler(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public void Listen()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string raw = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var message = NetworkMessage.Parse(raw);

                    Console.WriteLine($"Получено: {message.Command} | {message.Payload}");

                    HandleMessage(message);
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Клиент отключился: " + ex.Message);
            }
        }

        /*private void HandleMessage(NetworkMessage message)
        {
            switch (message.Command)
            {
                case NetworkCommand.Hello:
                    Send(new NetworkMessage(NetworkCommand.Hello, "Привет, клиент! Сервер тебя слышит"));
                    break;

                case NetworkCommand.PlaceShips:
                    Console.WriteLine($"Клиент расставил корабли: {message.Payload}");
                    Send(new NetworkMessage(NetworkCommand.PlaceShips, "Корабли приняты"));
                    break;

                case NetworkCommand.Shoot:
                    Console.WriteLine($"Клиент стреляет: {message.Payload}");
                    // Для теста возвращаем рандомный результат
                    Send(new NetworkMessage(NetworkCommand.ShotResult, "Hit"));
                    break;

                default:
                    Send(new NetworkMessage(NetworkCommand.Error, "Неизвестная команда"));
                    break;
            }
        }*/

        //для теста передачи и выстрелов:
        private void HandleMessage(NetworkMessage message)
        {
            switch (message.Command)
            {
                case NetworkCommand.Hello:
                    Send(new NetworkMessage(NetworkCommand.Hello, "Привет, клиент!"));
                    break;

                case NetworkCommand.PlaceShips:
                    Console.WriteLine("Получена расстановка: " + message.Payload);
                    // TODO: здесь сервер сохранит расстановку для игрока
                    break;

                case NetworkCommand.Shoot:
                    Console.WriteLine("Выстрел: " + message.Payload);
                    // TODO: здесь сервер вычисляет Hit/Miss/Sunk
                    var result = "Hit"; // временно
                    Send(new NetworkMessage(NetworkCommand.ShotResult, result));
                    break;

                default:
                    Send(new NetworkMessage(NetworkCommand.Error, "Неизвестная команда"));
                    break;
            }
        }



        public void Send(NetworkMessage msg)
        {
            if (!_client.Connected)
                return;

            byte[] data = Encoding.UTF8.GetBytes(msg.ToString());
            _stream.Write(data, 0, data.Length);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SeaBattle.Server
{
    public class GameServer
    {
        private TcpListener _listener;
        private List<ClientHandler> _clients = new List<ClientHandler>();
        private bool _running = false;

        public void Start(int port = 5000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _running = true;

            Console.WriteLine($"Сервер запущен на порту {port}");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (_running)
            {
                var tcpClient = _listener.AcceptTcpClient();
                Console.WriteLine("Клиент подключился");

                var client = new ClientHandler(tcpClient);
                _clients.Add(client);

                client.MessageReceived += msg =>
                {
                    Console.WriteLine($"Сообщение от клиента: {msg}");
                    // Эхо всем остальным клиентам
                    foreach (var c in _clients)
                        if (c != client) c.SendMessage($"Другой клиент сказал: {msg}");
                };

                Thread clientThread = new Thread(client.Listen);
                clientThread.Start();
            }
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }
    }
}

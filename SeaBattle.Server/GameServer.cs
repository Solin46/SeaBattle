using SeaBattle.Common.Game;
using SeaBattle.Common.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SeaBattle.Server
{
    public class GameServer
    {
        private TcpListener _listener;
        private TcpClient _player1;
        private TcpClient _player2;
        private bool _running;

        private GameBoard _board1;
        private GameBoard _board2;

        private bool _player1Ready;
        private bool _player2Ready;

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
                var client = _listener.AcceptTcpClient();

                if (_player1 == null)
                {
                    _player1 = client;
                    Console.WriteLine("Подключился игрок 1");
                    new Thread(() => ListenClient(_player1)).Start();
                }
                else if (_player2 == null)
                {
                    _player2 = client;
                    Console.WriteLine("Подключился игрок 2");
                    new Thread(() => ListenClient(_player2)).Start();
                }
                else
                {
                    Console.WriteLine("Лишний клиент — отключён");
                    client.Close();
                }
            }
        }


        private void ListenClient(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    // ← ВАЖНО: raw объявляется ЗДЕСЬ
                    string raw = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var msg = NetworkMessage.Parse(raw);

                    Console.WriteLine($"Получено: {msg.Command} | {msg.Payload}");

                    if (msg.Command == NetworkCommand.Shoot)
                    {
                        var parts = msg.Payload.Split(',');
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);

                        var attacker = client;
                        var defenderBoard = client == _player1 ? _board2 : _board1;
                        var defenderClient = client == _player1 ? _player2 : _player1;

                        if (defenderBoard == null || defenderClient == null)
                            continue;

                        var result = defenderBoard.Shoot(x, y);
                        string res = result.ToString().ToLower();

                        Send(attacker, new NetworkMessage(
                            NetworkCommand.ShotResult,
                            $"{x},{y},{res}"
                        ));

                        Send(defenderClient, new NetworkMessage(
                            NetworkCommand.EnemyShot,
                            $"{x},{y},{res}"
                        ));

                        if (defenderBoard.AllShipsSunk())
                        {
                            SendToBoth(new NetworkMessage(NetworkCommand.GameOver, ""));
                        }
                    }

                    //не нужен, наделяет клиента принятием решения
                    /*else if (msg.Command == NetworkCommand.ShotResult){
                            TcpClient target =
                                client == _player1 ? _player2 : _player1;

                            if (target != null && target.Connected)
                            {
                                byte[] data = Encoding.UTF8.GetBytes(msg.ToString());
                                target.GetStream().Write(data, 0, data.Length);
                            }
                    }*/

                    else if (msg.Command == NetworkCommand.PlaceShips)
                    {
                        var board = client == _player1 ? _board1 : _board2;
                        if (board == null)
                            board = new GameBoard();

                        var ships = msg.Payload.Split(';');
                        foreach (var ship in ships)
                        {
                            var coords = ship.Split('|')
                                .Select(p =>
                                {
                                    var xy = p.Split(',');
                                    return (int.Parse(xy[0]), int.Parse(xy[1]));
                                })
                                .ToList();

                            board.PlaceShip(new Ship(coords));
                        }

                        if (client == _player1)
                        {
                            _board1 = board;
                            _player1Ready = true;
                        }
                        else
                        {
                            _board2 = board;
                            _player2Ready = true;
                        }

                        Console.WriteLine("Игрок прислал расстановку");

                        if (_player1Ready && _player2Ready)
                        {
                            SendToBoth(new NetworkMessage(NetworkCommand.GameStart, ""));
                        }

                        continue;
                    }




                    /* Эхо обратно всем клиентам
                    //Broadcast(msg.ToString(), client);

                    // Вместо Broadcast с исключением отправителя
                    var data = Encoding.UTF8.GetBytes(msg.ToString());
                    stream.Write(data, 0, data.Length);  // отправляем обратно тому же клиенту*/

                }
            }
            catch
            {
                Console.WriteLine("Клиент отключился");
            }
        }
        //вызывает ошибку, потому что _client заменён на двух игроков
        /*private void Broadcast(string message, TcpClient sender = null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var client in _clients)
            {
                if (!client.Connected)
                    continue;

                // Если клиент отправил сообщение сам, то для теста с 1 игроком можно его пропустить,
                // но если это игра, sender != null — не отправляем обратно отправителю
                if (sender != null && client == sender)
                    continue;

                try
                {
                    client.GetStream().Write(data, 0, data.Length);
                }
                catch
                {
                    // Игнорируем ошибки записи, например, если клиент отключился
                    Console.WriteLine("Не удалось отправить сообщение клиенту");
                }
            }
        }*/

        private void SendToBoth(NetworkMessage msg)
        {
            var data = Encoding.UTF8.GetBytes(msg.ToString());
            _player1?.GetStream().Write(data, 0, data.Length);
            _player2?.GetStream().Write(data, 0, data.Length);
        }

        private void Send(TcpClient client, NetworkMessage msg)
        {
            var data = Encoding.UTF8.GetBytes(msg.ToString());
            client.GetStream().Write(data, 0, data.Length);
        }


        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }

    }
}

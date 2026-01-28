using SeaBattle.Common.Enums;
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
        //готовность игроков
        private bool _player1Ready;
        private bool _player2Ready;

        private TcpClient _currentTurn;   // кто сейчас ходит
        private TcpClient _firstPlayer;   // кто первым расставился


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
                    Send(_player1, new NetworkMessage(
                        NetworkCommand.PlayerRole, "player1"));

                    Console.WriteLine("Подключился игрок 1");
                    new Thread(() => ListenClient(_player1)).Start();
                }
                else if (_player2 == null)
                {
                    _player2 = client;
                    Send(_player2, new NetworkMessage(
                        NetworkCommand.PlayerRole, "player2"));

                    Console.WriteLine("Подключился игрок 2");
                    new Thread(() => ListenClient(_player2)).Start();
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

                    string raw = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var msg = NetworkMessage.Parse(raw);

                    Console.WriteLine($"Получено: {msg.Command} | {msg.Payload}");

                    if (msg.Command == NetworkCommand.Shoot)
                    {
                        if (client != _currentTurn)
                        {
                            // не его ход — игнорируем
                            continue;
                        }

                        var parts = msg.Payload.Split(',');
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);

                        var attacker = client;
                        var defenderBoard = client == _player1 ? _board2 : _board1;
                        var defenderClient = client == _player1 ? _player2 : _player1;

                        if (defenderBoard == null || defenderClient == null)
                            continue;

                        /*var result = defenderBoard.Shoot(x, y);

                        // если уже стреляли — просто игнорируем
                        if (result == ShotResult.AlreadyShot)
                            continue;

                        string payload;

                        if (result == ShotResult.Miss)
                        {
                            _currentTurn = client == _player1 ? _player2 : _player1;
                            payload = $"{x},{y},miss";
                        }
                        else if (result == ShotResult.Hit)
                        {
                            payload = $"{x},{y},hit";
                        }
                        else // ShotResult.Sunk
                        {
                            var ship = defenderBoard.GetShipAt(x, y);

                            var shipCells = string.Join("|",
                                ship.Decks.Select(d => $"{d.X}:{d.Y}")
                            );

                            payload = $"{x},{y},sunk,{shipCells}";
                        }

                        Send(attacker, new NetworkMessage(
                            NetworkCommand.ShotResult,
                            payload
                        ));

                        Send(defenderClient, new NetworkMessage(
                            NetworkCommand.EnemyShot,
                            payload
                        ));

                        // сервер
                        Send(attacker, new NetworkMessage(
                            NetworkCommand.YourTurn, "false"
                        ));

                        Send(defenderClient, new NetworkMessage(
                            NetworkCommand.YourTurn, "true"
                        ));*/

                        var result = defenderBoard.Shoot(x, y);

                        if (result == ShotResult.AlreadyShot)
                            continue;

                        string payload;

                        if (result == ShotResult.Miss)
                        {
                            payload = $"{x},{y},miss";
                        }
                        else if (result == ShotResult.Hit)
                        {
                            payload = $"{x},{y},hit";
                        }
                        else // Sunk
                        {
                            var ship = defenderBoard.GetShipAt(x, y);
                            var shipCells = string.Join("|",
                                ship.Decks.Select(d => $"{d.X}:{d.Y}")
                            );

                            payload = $"{x},{y},sunk,{shipCells}";
                        }

                        // отправка результата
                        Send(attacker, new NetworkMessage(NetworkCommand.ShotResult, payload));
                        Send(defenderClient, new NetworkMessage(NetworkCommand.EnemyShot, payload));

                        // ===== ЛОГИКА ХОДОВ =====
                        bool changeTurn = result == ShotResult.Miss;

                        if (changeTurn)
                        {
                            _currentTurn = client == _player1 ? _player2 : _player1;
                        }

                        Send(_player1, new NetworkMessage(
                            NetworkCommand.YourTurn,
                            _currentTurn == _player1 ? "true" : "false"
                        ));

                        Send(_player2, new NetworkMessage(
                            NetworkCommand.YourTurn,
                            _currentTurn == _player2 ? "true" : "false"
                        ));



                        if (defenderBoard.AllShipsSunk())
                        {
                            string winner = client == _player1 ? "player1" : "player2";

                            SendToBoth(new NetworkMessage(NetworkCommand.GameOver, winner));
                        }
                    }

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

                        //первый игрок - первый, отправивший корабли
                        if (_firstPlayer == null)
                        {
                            _firstPlayer = client;
                        }

                        /*if (_player1Ready && _player2Ready)
                        {
                            _currentTurn = _firstPlayer;

                            SendToBoth(new NetworkMessage(
                                NetworkCommand.GameStart,
                                _currentTurn == _player1 ? "player1" : "player2"
                            ));
                        }*/

                        if (_player1Ready && _player2Ready)
                        {
                            _currentTurn = _firstPlayer;

                            // просто уведомление
                            SendToBoth(new NetworkMessage(NetworkCommand.GameStart, ""));

                            // А ВОТ ЗДЕСЬ — НАЧАЛЬНЫЙ ХОД
                            Send(_player1, new NetworkMessage(
                                NetworkCommand.YourTurn,
                                _currentTurn == _player1 ? "true" : "false"
                            ));

                            Send(_player2, new NetworkMessage(
                                NetworkCommand.YourTurn,
                                _currentTurn == _player2 ? "true" : "false"
                            ));
                        }


                        continue;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Клиент отключился");
            }
        }
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
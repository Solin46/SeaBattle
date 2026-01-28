using SeaBattle.Common.Enums;
using SeaBattle.Common.Game;
using SeaBattle.Common.Networking;
using System;
using System.IO;
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

        private bool _gameFinished = false;
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
                try
                {
                    var client = _listener.AcceptTcpClient();
                    Console.WriteLine($"Новое подключение от {client.Client.RemoteEndPoint}");

                    // Проверяем и очищаем "мертвые" соединения
                    CheckAndCleanDeadConnections();

                    // Сбрасываем состояние игры при новом подключении
                    _gameFinished = false;

                    if (_player1 == null || !IsClientConnected(_player1))
                    {
                        _player1 = client;
                        _player1Ready = false;
                        _board1 = null;

                        Console.WriteLine("Назначен как player1");
                        Send(_player1, new NetworkMessage(NetworkCommand.PlayerRole, "player1"));

                        new Thread(() => ListenClient(_player1)).Start();
                    }
                    else if (_player2 == null || !IsClientConnected(_player2))
                    {
                        _player2 = client;
                        _player2Ready = false;
                        _board2 = null;

                        Console.WriteLine("Назначен как player2");
                        Send(_player2, new NetworkMessage(NetworkCommand.PlayerRole, "player2"));

                        new Thread(() => ListenClient(_player2)).Start();
                    }
                    else
                    {
                        Console.WriteLine("Отклонено: сервер полон");
                        Send(client, new NetworkMessage(NetworkCommand.Error, "Server full"));
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка AcceptClients: {ex.Message}");
                }
            }
        }

        private bool IsClientConnected(TcpClient client)
        {
            if (client == null) return false;

            try
            {
                // Простая проверка подключения
                return client.Connected &&
                       client.Client != null &&
                       client.Client.Connected;
            }
            catch
            {
                return false;
            }
        }

        private void CheckAndCleanDeadConnections()
        {
            // Проверяем player1
            if (_player1 != null && !IsClientConnected(_player1))
            {
                Console.WriteLine("Обнаружен отключенный player1, очистка...");
                try { _player1.Close(); } catch { }
                _player1 = null;
                _player1Ready = false;
                _board1 = null;
            }

            // Проверяем player2
            if (_player2 != null && !IsClientConnected(_player2))
            {
                Console.WriteLine("Обнаружен отключенный player2, очистка...");
                try { _player2.Close(); } catch { }
                _player2 = null;
                _player2Ready = false;
                _board2 = null;
            }
        }


        private void ListenClient(TcpClient client)
        {
            NetworkStream stream = null;

            try
            {
                stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) //клиент отключился
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

                            // Проверяем, не закончилась ли уже игра
                            if (!_gameFinished)
                            {
                                _gameFinished = true; // Помечаем игру как завершенную

                                Console.WriteLine($"=== ИГРА ОКОНЧЕНА. ПОБЕДИТЕЛЬ: {winner} ===");
                                SendToBoth(new NetworkMessage(NetworkCommand.GameOver, winner));
                            }
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
            catch (IOException ioex)
            {
                Console.WriteLine($"Клиент отключился (IOException): {ioex.Message}");
            }
            catch (SocketException sex)
            {
                Console.WriteLine($"Клиент отключился (SocketException): {sex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ListenClient: {ex.Message}");
            }
            finally
            {
                try
                {
                    // Закрываем соединение
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }

                    if (client != null && client.Connected)
                    {
                        client.Close();
                    }
                }
                catch { }

                // Очищаем состояние
                CleanupPlayer(client);
            }
        }

        private void CleanupPlayer(TcpClient disconnectedClient)
        {
            string disconnectedPlayer = disconnectedClient == _player1 ? "player1" : "player2";
            Console.WriteLine($"Очистка {disconnectedPlayer}");

            if (disconnectedClient == _player1)
            {
                _player1 = null;
                _player1Ready = false;
                _board1 = null;

                // Если игрок 2 еще в игре - НЕ отправляем GameOver, а сбрасываем его состояние
                if (_player2 != null && _player2.Connected)
                {
                    Console.WriteLine("Сбрасываем состояние player2 для новой игры");

                    // Отправляем сообщение о сбросе
                    Send(_player2, new NetworkMessage(
                        NetworkCommand.OpponentDisconnected, // Новая команда
                        ""
                    ));

                    // Сбрасываем готовность player2
                    _player2Ready = false;
                    _board2 = null;
                }
            }
            else if (disconnectedClient == _player2)
            {
                _player2 = null;
                _player2Ready = false;
                _board2 = null;

                if (_player1 != null && _player1.Connected)
                {
                    Console.WriteLine("Сбрасываем состояние player1 для новой игры");

                    Send(_player1, new NetworkMessage(
                        NetworkCommand.OpponentDisconnected,
                        ""
                    ));

                    _player1Ready = false;
                    _board1 = null;
                }
            }

            ResetGameState();
        }

        private void ResetGameState()
        {
            Console.WriteLine("Сброс состояния игры");
            // Сбрасываем флаг завершения игры
            _gameFinished = false;

            // Если один игрок отключился - сбрасываем готовность второго
            if (_player1 == null && _player2 != null)
            {
                _player2Ready = false;
                _board2 = null;
                Send(_player2, new NetworkMessage(NetworkCommand.Hello, "Ожидание второго игрока..."));
            }
            else if (_player2 == null && _player1 != null)
            {
                _player1Ready = false;
                _board1 = null;
                Send(_player1, new NetworkMessage(NetworkCommand.Hello, "Ожидание второго игрока..."));
            }

            _currentTurn = null;
            _firstPlayer = null;
        }
        private void Send(TcpClient client, NetworkMessage msg)
        {
            if (client == null || !client.Connected)
            {
                Console.WriteLine($"Попытка отправить сообщение отключенному клиенту: {msg.Command}");
                return;
            }

            try
            {
                var data = Encoding.UTF8.GetBytes(msg.ToString());
                var stream = client.GetStream();

                // Проверяем, доступен ли поток для записи
                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    Console.WriteLine($"Поток клиента недоступен для записи: {msg.Command}");
                }
            }
            catch (SocketException sex)
            {
                Console.WriteLine($"SocketException при отправке {msg.Command}: {sex.Message}");
                // Помечаем клиента как отключенного
                if (client == _player1)
                {
                    _player1 = null;
                    _player1Ready = false;
                }
                else if (client == _player2)
                {
                    _player2 = null;
                    _player2Ready = false;
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine($"IOException при отправке {msg.Command}: {ioex.Message}");
                // Клиент разорвал соединение
                try { client.Close(); } catch { }

                if (client == _player1)
                {
                    _player1 = null;
                    _player1Ready = false;
                    Console.WriteLine("Игрок 1 отключился (при отправке)");
                }
                else if (client == _player2)
                {
                    _player2 = null;
                    _player2Ready = false;
                    Console.WriteLine("Игрок 2 отключился (при отправке)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки {msg.Command}: {ex.Message}");
            }
        }

        private void SendToBoth(NetworkMessage msg)
        {
            // Отправляем только подключенным клиентам
            if (_player1 != null && _player1.Connected)
            {
                try { Send(_player1, msg); }
                catch { /* игнорируем, так как Send уже обрабатывает ошибки */ }
            }

            if (_player2 != null && _player2.Connected)
            {
                try { Send(_player2, msg); }
                catch { /* игнорируем */ }
            }
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }

    }
}
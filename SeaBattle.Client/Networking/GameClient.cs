using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SeaBattle.Common.Networking;

namespace SeaBattle.Client.Networking
{
    public class GameClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread _listenThread;

        public event Action<NetworkMessage> MessageReceived;

        public void Connect(string host, int port)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(host, port);
            _stream = _tcpClient.GetStream();

            _listenThread = new Thread(Listen);
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        private void Listen()
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
                    var msg = NetworkMessage.Parse(raw);

                    MessageReceived?.Invoke(msg);
                }
            }
            catch
            {
                Console.WriteLine("Сервер отключился");
            }
        }

        public void Send(NetworkMessage msg)
        {
            if (_tcpClient?.Connected == true)
            {
                byte[] data = Encoding.UTF8.GetBytes(msg.ToString());
                _stream.Write(data, 0, data.Length);
            }
        }
        //Отправка расстановки и выстрелов
        public void SendShipPlacement(string payload)
        {
            Send(new NetworkMessage(NetworkCommand.PlaceShips, payload));
        }

        public void SendShot(int x, int y)
        {
            Send(new NetworkMessage(NetworkCommand.Shoot, $"{x},{y}"));
        }


        public void Disconnect()
        {
            _tcpClient?.Close();
        }
    }
}

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeaBattle.Client.Networking
{
    public class GameClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public event Action<string> MessageReceived;

        public void Connect(string ip, int port)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(ip, port);
            _stream = _tcpClient.GetStream();

            // Запуск чтения сообщений в отдельном потоке
            Task.Run(() => ReceiveLoop());
        }

        public void Send(string message)
        {
            if (_stream == null) return;
            var data = Encoding.UTF8.GetBytes(message + "\n");
            _stream.Write(data, 0, data.Length);
        }

        private void ReceiveLoop()
        {
            var buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    MessageReceived?.Invoke(msg);
                }
                catch
                {
                    break;
                }
            }
        }

        public void Close()
        {
            _stream?.Close();
            _tcpClient?.Close();
        }
    }
}

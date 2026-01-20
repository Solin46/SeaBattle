using System;
using System.Net.Sockets;
using System.Text;

namespace SeaBattle.Server
{
    public class ClientHandler
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public event Action<string> MessageReceived;

        public ClientHandler(TcpClient client)
        {
            _client = client;
            _stream = _client.GetStream();
        }

        public void Listen()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int byteCount = _stream.Read(buffer, 0, buffer.Length);
                    if (byteCount == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    MessageReceived?.Invoke(msg);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Клиент отключился");
            }
        }

        public void SendMessage(string message)
        {
            if (!_client.Connected) return;
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);
        }
    }
}

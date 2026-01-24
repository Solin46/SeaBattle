using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Common.Networking
{
    public class NetworkMessage
    {
        public NetworkCommand Command { get; }
        public string Payload { get; }

        public NetworkMessage(NetworkCommand command, string payload = "")
        {
            Command = command;
            Payload = payload;
        }

        // =========================
        // Сериализация → строка
        // =========================
        public override string ToString()
        {
            return $"{Command}|{Payload}";
        }

        // =========================
        // Десериализация ← строка
        // =========================
        public static NetworkMessage Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Empty message");

            // исправлено для .NET Framework
            var parts = message.Split(new[] { '|' }, 2);

            if (!Enum.TryParse(parts[0], out NetworkCommand command))
                throw new ArgumentException("Unknown command");

            string payload = parts.Length > 1 ? parts[1] : "";

            return new NetworkMessage(command, payload);
        }

    }
}


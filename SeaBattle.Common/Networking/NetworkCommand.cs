using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Common.Networking
{
    public enum NetworkCommand
    {
        Hello,        // тест / подключение
        PlayerRole,
        YourTurn,
        Error,        // ошибка
        PlaceShips,   // клиент отправил расстановку
        GameStart,    // сервер сообщает: игра началась
        Shoot,        // выстрел (x,y)
        ShotResult,   // результат твоего выстрела
        EnemyShot,    // по тебе стреляли (x,y,hit/miss)
        GameOver      // конец игры
    }
}


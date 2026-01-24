using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Common.Networking
{
    public enum NetworkCommand
    {
        Hello,          // клиент подключился
        PlaceShips,     // клиент закончил расстановку
        Shoot,          // выстрел: x,y
        ShotResult,     // результат выстрела
        GameOver,       // конец игры
        Restart,        // переиграть
        Error           // ошибка
    }
}


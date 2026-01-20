using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Common.Enums
{
    public enum ShotResult
    {
        Miss,           // промах
        Hit,            // попадание
        Sunk,           // корабль потоплен
        AlreadyShot,    // уже стреляли в эту клетку
    }
}


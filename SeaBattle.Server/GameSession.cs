using SeaBattle.Common.Game;
using SeaBattle.Common.Networking;
using SeaBattle.Server;

public class GameSession
{
    private ClientHandler _player1;
    private ClientHandler _player2;
    private Game _game;

    public GameSession(ClientHandler p1, ClientHandler p2)
    {
        _player1 = p1;
        _player2 = p2;
        _game = new Game();
    }

    public void Start()
    {
        // Можно отправить клиентам сигнал начала игры
        _player1.Send(new NetworkMessage(
    NetworkCommand.Hello,
    "Начало игры"
));

        _player2.Send(new NetworkMessage(
            NetworkCommand.Hello,
            "Начало игры"
        ));

    }

    public void HandlePlacement(ClientHandler player, Ship ship)
    {
        // Обработка расстановки кораблей
        // Добавить проверку через GameBoard
    }

    public void HandleShot(ClientHandler player, int x, int y)
    {
        // Обработка выстрела, смена хода, проверка победы
    }
}

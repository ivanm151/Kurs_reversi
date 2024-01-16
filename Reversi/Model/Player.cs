using System.Windows.Media;

namespace Reversi.Model
{
    /// <summary>
    /// Игрок
    /// </summary>
    public class Player
    {
        public string Player_name { get; set; }
        public int Score { get; set; } // кол-во очков полученных игроком
        public SolidColorBrush Players_brush; // цвет фишек игрока
        public int ID { get; set; } // первый или второй игрок
        public Player(string name, SolidColorBrush brush, int id, int score = 0)
        {
            Player_name = name;
            Score = score;
            Players_brush = brush;
            ID = id;
        }


    }
}
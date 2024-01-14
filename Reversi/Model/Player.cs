using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Reversi.Model
{
    public class Player
    {
        public string Player_name { get; set; }
        public int Score { get; set; }
        public SolidColorBrush Players_brush;
        public int ID { get; set; }
        public Player(string name, SolidColorBrush brush, int id, int score = 0)
        {
            Player_name = name;
            Score = score;
            Players_brush = brush;
            ID = id;
        }

       
    }
}
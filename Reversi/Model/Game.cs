using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Reversi.Model
{
    public interface ISetGame
    {
        Markers CreateGameField();
    }
    public class NewGame : ISetGame
    {
        int height;
        public NewGame(int h)
        {
            height = h;
        }
        public Markers CreateGameField()
        {
            Markers markers = new Markers(height);
            Set_Init_Disks(markers);
            return markers;
        }
        private void Set_Init_Disks(Markers markers)
        {
            Markers.Marker[,] Placed_markers = markers.Placed_markers;
            height = Placed_markers.GetLength(0) - 1;

            if (height % 2 == 0)
                height = height / 2;
            else
                height = (int)Math.Floor((double)height / 2);
            int border = (int)Math.Floor((double)Placed_markers.GetLength(0) / 8);

            for (int i = 0; i <= border; i++)
                for (int j = 0; j <= border; j += 2)
                {
                    Placed_markers[height + i, height + i + j] = new Markers.Marker(Brushes.Black, 1);
                    Placed_markers[height + i, height - i + j + 1] = new Markers.Marker(Brushes.White, 2);
                }
        }
    }
    
    public class GameSetter
    {
        public GameAttributes MakeGame(string name1, string name2, bool fpnext, ISetGame setter)
        {
            
            Player Player1 = new Player("Игрок 1", Brushes.Black, 1);          
            Player Player2 = new Player("Игрок 2", Brushes.White, 2);
            Markers placedMarkers = setter.CreateGameField();
            GameAttributes g = new GameAttributes(Player1, Player2, fpnext, placedMarkers);
            g.CalculateScore();
            return g;
        }
    }
    public class GameAttributes
    {
        public Player ActivePlayer;
        public Player Player1;
        public Player Player2;
        public Markers Placed;
        public bool first_player_next;
        public bool gameFinished = false;
        public GameAttributes(Player p1, Player p2, bool firstPlayer, Markers markers)
        {
            Player1 = p1;
            Player2 = p2;
            first_player_next = firstPlayer;
            Placed = markers;
            ActivePlayer = first_player_next ? Player2 : Player1;
        }
        public void SetActivePlayer()
        {
            ActivePlayer = first_player_next ? Player1 : Player2;
            first_player_next = !first_player_next;
        }
        public void Add_Marker(int r, int c)
        {
            Placed.Add_Marker(r, c, ActivePlayer); // Обращение к классу поля, помещение диска в массив фишек
            SetActivePlayer(); // Смена активного игрока по завершении хода
            CalculateScore(); // Вычисление значений счёта игроков
            if (Player1.Score + Player2.Score == Placed.Placed_markers.Length)
            {
                gameFinished = true;
            } // Если количество фишек, поставленных игроками, равно размеру поля, то игра окончена
        }
        public void CalculateScore()
        {
            (Player1.Score, Player2.Score) = Placed.CalcScore();
        }
        public Player Winner()
        {
            if (Player1.Score != Player2.Score)
                return Player1.Score > Player2.Score ? Player1 : Player2;
            else
                return new Player("Дружба", null, Player1.Score);
        }
        public int[] DisksToInt()
        {
            Markers.Marker[,] markers = Placed.Placed_markers;
            int h = markers.GetLength(0);
            int[,] toInt = new int[h, h];
            for (int i = 0; i < h; i++)
                for (int j = 0; j < h; j++)
                {
                    if (markers[i, j] == null)
                        toInt[i, j] = 0;
                    else
                        toInt[i, j] = markers[i, j].OwnerID;
                }
            return toInt.Cast<int>().ToArray();
        }
    }
}

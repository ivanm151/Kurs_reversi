using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Reversi.Model.Markers;
using System.Windows.Shapes;

namespace Reversi.Model
{
    /// <summary>
    /// ИИ противник-компьютер для режима игры для одного игрока
    /// </summary>
    public class Computer_Player
    {
        private static Computer_Player instance;

        private Computer_Player()
        { }
        /// <summary>
        /// Использован паттерн Singleton тк Singleton позволяет создать объект только при его необходимости и в единственном экземпляре.
        /// Игрок-компьютер создается один раз и только в режиме для одного игрока
        /// </summary>
        /// <returns></returns>
        public static Computer_Player getInstance()
        {
            if (instance == null)
                instance = new Computer_Player();
            return instance;
        }

        private bool added_successfully;
        private Player player;
        private Ellipse added_marker;
        private (int, int) canva_ind;
        public Computer_Player(Player pl)
        {
            player = pl;
        }
        /// <summary>
        /// ход компьютера
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public ((int, int), Ellipse) Computers_Turn(GameAttributes game)
        {
            added_successfully = false;
            Random rand = new Random();
            int rnd = rand.Next(0, 4);
            (int, int)[] directions = { (0, 1), (-1, 0), (0, -1), (1, 0) };
            (int, int) dir = directions[rnd];
            Marker[,] markers = game.Placed.Placed_markers;
            int height = markers.GetLength(0);
            PlaceMarker(markers, height, dir, game);
            if (added_successfully)
            {
                return (canva_ind, added_marker);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    if (i != rnd)
                    {
                        PlaceMarker(markers, height, directions[i], game);
                        if (added_successfully)
                            return (canva_ind, added_marker);
                    }
            }
            return ((-1, -1), null);
        }
        private void PlaceMarker(Marker[,] markers, int height, (int, int) dir, GameAttributes game)
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < height; j++)
                    if (Is_In_Borders(i + dir.Item1, j + dir.Item2, markers))
                        if (markers[i, j] == null && markers[i + dir.Item1, j + dir.Item2] != null)
                            if (markers[i + dir.Item1, j + dir.Item2].OwnerID != player.ID)
                            {
                                game.Add_Marker(i, j);
                                canva_ind = (i, j);
                                added_marker = game.Placed.Placed_markers[i, j].shape;
                                added_successfully = true;
                                return;
                            }
        }
        private bool Is_In_Borders(int c, int r, Marker[,] markers)
        {
            if (c < 0 || r < 0 || r >= markers.GetLength(0) || c >= markers.GetLength(1))
                return false;
            return true;
        }
    }
}

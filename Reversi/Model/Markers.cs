using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Reversi.Model
{
    public class Markers
    {
        public class Marker
        {
            public Ellipse shape;
            private int grid_size = 20;
            public int OwnerID { get; set; }
            public Marker(SolidColorBrush brush, int id)
            {
                OwnerID = id;
                shape = new Ellipse
                {
                    Width = grid_size,
                    Height = grid_size,
                    Fill = brush
                };
            }
        }

        public Marker[,] Placed_markers;
        public Markers(int height)
        {
            Placed_markers = new Marker[height, height];
        }
        private bool Borders(int c, int r)
        {
            if (c < 0 || r < 0 || r >= Placed_markers.GetLength(0) || c >= Placed_markers.GetLength(0))
                return false;
            return true;
        }
        private bool Neighbours(int r, int c)
        {
            for (int i = r - 1; i < r + 2; i++)
                for (int j = c - 1; j < c + 2; j++)
                {
                    if (Borders(i, j))
                        if (Placed_markers[i, j] != null)
                            return true;
                }
            return false;
        }
        public void Add_Marker(int i, int j, Player ActivePlayer)
        {
            if (Neighbours(i, j) && Placed_markers[i, j] == null)
            {
                Placed_markers[i, j] = new Marker(ActivePlayer.Players_brush, ActivePlayer.ID);
                Flip_Marker(i, j, ActivePlayer);
            }
            else
                throw new NotImplementedException();
        }

        public (int, int) CalcScore()
        {
            int pl1Score = 0, pl2Score = 0;
            for (int i = 0; i < Placed_markers.GetLength(0); i++)
                for (int j = 0; j < Placed_markers.GetLength(1); j++)
                    if (Placed_markers[i, j] != null)
                        if (Placed_markers[i, j].OwnerID == 1)
                            pl1Score++;
                        else
                            pl2Score++;
            return (pl1Score, pl2Score);
        }

        private void Flip_Marker(int r, int c, Player ActivePlayer)
        {
            (int, int)[] directions = { (1, 0), (1, 1), (0, 1), (-1, 0), (-1, -1), (0, -1), (1, -1), (-1, 1) };
            int Act_ID = ActivePlayer.ID;
            SolidColorBrush brush = ActivePlayer.Players_brush;
            int count, i, j;
            foreach (var dir in directions)
            {
                count = 0;
                i = r + dir.Item1;
                j = c + dir.Item2;
                while (Borders(i, j))
                {
                    if (Placed_markers[i, j] != null)
                    {
                        if (Placed_markers[i, j].OwnerID == Act_ID)
                        {
                            Reverse_Markers(r, c, count, dir, Act_ID, brush);
                            break;
                        }
                    }
                    else
                        break;
                    i += dir.Item1;
                    j += dir.Item2;
                    count++;
                }
            }
        }
        private void Reverse_Markers(int r, int c, int count, (int, int) dir, int Act_ID, SolidColorBrush brush)
        {
            for (int i = 0; i < count; i++)
            {
                r += dir.Item1;
                c += dir.Item2;
                Placed_markers[r, c].OwnerID = Act_ID;
                Placed_markers[r, c].shape.Fill = brush;
            }
        }


    }
}

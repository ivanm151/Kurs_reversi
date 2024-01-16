using Reversi.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Windows;
using System.IO;
using System.Xml.Linq;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Controls;

namespace Reversi
{
    public class ViewModel : INotifyPropertyChanged
    {
        public delegate void AccountHandler((int, int) iter, Ellipse disk);
        public event AccountHandler AddDiskToField;
        public delegate void AccountHandler2();
        public event AccountHandler2 StartNewGame, IncorrectInput, EndGame, ShowActivePlayer, LoadGame1;
        private (int, int) canvas_indexes;
        private Ellipse last_added;
        public GameAttributes Game;
        private bool single_player;
        private Computer_Player Comp_player;
        private bool computerTurnInProcess;
        private GameSetter gameSetter;       

        private int low;
        private int high;

        
        public ViewModel()
        {
            Visibility = "Hidden";
            gameSetter = new GameSetter();

            var appSettings = ConfigurationManager.AppSettings;
            low = Convert.ToInt32(appSettings["low"]);
            high = Convert.ToInt32(appSettings["high"]);          
        }

        public void SaveGame()
        {
            var height = Game.GetHeight();
            var score1 = Game.Player1.Score;
            var score2 = Game.Player2.Score;
            var active = Game.ActivePlayer.Player_name;
            var markers = Game.Stroka();         
            var fileName = "saveGame.txt";

            using (StreamWriter sw = File.CreateText(fileName))
            {
                sw.Write(height);
                sw.Write("\n");
                sw.Write(score1);
                sw.Write("\n");
                sw.Write(score2);
                sw.Write("\n");
                sw.Write(active);
                sw.Write("\n");
                sw.Write(markers);
                sw.Write("\n");               
            }
        }

        private Command save;
        public Command Save
        {
            get
            {
                return save ?? (save = new Command(obj =>
                {
                    try
                    {
                        SaveGame();
                    }
                    catch
                    {
                        IncorrectInput.Invoke();
                    }                   
                }));
            }
        }
       
        private Command load;
        public Command Load
        {
            get
            {
                return load ?? (load = new Command(obj =>
                {
                    try
                    {
                        
                        var inputFileName = "saveGame.txt";
                        string fileContents;
                        using (StreamReader sr = File.OpenText(inputFileName))
                        {
                            fileContents = sr.ReadLine();
                            var height = fileContents;
                            fileContents = sr.ReadLine();
                            var score1 = fileContents;
                            fileContents = sr.ReadLine();
                            var score2 = fileContents;
                            fileContents = sr.ReadLine();
                            var active = fileContents;
                            fileContents = sr.ReadLine();
                            var markers = fileContents;

                            FieldHeight = height;                                                        
                            LoadGame1.Invoke();
                            Visibility = "Visible";
                            int count = 0;
                            for (int i = 0; i < Convert.ToInt32(height); i++)
                                for (int j = 0; j < Convert.ToInt32(height); j++)
                                {
                                    
                   
                                    if (markers[count] == '1')
                                        AddDiskToField.Invoke((i, j), new Ellipse { Width = 20, Height = 20, Fill = Brushes.Black });
                                    if (markers[count] == '2')
                                        AddDiskToField.Invoke((i, j), new Ellipse { Width = 20, Height = 20, Fill = Brushes.White });
                                    count++;
                                }
                            Game.Player1.Score = Convert.ToInt32(score1);
                            Game.Player2.Score = Convert.ToInt32(score2);
                        }
                        
                    }
                    catch
                    {
                        IncorrectInput.Invoke();
                    }
                }));
            }
        }

        public void Create_GameAttributes(int h, bool single_player, string name1, string name2)
        {
            Game = gameSetter.MakeGame(name1, name2, false, new NewGame(h));
            Display_Init_Gameset();
            this.single_player = single_player;
            computerTurnInProcess = false;
            if (single_player)
                Comp_player = new Computer_Player(Game.Player2);
            ShowActivePlayer.Invoke();          
        }

        private void Display_Init_Gameset()
        {
            int h = Game.Placed.Placed_markers.GetLength(0);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < h; j++)
                    if (Game.Placed.Placed_markers[i, j] != null)
                        AddDiskToField.Invoke((i, j), Game.Placed.Placed_markers[i, j].shape);
        }

        public void Process_Click(System.Windows.Point p)
        {
            if (!computerTurnInProcess)
            {
                int col = (int)Math.Floor(p.X / 20);
                int row = (int)Math.Floor(p.Y / 20);
                try
                {
                    Game.Add_Marker(row, col);
                    canvas_indexes = (row, col);
                    last_added = Game.Placed.Placed_markers[row, col].shape;
                    ShowActivePlayer.Invoke();
                }
                catch
                {
                    canvas_indexes = (-1, -1);
                    last_added = null;
                }
                AddDiskToField.Invoke(canvas_indexes, last_added);
                if (!Game.gameFinished)
                {
                    if (single_player && last_added != null)
                        Make_Computers_Turn();
                }
                else
                {
                    ProcessEndGame();
                }
            }
        }

        private async void Make_Computers_Turn()
        {
            computerTurnInProcess = true;
            await Task.Delay(500);
            (canvas_indexes, last_added) = Comp_player.Computers_Turn(Game);
            if (last_added != null)
                AddDiskToField.Invoke(canvas_indexes, last_added);
            if (last_added == null || Game.gameFinished)
                ProcessEndGame();
            else
            {
                ShowActivePlayer.Invoke();
            }
            computerTurnInProcess = false;
        }

        private void ProcessEndGame()
        {            
            EndGame.Invoke();
            Visibility = "Hidden";
        }
        
        private Command startGame;
        public Command StartGame
        {
            get
            {
                return startGame ?? (startGame = new Command(obj =>
                {
                    try
                    {
                        MessageBox.Show("Первый ход делают чёрные. Далее игроки ходят по очереди.\r\nДелая ход, игрок должен поставить свою фишку на одну из клеток доски таким образом, чтобы между этой поставленной фишкой и одной из имеющихся уже на доске фишек его цвета находился непрерывный ряд фишек соперника, горизонтальный, вертикальный или диагональный (другими словами, чтобы непрерывный ряд фишек соперника оказался «закрыт» фишками игрока с двух сторон). Все фишки соперника, входящие в «закрытый» на этом ходу ряд, переворачиваются на другую сторону (меняют цвет) и переходят к ходившему игроку.\r\nЕсли в результате одного хода «закрывается» одновременно более одного ряда фишек противника, то переворачиваются все фишки, оказавшиеся на тех «закрытых» рядах, которые идут от поставленной фишки.\r\nИгрок вправе выбирать любой из возможных для него ходов. Если игрок имеет возможные ходы, он не может отказаться от хода. Если игрок не имеет допустимых ходов, то ход передаётся сопернику.\r\nИгра прекращается, когда на доску выставлены все фишки или когда ни один из игроков не может сделать хода. По окончании игры проводится подсчёт фишек каждого цвета, и игрок, чьих фишек на доске выставлено больше, объявляется победителем. В случае равенства количества фишек засчитывается ничья.");
                        int h = int.Parse(FieldHeight);
                        if (h < low || h > high)
                            IncorrectInput.Invoke();
                        else
                        {
                            StartNewGame.Invoke();
                            Visibility = "Visible";                           
                        }
                    }
                    catch
                    {
                        IncorrectInput.Invoke();
                    }
                    FieldHeight = "";
                }));
            }
        }        
        private string fieldHeight;
        public string FieldHeight
        {
            get
            {
                return this.fieldHeight;
            }
            set
            {
                fieldHeight = value;
                OnPropertyChanged();
            }
        }

        private string visibility;
        public string Visibility
        {
            get
            {
                return this.visibility;
            }
            set
            {
                visibility = value;
                OnPropertyChanged();
            }
        }

        private string currentName;
        public string CurrentName
        {
            get
            {
                return this.currentName;
            }
            set
            {
                currentName = value;
                OnPropertyChanged();
            }
        }
      
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}

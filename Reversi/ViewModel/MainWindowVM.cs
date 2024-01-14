using Reversi.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;



namespace Reversi
{
    public class ViewModel : INotifyPropertyChanged
    {
        public delegate void AccountHandler((int, int) iter, Ellipse disk);
        public event AccountHandler AddDiskToField;
        public delegate void AccountHandler2();
        public event AccountHandler2 StartNewGame, IncorrectInput, EndGame, ShowActivePlayer;
        private (int, int) canvas_indexes;
        private Ellipse last_added;
        public GameAttributes Game;
        private bool single_player;
        private Computer_Player Comp_player;
        private bool computerTurnInProcess;
        private GameSetter gameSetter;
        
        public ViewModel()
        {
            Visibility = "Hidden";
            gameSetter = new GameSetter();
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
                        int h = int.Parse(FieldHeight);
                        if (h < 8 || h > 20)
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

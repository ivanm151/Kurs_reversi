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
                        LoadGame();
                    }
                    catch
                    {
                        IncorrectInput.Invoke();
                    }
                }));
            }
        }
        public void LoadGame()
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

                
                //Game = gameSetter.MakeGame("Игрок 1", "Игрок 2", false, new NewGame(Convert.ToInt32(height)));
                //Create_GameAttributes(Convert.ToInt32(height), false, "Игрок 1", "Игрок 2");
                /*Game.Player1.Score = Convert.ToInt32(score1);
                Game.Player2.Score = Convert.ToInt32(score2);
                Game.ActivePlayer.Player_name = active;*/

                /*int count = 0;
                for(int i = 0; i < Convert.ToInt32(height); i++)
                    for(int j = 0; j < Convert.ToInt32(height); j++)
                    {
                        Game.Add_Marker(i, j, markers[count]);
                        count++;
                    }*/
                

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

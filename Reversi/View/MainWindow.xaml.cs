using Microsoft.Win32;
using Reversi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace Reversi.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
            ((ViewModel)DataContext).AddDiskToField += RenewGameArea;
            ((ViewModel)DataContext).ShowActivePlayer += ChangeCurrentInfo;            
            ((ViewModel)DataContext).StartNewGame += Start_Click;
            ((ViewModel)DataContext).IncorrectInput += IncorrectInput;
            ((ViewModel)DataContext).EndGame += EndGame;
            
        }

        private void IncorrectInput()
        {
            MessageBox.Show("Введите числа от 8 до 20!");
        }

        

        private void EndGame()
        {
            int Score1 = ((ViewModel)DataContext).Game.Player1.Score;
            int Score2 = ((ViewModel)DataContext).Game.Player2.Score;
            string name1 = ((ViewModel)DataContext).Game.Player1.Player_name;
            string name2 = ((ViewModel)DataContext).Game.Player2.Player_name;
            MessageBox.Show($"Конец игры! \n Счёт игрока {name1}: {Score1}" +
                $"\n Счёт игрока {name2}: {Score2}" +
                $"\n Победитель - {((ViewModel)DataContext).Game.Winner().Player_name}!" +
                $"\n Начать новую игру?");
            GameArea.Children.Clear();
        }

        private void ChangeCurrentInfo()
        {
            Player player = ((ViewModel)DataContext).Game.ActivePlayer;
            CurrentPlayerName.Content = player.Player_name;
            CurrentPlayerBrush.Fill = player.Players_brush;
            Score.Content = player.Score;
        }

        private void DrawGameArea(int field_height, int side = 20)
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            int colCounter = 0;
            bool nextIsOdd = false;
            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = side,
                    Height = side,
                    Fill = nextIsOdd ? Brushes.Silver : Brushes.DarkGray
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);
                Canvas.SetZIndex(rect, 0);
                nextIsOdd = !nextIsOdd;
                nextX += side;
                colCounter++;
                if (nextX > GameArea.ActualWidth || colCounter == field_height)
                {
                    nextX = 0;
                    nextY += side;
                    rowCounter++;
                    colCounter = 0;
                    nextIsOdd = rowCounter % 2 != 0;
                }

                if (nextY > GameArea.ActualHeight || rowCounter == field_height)
                    doneDrawingBackground = true;
            }
        }

        private void RenewGameArea((int, int) iter, Ellipse disk)
        {
            if (disk == null)
                MessageBox.Show("Фишки можно размещать только рядом с другими дисками!");
            else
            {
                GameArea.Children.Add(disk);
                Canvas.SetTop(disk, iter.Item1 * 20);
                Canvas.SetLeft(disk, iter.Item2 * 20);
                Canvas.SetZIndex(disk, 1);
            }
        }
        private void Clear()
        {
            GameArea.Children.Clear();
        }
        private void Start_Click()
        {
            Clear();
            int height;
            height = int.Parse(fieldHeight.Text);
            DrawGameArea(height);
            ((ViewModel)DataContext).Create_GameAttributes(height, (bool)One.IsChecked, "Игрок 1", "Игрок 2");
        }

        private void MouseClick(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(GameArea);
            ((ViewModel)DataContext).Process_Click(p);
        }   
    }
}


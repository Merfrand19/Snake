
using System;
using System.Collections.Generic;
using System.Linq;
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
using Snake.Games;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rnd= new Random();
        const int SnakeSquareSize=20;
        const int SnakeStartLength= 3;
        const int SnakeStartSpeed=400;
        const int SnakeSpeedThreshold=100;
        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();
        public enum SnakeDirection { Left, Right, Up, Down};
        private SnakeDirection snakeDirection=SnakeDirection.Right;
        private int snakeLength;
        private int currentScore =0;
        private UIElement snakefood= null;
        private SolidColorBrush foodBrush=Brushes.Red;
        private void Window_ContentRendered(object sender , EventArgs e)
       {
           DrawGameArea();
           StartNewGame();
       }
       public MainWindow()
       {
           InitializeComponent();
           gameTickTimer.Tick += GameTickTimer_Tick;
       }
       private void DrawGameArea()
       {
           bool doneDrawingBackground=false;
           int nextX=0, nextY=0;
           int rowCounter=0;
           bool nextIsOdd=false;
    
            while(doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.Black :Brushes.White
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect , nextX);
                Canvas.SetLeft(rect, nextY);
                
                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;
                if(nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 !=0); 
                }
                if(nextY >= GameArea.ActualHeight)
                {
                    doneDrawingBackground = true ;
                }
            } 
        }
        //Create snake
        private void DrawSnake()
        {
            foreach(SnakePart snakePart in snakeParts)
            {
                if(snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width= SnakeSquareSize,
                        Height= SnakeSquareSize,
                        Fill= (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush )
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }
        private void MoveSnake()
        {
            //Remove the last part of the snake, in preparation of the new part added below
            while(snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }
            //Next up , we'll add a new element to the snake, which will be the (new) head
            //Therefore, we mark all existing parts as non-head (body) elements and then
            //we makesure that they usethe bofy brush
            foreach(SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }
            //Determine in which direction to expand the snake,based on the current direction 
            SnakePart snakeHead= snakeParts[snakeParts.Count-1];
            double nextX= snakeHead.Position.X;
            double nextY= snakeHead.Position.Y;
            switch(snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY  -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }
            //Now add the new head part to our list of snake parts...
            snakeParts.Add(new SnakePart()
            {
                Position =new Point(nextX, nextY),
                IsHead = true
            });
            DrawSnake();
            DoCollisionCheck();
           
        }
        private void GameTickTimer_Tick(object sender , EventArgs e)
        {
            MoveSnake();
        } 
        private void StartNewGame()
        {
           //remove potential dead snake partd and leftover food...
           foreach (SnakePart snakeBodyPart in snakeParts)
           {
               if(snakeBodyPart.UiElement != null)
               {
                   GameArea.Children.Remove(snakeBodyPart.UiElement);
               }
            }   
            snakeParts.Clear();
            if(snakefood != null)
            GameArea.Children.Remove(snakefood);
            //Reset stuff
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection= SnakeDirection.Right;
            snakeParts.Add(new SnakePart() {Position = new Point(SnakeSquareSize * 5 , SnakeSquareSize * 5)});
            gameTickTimer.Interval= TimeSpan.FromMilliseconds(SnakeStartSpeed);
           //Draw the snake again and some food...
           DrawSnake();
           DrawSnakeFood();
           //Update status
           UpdateGameStatus();               //Go!
           gameTickTimer.IsEnabled= true;
           
        }

        //Add food for the snake
        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0 , maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0 , maxY) * SnakeSquareSize;
            foreach (SnakePart snakePart in snakeParts)
            {
                if((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                {
                    return GetNextFoodPosition();
                }
            }
             return new Point(foodX , foodY);
        }
         private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            snakefood= new Ellipse()
            {
                Width= SnakeSquareSize, 
                Height = SnakeSquareSize,
                Fill=  foodBrush
            };
            GameArea.Children.Add(snakefood);
            Canvas.SetTop(snakefood, foodPosition.Y);
            Canvas.SetLeft(snakefood, foodPosition.X);
        }
        private void Window_KeyUp(Object sender , KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection=snakeDirection;
            switch(e.Key)
            {
                case Key.Up:
                    if(snakeDirection != SnakeDirection.Down)
                    {
                        snakeDirection = SnakeDirection.Up;
                    }
                    break;
                case Key.Down:
                    if(snakeDirection != SnakeDirection.Up)
                    {
                        snakeDirection = SnakeDirection.Down;
                    }
                    break;
                case Key.Right:
                    if(snakeDirection != SnakeDirection.Left)
                    {
                        snakeDirection = SnakeDirection.Right;
                    }  
                    break;
                case Key.Left:
                    if(snakeDirection != SnakeDirection.Right)
                    {
                        snakeDirection = SnakeDirection.Left;
                    }  
                    break;
                case Key.Space:
                     StartNewGame();
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
            {
                MoveSnake();
            }
            
        }   
        private void DoCollisionCheck()
        {
            SnakePart snakeHead= snakeParts[snakeParts.Count-1];
            if((snakeHead.Position.X== Canvas.GetLeft(snakefood)) && (snakeHead.Position.Y == Canvas.GetTop(snakefood)))
            {
                EatSnakeFood();
                return ;
            }
            if((snakeHead.Position.Y <0) || (snakeHead.Position.Y >= GameArea.ActualHeight) || (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }
            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count -1))
            {
                if((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                {
                    EndGame();
                }
            }
        }
        private void EatSnakeFood()
        {
            snakeLength++;
            currentScore++;
            int timerInterval= Math.Max(SnakeSpeedThreshold , (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakefood);
            DrawSnakeFood();
            UpdateGameStatus();
        }
        private void UpdateGameStatus()
        {
            this.Title = "SnakeWPF - Score : "+currentScore+ " - GameSpeed : "+gameTickTimer.Interval.TotalMilliseconds;
        }
        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;
            MessageBox.Show("Oooops , you died\n Press the space bar to start new game");
        }
    }
}    

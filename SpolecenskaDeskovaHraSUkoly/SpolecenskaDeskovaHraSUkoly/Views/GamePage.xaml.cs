using SpolecenskaDeskovaHraSUkoly.Models;
using SpolecenskaDeskovaHraSUkoly.Services;
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
using System.Windows.Threading;

namespace SpolecenskaDeskovaHraSUkoly.Views
{
    /// <summary>
    /// Interakční logika pro GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private List<TaskItem> _tasks;
        private readonly Random _rnd = new Random();
        private TaskService _taskService;
        private GameState _gameState;
        private MainWindow _mainWindow;

        private TaskItem _currentTask;
        private bool _taskRevealed = false;
        private int _timeRemaining;
        private bool _isMainCountdown = false;
        private bool _taskCorrect = false;

        private string _selectedBoardType;
        private string _selectedBoardSize;

        private int totalTileCount;
        private int boardWidthHeight;
        private List<BoardTile> ActualBoard;

        public GamePage(MainWindow mainWindow, List<Player> players, string selectedBoardType, string selectedBoardSize)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            _taskService = new TaskService();
            _gameState = new GameState();

            _gameState.Players = players;
            RefreshPlayersList();

            this._selectedBoardType = selectedBoardType;
            this._selectedBoardSize = selectedBoardSize;

            this.Loaded += GamePage_Loaded;
        }
        private void RefreshPlayersList()
        {
            PlayersListBox.Items.Clear();
            for (int i = 0; i < _gameState.Players.Count; i++)
            {
                var p = _gameState.Players[i];
                string playerList = $"{i + 1}. {p.Name} — body: {p.Score} — pozice: {p.Position}";
                if (i == _gameState.CurrentPlayerIndex)
                {
                    playerList += " ← na tahu";
                }
                PlayersListBox.Items.Add(playerList);
            }
        }
        private async void ButtonDiceRoll_Click(object sender, RoutedEventArgs e)
        {
            ButtonDiceRoll.IsEnabled = false;

            int finalValue = 0;

            for (int i = 0; i < 10; i++)
            {
                int value = _rnd.Next(1, 7);
                LabelDiceResult.Content = value.ToString();
                await Task.Delay(150);
                finalValue = value;
            }

            var currentPlayer = _gameState.Players[_gameState.CurrentPlayerIndex];
            currentPlayer.Position += finalValue;


            currentPlayer.Position %= totalTileCount;
            RefreshPlayersList();
            RenderPlayerEllipses();

            BoardTile currentTile = ActualBoard[currentPlayer.Position];
            char addRemovePoints;
            if (currentTile.Type == TileType.Bonus)
            {
                int amountOfPoints = 2;
                currentPlayer.Score += amountOfPoints;
                RefreshPlayersList();

                addRemovePoints = '+';
                ShowBonusPenaltyOverlay(currentPlayer.Name, currentTile, addRemovePoints, amountOfPoints);

            }
            else if (currentTile.Type == TileType.Penalty)
            {
                int amountOfPoints;
                if (currentPlayer.Score > 2)
                {
                    amountOfPoints = 2;
                }
                else
                {
                    amountOfPoints = 0;
                }
                currentPlayer.Score -= amountOfPoints;
                RefreshPlayersList();

                addRemovePoints = '-';
                ShowBonusPenaltyOverlay(currentPlayer.Name, currentTile, addRemovePoints, amountOfPoints);
            }
            else if (currentTile.Type == TileType.Empty)
            {
                NextPlayerTurn();
            }
            else if (currentTile.Type == TileType.Task)
            {
                ShowRandomTask();
            }
            //work!!!
        }

        private void ShowBonusPenaltyOverlay(string currentPlayerName, BoardTile currentTile, char addRemovePoints, int amountOfPoints)
        {
            BonusPenaltyText.Text = currentTile.Type.ToString();
            BonusPenaltyDesc.Text = $"{currentPlayerName}   {addRemovePoints}{amountOfPoints}";

            BonusPenaltyGrid.Visibility = Visibility.Visible;
            TaskButton.Visibility = Visibility.Visible;
            TaskGrid.Visibility = Visibility.Collapsed;
            CountdownGrid.Visibility = Visibility.Collapsed;
            CorrectAnswerGrid.Visibility = Visibility.Collapsed;
            Overlay.Visibility = Visibility.Visible;
        }

        private void ShowTaskOverlay(TaskItem task, string currentPlayerName)
        {
            _currentTask = task;
            _taskRevealed = false;

            TaskTypeText.Text = task.Type;
            TaskInstructionText.Text = $"Všichni hráči kromě {currentPlayerName} se otočí, aby neviděli zadání!";
            TaskButton.Content = "Zobrazit úkol";

            TaskGrid.Visibility = Visibility.Visible;
            TaskButton.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Collapsed;
            ContinueButton.Visibility = Visibility.Collapsed;
            CountdownGrid.Visibility = Visibility.Collapsed;
            CorrectAnswerGrid.Visibility = Visibility.Collapsed;
            BonusPenaltyGrid.Visibility = Visibility.Collapsed;
            Overlay.Visibility = Visibility.Visible;
        }

        private void HideTaskOverlay()
        {
            Overlay.Visibility = Visibility.Collapsed;
            _currentTask = null;
        }

        private async void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTask == null)
            {
                return;
            }

            if (!_taskRevealed)
            {
                TaskInstructionText.Text = _currentTask.Text;
                TaskButton.Content = "Pokračovat";
                _taskRevealed = true;
            }
            else
            {
                TaskGrid.Visibility = Visibility.Collapsed;
                CountdownGrid.Visibility = Visibility.Visible;
                await StartCountdownAsync(5, false);

                await StartCountdownAsync(10, true);
            }
        }

        private async Task StartCountdownAsync(int seconds, bool isMain)
        {
            _timeRemaining = seconds;
            _isMainCountdown = isMain;

            CountdownText.Text = _timeRemaining.ToString();
            FailButton.Visibility = Visibility.Collapsed;
            if(_isMainCountdown)
            {
                SuccessButton.Visibility = Visibility.Visible;
            }
            else
            {
                SuccessButton.Visibility = Visibility.Collapsed;
            }
            

            for (int i = _timeRemaining; i > 0 && !_taskCorrect; i--)
            {
                CountdownText.Text = i.ToString();
                await Task.Delay(1000);
            }

            if(!_isMainCountdown)
            {
                CountdownText.Text = "Start!";
                await Task.Delay(1000);
                return;
            }

            CountdownText.Text = "Konec!";
            FailButton.Visibility = Visibility.Visible;
            SuccessButton.Visibility = Visibility.Visible;
        }

        private void FailButton_Click(object sender, RoutedEventArgs e)
        {
            FailButton.IsEnabled = false;
            SuccessButton.IsEnabled = false;

            CountdownGrid.Visibility = Visibility.Collapsed;
            TaskGrid.Visibility = Visibility.Visible;
            TaskInstructionText.Text = _currentTask.Text;
            TaskButton.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Visible;
            ContinueButton.Visibility = Visibility.Visible;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            TaskGrid.Visibility = Visibility.Collapsed;
            BonusPenaltyGrid.Visibility = Visibility.Collapsed;

            NextPlayerTurn();
        }

        private void SuccessButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxPlayerAnswered.SelectedItem = null;
            _taskCorrect = true;

            FailButton.IsEnabled = false;
            SuccessButton.IsEnabled = false;

            CountdownGrid.Visibility = Visibility.Collapsed;
            CorrectAnswerGrid.Visibility = Visibility.Visible;

            List<string> playerNames = new List<string>();
            foreach (Player p in _gameState.Players)
            {
                if(p.Name != _gameState.Players[_gameState.CurrentPlayerIndex].Name)
                {
                    playerNames.Add(p.Name);
                }
            }
            ComboBoxPlayerAnswered.ItemsSource = playerNames;

            AddPointsButton.IsEnabled = false;
        }

        private void ComboBoxPlayerAnswered_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxPlayerAnswered.SelectedItem != null)
            {
                AddPointsButton.IsEnabled = true;
            }
            else
            {
                AddPointsButton.IsEnabled = false;
            }
        }

        private void AddPointsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentPlayer = _gameState.Players[_gameState.CurrentPlayerIndex];

            currentPlayer.Score += 2;

            string selectedPlayerName = ComboBoxPlayerAnswered.SelectedItem as string;
            if(selectedPlayerName != null)
            {
                foreach(Player p in _gameState.Players)
                {
                    if(p.Name == selectedPlayerName)
                    {
                        p.Score += 1;
                        break;
                    }
                }
            }

            RefreshPlayersList();

            NextPlayerTurn();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            FailButton.IsEnabled = true;
            SuccessButton.IsEnabled = true;

            CountdownGrid.Visibility = Visibility.Visible;
            CorrectAnswerGrid.Visibility = Visibility.Collapsed;
            TaskGrid.Visibility = Visibility.Collapsed;

        }

        private void NextPlayerTurn()
        {
            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;

            _taskCorrect = false;
            ComboBoxPlayerAnswered.SelectedItem = null;
            FailButton.IsEnabled = true;
            SuccessButton.IsEnabled = true;
            RefreshPlayersList();
            HideTaskOverlay();
            ButtonDiceRoll.IsEnabled = true;
        }

        private async void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(50);
            try
            {
                _tasks = _taskService.LoadTasks();

                if (!_tasks.Any())
                {
                    MessageBox.Show("Žádné úkoly nenalezeny. Návrat do nastavení hry.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _mainWindow.MainFrame.Navigate(new GameSettingsPage(_mainWindow, _gameState.Players));
                    return;
                }

                TxtLastTask.Text = $"Nahráno {_tasks.Count} úkolů. Příklad: [{_tasks[0].Type}] {_tasks[0].Text}";

                BoardTilePrint();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání úkolů: " + ex.Message + "\nNávrat do nastavení hry.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                _mainWindow.MainFrame.Navigate(new GameSettingsPage(_mainWindow, _gameState.Players));
            }
        }

        private void BoardTilePrint()
        {
            boardWidthHeight = 4; //Default

            if(string.Equals(_selectedBoardSize, "4x4"))
            {
                boardWidthHeight = 4;
            }
            else if (string.Equals(_selectedBoardSize, "6x6"))
            {
                boardWidthHeight = 6;
            }
            else if (string.Equals(_selectedBoardSize, "8x8"))
            {
                boardWidthHeight = 8;
            }
            else if (string.Equals(_selectedBoardSize, "10x10"))
            {
                boardWidthHeight = 10;
            }


            //Circle Board
            List<BoardTile> BoardCircle = new List<BoardTile>();

            int index = 0;
            for (int c = 0; c < boardWidthHeight; c++)
            {
                Direction from = Direction.Left;
                Direction to = Direction.Right;
                if(c == 0)
                {
                    from = Direction.Down;
                }
                else if(c == boardWidthHeight - 1)
                {
                    to = Direction.Down;
                }
                BoardCircle.Add(new BoardTile { Index = index, Row = 0, Column = c , From = from, To = to});
                index++;
            }

            for (int r = 1; r < boardWidthHeight - 1; r++)
            {
                Direction from = Direction.Up;
                Direction to = Direction.Down;
                BoardCircle.Add(new BoardTile { Index = index, Row = r, Column = boardWidthHeight - 1, From = from, To = to });
                index++;
            }

            for (int c = boardWidthHeight - 1; c >= 0; c--)
            {
                Direction from = Direction.Right;
                Direction to = Direction.Left;
                if (c == boardWidthHeight - 1)
                {
                    from = Direction.Up;
                }
                else if (c == 0)
                {
                    to = Direction.Up;
                }
                BoardCircle.Add(new BoardTile { Index = index, Row = boardWidthHeight - 1, Column = c, From = from, To = to });
                index++;
            }

            for (int r = boardWidthHeight - 2; r > 0; r--)
            {
                Direction from = Direction.Down;
                Direction to = Direction.Up;
                BoardCircle.Add(new BoardTile { Index = index, Row = r, Column = 0, From = from, To = to });
                index++;
            }


            //Snake Board
            List<BoardTile> BoardSnake = new List<BoardTile>();

            index = 0;
            int row = 0;
            for (int c = 0; c < boardWidthHeight; c++)
            {
                Direction from = Direction.Left;
                Direction to = Direction.Right;
                if (c == 0)
                {
                    from = Direction.Down;
                }
                else if (c == boardWidthHeight - 1)
                {
                    to = Direction.Down;
                }
                BoardSnake.Add(new BoardTile { Index = index, Row = row, Column = c, From = from, To = to });
                index++;
            }
            row++;

            while(row < boardWidthHeight - 1)
            {
                for (int c = boardWidthHeight - 1; c > 0; c--)
                {
                    Direction from = Direction.Right;
                    Direction to = Direction.Left;
                    if (c == boardWidthHeight - 1)
                    {
                        from = Direction.Up;
                    }
                    else if (c == 1)
                    {
                        to = Direction.Down;
                    }
                    BoardSnake.Add(new BoardTile { Index = index, Row = row, Column = c, From = from, To = to });
                    index++;
                }
                row++;

                for (int c = 1; c < boardWidthHeight; c++)
                {
                    Direction from = Direction.Left;
                    Direction to = Direction.Right;
                    if (c == 1)
                    {
                        from = Direction.Up;
                    }
                    else if (c == boardWidthHeight - 1)
                    {
                        to = Direction.Down;
                    }
                    BoardSnake.Add(new BoardTile { Index = index, Row = row, Column = c, From = from, To = to });
                    index++;
                }
                row++;
            }

            for (int c = boardWidthHeight - 1; c >= 0; c--)
            {
                Direction from = Direction.Right;
                Direction to = Direction.Left;
                if (c == boardWidthHeight - 1)
                {
                    from = Direction.Up;
                }
                else if (c == 0)
                {
                    to = Direction.Up;
                }
                BoardSnake.Add(new BoardTile { Index = index, Row = row, Column = c, From = from, To = to });
                index++;
            }
            row++;

            for (int r = boardWidthHeight - 2; r > 0; r--)
            {
                Direction from = Direction.Down;
                Direction to = Direction.Up;
                BoardSnake.Add(new BoardTile { Index = index, Row = r, Column = 0, From = from, To = to });
                index++;
            }

            

            totalTileCount = boardWidthHeight * boardWidthHeight;

            ActualBoard = BoardCircle; //default
            if (string.Equals(_selectedBoardType, "Circle"))
            {
                ActualBoard = BoardCircle;
                totalTileCount -= (boardWidthHeight - 2) * (boardWidthHeight - 2);
            }
            else if(string.Equals(_selectedBoardType, "Snake"))
            {
                ActualBoard = BoardSnake;
            }

            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < boardWidthHeight; i++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition());
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            BoardGrid.Width = BoardGrid.ActualWidth - 100;
            BoardGrid.Height = BoardGrid.Width;
            BoardGrid.HorizontalAlignment = HorizontalAlignment.Center;
            BoardGrid.VerticalAlignment = VerticalAlignment.Center;
            double tileWidth = (BoardGrid.Width / boardWidthHeight);
            double tileHeight = tileWidth;

            PlayerCanvas.Width = BoardGrid.Width;
            PlayerCanvas.Height = BoardGrid.Height;
            PlayerCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            PlayerCanvas.VerticalAlignment = VerticalAlignment.Center;


            //totalTileCount

            int bonusTileCount = (int)(totalTileCount * 0.15);
            int penaltyTileCount = (int)(totalTileCount * 0.10);
            int emptyTileCount = (int)(totalTileCount * 0.05);
            int taskTileCount = totalTileCount - (bonusTileCount + penaltyTileCount + emptyTileCount);

            Random rnd = new Random();
            for(int i = 0; i < bonusTileCount; i++)
            {
                bool nalezenoPolicko = false;
                while(!nalezenoPolicko)
                {
                    int cislo = rnd.Next(0, totalTileCount);

                    BoardTile foundTile = ActualBoard[cislo];

                    if(foundTile.Type == null)
                    {
                        foundTile.Type = TileType.Bonus;
                        nalezenoPolicko = true;
                    }
                }
            }

            for (int i = 0; i < penaltyTileCount; i++)
            {
                bool nalezenoPolicko = false;
                while (!nalezenoPolicko)
                {
                    int cislo = rnd.Next(0, totalTileCount);

                    BoardTile foundTile = ActualBoard[cislo];

                    if (foundTile.Type == null)
                    {
                        foundTile.Type = TileType.Penalty;
                        nalezenoPolicko = true;
                    }
                }
            }

            for (int i = 0; i < emptyTileCount; i++)
            {
                bool nalezenoPolicko = false;
                while (!nalezenoPolicko)
                {
                    int cislo = rnd.Next(0, totalTileCount);

                    BoardTile foundTile = ActualBoard[cislo];

                    if (foundTile.Type == null)
                    {
                        foundTile.Type = TileType.Empty;
                        nalezenoPolicko = true;
                    }
                }
            }

            for (int i = 0; i < taskTileCount; i++)
            {
                bool nalezenoPolicko = false;
                while (!nalezenoPolicko)
                {
                    int cislo = rnd.Next(0, totalTileCount);

                    BoardTile foundTile = ActualBoard[cislo];

                    if (foundTile.Type == null)
                    {
                        foundTile.Type = TileType.Task;
                        nalezenoPolicko = true;
                    }
                }
            }



            foreach (var tile in ActualBoard)
            {
                Brush backgroundColor = null;
                if(tile.Type == TileType.Bonus)
                {
                    backgroundColor = new SolidColorBrush(Color.FromRgb(51, 163, 52));
                }
                else if (tile.Type == TileType.Penalty)
                {
                    backgroundColor = new SolidColorBrush(Color.FromRgb(186, 47, 47));
                }
                else if (tile.Type == TileType.Empty)
                {
                    backgroundColor = Brushes.Gainsboro;
                }
                else if (tile.Type == TileType.Task)
                {
                    backgroundColor = new SolidColorBrush(Color.FromRgb(245, 231, 78));
                }

                Border b = new Border
                {
                    BorderThickness = GetBorderThickness(tile.From, tile.To),
                    BorderBrush = Brushes.Black,
                    Background = backgroundColor,
                    Width = tileWidth,
                    Height = tileHeight,
                    CornerRadius = GetCornerRadius(tile.From, tile.To),
                    Child = new TextBlock
                    {
                        Text = tile.Index.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                Grid.SetRow(b, tile.Row);
                Grid.SetColumn(b, tile.Column);
                BoardGrid.Children.Add(b);
            }

            RenderPlayerEllipses();
        }

        private CornerRadius GetCornerRadius(Direction from, Direction to)
        {
            int cornerRadiusNumber = 15;
            if((from == Direction.Down && to == Direction.Right) || (from == Direction.Right && to == Direction.Down))
            {
                return new CornerRadius(cornerRadiusNumber, 0, 0, 0);
            }

            if ((from == Direction.Down && to == Direction.Left) || (from == Direction.Left && to == Direction.Down))
            {
                return new CornerRadius(0, cornerRadiusNumber, 0, 0);
            }

            if ((from == Direction.Up && to == Direction.Left) || (from == Direction.Left && to == Direction.Up))
            {
                return new CornerRadius(0, 0, cornerRadiusNumber, 0);
            }

            if ((from == Direction.Up && to == Direction.Right) || (from == Direction.Right && to == Direction.Up))
            {
                return new CornerRadius(0, 0, 0, cornerRadiusNumber);
            }

            return new CornerRadius(0);
        }

        private Thickness GetBorderThickness(Direction from, Direction to)
        {
            double left = 3, top = 3, right = 3, bottom = 3;
            if(from == Direction.Left || to == Direction.Left)
            {
                left = 1;
            }
            if (from == Direction.Up || to == Direction.Up)
            {
                top = 1;
            }
            if (from == Direction.Right || to == Direction.Right)
            {
                right = 1;
            }
            if (from == Direction.Down || to == Direction.Down)
            {
                bottom = 1;
            }

            return new Thickness(left, top, right, bottom);
        }

        private void RenderPlayerEllipses()
        {
            PlayerCanvas.Children.Clear();

            for(int i = 0; i < totalTileCount; i++)
            {
                List<Player> playersOnTile = new List<Player>();
                foreach(Player curPlayer in _gameState.Players)
                {
                    if(curPlayer.Position == i)
                    {
                        playersOnTile.Add(curPlayer);
                    }
                }

                for(int j = 0; j < playersOnTile.Count; j++)
                {
                    double tileSize = (BoardGrid.Width / boardWidthHeight);
                    double playerSize = (tileSize / 3) - (tileSize / 3) * 0.3;
                    Ellipse playerEllipse = new Ellipse
                    {
                        Width = playerSize,
                        Height = playerSize,
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(playersOnTile[j].Color)),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 1.5
                    };

                    BoardTile currentTile = null;
                    foreach(BoardTile tile in ActualBoard)
                    {
                        if(tile.Index == i)
                        {
                            currentTile = tile;
                            break;
                        }
                    }

                    double tileCenterX = currentTile.Column * tileSize + tileSize / 2;
                    double tileCenterY = currentTile.Row * tileSize + tileSize / 2;

                    double offsetX = 0;
                    double offsetY = 0;

                    switch(j)
                    {
                        case 0:
                            {
                                offsetX = -tileSize / 3;
                                offsetY = -tileSize / 3;
                            }
                            break;
                        case 1:
                            {
                                offsetY = -tileSize / 3;
                            }
                            break;
                        case 2:
                            {
                                offsetX = tileSize / 3;
                                offsetY = -tileSize / 3;
                            }
                            break;
                        case 3:
                            {
                                offsetX = -tileSize / 3;
                                offsetY = tileSize / 3;
                            }
                            break;
                        case 4:
                            {
                                offsetY = tileSize / 3;
                            }
                            break;
                        case 5:
                            {
                                offsetX = tileSize / 3;
                                offsetY = tileSize / 3;
                            }
                            break;
                    }

                    Canvas.SetLeft(playerEllipse, tileCenterX - playerEllipse.Width / 2 + offsetX);
                    Canvas.SetTop(playerEllipse, tileCenterY - playerEllipse.Height / 2 + offsetY);

                    PlayerCanvas.Children.Add(playerEllipse);
                }

                
            }
        }

        private void ButtonLoadTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tasks = _taskService.LoadTasks();
                if (_tasks.Any())
                {
                    TxtLastTask.Text = $"Nahráno {_tasks.Count} úkolů. Příklad: [{_tasks[0].Type}] {_tasks[0].Text}";
                    MessageBox.Show($"Načteno {_tasks.Count} úkolů z Data/tasks.json", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    TxtLastTask.Text = "Žádné úkoly nenalezeny.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání úkolů: " + ex.Message);
            }
        }

        private void ShowRandomTask()
        {
            if (_tasks == null || _tasks.Count == 0)
            {
                return;
            }

            int taskIndex = _rnd.Next(0, _tasks.Count);
            TaskItem task = _tasks[taskIndex];

            ShowTaskOverlay(task, _gameState.Players[_gameState.CurrentPlayerIndex].Name);
        }

        private void ButtonNewGame_Click(object sender, RoutedEventArgs e)
        {
            foreach (var p in _gameState.Players)
            {
                p.Score = 0;
                p.Position = 0;
            }
            _gameState.CurrentPlayerIndex = 0;
            RefreshPlayersList();
            LabelDiceResult.Content = "-";
            MessageBox.Show("Nová hra (demo) byla inicializována.", "Nová hra", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

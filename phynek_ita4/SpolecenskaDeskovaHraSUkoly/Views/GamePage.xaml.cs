using Microsoft.Win32;
using SpolecenskaDeskovaHraSUkoly.Models;
using SpolecenskaDeskovaHraSUkoly.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpolecenskaDeskovaHraSUkoly.Views
{
    public partial class GamePage : Page
    {
        private MainWindow _mainWindow;

        private List<TaskItem> _tasks;
        private readonly Random _rnd = new Random();
        private TaskService _taskService;
        private GameState _gameState;

        private List<BoardTile> _board = new List<BoardTile>();

        private int _boardSizeCellCount;
        private double _boardSizePixels;

        private Dictionary<int, UniformGrid> _tileContainers = new Dictionary<int, UniformGrid>();

        private bool _isGameSaved;
        public GamePage(MainWindow mainWindow, List<Player> players, string boardType, string boardSize, bool bonus, bool penalty, bool empty, int score, int timer)
        {
            InitializeComponent();

            _mainWindow = mainWindow;

            _taskService = new TaskService();
            _tasks = _taskService.LoadTasks();

            _gameState = new GameState
            {
                Players = players,
                CurrentPlayerIndex = 0,
                SelectedBoardType = boardType,
                SelectedBoardSize = boardSize,
                UseBonusTiles = bonus,
                UsePenaltyTiles = penalty,
                UseEmptyTiles = empty,
                WinningScore = score,
                TaskTimer = timer
            };

            this.Loaded += GamePage_Loaded;
        }

        public GamePage(MainWindow mainWindow, GameState loadedState)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _taskService = new TaskService();
            _tasks = _taskService.LoadTasks();

            _gameState = loadedState;

            this.Loaded += GamePage_Loaded;
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            _board = _gameState.Board;
            if (_gameState.Board == null || _gameState.Board.Count == 0)
            {
                GenerateBoardGrid();
            }
            else
            {
                if (_gameState.SelectedBoardSize == "Malá (6x6)")
                {
                    _boardSizeCellCount = 6;
                }
                else if (_gameState.SelectedBoardSize == "Střední (8x8)")
                {
                    _boardSizeCellCount = 8;
                }
                else
                {
                    _boardSizeCellCount = 10;
                }
                DrawBoardUI();
            }

            double tileSize = _boardSizePixels / _boardSizeCellCount;
            double figureSize = tileSize * 0.25;
            foreach (var p in _gameState.Players)
            {
                p.Figure = new Ellipse
                {
                    Fill = p.PlayerColor,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(2),
                    Width = figureSize,
                    Height = figureSize
                };

                MovePlayerToTile(p, p.Position);
            }

            UpdatePlayerList();
            _isGameSaved = true;
        }

        private void GenerateBoardGrid()
        {
            if (_gameState.SelectedBoardSize == "Malá (6x6)")
            {
                _boardSizeCellCount = 6;
            }
            else if (_gameState.SelectedBoardSize == "Střední (8x8)")
            {
                _boardSizeCellCount = 8;
            }
            else
            {
                _boardSizeCellCount = 10;
            }

            var path = new List<(int c, int r)>();
            if (_gameState.SelectedBoardType == "Čtverec (Square)")
            {
                path = GetSquarePath(_boardSizeCellCount);
            }
            else
            {
                path = GetSnakePath(_boardSizeCellCount);
            }

            for (int i = 0; i < path.Count; i++)
            {
                var current = path[i];
                var tile = new BoardTile
                {
                    Index = i,
                    Column = current.c,
                    Row = current.r,
                    Type = TileType.Task
                };

                if (i == 0)
                {
                    tile.From = Direction.Down;
                }
                else
                {
                    tile.From = GetDirection(current, path[i - 1]);
                }

                if (i == path.Count - 1)
                {
                    tile.To = Direction.Up;
                }
                else
                {
                    tile.To = GetDirection(current, path[i + 1]);
                }
                _board.Add(tile);
            }

            AssignTileTypes();

            DrawBoardUI();
        }

        private void DrawBoardUI()
        {
            BoardGrid.Children.Clear();
            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < _boardSizeCellCount; i++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition());
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            double availableWidth = BoardContainer.ActualWidth - BoardContainer.Padding.Left - BoardContainer.Padding.Right;
            double availableHeight = BoardContainer.ActualHeight - BoardContainer.Padding.Top - BoardContainer.Padding.Bottom;

            _boardSizePixels = Math.Min(availableWidth, availableHeight);
            _boardSizePixels = Math.Floor(_boardSizePixels / 10) * 10;

            BoardGrid.Width = _boardSizePixels;
            BoardGrid.Height = _boardSizePixels;

            foreach (var tile in _board)
            {
                Border b = new Border
                {
                    BorderThickness = GetBorderThickness(tile.From, tile.To),
                    BorderBrush = Brushes.Black,
                    Background = GetColorForType(tile.Type),
                    CornerRadius = GetCornerRadius(tile.From, tile.To),
                    Margin = new Thickness(0)
                };

                Grid tileStack = new Grid();

                tileStack.Children.Add(new TextBlock
                {
                    Text = tile.Index.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Opacity = 0.3
                });

                UniformGrid playerHolder = new UniformGrid
                {
                    Rows = 2,
                    Columns = 3,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                _tileContainers[tile.Index] = playerHolder;

                tileStack.Children.Add(playerHolder);

                b.Child = tileStack;

                Grid.SetRow(b, tile.Row);
                Grid.SetColumn(b, tile.Column);

                BoardGrid.Children.Add(b);
            }
        }

        private List<(int c, int r)> GetSquarePath(int boardSize)
        {
            var path = new List<(int c, int r)>();

            for (int c = 0; c < boardSize; c++)
            {
                path.Add((c, 0));
            }
            for (int r = 1; r < boardSize - 1; r++)
            {
                path.Add((boardSize - 1, r));
            }
            for (int c = boardSize - 1; c >= 0; c--)
            {
                path.Add((c, boardSize - 1));
            }
            for (int r = boardSize - 2; r > 0; r--)
            {
                path.Add((0, r));
            }
            return path;
        }

        private List<(int c, int r)> GetSnakePath(int boardSize)
        {
            var path = new List<(int c, int r)>();

            for (int c = 0; c < boardSize; c++)
            {
                path.Add((c, 0));
            }

            for (int x = 0; x < (boardSize - 2) / 2; x++)
            {
                for (int c = boardSize - 1; c > 0; c--)
                {
                    path.Add((c, (2 * x) + 1));
                }
                for (int c = 1; c < boardSize; c++)
                {
                    path.Add((c, (2 * x) + 2));
                }
            }

            for (int c = boardSize - 1; c >= 0; c--)
            {
                path.Add((c, boardSize - 1));
            }
            for (int r = boardSize - 2; r > 0; r--)
            {
                path.Add((0, r));
            }
            return path;
        }

        private Direction GetDirection((int c, int r) current, (int c, int r) neighbor)
        {
            if (neighbor.c > current.c)
            {
                return Direction.Right;
            }
            if (neighbor.c < current.c)
            {
                return Direction.Left;
            }
            if (neighbor.r > current.r)
            {
                return Direction.Down;
            }
            return Direction.Up;
        }

        private void AssignTileTypes()
        {
            Random rnd = new Random();
            int totalCount = _board.Count;

            int bonusCount = 0;
            if (_gameState.UseBonusTiles)
            {
                bonusCount = (int)Math.Floor(totalCount * 0.15);
            }

            int penaltyCount = 0;
            if (_gameState.UsePenaltyTiles)
            {
                penaltyCount = (int)Math.Floor(totalCount * 0.10);
            }

            int emptyCount = 0;
            if (_gameState.UseEmptyTiles)
            {
                emptyCount = (int)Math.Floor(totalCount * 0.05);
            }

            for (int i = 0; i < bonusCount; i++)
            {
                bool nalezeno = false;
                while (!nalezeno)
                {
                    int tileIndex = rnd.Next(0, totalCount);
                    if (_board[tileIndex].Type == TileType.Task)
                    {
                        _board[tileIndex].Type = TileType.Bonus;
                        nalezeno = true;
                    }
                }
            }

            for (int i = 0; i < penaltyCount; i++)
            {
                bool nalezeno = false;
                while (!nalezeno)
                {
                    int tileIndex = rnd.Next(0, totalCount);
                    if (_board[tileIndex].Type == TileType.Task)
                    {
                        _board[tileIndex].Type = TileType.Penalty;
                        nalezeno = true;
                    }
                }
            }

            for (int i = 0; i < emptyCount; i++)
            {
                bool nalezeno = false;
                while (!nalezeno)
                {
                    int tileIndex = rnd.Next(0, totalCount);
                    if (_board[tileIndex].Type == TileType.Task)
                    {
                        _board[tileIndex].Type = TileType.Empty;
                        nalezeno = true;
                    }
                }
            }
        }

        private Brush GetColorForType(TileType type)
        {
            switch (type)
            {
                case TileType.Bonus:
                    {
                        return new SolidColorBrush(Color.FromRgb(51, 163, 52));
                    }
                case TileType.Penalty:
                    {
                        return new SolidColorBrush(Color.FromRgb(186, 47, 47));
                    }
                case TileType.Empty:
                    {
                        return new SolidColorBrush(Color.FromRgb(220, 220, 220));
                    }
                case TileType.Task:
                    {
                        return new SolidColorBrush(Color.FromRgb(245, 231, 78));
                    }
                default:
                    {
                        return Brushes.Black;
                    }
            }
        }

        private Thickness GetBorderThickness(Direction from, Direction to)
        {
            double left = 3, right = 3, up = 3, down = 3;

            if (from == Direction.Left || to == Direction.Left)
            {
                left = 1;
            }
            if (from == Direction.Right || to == Direction.Right)
            {
                right = 1;
            }
            if (from == Direction.Up || to == Direction.Up)
            {
                up = 1;
            }
            if (from == Direction.Down || to == Direction.Down)
            {
                down = 1;
            }

            return new Thickness(left, up, right, down);
        }
        private CornerRadius GetCornerRadius(Direction from, Direction to)
        {
            double radius = 15;

            if ((from == Direction.Right && to == Direction.Down) || (from == Direction.Down && to == Direction.Right))
            {
                return new CornerRadius(radius, 0, 0, 0);
            }
            if ((from == Direction.Left && to == Direction.Down) || (from == Direction.Down && to == Direction.Left))
            {
                return new CornerRadius(0, radius, 0, 0);
            }
            if ((from == Direction.Left && to == Direction.Up) || (from == Direction.Up && to == Direction.Left))
            {
                return new CornerRadius(0, 0, radius, 0);
            }
            if ((from == Direction.Right && to == Direction.Up) || (from == Direction.Up && to == Direction.Right))
            {
                return new CornerRadius(0, 0, 0, radius);
            }
            return new CornerRadius(0);
        }

        private void MovePlayerToTile(Player p, int tileIndex)
        {
            if (_tileContainers.ContainsKey(tileIndex))
            {
                UniformGrid newPlayerHolder = _tileContainers[tileIndex];

                if (p.Figure.Parent != null)
                {
                    Panel currentParent = (Panel)p.Figure.Parent;
                    currentParent.Children.Remove(p.Figure);
                }

                newPlayerHolder.Children.Add(p.Figure);

                p.Position = tileIndex;
            }
        }

        private void UpdatePlayerList()
        {
            PlayersListBox.Items.Clear();

            for (int i = 0; i < _gameState.Players.Count; i++)
            {
                Player player = _gameState.Players[i];
                bool isCurrent = (i == _gameState.CurrentPlayerIndex);

                Brush background = Brushes.Transparent;
                if (isCurrent)
                {
                    background = Brushes.LightYellow;
                }
                StackPanel playerRow = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5),
                    IsHitTestVisible = false,
                    Background = background
                };

                Ellipse colorCircle = new Ellipse
                {
                    Fill = player.PlayerColor,
                    Width = 24,
                    Height = 24,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                StackPanel infoPanel = new StackPanel
                {
                    Margin = new Thickness(10, 5, 10, 10)
                };

                FontWeight fontWeight = FontWeights.Normal;
                if (isCurrent)
                {
                    fontWeight = FontWeights.Bold;
                }
                TextBlock nameTxt = new TextBlock
                {
                    Text = player.Name,
                    FontSize = 18,
                    FontWeight = fontWeight
                };

                TextBlock statsTxt = new TextBlock
                {
                    Text = $"Skóre: {player.Score}",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                };

                infoPanel.Children.Add(nameTxt);
                infoPanel.Children.Add(statsTxt);

                playerRow.Children.Add(colorCircle);
                playerRow.Children.Add(infoPanel);

                if (isCurrent)
                {
                    TextBlock turnMarker = new TextBlock
                    {
                        Text = " ◀ NA TAHU",
                        FontSize = 18,
                        Foreground = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 5, 0)
                    };
                    playerRow.Children.Add(turnMarker);
                }

                PlayersListBox.Items.Add(playerRow);
            }
        }

        private async void ButtonDiceRoll_Click(object sender, RoutedEventArgs e)
        {
            BtnRoll.IsEnabled = false;
            BackBtn.IsEnabled = false;
            SaveBtn.IsEnabled = false;

            int finalRoll = _rnd.Next(1, 7);

            int animationSteps = 20;
            int lastVisualNumber = -1;

            for (int i = 0; i < animationSteps; i++)
            {
                int visualNumber;

                if (i == animationSteps - 1)
                {
                    visualNumber = finalRoll;
                }
                else if (i == animationSteps - 2)
                {
                    do
                    {
                        visualNumber = _rnd.Next(1, 7);
                    } while (visualNumber == lastVisualNumber || visualNumber == finalRoll);
                }
                else
                {
                    do
                    {
                        visualNumber = _rnd.Next(1, 7);
                    } while (visualNumber == lastVisualNumber);
                }

                lastVisualNumber = visualNumber;
                TxtDiceResult.Text = visualNumber.ToString();

                await Task.Delay((int)(40 + (i * i * 1.5)));
            }

            Player currentPlayer = _gameState.Players[_gameState.CurrentPlayerIndex];

            await MovePlayerStepByStep(currentPlayer, finalRoll);

            await CheckTileEffect(currentPlayer);
        }

        private async Task MovePlayerStepByStep(Player player, int steps)
        {
            int totalTiles = _board.Count;

            for (int i = 0; i < steps; i++)
            {
                int nextPos = (player.Position + 1) % totalTiles;

                MovePlayerToTile(player, nextPos);

                await Task.Delay(300);
            }
        }


        private TaskItem _currentTask;
        private Player _currentPlayer;
        private async Task CheckTileEffect(Player player)
        {
            await Task.Delay(1000);

            var currentTile = _board[player.Position];

            switch (currentTile.Type)
            {
                case TileType.Task:
                    {
                        int randomTaskIndex = _rnd.Next(0, _tasks.Count);
                        TaskItem randomTask = _tasks[randomTaskIndex];
                        _currentTask = randomTask;
                        _currentPlayer = player;
                        ShowTaskPhase_CloseEyes();
                    }
                    break;
                case TileType.Bonus:
                    {
                        player.Score += 3;
                        await ShowScoreChangeOverlay(3, 3000);
                        FinishTurn();
                    }
                    break;
                case TileType.Penalty:
                    {
                        if (player.Score >= 2)
                        {
                            player.Score -= 2;
                        }
                        else
                        {
                            player.Score = 0;
                        }
                        await ShowScoreChangeOverlay(-2, 3000);
                        FinishTurn();
                    }
                    break;
                case TileType.Empty:
                    {
                        FinishTurn();
                    }
                    break;
            }
        }

        private async Task ShowScoreChangeOverlay(int scoreChange, int delayMs)
        {
            OverlayContent.Children.Clear();
            OverlayBorder.Width = 400;
            OverlayBorder.Height = 250;

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });

            bool isBonus = scoreChange > 0;

            string title;
            Color titleColor;
            string pointsTextStr;
            if (isBonus)
            {
                title = "BONUS!";
                titleColor = Color.FromRgb(51, 163, 52);
                pointsTextStr = $"+{scoreChange} Body";
            }
            else
            {
                title = "TREST!";
                titleColor = Color.FromRgb(186, 47, 47);
                pointsTextStr = $"{scoreChange} Body";
            }

            TextBlock titleText = new TextBlock
            {
                Text = title,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(titleColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(titleText, 0);
            overlayWindowGrid.Children.Add(titleText);

            TextBlock pointsText = new TextBlock
            {
                Text = pointsTextStr,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(pointsText, 1);
            overlayWindowGrid.Children.Add(pointsText);

            OverlayContent.Children.Add(overlayWindowGrid);

            Overlay.Visibility = Visibility.Visible;
            await Task.Delay(delayMs);

            UpdatePlayerList();

            Overlay.Visibility = Visibility.Collapsed;
        }

        private void ShowTaskPhase_CloseEyes()
        {
            OverlayContent.Children.Clear();
            OverlayBorder.Width = 500;
            OverlayBorder.Height = 350;
            Overlay.Visibility = Visibility.Visible;

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

            TextBlock textHeader = new TextBlock
            {
                Text = _currentTask.Type,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textHeader, 0);
            overlayWindowGrid.Children.Add(textHeader);

            TextBlock textInstruction = new TextBlock
            {
                Text = $"Všichni kromě {_currentPlayer.Name} se otočí!",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textInstruction, 1);
            overlayWindowGrid.Children.Add(textInstruction);

            Border showBtnBorder = CreateButtonBorder();
            Button showBtn = new Button { Content = "Zobrazit", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            showBtn.Click += ShowBtn_Click;
            showBtnBorder.Child = showBtn;
            Grid.SetRow(showBtnBorder, 2);
            overlayWindowGrid.Children.Add(showBtnBorder);

            OverlayContent.Children.Add(overlayWindowGrid);
        }

        private void ShowBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowTaskPhase_Ready();
        }

        private void ShowTaskPhase_Ready()
        {
            OverlayContent.Children.Clear();

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

            TextBlock textHeader = new TextBlock
            {
                Text = _currentTask.Type,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textHeader, 0);
            overlayWindowGrid.Children.Add(textHeader);

            TextBlock textTask = new TextBlock
            {
                Text = _currentTask.Text,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textTask, 1);
            overlayWindowGrid.Children.Add(textTask);

            Border startBtnBorder = CreateButtonBorder();
            Button startBtn = new Button { Content = "Start", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            startBtn.Click += StartBtn_Click;
            startBtnBorder.Child = startBtn;
            Grid.SetRow(startBtnBorder, 2);
            overlayWindowGrid.Children.Add(startBtnBorder);

            OverlayContent.Children.Add(overlayWindowGrid);
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            await StartCountdown();
        }

        private async Task StartCountdown()
        {
            OverlayContent.Children.Clear();

            TextBlock textCount = new TextBlock
            {
                Text = "3",
                FontSize = 80,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };
            OverlayContent.Children.Add(textCount);

            for (int i = 2; i >= 1; i--)
            {
                await Task.Delay(1000);
                textCount.Text = i.ToString();
            }

            await Task.Delay(1000);
            textCount.Text = "START!";
            textCount.Foreground = Brushes.Green;
            await Task.Delay(800);

            RunTimer();
        }

        private bool timerStoppedManually;
        private async void RunTimer()
        {
            int timeLeft = _gameState.TaskTimer;
            timerStoppedManually = false;

            OverlayContent.Children.Clear();

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            TextBlock textTimer = new TextBlock
            {
                Text = timeLeft.ToString(),
                FontSize = 70,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            if (timeLeft <= 10)
            {
                textTimer.Foreground = Brushes.Red;
            }
            Grid.SetRow(textTimer, 0);
            overlayWindowGrid.Children.Add(textTimer);

            Border stopBtnBorder = CreateButtonBorder();
            Button stopBtn = new Button { Content = "Hotovo / Ukončit", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            stopBtn.Click += StopBtn_Click;
            stopBtnBorder.Child = stopBtn;
            Grid.SetRow(stopBtnBorder, 2);
            overlayWindowGrid.Children.Add(stopBtnBorder);

            OverlayContent.Children.Add(overlayWindowGrid);

            while (timeLeft > 0 && !timerStoppedManually)
            {
                await Task.Delay(1000);
                timeLeft--;
                textTimer.Text = timeLeft.ToString();
                if (timeLeft <= 10)
                {
                    textTimer.Foreground = Brushes.Red;
                }
            }

            ShowTaskPhase_Result();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            timerStoppedManually = true;
        }

        private void ShowTaskPhase_Result()
        {
            OverlayContent.Children.Clear();

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

            string textResStr;
            if (timerStoppedManually)
            {
                textResStr = "Konec času!";
            }
            else
            {
                textResStr = "Čas vypršel!";
            }
            TextBlock textRes = new TextBlock
            {
                Text = textResStr,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textRes, 0);
            overlayWindowGrid.Children.Add(textRes);

            TextBlock textInfo = new TextBlock
            {
                Text = _currentTask.Text,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textInfo, 1);
            overlayWindowGrid.Children.Add(textInfo);

            StackPanel sp = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
            };

            Border failBtnBorder = CreateButtonBorder();
            failBtnBorder.Margin = new Thickness(0, 0, 10, 0);
            Button failBtn = new Button { Content = "Nesplněno", Foreground = Brushes.Red, Style = (Style)Application.Current.Resources["ButtonStyle"] };
            failBtn.Click += FailBtn_Click;
            failBtnBorder.Child = failBtn;

            Border successBtnBorder = CreateButtonBorder();
            successBtnBorder.Margin = new Thickness(10, 0, 0, 0);
            Button successBtn = new Button { Content = "Splněno", Foreground = Brushes.Green, Style = (Style)Application.Current.Resources["ButtonStyle"] };
            successBtn.Click += SuccessBtn_Click;
            successBtnBorder.Child = successBtn;

            sp.Children.Add(failBtnBorder);
            sp.Children.Add(successBtnBorder);

            Grid.SetRow(sp, 2);
            overlayWindowGrid.Children.Add(sp);

            OverlayContent.Children.Add(overlayWindowGrid);
        }

        private void FailBtn_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Collapsed;
            FinishTurn();
        }

        private void SuccessBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowWinnerOfTaskSelection();
        }

        ComboBox comboBox;
        private void ShowWinnerOfTaskSelection()
        {
            OverlayContent.Children.Clear();

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

            TextBlock textHeader = new TextBlock
            {
                Text = "Kdo uhodl?",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textHeader, 0);
            overlayWindowGrid.Children.Add(textHeader);

            comboBox = new ComboBox
            {
                Width = 200,
                Height = 50,
                FontSize = 16,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(5)
            };

            foreach (var p in _gameState.Players)
            {
                if (p != _gameState.Players[_gameState.CurrentPlayerIndex])
                {
                    comboBox.Items.Add(p.Name);
                }
            }
            comboBox.SelectedIndex = 0;

            Grid.SetRow(comboBox, 1);
            overlayWindowGrid.Children.Add(comboBox);

            StackPanel sp = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
            };

            Border backToResultBtnBorder = CreateButtonBorder();
            backToResultBtnBorder.Margin = new Thickness(0, 0, 10, 0);
            Button backToResultBtn = new Button { Content = "Zpět", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            backToResultBtn.Click += BackToResultBtn_Click;
            backToResultBtnBorder.Child = backToResultBtn;

            Border addPointsBtnBorder = CreateButtonBorder();
            addPointsBtnBorder.Margin = new Thickness(10, 0, 0, 0);
            Button addPointsBtn = new Button { Content = "Připsat body", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            addPointsBtn.Click += AddPointsBtn_Click;
            addPointsBtnBorder.Child = addPointsBtn;

            sp.Children.Add(backToResultBtnBorder);
            sp.Children.Add(addPointsBtnBorder);

            Grid.SetRow(sp, 2);
            overlayWindowGrid.Children.Add(sp);

            OverlayContent.Children.Add(overlayWindowGrid);
        }

        private void BackToResultBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowTaskPhase_Result();
        }

        private void AddPointsBtn_Click(object sender, RoutedEventArgs e)
        {
            string winnerOfTask = comboBox.SelectedItem.ToString();
            foreach (var p in _gameState.Players)
            {
                if (p == _currentPlayer || p.Name == winnerOfTask)
                {
                    p.Score++;
                }
            }

            UpdatePlayerList();

            Overlay.Visibility = Visibility.Collapsed;
            FinishTurn();
        }

        private Border CreateButtonBorder(double width = 180, double height = 60)
        {
            return new Border
            {
                Style = (Style)this.FindResource("BorderStyle"),
                Width = width,
                Height = height,
            };
        }

        private async void FinishTurn()
        {
            List<Player> winners = new List<Player>();

            foreach (Player p in _gameState.Players)
            {
                if (p.Score >= _gameState.WinningScore)
                {
                    winners.Add(p);
                }
            }

            if (winners.Count > 0)
            {
                await DeclareWinners(winners);
                return;
            }

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;

            UpdatePlayerList();

            _isGameSaved = false;

            TxtDiceResult.Text = "-";
            BtnRoll.IsEnabled = true;
            BackBtn.IsEnabled = true;
            SaveBtn.IsEnabled = true;
        }

        private async Task DeclareWinners(List<Player> winners)
        {
            BtnRoll.IsEnabled = false;
            await Task.Delay(1000);

            OverlayContent.Children.Clear();
            OverlayBorder.Width = 600;
            OverlayBorder.Height = 400;


            Grid overlayWinnerGrid = new Grid { Margin = new Thickness(20) };

            overlayWinnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
            overlayWinnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });
            overlayWinnerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });


            string title;
            if (winners.Count > 1)
            {
                title = "Společné vítězství!";
            }
            else
            {
                title = "Vítězem se stává...";
            }
            TextBlock textTitle = new TextBlock
            {
                Text = title,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(textTitle, 0);
            overlayWinnerGrid.Children.Add(textTitle);

            string names = "🏆   ";
            for (int i = 0; i < winners.Count; i++)
            {
                names += winners[i].Name.ToString();

                if (i < winners.Count - 1)
                {
                    names += " a ";
                }
            }
            names += "   🏆";
            TextBlock textWinnerNames = new TextBlock
            {
                Text = names,
                FontSize = 40,
                Foreground = Brushes.Gold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetRow(textWinnerNames, 1);
            overlayWinnerGrid.Children.Add(textWinnerNames);

            Border backToMenuBtnBorder = CreateButtonBorder();
            Button backToMenuBtn = new Button { Content = "Zpět do menu", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            backToMenuBtn.Click += BackBtnAfterWin_Click;
            backToMenuBtnBorder.Child = backToMenuBtn;
            Grid.SetRow(backToMenuBtnBorder, 2);
            overlayWinnerGrid.Children.Add(backToMenuBtnBorder);

            OverlayContent.Children.Add(overlayWinnerGrid);
            Overlay.Visibility = Visibility.Visible;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isGameSaved)
            {
                NavigateBackToMenu();
                return;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Máte neuložené změny. Chcete hru před odchodem uložit?", "Upozornění",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (SaveGameProcess())
                    {
                        NavigateBackToMenu();
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    NavigateBackToMenu();
                }
            }
        }

        private void BackBtnAfterWin_Click(object sender, RoutedEventArgs e)
        {
            NavigateBackToMenu();
        }

        private void NavigateBackToMenu()
        {
            _mainWindow.MainFrame.Navigate(new MainMenuPage(_mainWindow));
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveGameProcess();
        }

        private bool SaveGameProcess()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON soubor (*.json)|*.json",
                FileName = "moje_ulozena_hra.json",
                Title = "Vyberte místo pro uložení hry"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

                    string jsonString = JsonSerializer.Serialize(_gameState, options);
                    File.WriteAllText(saveFileDialog.FileName, jsonString);

                    _isGameSaved = true;
                    MessageBox.Show("Hra byla úspěšně uložena!", "Uloženo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Při ukládání došlo k chybě: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return false;
        }
    }
}
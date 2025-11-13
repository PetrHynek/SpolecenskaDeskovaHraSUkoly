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

        public GamePage(MainWindow mainWindow, List<Player> players)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            _taskService = new TaskService();
            _gameState = new GameState();

            _gameState.Players = players;
            RefreshPlayersList();

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

            var current = _gameState.CurrentPlayerIndex;
            _gameState.Players[current].Position += finalValue;

            int boardSize = 20;
            _gameState.Players[current].Position %= boardSize;
            RefreshPlayersList();

            ShowRandomTask();
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
            BackButton1.Visibility = Visibility.Collapsed;
            ContinueButton.Visibility = Visibility.Collapsed;
            CountdownGrid.Visibility = Visibility.Collapsed;
            CorrectAnswerGrid.Visibility = Visibility.Collapsed;
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
            BackButton1.Visibility = Visibility.Visible;
            ContinueButton.Visibility = Visibility.Visible;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            TaskGrid.Visibility = Visibility.Collapsed;

            NextPlayerTurn();
        }

        private void SuccessButton_Click(object sender, RoutedEventArgs e)
        {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chyba při načítání úkolů: " + ex.Message + "\nNávrat do nastavení hry.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                _mainWindow.MainFrame.Navigate(new GameSettingsPage(_mainWindow, _gameState.Players));
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

            int index = _rnd.Next(0, _tasks.Count);
            TaskItem task = _tasks[index];

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

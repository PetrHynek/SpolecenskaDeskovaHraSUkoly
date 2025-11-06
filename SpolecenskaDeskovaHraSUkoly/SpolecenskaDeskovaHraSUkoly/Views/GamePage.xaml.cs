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

            ShowRandomTask();

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            RefreshPlayersList();

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

            TxtLastTask.Text = $"{task.Type}: {task.Text}";
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

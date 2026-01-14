using SpolecenskaDeskovaHraSUkoly.Models;
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
    /// Interakční logika pro GameSettingsPage.xaml
    /// </summary>
    public partial class GameSettingsPage : Page
    {
        private MainWindow _mainWindow;
        private List<Player> _existingPlayers;
        private List<string> _allColors = new List<string> { "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Aqua", "Indigo", "Yellowgreen" };
        private List<string> _boardTypes = new List<string> { "Circle", "Snake" };
        private List<string> _boardSize = new List<string> { "4x4", "6x6", "8x8", "10x10" };

        public GameSettingsPage(MainWindow mainWindow, List<Player> existingPlayers = null)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _existingPlayers = existingPlayers;

            if (_existingPlayers != null)
            {
                ComboBoxPlayerCount.SelectedIndex = _existingPlayers.Count - 2;
                PopulateExistingPlayers();
            }
            else
            {
                ComboBoxPlayerCount.SelectedIndex = 0;
            }

            ComboBoxBoardType.ItemsSource = _boardTypes;
            ComboBoxBoardType.SelectedIndex = 0;

            ComboBoxBoardSize.ItemsSource = _boardSize;
            ComboBoxBoardSize.SelectedIndex = 0;
        }

        private void PopulateExistingPlayers()
        {
            PlayersStackPanel.Children.Clear();
            int playerCount = _existingPlayers.Count;

            for (int i = 0; i < playerCount; i++)
            {
                var player = _existingPlayers[i];
                StackPanel playerPanel = CreatePlayerPanel(i + 1, player.Name, player.Color);
                PlayersStackPanel.Children.Add(playerPanel);
            }

            UpdateColorSelections();
        }

        private StackPanel CreatePlayerPanel(int index, string name, string color)
        {
            StackPanel playerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            TextBlock playerLabel = new TextBlock { Text = $"Hráč {index}:", Width = 60, VerticalAlignment = VerticalAlignment.Center };
            TextBox playerName = new TextBox { Width = 120, Margin = new Thickness(5, 0, 5, 0), Text = name };
            ComboBox playerColor = new ComboBox { Width = 100 };

            playerColor.ItemsSource = _allColors.ToList();
            playerColor.SelectedItem = color;

            playerColor.SelectionChanged += PlayerColor_SelectionChanged;

            playerPanel.Children.Add(playerLabel);
            playerPanel.Children.Add(playerName);
            playerPanel.Children.Add(playerColor);

            return playerPanel;
        }

        private void ComboBoxPlayerCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayersStackPanel.Children.Clear();

            int playerCount = ComboBoxPlayerCount.SelectedIndex + 2;

            for(int i = 0; i < playerCount; i++)
            {
                string defaultColor = _allColors[i];
                StackPanel playerPanel = CreatePlayerPanel(i + 1, $"Hráč {i + 1}", defaultColor);
                PlayersStackPanel.Children.Add(playerPanel);
            }

            UpdateColorSelections();
        }

        private void PlayerColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateColorSelections();
        }

        private void UpdateColorSelections()
        {
            // 1. zjisti všechny barvy, které jsou aktuálně vybrané
            var selectedColors = new List<string>();
            foreach (StackPanel panel in PlayersStackPanel.Children)
            {
                ComboBox combo = panel.Children[2] as ComboBox;
                if (combo.SelectedItem != null)
                {
                    selectedColors.Add(combo.SelectedItem.ToString());
                }
            }

            // 2️. projdeme všechny hráče a nastavíme jim dostupné barvy
            for (int i = 0; i < PlayersStackPanel.Children.Count; i++)
            {
                StackPanel panel = PlayersStackPanel.Children[i] as StackPanel;
                ComboBox combo = panel.Children[2] as ComboBox;
                string currentColor = combo.SelectedItem as string;

                List<string> availableColors = new List<string>();
                foreach (string color in _allColors)
                {
                    if (color == currentColor || !selectedColors.Contains(color))
                    {
                        availableColors.Add(color);
                    }
                }

                combo.ItemsSource = availableColors;
                combo.SelectedItem = currentColor;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new MainMenuPage(_mainWindow));
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            List<Player> players = new List<Player>();
            HashSet<string> usedNames = new HashSet<string>();
            HashSet<string> usedColors = new HashSet<string>();

            foreach (StackPanel panel in PlayersStackPanel.Children)
            {
                TextBox nameBox = panel.Children[1] as TextBox;
                ComboBox colorBox = panel.Children[2] as ComboBox;

                string playerName = nameBox.Text.Trim();
                string playerColor = (string)colorBox.SelectedItem;

                if (string.IsNullOrWhiteSpace(playerName))
                {
                    MessageBox.Show("Všichni hráči musí mít jméno!");
                    return;
                }
                if (usedNames.Contains(playerName))
                {
                    MessageBox.Show($"Jméno '{playerName}' už je použito. Vyberte jiné.");
                    return;
                }
                if (usedColors.Contains(playerColor))
                {
                    MessageBox.Show($"Barva '{playerColor}' už je použita. Vyberte jinou.");
                    return;
                }

                players.Add(new Player
                {
                    Name = playerName,
                    Color = playerColor,
                    Position = 0,
                    Score = 0
                });

                usedNames.Add(playerName);
                usedColors.Add(playerColor);
            }

            if (players.Count < 2)
            {
                MessageBox.Show("Musíte zadat alespoň 2 hráče!");
                return;
            }

            string selectedBoardType = (string)ComboBoxBoardType.SelectedItem;
            string selectedBoardSize = (string)ComboBoxBoardSize.SelectedItem;

            _mainWindow.MainFrame.Navigate(new GamePage(_mainWindow, players, selectedBoardType, selectedBoardSize));
        }
    }
}

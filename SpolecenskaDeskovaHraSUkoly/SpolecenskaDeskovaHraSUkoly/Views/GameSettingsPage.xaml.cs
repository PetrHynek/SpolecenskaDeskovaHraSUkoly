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
        }

        private void PopulateExistingPlayers()
        {
            PlayersStackPanel.Children.Clear();
            int playerCount = _existingPlayers.Count;

            for (int i = 0; i < playerCount; i++)
            {
                var player = _existingPlayers[i];
                StackPanel playerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                TextBlock playerLabel = new TextBlock { Text = $"Hráč {i + 1}:", Width = 60, VerticalAlignment = VerticalAlignment.Center };
                TextBox playerName = new TextBox { Width = 120, Margin = new Thickness(5, 0, 5, 0), Text = player.Name };
                ComboBox playerColor = new ComboBox { Width = 100 };

                playerColor.Items.Add("Red");
                playerColor.Items.Add("Blue");
                playerColor.Items.Add("Green");
                playerColor.Items.Add("Yellow");
                playerColor.Items.Add("Purple");
                playerColor.Items.Add("Orange");
                playerColor.SelectedItem = player.Color;

                playerPanel.Children.Add(playerLabel);
                playerPanel.Children.Add(playerName);
                playerPanel.Children.Add(playerColor);

                PlayersStackPanel.Children.Add(playerPanel);
            }
        }

        private void ComboBoxPlayerCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayersStackPanel.Children.Clear();

            int playerCount = ComboBoxPlayerCount.SelectedIndex + 2;

            for(int i = 1; i <= playerCount; i++)
            {
                StackPanel playerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                TextBlock playerLabel = new TextBlock { Text = $"Hráč {i}:", Width = 60, VerticalAlignment = VerticalAlignment.Center };
                TextBox playerName = new TextBox { Width = 120, Margin = new Thickness(5, 0, 5, 0), Name = $"PlayerName{i}" };
                ComboBox playerColor = new ComboBox { Width = 100, Name = $"PlayerColor{i}" };

                playerColor.Items.Add("Red");
                playerColor.Items.Add("Blue");
                playerColor.Items.Add("Green");
                playerColor.Items.Add("Yellow");
                playerColor.Items.Add("Purple");
                playerColor.Items.Add("Orange");
                playerColor.SelectedIndex = i - 1;

                playerPanel.Children.Add(playerLabel);
                playerPanel.Children.Add(playerName);
                playerPanel.Children.Add(playerColor);

                PlayersStackPanel.Children.Add(playerPanel);
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

            _mainWindow.MainFrame.Navigate(new GamePage(_mainWindow, players));
        }
    }
}

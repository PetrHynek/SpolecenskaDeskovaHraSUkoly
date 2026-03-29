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
    public partial class GameSettingsPage : Page
    {
        private MainWindow _mainWindow;
        public GameSettingsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            ComboBoxPlayerCount.SelectedIndex = 0;

            ComboBoxBoardType.SelectedIndex = 0;
            ComboBoxBoardSize.SelectedIndex = 0;

            TargetScoreTextBox.Text = "30";
            TaskTimerTextBox.Text = "30";

            CheckBonus.IsChecked = true;
            CheckPenalty.IsChecked = true;
            CheckEmpty.IsChecked = true;

            ComboBoxPlayerCount.SelectionChanged += ComboBoxPlayerCount_SelectionChanged;

            GeneratePlayerRows();
        }

        private void ComboBoxPlayerCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GeneratePlayerRows();
        }

        private void GeneratePlayerRows()
        {
            if (ComboBoxPlayerCount.SelectedItem is ComboBoxItem selectedItem)
            {
                int count = int.Parse(selectedItem.Content.ToString());

                PlayersContainer.Children.Clear();

                for (int i = 1; i <= count; i++)
                {
                    PlayersContainer.Children.Add(CreatePlayerRow(i));
                }
            }

            RefreshAllColorChoices();
        }

        private UIElement CreatePlayerRow(int playerIndex)
        {
            StackPanel row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 8) };

            row.Children.Add(new TextBlock
            {
                Text = $"{playerIndex}.",
                VerticalAlignment = VerticalAlignment.Center,
                Width = 25,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            });

            TextBox txtName = new TextBox
            {
                Width = 200,
                Height = 32,
                FontSize = 14,
                Margin = new Thickness(5, 0, 10, 0),
                Padding = new Thickness(5, 0, 0, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = $"Hráč {playerIndex}"
            };
            row.Children.Add(txtName);

            Ellipse colorPreview = new Ellipse
            {
                Width = 28,
                Height = 28,
                Margin = new Thickness(12, 0, 0, 0),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                VerticalAlignment = VerticalAlignment.Center
            };

            ComboBox cbColor = new ComboBox
            {
                Width = 130,
                Height = 32,
                FontSize = 14,
                VerticalContentAlignment = VerticalAlignment.Center,
                ItemsSource = ColorPalette.AvailableBrushes.Keys,
                SelectedIndex = (playerIndex - 1)
            };

            cbColor.Tag = colorPreview;

            cbColor.SelectionChanged += OnPlayerColorChanged;

            colorPreview.Fill = ColorPalette.AvailableBrushes[cbColor.SelectedItem.ToString()];

            row.Children.Add(cbColor);
            row.Children.Add(colorPreview);

            return row;
        }

        private void OnPlayerColorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            ComboBox currentCb = (ComboBox)sender;
            Ellipse preview = (Ellipse)currentCb.Tag;

            if (currentCb.SelectedItem != null && preview != null)
            {
                preview.Fill = ColorPalette.AvailableBrushes[currentCb.SelectedItem.ToString()];
            }

            RefreshAllColorChoices();
        }

        private void RefreshAllColorChoices()
        {
            HashSet<string> takenColors = new HashSet<string>();
            foreach (var child in PlayersContainer.Children)
            {
                if (child is StackPanel row)
                {
                    ComboBox cb = (ComboBox)row.Children[2];
                    if (cb.SelectedItem != null)
                    {
                        takenColors.Add(cb.SelectedItem.ToString());
                    }
                }
            }

            foreach (var child in PlayersContainer.Children)
            {
                if (child is StackPanel row)
                {
                    ComboBox cb = (ComboBox)row.Children[2];
                    string currentSelected = cb.SelectedItem.ToString();

                    List<string> availableForThisPlayer = new List<string>();

                    foreach (string colorName in ColorPalette.AvailableBrushes.Keys)
                    {
                        if (!takenColors.Contains(colorName) || colorName == currentSelected)
                        {
                            availableForThisPlayer.Add(colorName);
                        }
                    }

                    cb.SelectionChanged -= OnPlayerColorChanged;

                    cb.ItemsSource = availableForThisPlayer;
                    cb.SelectedItem = currentSelected;

                    cb.SelectionChanged += OnPlayerColorChanged;
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (!IsTextNumeric(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsTextNumeric(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateBeforeStart(TextBox textBox)
        {
            int min = 0;
            int max = 0;
            string fieldName = "";

            if (textBox.Name == "TargetScoreTextBox")
            {
                min = 10;
                max = 100;
                fieldName = "Cílové skóre";
            }
            else if (textBox.Name == "TaskTimerTextBox")
            {
                min = 5;
                max = 120;
                fieldName = "Časovač";
            }

            string cleanText = textBox.Text.Trim();

            if (!int.TryParse(cleanText, out int value) || value < min || value > max)
            {
                MessageBox.Show($"{fieldName} musí být číslo v rozmezí {min} až {max}!", "Neplatné nastavení", MessageBoxButton.OK, MessageBoxImage.Warning);

                textBox.Focus();
                textBox.SelectAll();

                return false;
            }

            textBox.Text = value.ToString();
            return true;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateBeforeStart(TargetScoreTextBox))
            {
                return;
            }
            if (!ValidateBeforeStart(TaskTimerTextBox))
            {
                return;
            }

            List<Player> playersList = new List<Player>();
            HashSet<string> usedNames = new HashSet<string>();

            int rowIndex = 1;
            foreach (var child in PlayersContainer.Children)
            {
                if (child is StackPanel row)
                {
                    TextBox txtName = (TextBox)row.Children[1];
                    ComboBox cbColor = (ComboBox)row.Children[2];

                    string rawName = txtName.Text.Trim();

                    if (string.IsNullOrWhiteSpace(rawName))
                    {
                        MessageBox.Show($"Hráč č. {rowIndex} nemá zadané jméno!", "Chyba jména", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtName.Focus();
                        txtName.SelectAll();
                        return;
                    }

                    if (usedNames.Contains(rawName.ToLower()))
                    {
                        MessageBox.Show($"Jméno '{rawName}' už používá jiný hráč. Každý hráč musí mít unikátní jméno!", "Duplicitní jméno", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtName.Focus();
                        txtName.SelectAll();
                        return;
                    }

                    usedNames.Add(rawName.ToLower());

                    Player newPlayer = new Player
                    {
                        Name = rawName,
                        ColorName = cbColor.SelectedItem.ToString(),
                        Score = 0,
                        Position = 0
                    };

                    playersList.Add(newPlayer);
                    rowIndex++;
                }
            }

            string selectedBoardType = (ComboBoxBoardType.SelectedItem as ComboBoxItem).Content.ToString();
            string selectedBoardSize = (ComboBoxBoardSize.SelectedItem as ComboBoxItem).Content.ToString();

            bool useBonusTiles = true;
            if (CheckBonus.IsChecked == false)
            {
                useBonusTiles = false;
            }
            bool usePenaltyTiles = true;
            if (CheckPenalty.IsChecked == false)
            {
                usePenaltyTiles = false;
            }
            bool useEmptyTiles = true;
            if (CheckEmpty.IsChecked == false)
            {
                useEmptyTiles = false;
            }

            int winningScore = int.Parse(TargetScoreTextBox.Text);
            int taskTimer = int.Parse(TaskTimerTextBox.Text);

            LaunchGame(playersList, selectedBoardType, selectedBoardSize, useBonusTiles, usePenaltyTiles, useEmptyTiles, winningScore, taskTimer);
        }

        private void LaunchGame(List<Player> players, string boardType, string boardSize, bool bonus, bool penalty, bool empty, int score, int timer)
        {
            _mainWindow.MainFrame.Navigate(new GamePage(_mainWindow, players, boardType, boardSize, bonus, penalty, empty, score, timer));
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new MainMenuPage(_mainWindow));
        }
    }
}